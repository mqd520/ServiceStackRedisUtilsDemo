using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using ServiceStack.Redis;
using ServiceStack.Text;

namespace ServiceStack.Redis.Utils
{
    public static class ServiceStackRedisUtils
    {
        #region Property
        /// <summary>
        /// Get PooledRedisClientManager
        /// </summary>
        public static PooledRedisClientManager Prcm { get; private set; }

        /// <summary>
        /// Get RedisClientsManager
        /// </summary>
        public static IRedisClientsManager Rcm { get; private set; }

        /// <summary>
        /// Get Is Sentinel Mode
        /// </summary>
        public static bool IsSentinelMode { get; private set; }
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

            KeepAliveService.Instance.Init();

            RedisConfigurationSection redis = ConfigurationManager.GetSection("redis") as RedisConfigurationSection;
            string[] arrRW = redis.ReadWriteHosts.Split(',').Select(x => x.Trim()).ToArray();
            string[] arrR = redis.ReadOnlyHosts.Split(',').Select(x => x.Trim()).ToArray();

            Prcm = new PooledRedisClientManager(arrRW, arrR, new RedisClientManagerConfig
            {
                MaxWritePoolSize = redis.MaxWritePoolSize,
                MaxReadPoolSize = redis.MaxReadPoolSize,
                AutoStart = redis.AutoStart
            });
            IsSentinelMode = false;

            //IList<string> ls = new List<string>();
            //ls.Add("192.168.0.60:16379");
            //RedisSentinel rs = new RedisSentinel(ls);
            //Rcm = rs.Start();
            //rs.OnFailover = x =>
            //{
            //    System.Diagnostics.Debug.WriteLine("OnFailover: 192.168.0.60:16379");
            //};
            //IsSentinelMode = true;
        }

        /// <summary>
        /// Exit
        /// </summary>
        public static void Exit()
        {
            KeepAliveService.Instance.Exit();
            Prcm.Dispose();
        }

        /// <summary>
        /// Get Client
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetClient()
        {
            if (IsSentinelMode)
            {
                return Rcm.GetReadOnlyClient();
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
                client.Password = "123456";

                return client;
            }
            else
            {
                IRedisClient client = null;

                int count = 0;
                while (count <= 10)
                {
                    client = Prcm.GetReadOnlyClient();
                    if (KeepAliveService.Instance.IsAvaiable(client))
                    {
                        break;
                    }
                    else
                    {
                        client.Dispose();
                    }

                    count++;
                }

                return client;
            }
        }
        #endregion
    }
}
