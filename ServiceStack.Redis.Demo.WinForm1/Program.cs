using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Newtonsoft.Json;

using Common;
using ServiceStack.Redis.Utils;

using ServiceStack.Redis.Demo.WinForm1._03_Service;

namespace ServiceStack.Redis.Demo.WinForm1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            ConsoleHelper.WriteLine(ELogCategory.Info, string.Format("ServiceStack.Redis.Demo.WinForm1 Startup..."), true);

            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                jss.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                return jss;
            };

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;

            ServiceStackRedisUtils.Init();
            MyRedisService.Instance.Init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            CommonLogger.WriteLog(
                ELogCategory.Fatal,
                string.Format("Application_ThreadException Exception: {0}", e.Exception.Message),
                e.Exception
            );

            MessageBox.Show(string.Format("Application_ThreadException Exception: {0}{1}", Environment.NewLine, e.Exception.Message));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            CommonLogger.WriteLog(
                ELogCategory.Fatal,
                string.Format("CurrentDomain_UnhandledException Exception: {0}", exception.Message),
                exception
            );

            MessageBox.Show(string.Format("CurrentDomain_UnhandledException Exception: {0}{1}", Environment.NewLine, exception.Message));
        }
    }
}
