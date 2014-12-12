using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MediaShowcase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MovieLoader MovieLoader { get; set; }
        private bool SetupDone = false;

        public MainWindow()
        {
            MovieLoader = new MovieLoader();
            MovieLoader.LoadMovies(true);
            InitializeComponent();
            
            MyConfigurationPanel.MyMainWindow = this;
            MySlideshowPanel.MyMainWindow = this;

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Activated += new EventHandler(MainWindow_Activated);
            this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStyle = System.Windows.WindowStyle.ThreeDBorderWindow;
                this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            }
        }

        private bool _inStateChange;

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized && !_inStateChange)
            {
                _inStateChange = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _inStateChange = false;
                Activate();
                ShowSlideshowPanel();
            }
            base.OnStateChanged(e);
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (MySlideshowPanel.Visibility == System.Windows.Visibility.Visible)
            {
                MySlideshowPanel.Focus();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyConfigurationPanel.MyMovies = MovieLoader.Movies;
            this.Activate();
            ShowSlideshowPanel();
        }

        public void ShowConfigurationPanel()
        {
            MyConfigurationPanel.Visibility = System.Windows.Visibility.Visible;
            MySlideshowPanel.Visibility = System.Windows.Visibility.Collapsed;
            MyConfigurationPanel.Focus();
        }

        public void ShowSlideshowPanel()
        {
            if (!SetupDone)
            {
                SetupDone = true;
                MySlideshowPanel.SetupMovieItems();
            }
            MyConfigurationPanel.Visibility = System.Windows.Visibility.Collapsed;
            MySlideshowPanel.Visibility = System.Windows.Visibility.Visible;
            MySlideshowPanel.Focus();
        }

        public void LoadMovies()
        {
            MovieLoader.LoadMovies(false);
            SetupDone = false;
        }
    }
}
