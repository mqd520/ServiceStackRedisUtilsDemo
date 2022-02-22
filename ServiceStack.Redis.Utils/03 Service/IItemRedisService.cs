using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.Redis.Utils._03_Service
{
    public interface IItemRedisService<T> : IRedisService<T>
    {
        /// <summary>
        /// Get Item
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetItem(string key);

        /// <summary>
        /// Set Item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool SetItem(string key, T item);
    }
}
