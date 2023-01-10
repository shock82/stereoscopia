using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Stereoscopia
{
    /// <summary>
    /// Logica di interazione per ImageViewerSlave.xaml
    /// </summary>
    public partial class ImageViewerSlave : Window
    {
        #region Fields

        private Point mouseClick;
        private double canvasLeft;
        private double canvasTop;

        private bool _locked;
        private bool _isMouseCaptured = false;
        private int angle;

        private ImageViewer _imageMaster;
        //private System.Drawing.Bitmap _bitmap;

        #endregion

        #region Constructor

        public ImageViewerSlave(string imagePath)
        {
            InitializeComponent();
            myimg.Source = new BitmapImage(new Uri(imagePath));

            Locked = false;
            
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

                    FlipH();
                }
                catch
                { }
            }
        }

        #endregion

        #region Evento Move

        void myimg_LostMouseCapture(object sender, MouseEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        void myimg_TextInput(object sender, TextCompositionEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        void myimg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        void myimg_MouseMove(object sender, MouseEventArgs e)
        {
            if (((Image)sender).IsMouseCaptured)
            {
                Point mouseCurrent = e.GetPosition(null);
                double Left = mouseCurrent.X - mouseClick.X;
                double Top = mouseCurrent.Y - mouseClick.Y;
                mouseClick = e.GetPosition(null);

                ((Image)sender).SetValue(Canvas.LeftProperty, canvasLeft + Left);
                ((Image)sender).SetValue(Canvas.TopProperty, canvasTop + Top);
                canvasLeft = Canvas.GetLeft(((Image)sender));
                canvasTop = Canvas.GetTop(((Image)sender));
            }
        }

        void myimg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseClick = e.GetPosition(null);
            canvasLeft = Canvas.GetLeft(((Image)sender));
            canvasTop = Canvas.GetTop(((Image)sender));
            ((Image)sender).CaptureMouse();
        }

        #endregion

        #region Events

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImageMaster.LockAll();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    // Zoom In
                    ImageMaster.ZoomIn();
                    break;
                case Key.Down:
                    // Zoom Out
                    ImageMaster.ZoomOut();
                    break;
                case Key.L:
                    ImageMaster.LockAll();
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ImageMaster.ZoomIn();
            else
                ImageMaster.ZoomOut();
        }

        #endregion

        #region Public Methods

        public void MouseMove(double Left, double Top)
        {
            if (_isMouseCaptured)
            {
                myimg.SetValue(Canvas.LeftProperty, canvasLeft + (Left*-1));
                myimg.SetValue(Canvas.TopProperty, canvasTop + Top);
                canvasLeft = Canvas.GetLeft(myimg);
                canvasTop = Canvas.GetTop(myimg);
            }
        }

        public void MouseDown()
        {
            _isMouseCaptured = true;
        }

        public void ZoomIn()
        {
            zoomView.ScaleX += .2;
            zoomView.ScaleY += .2;
        }

        public void ZoomOut()
        {
            zoomView.ScaleX -= .2;
            zoomView.ScaleY -= .2;
        }

        public void Rotate()
        {
            angle = angle + 90;
            rotateView.Angle = angle;
        }

        public void Lock()
        {
            if (Locked)
            {
                myimg.PreviewMouseDown += new MouseButtonEventHandler(myimg_MouseDown);
                myimg.PreviewMouseMove += new MouseEventHandler(myimg_MouseMove);
                myimg.PreviewMouseUp += new MouseButtonEventHandler(myimg_MouseUp);
                myimg.TextInput += new TextCompositionEventHandler(myimg_TextInput);
                myimg.LostMouseCapture += new MouseEventHandler(myimg_LostMouseCapture);
            }
            else
            {
                myimg.PreviewMouseDown -= new MouseButtonEventHandler(myimg_MouseDown);
                myimg.PreviewMouseMove -= new MouseEventHandler(myimg_MouseMove);
                myimg.PreviewMouseUp -= new MouseButtonEventHandler(myimg_MouseUp);
                myimg.TextInput -= new TextCompositionEventHandler(myimg_TextInput);
                myimg.LostMouseCapture -= new MouseEventHandler(myimg_LostMouseCapture);
            }
            Locked = !Locked;
        }

        public void FlipH()
        {
            if (angle > 0 && ((angle / 90) % 2 != 0))
                scaleV.ScaleY *= -1;
            else
                scaleH.ScaleX *= -1;
        }

        public void FlipV()
        {
            if (angle > 0 && ((angle / 90) % 2 != 0))
                scaleH.ScaleX *= -1;
            else
                scaleV.ScaleY *= -1;
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

        public bool MouseCaptured
        {
            get
            {
                return _isMouseCaptured;
            }
            set
            {
                _isMouseCaptured = value;
            }
        }

        public ImageViewer ImageMaster
        {
            get
            {
                return _imageMaster;
            }
            set
            {
                _imageMaster = value;
            }
        }

        #endregion

    }
}