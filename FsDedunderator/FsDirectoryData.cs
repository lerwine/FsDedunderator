using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public class FsDirectoryData : IFsDirectoryData, IFsCollection
    {
        #region Fields

        private readonly object _syncRoot = new object();
        private readonly List<FsStructureChild> _innerList = new List<FsStructureChild>();
        private readonly FsStructureKeyCollection<FsStructureChild> _keys;
        private readonly FsStructureValueCollection<FsStructureChild, IFsData> _values;

        #endregion

        #region Properties

        #region Indexer (string)

        public IFsData this[string key]
        {
            get
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    int index = IndexOf(key);
                    if (index >= 0)
                        return ((IFsStructureChild)(_innerList[index])).Data;
                }
                finally { Monitor.Exit(_syncRoot); }
                return null;
            }
            set => throw new NotSupportedException();
        }

        object IDictionary.this[object key] { get => (key != null && key is string) ? this[(string)key] : null; set => this[(string)key] = (IFsData)value; }

        #endregion

        #region Indexer (int)

        public FsStructureChild this[int index]
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

        IFsStructureChild IList<IFsStructureChild>.this[int index] { get => this[index]; set => this[index] = (FsStructureChild)value; }

        IFsData IList<IFsData>.this[int index] { get => ((IFsStructureChild)(this[index])).Data; set => throw new NotSupportedException(); }

        object IList.this[int index] { get => this[index]; set => this[index] = (FsStructureChild)value; }

        #endregion

        public int Count => _innerList.Count;

        #region Explicit Properties

        bool ICollection<KeyValuePair<string, IFsData>>.IsReadOnly => false;

        bool ICollection<IFsStructureChild>.IsReadOnly => false;

        bool ICollection<IFsData>.IsReadOnly => false;

        bool IDictionary.IsReadOnly => false;

        bool IList.IsReadOnly => false;
        
        bool IDictionary.IsFixedSize => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => true;

        ICollection<IFsStructureChild> IFsData.Links => new IFsStructureChild[] { Owner };

        #endregion

        #region Keys

        public ICollection<string> Keys => _keys;

        ICollection IDictionary.Keys => _keys;

        #endregion

        public VolumeInformation Volume => (Owner == null) ? VolumeInformation.Default : Owner.Volume;

        #region Owner

        public FsStructureDirectory Owner { get; private set; }

        IFsContainer IFsCollection.Owner => Owner;

        #endregion

        #region Values

        public ICollection<IFsData> Values => _values;

        ICollection IDictionary.Values => _values;

        #endregion

        object ICollection.SyncRoot => _syncRoot;

        #endregion

        #region Constructors

        internal FsDirectoryData(FsStructureDirectory owner)
        {
            Owner = owner ?? throw new ArgumentNullException("owner");
            _keys = new FsStructureKeyCollection<FsStructureChild>(_innerList, owner);
            _values = new FsStructureValueCollection<FsStructureChild, IFsData>(_innerList);
        }

        #endregion

        #region Methods

        #region Add

        private int AppendItem(FsStructureChild item)
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
                    if (item is FsStructureDirectory && Owner.IsAncestorOrSelf((FsStructureDirectory)item))
                        throw new InvalidOperationException("Cannot circular references");
                    index = _innerList.Count;
                    _innerList.Add(item);
                    try { ((IFsStructureChild)item).Parent = Owner; }
                    catch
                    {
                        if (((IFsStructureChild)item).Parent == null || !ReferenceEquals(((IFsStructureChild)item).Parent, Owner))
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

        public void Add(FsStructureFile item) => AppendItem(item);

        public void Add(FsStructureDirectory item) => AppendItem(item);

        void IDictionary<string, IFsData>.Add(string key, IFsData value) => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, IFsData>>.Add(KeyValuePair<string, IFsData> item) => throw new NotSupportedException();

        void ICollection<IFsStructureChild>.Add(IFsStructureChild item)
        {
            throw new NotImplementedException();
        }

        void ICollection<IFsData>.Add(IFsData item) => throw new NotSupportedException();

        void IDictionary.Add(object key, object value) => throw new NotSupportedException();

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AddLink

        public void AddLink(FsStructureChild structureFile)
        {
            throw new NotImplementedException();
        }

        void IFsData.AddLink(IFsStructureChild structureFile)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Clear()
        {
            throw new NotImplementedException();
        }

        #region Contains

        public bool Contains(FsStructureFile item) => item != null && _innerList.Contains(item);

        public bool Contains(FsStructureDirectory item) => item != null && _innerList.Contains(item);

        public bool Contains(FsFileData item) => item != null && _innerList.Cast<IFsStructureChild>().Any(c => ReferenceEquals(item, c.Data));

        public bool Contains(FsDirectoryData item) => item != null && _innerList.Cast<IFsStructureChild>().Any(c => ReferenceEquals(item, c.Data));

        bool ICollection<KeyValuePair<string, IFsData>>.Contains(KeyValuePair<string, IFsData> item)
        {
            if (item.Value == null)
                return false;

            IFsData v = this[item.Key];
            return v != null && ReferenceEquals(v, item.Value);
        }

        bool ICollection<IFsStructureChild>.Contains(IFsStructureChild item) => item != null && item is FsStructureChild && _innerList.Contains(item);

        bool ICollection<IFsData>.Contains(IFsData item) => item != null && _innerList.Cast<IFsStructureChild>().Any(c => ReferenceEquals(item, c.Data));

        bool IList.Contains(object value) => value != null && ((value is FsStructureChild) ? _innerList.Contains((FsStructureChild)value) : (value is IFsData && _innerList.Cast<IFsStructureChild>().Any(c => ReferenceEquals(value, c.Data))));

        #endregion

        #region ContainsKey

        public bool ContainsKey(string key) => key != null && _innerList.Select(c => c.Name).Contains(key, Owner.Volume);

        bool IDictionary.Contains(object key) => key != null && key is string && _innerList.Select(c => c.Name).Contains((string)key, Owner.Volume);

        #endregion

        #region CopyTo

        public void CopyTo(FsStructureChild[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

        public void CopyTo(IFsData[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Cast<IFsStructureChild>().Select(c => c.Data).ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<KeyValuePair<string, IFsData>>.CopyTo(KeyValuePair<string, IFsData>[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Cast<IFsStructureChild>().Select(c => new KeyValuePair<string, IFsData>(c.Name, c.Data)).ToArray().CopyTo(array, arrayIndex); }
            finally { Monitor.Exit(_syncRoot); }
        }

        void ICollection<IFsStructureChild>.CopyTo(IFsStructureChild[] array, int arrayIndex)
        {
            Monitor.Enter(_syncRoot);
            try { _innerList.Cast<IFsStructureChild>().ToArray().CopyTo(array, arrayIndex); }
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

        public IEnumerator<FsStructureChild> GetEnumerator() => _innerList.GetEnumerator();

        IEnumerator<KeyValuePair<string, IFsData>> IEnumerable<KeyValuePair<string, IFsData>>.GetEnumerator() => new DictionaryEnumerator<FsStructureChild, IFsData>(_innerList);

        IEnumerator<IFsStructureChild> IEnumerable<IFsStructureChild>.GetEnumerator() => _innerList.Cast<IFsStructureChild>().GetEnumerator();

        IEnumerator<IFsData> IEnumerable<IFsData>.GetEnumerator() => _innerList.Cast<IFsStructureChild>().Select(c => c.Data).GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator<FsStructureChild, IFsData>(_innerList);

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();

        #endregion

        #region Name_GetItemByNameMe

        public FsStructureChild GetItemByName(string name) => (name == null) ? null : _innerList.FirstOrDefault(i => Owner.Volume.Equals(i.Name, name));

        IFsStructureChild IFsCollection.GetItemByName(string name) => GetItemByName(name);

        #endregion

        #region IndexOf

        public int IndexOf(FsStructureDirectory item) => (item == null) ? -1 : _innerList.IndexOf(item);

        public int IndexOf(FsStructureFile item) => (item == null) ? -1 : _innerList.IndexOf(item);

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

        public int IndexOf(FsFileData item)
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
                        if (Owner.Volume.Equals(_innerList[i].Name, item))
                            return i;
                    }
                }
                finally { Monitor.Exit(_syncRoot); }
            }
            return -1;
        }

        int IList<IFsStructureChild>.IndexOf(IFsStructureChild item) => (item != null && item is FsStructureDirectory) ? IndexOf((FsStructureDirectory)item) : -1;

        int IList<IFsData>.IndexOf(IFsData item) => (item != null && item is FsDirectoryData) ? IndexOf((FsDirectoryData)item) : -1;

        int IList.IndexOf(object value) => (value == null) ? -1 : ((value is FsStructureDirectory) ? IndexOf((FsStructureDirectory)value) :
            ((value is FsStructureFile) ? IndexOf((FsStructureFile)value) : ((value is FsDirectoryData) ? IndexOf((FsDirectoryData)value) : ((value is FsFileData) ? IndexOf((FsFileData)value) : -1))));

        #endregion

        #region Insert

        public void Insert(int index, FsStructureDirectory item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, FsStructureFile item)
        {
            throw new NotImplementedException();
        }

        void IList<IFsStructureChild>.Insert(int index, IFsStructureChild item)
        {
            throw new NotImplementedException();
        }

        void IList<IFsData>.Insert(int index, IFsData item)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Remove

        public bool Remove(FsStructureDirectory item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(FsStructureFile item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, IFsData>>.Remove(KeyValuePair<string, IFsData> item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<IFsStructureChild>.Remove(IFsStructureChild item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<IFsData>.Remove(IFsData item)
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

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #region RemoveLink

        public void RemoveLink(FsStructureDirectory structureFile)
        {
            throw new NotImplementedException();
        }

        private void ReplaceItemAt(int index, FsStructureChild value)
        {
            throw new NotImplementedException();
        }

        void IFsData.RemoveLink(IFsStructureChild structureFile)
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool TryGetValue(string key, out IFsData value)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                int index = IndexOf(key);
                if (index >= 0)
                {
                    value = ((IFsStructureChild)(_innerList[index])).Data;
                    return true;
                }
            }
            finally { Monitor.Exit(_syncRoot); }
            value = null;
            return false;
        }

        #region Upsert

        public bool Upsert(string name, Func<FsStructureFile> createNew, out FsStructureFile item)
        {
            throw new NotImplementedException();
        }

        public bool Upsert(string name, Func<FsStructureDirectory> createNew, out FsStructureDirectory item)
        {
            throw new NotImplementedException();
        }

        bool IFsCollection.Upsert(string name, Func<IFsStructureChild> createNew, out IFsStructureChild item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}