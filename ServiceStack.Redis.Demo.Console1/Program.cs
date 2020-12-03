using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using ServiceStack.Redis.Utils;

namespace ServiceStack.Redis.Demo.Console1
{
    class Program
    {
        static Timer _t = new Timer();


        static void Main(string[] args)
        {
            ServiceStackRedisUtils.Init();

            _t.AutoReset = false;
            _t.Interval = 1000;
            _t.Elapsed += _t_Elapsed;



            while (true)
            {
                string line = Console.ReadLine();
                if (line != null)
                {
                    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else
                    {
                        if (line.Equals("start", StringComparison.OrdinalIgnoreCase))
                        {
                            _t.Start();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static void _t_Elapsed(object sender, ElapsedEventArgs e)
        {
            _t.Stop();

            string str1 = "";
            try
            {
                using (var client = ServiceStackRedisUtils.GetReadOnlyClient())
                {
                    string value = client.Get<string>("AAtest001");
                    str1 = string.Format("Addr: {0}{1}Value: {2}{1}", client.Host + ":" + client.Port, Environment.NewLine, value);
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("Redis Exception: {0}", e1.Message);
            }

            Console.WriteLine(str1);

            _t.Start();
        }
    }
}
