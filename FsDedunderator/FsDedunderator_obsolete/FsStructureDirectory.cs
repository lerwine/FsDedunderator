using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public class FsStructureDirectory : FsStructureChild, IFsStructureDirectory
    {
        #region Fields

        private VolumeInformation _volume;
        private readonly FsDirectoryData _items;

        #endregion

        #region Properties

        #region Items

        public FsDirectoryData Items => _items;

        protected override IFsData BaseData => throw new NotImplementedException();

        IFsDirectoryData IFsStructureDirectory.Items => (IFsDirectoryData)BaseData;

        IFsCollection IFsContainer.Items => _items;

        IFsData IFsStructureChild.Data => _items;

        #endregion

        #region Parent

        public IFsContainer Parent { get => BaseParent; set => BaseParent = value; }

        protected override IFsContainer BaseParent
        {
            get => base.BaseParent;
            set
            {
                Monitor.Enter(((ICollection)BaseData).SyncRoot);
                try
                {
                    if (value == null || ReferenceEquals(_volume, value.Volume))
                    {
                        base.BaseParent = value;
                        return;
                    }
                    VolumeInformation oldVolume = _volume;
                    Volume = value.Volume;
                    try { base.BaseParent = value; }
                    catch
                    {
                        _volume = oldVolume;
                        throw;
                    }
                }
                finally { Monitor.Exit(((ICollection)BaseData).SyncRoot); }
            }
        }

        #endregion

        public VolumeInformation Volume
        {
            get => _volume;
            set
            {
                Monitor.Enter(((ICollection)BaseData).SyncRoot);
                try
                {
                    VolumeInformation v = value ?? VolumeInformation.Default;
                    if (Parent != null && Parent.Volume.Equals(v))
                        v = Parent.Volume;
                    if (ReferenceEquals(v, _volume) || !v.IsCaseSensitive || _volume.IsCaseSensitive)
                        return;
                    if (Items.Keys.Distinct(v).Count() != Items.Count)
                        throw new InvalidOperationException("Changing volume would cause duplicate file names");
                    var oldVolumeInfo = Items.OfType<FsStructureDirectory>().Select(d => new { v = d.Volume, d }).ToArray();
                    try
                    {
                        foreach (FsStructureDirectory dir in Items.OfType<FsStructureDirectory>())
                            dir.Volume = v;
                    }
                    catch
                    {
                        foreach (var d in oldVolumeInfo)
                            d.d._volume = d.v;
                        throw;
                    }
                    _volume = v;
                }
                finally { Monitor.Exit(((ICollection)BaseData).SyncRoot); }
            }
        }

        public DateTime? LastCheckTimeUTC { get; private set; }

        #endregion

        #region Constructors

        internal FsStructureDirectory(string name, FsRootContainer root, VolumeInformation volume)
            : base(name, root)
        {
            _volume = volume ?? VolumeInformation.Default;
            _items = new FsDirectoryData(this);
        }

        internal FsStructureDirectory(string name, FsRootContainer root) : this(name, root, null) { }

        #endregion

        #region Methods

        public bool IsAncestorOrSelf(FsStructureDirectory parent)
        {
            if (parent != null)
            {
                if (ReferenceEquals(parent, this))
                    return true;
                Monitor.Enter(((ICollection)Items).SyncRoot);
                try
                {
                    for (IFsContainer p = parent.Parent; p != null && p is FsStructureDirectory; p = ((FsStructureDirectory)p).Parent)
                    {
                        if (ReferenceEquals(p, this))
                            return true;
                    }
                }
                finally { Monitor.Exit(((ICollection)Items).SyncRoot); }
            }
            return false;
        }

        #region NewDirectory

        public FsStructureDirectory NewDirectory(string name)
        {
            throw new NotImplementedException();
        }

        IFsStructureDirectory IFsContainer.NewDirectory(string name) => NewDirectory(name);

        #endregion

        #region NewFile

        public FsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime, IFileComparisonInfo comparisonInfo)
        {
            throw new NotImplementedException();
        }

        public FsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime) => NewFile(name, length, creationTime, lastWriteTime, null);
        
        IFsStructureFile IFsContainer.NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime, IFileComparisonInfo comparisonInfo) =>
            NewFile(name, length, creationTime, lastWriteTime, comparisonInfo);

        IFsStructureFile IFsContainer.NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime) => NewFile(name, length, creationTime, lastWriteTime);

        #endregion

        #endregion
    }
}