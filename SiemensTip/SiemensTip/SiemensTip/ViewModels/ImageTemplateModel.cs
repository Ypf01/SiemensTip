using Microsoft.Win32;
using NodeSettings;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using SiemensTip.Helper;
using SiemensTip.src.Model;
using SiemensTip.Views;
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
    public class ImageTemplateModel : BindableBase
    {
        #region Fields
        private ImageDisplayViewModel _disImage;
        private DelegateCommand<object> canvasMouseMove;
        private DelegateCommand<object> canvasLeftButtonDown;
        private DelegateCommand<object> canvasLeftButtonUp;
        private DelegateCommand<object> loaded;
        private DelegateCommand<object> openFile;
        private DelegateCommand<object> saveFile;
        private DelegateCommand writeBottom;
        private ObservableCollection<ImageTemplateInfo> tempList;
        private DelegateCommand closing;
        private DelegateCommand saveAsFile;
        /// <summary>
        /// 当前框
        /// </summary>
        private List<RectangleArea> theReg;
        private Mat mat = null;
        private ImageTemplateInfo currTargat;

        #region 画框属性
        private Canvas thenCanvas = null;
        private Rectangle currentArea = null;//拖动展示的提示框
        private bool isCanMove = false;//鼠标是否移动
        private System.Windows.Point RegStartPoint;//起始坐标
        private bool IsSelectChild = false;
        private FrameworkElement theRectangle = null;
        private bool isMove = false;
        private System.Windows.Point RegrRlativePoint;//起始坐标
        private FrameworkElement deleteTargat = null;
        #endregion

        #endregion

        #region Properties
        public ImageDisplayViewModel DisImage
        {
            get => _disImage;
            set => SetProperty(ref _disImage, value);
        }
        public ObservableCollection<ImageTemplateInfo> TempList
        {
            get { return tempList; }
            set { tempList = value; }
        }
        public ImageTemplateInfo CurrTargat
        {
            get => currTargat;
            set => SetProperty(ref currTargat, value);
        }
        #endregion

        #region Methods
        private void DrawMultiselectBorder(System.Windows.Point endPoint, System.Windows.Point startPoint)
        {
            if (currentArea == null)
            {
                currentArea = new Rectangle
                {
                    //currentArea.Fill = new SolidColorBrush(Colors.Transparent);
                    //currentArea.Opacity = 0.4;
                    StrokeThickness = 3,
                    Stroke = new SolidColorBrush(Colors.Green),
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
                        RectangleArea area = null;
                        List<Rectangle> rectangles = null;
                        foreach (var regare in theReg)
                        {
                            if (regare.Reg.Equals(deleteTargat))
                            {
                                area = regare;
                                break;
                            }
                            else
                            {
                                foreach (var reg in regare.ListRegs)
                                {
                                    if (reg.Equals(deleteTargat))
                                    {
                                        area = regare;
                                        rectangles = new List<Rectangle>();
                                        rectangles.Add(reg);
                                        break;
                                    }
                                }
                            }
                        }
                        if (rectangles == null && area == null)
                            thenCanvas.Children.Remove(deleteTargat);
                        else
                        {
                            if (rectangles == null && area != null)
                            {
                                thenCanvas.Children.Remove(area.Reg);
                                foreach (var rect in area.ListRegs)
                                    thenCanvas.Children.Remove(rect);
                                theReg.Remove(area);
                            }
                            else if (rectangles != null && area != null && rectangles.Count > 0)
                            {
                                thenCanvas.Children.Remove(rectangles[0]);
                                area.ListRegs.Remove(rectangles[0]);
                            }
                        }
                    }
                    deleteTargat = null;
                };
                var item1 = new MenuItem
                {
                    Header = "绘制直线"
                };
                item1.Click += (s, e) =>
                {
                    if (thenCanvas != null && deleteTargat != null)
                    {
                        if (CurrTargat != null && CurrTargat.Name == ConstHelper.Lateral_template)
                        {
                            if (deleteTargat is Rectangle)
                            {
                                double mul = mat.Width / thenCanvas.Width;
                                Rectangle rec = deleteTargat as Rectangle;
                                double x = Canvas.GetLeft(rec);
                                double y = Canvas.GetTop(rec);
                                OpenCvSharp.Point start = new OpenCvSharp.Point(x, y);
                                OpenCvSharp.Point end = new OpenCvSharp.Point(x + rec.ActualWidth, y + rec.ActualHeight);
                                OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)(start.X * mul), (int)(start.Y * mul), (int)(rec.Width * mul), (int)(rec.Height * mul));
                                Mat roi = new Mat(mat, rect);
                                LateralTemplate lateral = new LateralTemplate();
                                LateralTemplateModel lateralTemplate = new LateralTemplateModel(roi);
                                lateral.DataContext = lateralTemplate;
                                lateral.ShowDialog();
                            }
                        }
                    }
                    deleteTargat = null;
                };
                currentArea.ContextMenu.Items.Add(item);
                currentArea.ContextMenu.Items.Add(item1);
                Canvas.SetZIndex(currentArea, 100);
                this.thenCanvas.Children.Add(currentArea);
            }
            currentArea.Width = Math.Abs(endPoint.X - startPoint.X);
            currentArea.Height = Math.Abs(endPoint.Y - startPoint.Y);
            if (endPoint.X - startPoint.X >= 0)
                Canvas.SetLeft(currentArea, startPoint.X);
            else
                Canvas.SetLeft(currentArea, endPoint.X);
            if (endPoint.Y - startPoint.Y >= 0)
                Canvas.SetTop(currentArea, startPoint.Y);
            else
                Canvas.SetTop(currentArea, endPoint.Y);
        }
        private void Stretch(object sender)
        {
            var mousePosX = Mouse.GetPosition((IInputElement)sender).X;
            var mousePosY = Mouse.GetPosition((IInputElement)sender).Y;

            var xDiff = mousePosX - RegStartPoint.X - ((Rectangle)theRectangle).Width;
            var yDiff = mousePosY - RegStartPoint.Y - ((Rectangle)theRectangle).Height;
            var width = ((Rectangle)theRectangle).Width;
            var heigth = ((Rectangle)theRectangle).Height;

            if (thenCanvas.Cursor == Cursors.SizeNWSE)
            {
                if ((width += xDiff) < 2 || (heigth += yDiff) < 2)
                    return;
                theRectangle.Width += xDiff;
                theRectangle.Height += yDiff;
            }
            else if (thenCanvas.Cursor == Cursors.SizeWE)
            {
                if ((width += xDiff) < 2)
                    return;
                theRectangle.Width += xDiff;
            }
            else if (thenCanvas.Cursor == Cursors.SizeNS)
            {
                if ((heigth += yDiff) < 2)
                    return;
                theRectangle.Height += yDiff;
            }
            else
                thenCanvas.Cursor = Cursors.Arrow;
        }
        private void SaveImageFile(bool isASSave = false)
        {
            if (CurrTargat == null || theReg == null || theReg.Count == 0 || thenCanvas == null || mat == null)
                return;
            string path = string.Empty;
            if (isASSave)
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Title = "选择保存路径";
                saveFile.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
                saveFile.Filter = "bmp文件|*.bmp|jpeg文件|*.jpeg|png文件|*.png";
                saveFile.RestoreDirectory = true;
                saveFile.FileName = CurrTargat.Name;
                if (saveFile.ShowDialog() != true)
                    return;
                path = saveFile.FileName;
            }
            double mul = mat.Width / thenCanvas.Width;
            int i = 0;
            foreach (var item in theReg)
            {
                if (item.Reg != null)
                {
                    Rectangle rec = item.Reg;
                    double x = Canvas.GetLeft(rec);
                    double y = Canvas.GetTop(rec);
                    OpenCvSharp.Point start = new OpenCvSharp.Point(x, y);
                    OpenCvSharp.Point end = new OpenCvSharp.Point(x + rec.ActualWidth, y + rec.ActualHeight);
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)(start.X * mul), (int)(start.Y * mul), (int)(rec.Width * mul), (int)(rec.Height * mul));
                    Mat roi = new Mat(mat, rect);
                    if (i == 0)
                        roi.ImWrite(isASSave ? path : CurrTargat.Addr);
                    else
                    {
                        if (!isASSave)
                            roi.ImWrite(Environment.CurrentDirectory + "\\Model\\" + CurrTargat + i.ToString() + ConstHelper.PngExtension);
                    }
                    i++;
                    if (item.ListRegs != null && item.ListRegs.Count > 0)
                        foreach (var child in item.ListRegs)
                            thenCanvas.Children.Remove(child);
                    thenCanvas.Children.Remove(item.Reg);
                    roi.Dispose();
                }
            }
            theReg.Clear();
        }
        public void SetImage()
        {
            if (Extension.Common.ManaulMat != null)
            {
                mat = Extension.Common.ManaulMat;
                Application.Current.Dispatcher?.Invoke((Action)delegate
            {
                DisImage = new ImageDisplayViewModel() { DisImage = Extension.Common.ManaulMat.ToBitmapSource() };
            });
            }
        }
        #endregion

        #region Initialize
        public ImageTemplateModel()
        {
            theReg = new List<RectangleArea>();
            string path = Environment.CurrentDirectory + "\\Model\\";
            TempList = new ObservableCollection<ImageTemplateInfo>() { new ImageTemplateInfo { Addr = path + ConstHelper.Lateral_template + ConstHelper.PngExtension, ID = 1, Name = ConstHelper.Lateral_template }, new ImageTemplateInfo { Addr = path + ConstHelper.Bottom_template + ConstHelper.PngExtension, ID = 2, Name = ConstHelper.Bottom_template } };
            SiemensTip.Extension.Common.SetImage = SetImage;
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
                    RectangleArea temp = null;
                    foreach (RectangleArea item in theReg)
                        if (Canvas.GetLeft(currentArea) > Canvas.GetLeft(item.Reg) && Canvas.GetTop(currentArea) > Canvas.GetTop(item.Reg) && Canvas.GetLeft(item.Reg) + item.Reg.Width > Canvas.GetLeft(currentArea) + currentArea.Width && Canvas.GetTop(item.Reg) + item.Reg.Height > Canvas.GetTop(item.Reg) + currentArea.Height)
                        {
                            temp = item;
                            break;
                        }
                    if (temp == null)
                        theReg.Add(new RectangleArea { Reg = currentArea });
                    else
                    {
                        temp.ListRegs.Add(currentArea);
                        currentArea.Stroke = new SolidColorBrush(Colors.Blue);
                    }
                    currentArea = null;
                }
                isCanMove = false;
            }));
        }
        public DelegateCommand<object> CanvasLeftButtonDown
        {
            get => canvasLeftButtonDown ?? (canvasLeftButtonDown = new DelegateCommand<object>(c =>
            {
                if (!IsSelectChild)
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
        public DelegateCommand<object> OpenFile
        {
            get => openFile ?? (openFile = new DelegateCommand<object>(c =>
            {
                if (mat != null)
                {
                    mat.Dispose();
                    mat = null;
                }
                Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
                openFile.Title = "选择目标图片";
                openFile.Filter = "bmp文件|*.bmp|jpeg文件|*.jpeg|所有文件|*.*";
                if (openFile.ShowDialog() != false)
                {
                    if (theReg != null && theReg.Count > 0)
                    {
                        foreach (var item in theReg)
                        {
                            foreach (var child in item.ListRegs)
                                thenCanvas.Children.Remove(child);
                            thenCanvas.Children.Remove(item.Reg);
                        }
                        theReg = new List<RectangleArea>();
                    }
                    string fileName = openFile.FileName;
                    if (!File.Exists(fileName))
                        return;
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    mat = Mat.FromStream(fs, ImreadModes.Color);
                    fs.Close();
                    DisImage = new ImageDisplayViewModel();
                    DisImage.DisImage = mat.ToBitmapSource();
                }
            }));
        }
        public DelegateCommand<object> SaveFile
        {
            get => saveFile ?? (saveFile = new DelegateCommand<object>(c =>
            {
                return;
                SaveImageFile();
            }));
        }
        public DelegateCommand Closing
        {
            get => closing ?? (closing = new DelegateCommand(() =>
            {
                if (mat != null)
                    mat.Dispose();
                SiemensTip.Extension.Common.SetImage = null;
            }));
        }
        public DelegateCommand SaveAsFile
        {
            get => saveAsFile ?? (saveAsFile = new DelegateCommand(() =>
            {
                SaveImageFile(true);
            }));
        }
        public DelegateCommand WriteBottom
        {
            get => writeBottom ?? (writeBottom = new DelegateCommand(() =>
            {
                if (CurrTargat == null || theReg == null || theReg.Count == 0 || thenCanvas == null || mat == null)
                    return;
                double mul = mat.Width / thenCanvas.Width;
                Rectangle rec = theReg[0].Reg;
                double x = Canvas.GetLeft(rec);
                double y = Canvas.GetTop(rec);
                OpenCvSharp.Point start = new OpenCvSharp.Point(x, y);
                OpenCvSharp.Point end = new OpenCvSharp.Point(x + rec.ActualWidth, y + rec.ActualHeight);
                OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)(start.X * mul), (int)(start.Y * mul), (int)(rec.Width * mul), (int)(rec.Height * mul));
                Mat roi = new Mat(mat, rect);
                roi.ImWrite(Environment.CurrentDirectory + "\\Model\\" + ConstHelper.Bottom_template + ConstHelper.PngExtension);
                if (Extension.Common.Instance.BottomTemplate())
                    MessageBox.Show("BottomTemplate生成成功，请重新启动软件");
                roi.Dispose();
            }));
        }

        #endregion

        #region Event
        private void CurrentBoxSelectedBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSelectChild && thenCanvas != null && sender is Rectangle && theRectangle != null)
            {
                if (isMove)
                {
                    System.Windows.Point point = Mouse.GetPosition(thenCanvas);
                    theRectangle.SetValue(Canvas.LeftProperty, point.X - RegrRlativePoint.X);
                    theRectangle.SetValue(Canvas.TopProperty, point.Y - RegrRlativePoint.Y);
                }
                else
                    Stretch(thenCanvas);
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle && thenCanvas != null)
            {
                IsSelectChild = true;
                theRectangle = sender as Rectangle;
                //(theRectangle as Rectangle).StrokeDashArray = new DoubleCollection(5);
                System.Windows.Point py = Mouse.GetPosition(thenCanvas);
                RegrRlativePoint = Mouse.GetPosition(theRectangle);
                RegStartPoint.X = Canvas.GetLeft(theRectangle);
                RegStartPoint.Y = Canvas.GetTop(theRectangle);
                theRectangle.Focus();
                if (py.Y >= RegStartPoint.Y - 5 && py.Y <= RegStartPoint.Y + 5)
                {
                    thenCanvas.Cursor = Cursors.Hand;
                    isMove = true;
                }
                else if ((py.Y >= RegStartPoint.Y - 5 + theRectangle.Height && py.Y <= RegStartPoint.Y + 5 + theRectangle.Height) && (py.X >= RegStartPoint.X - 5 + theRectangle.Width && py.X <= RegStartPoint.X + 5 + theRectangle.Width))
                {
                    thenCanvas.Cursor = Cursors.SizeNWSE;
                    isMove = false;
                }
                else if ((py.Y >= RegStartPoint.Y - 5 + theRectangle.Height && py.Y <= RegStartPoint.Y + 5 + theRectangle.Height))
                {
                    thenCanvas.Cursor = Cursors.SizeNS;
                    isMove = false;
                }
                else
                {
                    thenCanvas.Cursor = Cursors.SizeWE;
                    isMove = false;
                }
                Mouse.Capture(theRectangle);
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectChild && sender is Rectangle)
            {
                theRectangle.ReleaseMouseCapture();
                IsSelectChild = false;
                isMove = false;
                thenCanvas.Cursor = Cursors.Arrow;
                (theRectangle as Rectangle).StrokeDashArray = new DoubleCollection(0);
                (sender as Rectangle).Focus();
            }
        }
        private void CurrentBoxSelectedBorder_MouseEnter(object sender, MouseEventArgs e)
        {

            if (thenCanvas != null && sender is Rectangle)
            {
                System.Windows.Point point = Mouse.GetPosition(thenCanvas);
                System.Windows.Point targat = new System.Windows.Point();
                Rectangle rec = sender as Rectangle;
                targat.X = Canvas.GetLeft(rec);
                targat.Y = Canvas.GetTop(rec);
                if (point.Y > targat.Y - 1 && point.Y < targat.Y + 1)
                    thenCanvas.Cursor = Cursors.Hand;
                else if (point.Y > targat.Y - 2 + rec.Height && point.Y < targat.Y + 2 + rec.Height && point.X > targat.X - 2 + rec.Width && point.X < targat.X + 2 + rec.Width)
                    thenCanvas.Cursor = Cursors.SizeNWSE;
                else if (point.X > targat.X - 1 + rec.Width && point.X < targat.X + 1 + rec.Width && point.Y > targat.Y && point.Y < targat.Y + rec.Height)
                    thenCanvas.Cursor = Cursors.SizeWE;
                else if (point.Y > targat.Y - 1 + rec.Height && point.Y < targat.Y + 1 + rec.Height && point.X > targat.X && point.X < targat.X + rec.Width)
                    thenCanvas.Cursor = Cursors.SizeNS;
                else
                    thenCanvas.Cursor = Cursors.Arrow;
            }
        }
        private void CurrentBoxSelectedBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (thenCanvas != null)
                thenCanvas.Cursor = Cursors.Arrow;
        }
        private void CurrentArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            deleteTargat = sender as Rectangle;
        }
        #endregion
    }

}
