using BMS_tip_wrapper;
using Newtonsoft.Json;
using NodeSettings;
using NodeSettings.Node.Device;
using OpenCvSharp;
using Pframe;
using SiemensTip.Helper;
using SiemensTip.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static BMS_tip_wrapper.Solution;

namespace SiemensTip.Extension
{
    public class Common
    {
        #region Single
        private static Common _instance;
        public static Common Instance
        {
            get
            {
                lock (SingleLock)
                {
                    return _instance ?? (_instance = new Common());
                }
            }
        }
        public Common()
        {
            TokenSources = new List<CancellationTokenSource>();
            BotRadiusMin = Settings.Default.BotRadiusMin;
            BotRadiusMax = Settings.Default.BotRadiusMax;
            CentrifugeAngleMin = Settings.Default.CentrifugeAngleMin;
            CentrifugeAngleMax = Settings.Default.CentrifugeAngleMax;
            Lateral_thMin = Settings.Default.Lateral_thMin;
            Lateral_thMax = Settings.Default.Lateral_thMax;
            SetImage = null;
        }
        #endregion

        #region LOCKER
        private readonly static object SingleLock = new object();
        private readonly static object WriteLock = new object();
        private readonly static object ReadLock = new object();
        #endregion

        #region Properties
        public static Action SetImage { get; set; }
        private static Mat manaulMat;
        public static Mat ManaulMat
        {
            get => manaulMat;
            set
            {
                if (manaulMat != null)
                    manaulMat.Dispose();
                if (value != null)
                {
                    manaulMat = value;
                    SetImage?.Invoke();
                }
            }
        }
        public static bool IsRunSts { get; set; } = false;
        public static Solution Sol = new Solution();
        public static float BotRadiusMin { get; set; } = 46.5f;
        public static float BotRadiusMax { get; set; } = 48f;
        public static float Lateral_thMin { get; set; } = 0;
        public static float Lateral_thMax { get; set; } = 3.5f;
        public static float CentrifugeAngleMin { get; set; } = 0;
        public static bool TemplateSts { get; set; } = false;
        public static float CentrifugeAngleMax { get; set; } = 5.0f;
        private NodeInovance _inovance;
        private static HikCamera[] _hikCameras;
        public static List<OpenCvSharp.Point> LeftPixelOffset = new List<OpenCvSharp.Point>();
        public static List<OpenCvSharp.Point> RightPixelOffset = new List<OpenCvSharp.Point>();
        public static bool IsStart
        {
            get => GetValue(ConstHelper.Status).ToString() == "1";
        }
        public List<CancellationTokenSource> TokenSources { get; set; }
        public NodeInovance Inovance
        {
            get { return _inovance; }
            private set { _inovance = value; }
        }
        public static HikCamera[] HikCameras
        {
            get { return _hikCameras; }
            set { _hikCameras = value; }
        }
        public static bool PlcContent
        {
            get
            {
                if (Instance.Inovance == null || !Instance.Inovance.IsConnected)
                    return false;
                return true;
            }
        }
        #endregion

        #region Methods
        public bool ReadPixelOffset()
        {
            string tempStr = Settings.Default.LeftPixelOffset;
            Log.AppLog("LeftPixelOffset:" + tempStr);
            if (string.IsNullOrEmpty(tempStr) || tempStr?.Split(',').Count() < 24)
                return false;
            LeftPixelOffset = GetSpiltVal(tempStr);
            tempStr = Settings.Default.RightPixelOffset;
            Log.AppLog("RightPixelOffset:" + tempStr);
            if (string.IsNullOrEmpty(tempStr) || tempStr?.Split(',').Count() < 24)
                return false;
            RightPixelOffset = GetSpiltVal(tempStr);
            if (LeftPixelOffset == null || RightPixelOffset == null)
                return false;
            return true;
        }

        private List<OpenCvSharp.Point> GetSpiltVal(string str)
        {
            string[] pixeStation = str?.Split(',');
            List<OpenCvSharp.Point> spiltVal = new List<OpenCvSharp.Point>();
            string[] ptxy = null;
            foreach (var item in pixeStation)
            {
                if (item.Contains('|') && item.Split('|').Count() >= 2)
                {
                    ptxy = item.Split('|');
                    if (int.TryParse(ptxy[0], out int val1) && int.TryParse(ptxy[0], out int val2))
                    {
                        var pt = new OpenCvSharp.Point() { X = val1, Y = val2 };
                        spiltVal.Add(pt);
                    }
                    else
                        return null;
                }
            }
            return spiltVal;
        }

