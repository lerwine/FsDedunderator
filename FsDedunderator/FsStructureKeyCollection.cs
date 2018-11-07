using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FsDedunderator
{
    internal class FsStructureKeyCollection<T> : ICollection<string>, ICollection
        where T : IFsStructureChild
    {
        #region Fields

        private IList<T> _innerList;
        private IFsContainer _owner;

        #endregion

        #region Properties

        public int Count => _innerList.Count;

        bool ICollection<string>.IsReadOnly => true;

        bool ICollection.IsSynchronized => _innerList is ICollection && ((ICollection)_innerList).IsSynchronized;

        object ICollection.SyncRoot => (_innerList is ICollection && ((ICollection)_innerList).IsSynchronized) ? ((ICollection)_innerList).SyncRoot : null;

        #endregion

        #region Constructors

        internal FsStructureKeyCollection(IList<T> list, IFsContainer owner)
        {
            _innerList = list ?? new List<T>();
            _owner = owner ?? throw new ArgumentNullException("owner");
        }

        #endregion

        #region Methods

        public bool Contains(string item) => item != null && _innerList.Select(i => i.Name).Contains(item, _owner.Volume);

        #region CopyTo

        public void CopyTo(string[] array, int arrayIndex) => _innerList.Select(i => i.Name).ToArray().CopyTo(array, arrayIndex);

        #endregion

        #region GetEnumerator

        public IEnumerator<string> GetEnumerator() => _innerList.Select(i => i.Name).GetEnumerator();

        #endregion

        void ICollection<string>.Add(string item) => throw new NotSupportedException();

        void ICollection<string>.Clear() => throw new NotSupportedException();

        void ICollection.CopyTo(Array array, int index) => _innerList.Select(i => i.Name).ToArray().CopyTo(array, index);

        IEnumerator IEnumerable.GetEnumerator() => _innerList.Select(i => i.Name).GetEnumerator();

        bool ICollection<string>.Remove(string item) => throw new NotSupportedException();

        #endregion
    }
}