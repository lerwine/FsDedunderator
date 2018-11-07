using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator.FileStats
{
    public class CompareGroup : IFileStats, IList<FileDirectoryNode>, IList, IEquatable<CompareGroup>
    {
        private readonly object _syncRoot;
        private readonly List<FileDirectoryNode> _innerList = new List<FileDirectoryNode>();
        private Checksum _parent = null;

        public Guid Value { get; private set; }

        public Checksum Parent
        {
            get => _parent;
            internal set
            {
                Checksum oldParent = _parent;
                IList list = (IList)(value ?? oldParent);
                if (list == null)
                    return;
                Monitor.Enter(list.SyncRoot);
                try
                {
                    if (_innerList.Count > 0)
                        throw new InvalidOperationException("Cannot change parent of non-empty collection");
                    _parent = value;
                    if (oldParent == null)
                    {
                        if (value != null && !value.Contains(this))
                        {
                            try { value.Add(this); }
                            catch
                            {
                                _parent = (oldParent.Contains(this)) ? oldParent : null;
                                throw;
                            }
                        }
                        return;
                    }
                    if (value != null)
                    {
                        if (ReferenceEquals(value, oldParent))
                            return;
                        if (!value.Contains(this))
                        {
                            try { value.Add(this); }
                            catch
                            {
                                _parent = (oldParent.Contains(this)) ? oldParent : null;
                                throw;
                            }
                        }
                    }
                    if (oldParent.Contains(this))
                        oldParent.Remove(this);
                }
                finally { Monitor.Exit(list.SyncRoot); }
            }
        }

        IFileStatsParent IFileStats.Parent => Parent;

        public FileDirectoryNode this[int index] { get => _innerList[index]; }

        long IFileStats.Length
        {
            get
            {
                Checksum parent = _parent;
                if (parent == null)
                    throw new InvalidOperationException("Detached items do not have a length");
                return ((IFileStats)parent).Length;
            }
        }

        MD5Checksum? IFileStats.Checksum
        {
            get
            {
                Checksum parent = _parent;
                if (parent == null)
                    throw new InvalidOperationException("Detached items do not have a checksum");
                return ((IFileStats)parent).Checksum;
            }
        }

        Guid? IFileStats.ComparisonGroup => throw new NotImplementedException();
        
        bool ICollection<FileDirectoryNode>.IsReadOnly => throw new NotImplementedException();

        bool IList.IsFixedSize => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();
        
        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        public int FileCount => _innerList.Count;

        public int Count => _innerList.Count;

        FileDirectoryNode IList<FileDirectoryNode>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        object IList.this[int index] { get => _innerList[index]; set => ((IList<FileDirectoryNode>)this)[index] = (FileDirectoryNode)value; }

        public CompareGroup(Guid groupID)
        {
            _syncRoot = (_innerList is IList && ((IList)_innerList).IsSynchronized) ? ((IList)_innerList).SyncRoot ?? new object() : new object();
            Value = groupID;
        }
        public bool Equals(CompareGroup other)
        {
            throw new NotImplementedException();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }
        
        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }
        
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileDirectoryNode> GetFiles()
        {
            throw new NotImplementedException();
        }

        public int AddFile(FileDirectoryNode file)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFile(FileDirectoryNode file)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(FileDirectoryNode item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, FileDirectoryNode item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(FileDirectoryNode item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(FileDirectoryNode item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(FileDirectoryNode[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(FileDirectoryNode item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<FileDirectoryNode> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}