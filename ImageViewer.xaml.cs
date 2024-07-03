using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Stereoscopia
{
    /// <summary>
    /// Logica di interazione per ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer
    {
        #region Fields

        private Point mouseClick;
        private double canvasLeft;
        private double canvasTop;

        private bool _locked;
        private int angle = 0;
        private bool StateClosed = true;
        private bool _isEditMode = false;
        private string _imagePath;
        private Ellipse elip = new Ellipse();
        private Point anchorPoint;
        private object _thick;
        private int _thickness;

        private ImageViewerSlave _imageSlave;

        #endregion

        #region Constructor

        public ImageViewer(string imagePath, ImageViewerSlave slave)
        {
            InitializeComponent();
            myimg.Source = new BitmapImage(new Uri(imagePath));

            Locked = false;
            _imageSlave = slave;

            foreach (object obj in mycanv.Children)
            {
                try
                {
                    Image img = (Image)obj;
                    img.PreviewMouseDown += new MouseButtonEventHandler(myimg_MouseDown);
                    img.PreviewMouseMove += new MouseEventHandler(myimg_MouseMove);
                    img.PreviewMouseUp += new MouseButtonEventHandler(myimg_MouseUp);
                    img.TextInput += new TextCompositionEventHandler(myimg_TextInput);
                    img.LostMouseCapture += new MouseEventHandler(myimg_LostMouseCapture);
                    img.SetValue(Canvas.LeftProperty, 0.0);
                    img.SetValue(Canvas.TopProperty, 0.0);
                }
                catch
                { }
            }

            ColorList = new List<string>() { "Red", "Green", "Blue", "Yellow", "Orange", "Violet", "Black" };
            SelectedColor = "Red";

            this.DataContext = this;
        }

        #endregion

        #region Evento Move

        public void myimg_LostMouseCapture(object sender, MouseEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseCaptured = false;
                }));
        }

        public void myimg_TextInput(object sender, TextCompositionEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseCaptured = false;
                }));
        }

        public void myimg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseCaptured = false;  //_imageSlave.MouseUp();
                }));
        }

        public void myimg_MouseMove(object sender, MouseEventArgs e)
        {
            if (((Image)sender).IsMouseCaptured)
            {
                if (!_isEditMode)
                {
                    Point mouseCurrent = e.GetPosition(null);
                    double Left = mouseCurrent.X - mouseClick.X;
                    double Top = mouseCurrent.Y - mouseClick.Y;
                    if (zoomView.ScaleX < 10)
                        Left = Left / 2;
                    else if (zoomView.ScaleX >= 10 && zoomView.ScaleX < 16)
                        Left = Left / 4;
                    else if (zoomView.ScaleX >= 16)
                        Left = Left / 8;


                    if (zoomView.ScaleY < 10)
                        Top = Top / 2;
                    else if (zoomView.ScaleY >= 10 && zoomView.ScaleY < 16)
                        Top = Top / 4;
                    else if (zoomView.ScaleY >= 16)
                        Top = Top / 8;
                    mouseClick = e.GetPosition(null);

                    if (Locked && _imageSlave != null && _imageSlave.Locked)
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            _imageSlave.MouseMove(Left, Top);
                        }));

                    ((Image)sender).SetValue(Canvas.LeftProperty, canvasLeft + Left);
                    ((Image)sender).SetValue(Canvas.TopProperty, canvasTop + Top);
                    canvasLeft = Canvas.GetLeft(((Image)sender));
                    canvasTop = Canvas.GetTop(((Image)sender));
                }
                else
                {
                    Line line = new Line();
                    var mycolor = ColorConverter.ConvertFromString(SelectedColor);
                    line.Stroke = new SolidColorBrush((System.Windows.Media.Color)mycolor);
                    line.X1 = mouseClick.X;
                    line.Y1 = mouseClick.Y;
                    Point tmp = e.MouseDevice.GetPosition(mycanv);
                    line.X2 = tmp.X;// e.GetPosition(this).X;
                    line.Y2 = tmp.Y;// e.GetPosition(this).Y;
                    line.StrokeThickness = 1;// _thickness;
                    mouseClick = e.GetPosition(this);
                    mycanv.Children.Add(line);

                    //Point location = e.MouseDevice.GetPosition(mycanv);

                    //double minX = Math.Min(location.X, anchorPoint.X);
                    //double minY = Math.Min(location.Y, anchorPoint.Y);
                    //double maxX = Math.Max(location.X, anchorPoint.X);
                    //double maxY = Math.Max(location.Y, anchorPoint.Y);

                    //Canvas.SetTop(elip, minY);
                    //Canvas.SetLeft(elip, minX);

                    //double height = maxY - minY;
                    //double width = maxX - minX;

                    //elip.Height = Math.Abs(height);
                    //elip.Width = Math.Abs(width);
                }
            }
        }

        public void myimg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //mouseClick = e.GetPosition(null);
            mouseClick = e.MouseDevice.GetPosition(mycanv);
            canvasLeft = Canvas.GetLeft(((Image)sender));
            canvasTop = Canvas.GetTop(((Image)sender));
            ((Image)sender).CaptureMouse();

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseDown();
                }));

            //anchorPoint = e.MouseDevice.GetPosition(mycanv);
            //var mycolor = ColorConverter.ConvertFromString(SelectedColor);
            ////line.Stroke = new SolidColorBrush((System.Windows.Media.Color)mycolor);
            //elip = new Ellipse
            //{
            //    Stroke = new SolidColorBrush((System.Windows.Media.Color)mycolor),//Brushes.Black,
            //    StrokeThickness = 2
            //};
            //mycanv.Children.Add(elip);
        }

        #endregion

        #region Events

        private void ButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            if (StateClosed)
            {
                Storyboard sb = this.FindResource("OpenMenu") as Storyboard;
                sb.Begin();
                tick.Label = "Tickness";
            }
            else
            {
                Storyboard sb = this.FindResource("CloseMenu") as Storyboard;
                sb.Begin();
                tick.Label = "";
            }

            StateClosed = !StateClosed;
        }

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            if (Locked)
            {
                btnUnLock.Visibility = Visibility.Collapsed;
                btnLock.Visibility = Visibility.Visible;
                btnZoomOut.IsEnabled = btnZoomIn.IsEnabled = false;
            }
            else
            {
                btnUnLock.Visibility = Visibility.Visible;
                btnLock.Visibility = Visibility.Collapsed;
                btnZoomOut.IsEnabled = btnZoomIn.IsEnabled = true;
            }
            Locked = !Locked;
        }

        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            angle = angle + 90;
            rotateView.Angle = angle;
        }
        
        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn();
            else
                ZoomOut();
        }

        private void btnFlipH_Click(object sender, RoutedEventArgs e)
        {
            if (angle > 0 && ((angle/90) % 2 != 0))
                scaleV.ScaleY *= -1;
            else
                scaleH.ScaleX *= -1;
        }

        private void btnFlipV_Click(object sender, RoutedEventArgs e)
        {
            if (angle > 0 && ((angle/90) % 2 != 0))
                scaleH.ScaleX *= -1;
            else
                scaleV.ScaleY *= -1;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    // Zoom In
                    ZoomIn();
                    break;
                case Key.Down:
                    // Zoom Out
                    ZoomOut();
                    break;
                case Key.L:
                    LockAll();
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            LockAll();
        }

        private void btnRotateSlave_Click(object sender, RoutedEventArgs e)
        {
            _imageSlave.Rotate();
        }

        private void btnLockSlave_Click(object sender, RoutedEventArgs e)
        {
            if (_imageSlave.Locked)
            {
                btnUnLockSlave.Visibility = Visibility.Collapsed;
                btnLockSlave.Visibility = Visibility.Visible;
            }
            else
            {
                btnUnLockSlave.Visibility = Visibility.Visible;
                btnLockSlave.Visibility = Visibility.Collapsed;
            }
            _imageSlave.Lock();
        }

        private void btnFlipHSlave_Click(object sender, RoutedEventArgs e)
        {
            _imageSlave.FlipH();
        }

        private void btnFlipVSlave_Click(object sender, RoutedEventArgs e)
        {
            _imageSlave.FlipV();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(mycanv);
            double dpi = 96d;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(mycanv);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                pngEncoder.Save(ms);
                ms.Close();

                System.IO.File.WriteAllBytes(_imagePath, ms.ToArray());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSaveAsImage_Click(object sender, RoutedEventArgs e)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(mycanv);
            double dpi = 96d;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(mycanv);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                pngEncoder.Save(ms);
                ms.Close();



                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = _imagePath.Substring(0, _imagePath.LastIndexOf("\\") + 1);
                dlg.FileName = _imagePath.Substring(_imagePath.LastIndexOf("\\") + 1).Replace(_imagePath.Substring(_imagePath.LastIndexOf(".")), "") + "_" + DateTime.Now.Ticks; // Default file name
                dlg.DefaultExt = _imagePath.Substring(_imagePath.LastIndexOf(".") + 1); // Default file extension
                dlg.Filter = "Image Files (*.*)|*"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    System.IO.File.WriteAllBytes(dlg.FileName, ms.ToArray());
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Public Methods

        public void ZoomIn()
        {
            if (Locked && _imageSlave.Locked)
            {
                if (zoomView.ScaleX < 20)
                    zoomView.ScaleX += .2;
                else
                    zoomView.ScaleX += .4;
                if (zoomView.ScaleY < 20)
                    zoomView.ScaleY += .2;
                else
                    zoomView.ScaleY += .4;

                if (Locked && _imageSlave != null && _imageSlave.Locked)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        _imageSlave.ZoomIn();
                    }));
            }
        }

        public void ZoomOut()
        {
            if (Locked && _imageSlave.Locked)
            {
                if (zoomView.ScaleX < 20)
                    zoomView.ScaleX -= .2;
                else
                    zoomView.ScaleX -= .4;
                if (zoomView.ScaleY < 20)
                    zoomView.ScaleY -= .2;
                else
                    zoomView.ScaleY -= .4;

                if (Locked && _imageSlave != null && _imageSlave.Locked)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        _imageSlave.ZoomOut();
                    }));
            }
        }

        public void LockAll()
        {
            if (Locked)
            {
                btnUnLock.Visibility = Visibility.Collapsed;
                btnLock.Visibility = Visibility.Visible;
                btnZoomOut.IsEnabled = btnZoomIn.IsEnabled = false;
                btnUnLockSlave.Visibility = Visibility.Collapsed;
                btnLockSlave.Visibility = Visibility.Visible;
            }
            else
            {
                btnUnLock.Visibility = Visibility.Visible;
                btnLock.Visibility = Visibility.Collapsed;
                btnZoomOut.IsEnabled = btnZoomIn.IsEnabled = true;
                btnUnLockSlave.Visibility = Visibility.Visible;
                btnLockSlave.Visibility = Visibility.Collapsed;
            }
            _imageSlave.Lock();
            Locked = !Locked;
        }

        #endregion

        #region Properties

        public bool Locked
        {
            get
            {
                return _locked;
            }
            set
            {
                _locked = value;
            }
        }

        public List<string> ColorList { get; set; }
        public string SelectedColor { get; set; }

        public object SelectedThickness
        {
            get
            {
                return _thick;
            }
            set
            {
                _thick = value;
                _thickness = Convert.ToInt32(((RibbonGalleryItem)_thick).Tag);
            }
        }

        #endregion

    }
}