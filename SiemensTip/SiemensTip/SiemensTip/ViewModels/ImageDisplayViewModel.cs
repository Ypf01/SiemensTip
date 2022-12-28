using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SiemensTip.ViewModels
{
    public class ImageDisplayViewModel : BindableBase
    {
        #region Fields
        private DelegateCommand<object> mouseRightButtonUp;
        private DelegateCommand<object> mouseWheel;
        private DelegateCommand<object> mouseLeftButtonDown;
        private DelegateCommand<object> mouseLeftButtonUp;
        private DelegateCommand<object> mouseMove;
        bool _isMoving = false;
        FrameworkElement _moveObj = null;
        Point _downPoint = new Point();
        private ImageSource _disImage;
        #endregion

        #region Properties
        public DelegateCommand<object> MouseRightButtonUp
        {
            get => mouseRightButtonUp ?? (mouseRightButtonUp = new DelegateCommand<object>(c =>
            {
                Image image = (c as MouseButtonEventArgs).Source as Image;
                image.RenderTransform = image.LayoutTransform;
            }));
        }
        public DelegateCommand<object> MouseWheel
        {
            get => mouseWheel ?? (mouseWheel = new DelegateCommand<object>(c =>
            {
                MouseWheelEventArgs e = c as MouseWheelEventArgs;
                Image image = e.Source as Image;
                if (image != null)
                {
                    var control = image.Parent as FrameworkElement;

                    System.Windows.Point p = e.GetPosition(image);
                    Matrix m = image.RenderTransform.Value;
                    if (e.Delta > 0)
                        m.ScaleAtPrepend(ConstHelper.ZoomOutScaleValue, ConstHelper.ZoomOutScaleValue, p.X, p.Y);
                    else
                        m.ScaleAtPrepend(ConstHelper.ZoomInScaleValue, ConstHelper.ZoomInScaleValue, p.X, p.Y);

                    image.RenderTransform = new MatrixTransform(m);
                }
            }));
        }
        public DelegateCommand<object> MouseLeftButtonDown
        {
            get => mouseLeftButtonDown ?? (mouseLeftButtonDown = new DelegateCommand<object>(obj =>
            {
                _moveObj = (obj as MouseButtonEventArgs).Source as FrameworkElement;
                _downPoint = Mouse.GetPosition(_moveObj);
                _moveObj.CaptureMouse();
                _isMoving = true;
            }));
        }
        public DelegateCommand<object> MouseLeftButtonUp
        {
            get => mouseLeftButtonUp ?? (mouseLeftButtonUp = new DelegateCommand<object>(obj =>
            {
                _isMoving = false;
                _moveObj.ReleaseMouseCapture();
                _moveObj = null;
            }));
        }
        public DelegateCommand<object> MouseMove
        {
            get => mouseMove ?? (mouseMove = new DelegateCommand<object>(obj =>
            {
                if (_isMoving && _moveObj != null)
                {
                    Point point = Mouse.GetPosition(obj as Canvas);

                    _moveObj.SetValue(Canvas.LeftProperty, point.X - _downPoint.X);
                    _moveObj.SetValue(Canvas.TopProperty, point.Y - _downPoint.Y);
                }
            }));
        }
        public ImageSource DisImage
        {
            get => _disImage;
            set => SetProperty(ref _disImage, value);
        }
        #endregion
    }
}
