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
        /// PooledRedisClientManager
        /// </summary>
        public static PooledRedisClientManager Prcm { get; private set; }

        public static IRedisClientsManager _mgr;
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
            RedisConfigurationSection redis = ConfigurationManager.GetSection("redis") as RedisConfigurationSection;
            string[] arrRW = redis.ReadWriteHosts.Split(',').Select(x => x.Trim()).ToArray();
            string[] arrR = redis.ReadOnlyHosts.Split(',').Select(x => x.Trim()).ToArray();

            Prcm = new PooledRedisClientManager(arrRW, arrR, new RedisClientManagerConfig
            {
                MaxWritePoolSize = redis.MaxWritePoolSize,
                MaxReadPoolSize = redis.MaxReadPoolSize,
                AutoStart = redis.AutoStart
            });


            JsConfig.DateHandler = DateHandler.ISO8601DateTime;
            JsConfig.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            //IList<string> ls = new List<string>();
            //ls.Add("192.168.0.60:6380");
            //ls.Add("192.168.0.60:6381");
            //RedisSentinel rs = new RedisSentinel(ls);
            //_mgr = rs.Start();
            //rs.OnFailover = x =>
            //{
            //    Console.WriteLine("ssssssssssssssssssssssssssssssssssss");
            //};
        }

        /// <summary>
        /// Exit
        /// </summary>
        public static void Exit()
        {
            Prcm.Dispose();
        }

        /// <summary>
        /// Get Client
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetClient()
        {
            return Prcm.GetClient();
        }

        /// <summary>
        /// Get Read Only Client
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetReadOnlyClient()
        {
            return Prcm.GetReadOnlyClient();
        }
        #endregion
    }
}
