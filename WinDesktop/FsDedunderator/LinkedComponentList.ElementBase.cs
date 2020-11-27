using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public abstract partial class LinkedComponentList<TList, TElement>
    {
        private TElement _firstNode = null;
        private TElement _lastNode = null;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="LinkedComponentList{TList, TElement}">.
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// Base class which synchronizes relationship between <typeparamref name"TElement" />s and <typeparamref name"TList" />s.
        /// </summary>
        public class ElementBase
        {
            #region Fields

            /// <summary>
            /// Error message used for exceptions when an item is attempted to be added when it is already contained in the list.
            /// </summary>
            public const string ErrorMessage_ItemExistsInCurrent = "Item already exists in the current list";

            /// <summary>
            /// Error message used for exceptions when an item is attempted to be added to list when it has already been added to another list.
            /// </summary>
            public const string ErrorMessage_ItemExistsInOther = "Item has been added to another list";

            private TElement _nextNode = null;
            private TList _container = null;

            #endregion

            /// <summary>
            /// 
            /// </summary>
            protected internal TList Container => _container;

            internal object SyncRoot { get; } = new object();

            /// <summary>
            /// Initializes a new <see cref="ElementBase" /> that does ot belong to a <typeparamref name="TList" />.
            /// </summary>
            protected ElementBase()
            {
                if (this.GetType().AssemblyQualifiedName != typeof(TElement).AssemblyQualifiedName)
                    throw new InvalidOperationException("Invalid node inheritance/type");
            }

            #region List Access Methods

            internal static void Clear(TList list)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                list.InvokeInMonitorLock(() =>
                {
                    if (list._firstNode == null)
                        return;
                    
                    WithAll(list.GetInChange(() =>
                    {
                        var r = list.Select((node, index) =>
                        {
                            RaiseRemovingItem(list, node, index);
                            return new { Item = node, Index = index };
                        }).ToArray();
                        list._firstNode = list._lastNode = null;
                        list.Count = 0L;
                        foreach (var i in r)
                        {
                            i.Item._nextNode = null;
                            i.Item._container = null;
                        }
                        return r;
                    }), a => RaiseItemRemoved(list, a.Item, a.Index));
                });
            }

            internal static bool Contains(TList list, TElement item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                return item != null && item.GetInMonitorLock(() => item._container != null && ReferenceEquals(item._container, list));
            }

            internal static void CopyTo(TList list, Array array, int arrayIndex)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (array == null)
                    throw new ArgumentNullException("array");
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException("arrayIndex");
                list.InvokeInMonitorLock(() =>
                {
                    if (list.Count + (long)arrayIndex > array.LongLength)
                        throw new ArgumentException("Not enough room at end of array", "arrayIndex");
                    long index = (long)arrayIndex - 1L;
                    for (TElement node = list._firstNode; node != null; node = node._nextNode)
                    {
                        try
                        {
                            index++;
                            array.SetValue(node, index);
                        }
                        catch (InvalidCastException exception) { throw new ArgumentException(exception.Message, "array", exception); }
                        catch (ArgumentException exception)
                        {
                            if (list.Count + (long)arrayIndex > array.LongLength)
                                throw new ArgumentException("Not enough room at end of array", "arrayIndex", exception);
                            if (exception.ParamName != null && exception.ParamName == "index")
                                throw new ArgumentException(exception.Message, "arrayIndex", exception);
                            throw new ArgumentException(exception.Message, "array", exception);
                        }
                        catch (Exception exception)
                        {
                            if (list.Count + (long)arrayIndex > array.LongLength)
                                throw new ArgumentException("Not enough room at end of array", "arrayIndex");
                            throw new ArgumentException(exception.Message, "array", exception);
                        }
                    }
                });
            }

            internal static TElement Get(TList list, long index)
            {
                if (index < 0L)
                    throw new IndexOutOfRangeException();
                return list.GetInMonitorLock(() =>
                {
                    if (index < list.Count)
                    {
                        TElement node = list._firstNode;
                        for (long i = 0; i < index; i++)
                        {
                            if (node == null)
                                break;
                            node = node._nextNode;
                        }
                        if (node != null)
                            return node;
                    }
                    throw new IndexOutOfRangeException();
                });
            }
            
            internal static long IndexOf(TList list, TElement item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                return list.GetInMonitorLock(() =>
                {
                    if (item != null && item.GetInMonitorLock(() => item._container != null && ReferenceEquals(item._container, list)))
                    {
                        if (item._nextNode == null)
                            return list.Count - 1;
                        long index = -1L;
                        for (TElement node = list._firstNode; node != null; node = node._nextNode)
                        {
                            index++;
                            if (ReferenceEquals(node, item))
                                return index;
                        }
                    }
                    return -1L;
                });
            }

            #endregion

            #region List Manipulation Methods
            
            internal static void Add(TList list, TElement item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (item == null)
                    throw new ArgumentNullException("item");
                list.InvokeInMonitorLock(() => RaiseItemAdded(list, item, item.GetInMonitorLock(() =>
                {
                    ValidateItemOrphaned(list, item);
                    return list.GetInChange(() =>
                    {
                        long i = list.Count;
                        RaiseAddingItem(list, item, i);
                        if (list._firstNode == null)
                        {
                            list._firstNode = list._lastNode = item;
                            list.Count = 1;
                        }
                        else
                        {
                            list._lastNode._nextNode = item;
                            list._lastNode = item;
                            list.Count++;
                        }
                        item._container = list;
                        return i;
                    });
                })));
            }
            
            internal static void Insert(TList list, long index, TElement item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0L)
                    throw new ArgumentOutOfRangeException("index");
                if (item == null)
                    throw new ArgumentNullException("item");
                list.InvokeInMonitorLock(() =>
                {
                    if (index > list.Count)
                        throw new ArgumentOutOfRangeException("index");
                    item.InvokeInMonitorLock(() =>
                    {
                        ValidateItemOrphaned(list, item);
                        list.InvokeInChange(() =>
                        {
                            RaiseAddingItem(list, item, index);
                            if (index == list.Count)
                            {
                                if (index == 0L)
                                {
                                    list._firstNode = item;
                                    list.Count = 1L;
                                }
                                else
                                {
                                    list._lastNode._nextNode = item;
                                    list.Count++;
                                }
                                list._lastNode = item;
                            }
                            else if (index == 0L)
                            {
                                item._nextNode = list._firstNode;
                                list._firstNode = item;
                            }
                            else if (index == list.Count - 1)
                            {
                                list._lastNode._nextNode = item;
                                list._lastNode = item;
                            }
                            else
                            {
                                TElement previousItem = list._firstNode;
                                for (long i = 1L; i < index; i++)
                                {
                                    if ((previousItem = previousItem._nextNode) == null)
                                    throw new ArgumentOutOfRangeException("index");
                                }
                                item._nextNode = previousItem._nextNode;
                                previousItem._nextNode = item;
                            }
                            item._container = list;
                        });
                    });
                    RaiseItemAdded(list, item, index);
                });
            }

            internal static bool Remove(TList list, TElement item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (item == null)
                    return false;
                return list.GetInMonitorLock(() =>
                {
                    if (list.Count == 0)
                        return false;
                    if (item.GetInMonitorLock(() => item._container == null || !ReferenceEquals(item._container, list)))
                        return false;
                    if (ReferenceEquals(list._firstNode, item))
                    {
                        list.InvokeInChange(() =>
                        {
                            RaiseRemovingItem(list, item, 0L);
                            list._firstNode = item._nextNode;
                            if (list._firstNode == null)
                            {
                                list.Count = 0L;
                                list._lastNode = null;
                            }
                            else if (list._firstNode._nextNode == null)
                            {
                                list.Count = 1L;
                                list._lastNode = list._firstNode;
                            }
                            else
                                list.Count--;
                            item._container = null;
                            item._nextNode = null;
                        });
                        RaiseItemRemoved(list, item, 0L);
                        return true;
                    }
                    long index = 0L;
                    TElement previousItem = list._firstNode;
                    for (TElement node = previousItem._nextNode; node != null; node = node._nextNode)
                    {
                        index++;
                        if (ReferenceEquals(node, item))
                        {
                            list.InvokeInChange(() =>
                            {
                                RaiseRemovingItem(list, item, index);
                                previousItem._nextNode = item._nextNode;
                                if (previousItem._nextNode == null)
                                    list._lastNode = previousItem;
                            });
                            RaiseItemRemoved(list, item, index);
                            return true;
                        }
                        previousItem = node;
                    }
                    return false;
                });
            }

            internal static void RemoveAt(TList list, long index)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");
                list.InvokeInMonitorLock(() =>
                {
                    if (index >= list.Count)
                        throw new ArgumentOutOfRangeException("index");
                    TElement item = list._firstNode;
                    TElement previousItem = null;
                    for (long i = 0L; i < index; i++)
                    {
                        previousItem = item;
                        if ((item = item._nextNode) == null)
                            throw new ArgumentOutOfRangeException("index");
                    }
                    list.InvokeInChange(() =>
                    {
                        RaiseRemovingItem(list, item, index);
                        if (index == 0)
                        {
                            if (list.Count == 1)
                            {
                                list._firstNode = list._lastNode = null;
                                list.Count = 0;
                            }
                            else if ((list._firstNode = item._nextNode)._nextNode == null)
                            {
                                list._lastNode = list._firstNode;
                                list.Count = 1;
                            }
                            else
                                list.Count--;
                        }
                        else
                        {
                            if ((previousItem._nextNode = item._nextNode)._nextNode == null)
                                list._lastNode = previousItem;
                            list.Count--;
                        }
                        item._container = null;
                        item._nextNode = null;
                    });
                    RaiseItemRemoved(list, item, index);
                });
            }

            internal static void Set(TList list, long index, TElement value)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0L)
                    throw new IndexOutOfRangeException();
                if (value == null)
                    throw new ArgumentNullException();
                list.InvokeInMonitorLock(() =>
                {
                    if (index < list.Count)
                    {
                        TElement previousNode = null;
                        TElement currentNode = list._firstNode;
                        for (long i = 0; i < index; i++)
                        {
                            if (currentNode == null)
                                break;
                            previousNode = currentNode;
                            currentNode = currentNode._nextNode;
                        }
                        if (currentNode != null)
                        {
                            value.InvokeInMonitorLock(() =>
                            {
                                if (value._container != null)
                                {
                                    if (ReferenceEquals(value._container, list))
                                    {
                                        if (ReferenceEquals(value, currentNode))
                                            return;
                                        throw new ArgumentOutOfRangeException("value", ErrorMessage_ItemExistsInCurrent);
                                    }
                                    throw new ArgumentOutOfRangeException("value", ErrorMessage_ItemExistsInCurrent);
                                }
                                list.InvokeInChange(() =>
                                {
                                    RaiseReplacingItem(list, currentNode, value, index);
                                    value._nextNode = currentNode._nextNode;
                                    if (previousNode == null)
                                        list._firstNode = value;
                                    else
                                        previousNode._nextNode = value;
                                    if (value._nextNode == null)
                                        list._lastNode = value;
                                    value._container = list;
                                    currentNode._container = null;
                                });
                            });
                            RaiseItemReplaced(list, currentNode, value, index);
                            return;
                        }
                    }
                    throw new IndexOutOfRangeException();
                });
            }

            #endregion

            #region Utility Methods

            /// <summary>
            /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="ElementBase" /> while invoking a <seealso cref="Func{T}" /> delegate and returning the result.
            /// </summary>
            /// <param name="func">Delegate function to invoke.</param>
            /// <typeparam name="T">Type of value returned from the delegate function.</typeparam>
            /// <returns>The value returned by the delegate function.</returns>
            protected T GetInMonitorLock<T>(Func<T> func)
            {
                Monitor.Enter(SyncRoot);
                try { return func(); }
                finally { Monitor.Exit(SyncRoot); }
            }

            /// <summary>
            /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="ElementBase" /> while invoking an <seealso cref="Action" /> delegate method.
            /// </summary>
            /// <param name="action">The delegate method to invoke.</param>
            protected void InvokeInMonitorLock(Action action)
            {
                Monitor.Enter(SyncRoot);
                try { action(); }
                finally { Monitor.Exit(SyncRoot); }
            }

            private static void ValidateItemOrphaned(TList list, TElement item, string paramName = "item")
            {
                if (item._container == null)
                    return;

                if (ReferenceEquals(item._container, list))
                    throw new ArgumentOutOfRangeException(paramName, ErrorMessage_ItemExistsInCurrent);
                throw new ArgumentOutOfRangeException(paramName, ErrorMessage_ItemExistsInCurrent);
            }
            
            internal static void WithAll(params Action[] action)
            {
                if (action == null)
                    return;
                Exception[] exceptions = action.Where(a => a != null).Select(a =>
                {
                    try { a(); }
                    catch (Exception e) { return e; }
                    return null;
                }).Where(e => e != null).ToArray();
                if (exceptions.Length == 0)
                    return;
                if (exceptions.Length == 1)
                    throw new Exception(exceptions[0].Message, exceptions[0]);
                throw new AggregateException(exceptions);
            }

            internal static void WithAll<T>(IEnumerable<T> source, Action<T> action)
            {
                Exception[] exceptions = source.Select(i =>
                {
                    try { action(i); }
                    catch (Exception e) { return e; }
                    return null;
                }).Where(e => e != null).ToArray();
                if (exceptions.Length == 0)
                    return;
                if (exceptions.Length == 1)
                    throw new Exception(exceptions[0].Message, exceptions[0]);
                throw new AggregateException(exceptions);
            }

            #endregion

            #region  Event Overridables

            /// <summary>
            /// This gets called before the current <see cref="ElementBase" /> is removed from its parent <typeparamref name="TList" />.
            /// </summary>
            /// <param name="index">The zero-based index of the current <see cref="ElementBase" />.</param>
            /// <remarks>While this method is being invoked, no items should be added to or removed from the parent <typeparamref name="TList" />.</remarks>
            protected virtual void OnRemoving(long index) { }

            /// <summary>
            /// This gets called after the current <see cref="ElementBase" /> has been removed from its parent <typeparamref name="TList" />.
            /// </summary>
            /// <param name="oldList">The list from which the current <see cref="ElementBase" /> was removed.</param>
            /// <param name="oldIndex">The zero-based index where the current <see cref="ElementBase" /> was removed from.</param>
            protected virtual void OnRemoved(TList oldList, long oldIndex) { }

            /// <summary>
            /// This gets called before the current <see cref="ElementBase" /> is added to a <typeparamref name="TList" />.
            /// </summary>
            /// <param name="list">The <typeparamref name="TList" /> to which the current <see cref="ElementBase" />  will be added.</param>
            /// <param name="index">The zero-based index where the current <see cref="ElementBase" /> will be added or inserted.</param>
            /// <remarks>While this method is being invoked, no items should be added to or removed from the parent <typeparamref name="TList" />.</remarks>
            protected virtual void OnAdding(TList list, long index) { }

            /// <summary>
            /// This gets called after the current <see cref="ElementBase" /> has been added to a <typeparamref name="TList" />.
            /// </summary>
            /// <param name="index">The zero-based index where the current <see cref="ElementBase" /> was added.</param>
            protected virtual void OnAdded(long index) { }

            #endregion

            #region Raise Event Methods

            private static void RaiseReplacingItem(TList list, TElement currentNode, TElement newNode, long index)
            {
                WithAll(
                    () => list.OnReplacingItem(currentNode, newNode, index),
                    () => RaiseRemovingItem(list, currentNode, index),
                    () => RaiseAddingItem(list, newNode, index)
                );
            }

            private static void RaiseItemReplaced(TList list, TElement oldNode, TElement currentNode, long index)
            {
                WithAll(
                    () => RaiseItemAdded(list, currentNode, index),
                    () => RaiseItemRemoved(list, oldNode, index),
                    () => list.OnItemReplaced(oldNode, currentNode, index)
                );
            }

            private static void RaiseAddingItem(TList list, TElement newNode, long index)
            {
                WithAll(
                    () => list.OnAddingItem(newNode, index),
                    () => newNode.OnAdding(list, index)
                );
            }

            private static void RaiseRemovingItem(TList list, TElement currentNode, long index)
            {
                WithAll(
                    () => list.OnRemovingItem(currentNode, index),
                    () => currentNode.OnRemoving(index)
                );
            }

            private static void RaiseItemAdded(TList list, TElement currentNode, long index)
            {
                WithAll(
                    () => list.OnItemAdded(currentNode, index),
                    () => currentNode.OnAdded(index)
                );
            }

            private static void RaiseItemRemoved(TList list, TElement oldNode, long index)
            {
                WithAll(
                    () => list.OnItemRemoved(oldNode, index),
                    () => oldNode.OnRemoved(list, index)
                );
            }

            #endregion

            /// <summary>
            /// Iterates over a <typeparamref name="TList" />.
            /// </summary>
            public class Enumerator : IEnumerator<TElement>
            {
                private TList _list;
                private TElement _firstNode;
                private TElement _lastNode;
                private long _count;

                /// <summary>
                /// The zero-based index of the <seealso cref="Current" /> element.
                /// </summary>
                public long Index { get; private set; }

                /// <summary>
                /// Gets the element in the <typeparamref name="TList" /> at the current position (<seealso cref="Index" />) of the enumerator.
                /// </summary>
                public TElement Current { get; private set; }

                object IEnumerator.Current => Current;

                /// <summary>
                /// Initializes a new <see cref="Enumerator" /> to iterate over the elements of a <typeparamref name="TList" />.
                /// </summary>
                /// <param name="list">The <typeparamref name="TList" /> whose elements are to be iterated.</param>
                public Enumerator(TList list)
                {
                    if (list == null)
                        throw new ArgumentNullException("list");
                    Index = -1;
                    _list = list;
                    list.InvokeInMonitorLock(() =>
                    {
                        _firstNode = list._firstNode;
                        _lastNode = list._lastNode;
                        _count = list.Count;
                    });
                }

                /// <summary>
                /// Advances the enumerator to the next <typeparamref name="TElement" /> of the <typeparamref name="TList" />.
                /// </summary>
                /// <returns><c>true</c> if the enumerator was successfully advanced to the next <typeparamref name="TElement" /> ;
                /// otherwise, <c>false</c> if the enumerator has passed the end of the typeparamref name="TList" />.</returns>
                /// <exception cref="InvalidOperationException">The <typeparamref name="TList" /> was modified after the enumerator was created or last <seealso cref="Reset">.</exception>
                /// <exception cref="ObjectDisposedException">The current enumerator or the <typeparamref name="TList" /> being iterated has been <sealso cref="Dispose" />d.</exception>
                public bool MoveNext()
                {
                    TList list = _list;
                    if (list == null)
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    return list.GetInMonitorLock(() =>
                    {
                        if (_list == null)
                            throw new ObjectDisposedException(typeof(Enumerator).FullName);
                        if (_count != list.Count || (_count > 0 && !(ReferenceEquals(_firstNode, list._firstNode) && ReferenceEquals(_lastNode, list._lastNode))))
                            throw new InvalidOperationException("List items have changed");
                        if (Index < 0)
                        {
                            if (_firstNode == null)
                                return false;
                            Current = _firstNode;
                            Index = 0;
                        }
                        else
                        {
                            TElement node = Current._nextNode;
                            if (node == null)
                                return false;
                            Current = node;
                            Index++;
                        }
                        return true;
                    });
                }

                /// <summary>
                /// Sets the enumerator to its initial position, which is before the first element in the <typeparamref name="TList" />.
                /// </summary>
                /// <exception cref="ObjectDisposedException">The current enumerator or the <typeparamref name="TList" /> being iterated has been <sealso cref="Dispose" />d.</exception>
                public void Reset()
                {
                    TList list = _list;
                    if (list == null)
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    list.InvokeInMonitorLock(() =>
                    {
                        if (_list == null)
                            throw new ObjectDisposedException(typeof(Enumerator).FullName);
                        Current = null;
                        Index = -1;
                        _firstNode = _list._firstNode;
                        _lastNode = _list._lastNode;
                        _count = _list.Count;
                    });
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="disposing"></param>
                protected virtual void Dispose(bool disposing)
                {
                    TList list = _list;
                    if (list != null && disposing)
                        list.InvokeInMonitorLock(() => _list = null);
                }

                /// <summary>
                /// 
                /// </summary>
                public void Dispose() => Dispose(true);
            }
        }
    }
}