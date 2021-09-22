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
using System.Windows.Threading;

//--/Develop MEMO/--//
/*
[Skill]
+온실 기체 -> 킬마다 스텟 증가, 일정량 되면 방출
+급발진 -> 그줄 모두 쓸기
[Balance]
[Control]
[Sound]
+평타 효과음 개선
+죽을때, 시작할때 효과음 추가
[Else]
+파티클 개선
+렉 최소화
+시작할때 잡음 제거
*/

namespace FlightGame
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        //--[VARIABLE 변수]--//
        //객체 1
        public static int[] HERO_POSITIONS = new int[] { 34, 171, 307 };
        public static int[] ABSOLUTES = new int[] { 0, 135, 270 };
        public static int[] MONSTER_POSITION = new int[] { -270, 0, 270 };
        public static int Hero_Locate = 0;
        public static int Speed_Y = 15;
        public static int Music_Size = 30;
        public static Random rd = new Random();
        public static bool is_stop = true;
        double dmg = 10;
        int Score = 0;
        int life = 0;
        int[] Tick_Value = new int[] { 0, 0 };
        bool log_mode = true;
        int level_value = 0;
        int level_before = 0;

        //객체 2
        Count[] wait = new Count[] { new Count(), new Count(), new Count() };

        Ability[] Skill = new Ability[] { new Ability(), new Ability(), new Ability(), new Ability() };
        DispatcherTimer Frame = new DispatcherTimer();
        DispatcherTimer Frame2 = new DispatcherTimer();
        DispatcherTimer Frame3 = new DispatcherTimer();
        DispatcherTimer Frame4 = new DispatcherTimer();
        List<Monster> Enemy = new List<Monster>();
        List<Bullet> Ammo = new List<Bullet>();
        List<Particle> Powder = new List<Particle>();

        //객체 3
        MediaPlayer music_bg = new MediaPlayer();
        Music music_missile = new Music("preview.mp3", 0.05);
        Music music_base = new Music("basic.wav",0.02);
        Music music_improve = new Music("improve.wav",0.05);
        Music music_lvlup = new Music("lvl_up.wav",0.07);
        Music music_killM = new Music("killM.wav",0.08);
        Music music_hit = new Music("hit.mp3",0.05);
        Music music_explosion = new Music("explosion.mp3", 0.03);

        //--[CLASS 클래스]--//
        //지연 클래스
        class Count
        {
            public double tick = -1;
            public DispatcherTimer timer = new DispatcherTimer();

            public Count()
            {
                timer.Interval = TimeSpan.FromSeconds(0.01);
                timer.Tick += new EventHandler(Tick);
            }

            void Tick(object sender, EventArgs e)
            {
                if (tick < 0)
                {
                    timer.Stop();
                }
                tick--;
            }

            public void Wait(double tick)
            {
                this.tick = tick;
                timer.Start();
            }
        }
        //음악 클래스
        class Music
        {
            public MediaPlayer[] player = new MediaPlayer[Music_Size];
            public int n = 0;
            public double vol = 0;

            public Music(string uri, double vol)
            {
                Set(uri, vol);
            }
            
            public void Set(string uri, double vol)
            {
                this.vol = vol;
                for (int i = 0; i < player.Length; i++)
                {
                    player[i] = new MediaPlayer();
                    player[i].Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\Musics\" + uri)); 
                    player[i].Volume = 0;
                }
            }

            public void Play()
            {
                player[n].Stop();
                player[n].Volume = vol;
                player[n].Play();
                n++;
                if (n >= player.Length)
                {
                    n = 0;
                }
            }
        }
        //스킬 클래스
        class Ability
        {
            public double cool = 0;
            public double tick = -1;
            public string name = "";

            public Ability() { }

            public void Set(string name, double tick, double cool)
            {
                this.cool = cool;
                this.tick = tick;
                this.name = name;
            }
        }
        //파티클 클래스
        class Particle
        {
            double value = 0;
            public List<Rectangle> dust = new List<Rectangle>();
            List<Point> angle = new List<Point>();
            DispatcherTimer timer = new DispatcherTimer();
            public bool stop = false;

            public Particle(Grid world, Thickness pos, Color c, int minS, int maxS, int minR, int maxR, int minRm, int maxRm)
            {
                timer.Interval = TimeSpan.FromSeconds(0.1);
                timer.Tick += new EventHandler(Tick);
                for (int i = 0; i < rd.Next(minS, maxS); i++)
                {
                    dust.Add(new Rectangle());
                    double r = rd.Next(minR, maxR);
                    dust[dust.Count - 1].Margin = new Thickness(pos.Left, pos.Top, 0, 0);
                    dust[dust.Count - 1].Width = r;
                    dust[dust.Count - 1].Height = r;
                    dust[dust.Count - 1].Fill = new SolidColorBrush(c);
                    dust[dust.Count - 1].Opacity = rd.Next(5, 100) * 0.01;
                    int[] a = new int[] { 1, 1 };
                    if (rd.Next(0, 2) == 1)
                    {
                        a[0] = -1;
                    }
                    if (rd.Next(0, 2) == 1)
                    {
                        a[1] = -1;
                    }
                    angle.Add(new Point(rd.Next(minRm, maxRm) * a[0], rd.Next(minRm, maxRm) * a[1]));
                    world.Children.Add(dust[dust.Count - 1]);
                }
            }

            void Tick(object sender, EventArgs e)
            {
                if (value > 12)
                {
                    stop = true;
                    timer.Stop();
                }
                value++;
            }

            public void Start()
            {
                for (int i = 0; i < dust.Count; i++)
                {
                    double dur = rd.Next(1, 12) * 0.1;
                    ThicknessAnimation an = new ThicknessAnimation();
                    an.From = dust[i].Margin;
                    an.To = new Thickness(dust[i].Margin.Left + angle[i].X, dust[i].Margin.Top + angle[i].Y, 0, 0);
                    an.Duration = new Duration(TimeSpan.FromSeconds(dur));
                    dust[i].BeginAnimation(MarginProperty, an);
                    opacAt(dust[i], 0, dur);
                }
            }
        }//

        //--[FUNCTION 함수]--//
        //onLoad
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("onLoad", "Entry");
            TimeSpan cool = TimeSpan.FromSeconds(0.01);
            closeBtn.Opacity = 0.5;
            Frame.Stop();
            Frame2.Stop();
            Frame3.Stop();
            Frame.Interval = cool;
            Frame.Tick += new EventHandler(Frame_Tick);
            Frame2.Interval = cool;
            Frame2.Tick += new EventHandler(Frame2_Tick);
            Frame3.Interval = cool;
            Frame3.Tick += new EventHandler(Frame3_Tick);
            music_bg = setMusic("main.mp3");
            music_bg.Volume = 0.1;
            music_bg.Play();
            opacAt(MainBG, 1, 0.8, 1);
            MainGrid.Margin = new Thickness(0, 15, 0, 0);
            Log("onLoad", "Complete");
        }
        //각 좌표값 반환
        Point[] getPoints(FrameworkElement f)
        {
            //1┌┐2
            //3└┘4
            double l = f.Margin.Left;
            double t = f.Margin.Top;
            double w = f.Width;
            double h = f.Height;
            return new Point[] { new Point(l, t), new Point(w, h) };
        }
        //맞은 여부 반환
        bool is_hit(Point[] p1, Point[] p2, int p1n, int p2n)
        {
            //bool r = p1[0].X + p1[1].X >= p2[0].X;
            //bool l = p1[0].X <= p2[0].X + p2[1].X;
            bool t = p1[0].Y <= p2[0].Y + p2[1].Y;
            bool b = p1[0].Y + p1[1].Y <= p2[0].Y;
            //Log("Hit",$"↓<{t}>\t↑<{b}>");
            return p1n == p2n && t && b;
        }
        //+-[SHOT]-+//
        //기본
        void Shot(double damage)
        {
            Ammo.Add(new Bullet(Hero_Locate, 25, damage));
            Ammo[Ammo.Count - 1].Width = 15;
            Ammo[Ammo.Count - 1].Height = 15;
            Ammo[Ammo.Count - 1].Margin = new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top, 0, 0);
            //Play_Particle(Color.FromRgb(160, 255, 246), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top - 75, 0, 0));
            World.Children.Add(Ammo[Ammo.Count - 1]);
        }
        //타입
        void Shot(double damage, int size, int t)
        {
            Ammo.Add(new Bullet(Hero_Locate, 25, damage, t));
            Ammo[Ammo.Count - 1].Width = size;
            Ammo[Ammo.Count - 1].Height = size;
            Ammo[Ammo.Count - 1].Margin = new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top, 0, 0);
            //Play_Particle(Color.FromRgb(160, 255, 246), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top - 75, 0, 0));
            World.Children.Add(Ammo[Ammo.Count - 1]);
        }
        //색깔 및 둥글기
        void Shot(double damage, int Size, Color c, double radius)
        {
            Ammo.Add(new Bullet(Hero_Locate, 25, damage));
            Ammo[Ammo.Count - 1].Width = Size;
            Ammo[Ammo.Count - 1].Height = Size;
            Ammo[Ammo.Count - 1].rect.RadiusX = radius;
            Ammo[Ammo.Count - 1].rect.RadiusY = radius;
            Ammo[Ammo.Count - 1].rect.Fill = new SolidColorBrush(c);
            Ammo[Ammo.Count - 1].Margin = new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top, 0, 0);
            //Play_Particle(Color.FromRgb(160, 255, 246), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top - 75, 0, 0));
            World.Children.Add(Ammo[Ammo.Count - 1]);
        }
        //몬스터 좌표 반환
        Thickness getPos(Monster e)
        {
            return new Thickness(MONSTER_POSITION[e.n], e.Margin.Top, 0, 0);
        }
        //+-[ENGINE]-+//
        //물리 엔진1 - 총알 / 데미지
        void Frame3_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < Enemy.Count; i++)
            {
                if (Skill[2].tick >= 0)
                {
                    if (Enemy[i].n == Hero_Locate)
                    {
                        Enemy[i].hp -= 3;
                        if (Skill[2].tick % 5 == 0)
                        {
                            Play_Particle(Color.FromArgb(50, 255, 232, 216), Enemy[i].Margin, 30, 40);
                        }
                    }
                }
                for (int j = 0; j < Ammo.Count; j++)
                {
                    //[HIT EVENT - AMMO]
                    if (is_hit(getPoints(Ammo[j]), getPoints(Enemy[i]), Ammo[j].n, Enemy[i].n))
                    {
                        if (Ammo[j].type == 0)
                        {
                            music_hit.Play();
                        }
                        if (Ammo[j].type == 1)
                        {
                            music_explosion.Play();
                        }
                        Enemy[i].hp -= Ammo[j].dmg;
                        Play_Particle(Color.FromRgb(250, 255, 105), getPos(Enemy[i]));
                        World.Children.Remove(Ammo[j]);
                        Ammo.RemoveAt(j);
                        break;
                    }
                }
            }
        }
        //물리 엔진2 - 이동
        void Frame2_Tick(object sender, EventArgs e)
        {
            List<int> list = new List<int>();
            Point[] p1 = getPoints(Hero);
            for (int i = 0; i < Enemy.Count; i++)
            {
                if (Enemy[i].is_down == true)
                {
                    Enemy[i].Down();
                    //[HIT EVENT - CRASH]
                    if (is_hit(p1, getPoints(Enemy[i]), Hero_Locate, Enemy[i].n))
                    {
                        addScore(3);
                        life--;
                        setLife();
                        list.Add(i);
                        Log("Hit Event", $"Enemy<{i}> & Hero -> Life<{life}>");
                        World.Children.Remove(Enemy[i]);
                        Play_Particle(Color.FromRgb(240, 255, 205), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top, 0, 0), 20, 50);
                        continue;
                    }
                    if (Enemy[i].Margin.Top > Height + 5)
                    {
                        addScore(2);
                        list.Add(i);
                        continue;
                    }               
                    if (Enemy[i].hp <= 0)
                    {
                        Log("Kill Event", $"Score[+{Enemy[i].score}] Enemy<{i}>");
                        Play_Particle(Color.FromRgb(150, 255, 110), getPos(Enemy[i]), 15, 50);
                        music_killM.Play();
                        addScore(Enemy[i].score);
                        list.Add(i);
                        continue;
                    }
                }
            }
            if (list.Count >= 1)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    World.Children.Remove(Enemy[list[i]]);
                    Enemy.RemoveAt(list[i]);
                }
            }
        }
        //물리 엔진3 - 스킬 / 소환
        void Frame_Tick(object sender, EventArgs e)
        {
            List<int> list2 = new List<int>();

            if (Skill[0].tick >= 0)
            {
                Shot(dmg / 2, 20, Color.FromArgb(50, 239, 77, 255), 50);
                Play_Particle(Color.FromArgb(50, 239, 77, 255), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top - 75, 0, 0), 20, 30);
                music_improve.Play();
            }
            if (Skill[1].tick >= 0)
            {
                Shot(dmg * 2, 25, 1);
                Play_Particle(Color.FromRgb(255, 165, 75), new Thickness(MONSTER_POSITION[Hero_Locate], Hero.Margin.Top - 75, 0, 0), 20, 50);
                music_missile.Play();
            }
            if (Skill[2].tick >= 0)
            {
                RushGrid.Opacity = 1;
            }
            else
            {
                RushGrid.Opacity = 0;
            }
            for (int i = 0; i < Skill.Length; i++)
            {
                if (Skill[i].tick >= 0)
                {
                    Skill[i].tick--;
                }
                if (Skill[i].tick < 0 && Skill[i].cool >= 0)
                {
                    Skill[i].cool--;
                    if (Skill[i].cool < 0)
                    {
                        Log("Ready Event", $"Ready<{Skill[i].name}>");
                        ReadyAlert(Skill[i].name);
                    }
                }
            }

            if (Tick_Value[1] > 20 - Score * 0.0005)
            {
                Shot(dmg);
                music_base.Play();
                Tick_Value[1] = 0;
            }
            for (int i = 0; i < Ammo.Count; i++)
            {
                Ammo[i].Up();
                if (Ammo[i].Margin.Top <= -550)
                {
                    list2.Add(i);
                }
            }
            if (list2.Count > 0)
            {
                for (int i = list2.Count - 1; i >= 0; i--)
                {
                    World.Children.Remove(Ammo[list2[i]]);
                    Ammo.RemoveAt(list2[i]);
                }
            }

            if (Tick_Value[0] > 40)
            {
                Log("Alive Event", "Score[+10] Monster[+1]");
                addScore(1);
                Monster_Type type = Monster_Type.Basic;
                switch (new Random().Next(0, 3))
                {
                    case 0:
                        break;
                    case 1:
                        type = Monster_Type.Big;
                        break;
                    case 2:
                        type = Monster_Type.Small;
                        break;
                }
                Summon_Monster(type);
                Tick_Value[0] = 0;
            }

            if (life < 0)
            {
                Log("Game Over", "Hero Dead");
                Die();
            }
            for (int i = Powder.Count - 1; i >= 0; i--)
            {
                if (Powder[i].stop == true)
                {
                    for (int j = Powder.Count - 1; j >= 0; j--)
                    {
                        World.Children.Remove(Powder[i].dust[j]);
                    }
                    Powder[i].dust.Clear(); ;
                }
            }
            Tick_Value[0]++;
            Tick_Value[1]++;
        }
        //죽음
        void Die()
        {
            is_stop = true;
            Frame.Stop();
            Frame2.Stop();
            Frame3.Stop();
            
            tb_life.Text = "life:-";
            OverScore.Text = tb_score.Text;
            OverGrid.Margin = new Thickness(0, 15, 0, 0);
            leftTo(OverGrid, -Width - 15, 0, 0.5);
            topTo(OverCover, -20, 0, 1);
            opacAt(OverCover, 0, 1, 1.2);
        }
        //로그
        public void Log(string title, object content)
        {
            if (log_mode)
            {
                Console.WriteLine($"[{title}] : {content}");
            }
        }
        //참조
        public MainWindow()
        {
            InitializeComponent();
        }
        //+-[PARTICLE]-+//
        //기본
        void Play_Particle(Color c, Thickness margin)
        {
            Powder.Add(new Particle(World, margin, c, 15, 30, 1, 10, 10, 30));
            Powder[Powder.Count - 1].Start();
        }
        //범위
        void Play_Particle(Color c, Thickness margin,int minRange, int maxRange)
        {
            Powder.Add(new Particle(World, margin, c, 15, 30, 1, 10, minRange, maxRange));
            Powder[Powder.Count - 1].Start();
        }
        //+-[ANIMATION]-+//
        //투명도1
        static void opacAt(FrameworkElement ctr, double to, double sec)
        {
            DoubleAnimation an = new DoubleAnimation();
            an.From = ctr.Opacity;
            an.To = to;
            an.Duration = new Duration(TimeSpan.FromSeconds(sec));
            ctr.BeginAnimation(OpacityProperty, an);
        }
        //투명도2
        void opacAt(FrameworkElement ctr, double from, double to, double sec)
        {
            DoubleAnimation an = new DoubleAnimation();
            an.From = from;
            an.To = to;
            an.Duration = new Duration(TimeSpan.FromSeconds(sec));
            ctr.BeginAnimation(OpacityProperty, an);
        }
        //top
        void topTo(FrameworkElement ctr, double from, double to, double sec)
        {
            ThicknessAnimation an = new ThicknessAnimation();
            an.From = new Thickness(ctr.Margin.Left, from, 0, 0);
            an.To = new Thickness(ctr.Margin.Left, to, 0, 0);
            an.Duration = new Duration(TimeSpan.FromSeconds(sec));
            ctr.BeginAnimation(MarginProperty, an);
        }
        //left
        void leftTo(FrameworkElement ctr, double from, double to, double sec)
        {
            ThicknessAnimation an = new ThicknessAnimation();
            an.From = new Thickness(from, ctr.Margin.Top, 0, 0);
            an.To = new Thickness(to, ctr.Margin.Top, 0, 0);
            an.Duration = new Duration(TimeSpan.FromSeconds(sec));
            ctr.BeginAnimation(MarginProperty, an);
        }
        //size
        void sizeTo(FrameworkElement ctr, double from, double to, double sec)
        {
            DoubleAnimation an = new DoubleAnimation();
            an.From = from;
            an.To = to;
            an.Duration = new Duration(TimeSpan.FromSeconds(sec));
            ctr.BeginAnimation(WidthProperty, an);
            ctr.BeginAnimation(HeightProperty, an);
        }
        
        //Hero 이동
        public void Hero_MoveTo(int value)
        {
            if (wait[0].tick < 0)
            {
                int t = 0;
                int pos = 0;
                if (value >= HERO_POSITIONS.Length)
                {
                    pos = HERO_POSITIONS.Length - 1;
                }
                else if (value < 0)
                {
                    pos = 0;
                }
                else
                {
                    pos = value;
                    t = 15;
                }
                Hero_Locate = pos;
                leftTo(Hero, Hero.Margin.Left, HERO_POSITIONS[pos], 0.1);
                if (Skill[2].tick >= 0)
                {
                    leftTo(RushGrid, RushGrid.Margin.Left, HERO_POSITIONS[pos] - RushGrid.Width / 4, 0.25);
                }
                wait[0].Wait(t);
            }
        }
        //일시 정지
        void Stop()
        {
            is_stop = true;
            Frame.Stop();
            Frame2.Stop();
            Frame3.Stop();
            leftTo(StopGrid, Width + 15, 0, 0.3);
            opacAt(StopGrid, 0, 1, 0.3);
            leftTo(StopMenu, 50, 0, 0.6);
            opacAt(StopMenu, 0, 1, 0.6);
        }
        //계속하기
        void Continue()
        {
            is_stop = false;
            Frame.Start();
            Frame2.Start();
            Frame3.Start();
            leftTo(StopGrid, 0, Width + 15, 0.3);
            opacAt(StopGrid, 1, 0, 0.3);
            leftTo(StopMenu, 0, 50, 0.6);
            opacAt(StopMenu, 1, 0, 0.6);
        }
        //__[GAME START]__//
        void Start()
        {
            Log("Start", "Entry");
            Score = 0;
            life = 3;
            dmg = 15;
            addScore(0);
            setLife();
            World.Children.Clear();
            Enemy.Clear();
            Ammo.Clear();
            Frame.Start();
            Frame2.Start();
            Frame3.Start();
            RushGrid.Opacity = 0;
            Tick_Value[0] = -200;
            is_stop = false;
            BigMessage("stage 0", 1.2);
            Log("Start", "Complete");
        }
        //준비 알림
        public void ReadyAlert(string name)
        {
            tb_ready.Text = name + " Ready";
            double t = 0.75;
            topTo(tb_ready, 67, 47, t);
            opacAt(tb_ready, 1, 0, t);
        }
        //숫자 콤마 넣기 반환
        string valueCut(int value) 
        {
            string a = (value + "");
            int v = 0;
            string result = "";
            for(int i = a.Length - 1; i >= 0; i--)
            {
                result = a[i] + result;
                v++;
                if (v >= 3 && i != 0)
                {
                    result = "," + result;
                    v = 0;
                }
            }
            return result;
        }
        //점수 더하기
        void addScore(int value)
        {
            Score += value;
            tb_add.Text = "+" + valueCut(value);
            opacAt(tb_add, 1, 0, 0.75);
            setScore();
        }
        //점수 텍스트 설정
        void setScore()
        {
            level_value = (int)(Score / 5000);
            if (level_before != level_value)
            {
                Tick_Value[0] = -200;
                BigMessage($"stage {level_value}", 1.2);
                Log("Stage", $"Level_Value<{level_value}>");
                music_lvlup.Play();
                dmg += Score * 0.005;
            }
            tb_score.Text = "score:";
            if (Score >= 100000000)
            {
                tb_score.Text += "HYPER";
            }
            else if (Score > 99999999)
            {
                tb_score.Text += "SUPER";
            }
            else if (Score > 9999999)
            {
                tb_score.Text += "MAX";
            }
            else
            {
                tb_score.Text += valueCut(Score);
            }
            level_before = level_value;
        }
        //남은 생명 텍스트 설정
        void setLife()
        {
            tb_life.Text = "life:" + life;
        }
        //몬스터 타입
        enum Monster_Type
        {
            Small,
            Basic,
            Big
        }
        //몬스터 소환
        void Summon_Monster(Monster_Type type)
        {
            int add_speed = (int)(Score * 0.00005);
            int width = 36;
            int height = 36;
            double hp = 50;
            int score = 10;
            int speed = 7;
            switch (type)
            {
                case Monster_Type.Small:
                    width = rd.Next(20, 26);
                    height = width;
                    speed = rd.Next(8, 12);
                    hp = rd.Next(5, 20);
                    score = 5;
                    break;
                case Monster_Type.Basic:
                    break;
                case Monster_Type.Big:
                    width = rd.Next(45, 51);
                    height = width;  
                    speed = rd.Next(5, 8);
                    hp = rd.Next(60, 80);
                    score = 15;
                    break;
            }
            int n = rd.Next(0, 3);
            Enemy.Add(new Monster(hp + Score * 0.0005, width, height, MONSTER_POSITION[n], speed + add_speed, n, (int)(score + (level_value * 5))));
            Log("Summon Monster", $"Monster<{Enemy.Count}>");
            World.Children.Add(Enemy[Enemy.Count - 1]);
        }
        //음악 설정
        MediaPlayer setMusic(string uri)
        {
            MediaPlayer media = new MediaPlayer();
            media.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory+@"\Musics\"+uri));
            return media;
        }
        //메세지 박스
        void BigMessage(string text, double sec)
        {
            tb_big.Text = text;
            topTo(tb_big, 240, 200, sec);
            opacAt(tb_big, 1, 0, sec);
        }
        //--[EVENT 이벤트]--//
        //닫기
        private void closeBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            opacAt(closeBtn, 1, 0.2);
        }

        private void closeBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            opacAt(closeBtn, 0.5, 0.2);
        }

        private void closeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        //상단바
        private void TopGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        //__[KEY DOWN]__//
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (is_stop == false)
            {
                if (e.Key == Key.A || e.Key == Key.Left)
                {
                    Hero_MoveTo(Hero_Locate - 1);
                }
                if (e.Key == Key.D || e.Key == Key.Right)
                {
                    Hero_MoveTo(Hero_Locate + 1);
                }
                if (e.Key == Key.Z && Skill[0].cool <= 0)
                {
                    Skill[0].Set("Z", 5, 400);
                }
                if (e.Key == Key.X && Skill[1].cool <= 0)
                {
                    Skill[1].Set("X", 0, 350);
                }
                if (e.Key == Key.C && Skill[2].cool <= 0)
                {
                    wait[0].Wait(15);
                    Skill[2].Set("C", 100, 450);
                    leftTo(RushGrid, RushGrid.Margin.Left, HERO_POSITIONS[Hero_Locate] - RushGrid.Width / 4, 0.1);
                }
            }
            if (e.Key == Key.Escape)
            {
                if (is_stop)
                {
                    Continue();
                }
                else
                {
                    Stop();
                }
            }
        }
        //재시작
        private void OverRestart_MouseEnter(object sender, MouseEventArgs e)
        {
            opacAt(sender as FrameworkElement, 1, 0.2);
        }

        private void OverRestart_MouseLeave(object sender, MouseEventArgs e)
        {
            opacAt(sender as FrameworkElement, 0.8, 0.2);
        }

        private void OverRestart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            leftTo(OverGrid, 0, -Width - 15, 0.5);
            topTo(OverCover, 0, -20, 1);
            opacAt(OverCover, 1, 0, 1.2);
            Start();
        }
        //시작
        private void MainStart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            leftTo(MainGrid, MainGrid.Margin.Left, Width + 15, 0.2);
            Start();
        }

        private void StopContinue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Continue();
        }

        private void StopQuit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Continue();
            Die();
        }
    }//
}