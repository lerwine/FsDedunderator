using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace FsDedunderator
{
    [XmlRoot("Directory")]
    public class ResultDirectory : ResultNode
    {
        [XmlElement("Directory", Type = typeof(ResultDirectory))]
        [XmlElement("File", Type = typeof(ResultFile))]
        public Collection<ResultNode> ChildNodes { get; set; }

        internal ResultDirectory(string name) : base(name) {
            ChildNodes = new Collection<ResultNode>();
        }
        
        public ResultDirectory() : this("") { }
    }
}