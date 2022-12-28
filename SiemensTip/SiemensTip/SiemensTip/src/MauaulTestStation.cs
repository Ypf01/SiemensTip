using BMS_tip_wrapper;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SiemensTip.Extension;
using SiemensTip.Extension.helpers;
using SiemensTip.Helper;
using SiemensTip.src.Model;
using SiemensTip.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SiemensTip.src
{
    public class MauaulTestStation : IWork
    {
        Action<List<ImageDisplayViewModel>, string, List<string>> PushDisResult;
        private CancellationTokenSource Token;
        public int TempResult { get; set; }
        public int StartIndex { get; set; }
        public int CameraNumber { get; set; }
        public Task[] PhotoGraph { get; set; }
        public List<Task> Inspect { get; set; }
        public Product TProduct { get; set; }
        public MauaulTestStation(Action<List<ImageDisplayViewModel>, string, List<string>> action, CancellationTokenSource _token, int num, int _startIndex)
        {
            Token = _token;
            PushDisResult = action;
            Inspect = new List<Task>();
            PhotoGraph = new Task[num];
            CameraNumber = num;
            StartIndex = _startIndex;
        }
        public Task InspectTask()
        {
            return Task.Run(() =>
            {
                int pos = 0;
                while (!Token.IsCancellationRequested)
                {
                    if (Common.IsRunSts && Common.PlcContent && !Common.IsStart)
                    {
                        Log.AppLog($"手动测试:等待D710不为0");
                        while (pos > 8 || pos <= 0)
                        {
                            pos = Common.GetValue<int>(ConstHelper.ExternalTrigger);
                            Thread.Sleep(40);
                            if (!Common.PlcContent || Common.IsStart)
                            {
                                Log.ErrorLog($"检测到{(Common.PlcContent ? "自动状态开始" : "PLC断线")},手动检测程序退出");
                                if (TProduct != null)
                                    TProduct.Dispose();
                                break;
                            }
                        }
                        if (!Common.PlcContent || Common.IsStart)
                            continue;
                        Common.Write(ConstHelper.ExternalTrigger, 0);
                        if (!Common.IsStart && pos >= 1 && pos <= 8)
                        {
                            Log.AppLog($"手动测试:等待获取第 {pos} 个相机图片");
                            ImageBufData photoResult = Common.HikCameras[pos - 1].WaitOnImageDataGet();
                            if (photoResult != null)
                            {
                                Log.AppLog($"手动测试:获取第 {pos} 个相机图片成功");
                                Mat mat = photoResult.ConvertImageType();

                                #region 算法处理
                                //Cv2.CvtColor(TProduct.Images[thepos, index], TProduct.Images[thepos, index], ColorConversionCodes.BGR2GRAY); //如果是彩图则先转成灰图
                                Mat resMat;
                                short result = 0;
                                List<bool> lstBool;
                                if (Common.TemplateSts)
                                {
                                    try
                                    {
                                        if (pos % 2 == 0)  //bottom 如果为零则是下相机，不为零是上相机
                                        {
                                            List<OpenCvSharp.Point> pix = Common.CutArray(pos > 5 ? ConstHelper.Right : ConstHelper.Left, 0);
                                            Common.Sol.measure_circles(mat, pix, out resMat, out List<Solution.Circle> circles,out lstBool);
                                            resMat.ImWrite("./bottom_viz.png");
                                            if (lstBool == null || lstBool.Count != ConstHelper.ProductNumber)
                                                result = 0;
                                            else
                                                for (int k = 0; k < lstBool.Count; k++)
                                                    if (lstBool[k])
                                                        result += (short)Math.Pow(2, k);
                                        }
                                        else
                                        {
                                            Common.Sol.detect_spines(mat, out resMat, out List<float> distances, out lstBool);
                                            resMat.ImWrite("./lateral_viz.png");
                                            if (lstBool == null || lstBool.Count != ConstHelper.ProductNumber)
                                                result = 0;
                                            else
                                                for (int k = 0; k < lstBool.Count; k++)
                                                    if (lstBool[k])
                                                        result += (short)Math.Pow(2, k);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.ErrorLog("手动测试异常："+ex.Message);
                                        Common.TemplateSts = false;
                                        resMat = mat;
                                        result = 0;
                                    }
                                }
                                else
                                {
                                    resMat = mat;
                                    result = 0;
                                }
                                #endregion

                                Common.ManaulMat = mat.Clone();

                                var v = Convert.ToString(result, 2).PadLeft(ConstHelper.ProductNumber, '0').ToCharArray();
                                bool flag = true;
                                for (short i = 0; i < ConstHelper.ProductNumber; i++)
                                    flag &= v[i] == '1';
                                List<ImageDisplayViewModel> images = new List<ImageDisplayViewModel>();
                                Application.Current.Dispatcher?.Invoke(() =>
                                {
                                    images.Add(new ImageDisplayViewModel() { DisImage = resMat.ToBitmapSource() });
                                });
                                PushDisResult?.Invoke(images, ConstHelper.Muaual, new List<string> { flag ? ConstHelper.OK : ConstHelper.NG });
                                mat.Dispose();
                                resMat?.Dispose();
                            }
                            else
                                Log.ErrorLog($"手动测试:相机{pos}取图超时");
                        }
                        else
                        {
                            if (Common.IsStart)
                                Log.AppLog($"手动测试:取第{pos}个相机图片取消");
                            else
                                Log.ErrorLog($"手动测试:时相机索引{pos}错误");
                        }
                        Thread.Sleep(40);
                        pos = 0;
                    }

                    #region 测试
                    //else
                    //{
                    //    List<ImageDisplayViewModel> bitmaps = new List<ImageDisplayViewModel>();
                    //    Mat mat = null;
                    //    if (File.Exists(@"D:\SiemensTip项目" + "\\" + pos.ToString() + ".jpg"))
                    //    {
                    //        FileStream fs = new FileStream(@"D:\SiemensTip项目" + "\\" + pos.ToString() + ".jpg", FileMode.Open);
                    //        mat = Mat.FromStream(fs, ImreadModes.Color);
                    //        fs.Close();
                    //    }
                    //    Product product = new Product(2, "sd");
                    //    ImageSource image = null;
                    //    Application.Current.Dispatcher?.Invoke(() =>
                    //    {
                    //        image = mat.ToBitmapSource();
                    //        product.DisplayImages.Add(mat.ToBitmapSource());                        
                    //    });
                    //   ImageSource source = product.DisplayImages[0];
                    //    bitmaps.Add(new ImageDisplayViewModel() { DisImage = image });
                    //    PushDisResult?.Invoke(bitmaps, ConstHelper.Muaual, null);
                    //    pos++;
                    //    if (pos >= 8)
                    //        pos = 0;
                    //    Thread.Sleep(1000);
                    //}
                    #endregion

                    Thread.Sleep(100);
                }
            });
        }
    }
}
