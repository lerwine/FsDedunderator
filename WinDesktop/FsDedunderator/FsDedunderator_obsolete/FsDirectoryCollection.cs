using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace FsDedunderator_obsolete
{
    public class FsDirectoryCollection : IFsCollection
    {
        #region Fields

        private readonly object _syncRoot = new object();
        private readonly List<FsStructureDirectory> _innerList = new List<FsStructureDirectory>();
        private readonly FsRootContainer _owner;
        private readonly FsStructureKeyCollection<FsStructureDirectory> _keys;
        private readonly FsStructureValueCollection<FsStructureDirectory, FsDirectoryData> _values;

        #endregion

        #region Properties

        #region Indexer (string)

        public FsDirectoryData this[string key]
        {
            get
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    int index = IndexOf(key);
                    if (index >= 0)
                        return _innerList[index].Items;
                }
                finally { Monitor.Exit(_syncRoot); }
                return null;
            }
        }

        IFsData IDictionary<string, IFsData>.this[string key] { get => this[key]; set => throw new NotSupportedException(); }

        object IDictionary.this[object key] { get => (key != null && key is string) ? this[(string)key] : null; set => throw new NotSupportedException(); }

        #endregion

        #region Indexer (int)

        public FsStructureDirectory this[int index]
        {
            get => _innerList[index];
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                try { ReplaceItemAt(index, value); }
                catch (ArgumentException e) { throw new ArgumentException(e.Message, "value", e); }
            }
        }

        IFsStructureChild IList<IFsStructureChild>.this[int index] { get => this[index]; set => this[index] = (FsStructureDirectory)value; }

        IFsData IList<IFsData>.this[int index] { get => this[index].Items; set => throw new NotSupportedException(); }

        object IList.this[int index] { get => this[index]; set => this[index] = (FsStructureDirectory)value; }

        #endregion

        #region Keys

        public ICollection<string> Keys => _keys;

        ICollection IDictionary.Keys => _keys;

        public ICollection<FsDirectoryData> Values => _values;

        #endregion

        #region Values

        ICollection<IFsData> IDictionary<string, IFsData>.Values => new CastCollection<FsDirectoryData, IFsData>(_values);

        ICollection IDictionary.Values => _values;

        #endregion

        public int Count => _values.Count;

        #region Owner

        public FsRootContainer Owner => _owner;

        IFsContainer IFsCollection.Owner => _owner;

        #endregion

        #region Explicit Properties

        bool IDictionary.IsFixedSize => false;

        bool IList.IsFixedSize => false;

        bool ICollection<KeyValuePair<string, IFsData>>.IsReadOnly => false;

        bool ICollection<IFsStructureChild>.IsReadOnly => false;

        bool ICollection<IFsData>.IsReadOnly => true;

        bool IDictionary.IsReadOnly => false;

        bool IList.IsReadOnly => false;
        
        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _syncRoot;

        #endregion

        #endregion

        #region Constructors

        internal FsDirectoryCollection(FsRootContainer owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (owner.Items != null)
                throw new InvalidOperationException();
            _owner = owner;
            _keys = new FsStructureKeyCollection<FsStructureDirectory>(_innerList, _owner);
            _values = new FsStructureValueCollection<FsStructureDirectory, FsDirectoryData>(_innerList);
        }

        #endregion

        #region Methods

        #region Add

        private int AppendItem(FsStructureDirectory item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            int index;
            Monitor.Enter(_syncRoot);
            try
            {
                index = IndexOf(item.Name);
                if (index < 0)
                {
                    index = _innerList.Count;
                    _innerList.Add(item);
                    try { item.Parent = Owner; }
                    catch
                    {
                        if (item.Parent == null || !ReferenceEquals(item.Parent, Owner))
                        {
                            if (index < _innerList.Count && ReferenceEquals(_innerList[index], item))
                                _innerList.RemoveAt(index);
                            else
                                _innerList.Remove(item);
                        }
                        throw;
                    }
                }
                else if (!ReferenceEquals(item, _innerList[index]))
                    throw new InvalidOperationException("Another item with the same name already exists in this collection.");
            }
            finally { Monitor.Exit(_syncRoot); }
            return index;
        }

        public void Add(FsStructureDirectory item) => AppendItem(item);
        
        void IDictionary<string, IFsData>.Add(string key, IFsData value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, IFsData>>.Add(KeyValuePair<string, IFsData> item) => throw new NotSupportedException();

        void ICollection<IFsStructureChild>.Add(IFsStructureChild item) => Add((FsStructureDirectory)item);

        void ICollection<IFsData>.Add(IFsData item) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();

        int IList.Add(object value) => AppendItem((FsStructureDirectory)value);

        #endregion

        public void Clear()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                List<FsStructureDirectory> clearedItems = new List<FsStructureDirectory>(_innerList);
                _innerList.Clear();
                try
                {
                    for (int i = 0; i < clearedItems.Count; i++)
                    {
                        if (clearedItems[i].Parent != null && ReferenceEquals(clearedItems[i].Parent, Owner))
                            clearedItems[i].Parent = null;
                        clearedItems.RemoveAt(i);
                        i--;
                    }
                }
                finally
                {
                    if (clearedItems.Count > 0)
                        _innerList.AddRange(clearedItems);
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        #region Contains

        public bool Contains(FsStructureDirectory item) => item != null && _innerList.Contains(item);

        public bool Contains(FsDirectoryData item) => item != null && _innerList.Any(c => ReferenceEquals(item, c.Items));

        bool ICollection<IFsStructureChild>.Contains(IFsStructureChild item) => item != null && item is FsStructureDirectory && _innerList.Contains(item);

        bool ICollection<IFsData>.Contains(IFsData item) => item != null && item is FsDirectoryData && _innerList.Any(c => ReferenceEquals(item, c.Items));

        bool IList.Contains(object value) => value != null && ((value is FsDirectoryData) ? _innerList.Any(c => ReferenceEquals(value, c.Items)) : (value is FsStructureDirectory && _innerList.Contains((FsStructureDirectory)value)));

        bool ICollection<KeyValuePair<string, IFsData>>.Contains(KeyValuePair<string, IFsData> item)
        {
            if (item.Value == null || !(item.Value is FsDirectoryData))
                return false;

            FsDirectoryData v = this[item.Key];
            return v != null && ReferenceEquals(v, item.Value);
        }

        #endregion

        #region ContainsKey

        public bool ContainsKey(string key) => key != null && _innerList.Select(c => c.Name).Contains(key, VolumeInformation.Default);

        bool IDictionary.Contains(object key) => key != null && key is string && _innerList.Select(c => c.Name).Contains((string)key, VolumeInformation.Default);

        #endregion

        #region CopyTo

        public void CopyTo(FsStructureDirectory[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

        public void CopyTo(FsDirectoryData[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Select(c => c.Items).ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<KeyValuePair<string, IFsData>>.CopyTo(KeyValuePair<string, IFsData>[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Select(c => new KeyValuePair<string, IFsData>(c.Name, c.Items)).ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<IFsStructureChild>.CopyTo(IFsStructureChild[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Cast<IFsStructureChild>().ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<IFsData>.CopyTo(IFsData[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Select(c => c.Items).Cast<IFsData>().ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.ToArray().CopyTo(array, index); }
            finally { Monitor.Exit(_syncRoot); }
        }

        #endregion

        #region GetEnumerator

        public IEnumerator<FsStructureDirectory> GetEnumerator() => _innerList.GetEnumerator();

        IEnumerator<KeyValuePair<string, IFsData>> IEnumerable<KeyValuePair<string, IFsData>>.GetEnumerator() => new DictionaryEnumerator<FsStructureDirectory, IFsData>(_innerList);

        IEnumerator<IFsStructureChild> IEnumerable<IFsStructureChild>.GetEnumerator() => _innerList.Cast<IFsStructureChild>().GetEnumerator();

        IEnumerator<IFsData> IEnumerable<IFsData>.GetEnumerator() => _innerList.Select(c => (IFsData)(c.Items)).GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator<FsStructureDirectory, IFsData>(_innerList);

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();

        #endregion

        #region GetItemByName

        public FsStructureDirectory GetItemByName(string name) => (name == null) ? null : _innerList.FirstOrDefault(i => VolumeInformation.Default.Equals(i.Name, name));

        IFsStructureChild IFsCollection.GetItemByName(string name) => GetItemByName(name);

        #endregion

        #region IndexOf

        public int IndexOf(FsStructureDirectory item) => (item == null) ? -1 : _innerList.IndexOf(item);

        public int IndexOf(FsDirectoryData item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    for (int i = 0; i < _innerList.Count; i++)
                    {
                        if (ReferenceEquals(_innerList[i], item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        public int IndexOf(string item)
        {
            if (item != null)
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    for (int i = 0; i < _innerList.Count; i++)
                    {
                        if (VolumeInformation.Default.Equals(_innerList[i].Name, item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        int IList<IFsStructureChild>.IndexOf(IFsStructureChild item) => (item != null && item is FsStructureDirectory) ? IndexOf((FsStructureDirectory)item) : -1;

        int IList<IFsData>.IndexOf(IFsData item) => (item != null && item is FsDirectoryData) ? IndexOf((FsDirectoryData)item) : -1;

        int IList.IndexOf(object value) => (value == null) ? -1 : ((value is FsStructureDirectory) ? IndexOf((FsStructureDirectory)value) : ((value is FsDirectoryData) ? IndexOf((FsDirectoryData)value) : -1));

        #endregion

        #region Insert

        public void Insert(int index, FsStructureDirectory item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Monitor.Enter(_syncRoot);
            try
            {
                int nIdx = IndexOf(item.Name);
                if (nIdx < 0)
                {
                    if (index == _innerList.Count)
                        _innerList.Add(item);
                    else
                        _innerList.Insert(index, item);
                    try { item.Parent = Owner; }
                    catch
                    {
                        if (item.Parent == null || !ReferenceEquals(item.Parent, Owner))
                        {
                            if (index < _innerList.Count && ReferenceEquals(_innerList[index], item))
                                _innerList.RemoveAt(index);
                            else
                                _innerList.Remove(item);
                        }
                        throw;
                    }
                }
                else if (ReferenceEquals(_innerList[nIdx], item))
                {
                    if (nIdx != index && nIdx != index - 1)
                        throw new InvalidOperationException("That item already exists in this collection.");
                }
                else
                    throw new InvalidOperationException("Another item with that same name already exists in this collection.");
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        void IList<IFsStructureChild>.Insert(int index, IFsStructureChild item) => Insert(index, (FsStructureDirectory)item);

        void IList<IFsData>.Insert(int index, IFsData item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => Insert(index, (FsStructureDirectory)value);

        #endregion

        #region Remove

        public bool Remove(string key)
        {
            if (key == null)
                return false;

            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(key);
                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        public bool Remove(FsStructureDirectory item)
        {
            if (item == null)
                return false;

            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(item);
                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }
        
        private bool _Remove(KeyValuePair<string, IFsData> item)
        {
            if (item.Value == null)
                return false;

            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(item.Key);
                if (index >= 0 && ReferenceEquals(item.Value, _innerList[index].Items))
                {
                    RemoveAt(index);
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        bool ICollection<KeyValuePair<string, IFsData>>.Remove(KeyValuePair<string, IFsData> item) => _Remove(item);

        bool ICollection<IFsStructureChild>.Remove(IFsStructureChild item) => (item != null && item is FsStructureDirectory) ? Remove((FsStructureDirectory)item) : false;

        bool ICollection<IFsData>.Remove(IFsData item) => throw new NotSupportedException();

        void IDictionary.Remove(object key)
        {
            if (key != null && key is string)
                Remove((string)key);
        }

        void IList.Remove(object value)
        {
            if (value == null)
                return;
            if (value is FsStructureDirectory)
                Remove((FsStructureDirectory)value);
            else if (value is string)
                Remove((string)value);
            else if (value is KeyValuePair<string, IFsData>)
                _Remove((KeyValuePair<string, IFsData>)value);
        }

        #endregion

        public void RemoveAt(int index)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                FsStructureDirectory removedItem = _innerList[index];
                _innerList.RemoveAt(index);
                if (removedItem.Parent != null && ReferenceEquals(removedItem.Parent, Owner))
                {
                    try { removedItem.Parent = null; }
                    catch
                    {
                        if (removedItem.Parent != null && ReferenceEquals(removedItem.Parent, Owner))
                        {
                            if (index < _innerList.Count)
                                _innerList.Insert(index, removedItem);
                            else
                                _innerList.Add(removedItem);
                        }
                        throw;
                    }
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        private void ReplaceItemAt(int index, FsStructureDirectory value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            Monitor.Enter(_syncRoot);
            try
            {
                FsStructureDirectory removedItem = _innerList[index];
                if (ReferenceEquals(removedItem, value))
                    return;
                _innerList[index] = value;
                if (removedItem.Parent != null && ReferenceEquals(removedItem.Parent, Owner))
                {
                    try { removedItem.Parent = null; }
                    catch
                    {
                        if (removedItem.Parent != null && ReferenceEquals(removedItem.Parent, Owner))
                            _innerList[index] = removedItem;
                        else
                            value.Parent = Owner;
                        throw;
                    }
                }
                try { value.Parent = Owner; }
                catch
                {
                    if (value.Parent == null || !ReferenceEquals(value.Parent, Owner))
                    {
                        _innerList[index] = removedItem;
                        removedItem.Parent = Owner;
                    }
                    throw;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        #region TryGetValue

        public bool TryGetValue(string key, out FsDirectoryData value)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(key);
                if (index >= 0)
                {
                    value = _innerList[index].Items;
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            value = null;
            return false;
        }
        
        bool IDictionary<string, IFsData>.TryGetValue(string key, out IFsData value)
        {
            bool result = TryGetValue(key, out FsDirectoryData v);
            value = v;
            return result;
        }

        #endregion

        #region Upsert

        public bool Upsert(string name, Func<FsStructureDirectory> createNew, out FsStructureDirectory item)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (createNew == null)
                throw new ArgumentNullException("createNew");

            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(name);
                if (index < 0)
                {
                    item = createNew();
                    AppendItem(item);
                    return true;
                }
                item = _innerList[index];
            }
            finally { Monitor.Exit(_syncRoot); }
            return false;
        }

        bool IFsCollection.Upsert(string name, Func<IFsStructureChild> createNew, out IFsStructureChild item)
        {
            bool result = Upsert(name, () => (FsStructureDirectory)createNew(), out FsStructureDirectory i);
            item = i;
            return result;
        }

        #endregion
        
        internal void ReadXml(XmlReader reader)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                foreach (FsStructureDirectory directory in _innerList)
                    throw new NotImplementedException();
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        internal void WriteTo(XmlWriter writer)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                foreach (FsStructureDirectory directory in _innerList)
                    throw new NotImplementedException();
            }
            finally { Monitor.Exit(_syncRoot); }
        }

        #endregion
    }
}