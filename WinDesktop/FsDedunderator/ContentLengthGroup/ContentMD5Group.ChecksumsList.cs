using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public partial class ContentLengthGroup
    {
        private ContentMD5Group _firstMD5Node = null;
        private ContentMD5Group _lastMD5Node = null;
        private long _md5Count = 0;
        internal object SyncRoot { get; } = new object();
        public partial class ContentMD5Group
        {
            private ContentMD5Group _nextNode = null;
            public sealed class ChecksumsList : IList<ContentMD5Group>, IList
            {
                public ContentLengthGroup Content { get; }
                public ContentMD5Group this[long index]
                {
                    get
                    {
                        if (index >= 0)
                        {
                            Monitor.Enter(Content.SyncRoot);
                            try
                            {
                                ContentMD5Group node = Content._firstMD5Node;
                                for (long i = 0L; i < index; i++)
                                {
                                    if (node == null)
                                        break;
                                    node = node._nextNode;
                                }
                                if (node != null)
                                    return node;
                            }
                            finally { Monitor.Exit(Content.SyncRoot); }
                        }
                        throw new IndexOutOfRangeException();
                    }
                }
                ContentMD5Group IList<ContentMD5Group>.this[int index] { get => this[index]; set => throw new NotImplementedException(); }
                object IList.this[int index] { get => this[index]; set => throw new NotImplementedException(); }
                public long Count => Content._md5Count;
                bool ICollection<ContentMD5Group>.IsReadOnly => true;
                bool IList.IsReadOnly => true;
                bool IList.IsFixedSize => false;
                bool ICollection.IsSynchronized => true;
                object ICollection.SyncRoot => Content.SyncRoot;

                int ICollection<ContentMD5Group>.Count => throw new NotImplementedException();

                int ICollection.Count => throw new NotImplementedException();

                internal ChecksumsList(ContentLengthGroup content) { Content = content; }
                void ICollection<ContentMD5Group>.Add(ContentMD5Group item) => throw new NotSupportedException();
                int IList.Add(object value) => throw new NotSupportedException();
                internal void Clear()
                {
                    Monitor.Enter(Content.SyncRoot);
                    try
                    {
                        ContentMD5Group firstRemoved = Content._firstMD5Node;
                        if (firstRemoved == null)
                            return;
                        Content._firstMD5Node = Content._lastMD5Node = null;
                        Content._md5Count = 0;
                        ContentMD5Group previous;
                        do
                        {
                            firstRemoved._owner = null;
                            firstRemoved = (previous = firstRemoved)._nextNode;
                            previous._nextNode = null;
                        } while (firstRemoved != null);
                    }
                    finally { Monitor.Exit(Content.SyncRoot); }
                }
                void ICollection<ContentMD5Group>.Clear() => Clear();
                void IList.Clear() => Clear();
                public bool Contains(ContentMD5Group item)
                {
                    ChecksumsList owner;
                    return item != null && (owner = item._owner) != null && ReferenceEquals(owner, this);
                }
                bool IList.Contains(object value) => value != null && value is ContentMD5Group && Contains((ContentMD5Group)value);
                
                /// <summary>
                /// Copies the elements of the <see cref="ChecksumsList" /> to an <seealso cref="Array" />, starting at a particular <seealso cref="Array" /> index.
                /// </summary>
                /// <param name="array">The one-dimensional <seealso cref="Array" /> that is the destination of the elements copied from <see cref="ChecksumsList" />.
                /// The <seealso cref="Array" /> must have zero-based indexing.</param>
                /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
                /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
                /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
                /// <exception cref="ArgumentException">The number of elements in the source <see cref="ChecksumsList" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
                public void CopyTo(ContentMD5Group[] array, int arrayIndex)
                {
                    if (array == null)
                        throw new ArgumentNullException("array");
                    if (arrayIndex < 0)
                        throw new ArgumentOutOfRangeException("arrayIndex", "Array index cannot be less than zero");
                    Monitor.Enter(Content.SyncRoot);
                    try
                    {
                        if (((long)arrayIndex + Content._md5Count) > array.LongLength)
                            throw new ArgumentException("Not enough available space at the end of the array", "arrayIndex");
                        long index = (long)arrayIndex;
                        for (ContentMD5Group node = Content._firstMD5Node; node != null; node = node._nextNode)
                        {
                            try { array.SetValue(node, index); }
                            catch (Exception exception)
                            {
                                if (((long)arrayIndex + Content._md5Count) > array.LongLength)
                                    throw new ArgumentException("Not enough available space at the end of the array", "arrayIndex", exception);
                                throw new ArgumentException(exception.Message, "array", exception);
                            }
                            index++;
                        }
                    }
                    finally { Monitor.Exit(Content.SyncRoot); }
                }
                void ICollection.CopyTo(Array array, int index)
                {
                    if (array == null)
                        throw new ArgumentNullException("array");
                    if (index < 0)
                        throw new ArgumentOutOfRangeException("index", "Array index cannot be less than zero");
                    if (array.Rank != 1)
                        throw new ArgumentException("Array is not single-dimensional", "array");
                    Monitor.Enter(Content.SyncRoot);
                    try
                    {
                        if (((long)index + Content._md5Count) > array.LongLength)
                            throw new ArgumentException("Not enough available space at the end of the array", "arrayIndex");
                        long arrayIndex = (long)index;
                        for (ContentMD5Group node = Content._firstMD5Node; node != null; node = node._nextNode)
                        {
                            try { array.SetValue(node, arrayIndex); }
                            catch (Exception exception)
                            {
                                if (((long)index + Content._md5Count) > array.LongLength)
                                    throw new ArgumentException("Not enough available space at the end of the array", "arrayIndex", exception);
                                throw new ArgumentException(exception.Message, "array", exception);
                            }
                            arrayIndex++;
                        }
                    }
                    finally { Monitor.Exit(Content.SyncRoot); }
                }
                public IEnumerator<ContentMD5Group> GetEnumerator() => new Enumerator(Content);
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                public long IndexOf(ContentMD5Group item)
                {
                    if (item != null)
                    {
                        Monitor.Enter(Content.SyncRoot);
                        try
                        {
                            if (item._owner != null && ReferenceEquals(item._owner, this))
                            {
                                long index = 0;
                                for (ContentMD5Group node = Content._firstMD5Node; node != null; node = node._nextNode)
                                {
                                    if (ReferenceEquals(item, node))
                                        return index;
                                    index++;
                                }
                            }
                        }
                        finally { Monitor.Exit(Content.SyncRoot); }
                    }
                    return -1L;
                }
                int IList<ContentMD5Group>.IndexOf(ContentMD5Group item)
                {
                    long index = IndexOf(item);
                    return (index > (long)(int.MaxValue)) ? -2 : (int)index;
                }
                int IList.IndexOf(object value)
                {
                    if (value != null && value is ContentMD5Group)
                    {
                        long index = IndexOf((ContentMD5Group)value);
                        return (index > (long)(int.MaxValue)) ? -2 : (int)index;
                    }
                    return -1;
                }
                void IList<ContentMD5Group>.Insert(int index, ContentMD5Group item) => throw new NotSupportedException();
                void IList.Insert(int index, object value) => throw new NotSupportedException();
                internal bool Remove(ContentMD5Group item)
                {
                    Monitor.Enter(Content.SyncRoot);
                    try
                    {
                        if (item._owner != null && ReferenceEquals(item._owner, this))
                        {
                            // List<ContentMD5Group> list = Content._innerList;
                            // for (int i = 0; i < list.Count; i++)
                            // {
                            //     if (ReferenceEquals(list[i], item))
                            //     {
                            //         list.RemoveAt(i);
                            //         return true;
                            //     }
                            // }
                            throw new NotImplementedException();
                        }
                    }
                    finally { Monitor.Exit(Content.SyncRoot); }
                    return false;
                }
                bool ICollection<ContentMD5Group>.Remove(ContentMD5Group item) => Remove(item);
                void IList.Remove(object value)
                {
                    if (value != null && value is ContentMD5Group)
                        Remove((ContentMD5Group)value);
                }
                internal void RemoveAt(long index)
                {
                    if (index >= 0)
                    {
                        Monitor.Enter(Content.SyncRoot);
                        try
                        {
                            // if (index < Content._md5Count)
                            // {
                            //     if (Content._md5Count == 1L)
                            //     {
                            //         Content._firstMD5Node = Content._lastMD5Node = null;
                            //         Content._md5Count = 0;
                            //         return;
                            //     }
                            //     if (index == 0L)
                            //     {
                            //         Content._firstMD5Node = Content._firstMD5Node._nextNode;
                            //         Content._md5Count--;
                            //         return;
                            //     }
                            //     ContentMD5Group node = Content._firstMD5Node;
                            //     for (long i = 1L; i < index; i++)
                            //     {
                            //         if (node._nextNode == null)
                            //             break;
                            //         node = node._nextNode;
                            //     }
                            //     if (node._nextNode != null)
                            //     {
                            //         if (node._nextNode._nextNode == null)
                            //             Content._lastMD5Node
                            //         return;
                            //     }
                            // }
                            throw new NotImplementedException();
                        }
                        finally { Monitor.Exit(Content.SyncRoot); }
                    }
                    throw new ArgumentOutOfRangeException("index");
                }
                void IList<ContentMD5Group>.RemoveAt(int index) => RemoveAt(index);
                void IList.RemoveAt(int index) => RemoveAt(index);

                class Enumerator : IEnumerator<ContentMD5Group>
                {
                    private ContentLengthGroup _content;
                    private ContentMD5Group _current = null;
                    private ContentMD5Group _firstMD5Node;
                    private ContentMD5Group _lastMD5Node;
                    private int _md5Count;
                    public ContentMD5Group Current => _current;
                    object IEnumerator.Current => _current;
                    internal Enumerator(ContentLengthGroup content)
                    {
                        _content = content;
                        Monitor.Enter(content.SyncRoot);
                        try
                        {
                            _firstMD5Node = content._firstMD5Node;
                            _lastMD5Node = content._lastMD5Node;
                            // md5Count = content._md5Count;
                            throw new NotImplementedException();
                        }
                        finally { Monitor.Exit(content.SyncRoot); }
                    }
                    public const string ErrorMessage_ListModified = "List was modified";
                    public bool MoveNext()
                    {
                        ContentLengthGroup content = _content;
                        if (content == null)
                            throw new ObjectDisposedException(this.GetType().FullName);
                        Monitor.Enter(content.SyncRoot);
                        try
                        {
                            if (_firstMD5Node == null)
                            {
                                if (content._firstMD5Node == null)
                                    return false;
                            }
                            else if (content._md5Count == _md5Count && content._firstMD5Node != null && ReferenceEquals(content._firstMD5Node, _firstMD5Node) && ReferenceEquals(content._lastMD5Node, _lastMD5Node))
                            {
                                ContentMD5Group node = Current;
                                if (node == null)
                                {
                                    _current = _firstMD5Node;
                                    return true;
                                }
                                else if ((node = node._nextNode) != null)
                                {
                                    _current = node;
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                        finally { Monitor.Exit(content.SyncRoot); }

                        throw new InvalidOperationException(ErrorMessage_ListModified);
                    }
                    public void Reset()
                    {
                        ContentLengthGroup content = _content;
                        if (content == null)
                            throw new ObjectDisposedException(this.GetType().FullName);
                        Monitor.Enter(content.SyncRoot);
                        try
                        {
                            if ((_firstMD5Node == null) ? content._firstMD5Node == null : (content._md5Count == _md5Count && content._firstMD5Node != null &&
                                ReferenceEquals(content._firstMD5Node, _firstMD5Node) && ReferenceEquals(content._lastMD5Node, _lastMD5Node)))
                            {
                                _current = null;
                                _firstMD5Node = content._firstMD5Node;
                                _lastMD5Node = content._lastMD5Node;
                                // _md5Count = content._md5Count;
                                // return;
                                throw new NotImplementedException();
                            }
                        }
                        finally { Monitor.Exit(content.SyncRoot); }

                        throw new InvalidOperationException(ErrorMessage_ListModified);
                    }
                    protected virtual void Dispose(bool disposing) => _content = null;
                    public void Dispose() => Dispose(true);
                }
            }
        }
    }
}