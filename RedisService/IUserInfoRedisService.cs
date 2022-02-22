using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack.Redis.Utils;
using ServiceStack.Redis.Utils._03_Service;
using Def;

namespace RedisService
{
    public interface IUserInfoRedisService : IItemRedisService<UserInfo>
    {
        /// <summary>
        /// Get Item By UserName
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        UserInfo GetItemByUserName(string username);

        /// <summary>
        /// Set Item By UserName
        /// </summary>
        /// <param name="username"></param>
        /// <param name="item"></param>
        void SetItemByUserName(string username, UserInfo item);
    }
}
