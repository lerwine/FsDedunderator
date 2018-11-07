using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FsDedunderator
{
    public class FsRootContainer : IFsContainer, IXmlSerializable
    {
        #region Fields

        private Dictionary<long, SizeGroupedFileData> _fileData = new Dictionary<long, SizeGroupedFileData>();

        #endregion

        #region Properties

        #region Items

        public FsDirectoryCollection Items { get; } = null;

        IFsCollection IFsContainer.Items => Items;

        #endregion

        FsRootContainer IFsStructureElement.Root => this;

        VolumeInformation IFsContainer.Volume => VolumeInformation.Default;

        #endregion

        #region Constructors

        public FsRootContainer() { Items = new FsDirectoryCollection(this); }

        #endregion

        #region Methods

        #region NewDirectory

        public FsStructureDirectory NewDirectory(string name) => new FsStructureDirectory(name, this);

        IFsStructureDirectory IFsContainer.NewDirectory(string name)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region NewFile

        public FsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime, IFileComparisonInfo comparisonInfo)
        {
            throw new NotImplementedException();
        }
        
        public FsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }
        
        IFsStructureFile IFsContainer.NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime, IFileComparisonInfo comparisonInfo)
        {
            throw new NotImplementedException();
        }

        IFsStructureFile IFsContainer.NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetFileData

        internal FsFileData GetFileData(long length, FileComparisonInfo comparisonInfo)
        {
            Monitor.Enter(_fileData);
            try
            {
                SizeGroupedFileData fileData;
                if (_fileData.ContainsKey(length))
                    fileData = _fileData[length];
                else
                {
                    fileData = new SizeGroupedFileData(length);
                    _fileData.Add(length, fileData);
                }
                return fileData.GetFile(comparisonInfo);
            }
            finally { Monitor.Exit(_fileData); }
        }

        internal FsFileData GetFileData(long length, MD5Checksum? checksum)
        {
            Monitor.Enter(_fileData);
            try
            {
                SizeGroupedFileData fileData;
                if (_fileData.ContainsKey(length))
                    fileData = _fileData[length];
                else
                {
                    fileData = new SizeGroupedFileData(length);
                    _fileData.Add(length, fileData);
                }
                return fileData.GetFile(checksum);
            }
            finally { Monitor.Exit(_fileData); }
        }

        #endregion

        #region NewFile

        internal void PurgeFileData(long length, FileComparisonInfo comparisonInfo)
        {
            Monitor.Enter(_fileData);
            try
            {
                throw new NotImplementedException();
            }
            finally { Monitor.Exit(_fileData); }
        }

        internal void PurgeFileData(long length, MD5Checksum? checksum)
        {
            Monitor.Enter(_fileData);
            try
            {
                throw new NotImplementedException();
            }
            finally { Monitor.Exit(_fileData); }
        }

        #endregion

        XmlSchema IXmlSerializable.GetSchema() => null;

        #region Load

        public static FsRootContainer Load(XmlReader reader)
        {
            FsRootContainer container = new FsRootContainer();
            container.ReadXml(reader);
            return container;
        }

        private void ReadXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            Monitor.Enter(_fileData);
            try { Items.ReadXml(reader); }
            finally { Monitor.Exit(_fileData); }
        }

        void IXmlSerializable.ReadXml(XmlReader reader) => ReadXml(reader);

        #endregion

        #region WriteTo

        public void WriteTo(XmlWriter writer)
        {
            Monitor.Enter(_fileData);
            try { Items.WriteTo(writer); }
            finally { Monitor.Exit(_fileData); }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) => WriteTo(writer);

        #endregion

        #endregion

        #region Nested classes
        
        class SizeGroupedFileData
        {
            #region Fields

            private readonly long _length;
            private FsFileData _noChecksum = null;
            private readonly Dictionary<MD5Checksum, ComparisonGroupedData> _fileData = new Dictionary<MD5Checksum, ComparisonGroupedData>();

            #endregion

            #region Properties

            public int Count => (_noChecksum == null) ? _fileData.Count : _fileData.Count + 1;

            #endregion

            #region Constructors

            internal SizeGroupedFileData(long length)
            {
                _length = length;
            }

            #endregion

            #region Methods

            internal void PurgeFile(FileComparisonInfo comparisonInfo)
            {
                throw new NotImplementedException();
            }

            internal void PurgeFile(MD5Checksum? checksum)
            {
                throw new NotImplementedException();
            }

            internal FsFileData GetFile(FileComparisonInfo comparisonInfo)
            {
                Monitor.Enter(_fileData);
                try
                {
                    ComparisonGroupedData groupData;
                    if (_fileData.ContainsKey(comparisonInfo.Checksum))
                        groupData = _fileData[comparisonInfo.Checksum];
                    else
                    {
                        groupData = new ComparisonGroupedData();
                        _fileData.Add(comparisonInfo.Checksum, groupData);
                    }
                    return groupData.GetFile(comparisonInfo, _length);
                }
                finally { Monitor.Exit(_fileData); }
            }

            internal FsFileData GetFile(MD5Checksum? checksum)
            {
                Monitor.Enter(_fileData);
                try
                {
                    if (checksum.HasValue)
                    {
                        ComparisonGroupedData groupData;
                        if (_fileData.ContainsKey(checksum.Value))
                            groupData = _fileData[checksum.Value];
                        else
                        {
                            groupData = new ComparisonGroupedData();
                            _fileData.Add(checksum.Value, groupData);
                        }
                        return groupData.GetFile(checksum.Value, _length);
                    }

                    if (_noChecksum == null)
                        _noChecksum = new FsFileData(_length);
                    return _noChecksum;
                }
                finally { Monitor.Exit(_fileData); }
            }
            
            #endregion
        }

        class ComparisonGroupedData
        {
            #region Fields

            private FsFileData _noGroupId = null;
            private readonly Dictionary<Guid, FsFileData> _fileData = new Dictionary<Guid, FsFileData>();

            #endregion

            #region Properties

            public int Count => (_noGroupId == null) ? _fileData.Count : _fileData.Count + 1;

            #endregion

            #region Constructors

            internal ComparisonGroupedData() { }

            #endregion

            #region Methods

            internal void PurgeFile(FileComparisonInfo comparisonInfo)
            {
                throw new NotImplementedException();
            }

            internal FsFileData GetFile(FileComparisonInfo comparisonInfo, long length)
            {
                Monitor.Enter(_fileData);
                try
                {

                    throw new NotImplementedException();
                }
                finally { Monitor.Exit(_fileData); }
            }

            internal FsFileData GetFile(MD5Checksum checksum, long length)
            {
                Monitor.Enter(_fileData);
                try
                {

                    throw new NotImplementedException();
                }
                finally { Monitor.Exit(_fileData); }
            }

            #endregion
        }

        #endregion
    }
}