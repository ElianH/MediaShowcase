using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace MediaShowcase
{
    [XmlRootAttribute("Movie", Namespace = "", IsNullable = false)]
    public class Movie : INotifyPropertyChanged
    {
        private int _Index;
        public int Index 
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    NotifyPropertyChanged("Index");
                }
            }
        }
        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        private int _Year;
        public int Year
        {
            get
            {
                return _Year;
            }
            set
            {
                if (value != _Year)
                {
                    _Year = value;
                    NotifyPropertyChanged("Year");
                }
            }
        }
        private string _PosterUrl;
        public string PosterUrl
        {
            get
            {
                return _PosterUrl;
            }
            set
            {
                if (value != _PosterUrl)
                {
                    _PosterUrl = value;
                    NotifyPropertyChanged("PosterUrl");
                }
            }
        }
        private string _ImdbUrl;
        public string ImdbUrl
        {
            get
            {
                return _ImdbUrl;
            }
            set
            {
                if (value != _ImdbUrl)
                {
                    _ImdbUrl = value;
                    NotifyPropertyChanged("ImdbUrl");
                }
            }
        }
        private string _DirectoryName;
        public string DirectoryName
        {
            get
            {
                return _DirectoryName;
            }
            set
            {
                if (value != _DirectoryName)
                {
                    _DirectoryName = value;
                    NotifyPropertyChanged("DirectoryName");
                }
            }
        }
        private string _Plot;
        public string Plot
        {
            get
            {
                return _Plot;
            }
            set
            {
                if (value != _Plot)
                {
                    _Plot = value;
                    NotifyPropertyChanged("Plot");
                }
            }
        }
        private double _Rating;
        public double Rating
        {
            get
            {
                return _Rating;
            }
            set
            {
                if (value != _Rating)
                {
                    _Rating = value;
                    NotifyPropertyChanged("Rating");
                }
            }
        }
        private List<string> _Cast;
        public List<string> Cast
        {
            get
            {
                return _Cast;
            }
            set
            {
                if (value != _Cast)
                {
                    _Cast = value;
                    NotifyPropertyChanged("Cast");
                }
            }
        }
        private string _Director;
        public string Director
        {
            get
            {
                return _Director;
            }
            set
            {
                if (value != _Director)
                {
                    _Director = value;
                    NotifyPropertyChanged("Director");
                }
            }
        }
        private List<string> _Genres;
        public List<string> Genres
        {
            get
            {
                return _Genres;
            }
            set
            {
                if (value != _Genres)
                {
                    _Genres = value;
                    NotifyPropertyChanged("Genres");
                }
            }
        }
        private string _Length;
        public string Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (value != _Length)
                {
                    _Length = value;
                    NotifyPropertyChanged("Length");
                }
            }
        }
        private string _VideoQuality;
        public string VideoQuality
        {
            get
            {
                return _VideoQuality;
            }
            set
            {
                if (value != _VideoQuality)
                {
                    _VideoQuality = value;
                    NotifyPropertyChanged("VideoQuality");
                }
            }
        }

        public string GenresStr
        {
            get
            {
                return string.Join(" | ", Genres);
            }
        }

        public string CastStr
        {
            get
            {
                return string.Join(", ", Cast);
            }
        }

        public Movie()
        {
            Cast = new List<string>();
            Genres = new List<string>();
            VideoQuality = "DVD";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
