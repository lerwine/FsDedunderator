using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator.FileStats
{
    public class Checksum : IFileStats, IFileStatsParent<Guid, CompareGroup>, IEquatable<Checksum>
    {
        private readonly object _syncRoot;
        private readonly List<CompareGroup> _innerList = new List<CompareGroup>();
        private readonly List<FileDirectoryNode> _files = new List<FileDirectoryNode>();
        private Size _parent = null;
        private readonly KeyCollection _keys;
        private readonly ValueCollection _values;

        public CompareGroup this[Guid key]
        {
            get
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    int index = IndexOf(key);
                    return (index < 0) ? null : _innerList[index];
                }
                finally { Monitor.Exit(_syncRoot); }
            }
        }

        public CompareGroup this[int index] { get => _innerList[index]; }

        object IDictionary.this[object key] { get => (key != null && key is Guid) ? this[(Guid)key] : null; set => throw new NotSupportedException(); }

        object IList.this[int index] { get => _innerList[index]; set => ((IList<CompareGroup>)this)[index] = (CompareGroup)value; }

        public MD5Checksum Value { get; private set; }

        public Size Parent
        {
            get => _parent;
            internal set
            {
                Size oldParent = _parent;
                IList list = (IList)(value ?? oldParent);
                if (list == null)
                    return;
                Monitor.Enter(list.SyncRoot);
                try
                {
                    if (_files.Count > 0 || _innerList.Count > 0)
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

        public int FileCount => _files.Count;

        long IFileStats.Length
        {
            get
            {
                Size parent = _parent;
                if (parent == null)
                    throw new InvalidOperationException("Detached items do not have a length");
                return ((IFileStats)parent).Length;
            }
        }

        MD5Checksum? IFileStats.Checksum => Value;

        Guid? IFileStats.ComparisonGroup => null;
        
        bool ICollection<KeyValuePair<Guid, CompareGroup>>.IsReadOnly => throw new NotImplementedException();

        bool ICollection<CompareGroup>.IsReadOnly => throw new NotImplementedException();

        bool IDictionary.IsReadOnly => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();

        bool IDictionary.IsFixedSize => throw new NotImplementedException();

        bool IList.IsFixedSize => throw new NotImplementedException();

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        public int Count => _innerList.Count;

        public ICollection<Guid> Keys => _keys;

        public ICollection<CompareGroup> Values => _values;

        ICollection IDictionary.Keys => _keys;

        ICollection IDictionary.Values => _values;

        CompareGroup IDictionary<Guid, CompareGroup>.this[Guid key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        CompareGroup IList<CompareGroup>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Checksum(MD5Checksum checksum)
        {
            _syncRoot = (_innerList is IList && ((IList)_innerList).IsSynchronized) ? ((IList)_innerList).SyncRoot ?? new object() : new object();
            Value = checksum;
            _keys = new KeyCollection(_innerList);
            _values = new ValueCollection(_innerList);
        }

        public int AddFile(FileDirectoryNode file)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileDirectoryNode> GetFiles()
        {
            throw new NotImplementedException();
        }

        public bool RemoveFile(FileDirectoryNode file)
        {
            throw new NotImplementedException();
        }

        void IDictionary<Guid, CompareGroup>.Add(Guid key, CompareGroup value)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<Guid, CompareGroup>>.Add(KeyValuePair<Guid, CompareGroup> item)
        {
            throw new NotImplementedException();
        }
        
        void IDictionary.Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }
        
        bool ICollection<KeyValuePair<Guid, CompareGroup>>.Contains(KeyValuePair<Guid, CompareGroup> item)
        {
            throw new NotImplementedException();
        }
        
        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }
        
        void ICollection<KeyValuePair<Guid, CompareGroup>>.CopyTo(KeyValuePair<Guid, CompareGroup>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        
        IEnumerator<KeyValuePair<Guid, CompareGroup>> IEnumerable<KeyValuePair<Guid, CompareGroup>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Guid item)
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
        
        bool ICollection<KeyValuePair<Guid, CompareGroup>>.Remove(KeyValuePair<Guid, CompareGroup> item)
        {
            throw new NotImplementedException();
        }
        
        void IDictionary.Remove(object key)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(CompareGroup item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, CompareGroup item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(CompareGroup item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(CompareGroup item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(CompareGroup[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(CompareGroup item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CompareGroup> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Guid key, out CompareGroup value)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Checksum other)
        {
            throw new NotImplementedException();
        }

        internal class KeyCollection : ICollection<Guid>, ICollection
        {
            private ICollection<CompareGroup> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<Guid>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal KeyCollection(ICollection<CompareGroup> collection) { _innerCollection = collection; }

            public bool Contains(Guid item) => _innerCollection.Any(i => i.Value.Equals(item));

            public void CopyTo(Guid[] array, int arrayIndex) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<Guid> GetEnumerator() => _innerCollection.Select(i => i.Value).GetEnumerator();

            void ICollection<Guid>.Add(Guid item) => throw new NotSupportedException();

            void ICollection<Guid>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<Guid>.Remove(Guid item) => throw new NotSupportedException();
        }

        internal class ValueCollection : ICollection<CompareGroup>, ICollection
        {
            private ICollection<CompareGroup> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<CompareGroup>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal ValueCollection(ICollection<CompareGroup> collection) { _innerCollection = collection; }

            public bool Contains(CompareGroup item) => _innerCollection.Contains(item);

            public void CopyTo(CompareGroup[] array, int arrayIndex) => _innerCollection.CopyTo(array, arrayIndex);

            public IEnumerator<CompareGroup> GetEnumerator() => _innerCollection.GetEnumerator();

            void ICollection<CompareGroup>.Add(CompareGroup item) => throw new NotSupportedException();

            void ICollection<CompareGroup>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<CompareGroup>.Remove(CompareGroup item) => throw new NotSupportedException();
        }

        internal class DictionaryEnumerator : IEnumerator<KeyValuePair<Guid, CompareGroup>>, IDictionaryEnumerator
        {
            private IEnumerator<CompareGroup> _innerEnumerator;

            public Guid Key => (_innerEnumerator.Current == null) ? Guid.Empty : _innerEnumerator.Current.Value;

            object IDictionaryEnumerator.Key => (_innerEnumerator.Current == null) ? Guid.Empty : _innerEnumerator.Current.Value;

            public CompareGroup Current => _innerEnumerator.Current;

            object IDictionaryEnumerator.Value => _innerEnumerator.Current;

            KeyValuePair<Guid, CompareGroup> IEnumerator<KeyValuePair<Guid, CompareGroup>>.Current => new KeyValuePair<Guid, CompareGroup>((_innerEnumerator.Current == null) ? Guid.Empty : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            object IEnumerator.Current => _innerEnumerator.Current;

            public DictionaryEntry Entry => new DictionaryEntry((_innerEnumerator.Current == null) ? Guid.Empty : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            public DictionaryEnumerator(IEnumerable<CompareGroup> list) { _innerEnumerator = (list ?? new CompareGroup[0]).GetEnumerator(); }

            public void Dispose() => _innerEnumerator.Dispose();

            public bool MoveNext() => _innerEnumerator.MoveNext();

            public void Reset() => _innerEnumerator.Reset();
        }
    }
}