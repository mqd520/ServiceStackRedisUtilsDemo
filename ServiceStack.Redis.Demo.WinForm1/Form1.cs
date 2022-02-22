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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var userinfo = MyRedisService.Instance.UserInfoRedisService.GetItemByUserName("AAtest043");
            var str = JsonConvert.SerializeObject(userinfo, Formatting.Indented);

            MessageBox.Show(str, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
