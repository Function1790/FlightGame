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
    /// Monster.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Monster : UserControl
    {
        public bool is_down = true;
        public int speed = 3;
        public int score = 1;
        public int n = 0;
        public double hp = 0;

        public Monster(double hp, int width, int height, int left, int speed, int n, int score)
        {
            InitializeComponent();
            Width = width;
            Height = height;
            int addh = -550;
            Margin = new Thickness(left, addh - height, 0, 0);
            this.speed = speed;
            this.n = n;
            this.hp = hp;
            this.score = score;
        }
        
        public Monster(double hp, int width, int height, int left, int top, bool is_down)
        {
            InitializeComponent();
            Width = width;
            Height = height;
            Margin = new Thickness(left, top, 0, 0);
            this.hp = hp;
            this.is_down = is_down;
        }

        public void Down()
        {
            if (is_down)
            {
                Margin = new Thickness(Margin.Left, Margin.Top + speed, 0, 0);
            }
        }

    }
}
