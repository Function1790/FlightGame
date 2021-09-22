using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightGame
{
    /// <summary>
    /// roundButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class roundButton : UserControl
    {
        public roundButton()
        {
            InitializeComponent();
        }

        public double Size
        {
            get
            {
                return Width;
            }
            set
            {
                rect.Width = value;
                rect.Height = value;
                Width = value;
                Height = value;
                rect.RadiusX = value * 2;
                rect.RadiusY = value * 2;
            }
        }

        void ColorTo(Rectangle get, Color to, double time)
        {
            ColorAnimation ca = new ColorAnimation();
            ca.From = (get.Fill as SolidColorBrush).Color;
            ca.To = to;
            ca.Duration = TimeSpan.FromSeconds(time);
            get.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorTo((sender as Rectangle), Color.FromArgb(50, 255, 255, 255), 0.15);
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorTo((sender as Rectangle), Color.FromArgb(20, 255, 255, 255), 0.15);
        }
    }
}
