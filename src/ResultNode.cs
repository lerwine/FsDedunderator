using System;
using System.Xml;
using System.Xml.Serialization;

namespace FsDedunderator
{
    public abstract class ResultNode
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("error", IsNullable = true)]
        public string ErrorMessage { get; set; }

        protected ResultNode(string name) {
            ErrorMessage = null;
            Name = name;
        }
    }
}