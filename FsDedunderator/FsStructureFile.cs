using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public class FsStructureFile : FsStructureChild, IFsStructureFile
    {
        #region Fields

        private readonly FsFileData _data;

        public FsFileData Data => _data;

        #endregion

        #region Properties

        #region Parent

        public FsStructureDirectory Parent { get => (FsStructureDirectory)BaseParent; set => BaseParent = value; }

        IFsStructureDirectory IFsStructureFile.Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override IFsContainer BaseParent { get => base.BaseParent; set => base.BaseParent = (FsStructureDirectory)value; }

        #endregion

        #region Data

        IFsFileData IFsStructureFile.Data => _data;

        protected override IFsData BaseData => _data;

        #endregion

        public long Length => Data.Length;

        public DateTime CreationTimeUTC { get; private set; }

        public DateTime LastWriteTimeUTC { get; private set; }

        public DateTime LastCheckTimeUTC { get; private set; }

        public FsCheckType CheckType => (Data.ComparisonInfo == null) ? FsCheckType.SizeOnly : ((Data.ComparisonInfo.ComparisonGroupID.HasValue) ? FsCheckType.Checksum : FsCheckType.GroupCompare);

        public IFileComparisonInfo ComparisonInfo => Data.ComparisonInfo;

        #endregion

        #region Constructors

        internal FsStructureFile(string name, FsRootContainer root, DateTime creationTime, DateTime lastWriteTime, long length, FileComparisonInfo comparisonInfo)
            : base(name, root)
        {
            _data = root.GetFileData(length, comparisonInfo);
            CreationTimeUTC = (creationTime.Kind == DateTimeKind.Utc) ? creationTime : ((creationTime.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(creationTime, DateTimeKind.Local) : creationTime).ToUniversalTime();
            LastWriteTimeUTC = (lastWriteTime.Kind == DateTimeKind.Utc) ? lastWriteTime : ((lastWriteTime.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Local) : lastWriteTime).ToUniversalTime();
            LastCheckTimeUTC = DateTime.Now;
        }

        internal FsStructureFile(string name, FsRootContainer root, DateTime creationTime, DateTime lastWriteTime, long length, MD5Checksum? checksum)
            : base(name, root)
        {
            _data = root.GetFileData(length, checksum);
            CreationTimeUTC = (creationTime.Kind == DateTimeKind.Utc) ? creationTime : ((creationTime.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(creationTime, DateTimeKind.Local) : creationTime).ToUniversalTime();
            LastWriteTimeUTC = (lastWriteTime.Kind == DateTimeKind.Utc) ? lastWriteTime : ((lastWriteTime.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Local) : lastWriteTime).ToUniversalTime();
            LastCheckTimeUTC = DateTime.Now;
        }

        #endregion

        #region Methods

        protected override void OnParentChanged(IFsContainer oldParent)
        {
            FsStructureDirectory oldDirectory = (FsStructureDirectory)oldParent;

            if (oldDirectory == null)
                Data.AddLink(this);
            else if (Parent == null)
                Data.RemoveLink(this);
            base.OnParentChanged(oldParent);
        }

        #endregion
    }
}