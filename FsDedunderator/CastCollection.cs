using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FsDedunderator
{
    /// <summary>
    /// Utility class for casting collection element types.
    /// </summary>
    /// <typeparam name="TSource">Source collection element type.</typeparam>
    /// <typeparam name="TBase">Casted collection element type.</typeparam>
    internal class CastCollection<TSource, TBase> : ICollection<TBase>, ICollection
        where TSource : TBase
    {
        #region Fields

        private readonly ICollection<TSource> _values;

        #endregion

        #region Properties

        public int Count => _values.Count;

        bool ICollection<TBase>.IsReadOnly => _values.IsReadOnly;

        bool ICollection.IsSynchronized => _values is ICollection && ((ICollection)_values).IsSynchronized;

        object ICollection.SyncRoot => (_values is ICollection && ((ICollection)_values).IsSynchronized) ? ((ICollection)_values).SyncRoot : null;

        #endregion

        #region Constructors

        public CastCollection(ICollection<TSource> values)
        {
            _values = values ?? new List<TSource>();
        }

        #endregion

        #region Methods

        void ICollection<TBase>.Add(TBase item) => _values.Add((TSource)item);

        void ICollection<TBase>.Clear() => _values.Clear();
        
        public bool Contains(TBase item) => item is TSource && _values.Contains((TSource)item);

        #region CopyTo

        public void CopyTo(TBase[] array, int arrayIndex) => _values.Cast<TBase>().ToArray().CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index) => _values.ToArray().CopyTo(array, index);

        #endregion

        #region GetEnumerator

        public IEnumerator<TBase> GetEnumerator() => _values.Cast<TBase>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Cast<TBase>().GetEnumerator();

        #endregion

        bool ICollection<TBase>.Remove(TBase item) => item is TSource && _values.Remove((TSource)item);

        #endregion
    }
}