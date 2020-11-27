using System;
using System.Collections;
using System.Collections.Generic;

namespace FsDedunderator_obsolete
{
    internal class DictionaryEnumerator<TStructure, TData> : IEnumerator<KeyValuePair<string, TData>>, IDictionaryEnumerator
        where TStructure : IFsStructureChild
        where TData : IFsData
    {
        #region Fields

        private IEnumerator<TStructure> _innerEnumerator;

        #endregion

        #region Properties

        #region Current

        public TStructure Current => _innerEnumerator.Current;

        KeyValuePair<string, TData> IEnumerator<KeyValuePair<string, TData>>.Current => new KeyValuePair<string, TData>(_innerEnumerator.Current.Name, (TData)(_innerEnumerator.Current.Data));

        object IEnumerator.Current => _innerEnumerator.Current;

        #endregion

        public DictionaryEntry Entry => new DictionaryEntry(_innerEnumerator.Current.Name, _innerEnumerator.Current.Data);

        #region Key

        public string Key => _innerEnumerator.Current.Name;

        object IDictionaryEnumerator.Key => Key;

        #endregion

        #region Value

        public TData Value => (TData)(_innerEnumerator.Current.Data);

        object IDictionaryEnumerator.Value => _innerEnumerator.Current.Data;

        #endregion

        #endregion

        #region Constructors

        internal DictionaryEnumerator(IEnumerable<TStructure> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerator");
            _innerEnumerator = enumerable.GetEnumerator();
        }

        #endregion

        #region Methods

        public void Dispose() => _innerEnumerator.Dispose();

        public bool MoveNext() => _innerEnumerator.MoveNext();

        public void Reset() => _innerEnumerator.Reset();

        #endregion
    }
}