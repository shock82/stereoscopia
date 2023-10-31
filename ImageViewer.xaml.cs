using System;
//using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
                Point mouseCurrent = e.GetPosition(null);
                double Left = mouseCurrent.X - mouseClick.X;
                double Top = mouseCurrent.Y - mouseClick.Y;
                if (zoomView.ScaleX < 10)
                    Left = Left / 2;
                else if (zoomView.ScaleX >= 10 && zoomView.ScaleX < 16)
                    Left = Left / 4;
                else if (zoomView.ScaleX >= 16 )
                    Left = Left / 8;


                if (zoomView.ScaleY < 10)
                    Top = Top / 2;
                else if (zoomView.ScaleY >= 10 && zoomView.ScaleY < 16)
                    Top = Top / 4;
                else if (zoomView.ScaleY >= 16 )
                    Top = Top / 8;
                mouseClick = e.GetPosition(null);

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseMove(Left, Top);
                }));

                ((Image)sender).SetValue(Canvas.LeftProperty, canvasLeft + Left);
                ((Image)sender).SetValue(Canvas.TopProperty, canvasTop + Top);
                canvasLeft = Canvas.GetLeft(((Image)sender));
                canvasTop = Canvas.GetTop(((Image)sender));
            }
        }

        public void myimg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseClick = e.GetPosition(null);
            canvasLeft = Canvas.GetLeft(((Image)sender));
            canvasTop = Canvas.GetTop(((Image)sender));
            ((Image)sender).CaptureMouse();

            if (Locked && _imageSlave != null && _imageSlave.Locked)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _imageSlave.MouseDown();
                }));
        }

        #endregion

        #region Events

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



        #endregion

    }
}