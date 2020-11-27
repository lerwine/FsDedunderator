using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace FsDedunderator
{
    [XmlRoot("CrawlResult")]
    public class CrawlResult
    {
        [XmlAttribute]
        public string JobName { get; set; }
        [XmlAttribute]
        public ExitCodes ExitCode { get; set; }

        [XmlElement(IsNullable = true)]
        public string ErrorMessage { get; set; }

        [XmlElement("Directory", Type = typeof(ResultDirectory))]
        [XmlElement("File", Type = typeof(ResultFile))]
        public Collection<ResultNode> Nodes { get; set; }
    }
}