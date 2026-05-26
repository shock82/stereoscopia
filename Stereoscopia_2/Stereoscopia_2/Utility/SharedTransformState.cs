using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Stereoscopia_2.Utility
{
    public class SharedTransformState : INotifyPropertyChanged
    {
        private static SharedTransformState _instance;
        public static SharedTransformState Instance => _instance ??= new SharedTransformState();

        private double _scale = 1.0;
        private double _rotation = 0.0;
        private double _translateX = 0.0;
        private double _translateY = 0.0;
        private bool _flipH = false;
        private bool _flipV = false;

        public double Scale { get => _scale; set => SetProp(ref _scale, value); }
        public double Rotation { get => _rotation; set => SetProp(ref _rotation, value); }
        public double TranslateX { get => _translateX; set => SetProp(ref _translateX, value); }
        public double TranslateY { get => _translateY; set => SetProp(ref _translateY, value); }
        public bool FlipH { get => _flipH; set => SetProp(ref _flipH, value); }
        public bool FlipV { get => _flipV; set => SetProp(ref _flipV, value); }

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProp<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (!Equals(field, value)) { field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        // Evento per propagare zoom/pan in tempo reale tra i due viewer
        public event Action<double, double> PanDelta;        // dx, dy
        public event Action<double, double, double> ZoomAt;  // factor, cx, cy

        public void RaisePanDelta(double dx, double dy) => PanDelta?.Invoke(dx, dy);
        public void RaiseZoomAt(double f, double cx, double cy) => ZoomAt?.Invoke(f, cx, cy);

        private bool _linked = false;
        public bool Linked
        {
            get => _linked;
            set => SetProp(ref _linked, value);
        }
    }
}