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
    public class RightStation : IWork
    {
        //5上6下 7上8下
        Action<List<ImageDisplayViewModel>, string, List<string>> PushDisResult;
        private CancellationTokenSource Token;
        private int temp = 0;
        public int TempResult { get; set; }
        public int StartIndex { get; set; }
        public List<Task> Inspect { get; set; }
        public Product TProduct { get; set; }
        public int CameraNumber { get; set; }
        public Task[] PhotoGraph { get; set; }
        /// <summary>
        /// 右相机构造函数
        /// </summary>
        /// <param name="action">图像上传委托</param>
        /// <param name="_token">线程循环标志</param>
        /// <param name="num">相机数量</param>
        /// <param name="_startIndex">相机索引起始位置</param>
        public RightStation(Action<List<ImageDisplayViewModel>, string, List<string>> action, CancellationTokenSource _token, int num, int _startIndex)
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
                    if (Common.IsRunSts && Common.PlcContent && Common.IsStart && Common.TemplateSts)
                    {
                        Log.AppLog(ConstHelper.Right + $"等待触发拍照.....{temp}");
                        while (pos > ConstHelper.PhotoNum || pos <= 0)
                        {
                            pos = Common.GetValue<int>(ConstHelper.RightCamera);
                            Thread.Sleep(20);
                            if (!Common.PlcContent || !Common.IsStart)
                            {
                                Log.ErrorLog($"检测到{(Common.PlcContent ? "自动状态停止" : "PLC断线")},右工位检测程序退出");
                                if (TProduct != null)
                                {
                                    if (Inspect != null && Inspect.Count > 0)
                                        Task.WaitAll(Inspect.ToArray());
                                    TProduct.Dispose();
                                    Inspect.Clear();
                                    TProduct = null;
                                }
                                break;
                            }
                        }
                        Log.AppLog(ConstHelper.Right + $"上次触发指令{temp}，此次触发指令{pos}");
                        if (!Common.PlcContent || !Common.IsStart)
                            continue;
                        if (pos == 1 || TProduct == null) //当拍照次数为1时清除上次的结果 清除算法任务
                        {
                            if (TProduct != null)
                            {
                                if (Inspect != null && Inspect.Count > 0)
                                    Task.WaitAll(Inspect.ToArray());
                                TProduct.Dispose();
                            }
                            Inspect.Clear();
                            TProduct = new Product(CameraNumber, ConstHelper.Right);
                        }
                        //收到拍照命令信号,回应下位机
                        Common.Write(ConstHelper.RightCamera, 0);
                        for (int i = 0; i < CameraNumber; i++)
                        {
                            var index = i;
                            //当前产品拍照次数
                            var theps = pos - 1;
                            PhotoGraph[index] = Task.Run(() =>
                            {
                                ImageBufData photoResult = Common.HikCameras[index + StartIndex].WaitOnImageDataGet();
                                //可以在此处判断  TProduct.Images[index] 是否为空,如果为空可以根据index+StartIndex找到相机编号请求再拍一次
                                if (TProduct.Images[theps, index] != null)
                                    TProduct.Images[theps, index].Dispose();
                                if (photoResult == null)
                                    Log.ErrorLog(ConstHelper.Right + $" 第{theps + 1}次拍照时第{index + StartIndex + 1}相机取图失败");
                                TProduct.Images[theps, index] = photoResult != null ? photoResult.ConvertImageType() : new Mat(400, 400, MatType.CV_8UC1);
                            });
                        }
                        Task.WaitAll(PhotoGraph);
                        //相机拍照完成，允许PLC运动
                        Common.Write(ConstHelper.RightCameraFinish, pos);
                        //算法任务
                        //当前产品拍照次数
                        var thepos = pos - 1;
                        Inspect.Add(Task.Run(() =>
                        {
                            for (int i = 0; i < CameraNumber; i++)
                            {
                                var index = i;                        
                                DateTime dt = DateTime.Now;
                                Log.AppLog(TProduct.Id + $"第{index + 1 + StartIndex}个相机{thepos + 1}次算法进入");

                                #region 算法处理
                                string str = string.Empty;
                                //Cv2.CvtColor(TProduct.Images[thepos, index], TProduct.Images[thepos, index], ColorConversionCodes.BGR2GRAY); //如果是彩图则先转成灰图
                                Mat resMat;
                                short result = 0;
                                if (TProduct.ResImages[thepos, index] != null)
                                    TProduct.ResImages[thepos, index].Dispose();
                                List<bool> lstBool; resMat = TProduct.Images[thepos, index];
                                if ((StartIndex + index + 1) % 2 == 0)  //bottom 如果为零则是下相机，不为零是上相机
                                {
                                    str = "Bottom: ";
                                    List<OpenCvSharp.Point> pix = Common.CutArray(ConstHelper.Right, thepos * ConstHelper.ProductNumber + (index == 1 ? 0 : ConstHelper.MaterialNum / 2));
                                    Common.Sol.measure_circles(TProduct.Images[thepos, index].Clone(), pix, out resMat, out List<Solution.Circle> circles, out lstBool);
                                    if (lstBool == null || lstBool.Count != ConstHelper.ProductNumber)
                                        result = 0;
                                    else
                                    {
                                        lstBool.Reverse();
                                        for (int k = 0; k < lstBool.Count; k++)
                                        {
                                            if (lstBool[k])
                                                result += (short)Math.Pow(2, k);
                                            str += i.ToString() + " :" + circles[k].r;
                                        }
                                    }
                                    result = (short)new Random().Next(0, 15);
                                }
                                else
                                {
                                    str = "Lateral: ";
                                    Common.Sol.detect_spines(TProduct.Images[thepos, index].Clone(), out resMat, out List<float> distances, out lstBool);
                                    if (lstBool == null || lstBool.Count != ConstHelper.ProductNumber)
                                        result = 0;
                                    else
                                        for (int k = 0; k < lstBool.Count; k++)
                                        {
                                            if (lstBool[k])
                                                result += (short)Math.Pow(2, k);
                                            str += i.ToString() + " :" + distances[k];
                                        }
                                    result = (short)new Random().Next(0, 15);
                                }
                                Log.AppLog(TProduct.Id + $"第{index + 1 + StartIndex}个相机第{thepos + 1}次拍照结果" + str);
                                TProduct.ResImages[thepos, index] = resMat ?? new Mat(400, 400, MatType.CV_8UC1);
                                #endregion

                                //最终结果
                                TProduct.Results[thepos, index] = result;
                                //算法完成标识
                                TProduct.InspectResult[thepos, index] = true;

                                Log.AppLog(TProduct.Id + $"第{index + 1 + StartIndex}个相机{thepos + 1}次算法完成" + "      " + "耗时：" + (DateTime.Now - dt).TotalMilliseconds);
                            }
                        }));
                        #region 判断上次算法结果是否已完成，如果完成则更新到UI
                        if (pos >= 2 && pos <= ConstHelper.PhotoNum)
                        {
                            bool flag = true;
                            for (int i = 0; i < CameraNumber; i++)
                                flag &= TProduct.InspectResult[pos - 2, i];
                            if (flag)
                            {
                                List<ImageDisplayViewModel> bitmaps = new List<ImageDisplayViewModel>();
                                for (int i = 0; i < CameraNumber; i++)
                                {
                                    //缩放结果图便于展示
                                    Mat mat = new Mat();
                                    Cv2.Resize(TProduct.ResImages[pos - 2, i], mat, new OpenCvSharp.Size(0, 0), 0.25, 0.25);
                                    ImageSource imageSource = null;
                                    Application.Current.Dispatcher?.Invoke(new Action(() =>
                                    {
                                        //可以重复执行某一次拍照
                                        imageSource = mat.ToBitmapSource();
                                        if (TProduct.DisplayImages.Count > 0 && TProduct.DisplayImages.Count - 1 >= (pos - 2) * CameraNumber + i)
                                            TProduct.DisplayImages[(pos - 2) * CameraNumber + i] = imageSource;
                                        else
                                            TProduct.DisplayImages.Add(imageSource);
                                    }));
                                    bitmaps.Add(new ImageDisplayViewModel() { DisImage = imageSource });
                                    mat.Dispose();
                                }
                                int cols = TProduct.Results.GetLength(1);
                                int thenRowsResult = 0;
                                TempResult = 0;
                                for (int j = 0; j < pos - 1; j++)
                                {
                                    thenRowsResult = 0;
                                    //获得当前行所有结果
                                    for (int k = 0; k < cols; k += 2)
                                        thenRowsResult = ((TProduct.Results[j, k] & TProduct.Results[j, k + 1]) << ConstHelper.MaterialNum / 2 * (k / 2)) | thenRowsResult;
                                    TempResult = (thenRowsResult << j * ConstHelper.ProductNumber) | TempResult;
                                }
                                PushDisResult?.Invoke(bitmaps, ConstHelper.Right, ConstHelper.IntToStringArray(TempResult, pos - 1));
                            }
                        }
                        #endregion

                        #region 当前产品拍照完成,写入PLC最终结果
                        //当前产品已全部拍完
                        if (pos == ConstHelper.PhotoNum)
                        {
                            Task.WaitAll(Inspect.ToArray());
                            int rows = TProduct.Results.GetLength(0);
                            int cols = TProduct.Results.GetLength(1);
                            int thenRowsResult = 0;
                            for (int j = 0; j < rows; j++)
                            {
                                thenRowsResult = 0;
                                //获得当前行所有结果
                                for (int k = 0; k < cols; k += 2)
                                    thenRowsResult = ((TProduct.Results[j, k] & TProduct.Results[j, k + 1]) << ConstHelper.MaterialNum / 2 * (k / 2)) | thenRowsResult;
                                TProduct.ResultNum = (thenRowsResult << j * ConstHelper.ProductNumber) | TProduct.ResultNum;
                            }
                            //写入当前结果 结果为2进制101010...10  ConstHelper.MaterialNum位
                            Common.Write(ConstHelper.RightResult, TProduct.ResultNum);
                            List<ImageDisplayViewModel> bitmaps = new List<ImageDisplayViewModel>();
                            for (int i = 0; i < CameraNumber; i++)
                            {
                                //缩放结果图便于展示
                                Mat mat = new Mat();
                                Cv2.Resize(TProduct.ResImages[pos - 1, i], mat, new OpenCvSharp.Size(0, 0), 0.25, 0.25);
                                ImageSource imageSource = null;
                                Application.Current.Dispatcher?.Invoke(new Action(() =>
                                {
                                    //可以重复执行某一次拍照
                                    imageSource = mat.ToBitmapSource();
                                    if (TProduct.DisplayImages.Count > 0 && TProduct.DisplayImages.Count - 1 >= (pos - 1) * CameraNumber + i)
                                        TProduct.DisplayImages[(pos - 1) * CameraNumber + i] = imageSource;
                                    else
                                        TProduct.DisplayImages.Add(imageSource);
                                }));
                                bitmaps.Add(new ImageDisplayViewModel() { DisImage = imageSource });
                                mat.Dispose();
                            }
                            PushDisResult?.Invoke(bitmaps, ConstHelper.Right, ConstHelper.IntToStringArray(TProduct.ResultNum, pos));
                            ProductPushOut.Instance.Enqueue(TProduct);
                            TProduct = null;
                        }
                        #endregion

                        temp = pos;
                        pos = 0;
                    }

                    #region 测试
                    //else
                    //{
                    //    List<ImageDisplayViewModel> bitmaps = new List<ImageDisplayViewModel>();
                    //    Application.Current.Dispatcher?.Invoke(() =>
                    //    {
                    //        for (int i = 0; i < 4; i++)
                    //        {
                    //            Mat mat = null;

                    //            if (pos == i && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + (i + 4).ToString() + ".jpg"))
                    //            {
                    //                FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + (i + 4).ToString() + ".jpg", FileMode.Open);
                    //                mat = Mat.FromStream(fs, ImreadModes.Color);
                    //                fs.Close();
                    //            }
                    //            else
                    //                mat = new Mat(400, 400, i > 1 ? MatType.CV_8UC1 : MatType.CV_16UC3);
                    //            bitmaps.Add(new ImageDisplayViewModel() { DisImage = mat.ToBitmapSource() });
                    //            mat.Dispose();
                    //        }
                    //    });
                    //    Random random = new Random();
                    //    PushDisResult?.Invoke(bitmaps, ConstHelper.Right, ConstHelper.IntToStringArray(random.Next(1000, 16777218), (pos + 1) * 2 * ConstHelper.ProductNumber));
                    //    pos++;
                    //    if (pos >= 4)
                    //    {
                    //        pos = 0;
                    //    }
                    //    Thread.Sleep(50);
                    //}
                    #endregion

                    Thread.Sleep(40);
                }
            });
        }
    }
}
