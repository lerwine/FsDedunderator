using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator.FileStats
{
    public class Size : IFileStats, IFileStatsParent<MD5Checksum, Checksum>, IEquatable<Size>
    {
        private readonly object _syncRoot;
        private readonly List<Checksum> _innerList = new List<Checksum>();
        private readonly List<FileDirectoryNode> _files = new List<FileDirectoryNode>();
        private SizeCollection _parent = null;
        private readonly KeyCollection _keys;
        private readonly ValueCollection _values;

        public long Value { get; private set; }

        public SizeCollection Parent
        {
            get => _parent;
            internal set
            {
                SizeCollection oldParent = _parent;
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

        long IFileStats.Length => Value;

        MD5Checksum? IFileStats.Checksum => null;

        Guid? IFileStats.ComparisonGroup => null;
        
        bool ICollection<KeyValuePair<MD5Checksum, Checksum>>.IsReadOnly => throw new NotImplementedException();
        
        bool ICollection<Checksum>.IsReadOnly => throw new NotImplementedException();

        bool IDictionary.IsFixedSize => throw new NotImplementedException();

        bool IDictionary.IsReadOnly => throw new NotImplementedException();
        
        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        bool IList.IsFixedSize => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();

        public int FileCount => _files.Count;

        public ICollection<MD5Checksum> Keys => _keys;

        public ICollection<Checksum> Values => _values;

        public int Count => _innerList.Count;

        ICollection IDictionary.Keys => _keys;

        ICollection IDictionary.Values => _values;

        Checksum IList<Checksum>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Checksum IDictionary<MD5Checksum, Checksum>.this[MD5Checksum key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Checksum this[int index] { get => _innerList[index]; }

        public Checksum this[MD5Checksum key]
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

        object IList.this[int index] { get => _innerList[index]; set => ((IList<Checksum>)this)[index] = (Checksum)value; }

        object IDictionary.this[object key] { get => (key != null && key is MD5Checksum) ? this[(MD5Checksum)key] : null; set => throw new NotSupportedException(); }

        public Size(long length)
        {
            if (length < 0L)
                throw new ArgumentOutOfRangeException("length");
            _syncRoot = (_innerList is IList && ((IList)_innerList).IsSynchronized) ? ((IList)_innerList).SyncRoot ?? new object() : new object();
            Value = length;
            _keys = new KeyCollection(_innerList);
            _values = new ValueCollection(_innerList);
        }

        void IDictionary<MD5Checksum, Checksum>.Add(MD5Checksum key, Checksum value)
        {
            throw new NotImplementedException();
        }
        
        void ICollection<KeyValuePair<MD5Checksum, Checksum>>.Add(KeyValuePair<MD5Checksum, Checksum> item)
        {
            throw new NotImplementedException();
        }
        
        bool ICollection<KeyValuePair<MD5Checksum, Checksum>>.Contains(KeyValuePair<MD5Checksum, Checksum> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<MD5Checksum, Checksum>>.CopyTo(KeyValuePair<MD5Checksum, Checksum>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<MD5Checksum, Checksum>>.Remove(KeyValuePair<MD5Checksum, Checksum> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<MD5Checksum, Checksum>> IEnumerable<KeyValuePair<MD5Checksum, Checksum>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int IndexOf(MD5Checksum item)
        {
            throw new NotImplementedException();
        }

        void IList<Checksum>.Insert(int index, Checksum item)
        {
            throw new NotImplementedException();
        }
        
        void IDictionary.Add(object key, object value)
        {
            throw new NotImplementedException();
        }
        
        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void IDictionary.Remove(object key)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
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
        
        public bool Equals(Size other)
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

        public bool ContainsKey(MD5Checksum key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(MD5Checksum key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(MD5Checksum key, out Checksum value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Checksum item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(Checksum item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Checksum item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Checksum[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Checksum item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Checksum> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        internal class KeyCollection : ICollection<MD5Checksum>, ICollection
        {
            private ICollection<Checksum> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<MD5Checksum>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal KeyCollection(ICollection<Checksum> collection) { _innerCollection = collection; }

            public bool Contains(MD5Checksum item) => _innerCollection.Any(i => i.Value.Equals(item));

            public void CopyTo(MD5Checksum[] array, int arrayIndex) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<MD5Checksum> GetEnumerator() => _innerCollection.Select(i => i.Value).GetEnumerator();

            void ICollection<MD5Checksum>.Add(MD5Checksum item) => throw new NotSupportedException();

            void ICollection<MD5Checksum>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<MD5Checksum>.Remove(MD5Checksum item) => throw new NotSupportedException();
        }

        internal class ValueCollection : ICollection<Checksum>, ICollection
        {
            private ICollection<Checksum> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<Checksum>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal ValueCollection(ICollection<Checksum> collection) { _innerCollection = collection; }

            public bool Contains(Checksum item) => _innerCollection.Contains(item);

            public void CopyTo(Checksum[] array, int arrayIndex) => _innerCollection.CopyTo(array, arrayIndex);

            public IEnumerator<Checksum> GetEnumerator() => _innerCollection.GetEnumerator();

            void ICollection<Checksum>.Add(Checksum item) => throw new NotSupportedException();

            void ICollection<Checksum>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<Checksum>.Remove(Checksum item) => throw new NotSupportedException();
        }

        internal class DictionaryEnumerator : IEnumerator<KeyValuePair<MD5Checksum, Checksum>>, IDictionaryEnumerator
        {
            private IEnumerator<Checksum> _innerEnumerator;

            public MD5Checksum Key => (_innerEnumerator.Current == null) ? new MD5Checksum() : _innerEnumerator.Current.Value;

            object IDictionaryEnumerator.Key => (_innerEnumerator.Current == null) ? new MD5Checksum() : _innerEnumerator.Current.Value;

            public Checksum Current => _innerEnumerator.Current;

            object IDictionaryEnumerator.Value => _innerEnumerator.Current;

            KeyValuePair<MD5Checksum, Checksum> IEnumerator<KeyValuePair<MD5Checksum, Checksum>>.Current => new KeyValuePair<MD5Checksum, Checksum>((_innerEnumerator.Current == null) ? new MD5Checksum() : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            object IEnumerator.Current => _innerEnumerator.Current;

            public DictionaryEntry Entry => new DictionaryEntry((_innerEnumerator.Current == null) ? new MD5Checksum() : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            public DictionaryEnumerator(IEnumerable<Checksum> list) { _innerEnumerator = (list ?? new Checksum[0]).GetEnumerator(); }

            public void Dispose() => _innerEnumerator.Dispose();

            public bool MoveNext() => _innerEnumerator.MoveNext();

            public void Reset() => _innerEnumerator.Reset();
        }
    }
}
