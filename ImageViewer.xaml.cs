using System;
//using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

            //mouseClick = e.GetPosition(null);

            //canvasLeft = Canvas.GetLeft(((Image)sender));
            //canvasTop = Canvas.GetTop(((Image)sender));

            //if (canvasLeft < 0)
            //    canvasLeft = 0;

            //if (canvasTop < 0)
            //    canvasTop = 0;

            //if (canvasLeft > mycanv.ActualWidth)
            //    canvasLeft = mycanv.ActualWidth - ((Image)sender).ActualWidth;

            //if (canvasTop > mycanv.ActualHeight)
            //    canvasTop = mycanv.ActualHeight - ((Image)sender).ActualHeight;

            //((Image)sender).SetValue(Canvas.LeftProperty, canvasLeft);
            //((Image)sender).SetValue(Canvas.TopProperty, canvasTop);
        }

        public void myimg_MouseMove(object sender, MouseEventArgs e)
        {
            if (((Image)sender).IsMouseCaptured)
            {
                Point mouseCurrent = e.GetPosition(null);
                double Left = mouseCurrent.X - mouseClick.X;
                double Top = mouseCurrent.Y - mouseClick.Y;
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
            RotateTransform rotateTransform = new RotateTransform(angle);
            TranslateTransform translateTransform = new TranslateTransform();
            switch (angle)
            {
                case 90:
                    translateTransform.X = myimg.ActualHeight;
                    break;
                case 180:
                    translateTransform.Y = myimg.ActualHeight;
                    translateTransform.X = myimg.ActualWidth;
                    break;
                case 270:
                    translateTransform.Y = myimg.ActualWidth;
                    break;
            }
            if (angle == 360) angle = 0;
            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(rotateTransform);
            myTransformGroup.Children.Add(translateTransform);
            //myimg.RenderTransformOrigin = new Point(0.5, 0.5);
            myimg.RenderTransform = myTransformGroup;

            //Point centerPoint = new Point();
            //switch (angle)
            //{
            //    case 0: centerPoint = new Point(myimg.ActualHeight / 2, myimg.ActualHeight / 2); break;
            //    case 90: centerPoint = new Point(myimg.ActualWidth / 2, myimg.ActualHeight / 2); break;
            //    case 180: centerPoint = new Point(myimg.ActualWidth / 2, myimg.ActualWidth / 2); break;
            //    default: break;
            //}
            //angle = angle + 90;
            //if (angle == 360) angle = 0;
            //RotateTransform rotateTransform = new RotateTransform(angle) { CenterX = centerPoint.X, CenterY = centerPoint.Y };
            //myimg.RenderTransform = rotateTransform;
        }
        
        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (Locked && _imageSlave.Locked)
            {
                ScaleTransform st = new ScaleTransform();
                st.ScaleX += .2;
                st.ScaleY += .2;
                //myimg.RenderTransformOrigin = new Point(myimg.ActualHeight / 2, myimg.ActualHeight / 2);
                if (myimg.RenderTransform is TransformGroup)
                {
                    ((TransformGroup)myimg.RenderTransform).Children.Add(st);
                }
                else
                {
                    TransformGroup myTransformGroup = new TransformGroup();
                    myTransformGroup.Children.Add(st);
                    myimg.RenderTransform = myTransformGroup;
                }

                if (Locked && _imageSlave != null && _imageSlave.Locked)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        _imageSlave.ZoomIn();
                    }));
            }
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (Locked && _imageSlave.Locked)
            {
                ScaleTransform st = new ScaleTransform();
                st.ScaleX -= .2;
                st.ScaleY -= .2;

                if (myimg.RenderTransform is TransformGroup)
                {
                    ((TransformGroup)myimg.RenderTransform).Children.Add(st);
                }
                else
                {
                    TransformGroup myTransformGroup = new TransformGroup();
                    myTransformGroup.Children.Add(st);
                    myimg.RenderTransform = myTransformGroup;
                }

                if (Locked && _imageSlave != null && _imageSlave.Locked)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        _imageSlave.ZoomOut();
                    }));
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Locked && _imageSlave.Locked)
            {
                Point p = viewfinder.TranslatePoint(new Point(0, 0), myimg);

                Matrix m = myimg.RenderTransform.Value;
                if (e.Delta > 0)
                    m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
                else
                    m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

                myimg.RenderTransform = new MatrixTransform(m);

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _imageSlave.Window_MouseWheel(e.Delta);
                }));
            }
        }

        private void btnFlipH_Click(object sender, RoutedEventArgs e)
        {
            myimg.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleX = -1;
            if (myimg.RenderTransform is TransformGroup)
            {
                ((TransformGroup)myimg.RenderTransform).Children.Add(flipTrans);
            }
            else
            {
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(flipTrans);
                myimg.RenderTransform = myTransformGroup;
            }
        }

        private void btnFlipV_Click(object sender, RoutedEventArgs e)
        {
            myimg.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleY = -1;
            if (myimg.RenderTransform is TransformGroup)
            {
                ((TransformGroup)myimg.RenderTransform).Children.Add(flipTrans);
            }
            else
            {
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(flipTrans);
                myimg.RenderTransform = myTransformGroup;
            }
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