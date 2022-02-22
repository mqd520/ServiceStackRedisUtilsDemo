﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using ServiceStack.Redis;
using ServiceStack.Text;

namespace ServiceStack.Redis.Utils
{
    /// <summary>
    /// ServiceStack.Redis Utils
    /// </summary>
    public static class ServiceStackRedisUtils
    {
        #region Property
        /// <summary>
        /// Get PooledRedisClientManager
        /// </summary>
        public static PooledRedisClientManager Prcm { get; private set; }

        /// <summary>
        /// Get Sentinel
        /// </summary>
        public static RedisSentinel Sentinel { get; private set; }

        /// <summary>
        /// Get RedisClientsManager
        /// </summary>
        public static IRedisClientsManager Rcm { get; private set; }

        /// <summary>
        /// Get RedisConfigSection
        /// </summary>
        public static RedisConfigurationSection RedisConfigSection { get; private set; }

        /// <summary>
        /// Get Is Sentinel Mode
        /// </summary>
        public static bool IsSentinelMode { get; private set; }

        /// <summary>
        /// Get Sentinel Node Pwd
        /// </summary>
        public static string SentinelNodePwd { get; private set; } = null;

        /// <summary>
        /// Get Max Allow Re Get Client
        /// </summary>
        public static int MaxAllowReGetClient { get; private set; } = 10;

        /// <summary>
        /// Get Is KeepAlive Mode
        /// </summary>
        public static bool IsKeepAliveMode { get; private set; }
        #endregion


        #region Constructor
        /// <summary>
        /// ServiceStackRedisUtils
        /// </summary>
        static ServiceStackRedisUtils()
        {

        }
        #endregion


        #region Method
        /// <summary>
        /// Init
        /// </summary>
        public static void Init()
        {
            JsConfig.DateHandler = DateHandler.ISO8601DateTime;
            JsConfig.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            RedisConfigSection = ConfigurationManager.GetSection("redis") as RedisConfigurationSection;

            IsKeepAliveMode = RedisConfigSection.IsKeepAlive;
            IsSentinelMode = RedisConfigSection.IsSentinel;

            if (!IsSentinelMode)
            {
                if (IsKeepAliveMode)
                {
                    KeepAliveService.Instance.Init();
                }

                string[] arrRW = RedisConfigSection.ReadWriteHosts.Split(',').Select(x => x.Trim()).ToArray();
                string[] arrR = RedisConfigSection.ReadOnlyHosts.Split(',').Select(x => x.Trim()).ToArray();

                Prcm = new PooledRedisClientManager(arrRW, arrR, new RedisClientManagerConfig
                {
                    MaxWritePoolSize = RedisConfigSection.MaxWritePoolSize,
                    MaxReadPoolSize = RedisConfigSection.MaxReadPoolSize,
                    AutoStart = RedisConfigSection.AutoStart
                });
            }
            else
            {
                SentinelNodePwd = RedisConfigSection.SentinelNodePwd;

                var ls = RedisConfigSection.SentinelHosts.Split(',').Select(x => x.Trim()).ToList();
                Sentinel = new RedisSentinel(ls);
                Rcm = Sentinel.Start();
            }
        }

        /// <summary>
        /// Exit
        /// </summary>
        public static void Exit()
        {
            if (IsKeepAliveMode)
            {
                KeepAliveService.Instance.Exit();
            }

            if (Prcm != null)
            {
                Prcm.Dispose();
            }
            if (Rcm != null)
            {
                Rcm.Dispose();
            }
        }

        /// <summary>
        /// Get Client
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetClient()
        {
            if (IsSentinelMode)
            {
                return Rcm.GetClient();
            }
            else
            {
                return Prcm.GetClient();
            }
        }

        /// <summary>
        /// Get Read Only Client
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetReadOnlyClient()
        {
            if (IsSentinelMode)
            {
                var client = Rcm.GetReadOnlyClient();
                if (string.IsNullOrEmpty(SentinelNodePwd))
                {
                    client.Password = SentinelNodePwd;
                }

                if (IsKeepAliveMode)
                {
                    int count = 0;
                    while (count <= MaxAllowReGetClient)
                    {
                        if (KeepAliveService.Instance.IsAvaiable(client))
                        {
                            break;
                        }
                        else
                        {
                            client.Dispose();
                            client = null;
                            client = Rcm.GetReadOnlyClient();

                            count++;
                        }
                    }
                }

                return client;
            }
            else
            {
                IRedisClient client = Prcm.GetReadOnlyClient();

                if (IsKeepAliveMode)
                {
                    int count = 0;
                    while (count <= MaxAllowReGetClient)
                    {
                        if (KeepAliveService.Instance.IsAvaiable(client))
                        {
                            break;
                        }
                        else
                        {
                            client.Dispose();
                            client = null;
                            client = Prcm.GetReadOnlyClient();

                            count++;
                        }
                    }
                }

                return client;
            }
        }

        /// <summary>
        /// Set Redis Client Options
        /// </summary>
        /// <param name="client"></param>
        public static void SetRedisClientOptions(IRedisClient client)
        {
            if (client != null)
            {
                client.RetryCount = RedisConfigSection.RetryCount;
                client.RetryTimeout = RedisConfigSection.RetryTimeout;
                client.ConnectTimeout = RedisConfigSection.ConnectTimeout;
            }
        }
        #endregion
    }
}
