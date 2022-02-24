using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.Net.Sockets;

using Common;

using ServiceStack.Redis.Utils._00_Def;
using ServiceStack.Redis.Utils._02_Common;

namespace ServiceStack.Redis.Utils
{
    /// <summary>
    /// KeepAliveService
    /// </summary>
    internal class KeepAliveService
    {
        #region Field
        private object _obj = new object();

        private IList<KeepAliveInfo> _lsKeepAliveInfos = new List<KeepAliveInfo>();

        private IList<RedisStatusInfo> _lsStatusInfos = new List<RedisStatusInfo>();

        private Timer _tKeepAlive = new Timer();
        #endregion


        #region Property
        /// <summary>
        /// Get Instance
        /// </summary>
        public static KeepAliveService Instance { get; private set; } = null;
        #endregion


        #region Constructor
        private KeepAliveService()
        {
            _tKeepAlive.AutoReset = false;
            _tKeepAlive.Elapsed += _tKeepAlive_Elapsed;
        }

        static KeepAliveService()
        {
            if (Instance == null)
            {
                Instance = new KeepAliveService();
            }
        }
        #endregion


        #region Method
        /// <summary>
        /// Init
        /// </summary>
        public void Init()
        {
            _tKeepAlive.Interval = ServiceStackRedisUtils.RedisConfigSection.KeepAliveInterval;
            string[] arrR = ServiceStackRedisUtils.RedisConfigSection.ReadOnlyHosts.Split(',').Select(x => x.Trim()).ToArray();
            foreach (var item in arrR)
            {
                var info = RedisConnectionInfoTool.Parse(item);
                _lsKeepAliveInfos.Add(new KeepAliveInfo(info.Ip, info.Port, info.Pwd));

                _lsStatusInfos.Add(new RedisStatusInfo
                {
                    Addr = string.Format("{0}:{1}", info.Ip, info.Port),
                    IsOnline = true
                });
            }

            _tKeepAlive.Start();
        }

        /// <summary>
        /// Exit
        /// </summary>
        public void Exit()
        {
            _tKeepAlive.Stop();
            _tKeepAlive.Dispose();
        }

        /// <summary>
        /// Is Avaiable
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsAvaiable(IRedisClient client)
        {
            string addr = string.Format("{0}:{1}", client.Host, client.Port);
            var item = _lsStatusInfos.FirstOrDefault(x => x.Addr.Equals(addr));
            if (item != null)
            {
                return item.IsOnline;
            }

            return false;
        }
        #endregion


        #region Event Callback
        private void _tKeepAlive_Elapsed(object sender, ElapsedEventArgs e)
        {
            _tKeepAlive.Stop();

            IList<RedisStatusInfo> ls = new List<RedisStatusInfo>();

            foreach (var item in _lsKeepAliveInfos)
            {
                bool bOnline = false;
                string addr = string.Format("{0}:{1}", item.Ip, item.Port);

                bool bIsConnected = true;
                if (item.TcpClient == null || (item.TcpClient != null && item.TcpClient.Client == null))
                {
                    item.TcpClient = new TcpClient();
                    bIsConnected = false;
                }

                bool bIsNeedSendPwd = false;
                if (!bIsConnected)
                {
                    bIsNeedSendPwd = true;
                    bIsConnected = StartTcpClientConnect(item.TcpClient, item.Ip, item.Port);
                    if (!bIsConnected)
                    {
                        item.TcpClient.Close();
                        item.TcpClient = null;

                        ConsoleHelper.WriteLine(
                            ELogCategory.Warn,
                            string.Format("Connect to Redis Failed: {0}:{1}", item.Ip, item.Port),
                            true
                        );
                    }
                }

                if (bIsConnected)
                {
                    bool bSendPing = true;
                    if (bIsNeedSendPwd)
                    {
                        bool bIsLoginSuccess = SendPwd(item.TcpClient, item.Pwd);
                        bSendPing = bIsLoginSuccess;
                    }

                    if (bSendPing)
                    {
                        bOnline = SendPing(item.TcpClient);
                    }
                }

                ls.Add(new RedisStatusInfo
                {
                    Addr = addr,
                    IsOnline = bOnline
                });
            }

            var lsStatusChanged = new List<RedisStatusInfo>();
            foreach (var item in ls)
            {
                var item1 = _lsStatusInfos.FirstOrDefault(x => x.Addr.Equals(item.Addr));
                if (item1 != null)
                {
                    if (item.IsOnline != item1.IsOnline)
                    {
                        lsStatusChanged.Add(new RedisStatusInfo
                        {
                            Addr = item1.Addr,
                            IsOnline = item.IsOnline
                        });
                    }
                    item1.IsOnline = item.IsOnline;
                }
            }

            if (lsStatusChanged.Count > 0)
            {
                if (ServiceStackRedisUtils.RedisStatusChangedHandle != null)
                {
                    try
                    {
                        ServiceStackRedisUtils.RedisStatusChangedHandle.Invoke(lsStatusChanged);
                    }
                    catch (Exception ex)
                    {
                        CommonLogger.WriteLog(
                            ELogCategory.Fatal,
                            string.Format("ServiceStack.Redis.Utils Redis Status Changed Exception: ", ex.Message),
                            e: ex
                        );
                    }
                }
            }

            _tKeepAlive.Start();
        }

