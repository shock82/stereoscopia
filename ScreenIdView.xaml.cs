using System.Windows;

namespace Stereoscopia
{
    /// <summary>
    /// Logica di interazione per ScreenIdView.xaml
    /// </summary>
    public partial class ScreenIdView : Window
    {
        public ScreenIdView(string content)
        {
            InitializeComponent();
            LabelContent = content;
            this.DataContext = this;
        }

        public string LabelContent { get; set; }
    }
}