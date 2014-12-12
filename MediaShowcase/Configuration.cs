using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MediaShowcase
{
    [XmlRootAttribute("Configuration", Namespace = "", IsNullable = false)]
    public class Configuration
    {
        public string DefaultMoviesDirectory { get; set; }
    }
}