        private bool SendPing(TcpClient client)
        {
            var buf = RedisCmdUtils.Ping();
            ConsoleHelper.WriteLine(
                ELogCategory.Info,
                string.Format("Send Ping To {0}", client.Client.RemoteEndPoint.ToString()),
                true
            );
            var buf2 = SendAndRecv(client, buf, 0, buf.Length, 1 * 1000, 3 * 1000);
            if (buf2 != null)
            {
                string str = Encoding.UTF8.GetString(buf2);
                ConsoleHelper.WriteLine(
                    ELogCategory.Info,
                    string.Format("Recv Pong From {0}: {1}", client.Client.RemoteEndPoint.ToString(), str),
                    true
                );
                if (str.Contains("PONG"))
                {
                    return true;
                }
            }

            ConsoleHelper.WriteLine(
                ELogCategory.Warn,
                string.Format("Recv Pong From {0} Failed.", client.Client.RemoteEndPoint.ToString()),
                true
            );

            return false;
        }

        private bool SendPwd(TcpClient client, string pwd)
        {
            if (!string.IsNullOrEmpty(pwd))
            {
                return true;
            }

            return true;
        }

        private bool StartTcpClientConnect(TcpClient client, string ip, int port)
        {
            try
            {
                client.ConnectAsync(ip, port).Wait(ServiceStackRedisUtils.RedisConfigSection.ConnectTimeout);
            }
            catch (Exception ex)
            {
                CommonLogger.WriteLog(
                    ELogCategory.Fatal,
                    string.Format("ServiceStack.Redis.Utils KeepAlive Connect Redis Exception: {0}", ex.Message),
                    e: ex
                );
            }

            return client.Connected;
        }

        private byte[] SendAndRecv(TcpClient client,
            byte[] buf, int offset, int length,
            int writeTimeout, int recvTimeout)
        {
            byte[] buf2 = null;
            //Console.WriteLine(string.Format("Client Socket: {0}", client.Client.Handle));

            try
            {
                var ns = client.GetStream();
                ns.WriteAsync(buf, 0, buf.Length).Wait(writeTimeout);

                long timestamp = DateTime.Now.GetMilliTimestamp();
                while (true)
                {
                    System.Threading.Thread.SpinWait(100);

                    if (client.Available > 0)
                    {
                        buf2 = new byte[client.Available];
                        ns.Read(buf2, 0, buf2.Length);

                        break;
                    }

                    long timestamp2 = DateTime.Now.GetMilliTimestamp();
                    if (timestamp2 - timestamp > recvTimeout)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                client.Close();

                CommonLogger.WriteLog(
                    ELogCategory.Fatal,
                    string.Format("ServiceStack.Redis.Utils SendAndRecv Exception: {0}", ex.Message),
                    e: ex
                );
            }

            return buf2;
        }
        #endregion
    }
}
