using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Stereoscopia
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _fileImage1 = "", _fileImage2 = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnImg1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Files|*.jpg;*.jpeg;*.png;*.tif;*.tiff;";
            if (openFileDialog.ShowDialog() == true)
            {
                _fileImage1 = openFileDialog.FileName;
                txbImg1.Text = _fileImage1;
                //myimg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void btnImg2_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Files|*.jpg;*.jpeg;*.png;*.tif;*.tiff;";
            if (openFileDialog.ShowDialog() == true)
            {
                _fileImage2 = openFileDialog.FileName;
                txbImg2.Text = _fileImage2;
            }
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_fileImage1) || string.IsNullOrEmpty(_fileImage2))
            {
                System.Windows.Forms.MessageBox.Show("Seleziona le immagini da visualizzare");
                return;
            }
            ImageViewerSlave ivSlave = new ImageViewerSlave(_fileImage2);
            ImageViewer ivMaster = new ImageViewer(_fileImage1, ivSlave);

            Screen s1 = Screen.AllScreens[0];
            Screen s2 = Screen.AllScreens[1];

            Rectangle r1 = s1.WorkingArea;
            Rectangle r2 = s2.WorkingArea;

            ivMaster.Top = r1.Top;
            ivMaster.Left = r1.Left;
            ivMaster.Width = r1.Width;
            ivMaster.Height = r1.Height;

            ivSlave.Top = r2.Top;
            ivSlave.Left = r2.Left;
            ivSlave.Width = r2.Width;
            ivSlave.Height = r2.Height;

            ivMaster.Show();
            ivSlave.Show();

            ivSlave.WindowState = WindowState.Maximized;
            ivMaster.WindowState = WindowState.Maximized;

            ivSlave.Owner = ivMaster;
        }
    }
}
