using System;
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
        private int angle = 0;

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

        #region Public Methods

        public void MouseMove(double Left, double Top)
        {
            if (_isMouseCaptured)
            {
                //Point mouseCurrent = e.GetPosition(null);
                //double Left = mouseCurrent.X - mouseClick.X;
                //double Top = mouseCurrent.Y - mouseClick.Y;
                //mouseClick = mouseCurrent;

                myimg.SetValue(Canvas.LeftProperty, canvasLeft + (Left*-1));
                myimg.SetValue(Canvas.TopProperty, canvasTop + Top);
                canvasLeft = Canvas.GetLeft(myimg);
                canvasTop = Canvas.GetTop(myimg);
                Console.WriteLine(string.Format("Auto -> Left: {0}, Top: {1}", canvasLeft, canvasTop));
            }
        }

        public void MouseDown()
        {
            _isMouseCaptured = true;
        }

        public void ZoomIn()
        {
            ScaleTransform st = new ScaleTransform();
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
            }
        }

        public void ZoomOut()
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
        }

        public void Window_MouseWheel(int delta)
        {
            Point p = viewFinder.TranslatePoint(new Point(0, 0), myimg);

            Matrix m = myimg.RenderTransform.Value;
            if (delta > 0)
                m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
            else
                m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

            myimg.RenderTransform = new MatrixTransform(m);
        }

        public void Rotate()
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
            myimg.RenderTransform = myTransformGroup;
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

        public void FlipV()
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

        #endregion

        #region Events

        //private void btnLock_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Locked)
        //    {
        //        btnUnLock.Visibility = Visibility.Collapsed;
        //        btnLock.Visibility = Visibility.Visible;

        //        myimg.PreviewMouseDown += new MouseButtonEventHandler(myimg_MouseDown);
        //        myimg.PreviewMouseMove += new MouseEventHandler(myimg_MouseMove);
        //        myimg.PreviewMouseUp += new MouseButtonEventHandler(myimg_MouseUp);
        //        myimg.TextInput += new TextCompositionEventHandler(myimg_TextInput);
        //        myimg.LostMouseCapture += new MouseEventHandler(myimg_LostMouseCapture);
        //    }
        //    else
        //    {
        //        btnUnLock.Visibility = Visibility.Visible;
        //        btnLock.Visibility = Visibility.Collapsed;

        //        myimg.PreviewMouseDown -= new MouseButtonEventHandler(myimg_MouseDown);
        //        myimg.PreviewMouseMove -= new MouseEventHandler(myimg_MouseMove);
        //        myimg.PreviewMouseUp -= new MouseButtonEventHandler(myimg_MouseUp);
        //        myimg.TextInput -= new TextCompositionEventHandler(myimg_TextInput);
        //        myimg.LostMouseCapture -= new MouseEventHandler(myimg_LostMouseCapture);
        //    }
        //    Locked = !Locked;
        //}

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
            myimg.RenderTransform = myTransformGroup;
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

        #endregion
    }
}
