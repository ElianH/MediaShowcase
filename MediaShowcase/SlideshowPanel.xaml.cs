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
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Media.Media3D;

namespace MediaShowcase
{
    /// <summary>
    /// Interaction logic for SlideshowPanel.xaml
    /// </summary>
    public partial class SlideshowPanel : UserControl
    {
        public MainWindow MyMainWindow { get; set; }
        private List<MovieItem> MovieItems = new List<MovieItem>();
        private MovieItem LeftMovieItem;
        private MovieItem RightMovieItem;
        private int CurrentIndex = 0;
        private int NumberOfMovies = 0;
        private double ScreenWidth = 0;
        private double ScreenHeight = 0;
        private double MoveWidthMultiplier = 1.5;
        private double ScaleTo = 0.5;

        private bool filtersInitialized = false;
        private bool hasGenreFilter = false;
        private string genreFilter;
        private string genreFilterNoFilter = "All";

        private string sortBy;
        private const string sortByTitle = "Title";
        private const string sortByRating = "Rating";
        private const string sortByYear = "Year";

        public SlideshowPanel()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(SlideshowKeyDown);
        }

        public void SetupMovieItems()
        {
            LeftMovieItem = null;
            RightMovieItem = null;
            CurrentIndex = 0;
            NumberOfMovies = 0;
            var movies = MyMainWindow.MovieLoader.Movies.ToList();

            if (!filtersInitialized)
            {
                var genresList = new List<string>();
                genresList.Add(genreFilterNoFilter);
                genresList.AddRange(movies.SelectMany(a => a.Genres).Distinct().ToList<string>());
                GenreFilterComboBox.ItemsSource = genresList;
                GenreFilterComboBox.SelectedIndex = 0;
                GenreFilterComboBox.SelectionChanged += GenreFilterSelectionChanged;

                var sortByList = new List<string>();
                sortByList.Add(sortByTitle);
                sortByList.Add(sortByRating);
                sortByList.Add(sortByYear);
                SortByComboBox.ItemsSource = sortByList;
                SortByComboBox.SelectedIndex = 0;
                SortByComboBox.SelectionChanged += SortBySelectionChanged;

                filtersInitialized = true;
            }

            MovieItems.Clear();
            MovieItemsGrid.Children.Clear();
            ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            ScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double factor = 0.9;

            var filteredMovies = movies.Where(a => !hasGenreFilter || a.Genres.Contains(genreFilter));

            var sortedAndFilteredMovies = filteredMovies;
            switch (sortBy)
            {
                case sortByTitle:
                    sortedAndFilteredMovies = filteredMovies.OrderBy(a => a.Title);
                    break;
                case sortByRating:
                    sortedAndFilteredMovies = filteredMovies.OrderByDescending(a => a.Rating);
                    break;
                case sortByYear:
                    sortedAndFilteredMovies = filteredMovies.OrderByDescending(a => a.Year);
                    break;
            }

            NumberOfMovies = sortedAndFilteredMovies.Count();
            int count = 0;
            foreach (Movie m in sortedAndFilteredMovies)
            {
                m.Index = count;
                MovieItem movieItem = new MovieItem();
                movieItem.Name = "movieItem" + m.Index;
                movieItem.DataContext = m;
                movieItem.Height = ScreenHeight * factor;
                movieItem.Width = ScreenWidth - (ScreenHeight - movieItem.Height);
                movieItem.Margin = new Thickness(0);
                movieItem.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
                MovieItems.Add(movieItem);
                MovieItemsGrid.Children.Add(movieItem);
                count++;
            }
            UpdateIndexTextBlock();
            MovieItemsGrid.InvalidateVisual();
            MovieItemsGrid.UpdateLayout();
            this.Focus();
        }

