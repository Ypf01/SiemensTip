using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Extension;
using SiemensTip.Helper;
using SiemensTip.Properties;
using SiemensTip.src;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SiemensTip.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        #region Fields
        private bool isWriteRes;
        private bool isWriteQuality;
        private bool isSaveImage;
        private bool isWriteNG;
        private float botRadiusMin;
        private float botRadiusMax;
        private float lateral_thMin;
        private float lateral_thMax;
        private float centrifugeAngleMin;
        private float centrifugeAngleMax;
        private DelegateCommand<object> saveSetting;
        #endregion

        #region Initialize
        public SettingViewModel()
        {
            Settings.Default.Reload();
            Lateral_thMax = Settings.Default.Lateral_thMax;
            Lateral_thMin = Settings.Default.Lateral_thMin;
            BotRadiusMin = Settings.Default.BotRadiusMin;
            BotRadiusMax = Settings.Default.BotRadiusMax;
            CentrifugeAngleMin = Settings.Default.CentrifugeAngleMin;
            CentrifugeAngleMax = Settings.Default.CentrifugeAngleMax;
            IsWriteNG = Settings.Default.IsWriteNG;
            IsSaveImage = Settings.Default.IsSaveImage;
            IsWriteQuality = Settings.Default.IsWriteQuality;
            IsWriteRes = Settings.Default.IsWriteRes;
        }
        #endregion

        #region Properties
        public float Lateral_thMax
        {
            get => lateral_thMax;
            set => SetProperty(ref lateral_thMax, value);
        }
        public float Lateral_thMin
        {
            get => lateral_thMin;
            set => SetProperty(ref lateral_thMin, value);
        }
        public float BotRadiusMin
        {
            get => botRadiusMin;
            set => SetProperty(ref botRadiusMin, value);
        }
        public float BotRadiusMax
        {
            get => botRadiusMax;
            set => SetProperty(ref botRadiusMax, value);
        }
        public float CentrifugeAngleMin
        {
            get => centrifugeAngleMin;
            set => SetProperty(ref centrifugeAngleMin, value);
        }
        public float CentrifugeAngleMax
        {
            get => centrifugeAngleMax;
            set => SetProperty(ref centrifugeAngleMax, value);
        }
        public bool IsWriteRes
        {
            get => isWriteRes;
            set => SetProperty(ref isWriteRes, value);
        }
        public bool IsWriteQuality
        {
            get => isWriteQuality;
            set => SetProperty(ref isWriteQuality, value);
        }
        public bool IsSaveImage
        {
            get => isSaveImage;
            set
            {
                if (!value)
                {
                    IsWriteRes = false;
                    IsWriteQuality = false;
                    IsWriteNG = false;
                }
                SetProperty(ref isSaveImage, value);
            }
        }
        public bool IsWriteNG
        {
            get => isWriteNG;
            set => SetProperty(ref isWriteNG, value);
        }
        #endregion

        #region Command
        public DelegateCommand<object> SaveSetting
        {
            get => saveSetting ?? (saveSetting = new DelegateCommand<object>(c =>
            {
                Settings.Default.Lateral_thMax = Lateral_thMax;
                Settings.Default.Lateral_thMin = Lateral_thMin;
                Settings.Default.BotRadiusMin = BotRadiusMin;
                Settings.Default.BotRadiusMax = BotRadiusMax;
                Settings.Default.CentrifugeAngleMin = CentrifugeAngleMin;
                Settings.Default.CentrifugeAngleMax = CentrifugeAngleMax;
                Settings.Default.IsWriteNG = IsWriteNG;
                Settings.Default.IsSaveImage = IsSaveImage;
                Settings.Default.IsWriteQuality = IsWriteQuality;
                Settings.Default.IsWriteRes = IsWriteRes;
                Settings.Default.Save();
                ProductPushOut.Instance.IsWriteRes = IsWriteRes;
                ProductPushOut.Instance.IsSaveImage = IsSaveImage;
                ProductPushOut.Instance.IsWriteQuality = IsWriteQuality;
                ProductPushOut.Instance.IsWriteNG = IsWriteNG;
                Common.BotRadiusMin = BotRadiusMin;
                Common.BotRadiusMax = BotRadiusMax;
                Common.Lateral_thMax = Lateral_thMax;
                Common.Lateral_thMin = Lateral_thMin;
                Common.CentrifugeAngleMin = CentrifugeAngleMin;
                Common.CentrifugeAngleMax = CentrifugeAngleMax;
                Settings.Default.Reload();
                Common.Sol.solution_params.lateral_th = new float[2] { Lateral_thMin, Lateral_thMax };   // lateral threshold / 批锋阈值
                Common.Sol.solution_params.radius_th = new float[2] { BotRadiusMin, BotRadiusMax }; // radius threshold / 圆半径阈值
                Common.Sol.solution_params.axial_distance_th = new float[2] { CentrifugeAngleMin, CentrifugeAngleMax };    // axial distance threshold / 离心度阈值
                File.WriteAllText(Environment.CurrentDirectory + "\\Model\\" + ConstHelper.Modelparams, JsonConvert.SerializeObject(Common.Sol.solution_params));
                if (c is Window)
                    (c as Window).DialogResult = true;
            }));
        }
        #endregion
    }
}
