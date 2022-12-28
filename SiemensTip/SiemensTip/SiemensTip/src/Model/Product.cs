using OpenCvSharp;
using Prism.Mvvm;
using SiemensTip.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SiemensTip.src.Model
{
    public class Product : BindableBase
    {
        private string _id;
        private bool _result = true;
        private short[,] _results;
        private int _resultNum;
        private Mat[,] _images;
        private Mat[,] _resImages;
        private string workName;
        private bool[,] inspectResult;

        /// <summary>
        /// 算法是否完成结果
        /// </summary>
        public bool[,] InspectResult { get => inspectResult; set => SetProperty(ref inspectResult, value); }
        public string WorkName
        {
            get => workName;
            set => SetProperty(ref workName, value);
        }
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value, nameof(Id));
        }
        public bool Result
        {
            get => _result;
            set => SetProperty(ref _result, value, nameof(Result));
        }
        /// <summary>
        /// 产品二维数组结果
        /// </summary>
        public short[,] Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }
        /// <summary>
        /// 24位产品结果
        /// </summary>
        public int ResultNum
        {
            get => _resultNum;
            set => SetProperty(ref _resultNum, value);
        }
        public Mat[,] Images
        {
            get => _images;
            set => SetProperty(ref _images, value, nameof(Images));
        }
        public Mat[,] ResImages
        {
            get => _resImages;
            set => SetProperty(ref _resImages, value, nameof(ResImages));
        }

        private ObservableCollection<ImageSource> _displayImages;
        public ObservableCollection<ImageSource> DisplayImages
        {
            get => _displayImages;
            set => SetProperty(ref _displayImages, value, nameof(DisplayImages));
        }

        public Product(int count, string workstation)
        {
            Id = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
            Images = new Mat[ConstHelper.MaterialNum / 2 / ConstHelper.ProductNumber, count];
            ResImages = new Mat[ConstHelper.MaterialNum / 2 / ConstHelper.ProductNumber, count];
            DisplayImages = new ObservableCollection<ImageSource>();
            Results = new short[ConstHelper.MaterialNum / 2 / ConstHelper.ProductNumber, count];
            InspectResult = new bool[ConstHelper.MaterialNum / 2 / ConstHelper.ProductNumber, count];
            workName = workstation;
            ResultNum = 0;
        }
        /// <summary>
        /// 图片写入
        /// </summary>
        /// <param name="isOnlyWriteNG">是否只保存NG</param>
        /// <param name="saveRes">是否写入结果图</param>
        /// <param name="compress">是否保存原图</param>
        public void PushOut(bool isOnlyWriteNG, bool saveRes = false, bool compress = false)
        {
            //ok jpeg ng bmp
            Result = true;
            char[] resultStrs = Convert.ToString(ResultNum, 2).PadLeft(ConstHelper.MaterialNum, '0').ToCharArray();
            for (int i = 0; i < ConstHelper.MaterialNum; i++)
                if (resultStrs[i] != '1')
                    Result = false;
            var res = Result ? "OK" : "NG";
            if (Result && isOnlyWriteNG)
                return;
            var path = ConstHelper.ImageSavePath + $"\\{WorkName}_{res}\\{DateTime.Now.ToString("yyyyMMdd")}\\{Id}";
            Directory.CreateDirectory(path);
            if (Images == null)
                return;
            int rows = Images.GetLength(0);
            int cols = Images.GetLength(1);
            //原图写入
            for (int k = 0; k < rows; k++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Mat image = Images[k, j];
                    if (image != null && !image.IsDisposed)
                    {
                        var spath = path + $"\\{(k + 1)}-{(j + 1)}";
                        if (!compress)
                        {
                            ImageUtils.JpegComress(image, out var compressed);
                            var sspath = spath + ConstHelper.JpegExtension;
                            using (FileStream fs = new FileStream(sspath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                            {
                                fs.Write(compressed, 0, compressed.Length);
                            }
                        }
                        else
                            image.ImWrite(spath + ConstHelper.ImageExtension);
                    }
                }
            }
            //结果图片写入
            if (saveRes)
            {
                rows = ResImages.GetLength(0);
                cols = ResImages.GetLength(1);

                for (int k = 0; k < rows; k++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Mat image = Images[k, j];
                        if (image != null && !image.IsDisposed)
                        {
                            var spath = path + $"\\Res_{(k + 1)}-{(j + 1)}";
                            if (!compress)
                            {
                                ImageUtils.JpegComress(image, out var compressed);
                                var sspath = spath + ConstHelper.JpegExtension;
                                using (FileStream fs = new FileStream(sspath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                {
                                    fs.Write(compressed, 0, compressed.Length);
                                }
                            }
                            else
                                image.ImWrite(spath + ConstHelper.ImageExtension);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var image in Images)
            {
                if (image != null && !image.IsDisposed)
                    image.Dispose();
            }
            Images = null;
            foreach (var image in ResImages)
            {
                if (image != null && !image.IsDisposed)
                    image.Dispose();
            }
            ResImages = null;
            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    DisplayImages.Clear();
            //});
            DisplayImages = null;
        }
    }
}
