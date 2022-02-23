using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.Redis.Utils._00_Def
{
    /// <summary>
    /// Redis Status Info
    /// </summary>
    public class RedisStatusInfo
    {
        public string Addr { get; set; }

        public bool IsOnline { get; set; }
    }
}
