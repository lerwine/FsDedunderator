using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace FsDedunderator
{
    public abstract partial class ContentGroup<T> :  IContentGroup
        where T : IFileReference
    {
        public sealed class FileReferenceList : IList<T>, IList
        {
            private ContentGroup<T> _owner;
            private List<T> _innerList = new List<T>();

            public T this[int index]
            {
                get => _innerList[index]; set
                {
                    Monitor.Enter(SyncRoot);
                    try
                    {
                        _innerList[index] = value;
                    }
                    finally { Monitor.Exit(SyncRoot); }
                }
            }

            object IList.this[int index] { get => _innerList[index]; set => this[index] = (T)value; }

            public int Count => _innerList.Count;

            bool ICollection<T>.IsReadOnly => false;

            bool IList.IsReadOnly => false;

            bool IList.IsFixedSize => false;

            bool ICollection.IsSynchronized => true;

            internal object SyncRoot { get; } = new object();

            object ICollection.SyncRoot => SyncRoot;

            internal FileReferenceList(ContentGroup<T> owner) { _owner = owner; }

            public void Add(T item)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    _innerList.Add(item);
                }
                finally { Monitor.Exit(SyncRoot); }
            }

            int IList.Add(object value)
            {
                int index;
                Monitor.Enter(SyncRoot);
                try
                {
                    index = _innerList.Count;
                    Add((T)value);
                }
                finally { Monitor.Exit(SyncRoot); }
                return index;
            }

            public void Clear()
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    _innerList.Clear();
                }
                finally { Monitor.Exit(SyncRoot); }
            }

            public bool Contains(T item)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    return _innerList.Contains(item);
                }
                finally { Monitor.Exit(SyncRoot); }
            }

            bool IList.Contains(object value) => value != null && value is T && Contains((T)value);

            public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

            void ICollection.CopyTo(Array array, int index) => _innerList.ToArray().CopyTo(array, index);

            public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int IndexOf(T item)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    return _innerList.IndexOf(item);
                }
                finally { Monitor.Exit(SyncRoot); }
            }

            int IList.IndexOf(object value) => (value != null & value is T) ? IndexOf((T)value) : -1;

            public void Insert(int index, T item)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    _innerList.Insert(index, item);
                }
                finally { Monitor.Exit(SyncRoot); }
            }

            void IList.Insert(int index, object value) => Insert(index, (T)value);

            public bool Remove(T item)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    return _innerList.Remove(item);
                }
                finally { Monitor.Exit(SyncRoot); }
            }
 
            void IList.Remove(object value)
            {
                if (value != null && value is T)
                    Remove((T)value);
            }

            public void RemoveAt(int index)
            {
                Monitor.Enter(SyncRoot);
                try
                {
                    _innerList.RemoveAt(index);
                }
                finally { Monitor.Exit(SyncRoot); }
            }
        }
    }
}