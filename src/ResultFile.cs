using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FsDedunderator
{
    [XmlRoot("File")]
    public class ResultFile : ResultNode
    {
        [XmlAttribute("length")]
        public long Length { get; set; }
        
        private DateTime? _creationTimeUtc = null;

        [XmlAttribute("created")]
        public string __CreationTimeUtc
        {
            get { return (_creationTimeUtc.HasValue) ? XmlConvert.ToString(_creationTimeUtc.Value, XmlDateTimeSerializationMode.Utc) : null; }
            set
            {
                string d = value;
                if (null == d || (d = d.Trim()).Length == 0)
                    _creationTimeUtc = null;
                else
                    _creationTimeUtc = XmlConvert.ToDateTime(d, XmlDateTimeSerializationMode.Utc);
            }
        }

        [XmlIgnore]
        public DateTime? CreationTimeUtc { get { return _creationTimeUtc; } set { _creationTimeUtc = value; } }

        private DateTime? _lastWriteTimeUtc = null;
        
        [XmlAttribute("modified")]
        public string __LastWriteTimeUtc
        {
            get { return (_lastWriteTimeUtc.HasValue) ? XmlConvert.ToString(_lastWriteTimeUtc.Value, XmlDateTimeSerializationMode.Utc) : null; }
            set
            {
                string d = value;
                if (null == d || (d = d.Trim()).Length == 0)
                    _lastWriteTimeUtc = null;
                else
                    _lastWriteTimeUtc = XmlConvert.ToDateTime(d, XmlDateTimeSerializationMode.Utc);
            }
        }

        [XmlIgnore]
        public DateTime? LastWriteTimeUtc { get { return _lastWriteTimeUtc; } set { _lastWriteTimeUtc = value; } }

        internal ResultFile(FileInfo file) : base(file.Name) {
            Length = file.Length;
            LastWriteTimeUtc = file.LastWriteTimeUtc;
            CreationTimeUtc = file.CreationTimeUtc;
        }
        
        internal ResultFile(string name, long length) : base(name) {
            Length = length;
        }
        
        public ResultFile() : this("", -1L) { }
    }
}