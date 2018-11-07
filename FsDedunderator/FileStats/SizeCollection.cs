using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator.FileStats
{
    public class SizeCollection : IFileStatsParent<long, Size>
    {
        private readonly object _syncRoot;
        private readonly List<Size> _innerList = new List<Size>();
        private readonly KeyCollection _keys;
        private readonly ValueCollection _values;

        public Size this[long key]
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
       
        public Size this[int index] { get => _innerList[index]; }

        object IDictionary.this[object key] { get => (key != null && key is long) ? this[(long)key] : null; set => throw new NotSupportedException(); }

        Size IList<Size>.this[int index]
        {
            get => this[index];
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                Monitor.Enter(_syncRoot);
                try
                {
                    int i = IndexOf(value.Value);
                    if (i >= 0)
                    {
                        if (index == i && ReferenceEquals(value, _innerList[i]))
                            return;
                        throw new InvalidOperationException("This collection already conatains an item with that size");
                    }

                    _innerList.Add(value);
                }
                finally { Monitor.Exit(_syncRoot); }
            }
        }

        Size IDictionary<long, Size>.this[long key] { get => throw new NotImplementedException(); set => throw new NotSupportedException(); }

        object IList.this[int index] { get => _innerList[index]; set => ((IList<Size>)this)[index] = (Size)value; }

        public int Count => _innerList.Count;

        public ICollection<long> Keys => throw new NotImplementedException();

        ICollection IDictionary.Keys => throw new NotImplementedException();

        public ICollection<Size> Values => throw new NotImplementedException();

        ICollection IDictionary.Values => throw new NotImplementedException();

        bool ICollection<KeyValuePair<long, Size>>.IsReadOnly => false;

        bool ICollection<Size>.IsReadOnly => false;

        bool IDictionary.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;

        public SizeCollection()
        {
            _syncRoot = (_innerList is IList && ((IList)_innerList).IsSynchronized) ? ((IList)_innerList).SyncRoot ?? new object() : new object();
            _keys = new KeyCollection(_innerList);
            _values = new ValueCollection(_innerList);
        }

        private int _Add(Size item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            int index;
            Monitor.Enter(_syncRoot);
            try
            {
                index = IndexOf(item.Value);
                if (index >= 0)
                {
                    if (ReferenceEquals(item, _innerList[index]))
                        return index;
                    throw new InvalidOperationException("This collection already conatains an item with that size");
                }
                item.Parent = this;
                index = _innerList.IndexOf(item);
                if (index < 0)
                {
                    index = _innerList.Count;
                    _innerList.Add(item);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return index;
        }

        public void Add(Size item) => _Add(item);

        void IDictionary<long, Size>.Add(long key, Size value)
        {
            if (key < 0)
                throw new ArgumentOutOfRangeException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value.Value != key)
                throw new InvalidOperationException("Key doesn't match item value");

            Monitor.Enter(_syncRoot);
            try
            {
                int i = IndexOf(key);
                if (i >= 0)
                {
                    if (ReferenceEquals(value, _innerList[i]))
                        return;
                    throw new InvalidOperationException("This collection already conatains an item with that size");
                }
                value.Parent = this;
                if (!_innerList.Contains(value))
                    _innerList.Add(value);
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<KeyValuePair<long, Size>>.Add(KeyValuePair<long, Size> item) => ((IDictionary<long, Size>)this).Add(item.Key, item.Value);

        void IDictionary.Add(object key, object value) => ((IDictionary<long, Size>)this).Add((long)key, (Size)value);

        int IList.Add(object value) => _Add((Size)value);

        public void Clear()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                for (int i = _innerList.Count - 1; i >= 0; i--)
                {
                    Size removedItem = _innerList[i];
                    removedItem.Parent = null;
                    if (i < _innerList.Count && ReferenceEquals(_innerList[i], removedItem))
                        _innerList.RemoveAt(i);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        public bool Contains(Size item) => _innerList.Contains(item);

        bool ICollection<KeyValuePair<long, Size>>.Contains(KeyValuePair<long, Size> item) => item.Value != null && item.Value.Value.Equals(item.Key) && _innerList.Contains(item.Value);

        bool IList.Contains(object value) => value != null && ((value is Size) ? _innerList.Contains((Size)value) :
            ((value is KeyValuePair<long, Size>) ?
                (((KeyValuePair<long, Size>)value).Value != null && ((KeyValuePair<long, Size>)value).Value.Value.Equals(((KeyValuePair<long, Size>)value).Key) && _innerList.Contains(((KeyValuePair<long, Size>)value).Value)) :
                (value is DictionaryEntry && ((DictionaryEntry)value).Value != null && ((DictionaryEntry)value).Value is Size && ((DictionaryEntry)value).Key is long &&
                ((Size)(((DictionaryEntry)value).Value)).Value.Equals((int)(((DictionaryEntry)value).Key)) && _innerList.Contains(((Size)(((DictionaryEntry)value).Value))))));

        public bool ContainsKey(long key)
        {
            if (key < 0)
                return false;
            Monitor.Enter(_syncRoot);
            try
            {
                foreach (Size s in _innerList)
                {
                    if (s.Value.Equals(key))
                        return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        bool IDictionary.Contains(object key) => key != null && key is long && ContainsKey((long)key);

        public void CopyTo(Size[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

        void ICollection<KeyValuePair<long, Size>>.CopyTo(KeyValuePair<long, Size>[] array, int arrayIndex) => _innerList.Select(s => new KeyValuePair<long, Size>(s.Value, s)).ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _innerList.ToArray().CopyTo(array, index);

        public IEnumerator<Size> GetEnumerator() => _innerList.GetEnumerator();

        IEnumerator<KeyValuePair<long, Size>> IEnumerable<KeyValuePair<long, Size>>.GetEnumerator() => new DictionaryEnumerator(_innerList);

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(_innerList);

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();

        public int IndexOf(Size item) => (item == null) ? -1 : _innerList.IndexOf(item);

        public int IndexOf(long key)
        {
            if (key < 0)
                return -1;
            Monitor.Enter(_syncRoot);
            try
            {
                for (int i = 0; i < _innerList.Count; i++)
                {
                    if (_innerList[i].Value.Equals(key))
                        return i;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return -1;
        }

        int IList.IndexOf(object value)
        {
            if (value != null)
            {
                if (value is Size)
                    return IndexOf((Size)value);
                if (value is long)
                    return IndexOf((long)value);
            }
            return -1;
        }

        public void Insert(int index, Size item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            Monitor.Enter(_syncRoot);
            try
            {
                index = IndexOf(item.Value);
                if (index >= 0)
                {
                    if (ReferenceEquals(item, _innerList[index]))
                        return;
                    throw new InvalidOperationException("This collection already conatains an item with that size");
                }
                item.Parent = this;
                if (!_innerList.Contains(item) && ReferenceEquals(item.Parent, this))
                {
                    if (index == _innerList.Count)
                        _innerList.Add(item);
                    else
                        _innerList.Insert(index, item);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void IList.Insert(int index, object value) => Insert(index, (Size)value);

        public bool TryGetValue(long key, out Size value)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(key);
                if (index < 0)
                {
                    value = null;
                    return false;
                }
                value = _innerList[index];
            }
            finally { Monitor.Exit(_syncRoot); }
            return true;
        }

        public bool Remove(Size item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    if (_innerList.Contains(item))
                    {
                        item.Parent = null;
                        if (_innerList.Contains(item))
                            _innerList.Remove(item);
                        return true;
                    }

                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return false;
        }

        bool ICollection<KeyValuePair<long, Size>>.Remove(KeyValuePair<long, Size> item) => item.Value != null && item.Value.Value.Equals(item.Key) && Remove(item.Value);

        public bool Remove(long key)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(key);
                if (index >= 0)
                    return Remove(_innerList[index]);
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        void IDictionary.Remove(object key)
        {
            if (key != null && key is long)
                Remove((long)key);
        }

        void IList.Remove(object value)
        {
            if (value is Size)
                Remove((Size)value);
            else if (value is long)
                Remove((long)value);
        }

        public void RemoveAt(int index)
        {
            Monitor.Enter(_syncRoot);
            try { Remove(_innerList[index]); }
            finally { Monitor.Exit(_syncRoot); }
        }

        internal class KeyCollection : ICollection<long>, ICollection
        {
            private ICollection<Size> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<long>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal KeyCollection(ICollection<Size> collection) { _innerCollection = collection; }

            public bool Contains(long item) => _innerCollection.Any(i => i.Value.Equals(item));

            public void CopyTo(long[] array, int arrayIndex) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<long> GetEnumerator() => _innerCollection.Select(i => i.Value).GetEnumerator();

            void ICollection<long>.Add(long item) => throw new NotSupportedException();

            void ICollection<long>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.Select(i => i.Value).ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<long>.Remove(long item) => throw new NotSupportedException();
        }

        internal class ValueCollection : ICollection<Size>, ICollection
        {
            private ICollection<Size> _innerCollection;

            public int Count => _innerCollection.Count;

            bool ICollection<Size>.IsReadOnly => true;

            bool ICollection.IsSynchronized => _innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized;

            object ICollection.SyncRoot => (_innerCollection is ICollection && ((ICollection)_innerCollection).IsSynchronized) ? ((ICollection)_innerCollection).SyncRoot : null;

            internal ValueCollection(ICollection<Size> collection) { _innerCollection = collection; }

            public bool Contains(Size item) => _innerCollection.Contains(item);

            public void CopyTo(Size[] array, int arrayIndex) => _innerCollection.CopyTo(array, arrayIndex);

            public IEnumerator<Size> GetEnumerator() => _innerCollection.GetEnumerator();

            void ICollection<Size>.Add(Size item) => throw new NotSupportedException();

            void ICollection<Size>.Clear() => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) => _innerCollection.ToArray().CopyTo(array, index);

            IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();

            bool ICollection<Size>.Remove(Size item) => throw new NotSupportedException();
        }

        internal class DictionaryEnumerator : IEnumerator<KeyValuePair<long, Size>>, IDictionaryEnumerator
        {
            private IEnumerator<Size> _innerEnumerator;

            public long Key => (_innerEnumerator.Current == null) ? -1 : _innerEnumerator.Current.Value;

            object IDictionaryEnumerator.Key => (_innerEnumerator.Current == null) ? -1 : _innerEnumerator.Current.Value;

            public Size Current => _innerEnumerator.Current;

            object IDictionaryEnumerator.Value => _innerEnumerator.Current;

            KeyValuePair<long, Size> IEnumerator<KeyValuePair<long, Size>>.Current => new KeyValuePair<long, Size>((_innerEnumerator.Current == null) ? -1 : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            object IEnumerator.Current => _innerEnumerator.Current;

            public DictionaryEntry Entry => new DictionaryEntry((_innerEnumerator.Current == null) ? -1 : _innerEnumerator.Current.Value, _innerEnumerator.Current);

            public DictionaryEnumerator(IEnumerable<Size> list) { _innerEnumerator = (list ?? new Size[0]).GetEnumerator(); }

            public void Dispose() => _innerEnumerator.Dispose();

            public bool MoveNext() => _innerEnumerator.MoveNext();

            public void Reset() => _innerEnumerator.Reset();
        }
    }
}