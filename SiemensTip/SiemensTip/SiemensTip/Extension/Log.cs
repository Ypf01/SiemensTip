using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SiemensTip.Extension
{
    public static class Log
    {

        #region Log接口
        private static ILog CommLogger
        {
            get
            {
                var result = LogManager.GetLogger("DataLabLogger");

                if (result == null)
                    throw new Exception("Can't get DataLabLogger");

                return (ILog)result;
            }
        }

        private static ILog AppLogger
        {
            get
            {
                var result = LogManager.GetLogger("AppLabLogger");

                if (result == null)
                    throw new Exception("Can't get AppLabLogger");

                return (ILog)result;
            }
        }

        private static ILog ErrorLogger
        {
            get
            {
                var result = LogManager.GetLogger("ErrorLogger");

                if (result == null)
                    throw new Exception("Can't get ErrorLogger");

                return (ILog)result;
            }
        }
        #endregion

        #region 日志录入
        public static void AppLog(string message)
        {
            Console.WriteLine(message);
            AppLogger.Info(message);
        }

        public static void CommunicationLog(string message)
        {
            CommLogger.Info(message);
        }

        public static void ErrorLog(string message, bool IsPopUp = false)
        {
            Console.WriteLine(message);
            if (IsPopUp)
                MessageBox.Show(message, "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
            ErrorLogger.Error(message);
        }
        #endregion

        #region 初始化

        static Log()
        {
            if (!Init())
                throw new Exception("Can't init logger");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static bool Init()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Extension\\log4net.config";
            if (!File.Exists(path))
                return false;
            FileInfo logCfg = new FileInfo(path);
            try
            {
                XmlConfigurator.ConfigureAndWatch(logCfg);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion

    }
}
