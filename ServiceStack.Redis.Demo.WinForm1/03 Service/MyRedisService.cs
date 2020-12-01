using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RedisService;

namespace ServiceStack.Redis.Demo.WinForm1._03_Service
{
    public class MyRedisService
    {
        #region Property
        /// <summary>
        /// Get IUserInfoRedisService
        /// </summary>
        public IUserInfoRedisService UserInfoRedisService { get; private set; }

        /// <summary>
        /// Get Instance
        /// </summary>
        public static MyRedisService Instance { get; private set; }
        #endregion


        #region Constructor
        private MyRedisService()
        {

        }

        static MyRedisService()
        {
            Instance = new MyRedisService();
        }
        #endregion


        #region Method
        public void Init()
        {
            UserInfoRedisService = new UserInfoRedisService();
            UserInfoRedisService.ExpireTs = new TimeSpan(0, 1, 0);
            UserInfoRedisService.Prefix = "UserInfo_";
        }

        public void Exit()
        {

        }
        #endregion
    }
}
