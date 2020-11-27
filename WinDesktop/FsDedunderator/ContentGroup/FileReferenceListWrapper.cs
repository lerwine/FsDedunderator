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
        public sealed class FileReferenceListWrapper : IList<IFileReference>
        {
            private FileReferenceList _innerList;
            public IFileReference this[int index] { get => _innerList[index]; set => _innerList[index] = (T)value; }
            public int Count => _innerList.Count;
            public bool IsReadOnly => false;
            internal FileReferenceListWrapper(FileReferenceList source) { _innerList = source; }
            public void Add(IFileReference item) => _innerList.Add((T)item);
            public void Clear() => _innerList.Clear();
            public bool Contains(IFileReference item) => _innerList.Contains((T)item);
            public void CopyTo(IFileReference[] array, int arrayIndex) => _innerList.ToArray().CopyTo(array, arrayIndex);
            public IEnumerator<IFileReference> GetEnumerator() => new GenericEnumerator(_innerList);
            public int IndexOf(IFileReference item) => (item != null && item is T) ? _innerList.IndexOf((T)item) : -1;
            public void Insert(int index, IFileReference item) => _innerList.Insert(index, (T)item);
            public bool Remove(IFileReference item) => (item != null && item is T) ? _innerList.Remove((T)item) : false;
            public void RemoveAt(int index) => _innerList.RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}