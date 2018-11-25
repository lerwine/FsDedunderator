using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public abstract partial class LongLinkedList<TParent, TNode>
    {
        private TNode _firstNode = null;
        private TNode _lastNode = null;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public long Count { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public class NodeLink
        {
            internal object SyncRoot { get; } = new object();
            private TNode _nextNode = null;
            private TParent _parent = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            protected NodeLink(TParent parent)
            {
                if (this.GetType().AssemblyQualifiedName != typeof(TNode).AssemblyQualifiedName)
                    throw new InvalidOperationException("Invalid node inheritance/type");
                _parent = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            protected virtual void OnRemoving(long index) { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="oldList"></param>
            /// <param name="oldIndex"></param>
            protected virtual void OnRemoved(TParent oldList, long oldIndex) { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="list"></param>
            /// <param name="index"></param>
            protected virtual void OnAdding(TParent list, long index) { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            protected virtual void OnAdded(long index) { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="action"></param>
            protected void InvokeInMonitor(Action action)
            {
                Monitor.Enter(SyncRoot);
                try { action(); }
                finally { Monitor.Exit(SyncRoot); }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="func"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            protected T GetInMonitor<T>(Func<T> func)
            {
                Monitor.Enter(SyncRoot);
                try { return func(); }
                finally { Monitor.Exit(SyncRoot); }
            }
            
            internal static TNode Get(TParent list, long index)
            {
                if (index < 0L)
                    throw new IndexOutOfRangeException();
                return list.GetInMonitor(() =>
                {
                    if (index < list.Count)
                    {
                        TNode node = list._firstNode;
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
            internal static void Set(TParent list, long index, TNode value)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0L)
                    throw new IndexOutOfRangeException();
                if (value == null)
                    throw new ArgumentNullException();
                list.InvokeInMonitor(() =>
                {
                    if (index < list.Count)
                    {
                        TNode previousNode = null;
                        TNode currentNode = list._firstNode;
                        for (long i = 0; i < index; i++)
                        {
                            if (currentNode == null)
                                break;
                            previousNode = currentNode;
                            currentNode = currentNode._nextNode;
                        }
                        if (currentNode != null)
                        {
                            value.InvokeInMonitor(() =>
                            {
                                if (value._parent != null)
                                {
                                    if (!ReferenceEquals(value._parent, list))
                                        throw new ArgumentOutOfRangeException("value", "Item has been added to another list.");
                                    if (value._nextNode != null || (list._firstNode != null && ReferenceEquals(value, list._lastNode)))
                                    {
                                        if (ReferenceEquals(value, currentNode))
                                            return;
                                        throw new ArgumentOutOfRangeException("value", "Item already exists in the current list.");
                                    }
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
                                    value._parent = list;
                                    currentNode._parent = null;
                                });
                            });
                            RaiseItemReplaced(list, currentNode, value, index);
                            return;
                        }
                    }
                    throw new IndexOutOfRangeException();
                });
            }
            internal static void Add(TParent list, TNode item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (item == null)
                    throw new ArgumentNullException("item");
                list.InvokeInMonitor(() => RaiseItemAdded(list, item, item.GetInMonitor(() =>
                {
                    if (item._parent != null)
                    {
                        if (!ReferenceEquals(item._parent, list))
                            throw new ArgumentOutOfRangeException("item", "Item has been added to another list.");
                        if (item._nextNode != null || (list._firstNode != null && ReferenceEquals(item, list._lastNode)))
                            throw new ArgumentOutOfRangeException("item", "Item already exists in the current list.");
                    }
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
                        item._parent = list;
                        return i;
                    });
                })));
            }
            internal static void Clear(TParent list)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                list.InvokeInMonitor(() =>
                {
                    if (list._firstNode == null)
                        return;
                    
                    foreach (var removed in list.GetInChange(() =>
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
                            i.Item._parent = null;
                        }
                        return r;
                    }))
                        RaiseItemRemoved(list, removed.Item, removed.Index);
                });
            }
            internal static bool Contains(TParent list, TNode item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (item == null)
                    return false;
                return item.GetInMonitor(() => item._parent != null && ReferenceEquals(item._parent, list) && (item._nextNode != null || list.GetInMonitor(() => list.Count > 0 && ReferenceEquals(list._lastNode, item))));
            }
            internal static void CopyTo(TParent list, Array array, int arrayIndex)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (array == null)
                    throw new ArgumentNullException("array");
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException("arrayIndex");
                list.InvokeInMonitor(() =>
                {
                    if (list.Count + (long)arrayIndex > array.LongLength)
                        throw new ArgumentException("Not enough room at end of array", "arrayIndex");
                    long index = (long)arrayIndex - 1L;
                    for (TNode node = list._firstNode; node != null; node = node._nextNode)
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
            internal static long IndexOf(TParent list, TNode item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                return list.GetInMonitor(() =>
                {
                    if (item != null && item.GetInMonitor(() => item._parent != null && ReferenceEquals(item._parent, list)))
                    {
                        if (item._nextNode == null)
                        {
                            if (list.Count > 0L && ReferenceEquals(item, list._lastNode))
                                return list.Count - 1;
                        }
                        else
                        {
                            long index = -1L;
                            for (TNode node = list._firstNode; node != null; node = node._nextNode)
                            {
                                index++;
                                if (ReferenceEquals(node, item))
                                    return index;
                            }
                        }
                    }
                    return -1L;
                });
            }
            internal static void Insert(TParent list, long index, TNode item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0L)
                    throw new ArgumentOutOfRangeException("index");
                if (item == null)
                    throw new ArgumentNullException("item");
                list.InvokeInMonitor(() =>
                {
                    if (index > list.Count)
                        throw new ArgumentOutOfRangeException("index");
                    item.InvokeInMonitor(() =>
                    {
                        if (item._parent != null)
                        {
                            if (!ReferenceEquals(item._parent, list))
                                throw new ArgumentOutOfRangeException("item", "Item has been added to another list.");
                            if (item._nextNode != null || (list._firstNode != null && ReferenceEquals(item, list._lastNode)))
                                throw new ArgumentOutOfRangeException("item", "Item already exists in the current list.");
                        }
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
                                TNode previousItem = list._firstNode;
                                for (long i = 1L; i < index; i++)
                                {
                                    if ((previousItem = previousItem._nextNode) == null)
                                    throw new ArgumentOutOfRangeException("index");
                                }
                                item._nextNode = previousItem._nextNode;
                                previousItem._nextNode = item;
                            }
                            item._parent = list;
                        });
                    });
                    RaiseItemAdded(list, item, index);
                });
            }
            internal static bool Remove(TParent list, TNode item)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (item == null)
                    return false;
                return list.GetInMonitor(() =>
                {
                    if (list.Count == 0)
                        return false;
                    if (item.GetInMonitor(() => item._parent == null || !ReferenceEquals(item._parent, list)))
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
                            item._parent = null;
                            item._nextNode = null;
                        });
                        RaiseItemRemoved(list, item, 0L);
                        return true;
                    }
                    long index = 0L;
                    TNode previousItem = list._firstNode;
                    for (TNode node = previousItem._nextNode; node != null; node = node._nextNode)
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
            internal static void RemoveAt(TParent list, long index)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");
                list.InvokeInMonitor(() =>
                {
                    if (index >= list.Count)
                        throw new ArgumentOutOfRangeException("index");
                    TNode item = list._firstNode;
                    TNode previousItem = null;
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
                        item._parent = null;
                        item._nextNode = null;
                    });
                    RaiseItemRemoved(list, item, index);
                });
            }
            private static void RaiseReplacingItem(TParent list, TNode currentNode, TNode newNode, long index)
            {
                try { list.OnReplacingItem(currentNode, newNode, index); }
                finally
                {
                    try { RaiseRemovingItem(list, currentNode, index); }
                    finally { RaiseAddingItem(list, newNode, index); }
                }
            }
            private static void RaiseItemReplaced(TParent list, TNode oldNode, TNode currentNode, long index)
            {
                try { RaiseItemAdded(list, currentNode, index); }
                finally
                {
                    try { RaiseItemRemoved(list, oldNode, index); }
                    finally { list.OnItemReplaced(oldNode, currentNode, index); }
                }
            }
            private static void RaiseAddingItem(TParent list, TNode newNode, long index)
            {
                try { list.OnAddingItem(newNode, index); }
                finally { newNode.OnAdding(list, index); }
            }
            private static void RaiseRemovingItem(TParent list, TNode currentNode, long index)
            {
                try { list.OnRemovingItem(currentNode, index); }
                finally { currentNode.OnRemoving(index); }
            }
            private static void RaiseItemAdded(TParent list, TNode currentNode, long index)
            {
                try { list.OnItemAdded(currentNode, index); }
                finally { currentNode.OnAdded(index); }
            }
            private static void RaiseItemRemoved(TParent list, TNode oldNode, long index)
            {
                try { list.OnItemRemoved(oldNode, index); }
                finally { oldNode.OnRemoved(list, index); }
            }

            /// <summary>
            /// 
            /// </summary>
            public class Enumerator : IEnumerator<TNode>
            {
                private TParent _list;
                private TNode _firstNode;
                private TNode _lastNode;
                private long _count;

                /// <summary>
                /// 
                /// </summary>
                public long Index { get; private set; }

                /// <summary>
                /// 
                /// </summary>
                public TNode Current { get; private set; }
                object IEnumerator.Current => Current;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="list"></param>
                public Enumerator(TParent list)
                {
                    if (list == null)
                        throw new ArgumentNullException("list");
                    Index = -1;
                    _list = list;
                    list.InvokeInMonitor(() =>
                    {
                        _firstNode = list._firstNode;
                        _lastNode = list._lastNode;
                        _count = list.Count;
                    });
                }

                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public bool MoveNext()
                {
                    TParent list = _list;
                    if (list == null)
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    return list.GetInMonitor(() =>
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
                            TNode node = Current._nextNode;
                            if (node == null)
                                return false;
                            Current = node;
                            Index++;
                        }
                        return true;
                    });
                }

                /// <summary>
                /// 
                /// </summary>
                public void Reset()
                {
                    TParent list = _list;
                    if (list == null)
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    list.InvokeInMonitor(() =>
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
                    TParent list = _list;
                    if (list != null && disposing)
                        list.InvokeInMonitor(() => _list = null);
                }

                /// <summary>
                /// 
                /// </summary>
                public void Dispose() => Dispose(true);
            }
        }
    }
}