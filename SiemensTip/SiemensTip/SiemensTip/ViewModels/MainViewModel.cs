using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Extension;
using SiemensTip.Helper;
using SiemensTip.Properties;
using SiemensTip.src;
using SiemensTip.src.Model;
using SiemensTip.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SiemensTip.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields
        private ImageDisplayViewModel _disImage;
        private List<Task> Tasks;
        private List<IWork> works;
        private ObservableCollection<ImageDisplayViewModel> leftDisImages;
        private ObservableCollection<ImageDisplayViewModel> rightDisImages;
        private bool isMauaul;
        private ObservableCollection<string> leftResult;
        private ObservableCollection<string> rightResult;
        private DelegateCommand<object> closing;
        private DelegateCommand<object> previewMouseLeftButtonDown;
        private DelegateCommand<object> leftDoubleClick;
        private DelegateCommand openTemplateSetting;
        private DelegateCommand openSetting;
        private int leftTotal;
        private int rightTotal;
        private int ngTotal;
        private int okTatal;
        private bool isModifyTemplate;
        bool isClose = false;
        private string manaulResult;
        private bool isModifySetting;
        //Mat mat = null;
        #endregion

        #region Properties
        public ImageDisplayViewModel DisImage
        {
            get => _disImage;
            set => SetProperty(ref _disImage, value);
        }
        public ObservableCollection<ImageDisplayViewModel> LeftDisImages
        {
            get => leftDisImages ?? (leftDisImages = new ObservableCollection<ImageDisplayViewModel>());
            set => SetProperty(ref leftDisImages, value);
        }
        public ObservableCollection<ImageDisplayViewModel> RightDisImages
        {
            get => rightDisImages ?? (rightDisImages = new ObservableCollection<ImageDisplayViewModel>());
            set => SetProperty(ref rightDisImages, value);
        }
        public bool IsMauaul
        {
            get => isMauaul;
            set => SetProperty(ref isMauaul, value);
        }
        public ObservableCollection<string> LeftResult
        {
            get => leftResult ?? (leftResult = new ObservableCollection<string>());
            set => SetProperty(ref leftResult, value);
        }
        public ObservableCollection<string> RightResult
        {
            get => rightResult ?? (rightResult = new ObservableCollection<string>());
            set => SetProperty(ref rightResult, value);
        }
        public int LeftTotal
        {
            get => leftTotal;
            set => SetProperty(ref leftTotal, value);
        }
        public int RightTotal
        {
            get => rightTotal;
            set => SetProperty(ref rightTotal, value);
        }
        public int NgTotal
        {
            get => ngTotal;
            set => SetProperty(ref ngTotal, value);
        }
        public int OkTatal
        {
            get => okTatal;
            set => SetProperty(ref okTatal, value);
        }
        public string ManaulResult
        {
            get => manaulResult;
            set => SetProperty(ref manaulResult, value);
        }
        /// <summary>
        /// 是否允许修改模板
        /// </summary>
        public bool IsModifyTemplate
        {
            get => isModifyTemplate;
            set => SetProperty(ref isModifyTemplate, value);
        }
        /// <summary>
        /// 是否允许修改设置
        /// </summary>
        public bool IsModifySetting
        {
            get => isModifySetting;
            set => SetProperty(ref isModifySetting, value);
        }
        #endregion

        #region Initialize
        public MainViewModel()
        {
            IsModifyTemplate = Settings.Default.IsModifyTemplate;
            IsModifySetting = Settings.Default.IsModifySetting;
            Tasks = new List<Task>();
            //心跳
            CancellationTokenSource Token = new CancellationTokenSource();
            Common.Instance.TokenSources.Add(Token);
            Tasks.Add(Task.Run(() => { Communication(Token); }));
            //左工位
            works = new List<IWork>();
            Token = new CancellationTokenSource();
            Common.Instance.TokenSources.Add(Token);
            IWork _work = new LeftStation(ShowImage, Token, 4, 0);
            works.Add(_work);
            //右工位
            Token = new CancellationTokenSource();
            Common.Instance.TokenSources.Add(Token);
            _work = new RightStation(ShowImage, Token, 4, 4);
            works.Add(_work);
            //中间工位
            Token = new CancellationTokenSource();
            Common.Instance.TokenSources.Add(Token);
            _work = new MauaulTestStation(ShowImage, Token, 4, 4);
            works.Add(_work);
            works.ForEach(c =>
            {
                c.InspectTask();
            });
            IsMauaul = false;
            //mat = null;
            //FileStream fs = new FileStream(@"C:\Users\user\Desktop\0.bmp", FileMode.Open);
            //mat = Mat.FromStream(fs, ImreadModes.Color);
            //fs.Close();
            //DisImage = new ImageDisplayViewModel();
            //DisImage.DisImage = mat.ToBitmapSource();
            ////DisImage = new ImageDisplayViewModel();
            ////DisImage.DisImage = new BitmapImage(new Uri(@"C:\Users\user\Desktop\0.bmp"));
        }
        #endregion

        #region Methods
        private void Communication(CancellationTokenSource communicationToken)
        {
            bool isPushInfo = false;
            while (!communicationToken.IsCancellationRequested)
            {
                if (Common.PlcContent)
                {
                    isPushInfo = false;
                    Common.Write(ConstHelper.HeartBeat, 1);
                    Thread.Sleep(300);
                    Common.Write(ConstHelper.HeartBeat, 0);
                    Thread.Sleep(300);
                    LeftTotal = Common.GetValue<int>(nameof(LeftTotal));
                    RightTotal = Common.GetValue<int>(nameof(RightTotal));
                    NgTotal = Common.GetValue<int>(nameof(NgTotal));
                    OkTatal = Common.GetValue<int>(nameof(OkTatal));
                }
                else
                {
                    if (!isPushInfo)
                        Log.ErrorLog("PLC未连线,请检查连接状态");
                    isPushInfo = true;
                    Thread.Sleep(1000);
                }
                Thread.Sleep(50);
                IsMauaul = !Common.IsStart;
            }
        }
        private void ShowImage(List<ImageDisplayViewModel> imageSources, string target, List<string> result)
        {
            if (imageSources == null || imageSources.Count == 0)
                return;
            Application.Current.Dispatcher?.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
            {
                switch (target)
                {
                    case ConstHelper.Left:
                        LeftDisImages.Clear();
                        imageSources.ForEach(imageSource => LeftDisImages.Add(imageSource));
                        LeftResult.Clear();
                        LeftResult.AddRange(result);
                        break;
                    case ConstHelper.Right:
                        RightDisImages.Clear();
                        imageSources.ForEach(imageSource => RightDisImages.Add(imageSource));
                        RightResult.Clear();
                        RightResult.AddRange(result);
                        break;
                    case ConstHelper.Muaual:
                        if (RightResult?.Count > 0 || LeftResult?.Count > 0)
                        {
                            RightResult?.Clear();
                            LeftResult?.Clear();
                        }
                        DisImage = imageSources.FirstOrDefault();
                        ManaulResult = result != null && result.Count > 0 ? result[0] : null;
                        break;
                }
            }));
        }
        #endregion

        #region Command
        public DelegateCommand<object> Closing
        {
            get => closing ?? (closing = new DelegateCommand<object>(c =>
            {
                if (isClose)
                    return;
                Common.Instance.Stop();
                ProductPushOut.Instance.Stop();
                Application.Current.Shutdown();
            }));
        }
        public DelegateCommand<object> PreviewMouseLeftButtonDown
        {
            get => previewMouseLeftButtonDown ?? (previewMouseLeftButtonDown = new DelegateCommand<object>(c =>
            {
                if (c is System.Windows.Window)
                    (c as System.Windows.Window).DragMove();
            }));
        }
        public DelegateCommand<object> LeftDoubleClick
        {
            get => leftDoubleClick ?? (leftDoubleClick = new DelegateCommand<object>(c =>
            {
                if (c is System.Windows.Window)
                {
                    var window = (System.Windows.Window)c;
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Maximized;
                    else if (window.WindowState == WindowState.Maximized)
                        window.WindowState = WindowState.Normal;
                    else if (window.WindowState == WindowState.Normal)
                        window.WindowState = WindowState.Maximized;
                }
            }));
        }
        public DelegateCommand OpenTemplateSetting
        {
            get => openTemplateSetting ?? (openTemplateSetting = new DelegateCommand(() =>
            {
                new ImageTemplate().Show();
            }));
        }
        public DelegateCommand OpenSetting
        {
            get => openSetting ?? (openSetting = new DelegateCommand(() =>
            {
                new SettingView().ShowDialog();
            }));
        }
        #endregion

    }
}
