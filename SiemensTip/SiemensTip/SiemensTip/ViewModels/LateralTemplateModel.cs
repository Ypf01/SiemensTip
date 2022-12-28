using Microsoft.Win32;
using NodeSettings;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Helper;
using SiemensTip.src.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SiemensTip.ViewModels
{
    public class LateralTemplateModel : BindableBase
    {
        #region Fields
        private ImageDisplayViewModel _disImage;
        private DelegateCommand<object> canvasMouseMove;
        private DelegateCommand<object> canvasLeftButtonDown;
        private DelegateCommand<object> canvasLeftButtonUp;
        private DelegateCommand<object> loaded;
        private DelegateCommand closing;
        private DelegateCommand<object> writeLateral;
        /// <summary>
        /// 线集合
        /// </summary>
        private List<Line> Lines;
        public Mat mat = null;

        #region 画框属性
        private Canvas thenCanvas = null;
        private Line currentArea = null;//拖动展示的提示框
        private bool isCanMove = false;//鼠标是否移动
        private System.Windows.Point RegStartPoint;//起始坐标
        private bool IsSelectChild = false;
        private FrameworkElement theLine = null;
        private bool isMove = false;
        private System.Windows.Point RegrRlativePoint;//起始坐标
        private FrameworkElement deleteTargat = null;
        #endregion

        #endregion

        #region Initialize
        public LateralTemplateModel(Mat _mat)
        {
            Lines = new List<Line>();
            if (_mat != null)
            {
                mat = _mat;
                this.DisImage = new ImageDisplayViewModel() { DisImage = mat.ToBitmapSource() };
            }
        }
        #endregion

        #region Properties
        public ImageDisplayViewModel DisImage
        {
            get => _disImage;
            set => SetProperty(ref _disImage, value);
        }
        #endregion

        #region Methods
        private void DrawMultiselectBorder(System.Windows.Point endPoint, System.Windows.Point startPoint)
        {
            if (currentArea == null)
            {
                currentArea = new Line
                {
                    StrokeThickness = 5,
                    Stroke = new SolidColorBrush(Colors.Red),
                    ContextMenu = new ContextMenu()
                };
                var item = new MenuItem
                {
                    Header = "删除"
                };
                item.Click += (s, e) =>
                {
                    if (thenCanvas != null && deleteTargat != null)
                    {
                        Line area = null;
                        foreach (var regare in Lines)
                        {
                            if (regare == deleteTargat)
                            {
                                area = regare;
                                thenCanvas.Children.Remove(deleteTargat);
                            }
                        }
                        Lines.Remove(area);
                    }
                    deleteTargat = null;
                };
                currentArea.ContextMenu.Items.Add(item);
                Canvas.SetZIndex(currentArea, 100);
                this.thenCanvas.Children.Add(currentArea);
            }
            currentArea.X1 = startPoint.X;
            currentArea.Y1 = startPoint.Y;
            currentArea.X2 = endPoint.X;
            currentArea.Y2 = endPoint.Y;
        }
        private void Stretch(object sender)
        {
            var mousePosX = Mouse.GetPosition((IInputElement)sender).X;
            var mousePosY = Mouse.GetPosition((IInputElement)sender).Y;

            var xDiff = mousePosX - RegStartPoint.X - ((Line)theLine).Width;
            var yDiff = mousePosY - RegStartPoint.Y - ((Line)theLine).Height;
            var width = ((Line)theLine).Width;
            var heigth = ((Line)theLine).Height;

            if (thenCanvas.Cursor == Cursors.SizeNWSE)
            {
                if ((width += xDiff) < 2 || (heigth += yDiff) < 2)
                    return;
                theLine.Width += xDiff;
                theLine.Height += yDiff;
            }
            else if (thenCanvas.Cursor == Cursors.SizeWE)
            {
                if ((width += xDiff) < 2)
                    return;
                theLine.Width += xDiff;
            }
            else if (thenCanvas.Cursor == Cursors.SizeNS)
            {
                if ((heigth += yDiff) < 2)
                    return;
                theLine.Height += yDiff;
            }
            else
                thenCanvas.Cursor = Cursors.Arrow;
        }

        public void SetImage()
        {
            if (Extension.Common.ManaulMat != null)
            {
                Application.Current.Dispatcher?.Invoke((Action)delegate
            {
                DisImage = new ImageDisplayViewModel() { DisImage = SiemensTip.Extension.Common.ManaulMat.ToBitmapSource() };
            });
            }
        }
        #endregion

        #region Command
        public DelegateCommand<object> CanvasLeftButtonUp
        {
            get => canvasLeftButtonUp ?? (canvasLeftButtonUp = new DelegateCommand<object>(c =>
            {
                if (currentArea != null)
                {
                    currentArea.MouseLeftButtonUp += CurrentBoxSelectedBorder_MouseLeftButtonUp;
                    currentArea.MouseLeftButtonDown += CurrentBoxSelectedBorder_MouseLeftButtonDown;
                    currentArea.MouseMove += CurrentBoxSelectedBorder_MouseMove;
                    currentArea.MouseLeave += CurrentBoxSelectedBorder_MouseLeave;
                    currentArea.MouseEnter += CurrentBoxSelectedBorder_MouseEnter;
                    currentArea.MouseRightButtonUp += CurrentArea_MouseRightButtonUp;
                    Lines.Add(currentArea);
                    currentArea = null;
                }
                isCanMove = false;
            }));
        }
        public DelegateCommand<object> CanvasLeftButtonDown
        {
            get => canvasLeftButtonDown ?? (canvasLeftButtonDown = new DelegateCommand<object>(c =>
            {
                if (!IsSelectChild && Lines.Count == 0)
                {
                    MouseButtonEventArgs parent = c as MouseButtonEventArgs;
                    if (parent.Source is Image)
                        thenCanvas = (VisualTreeHelper.GetParent(parent.Source as Image) as Canvas);
                    if (thenCanvas == null)
                        return;
                    isCanMove = true;
                    RegStartPoint = Mouse.GetPosition(thenCanvas);
                    parent.Handled = true;
                }
            }));
        }
        public DelegateCommand<object> CanvasMouseMove
        {
            get => canvasMouseMove ?? (canvasMouseMove = new DelegateCommand<object>(c =>
            {
                if (isCanMove)
                {
                    System.Windows.Point tempEndPoint = Mouse.GetPosition(thenCanvas);
                    //绘制跟随鼠标移动的方框
                    DrawMultiselectBorder(tempEndPoint, RegStartPoint);
                }
            }));
        }
        public DelegateCommand<object> Loaded
        {
            get => loaded ?? (loaded = new DelegateCommand<object>(c =>
            {
                if (c is Canvas)
                    thenCanvas = c as Canvas;
            }));
        }
        public DelegateCommand Closing
        {
            get => closing ?? (closing = new DelegateCommand(() =>
            {
                if (mat != null)
                    mat.Dispose();
            }));
        }
        public DelegateCommand<object> WriteLateral
        {
            get => writeLateral ?? (writeLateral = new DelegateCommand<object>(c =>
            {
                if (mat != null && Lines != null && Lines.Count > 0)
                {
                    Line line = Lines.FirstOrDefault();
                    double mul = mat.Width / thenCanvas.Width;
                    OpenCvSharp.Point start = new OpenCvSharp.Point((int)(line.X1 * mul), (int)(line.Y1 * mul));
                    OpenCvSharp.Point end = new OpenCvSharp.Point((int)(line.X2 * mul), (int)(line.Y2 * mul));
                    mat.ImWrite(Environment.CurrentDirectory + "\\Model\\" + ConstHelper.Lateral_template + ConstHelper.PngExtension);
                    if (Extension.Common.Instance.LateralTemplate(new List<OpenCvSharp.Point>() { start, end }))
                        MessageBox.Show("LateralTemplate生成成功，请重新启动软件");
                    if (c is System.Windows.Window)
                        (c as System.Windows.Window).DialogResult = true;
                }
            }));
        }

        #endregion

        #region Event
        private void CurrentBoxSelectedBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSelectChild && thenCanvas != null && sender is Line && theLine != null)
            {
                if (isMove)
                {
                    System.Windows.Point point = Mouse.GetPosition(thenCanvas);
                    theLine.SetValue(Canvas.LeftProperty, point.X - RegrRlativePoint.X);
                    theLine.SetValue(Canvas.TopProperty, point.Y - RegrRlativePoint.Y);
                }
                else
                    Stretch(thenCanvas);
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Line && thenCanvas != null)
            {
                IsSelectChild = true;
                theLine = sender as Line;
                //(theLine as Line).StrokeDashArray = new DoubleCollection(5);
                System.Windows.Point py = Mouse.GetPosition(thenCanvas);
                RegrRlativePoint = Mouse.GetPosition(theLine);
                RegStartPoint.X = Canvas.GetLeft(theLine);
                RegStartPoint.Y = Canvas.GetTop(theLine);
                theLine.Focus();
                isMove = true;
                Mouse.Capture(theLine);
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectChild && sender is Line)
            {
                theLine.ReleaseMouseCapture();
                IsSelectChild = false;
                isMove = false;
                thenCanvas.Cursor = Cursors.Arrow;
                (theLine as Line).StrokeDashArray = new DoubleCollection(0);
                (sender as Line).Focus();
            }
        }
        private void CurrentBoxSelectedBorder_MouseEnter(object sender, MouseEventArgs e)
        {

            if (thenCanvas != null && sender is Line)
            {
                System.Windows.Point point = Mouse.GetPosition(thenCanvas);
                System.Windows.Point targat = new System.Windows.Point();
                Line rec = sender as Line;
                targat.X = Canvas.GetLeft(rec);
                targat.Y = Canvas.GetTop(rec);
                thenCanvas.Cursor = Cursors.Hand;
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (thenCanvas != null)
                thenCanvas.Cursor = Cursors.Arrow;
        }
        private void CurrentArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            deleteTargat = sender as Line;
        }
        #endregion
    }
}
