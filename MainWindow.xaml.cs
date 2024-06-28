using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Stereoscopia
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private string _fileImage1 = "", _fileImage2 = "";

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            MachineTypePlanar = true;
            AllDisplay = Screen.AllScreens.ToList();
            ReadInput();
            this.DataContext = this;
        }

        #endregion

        private void ReadInput()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (isoStore.FileExists("StereoScopia_Input.txt"))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("StereoScopia_Input.txt", FileMode.Open, isoStore))
                {
                    string line;
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        int count = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            switch (count)
                            {
                                case 0:
                                    Display_1 = AllDisplay.First(op => op.DeviceName.Equals(line));
                                    break;
                                case 1:
                                    Display_2 = AllDisplay.First(op => op.DeviceName.Equals(line));
                                    break;
                                case 2:
                                    MachineTypePlanar = Convert.ToBoolean(line);
                                    break;
                                case 3:
                                    MachineTypePluraview = Convert.ToBoolean(line);
                                    break;
                            }
                            count++;
                        }
                    }
                }
            }
            else
            {
                Display_1 = AllDisplay.FirstOrDefault(op => op.DeviceName.Contains("1"));
                Display_2 = AllDisplay.FirstOrDefault(op => op.DeviceName.Contains("2"));
            }
        }

        private void WriteInput()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            //if (isoStore.FileExists("StereoScopia_Input.txt"))
            //{
            //    //Console.WriteLine("The file already exists!");
            //    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("TestStore.txt", FileMode.Open, isoStore))
            //    {
            //        using (StreamReader reader = new StreamReader(isoStream))
            //        {
            //            string disp1 = reader.ReadLine();
            //            string disp2 = reader.ReadLine();
            //            //Console.WriteLine("Reading contents:");
            //            //Console.WriteLine(reader.ReadToEnd());
            //        }
            //    }
            //}
            //else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("StereoScopia_Input.txt", FileMode.OpenOrCreate, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.WriteLine(Display_1.DeviceName);
                        writer.WriteLine(Display_2.DeviceName);
                        writer.WriteLine(MachineTypePlanar.ToString());
                        writer.WriteLine(MachineTypePluraview.ToString());
                        //Console.WriteLine("You have written to the file.");
                    }
                }
            }
        }

        #region Events

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
            ImageViewerSlave ivSlave = new ImageViewerSlave(_fileImage2, MachineTypePlanar);
            ImageViewer ivMaster = new ImageViewer(_fileImage1, ivSlave);
            ivSlave.ImageMaster = ivMaster;

            //Screen s1 = Screen.AllScreens[0];
            //Screen s2 = Screen.AllScreens[1];

            Rectangle r1 = Display_1.WorkingArea;
            Rectangle r2 = Display_2.WorkingArea;

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
            WriteInput();
        }

        #endregion

        #region Properties

        public bool MachineTypePlanar { get; set; }

        public bool MachineTypePluraview { get; set; }

        public List<Screen> AllDisplay { get; set; }

        public Screen Display_1 { get; set; }

        public Screen Display_2 { get; set; }

        #endregion
    }
}
