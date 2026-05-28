using Stereoscopia_2.Views;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Threading;

namespace Stereoscopia_2
{
    public partial class MainWindow : Window
    {
        #region Fields

        private string _leftPath, _rightPath;
        private static List<ScreenIdView> _screenIdentifier;

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

        #region Events

        private void BtnSelectLeft_Click(object sender, RoutedEventArgs e)
        {
            _leftPath = PickFile();
            if (_leftPath != null) TxtLeft.Text = System.IO.Path.GetFileName(_leftPath);
            UpdateStart();
        }

        private void BtnSelectRight_Click(object sender, RoutedEventArgs e)
        {
            _rightPath = PickFile();
            if (_rightPath != null) TxtRight.Text = System.IO.Path.GetFileName(_rightPath);
            UpdateStart();
        }

        private void UpdateStart() =>
            BtnStart.IsEnabled = _leftPath != null && _rightPath != null;

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var screens = Screen.AllScreens;
            // Ordina per posizione X così screen[0] è sempre quello più a sinistra
            Array.Sort(screens, (a, b) => a.Bounds.X.CompareTo(b.Bounds.X));

            var left = new ViewerWindow(_leftPath, "SINISTRA", isFlippedMonitor: false);
            var right = new ViewerWindow(_rightPath, "DESTRA", isFlippedMonitor: screens.Length > 1);

            // Collegamento peer
            left.Peer = right;
            right.Peer = left;

            // Imposta posizione PRIMA di Show(), senza Maximized
            PlaceOnScreen(left, screens[0]);
            PlaceOnScreen(right, screens.Length > 1 ? screens[1] : screens[0]);

            left.Show();
            right.Show();

            // Maximized DOPO Show(), altrimenti Windows ignora Left/Top
            left.WindowState = WindowState.Maximized;
            right.WindowState = WindowState.Maximized;

            left.InitDrawingTools();
            // Toolbar solo sul viewer sinistro
            left.InitAsMain();
            WriteInput();
            this.Hide();
        }

        private void btnIdentifica_Click(object sender, RoutedEventArgs e)
        {
            _screenIdentifier = new List<ScreenIdView>();
            foreach (var screen in Screen.AllScreens)
            {
                Rectangle r1 = screen.WorkingArea;
                ScreenIdView window = new ScreenIdView(screen.DeviceName.Replace(@"\\.\", ""));

                window.Top = r1.Top;
                window.Left = r1.Left;
                window.Width = r1.Width;
                window.Height = r1.Height;
                window.Show();
                _screenIdentifier.Add(window);
            }
            StartCloseTimer();
        }

        #endregion

        #region Private Methods

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
                                    Display_1 = AllDisplay.FirstOrDefault(op => op.DeviceName.Equals(line));
                                    break;
                                case 1:
                                    Display_2 = AllDisplay.FirstOrDefault(op => op.DeviceName.Equals(line));
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
                Display_1 = AllDisplay.FirstOrDefault(op => op.Primary);//.DeviceName.Contains("1"));
                Display_2 = AllDisplay.FirstOrDefault(op => !op.DeviceName.Contains(Display_1.DeviceName));
            }
        }

        private void WriteInput()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
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

        private string PickFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Immagini|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.jp2;*.j2k;*.jpx|Tutti i file|*.*"
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private static void PlaceOnScreen(Window w, Screen s)
        {
            // Converti da pixel fisici a WPF units (DPI-aware)
            var src = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);
            double dpiX = src?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
            double dpiY = src?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = s.Bounds.Left / dpiX;
            w.Top = s.Bounds.Top / dpiY;
            w.Width = s.Bounds.Width / dpiX;
            w.Height = s.Bounds.Height / dpiY;
        }

        private static void StartCloseTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3d);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private static void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            foreach (var item in _screenIdentifier)
            {
                item.Close();
            }
            //this.myPopup.IsOpen = false;
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