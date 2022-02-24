using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.IO;

using log4net;
using log4net.Config;
using Common;
using ServiceStack.Redis.Utils;

namespace ServiceStack.Redis.Demo.Console1
{
    class Program
    {
        static int _nExitCode = 0;
        static Semaphore _semaphore = new Semaphore(0, 1);
        static Semaphore _semaphore2 = new Semaphore(0, 1);

        static System.Timers.Timer _t = new System.Timers.Timer();

        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            ConsoleHelper.WriteLine(ELogCategory.Info, string.Format("ServiceStack.Redis.Demo.Console1 Startup..."), true);

            ServiceStackRedisUtils.Init();
            ServiceStackRedisUtils.RedisStatusChangedHandle = ls =>
            {
                foreach (var item in ls)
                {
                    ConsoleHelper.WriteLine(
                        item.IsOnline ? ELogCategory.Info : ELogCategory.Warn,
                        string.Format("Redis Changed: {0}, IsOnline = {1}", item.Addr, item.IsOnline),
                        true
                    );
                }
            };

            #region Write key Task
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (_nExitCode > 0)
            //        {
            //            break;
            //        }

            //        IRedisClient client = null;
            //        try
            //        {
            //            //using (client = ServiceStackRedisUtils.GetClient())
            //            //{
            //            //    string str = Guid.NewGuid().ToString();
            //            //    client.Set("key", str);
            //            //    ConsoleHelper.WriteLine(
            //            //        ELogCategory.Info,
            //            //        string.Format("{0}: Set key = {1}", client.Host, str),
            //            //        true
            //            //    );
            //            //}
            //        }
            //        catch (Exception ex)
            //        {
            //            if (client != null)
            //            {
            //                client.Dispose();
            //            }

            //            ConsoleHelper.WriteLine(
            //                ELogCategory.Fatal,
            //                string.Format("Write Redis Key Task Exception: ", ex.Message),
            //                true,
            //                e: ex
            //            );
            //        }

            //        Thread.Sleep(1300);
            //    }

            //    _semaphore.Release();
            //});
            #endregion

            //Thread.Sleep(500);

            #region Read Key Task
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (_nExitCode > 0)
            //        {
            //            break;
            //        }

            //        try
            //        {
            //            using (var client = ServiceStackRedisUtils.GetReadOnlyClient())
            //            {
            //                string str = Guid.NewGuid().ToString();
            //                string value = client.Get<string>("key");
            //                ConsoleHelper.WriteLine(
            //                    ELogCategory.Info,
            //                    string.Format("{0}: Get key = {1}", client.Host, value),
            //                    true
            //                );
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            ConsoleHelper.WriteLine(
            //                ELogCategory.Fatal,
            //                string.Format("Read Redis Key Task Exception: ", ex.Message),
            //                true,
            //                e: ex
            //            );
            //        }

            //        Thread.Sleep(1 * 1000);
            //    }

            //    _semaphore2.Release();
            //});
            #endregion


            while (true)
            {
                string line = Console.ReadLine();
                if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Interlocked.Increment(ref _nExitCode);

                    _semaphore.WaitOne(1 * 1000);
                    _semaphore2.WaitOne(1 * 1000);

                    break;
                }
                else
                {
                    try
                    {
                        using (var client = ServiceStackRedisUtils.GetReadOnlyClient())
                        {
                            string value = client.Get<string>("key");
                            Console.WriteLine(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            //_t.AutoReset = false;
            //_t.Interval = 1000;
            //_t.Elapsed += _t_Elapsed;

            //while (true)
            //{
            //    string line = Console.ReadLine();
            //    if (line != null)
            //    {
            //        if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
            //        {
            //            break;
            //        }
            //        else
            //        {
            //            if (line.Equals("start", StringComparison.OrdinalIgnoreCase))
            //            {
            //                _t.Start();
            //            }
            //        }
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}

            Console.WriteLine("Program will be exited ...");
            System.Threading.Thread.Sleep(500);
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
