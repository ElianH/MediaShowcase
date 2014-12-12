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
using System.Collections.ObjectModel;

namespace MediaShowcase
{
    /// <summary>
    /// Interaction logic for ConfigurationPanel.xaml
    /// </summary>
    public partial class ConfigurationPanel : UserControl
    {
        public MainWindow MyMainWindow { get; set; }

        public ObservableCollection<Movie> MyMovies
        {
            get { return (ObservableCollection<Movie>)GetValue(MyMoviesProperty); }
            set { SetValue(MyMoviesProperty, value); }
        }
        public static DependencyProperty MyMoviesProperty = DependencyProperty.Register("MyMovies",
            typeof(ObservableCollection<Movie>), typeof(ConfigurationPanel), null);

        public ConfigurationPanel()
        {
            InitializeComponent();
        }

        private void ShowSlideshowButtonClick(object sender, RoutedEventArgs e)
        {
            MyMainWindow.ShowSlideshowPanel();
        }

        private void LoadMoviesButtonClick(object sender, RoutedEventArgs e)
        {
            MyMainWindow.LoadMovies();
        }

        private void SaveMoviesButtonClick(object sender, RoutedEventArgs e)
        {
            MyMainWindow.MovieLoader.SaveAllMovies();
        }

    }
}
