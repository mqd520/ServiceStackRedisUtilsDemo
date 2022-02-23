using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ServiceStack.Redis.Utils._00_Def
{
    /// <summary>
    /// KeepAliveInfo
    /// </summary>
    public class KeepAliveInfo
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Pwd { get; set; }

        public TcpClient TcpClient { get; set; }

        public KeepAliveInfo(string ip, int port, string pwd)
        {
            Ip = ip;
            Port = port;
            Pwd = pwd;
        }
    }
}
