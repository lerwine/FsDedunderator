using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FsDedunderator_obsolete
{
    internal class FsStructureValueCollection<TElement, TData> : ICollection<TData>, ICollection
        where TElement : IFsStructureChild
        where TData : IFsData
    {
        #region Fields

        private IList<TElement> _innerList;

        #endregion

        #region Properties

        public int Count => _innerList.Count;

        bool ICollection<TData>.IsReadOnly => true;

        bool ICollection.IsSynchronized => _innerList is ICollection && ((ICollection)_innerList).IsSynchronized;

        object ICollection.SyncRoot => (_innerList is ICollection && ((ICollection)_innerList).IsSynchronized) ? ((ICollection)_innerList).SyncRoot : null;

        #endregion

        #region Constructors

        internal FsStructureValueCollection(IList<TElement> list)
        {
            _innerList = list ?? new List<TElement>();
        }

        #endregion

        #region Methods

        public bool Contains(TData item) => item != null && _innerList.Any(i => ReferenceEquals(i.Data, item));

        #region CopyTo

        public void CopyTo(TData[] array, int arrayIndex) => _innerList.Select(i => i.Data).ToArray().CopyTo(array, arrayIndex);

        #endregion

        #region GetEnumerator

        public IEnumerator<TData> GetEnumerator() => _innerList.Select(i => (TData)(i.Data)).GetEnumerator();

        #endregion

        void ICollection<TData>.Add(TData item) => throw new NotSupportedException();

        void ICollection<TData>.Clear() => throw new NotSupportedException();

        void ICollection.CopyTo(Array array, int index) => _innerList.Select(i => (TData)(i.Data)).ToArray().CopyTo(array, index);

        IEnumerator IEnumerable.GetEnumerator() => _innerList.Select(i => (TData)(i.Data)).GetEnumerator();

        bool ICollection<TData>.Remove(TData item) => throw new NotSupportedException();

        #endregion
    }
}