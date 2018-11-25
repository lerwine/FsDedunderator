using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public sealed class FileDirectory : DirectoryNode, IList<DirectoryNode>, IList, IEquatable<FileDirectory>
    {
        private List<DirectoryNode> _innerList = new List<DirectoryNode>();
        public DirectoryNode this[int index]
        {
            get => _innerList[index]; internal set
            {
                _innerList[index] = value;
            }
        }
        DirectoryNode IList<DirectoryNode>.this[int index] { get => _innerList[index]; set => this[index] = value; }
        object IList.this[int index] { get => _innerList[index]; set => this[index] = (DirectoryNode)value; }

        public int Count => _innerList.Count;

        internal object SyncRoot { get; } = new object();

        bool ICollection<DirectoryNode>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => SyncRoot;

        bool ICollection.IsSynchronized => true;

        internal void Add(DirectoryNode item)
        {
            _innerList.Add(item);
        }

        void ICollection<DirectoryNode>.Add(DirectoryNode item)
        {
            throw new NotImplementedException();
        }
        int IList.Add(object value)
        {
            int index;
                index = _innerList.Count;
                _innerList.Add((DirectoryNode)value);
            return index;
        }

        internal void Clear()
        {
            _innerList.Clear();
        }

        void ICollection<DirectoryNode>.Clear()
        {
            throw new NotImplementedException();
        }
        void IList.Clear()
        {
            throw new NotImplementedException();
        }
        public bool Contains(DirectoryNode item)
        {
            return _innerList.Contains(item);
        }

        bool IList.Contains(object value)
        {
            return value != null && value is DirectoryNode && Contains((DirectoryNode)value);
        }

        public void CopyTo(DirectoryNode[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _innerList.ToArray().CopyTo(array, index);
        }
        public bool Equals(FileDirectory other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(DirectoryNode other)
        {
            throw new NotImplementedException();
        }
        public IEnumerator<DirectoryNode> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(DirectoryNode item)
        {
            return _innerList.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            return (value != null && value is DirectoryNode) ? IndexOf((DirectoryNode)value) : -1;
        }

        internal void Insert(int index, DirectoryNode item)
        {
            _innerList.Insert(index, item);
        }
        void IList<DirectoryNode>.Insert(int index, DirectoryNode item)
        {
            throw new NotImplementedException();
        }
        void IList.Insert(int index, object value)
        {
            _innerList.Insert(index, (DirectoryNode)value);
        }

        internal bool Remove(DirectoryNode item)
        {
            return _innerList.Remove(item);
        }

        bool ICollection<DirectoryNode>.Remove(DirectoryNode item)
        {
            throw new NotImplementedException();
        }
        void IList.Remove(object value)
        {
            if (value != null && value is DirectoryNode)
                Remove((DirectoryNode)value);
        }

        internal void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        void IList<DirectoryNode>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}