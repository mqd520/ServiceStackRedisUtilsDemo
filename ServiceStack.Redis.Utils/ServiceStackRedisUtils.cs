using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using ServiceStack.Text;
using ServiceStack.Logging;

using ServiceStack.Redis.Utils._00_Def;

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
        /// Get Is KeepAlive Mode
        /// </summary>
        public static bool IsKeepAliveMode { get; private set; }

        /// <summary>
        /// Get or Set RedisStatusChangedHandle
        /// </summary>
        public static Action<IEnumerable<RedisStatusInfo>> RedisStatusChangedHandle { get; set; }
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

            RedisConfig.EnableVerboseLogging = true;
            RedisConfig.DefaultRetryTimeout = RedisConfigSection.RetryTimeout;
            RedisConfig.DefaultConnectTimeout = RedisConfigSection.ConnectTimeout;

            if (RedisConfigSection.EnableLog)
            {
                LogManager.LogFactory = new MyLogFactory();
            }

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

                return client;
            }
            else
            {
                IRedisClient client = Prcm.GetReadOnlyClient();
                if (IsKeepAliveMode)
                {
                    var lsInvalidRedisHost = new List<string>();
                    var lsReadOnlyRedisHost = RedisConfigSection.ReadOnlyHosts.Split(',').Select(x => x.Trim()).ToArray();
                    while (true)
                    {
                        if (IsEquals(lsInvalidRedisHost, lsReadOnlyRedisHost))
                        {
                            client.Dispose();
                            client = null;

                            break;
                        }

                        if (KeepAliveService.Instance.IsAvaiable(client))
                        {
                            break;
                        }
                        else
                        {
                            lsInvalidRedisHost.Add(client.Host);

                            client.Dispose();
                            client = Prcm.GetReadOnlyClient();
                        }
                    }

                    if (client == null)
                    {
                        throw new Exception("No Avaliable Redis");
                    }
                }

                return client;
            }
        }

        private static bool IsEquals(IList<string> ls, IList<string> ls2)
        {
            if (ls.Count == ls2.Count && ls.Count > 0 && ls.Count > 0)
            {
                bool b = true;
                foreach (var item in ls)
                {
                    int n = ls2.Count(x => x.Equals(item));
                    if (n == 0)
                    {
                        b = false;

                        break;
                    }
                }

                return b;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
