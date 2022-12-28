using BMS_tip_wrapper;
using SiemensTip.Extension;
using SiemensTip.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SiemensTip
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Common.TemplateSts = Common.Instance.GetTemplate();
            Common.IsRunSts = Common.Instance.Init();
            if (!Common.IsRunSts && !Common.TemplateSts)
            {
                //相机或PLC连接异常时弹出窗体展示信息后关闭
                Log.AppLog("相机与PLC已被关闭");
                Common.Instance.Stop();
            }
            if (!Common.Instance.ReadPixelOffset())
            {
                Common.IsRunSts &= false;
                Log.ErrorLog("像素位置偏移值数组读取失败", true);
            }
            MainView mainView = new MainView();
            mainView.Show();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.ErrorLog(e.ToString());
            try
            {
                MessageBox.Show("捕获未处理异常:" + e.Exception.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("程序发生致命错误，将终止，请联系运营商！" + ex.Message);
            }
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.ErrorLog(e.ToString());
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("程序发生致命错误，将终止，请联系运营商！\n");
            }
            sbEx.Append("捕获未处理异常：");

            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            MessageBox.Show(sbEx.ToString());
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.ErrorLog(e.ToString());
            MessageBox.Show("捕获线程内未处理异常：" + e.Exception.Message);
            e.SetObserved();
        }
    }
}
