using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.Redis.Utils._02_Common
{
    /// <summary>
    /// Redis Cmd Utils
    /// </summary>
    public static class RedisCmdUtils
    {
        static RedisCmdUtils()
        {

        }

        public static byte[] Ping()
        {
            //var cmd = string.Format("*1{0}$4{0}PING{0}", Environment.NewLine);
            //byte[] buf = Encoding.UTF8.GetBytes(cmd);
            var buf = new byte[] { 0x2a, 0x31, 0x0d, 0x0a, 0x24, 0x34, 0x0d, 0x0a, 0x50, 0x49, 0x4e, 0x47, 0x0d, 0x0a };

            return buf;
        }

        public static byte[] Auth(string pwd)
        {
            var cmd = string.Format("*2{0}$4{0}Auth{0}${1}{0}{2}{0}", Environment.NewLine, pwd.Length, pwd);
            byte[] buf = Encoding.UTF8.GetBytes(cmd);

            return buf;
        }
    }
}