        #region 连接相机与PLC
        /// <summary>
        /// 连接相机与PLC
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {

            if (!TemplateSts)
                return false;

            #region PLC初始化
            if (Inovance == null)
            {
                string plcConfig = Environment.CurrentDirectory + "\\Resource\\Sim.xml";
                if (!File.Exists(plcConfig))
                {
                    Log.ErrorLog($"PLC配置文件{ Path.GetFileName(plcConfig)}未找到", true);
                    return false;
                }
                List<NodeInovance> melsec;
                try
                {
                    melsec = InovanceCFG.LoadXmlFile(plcConfig);
                    Log.AppLog($"{ Path.GetFileName(plcConfig)} PLC配置文件加载成功");
                }
                catch (Exception ex)
                {
                    Log.ErrorLog(ex.Message, true);
                    return false;
                }
                if (melsec.Count == 0)
                {
                    Log.ErrorLog("配置文件加载错误,未寻找到下位机", true);
                    return false;
                }
                Inovance = melsec[0];
                Inovance.AlarmEvent += Inovance_AlarmEvent;
                //PLC开始扫描
                Log.AppLog($"连接PLC {Inovance.Name},开启线程扫描!");
                Inovance.Start();
            }
            #endregion

            #region 相机初始化
            HikCameras = new HikCamera[ConstHelper.CameraIDs.Length];
            bool flag = true;
            List<int> _list = new List<int>();
            for (int i = 0; i < ConstHelper.CameraIDs.Length; i++)
                _list.Add(i);
            List<int> camContent = new List<int>();
            Parallel.ForEach(_list, cam =>
            {
                var ff = true;
                HikCameras[cam] = new HikCamera();
                ff &= HikCameras[cam].ConnectCamera(ConstHelper.CameraIDs[cam]);
                ff &= HikCameras[cam].SetImageCallback();
                ff &= HikCameras[cam].LoadUserSet(1);
                ff &= HikCameras[cam].SetTriggerMode(1);
                ff &= HikCameras[cam].StartCamera();
                if (!ff)
                {
                    HikCameras[cam] = null;
                    Log.ErrorLog($"相机{ConstHelper.CameraIDs[cam]}加载失败!");
                    camContent.Add(cam + 1);
                }
                else
                    Log.AppLog($"相机{ConstHelper.CameraIDs[cam]}加载成功!");
                flag &= ff;
            });
            if (camContent.Count > 0)
            {
                var loadsts = camContent.OrderBy(c => c);
                Log.ErrorLog(string.Join("、", loadsts.ToArray()) + " 相机异常");
            }
            if (!flag)
            {
                Log.ErrorLog($"相机加载失败,请检查外围设备连接是否正常!", true);
                return false;
            }
            #endregion

            #region 返回加载结果
            return true;
            #endregion
        }
        #endregion

