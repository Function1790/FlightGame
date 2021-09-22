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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightGame
{
    /// <summary>
    /// Bullet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Bullet : UserControl
    {
        public int speed = 5;
        public int n = 0;
        public double dmg = 1;
        public int type = 0;

        public Bullet(int n, int speed, double dmg)
        {
            InitializeComponent();
            this.n = n;
            this.speed = speed;
            this.dmg = dmg;
        }
        
        public Bullet(int n, int speed, double dmg, int t)
        {
            InitializeComponent();
            rect2.Visibility = Visibility.Hidden;
            this.n = n;
            this.speed = speed;
            this.dmg = dmg;
            type = t;
            switch (t)
            {
                case 1:
                    rect.Visibility = Visibility.Hidden;
                    rect2.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void Up()
        {
            Margin = new Thickness(Margin.Left, Margin.Top - speed, 0, 0);
        }
    }
}
