using SiemensTip.Extension;
using SiemensTip.Properties;
using SiemensTip.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiemensTip.src
{
    public class ProductPushOut
    {
        #region Fields
        Queue<Product> PushProduct = new Queue<Product>();
        CancellationTokenSource Token;
        #endregion

        #region Properties
        public bool IsWriteRes { get; set; }
        public bool IsWriteQuality { get; set; }
        public bool IsSaveImage { get; set; }
        public bool IsWriteNG { get; set; }
        #endregion

        #region LOCK
        private static object LOCKSIDE = new object();
        private static object _instanceLocker = new object();
        #endregion

        #region Initialize
        public ProductPushOut()
        {
            //是否写入原图
            IsWriteQuality = Settings.Default.IsWriteQuality;
            //是否写入结果图
            IsWriteRes = Settings.Default.IsWriteRes;
            //是否保存图片
            IsSaveImage = Settings.Default.IsSaveImage;
            IsWriteNG = Settings.Default.IsWriteNG;

            Token = new CancellationTokenSource();
            Task.Run(() =>
            {
                WriteProductTask(Token);
            });
        }
        #endregion

        #region Singleton
        private static ProductPushOut _instance;
        public static ProductPushOut Instance
        {
            get
            {
                lock (_instanceLocker)
                {
                    return _instance ?? (_instance = new ProductPushOut());
                }
            }
        }
        #endregion

        #region Methods
        private void WriteProductTask(CancellationTokenSource token)
        {
            while (!token.IsCancellationRequested)
            {
                Product product = null;
                lock (LOCKSIDE)
                {
                    while (PushProduct.Count <= 0)
                    {
                        Monitor.Wait(LOCKSIDE);
                        if (token.IsCancellationRequested)
                            return;
                    }
                    product = PushProduct.Dequeue();
                    Monitor.Pulse(LOCKSIDE);
                }
                if (IsSaveImage)
                {
                    Log.AppLog($"正在写入{product.WorkName}-{product.Id}产品图片");
                    product.PushOut(IsWriteNG, IsWriteRes, IsWriteQuality);
                    Log.AppLog($"产品图片{product.WorkName}-{product.Id}写入完成");
                }
                product.Dispose();
            }
        }
        public void Enqueue(Product product)
        {
            lock (LOCKSIDE)
            {
                while (PushProduct.Count > 5)
                {
                    Monitor.Wait(LOCKSIDE);
                    Thread.Sleep(5);
                }
                PushProduct.Enqueue(product);
                Monitor.Pulse(LOCKSIDE);
            }
        }

        public bool Stop()
        {
            Token.Cancel();
            return true;
        }
        #endregion

    }
}
