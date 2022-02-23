using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

using Common;
using Def;
using ServiceStack.Redis.Utils;
using ServiceStack.Redis.Utils._03_Service;

using ServiceStack.Redis.Demo.WinForm1._03_Service;

namespace ServiceStack.Redis.Demo.WinForm1
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer t = new System.Timers.Timer();

        public Form1()
        {
            t.Interval = 1000;
            t.AutoReset = true;
            t.Elapsed += T_Elapsed;

            InitializeComponent();
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var userinfo = MyRedisService.Instance.UserInfoRedisService.GetItemByUserName("AAtest043");
            //var str = JsonConvert.SerializeObject(userinfo, Formatting.Indented);

            //MessageBox.Show(str, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            using (var client = ServiceStackRedisUtils.GetClient())
            {
                var value = client.Get<string>("key");
                System.Diagnostics.Debug.WriteLine(string.Format("key = {0}", value));
            }


            //RedisNativeClient client2 = new RedisNativeClient("192.168.0.201", 6379);
            //client2.Ping();
            //return;

            //var buf3 = System.Text.Encoding.UTF8.GetBytes("PING");
            //byte[][] cmdWithBinaryArgs = new byte[][] { buf3 };
            //WriteAllToSendBuffer(cmdWithBinaryArgs);

            //var client = new System.Net.Sockets.TcpClient();
            //client.Connect("192.168.0.154", 6379);
            //var s = client.GetStream();
            //string cmd = string.Format("*1{0}$4{0}PING{0}", Environment.NewLine);
            //var buf = System.Text.Encoding.UTF8.GetBytes(cmd);
            //var buf = new byte[] { 0x2a, 0x32, 0x0d, 0x0a, 0x24, 0x33, 0x0d, 0x0a, 0x47, 0x45, 0x54, 0x0d, 0x0a, 0x24, 0x33, 0x0d, 0x0a, 0x6b, 0x65, 0x79, 0x0d, 0x0a };
            //s.Write(buf, 0, buf.Length);
            //s.Flush();
            //while (true)
            //{
            //    if (client.Available > 0)
            //    {
            //        var buf2 = new byte[client.Available];
            //        s.Read(buf2, 0, buf2.Length);

            //        string str = System.Text.Encoding.UTF8.GetString(buf2);
            //        System.Diagnostics.Debug.WriteLine(str);

            //        break;
            //    }
            //}
        }

        public void WriteAllToSendBuffer(params byte[][] cmdWithBinaryArgs)
        {
            var buf = GetCmdBytes('*', cmdWithBinaryArgs.Length);
        }

        private byte[] GetCmdBytes(char cmdPrefix, int noOfLines)
        {
            string str = noOfLines.ToString();
            int length = str.Length;
            byte[] buffer = new byte[] { (byte)cmdPrefix };
            for (int i = 0; i < length; i++)
            {
                buffer[i + 1] = (byte)str[i];
            }
            buffer[1 + length] = 13;
            buffer[2 + length] = 10;
            return buffer;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = string.Format("AAtest{0}", RandTool.CreateRandValWithMinMax(1, 100).ToString().PadLeft(3, '0'));
            var userinfo = new UserInfo
            {
                Id = RandTool.CreateRandValWithMinMax(1, 100000),
                NickName = Guid.NewGuid().ToString(),
                Pwd = "123456",
                UserName = username,
                DateTime = DateTime.Now
            };

            MyRedisService.Instance.UserInfoRedisService.SetItemByUserName(username, userinfo);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string str = "";

            using (var client = ServiceStackRedisUtils.GetReadOnlyClient())
            {
                string info = client.Get<string>("AAtest001");
                str = string.Format("Addr: {0}{1}Info: {2}", client.Host + ":" + client.Port, Environment.NewLine, info);
            }

            MessageBox.Show(str, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
