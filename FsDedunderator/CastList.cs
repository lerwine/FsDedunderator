using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FsDedunderator
{
    /// <summary>
    /// Utility class for casting one list element type to another.
    /// </summary>
    /// <typeparam name="TSource">Element type of source list.</typeparam>
    /// <typeparam name="TBase">Casted element type.</typeparam>
    internal class CastList<TSource, TBase> : IList<TBase>, IList
        where TSource : TBase
    {
        #region Fields

        private IList<TSource> _values;

        #endregion

        #region Properties

        #region Indexer

        public TBase this[int index] { get => _values[index]; set => _values[index] = (TSource)value; }

        object IList.this[int index] { get => _values[index]; set => _values[index] = (TSource)value; }

        #endregion

        public int Count => _values.Count;

        #region Explicit Properties

        bool ICollection<TBase>.IsReadOnly => _values.IsReadOnly;

        bool IList.IsReadOnly => _values.IsReadOnly;

        bool ICollection.IsSynchronized => _values is ICollection && ((ICollection)_values).IsSynchronized;

        object ICollection.SyncRoot => (_values is ICollection && ((ICollection)_values).IsSynchronized) ? ((ICollection)_values).SyncRoot : null;

        bool IList.IsFixedSize => _values is IList && ((IList)_values).IsFixedSize;

        #endregion

        #endregion

        #region Constructors

        public CastList(IList<TSource> values)
        {
            _values = values ?? new List<TSource>();
        }

        #endregion

        #region Methods

        #region Add

        int IList.Add(object value) => ((IList)_values).Add(value);

        void ICollection<TBase>.Add(TBase item) => _values.Add((TSource)item);

        #endregion

        #region Clear

        void ICollection<TBase>.Clear() => _values.Clear();

        void IList.Clear() => _values.Clear();

        #endregion

        #region Contains

        public bool Contains(TBase item) => item is TSource && _values.Contains((TSource)item);

        bool IList.Contains(object value) => value != null && value is TSource && _values.Contains((TSource)value);

        #endregion

        #region CopyTo

        public void CopyTo(TBase[] array, int arrayIndex) => _values.Cast<TBase>().ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _values.ToArray().CopyTo(array, index);

        #endregion

        #region GetEnumerator

        public IEnumerator<TBase> GetEnumerator() => _values.Cast<TBase>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Cast<TBase>().GetEnumerator();

        #endregion

        #region IndexOf

        int IList<TBase>.IndexOf(TBase item) => (item is TSource) ? _values.IndexOf((TSource)item) : -1;

        int IList.IndexOf(object value) => (value != null && value is TSource) ? _values.IndexOf((TSource)value) : -1;

        #endregion

        #region Insert

        void IList<TBase>.Insert(int index, TBase item) => _values.Insert(index, ((TSource)item));

        void IList.Insert(int index, object value) => _values.Insert(index, ((TSource)value));

        #endregion

        #region Remove

        bool ICollection<TBase>.Remove(TBase item) => item is TSource && _values.Remove((TSource)item);

        void IList.Remove(object value)
        {
            if (value != null && value is TSource)
                _values.Remove((TSource)value);
        }

        #endregion

        #region RemoveAt

        void IList<TBase>.RemoveAt(int index) => _values.RemoveAt(index);

        void IList.RemoveAt(int index) => _values.RemoveAt(index);

        #endregion

        #endregion
    }
}