        private void CreateAnimation(Storyboard myStoryboard, FrameworkElement itemToMove, 
            double fromX, double toX, double fromScale, double toScale)
        {
            double accelerationRatio = 0.3;
            double duration = 500;

            TranslateTransform translate = new TranslateTransform(0, 0);
            
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            itemToMove.RenderTransformOrigin = new Point(0.5, 0.5);

            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(translate);
            myTransformGroup.Children.Add(scale);
            
            itemToMove.RenderTransform = myTransformGroup;

            DoubleAnimation moveAnimationX = new DoubleAnimation();
            moveAnimationX.Duration = TimeSpan.FromMilliseconds(duration);
            moveAnimationX.AccelerationRatio = accelerationRatio;
            moveAnimationX.From = fromX;
            moveAnimationX.To = toX;
            myStoryboard.Children.Add(moveAnimationX);
            Storyboard.SetTargetProperty(moveAnimationX, new PropertyPath("RenderTransform.Children[0].X"));
            Storyboard.SetTarget(moveAnimationX, itemToMove);

            DoubleAnimation growAnimationX = new DoubleAnimation();
            growAnimationX.Duration = TimeSpan.FromMilliseconds(duration);
            growAnimationX.AccelerationRatio = accelerationRatio;
            growAnimationX.From = fromScale;
            growAnimationX.To = toScale;
            myStoryboard.Children.Add(growAnimationX);
            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.Children[1].ScaleX"));
            Storyboard.SetTarget(growAnimationX, itemToMove);

            DoubleAnimation growAnimationY = new DoubleAnimation();
            growAnimationY.Duration = TimeSpan.FromMilliseconds(duration);
            growAnimationY.AccelerationRatio = accelerationRatio;
            growAnimationY.From = fromScale;
            growAnimationY.To = toScale;
            myStoryboard.Children.Add(growAnimationY);
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.Children[1].ScaleY"));
            Storyboard.SetTarget(growAnimationY, itemToMove);
        }

        void SlideshowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                NextButtonClick(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Left)
            {
                PrevButtonClick(null, null);
                e.Handled = true;
            }
        }

        private void PrevButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex - 1 >= 0)
            {
                Storyboard myStoryboard = new Storyboard();
                double delta = System.Windows.SystemParameters.PrimaryScreenWidth;
                LeftMovieItem = MovieItems[CurrentIndex - 1];
                RightMovieItem = MovieItems[CurrentIndex];
                CreateAnimation(myStoryboard, LeftMovieItem, -ScreenWidth * MoveWidthMultiplier, 0, ScaleTo, 1);
                CreateAnimation(myStoryboard, RightMovieItem, 0, ScreenWidth * MoveWidthMultiplier, 1, ScaleTo);
                myStoryboard.Begin(this);
                CurrentIndex--;
                UpdateIndexTextBlock();
            }
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex + 1 < MovieItems.Count)
            {
                Storyboard myStoryboard = new Storyboard();
                LeftMovieItem = MovieItems[CurrentIndex];
                RightMovieItem = MovieItems[CurrentIndex + 1];
                RightMovieItem.Visibility = System.Windows.Visibility.Visible;
                CreateAnimation(myStoryboard, LeftMovieItem, 0, -ScreenWidth * MoveWidthMultiplier, 1, ScaleTo);
                CreateAnimation(myStoryboard, RightMovieItem, ScreenWidth * MoveWidthMultiplier, 0, ScaleTo, 1);
                myStoryboard.Begin(this);
                CurrentIndex++;
                UpdateIndexTextBlock();
            }
        }

        private void UpdateIndexTextBlock()
        {
            IndexTextBlock.Text = CurrentIndex.ToString() + " / " + NumberOfMovies.ToString();
        }

        public void ShowConfigurationPanel(object sender, RoutedEventArgs e)
        {
            MyMainWindow.ShowConfigurationPanel();
        }

        private void GenreFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            genreFilter = GenreFilterComboBox.SelectedValue.ToString();
            hasGenreFilter = (genreFilter != genreFilterNoFilter);
            SetupMovieItems();
        }

        private void SortBySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sortBy = SortByComboBox.SelectedValue.ToString();
            SetupMovieItems();
        }


        private void WatchMovieButtonClick(object sender, RoutedEventArgs e)
        {
            var m = MovieItems[CurrentIndex].DataContext as Movie;
            var fileNames = new List<string>();
            foreach (var f in Directory.GetFiles(Path.Combine(MovieLoader.Root, m.DirectoryName)))
            {
                FileInfo fi = new FileInfo(f);

                // If not srt file and size greater than 50 mb, then we assume it's a video file.
                if ((string.Compare(fi.Extension, ".srt", true) != 0) && (fi.Length > 50 * 1024 * 1024))
                {
                    fileNames.Add('"' + f + '"');
                }
            }

            try
            {
                // Try running Windows Media Player
                System.Diagnostics.Process.Start("mpc-hc.exe", string.Join(" ", fileNames));
            }
            catch
            {
                MessageBox.Show("There was an error opening Windows Media Player Classic. Is it installed in this computer?");
            }
        }

    }
}
