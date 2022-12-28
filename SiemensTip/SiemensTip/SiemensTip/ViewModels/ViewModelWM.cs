using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SiemensTip.ViewModels
{
    public class ViewModelWM : BindableBase
    {
        #region Discard
        //private ObservableCollection<ImageSource> rightDisImages;
        //private DelegateCommand<object> mouseLeftButtonDown;
        //private DelegateCommand<object> mouseLeftButtonUp;
        //private DelegateCommand<object> mouseMove;
        //bool _isMoving = false;
        //FrameworkElement _moveObj = null;
        //System.Windows.Point _downPoint = new System.Windows.Point();
        //public ObservableCollection<ImageSource> RightDisImages
        //{
        //    get => rightDisImages;
        //    set => SetProperty(ref rightDisImages, value);
        //}
        //public ViewModelWM()
        //{
        //    RightDisImages = new ObservableCollection<ImageSource>();
        //    for (int i = 0; i < 2; i++)
        //    {
        //        Mat mat = new Mat(new OpenCvSharp.Size(200, 200), MatType.CV_8UC1);
        //        RightDisImages.Add(mat.ToBitmapSource());
        //    }
        //}

        //private DelegateCommand<object> mouseRightButtonUp;

        //public DelegateCommand<object> MouseRightButtonUp
        //{
        //    get => mouseRightButtonUp ?? (mouseRightButtonUp = new DelegateCommand<object>(c =>
        //    {
        //        Image image = (c as MouseButtonEventArgs).Source as Image;
        //        image.RenderTransform = image.LayoutTransform;
        //    }));
        //}

        //private DelegateCommand<object> mouseWheel;

        //public DelegateCommand<object> MouseWheel
        //{
        //    get => mouseWheel ?? (mouseWheel = new DelegateCommand<object>(c =>
        //    {
        //        MouseWheelEventArgs e = c as MouseWheelEventArgs;
        //        Image image = e.Source as Image;
        //        if (image != null)
        //        {
        //            var control = image.Parent as FrameworkElement;

        //            System.Windows.Point p = e.GetPosition(image);
        //            Matrix m = image.RenderTransform.Value;
        //            if (e.Delta > 0)
        //                m.ScaleAtPrepend(ConstHelper.ZoomOutScaleValue, ConstHelper.ZoomOutScaleValue, p.X, p.Y);
        //            else
        //                m.ScaleAtPrepend(ConstHelper.ZoomInScaleValue, ConstHelper.ZoomInScaleValue, p.X, p.Y);

        //            image.RenderTransform = new MatrixTransform(m);
        //        }
        //    }));
        //}

        //public DelegateCommand<object> MouseLeftButtonDown
        //{
        //    get => mouseLeftButtonDown ?? (mouseLeftButtonDown = new DelegateCommand<object>(obj =>
        //    {
        //        _moveObj = (obj as MouseButtonEventArgs).Source as FrameworkElement;
        //        _downPoint = Mouse.GetPosition(_moveObj);
        //        _moveObj.CaptureMouse();
        //        _isMoving = true;
        //    }));
        //}
        //public DelegateCommand<object> MouseLeftButtonUp
        //{
        //    get => mouseLeftButtonUp ?? (mouseLeftButtonUp = new DelegateCommand<object>(obj =>
        //    {
        //        _isMoving = false;
        //        _moveObj.ReleaseMouseCapture();
        //        _moveObj = null;
        //    }));
        //}
        //public DelegateCommand<object> MouseMove
        //{
        //    get => mouseMove ?? (mouseMove = new DelegateCommand<object>(obj =>
        //    {
        //        if (_isMoving && _moveObj != null)
        //        {
        //            System.Windows.Point point = Mouse.GetPosition(obj as Border);

        //            _moveObj.SetValue(Canvas.LeftProperty, point.X - _downPoint.X);
        //            _moveObj.SetValue(Canvas.TopProperty, point.Y - _downPoint.Y);
        //        }
        //    }));
        //}
        #endregion

        private ObservableCollection<ImageDisplayViewModel> dis;

        public ObservableCollection<ImageDisplayViewModel> Dis
        {
            get => dis;
            set => SetProperty(ref dis, value);
        }

        public ViewModelWM()
        {
            Dis = new ObservableCollection<ImageDisplayViewModel>();
            for (int i = 0; i < 4; i++)
            {
                Dis.Add(new ImageDisplayViewModel { DisImage = new Mat(new OpenCvSharp.Size(200, 200), MatType.CV_8UC1).ToBitmapSource() });
            }
        }

        private ObservableCollection<string> result;

        public ObservableCollection<string> Result
        {
            get => result ?? (result = new ObservableCollection<string>());
            set => SetProperty(ref result, value);
        }
        private int targat;

        public int Targat
        {
            get => targat;
            set => SetProperty(ref targat, value);
        }

        private int endPos;

        public int EndPos
        {
            get => endPos;
            set => SetProperty(ref endPos, value);
        }
        private DelegateCommand write;

        public DelegateCommand Write
        {
            get => write ?? (write = new DelegateCommand(() =>
            {
                Result.Clear();
                Result.AddRange(ConstHelper.IntToStringArray(Targat,EndPos));
            }));
        }

    }
}
