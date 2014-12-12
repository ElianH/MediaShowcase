using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MediaShowcase
{
    public class MovieLoader
    {
        public ObservableCollection<Movie> Movies { get; set; }
        public static string Root;
        public static string MetadataFolder = "metadata";
        public static string ConfigurationFileName = "config.xml";

        private string FullMetadataPath
        {
            get
            {
                return Path.Combine(Root, MetadataFolder);
            }
        }

        private string ConfigurationFilePath
        {
            get
            {
                return Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
            }
        }

        private bool MetadataDirectoryExists
        {
            get
            {
                return Directory.Exists(FullMetadataPath);
            }
        }

        public MovieLoader()
        {
            Movies = new ObservableCollection<Movie>();
        }

        public WebClient GetNewWebClient()
        {
            WebClient w = new WebClient();
            w.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
            w.Headers.Add("Accept", "*/*");
            w.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
            w.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            return w;
        }

        public Configuration LoadConfiguration()
        {
            if (File.Exists(ConfigurationFilePath))
            {
                var x = new XmlSerializer(typeof(Configuration));
                var r = new StreamReader(ConfigurationFilePath);
                var configuration = (Configuration)x.Deserialize(r);
                r.Close();
                return configuration;
            }
            return null;
        }

        public void SaveConfiguration(Configuration config)
        {
            var x = new XmlSerializer(typeof(Configuration));
            var w = new StreamWriter(ConfigurationFilePath);
            x.Serialize(w, config);
            w.Close();
        }

        public void LoadMovies(bool useConfigurationFile)
        {
            Configuration config = LoadConfiguration();
            if (useConfigurationFile && config != null)
            {
                MovieLoader.Root = config.DefaultMoviesDirectory;
            }
            else
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Select movies directory";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    MessageBox.Show("Invalid Movies Directory");
                    return;
                }
                else
                {
                    MovieLoader.Root = dialog.SelectedPath;
                    if (config == null)
                    {
                        config = new Configuration();
                    }
                    config.DefaultMoviesDirectory = MakeRelativePath(System.AppDomain.CurrentDomain.BaseDirectory, dialog.SelectedPath); 
                    SaveConfiguration(config);
                }
            }

            Movies.Clear();
            LoadMoviesFromMetadata();
            LoadMoviesFromRoot();
            int reIndex = 0;
            foreach (var m in Movies)
            {
                m.Index = reIndex++;
                m.Title = StripStrangeCharacters(m.Title);
                m.Plot = StripStrangeCharacters(m.Plot);
            }
        }

        public String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        private void LoadMoviesFromMetadata()
        {
            if (!MetadataDirectoryExists)
            {
                MessageBox.Show("No metadata saved.");
            }
            else
            {
                foreach (var xmlFile in Directory.GetFiles(FullMetadataPath))
                {
                    var x = new XmlSerializer(typeof(Movie));
                    var r = new StreamReader(xmlFile);
                    var m = (Movie)x.Deserialize(r);
                    r.Close();
                    if (Directory.Exists(Path.Combine(Root, m.DirectoryName)))
                    {
                        m.PropertyChanged += new PropertyChangedEventHandler(m_PropertyChanged);
                        Movies.Add(m);
                    }
                }
            }
        }

        private void LoadMoviesFromRoot()
        {
            string[] separators = { ".", "[eng]", "[", "]", "axxo", "dvdrip", "-", "ac3", "2006", "2007", "2008", "2009" };

            if (Directory.Exists(Root))
            {
                string[] directories = Directory.GetDirectories(Root);
                foreach (var fullDirectoryName in directories)
                {
                    var directory = fullDirectoryName.Remove(0, Root.Length + 1);
                    if (directory != MetadataFolder && !Movies.Any(a => a.DirectoryName == directory))
                    {
                        string[] words = directory.ToLowerInvariant().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        var movieTitle = string.Join(" ", words);
                        var m = new Movie();
                        m.DirectoryName = directory;
                        m.Title = movieTitle;
                        m.PropertyChanged += new PropertyChangedEventHandler(m_PropertyChanged);
                        Movies.Add(m);
                    }
                }
            }
        }

        private string StripStrangeCharacters(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Replace("&#x26;", "&");
                text = text.Replace("&#x27;", "'");
                text = text.Replace("&#x22;", @"""");
                text = text.Replace("&#xE9;", @"é");
            }
            return text;
        }

        void m_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ImdbUrl")
            {
                Movie m = sender as Movie;
                TryLoadDataFromImdb(m);
            }
        }

        private void TryLoadDataFromImdb(Movie m)
        {
            Task.Factory.StartNew((Action)delegate
            {
                if (!string.IsNullOrWhiteSpace(m.ImdbUrl))
                {
                    WebClient webClient = GetNewWebClient();
                    var imdbHtml = webClient.DownloadString(m.ImdbUrl);

                    m.Plot = "LOADING...";
                    m.PosterUrl = "LOADING...";

                    try
                    {
                        var startText = @"<p itemprop=""description"">";
                        var endText = @"</p>";
                        var startIndex = imdbHtml.IndexOf(startText) + startText.Length;
                        var endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var plot = imdbHtml.Substring(startIndex, endIndex - startIndex);
                        plot = plot.Replace("\n", "");
                        m.Plot = plot;

                        startText = @"<meta property=""og:title"" content=""";
                        endText = @"""";
                        startIndex = imdbHtml.IndexOf(startText) + startText.Length;
                        endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var title = imdbHtml.Substring(startIndex, endIndex - startIndex);
                        title = title.Replace("\n", "");
                        m.Title = title;

                        startText = @"itemprop=""genre""";
                        endText = @"<";
                        endIndex = 0;
                        m.Genres.Clear();
                        while(startIndex >= 0)
                        {                            
                            startIndex = imdbHtml.IndexOf(startText, endIndex) + startText.Length;
                            if (startIndex < endIndex)
                            {
                                break;
                            }
                            startIndex = imdbHtml.IndexOf(">", startIndex) + 1;
                            if (startIndex > 0)
                            {
                                endIndex = imdbHtml.IndexOf(endText, startIndex);
                                var genre = imdbHtml.Substring(startIndex, endIndex - startIndex);
                                if (genre.Length < 100)
                                {
                                    m.Genres.Add(genre);
                                }
                                else
                                {
                                    startIndex = -1;
                                }
                            }
                        }

                        startText = @"<span itemprop=""ratingValue"">";
                        endText = @"<";
                        startIndex = imdbHtml.IndexOf(startText) + startText.Length;
                        endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var ratingValue = imdbHtml.Substring(startIndex, endIndex - startIndex);
                        m.Rating = double.Parse(ratingValue);

                        startText = @"itemprop=""director""";
                        endText = @"<";
                        startIndex = imdbHtml.IndexOf(startText) + startText.Length;
                        startIndex = imdbHtml.IndexOf(">", startIndex) + 1;
                        endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var director = imdbHtml.Substring(startIndex, endIndex - startIndex);
                        m.Director = director;

                        startText = @"itemprop=""duration""";
                        endText = @"<";
                        startIndex = imdbHtml.IndexOf(startText) + startText.Length;
                        startIndex = imdbHtml.IndexOf(">", startIndex) + 1;
                        endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var duration = imdbHtml.Substring(startIndex, endIndex - startIndex);
                        m.Length = duration;

                        startText = @"itemprop=""actors""";
                        endText = @"<";
                        endIndex = 0;
                        m.Cast.Clear();
                        while (startIndex >= 0)
                        {
                            startIndex = imdbHtml.IndexOf(startText, endIndex) + startText.Length;
                            if (startIndex < endIndex)
                            {
                                break;
                            }
                            startIndex = imdbHtml.IndexOf(">", startIndex) + 1;
                            if (startIndex > 0)
                            {
                                endIndex = imdbHtml.IndexOf(endText, startIndex);
                                var actor = imdbHtml.Substring(startIndex, endIndex - startIndex);
                                if (actor.Length < 100)
                                {
                                    m.Cast.Add(actor);
                                }
                                else
                                {
                                    startIndex = -1;
                                }
                            }
                        }

                        startText = @"id=""img_primary""";
                        startIndex = imdbHtml.IndexOf(startText);
                        startText = @"href=""";
                        endText = @"""";
                        startIndex = imdbHtml.IndexOf(startText, startIndex) + startText.Length;
                        endIndex = imdbHtml.IndexOf(endText, startIndex);
                        var imgLink = "http://www.imdb.com" + imdbHtml.Substring(startIndex, endIndex - startIndex);
                        var imdbImgHtml = webClient.DownloadString(imgLink);
                        startText = @"id=""primary-img""";
                        startIndex = imdbImgHtml.IndexOf(startText);
                        startText = @"src=""";
                        endText = @"""";
                        startIndex = imdbImgHtml.IndexOf(startText, startIndex) + startText.Length;
                        endIndex = imdbImgHtml.IndexOf(endText, startIndex);
                        var posterUrl = imdbImgHtml.Substring(startIndex, endIndex - startIndex);
                        m.PosterUrl = posterUrl;
                    }
                    catch
                    {
                        // do nothing...
                    }

                    // Persist object
                    SaveMovie(m);
                }
            });
        }

        public void SaveAllMovies()
        {
            if (MetadataDirectoryExists)
            {
                Directory.Delete(FullMetadataPath, true);
            }
            Directory.CreateDirectory(FullMetadataPath);

            foreach (var m in Movies)
            {
                SaveMovie(m);
            }
        }

        private void SaveMovie(Movie m)
        {
            if (!MetadataDirectoryExists)
            {
                Directory.CreateDirectory(FullMetadataPath);
            }

            var x = new XmlSerializer(typeof(Movie));
            var w = new StreamWriter(Path.Combine(FullMetadataPath, m.DirectoryName + ".xml"));
            x.Serialize(w, m);
            w.Close();
        }


    }
}

