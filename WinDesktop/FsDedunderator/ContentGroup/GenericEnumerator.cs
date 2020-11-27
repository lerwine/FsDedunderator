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
        public sealed class GenericEnumerator : IEnumerator<IFileReference>
        {
            private IEnumerator<T> _innerEnumerator;
            public IFileReference Current => _innerEnumerator.Current;
            object IEnumerator.Current => _innerEnumerator.Current;
            public GenericEnumerator(IEnumerable<T> source) { _innerEnumerator = source.GetEnumerator(); }
            public void Dispose() => _innerEnumerator.Dispose();
            public bool MoveNext() => _innerEnumerator.MoveNext();
            public void Reset() => _innerEnumerator.Reset();
        }
    }
}