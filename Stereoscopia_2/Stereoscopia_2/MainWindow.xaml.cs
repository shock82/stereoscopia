using Microsoft.Win32;
using Stereoscopia_2.Views;
using System.Windows;
using System.Windows.Forms; // per Screen — aggiungi ref a System.Windows.Forms

namespace Stereoscopia_2
{
    public partial class MainWindow : Window
    {
        private string _leftPath, _rightPath;

        public MainWindow() => InitializeComponent();

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

        private string PickFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Immagini|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.jp2;*.j2k;*.jpx|Tutti i file|*.*"
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
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

            // Imposta posizione PRIMA di Show(), senza Maximized
            PlaceOnScreen(left, screens[0]);
            PlaceOnScreen(right, screens.Length > 1 ? screens[1] : screens[0]);

            left.Show();
            right.Show();

            // Maximized DOPO Show(), altrimenti Windows ignora Left/Top
            left.WindowState = WindowState.Maximized;
            right.WindowState = WindowState.Maximized;

            this.Hide();
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

    }
}