        #region 模板
        public bool GetTemplate()
        {
            #region 模型加载
            bool res = true;
            string path = Environment.CurrentDirectory + "\\Model\\";
            if (!File.Exists(path + ConstHelper.Modelparams))
            {
                Log.ErrorLog(ConstHelper.Modelparams + "文件不存在");
                return false;
            }

            try
            {
                Sol.solution_params = JsonConvert.DeserializeObject<Params>(File.ReadAllText(path + ConstHelper.Modelparams));
            }
            catch (Exception ex)
            {
                Log.ErrorLog(ex.Message);
                return false;
            }

            #region 视觉参数配置初始化
            bool isHaveParms = false;
            if (Sol.solution_params == null)
                isHaveParms = true;
            else
            {
                if (Sol.solution_params.lateral_th == null || Sol.solution_params.lateral_th.Count() != 2 || Sol.solution_params.radius_th == null || Sol.solution_params.radius_th.Count() != 2 || Sol.solution_params.axial_distance_th == null || Sol.solution_params.axial_distance_th.Count() != 2 || Sol.solution_params.metro_line == null || Sol.solution_params.metro_line.Count() != 2)
                    isHaveParms = true;
                else
                {
                    Lateral_thMin = Sol.solution_params.lateral_th[0];
                    Lateral_thMax = Sol.solution_params.lateral_th[1];
                    BotRadiusMin = Sol.solution_params.radius_th[0];
                    BotRadiusMax = Sol.solution_params.radius_th[1];
                    CentrifugeAngleMin = Sol.solution_params.axial_distance_th[0];
                    CentrifugeAngleMax = Sol.solution_params.axial_distance_th[0];
                }
            }
            #endregion

            #region lateral面加载
            Log.AppLog("lateral面模型文件匹配....");
            if (!isHaveParms && File.Exists(path + ConstHelper.Lateral_template + ConstHelper.ModelFormat))
            {
                Log.AppLog("加载lateral面模型文件....");
                // Load lateral model
                Sol.load_template(path + ConstHelper.Lateral_template + ConstHelper.ModelFormat, "top");
                Log.AppLog("lateral面模型文件加载完成");
            }
            else
            {
                //if (Sol.solution_params.metro_line == null || Sol.solution_params.metro_line.Count() < 2 || Sol.solution_params.metro_line[0] == null || Sol.solution_params.metro_line[1] == null)
                //{
                //    Log.ErrorLog("Lateral模型线数据不正确", true);
                //    res = false;
                //}
                //else
                //    res = !LateralTemplate(null, true);
                res = false;
            }
            #endregion

            #region bottom加载
            Log.AppLog("bottom面模型文件匹配....");
            if (!isHaveParms && File.Exists(path + ConstHelper.Bottom_template + ConstHelper.ModelFormat))
            {
                Log.AppLog("加载bottom面模型文件....");
                Sol.load_template(path + ConstHelper.Bottom_template + ConstHelper.ModelFormat, "circle");
                Log.AppLog("bottom面模型文件加载完成");
            }
            else
                res &= BottomTemplate();
            #endregion

            #endregion

            return res;
        }

        public bool LateralTemplate(List<OpenCvSharp.Point> pts, bool istheCall = false)
        {
            try
            {
                if ((pts == null || pts.Count < 2) && !istheCall)
                    return false;
                string path = Environment.CurrentDirectory + "\\Model\\";
                // Setting thresholds / 设置阈值
                Sol.solution_params.lateral_th = new float[2] { Lateral_thMin, Lateral_thMax };   // lateral threshold / 批锋阈值
                if (!istheCall)
                    Sol.solution_params.metro_line = new KeyPoint[2] { new KeyPoint(pts[0].X, pts[0].Y, 1), new KeyPoint(pts[1].X, pts[1].Y, 1) };
                else
                    Log.AppLog("未找到lateral面模型文件,生成lateral模型中,这可能需要一些时间");
                // Create lateral template
                if (!File.Exists(path + ConstHelper.Lateral_template + ConstHelper.PngExtension))
                {
                    Log.ErrorLog("lateral面模板源文件lateral_template不存在", true);
                    return false;
                }
                Mat img = Cv2.ImRead(path + ConstHelper.Lateral_template + ConstHelper.PngExtension);
                Sol.create_lateral_template(img, path + ConstHelper.Lateral_template + ConstHelper.ModelFormat);
                if (!istheCall)
                    Sol.get_top_line(img, new OpenCvSharp.Point[2] {
                    new OpenCvSharp.Point(pts[0].X, pts[0].Y),
                    new OpenCvSharp.Point(pts[1].X, pts[1].Y) });
                File.WriteAllText(path + ConstHelper.Modelparams, JsonConvert.SerializeObject(Sol.solution_params));
                img.Dispose();
                Log.AppLog("lateral面模型文件加载完成");
            }
            catch (Exception ex)
            {
                TemplateSts = false;
                Log.ErrorLog("Lateral模型生成：" + ex.Message, true);
                return false;
            }
            TemplateSts = false;
            return true;
        }

