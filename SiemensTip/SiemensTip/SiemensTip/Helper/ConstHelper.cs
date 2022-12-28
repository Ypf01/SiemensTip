using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTip.Helper
{
    public class ConstHelper
    {
        /// <summary>
        /// 侧面名称
        /// </summary>
        public const string Lateral_template = "lateral_template";
        /// <summary>
        /// 底面名称
        /// </summary>
        public const string Bottom_template = "bottom_template";
        /// <summary>
        /// 模型后缀名
        /// </summary>
        public const string ModelFormat = ".hmodel";
        /// <summary>
        /// 侧面模型参数
        /// </summary>
        public const string Modelparams = "tip_params.yml";
        /// <summary>
        /// 左相机触发
        /// </summary>
        public const string LeftCamera = "LeftCamera";
        /// <summary>
        /// 左相机拍照完成
        /// </summary>
        public const string LeftCameraFinish = "LeftCameraFinish";
        /// <summary>
        /// 右相机触发
        /// </summary>
        public const string RightCamera = "RightCamera";
        /// <summary>
        /// 右相机拍照完成
        /// </summary>
        public const string RightCameraFinish = "RightCameraFinish";
        /// <summary>
        /// 左边仓库当前总数量
        /// </summary>
        public const string LeftTotal = "LeftTotal";
        /// <summary>
        /// 右边仓库当前总数量
        /// </summary>
        public const string RightTotal = "RightTotal";
        /// <summary>
        /// 当前NG总数量
        /// </summary>
        public const string NgTotal = "NgTotal";
        /// <summary>
        /// 当前OK总数量
        /// </summary>
        public const string OkTatal = "OkTatal";
        /// <summary>
        /// 相机手动测试1-8对应需要取照片的相机
        /// </summary>
        public const string ExternalTrigger = "ExternalTrigger";
        /// <summary>
        /// 左侧结果
        /// </summary>
        public const string LeftResult = "LeftResult";
        /// <summary>
        /// 右侧结果
        /// </summary>
        public const string RightResult = "RightResult";
        /// <summary>
        /// 手自动状态/0手动/1自动
        /// </summary>
        public const string Status = "Status";
        /// <summary>
        /// 下位机心跳
        /// </summary>
        public const string HeartBeat = "HeartBeat";
        /// <summary>
        /// 照片存放地址
        /// </summary>
        public static string ImageSavePath = "E:\\Images";
        /// <summary>
        /// Bmp格式
        /// </summary>
        public const string ImageExtension = ".bmp";
        /// <summary>
        /// Jpeg格式
        /// </summary>
        public const string JpegExtension = ".jpeg";
        /// <summary>
        /// Png格式
        /// </summary>
        public const string PngExtension = ".png";

        public const int JpegQuality = 85;
        /// <summary>
        /// 相机名称数组
        /// </summary>
        public static string[] CameraIDs = new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };
        /// <summary>
        /// 相机视野内产品个数
        /// </summary>
        public const short ProductNumber = 4;
        /// <summary>
        /// 左工位名称
        /// </summary>
        public const string Left = "Left";
        /// <summary>
        /// 右工位名称
        /// </summary>
        public const string Right = "Right";
        /// <summary>
        /// 手动工位
        /// </summary>
        public const string Muaual = "Muaual";
        /// <summary>
        /// 手动测试工位
        /// </summary>
        public const string HandStation = "HandStation";
        /// <summary>
        /// 单边产品个数
        /// </summary>
        public const int MaterialNum = 24;
        /// <summary>
        /// 拍照次数
        /// </summary>
        public const int PhotoNum = MaterialNum / 2 / ProductNumber;
        public const string OK = "OK";
        public const string NG = "NG";
        public const string Null = "Null";
        public const double ZoomOutScaleValue = 1.1;
        public const double ZoomInScaleValue = 1 / ZoomOutScaleValue;

        public static List<string> IntToStringArray(int targat, int num)
        {
            if (num > PhotoNum)
                num = PhotoNum;
            string str = System.Convert.ToString(targat, 2).PadLeft(MaterialNum, '0');
            char[] chs = str.ToCharArray().Reverse().ToArray();
            List<string> list = new List<string>();
            for (int i = 0; i < chs.Length; i++)
            {
                if (i < ProductNumber * num || (i >= MaterialNum / 2 && i < MaterialNum / 2 + num * ProductNumber))
                    list.Add(chs[i] == '1' ? OK : NG);
                else
                    list.Add(Null);
            }
            return list;
        }
        public static List<string> GetResult()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < MaterialNum; i++)
                result.Add(Null);
            return result;
        }
    }
}
