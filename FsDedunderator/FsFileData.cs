using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace FsDedunderator
{
    public class FsFileData : IFsFileData, IEquatable<FsFileData>
    {
        #region Fields

        private List<FsStructureFile> _links = new List<FsStructureFile>();

        #endregion

        #region Properties

        public long Length { get; private set; }

        public IFileComparisonInfo ComparisonInfo { get; private set; }

        #region Links

        public LinkCollection Links { get; private set; }

        ICollection<IFsStructureFile> IFsFileData.Links => throw new NotImplementedException();

        ICollection<IFsStructureChild> IFsData.Links => throw new NotImplementedException();

        #endregion

        #endregion

        #region Constructors

        internal FsFileData(long length, FileComparisonInfo comparisonInfo) : this(length) { ComparisonInfo = comparisonInfo; }

        internal FsFileData(long length, MD5Checksum checksum) : this(length) { ComparisonInfo = checksum; }

        internal FsFileData(long length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            Length = length;
            Links = new LinkCollection(_links);
        }

        #endregion

        #region Methods

        #region AddLink

        internal void AddLink(FsStructureFile structureFile)
        {
            if (structureFile == null)
                throw new ArgumentNullException("structureFile");

            Monitor.Enter(((ICollection)Links).SyncRoot);
            try
            {
                if (_links.Contains(structureFile))
                    return;
                if (!ReferenceEquals(structureFile.Data, this))
                    throw new InvalidOperationException("File does not reference this data");
                if (structureFile.Parent == null)
                    throw new InvalidOperationException("File does not have a parent");
                _links.Add(structureFile);
            }
            finally { Monitor.Exit(((ICollection)Links).SyncRoot); }
        }

        void IFsFileData.AddLink(IFsStructureFile structureFile) => AddLink((FsStructureFile)structureFile);

        void IFsData.AddLink(IFsStructureChild structureFile) => AddLink((FsStructureFile)structureFile);

        #endregion

        #region RemoveLink

        internal void RemoveLink(FsStructureFile structureFile)
        {
            Monitor.Enter(((ICollection)Links).SyncRoot);
            try
            {
                if (!_links.Contains(structureFile))
                    return;
                if (structureFile.Parent != null)
                    throw new InvalidOperationException("File still has a parent");
                if (ReferenceEquals(structureFile.Data, this))
                    throw new InvalidOperationException("File still references this data");
                _links.Remove(structureFile);
            }
            finally { Monitor.Exit(((ICollection)Links).SyncRoot); }
        }

        void IFsFileData.RemoveLink(IFsStructureFile structureFile) => RemoveLink((FsStructureFile)structureFile);

        void IFsData.RemoveLink(IFsStructureChild structureFile) => RemoveLink((FsStructureFile)structureFile);

        #endregion

        #region Equals

        public bool Equals(FsFileData other) => other != null && (ReferenceEquals(this, other) || Length == other.Length && ((ComparisonInfo == null) ? other.ComparisonInfo == null : ComparisonInfo.Equals(other.ComparisonInfo)));
        
        public override bool Equals(object obj) => obj != null && obj is FsFileData && Equals((FsFileData)obj);

        #endregion

        public override int GetHashCode() => (ComparisonInfo == null) ? Length.GetHashCode() : (int)(Length & 0xfff) | ComparisonInfo.GetHashCode() << 12;

        public override string ToString() => (ComparisonInfo == null) ? Length.ToString() : Length.ToString() + " (" + ComparisonInfo.ToString() + ")";

        internal void WriteLinksTo(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}