        public bool BottomTemplate(bool istheCall = false)
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\Model\\";
                Sol.solution_params.radius_th = new float[2] { BotRadiusMin, BotRadiusMax }; // radius threshold / 圆半径阈值
                Sol.solution_params.axial_distance_th = new float[2] { CentrifugeAngleMin, CentrifugeAngleMax };    // axial distance threshold / 离心度阈值
                if (istheCall)
                    Log.AppLog("未找到bottom面模型文件,生成bottom模型中,这可能需要一些时间");
                // Create lateral template
                if (!File.Exists(path + ConstHelper.Bottom_template + ConstHelper.PngExtension))
                {
                    Log.ErrorLog("bottom面模板源文件bottom_template不存在", true);
                    return false;
                }
                Mat img = Cv2.ImRead(path + ConstHelper.Bottom_template + ConstHelper.PngExtension);
                Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
                Sol.create_bottom_template(img, path + ConstHelper.Bottom_template + ConstHelper.ModelFormat);
                img.Dispose();
                TemplateSts = false;
                return true;
            }
            catch (Exception ex)
            {
                TemplateSts = false;
                Log.ErrorLog("Bottom模型生成：" + ex.Message, true);
                return false;
            }
        }
        #endregion

        #region 停止扫描、取消线程任务、关闭相机
        /// <summary>
        /// 停止扫描、取消线程任务、关闭相机
        /// </summary>
        public void Stop()
        {
            if (Inovance != null)
            {
                Inovance.Stop();
                Inovance.inovance?.DisConnect();
            }
            //取消所有线程任务
            foreach (var item in TokenSources)
                item.Cancel();
            new Action(() =>
            {
                if (HikCameras != null)
                    for (int i = 0; i < HikCameras.Length; i++)
                        if (HikCameras[i] != null)
                            HikCameras[i].CloseCamera();
            }).BeginInvoke(null, null);
        }
        #endregion

        #region PLC错误信息
        private void Inovance_AlarmEvent(object arg1, AlarmEventArgs arg2)
        {
            Log.ErrorLog(arg2.alarmInfo);
        }
        #endregion

        #region 写入PLC数据

        /// <summary>
        /// 写入PLC数据
        /// </summary>
        /// <param name="keyName">需要写入的键名</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        public static bool Write(string keyName, object value)
        {
            lock (WriteLock)
            {
                if (Instance.Inovance == null || !Instance.Inovance.IsConnected)
                    return false;
                CalResult result = Instance.Inovance?.Write(keyName, value.ToString());
                if (!result.IsSuccess)
                {
                    Log.ErrorLog(result.Message);
                    return false;
                }
                else
                    return result.IsSuccess;
            }
        }
        #endregion

        #region 获得采集数据
        /// <summary>
        /// 获得指定键名值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="keyName">键名</param>
        /// <returns></returns>
        public static T GetValue<T>(string keyName)
        {
            lock (ReadLock)
            {
                if (Instance.Inovance == null || !Instance.Inovance.IsConnected || !Instance.Inovance.CurrentValue.ContainsKey(keyName))
                    return default;
                return (T)Convert.ChangeType(Instance.Inovance.CurrentValue[keyName], typeof(T));
            }
        }
        /// <summary>
        /// 获得指定键名值
        /// </summary>
        /// <param name="keyName">键名</param>
        /// <returns></returns>
        public static string GetValue(string keyName)
        {
            lock (ReadLock)
            {
                if (Instance.Inovance == null || !Instance.Inovance.IsConnected || !Instance.Inovance.CurrentValue.ContainsKey(keyName))
                    return "";
                return Instance.Inovance.CurrentValue[keyName].ToString();
            }
        }
        #endregion

        #endregion

        #region 截取字符串
        public static List<OpenCvSharp.Point> CutArray(string targat, int startIndex)
        {
            List<OpenCvSharp.Point> list = null;
            switch (targat)
            {
                case ConstHelper.Left:
                    list = LeftPixelOffset;
                    break;
                case ConstHelper.Right:
                    list = RightPixelOffset;
                    break;
            }
            if (list == null)
                return null;
            if (startIndex + ConstHelper.ProductNumber > list.Count) return null;
            List<OpenCvSharp.Point> returnByte = new List<OpenCvSharp.Point>();
            for (int i = 0; i < ConstHelper.ProductNumber; i++)
                returnByte.Add(list[i + startIndex]);
            return returnByte;
        }
        #endregion
    }
}
