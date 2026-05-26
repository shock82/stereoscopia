using Stereoscopia_2.Utility;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Stereoscopia_2.Views
{
    public partial class ViewerWindow : Window
    {
        private readonly SharedTransformState _shared = SharedTransformState.Instance;
        private readonly bool _isFlippedMonitor;

        // _localMatrix contiene TUTTO lo stato visivo corrente di questo viewer.
        // Non viene mai ricostruita da parametri: ogni operazione la modifica direttamente.
        private Matrix _localMatrix = Matrix.Identity;

        // Flip separati perché ScaleAt(-1,1,cx,cy) non è commutativo con le altre op
        private bool _flipH = false;
        private bool _flipV = false;

        // Matrice di partenza (fit-to-viewport), usata per il reset
        private Matrix _resetMatrix = Matrix.Identity;

        // Pan
        private System.Windows.Point _panStart;
        private bool _isPanning;

        // Mirino
        private Line _crossH, _crossV;
        private Ellipse _crossCircle;

        private double _srcW, _srcH;
        private bool _ready = false;

        public ViewerWindow(string imagePath, string label, bool isFlippedMonitor)
        {
            InitializeComponent();
            _isFlippedMonitor = isFlippedMonitor;
            TitleLabel.Text = label;
            MainImage.Source = ImageLoader.Load(imagePath);

            if (isFlippedMonitor) { _flipH = true; _flipV = true; }

            BuildCrosshair();

            // Le operazioni sync vengono applicate direttamente alla _localMatrix
            // di QUESTO viewer quando arriva la notifica
            _shared.PropertyChanged += OnSharedChanged;

            // Init DOPO che la finestra è massimizzata e le dimensioni sono definitive
            ContentRendered += (s, e) => TryInit();
        }

        // ── INIT ──────────────────────────────────────────────────────────────

        private void ImageContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCrosshair(e.NewSize.Width, e.NewSize.Height);
            //TryInit();
        }

        private void TryInit()
        {
            //if (_ready) return;
            if (MainImage.Source is not System.Windows.Media.Imaging.BitmapSource bmp) return;

            double w = ImageContainer.ActualWidth;
            double h = ImageContainer.ActualHeight;
            if (w <= 0 || h <= 0) return;

            _srcW = bmp.PixelWidth;
            _srcH = bmp.PixelHeight;

            // Scala per adattare l'immagine al viewport mantenendo le proporzioni
            double scale = Math.Min(w / _srcW, h / _srcH);

            // Matrice iniziale: scala + centra nel viewport
            _localMatrix = Matrix.Identity;
            //_localMatrix.Scale(scale, scale);
            _localMatrix.Translate((w - _srcW * scale) / 2.0, (h - _srcH * scale) / 2.0);

            _resetMatrix = _localMatrix; // salva per il reset

            Apply();
            _ready = true;
        }

        // ── APPLICA MATRICE ───────────────────────────────────────────────────

        private void Apply() => ImgMatrix.Matrix = _localMatrix;

        // ── OPERAZIONI SULLA MATRICE ──────────────────────────────────────────

        // Il pivot di tutte le operazioni è il centro del viewport
        private System.Windows.Point ViewportCenter =>
            new System.Windows.Point(ImageContainer.ActualWidth / 2.0, ImageContainer.ActualHeight / 2.0);

        private void DoZoom(double factor)
        {
            var c = ViewportCenter;
            _localMatrix.ScaleAt(factor, factor, c.X, c.Y);
            Apply();
        }

        private void DoRotate(double degrees)
        {
            var c = ViewportCenter;
            _localMatrix.RotateAt(degrees, c.X, c.Y);
            Apply();
        }

        private void DoFlipH()
        {
            _flipH = !_flipH;
            var c = ViewportCenter;
            _localMatrix.ScaleAt(-1, 1, c.X, c.Y);
            Apply();
        }

        private void DoFlipV()
        {
            _flipV = !_flipV;
            var c = ViewportCenter;
            _localMatrix.ScaleAt(1, -1, c.X, c.Y);
            Apply();
        }

        private void DoReset()
        {
            _localMatrix = _resetMatrix;
            // Riapplica flip del monitor ruotato
            _flipH = _isFlippedMonitor;
            _flipV = _isFlippedMonitor;
            if (_flipH) { var c = ViewportCenter; _localMatrix.ScaleAt(-1, 1, c.X, c.Y); }
            if (_flipV) { var c = ViewportCenter; _localMatrix.ScaleAt(1, -1, c.X, c.Y); }
            Apply();
        }

        // ── GESTIONE EVENTI SHARED ────────────────────────────────────────────
        //
        // Invece di "ricostruire" la matrice da parametri shared ad ogni Redraw,
        // teniamo traccia dell'ultimo valore applicato e applichiamo solo il DELTA.
        // Così le operazioni sync si comportano esattamente come quelle locali.

        private double _appliedSharedScale = 1.0;
        private double _appliedSharedRotation = 0.0;
        private double _appliedSharedTX = 0.0;
        private double _appliedSharedTY = 0.0;
        private bool _appliedSharedFlipH = false;
        private bool _appliedSharedFlipV = false;

        private void OnSharedChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!_ready) return;
                var c = ViewportCenter;

                // Scale: applica il delta rispetto all'ultimo valore applicato
                if (Math.Abs(_shared.Scale - _appliedSharedScale) > 1e-9)
                {
                    double factor = _shared.Scale / _appliedSharedScale;
                    _localMatrix.ScaleAt(factor, factor, c.X, c.Y);
                    _appliedSharedScale = _shared.Scale;
                }

                // Rotation: applica il delta
                if (Math.Abs(_shared.Rotation - _appliedSharedRotation) > 1e-9)
                {
                    double delta = _shared.Rotation - _appliedSharedRotation;
                    _localMatrix.RotateAt(delta, c.X, c.Y);
                    _appliedSharedRotation = _shared.Rotation;
                }

                // Translate: applica il delta
                double dtx = _shared.TranslateX - _appliedSharedTX;
                double dty = _shared.TranslateY - _appliedSharedTY;
                if (Math.Abs(dtx) > 1e-9 || Math.Abs(dty) > 1e-9)
                {
                    _localMatrix.Translate(dtx, dty);
                    _appliedSharedTX = _shared.TranslateX;
                    _appliedSharedTY = _shared.TranslateY;
                }

                // FlipH: se cambiato, applica
                if (_shared.FlipH != _appliedSharedFlipH)
                {
                    _localMatrix.ScaleAt(-1, 1, c.X, c.Y);
                    _appliedSharedFlipH = _shared.FlipH;
                }

                // FlipV: se cambiato, applica
                if (_shared.FlipV != _appliedSharedFlipV)
                {
                    _localMatrix.ScaleAt(1, -1, c.X, c.Y);
                    _appliedSharedFlipV = _shared.FlipV;
                }

                Apply();
            });
        }

        // ── COMANDI TOOLBAR ───────────────────────────────────────────────────

        private void Cmd_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button)?.Tag?.ToString() ?? "";
            bool isSync = tag.StartsWith("sync_");
            string cmd = tag.Replace("sync_", "").Replace("single_", "");

            if (isSync)
            {
                // Le operazioni sync modificano SharedTransformState;
                // OnSharedChanged le applica alla matrice locale via delta
                switch (cmd)
                {
                    case "rotL": _shared.Rotation -= 90; break;
                    case "rotR": _shared.Rotation += 90; break;
                    case "flipH": _shared.FlipH = !_shared.FlipH; break;
                    case "flipV": _shared.FlipV = !_shared.FlipV; break;
                    case "zoomI": _shared.Scale *= 1.25; break;
                    case "zoomO": _shared.Scale = Math.Max(0.01, _shared.Scale / 1.25); break;
                    case "reset":
                        // Reset sync: annulla i delta applicati
                        // Invertiamo ciò che era stato applicato
                        var c = ViewportCenter;

                        if (Math.Abs(_appliedSharedScale - 1.0) > 1e-9)
                            _localMatrix.ScaleAt(1.0 / _appliedSharedScale, 1.0 / _appliedSharedScale, c.X, c.Y);

                        if (Math.Abs(_appliedSharedRotation) > 1e-9)
                            _localMatrix.RotateAt(-_appliedSharedRotation, c.X, c.Y);

                        if (Math.Abs(_appliedSharedTX) > 1e-9 || Math.Abs(_appliedSharedTY) > 1e-9)
                            _localMatrix.Translate(-_appliedSharedTX, -_appliedSharedTY);

                        if (_appliedSharedFlipH) _localMatrix.ScaleAt(-1, 1, c.X, c.Y);
                        if (_appliedSharedFlipV) _localMatrix.ScaleAt(1, -1, c.X, c.Y);

                        _appliedSharedScale = 1.0;
                        _appliedSharedRotation = 0.0;
                        _appliedSharedTX = 0.0;
                        _appliedSharedTY = 0.0;
                        _appliedSharedFlipH = false;
                        _appliedSharedFlipV = false;

                        // Aggiorna anche SharedTransformState senza notificare (siamo già sincronizzati)
                        _shared.PropertyChanged -= OnSharedChanged;
                        _shared.Scale = 1;
                        _shared.Rotation = 0;
                        _shared.TranslateX = 0;
                        _shared.TranslateY = 0;
                        _shared.FlipH = false;
                        _shared.FlipV = false;
                        _shared.PropertyChanged += OnSharedChanged;

                        Apply();
                        return;
                }
            }
            else
            {
                switch (cmd)
                {
                    case "rotL": DoRotate(-90); break;
                    case "rotR": DoRotate(90); break;
                    case "flipH": DoFlipH(); break;
                    case "flipV": DoFlipV(); break;
                    case "zoomI": DoZoom(1.25); break;
                    case "zoomO": DoZoom(1.0 / 1.25); break;
                    case "reset": DoReset(); break;
                }
            }
        }

        // ── MOUSE ─────────────────────────────────────────────────────────────

        private void ImageContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            DoZoom(e.Delta > 0 ? 1.1 : 1.0 / 1.1);
            e.Handled = true;
        }

        private void ImageContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _panStart = e.GetPosition(ImageContainer);
            _isPanning = true;
            ImageContainer.CaptureMouse();
        }

        private void ImageContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            ImageContainer.ReleaseMouseCapture();
        }

        private void ImageContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isPanning) return;
            var pos = e.GetPosition(ImageContainer);
            _localMatrix.Translate(pos.X - _panStart.X, pos.Y - _panStart.Y);
            _panStart = pos;
            Apply();
        }

        // ── MIRINO ────────────────────────────────────────────────────────────

        private void BuildCrosshair()
        {
            var dash = new DoubleCollection { 6, 4 };
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 233, 69, 96));
            var brushC = new SolidColorBrush(System.Windows.Media.Color.FromArgb(140, 233, 69, 96));

            _crossH = new Line { Stroke = brush, StrokeThickness = 1, StrokeDashArray = dash };
            _crossV = new Line { Stroke = brush, StrokeThickness = 1, StrokeDashArray = dash };
            _crossCircle = new Ellipse
            {
                Width = 14,
                Height = 14,
                Stroke = brushC,
                StrokeThickness = 1.5,
                Fill = System.Windows.Media.Brushes.Transparent
            };
            CrosshairCanvas.Children.Add(_crossH);
            CrosshairCanvas.Children.Add(_crossV);
            CrosshairCanvas.Children.Add(_crossCircle);
        }

        private void UpdateCrosshair(double w, double h)
        {
            if (_crossH == null) return;
            double cx = w / 2, cy = h / 2;
            _crossH.X1 = 0; _crossH.X2 = w; _crossH.Y1 = cy; _crossH.Y2 = cy;
            _crossV.X1 = cx; _crossV.X2 = cx; _crossV.Y1 = 0; _crossV.Y2 = h;
            Canvas.SetLeft(_crossCircle, cx - 7);
            Canvas.SetTop(_crossCircle, cy - 7);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}