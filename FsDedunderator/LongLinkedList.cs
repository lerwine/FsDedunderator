using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    public abstract partial class LongLinkedList<TParent, TNode> : IList<TNode>, IList, IDisposable
        where TNode : LongLinkedNode<TNode, TParent>
        where TParent : LongLinkedList<TParent, TNode>
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ErrorMessage_ChangeInProgress = "List contents cannot be changed while another change is already in progress";

        private ManualResetEvent _collectionChangableEvent = new ManualResetEvent(true);
        internal object SyncRoot { get; } = new object();

        /// <summary>
        /// 
        /// </summary>
        public TNode this[long index]
        {
            get => NodeLink.Get((TParent)this, index);
            protected set => NodeLink.Set((TParent)this, index, value);
        }

        /// <summary>
        /// 
        /// </summary>
        protected int Count32
        {
            get
            {
                long count = Count;
                return (count > (long)(int.MaxValue)) ? int.MaxValue : (int)count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsCollectionChanging() => GetInMonitor(() => !_collectionChangableEvent.WaitOne(0));

        /// <summary>
        /// 
        /// </summary>
        protected LongLinkedList()
        {
            if (this.GetType().AssemblyQualifiedName != typeof(TParent).AssemblyQualifiedName)
                throw new InvalidOperationException("Invalid node inheritance/type");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        protected void Add(TNode item) => NodeLink.Add((TParent)this, item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="seed"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult Aggregate<TResult>(Func<TResult, TNode, long, TResult> func, TResult seed)
        {
            using (NodeLink.Enumerator enumerator = new NodeLink.Enumerator((TParent)this))
            {
                while (enumerator.MoveNext())
                    seed = func(seed, enumerator.Current, enumerator.Index);
            }
            return seed;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Clear() => NodeLink.Clear((TParent)this);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(TNode item) => NodeLink.Contains((TParent)this, item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(TNode[] array, int arrayIndex) => NodeLink.CopyTo((TParent)this, array, arrayIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<TNode, long> action)
        {
            using (NodeLink.Enumerator enumerator = new NodeLink.Enumerator((TParent)this))
            {
                while (enumerator.MoveNext())
                    action(enumerator.Current, enumerator.Index);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TNode> GetEnumerator() => new NodeLink.Enumerator((TParent)this);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public long IndexOf(TNode item) => NodeLink.IndexOf((TParent)this, item);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected int IndexOf32(TNode item)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected void Insert(long index, TNode item) => NodeLink.Insert((TParent)this, index, item);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual bool Remove(TNode item) => NodeLink.Remove((TParent)this, item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        protected virtual void RemoveAt(long index) => NodeLink.RemoveAt((TParent)this, index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public IEnumerable<TResult> Select<TResult>(Func<TNode, long, TResult> func)
        {
            using (NodeLink.Enumerator enumerator = new NodeLink.Enumerator((TParent)this))
            {
                while (enumerator.MoveNext())
                    yield return func(enumerator.Current, enumerator.Index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        protected bool WaitCollectionChangable(int millisecondsTimeout) => GetInMonitor(() => _collectionChangableEvent.WaitOne(millisecondsTimeout));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                ManualResetEvent collectionChangableEvent = _collectionChangableEvent;
                _collectionChangableEvent = null;
                if (collectionChangableEvent == null || !disposing)
                    return;
                try
                {
                    if (!collectionChangableEvent.WaitOne(0))
                        collectionChangableEvent.Set();
                }
                finally { collectionChangableEvent.Dispose(); }
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() => Dispose(true);
        private void InvokeInChange<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2) => InvokeInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(arg1, arg2); }
            finally { _collectionChangableEvent.Set(); }
        });
        private void InvokeInChange<T>(Action<T> action, T arg) => InvokeInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(arg); }
            finally { _collectionChangableEvent.Set(); }
        });
        private void InvokeInChange(Action action) => InvokeInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(); }
            finally { _collectionChangableEvent.Set(); }
        });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        protected void InvokeInMonitor<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                action(arg1, arg2);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        /// <typeparam name="T"></typeparam>
        protected void InvokeInMonitor<T>(Action<T> action, T arg)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                action(arg);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        protected void InvokeInMonitor(Action action)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                action();
            }
            finally { Monitor.Exit(SyncRoot); }
        }
        private TResult GetInChange<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2) => GetInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(arg1, arg2); }
            finally { _collectionChangableEvent.Set(); }
        });
        private TResult GetInChange<TArg, TResult>(Func<TArg, TResult> func, TArg arg) => GetInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(arg); }
            finally { _collectionChangableEvent.Set(); }
        });
        private T GetInChange<T>(Func<T> func) => GetInMonitor(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(); }
            finally { _collectionChangableEvent.Set(); }
        });
        private TResult GetInMonitor<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                return func(arg1, arg2);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="arg"></param>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        protected TResult GetInMonitor<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                return func(arg);
            }
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
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TParent).AssemblyQualifiedName);
                return func();
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="newNode"></param>
        /// <param name="index"></param>
        protected virtual void OnReplacingItem(TNode currentNode, TNode newNode, long index) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="index"></param>
        protected virtual void OnAddingItem(TNode newNode, long index) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="index"></param>
        protected virtual void OnRemovingItem(TNode currentNode, long index) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldNode"></param>
        /// <param name="currentNode"></param>
        /// <param name="index"></param>
        protected virtual void OnItemReplaced(TNode oldNode, TNode currentNode, long index) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="index"></param>
        protected virtual void OnItemAdded(TNode currentNode, long index) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldNode"></param>
        /// <param name="index"></param>
        protected virtual void OnItemRemoved(TNode oldNode, long index) { }

        #region Explicit Members

        TNode IList<TNode>.this[int index] { get => NodeLink.Get((TParent)this, index); set => throw new NotSupportedException(); }
        object IList.this[int index] { get => NodeLink.Get((TParent)this, index); set => throw new NotSupportedException(); }
        int ICollection<TNode>.Count => Count32;
        int ICollection.Count => Count32;
        bool ICollection<TNode>.IsReadOnly => true;
        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => SyncRoot;
        bool ICollection.IsSynchronized => true;
        void ICollection<TNode>.Add(TNode item) => throw new NotSupportedException();
        int IList.Add(object value) => throw new NotSupportedException();
        void ICollection<TNode>.Clear() => Clear();
        void IList.Clear() => Clear();
        bool IList.Contains(object value) => value != null && value is TNode && Contains((TNode)value);
        void ICollection.CopyTo(Array array, int index)
        {
            try { NodeLink.CopyTo((TParent)this, array, index); }
            catch (ArgumentOutOfRangeException exception)
            {
                if (exception.ParamName != null && exception.ParamName == "arrayIndex")
                    throw new ArgumentOutOfRangeException("index", index, exception.Message);
                throw new ArgumentOutOfRangeException("array", exception.Message);
            }
            catch (ArgumentNullException) { throw; }
            catch (ArgumentException exception)
            {
                if (exception.ParamName != null && exception.ParamName == "arrayIndex")
                    throw new ArgumentException(exception.Message, "index", exception);
                throw new ArgumentException(exception.Message, "array", exception);
            }
            catch (Exception exception) { throw new ArgumentException(exception.Message, "array", exception); }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        int IList<TNode>.IndexOf(TNode item) => IndexOf32(item);
        int IList.IndexOf(object value) => (value != null && value is TNode) ? IndexOf32((TNode)value) : -1;
        void IList<TNode>.Insert(int index, TNode item) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        bool ICollection<TNode>.Remove(TNode item) => Remove(item);
        void IList.Remove(object value)
        {
            if (value != null && value is TNode)
                Remove((TNode)value);
        }
        void IList<TNode>.RemoveAt(int index) => RemoveAt(index);
        void IList.RemoveAt(int index) => RemoveAt(index);

        #endregion
    }
}