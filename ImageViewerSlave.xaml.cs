using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private ImageViewer _imageMaster;
        private System.Drawing.Bitmap _bitmap;

        #endregion

        #region Constructor

        public ImageViewerSlave(string imagePath)
        {
            InitializeComponent();
            //myimg.Source = new BitmapImage(new Uri(imagePath));

            _bitmap = new System.Drawing.Bitmap(imagePath);
            IntPtr hBitmap = _bitmap.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            myimg.Source = WpfBitmap;

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
                //Console.WriteLine(string.Format("Auto -> Left: {0}, Top: {1}", canvasLeft, canvasTop));
            }
        }

        public void MouseDown()
        {
            _isMouseCaptured = true;
        }

        public void ZoomIn()
        {
            Point p = viewFinderSlave.TranslatePoint(new Point(0, 0), myimg);

            Matrix m = myimg.RenderTransform.Value;
            m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
            myimg.RenderTransform = new MatrixTransform(m);

            /*ScaleTransform st = new ScaleTransform();
            st.ScaleX += .2;
            st.ScaleY += .2;
            if (myimg.RenderTransform is TransformGroup)
            {
                ((TransformGroup)myimg.RenderTransform).Children.Add(st);
            }
            else
            {
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(st);
                myimg.RenderTransform = myTransformGroup;
            }*/
        }

        public void ZoomOut()
        {
            Point p = viewFinderSlave.TranslatePoint(new Point(0, 0), myimg);

            Matrix m = myimg.RenderTransform.Value;
            m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);
            myimg.RenderTransform = new MatrixTransform(m);

            /*ScaleTransform st = new ScaleTransform();
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
            }*/
        }

        public void Rotate()
        {
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(_bitmap.GetHbitmap());
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);

            _bitmap = new System.Drawing.Bitmap(image);
            IntPtr hBitmap = _bitmap.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            myimg.Source = WpfBitmap;
        }

        public void Lock()
        {
            if (Locked)
            {
                //btnUnLock.Visibility = Visibility.Collapsed;
                //btnLock.Visibility = Visibility.Visible;

                myimg.PreviewMouseDown += new MouseButtonEventHandler(myimg_MouseDown);
                myimg.PreviewMouseMove += new MouseEventHandler(myimg_MouseMove);
                myimg.PreviewMouseUp += new MouseButtonEventHandler(myimg_MouseUp);
                myimg.TextInput += new TextCompositionEventHandler(myimg_TextInput);
                myimg.LostMouseCapture += new MouseEventHandler(myimg_LostMouseCapture);
            }
            else
            {
                //btnUnLock.Visibility = Visibility.Visible;
                //btnLock.Visibility = Visibility.Collapsed;

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
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(_bitmap.GetHbitmap());
            image.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);

            _bitmap = new System.Drawing.Bitmap(image);
            IntPtr hBitmap = _bitmap.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            myimg.Source = WpfBitmap;
        }

        public void FlipV()
        {
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(_bitmap.GetHbitmap());
            image.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

            _bitmap = new System.Drawing.Bitmap(image);
            IntPtr hBitmap = _bitmap.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            myimg.Source = WpfBitmap;
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

        private void Window_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ImageMaster.ZoomIn();
            else
                ImageMaster.ZoomOut();
        }
    }
}