using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack.Redis;

using Common;
using ServiceStack.Redis.Utils;
using Def;

namespace RedisService
{
    /// <summary>
    /// UserInfo RedisService
    /// </summary>
    public class UserInfoRedisService : ItemRedisService<UserInfo>, IUserInfoRedisService
    {
        /// <summary>
        /// Get Item By UserName
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public UserInfo GetItemByUserName(string username)
        {
            string key = string.Format("{0}{1}", Prefix, username);
            return GetItem(key);
        }

        /// <summary>
        /// Set Item By UserName
        /// </summary>
        /// <param name="username"></param>
        /// <param name="item"></param>
        public void SetItemByUserName(string username, UserInfo item)
        {
            string key = string.Format("{0}{1}", Prefix, username);
            SetItem(key, item);
        }
    }
}
