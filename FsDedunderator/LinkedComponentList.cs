using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// Represents a list of <seealso cref="LinkedComponentElement{TElement, TList}" /> objects that are exclusively associated with the inherited list.
    /// </summary>
    /// <typeparam name="TList">The type of object that is inheriting from this class.</typeparam>
    /// <typeparam name="TElement">The type of objects that will be associated with this list.</typeparam>
    public abstract partial class LinkedComponentList<TList, TElement> : IList<TElement>, IList, IDisposable
        where TElement : LinkedComponentElement<TElement, TList>
        where TList : LinkedComponentList<TList, TElement>
    {
        /// <summary>
        /// Exception message that is used when a modification is attempted on a list before a change is complete.
        /// </summary>
        public const string ErrorMessage_ChangeInProgress = "List contents cannot be changed while another change is already in progress";

        private ManualResetEvent _collectionChangableEvent = new ManualResetEvent(true);

        internal object SyncRoot { get; } = new object();

        
        /// <summary>
        /// Gets or sets the <typeparamref name"TElement" /> at the specified index.
        /// </summary>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">value already belongs to the current list or to another list.</exception>
        public TElement this[long index]
        {
            get => ElementBase.Get((TList)this, index);
            protected set => ElementBase.Set((TList)this, index, value);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="LinkedComponentList{TList, TElement}" /> as a 32-bit value.
        /// </summary>
        /// <remarks>If the actual number of items exceeds <seealso cref="Int32.MaxValue" />, then <seealso cref="Int32.MaxValue" /> is returned.</remarks>
        protected int Count32
        {
            get
            {
                long count = Count;
                return (count > (long)(int.MaxValue)) ? int.MaxValue : (int)count;
            }
        }

        /// <summary>
        /// Determines whether a change to the current <see cref="LinkedComponentList{TList, TElement}" /> is in progress.
        /// </summary>
        /// <returns><c>true</c> if a change to the current <see cref="LinkedComponentList{TList, TElement}" /> is in progress; otherwise, <c>false</c>.</returns>
        protected bool IsCollectionChanging() => GetInMonitorLock(() => !_collectionChangableEvent.WaitOne(0));

        /// <summary>
        /// Initializes a new, empty <see cref="LinkedComponentList{TList, TElement}" />.
        /// </summary>
        protected LinkedComponentList()
        {
            if (this.GetType().AssemblyQualifiedName != typeof(TList).AssemblyQualifiedName)
                throw new InvalidOperationException("Invalid node inheritance/type");
        }

        /// <summary>
        /// Adds an item to the System.Collections.Generic.ICollection`1.
        /// </summary>
        /// <param name="item">The object to add to the System.Collections.Generic.ICollection`1.</param>
        /// <exception cref="ArgumentNullException">item is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">item already belongs to the current list or to another list.</exception>
        protected void Add(TElement item) => ElementBase.Add((TList)this, item);

        /// <summary>
        /// Applies an accumulator function over the list elements, incorporating the element's zero-based index. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element;
        /// the third parameter of the function represents the index of the element.</param>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="ArgumentNullException">func is null.</exception>
        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TElement, long, TAccumulate> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            using (ElementBase.Enumerator enumerator = new ElementBase.Enumerator((TList)this))
            {
                while (enumerator.MoveNext())
                    seed = func(seed, enumerator.Current, enumerator.Index);
            }
            return seed;
        }

        /// <summary>
        /// Removes all items from the System.Collections.Generic.ICollection`1.
        /// </summary>
        protected virtual void Clear() => ElementBase.Clear((TList)this);

        /// <summary>
        /// Determines whether the System.Collections.Generic.ICollection`1 contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the System.Collections.Generic.ICollection`1.</param>
        /// <returns>true if item is found in the System.Collections.Generic.ICollection`1; otherwise, false.</returns>
        public bool Contains(TElement item) => ElementBase.Contains((TList)this, item);

        /// <summary>
        /// Copies the elements of the System.Collections.Generic.ICollection`1 to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.Generic.ICollection`1. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source System.Collections.Generic.ICollection`1 is greater than the available space from arrayIndex to the end of the destination array.</exception>
        public void CopyTo(TElement[] array, int arrayIndex) => ElementBase.CopyTo((TList)this, array, arrayIndex);

        /// <summary>
        /// Invokes a delegate method on each of the list elements, incorporating the element's zero-based index.
        /// </summary>
        /// <param name="action">Delegate method to invoke on each of the list elements;
        /// the second parameter of the function represents the index of the element.</param>
        /// <exception cref="ArgumentNullException">action is null.</exception>
        public void ForEach(Action<TElement, long> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            using (ElementBase.Enumerator enumerator = new ElementBase.Enumerator((TList)this))
            {
                while (enumerator.MoveNext())
                    action(enumerator.Current, enumerator.Index);
            }
        }
        
        /// <summary>
        /// Returns an <seealso cref="ElementBase.Enumerator" /> that iterates through the collection.
        /// </summary>
        /// <returns>A <seealso cref="ElementBase.Enumerator" /> that can be used to iterate through the collection.</returns>
        public ElementBase.Enumerator GetEnumerator() => new ElementBase.Enumerator((TList)this);
        
        /// <summary>
        /// Determines the index of a specific <typeparamref name"TElement" /> in the <see cref="LinkedComponentList{TList, TElement}" />.
        /// </summary>
        /// <param name="item">The <typeparamref name"TElement" /> to locate in the <see cref="LinkedComponentList{TList, TElement}" />.</param>
        /// <returns>The zero-based index of <typeparamref name"TElement" /> if found in the <see cref="LinkedComponentList{TList, TElement}" />;
        /// otherwise, -1.</returns>
        public long IndexOf(TElement item) => ElementBase.IndexOf((TList)this, item);
        
        /// <summary>
        /// Determines the 32-bit index of a specific <typeparamref name"TElement" /> in the <see cref="LinkedComponentList{TList, TElement}" />.
        /// </summary>
        /// <param name="item">The <typeparamref name"TElement" /> to locate in the <see cref="LinkedComponentList{TList, TElement}" />.</param>
        /// <returns>The zero-based index of <typeparamref name"TElement" /> if found in the <see cref="LinkedComponentList{TList, TElement}" />;
        /// otherwise, -1. If the index of the <typeparamref name"TElement" /> is greater than <seealso cref="Int32.MaxValue" />, then -2 is returned.</returns>
        protected int IndexOf32(TElement item)
        {
            long index = IndexOf(item);
            return (index < 0L) ? -1 : ((index > (long)(int.MaxValue)) ? -2 : (int)index);
        }
        
        /// <summary>
        /// Inserts an item to the System.Collections.Generic.IList`1 at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the System.Collections.Generic.IList`1.</param>
        /// <exception cref="ArgumentNullException">item is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">item already belongs to the current list or to another list.
        /// <para>-or</para>
        /// <para>index is not a valid index in the System.Collections.Generic.IList`1.</para></exception>
        protected void Insert(long index, TElement item) => ElementBase.Insert((TList)this, index, item);
        
        /// <summary>
        /// Removes a specific object from the System.Collections.Generic.ICollection`1.
        /// </summary>
        /// <param name="item">The object to remove from the System.Collections.Generic.ICollection`1.</param>
        /// <returns>true if item was successfully removed from the System.Collections.Generic.ICollection`1;
        /// otherwise, false. This method also returns false if item is not found in the original System.Collections.Generic.ICollection`1.</returns>
        protected virtual bool Remove(TElement item) => ElementBase.Remove((TList)this, item);

        /// <summary>
        /// Removes the System.Collections.Generic.IList`1 item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the System.Collections.Generic.IList`1.</exception>
        protected virtual void RemoveAt(long index) => ElementBase.RemoveAt((TList)this, index);

        /// <summary>
        /// Projects each list of a sequence into a new form by incorporating the element's zero-based index.
        /// </summary>
        /// <param name="selector">A transform function to apply to each list element;
        /// the second parameter of the function represents the index of the element.</param>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <returns>An System.Collections.Generic.IEnumerable`1 whose elements are the result of invoking the transform function on each list element.</returns>
        /// <exception cref="ArgumentNullException">selector is null.</exception>
        public IEnumerable<TResult> Select<TResult>(Func<TElement, long, TResult> selector)
        {
            using (ElementBase.Enumerator enumerator = new ElementBase.Enumerator((TList)this))
            {
                while (enumerator.MoveNext())
                    yield return selector(enumerator.Current, enumerator.Index);
            }
        }

        /// <summary>
        /// Blocks the current thread until items can be added to or removed from the current list,
        /// using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait,
        /// or <seealso cref="Timeout.Infinite" /> to wait indefinitely.</param>
        /// <returns>true if the current list is changeable within the specified number of milliseconds; otherwise, false.</returns>
        protected bool WaitCollectionChangable(int millisecondsTimeout) => GetInMonitorLock(() => _collectionChangableEvent.WaitOne(millisecondsTimeout));

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

        public void Dispose() => Dispose(true);
        
        private void InvokeInChange<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2) => InvokeInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(arg1, arg2); }
            finally { _collectionChangableEvent.Set(); }
        });
        private void InvokeInChange<T>(Action<T> action, T arg) => InvokeInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(arg); }
            finally { _collectionChangableEvent.Set(); }
        });
        private void InvokeInChange(Action action) => InvokeInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { action(); }
            finally { _collectionChangableEvent.Set(); }
        });

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking an <seealso cref="Action{T1, T2}" /> delegate method.
        /// </summary>
        /// <param name="action">The delegate method to invoke.</param>
        /// <param name="arg1">The first parameter to pass to the <seealso cref="Action{T1, T2}" /> delegate method.</param>
        /// <param name="arg2">The second parameter to pass to the <seealso cref="Action{T1, T2}" /> delegate method.</param>
        /// <typeparam name="T1">The type of the first parameter to pass to the <seealso cref="Action{T1, T2}" /> delegate method.</typeparam>
        /// <typeparam name="T2">The type of the second parameter to pass to the <seealso cref="Action{T1, T2}" /> delegate method.</typeparam>
        protected void InvokeInMonitorLock<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                action(arg1, arg2);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking an <seealso cref="Action{T}" /> delegate method.
        /// </summary>
        /// <param name="action">The delegate method to invoke.</param>
        /// <param name="arg">The parameter to pass to the <seealso cref="Action{T}" /> delegate method.</param>
        /// <typeparam name="T">The type of parameter to pass to the <seealso cref="Action{T}" /> delegate method.</typeparam>
        protected void InvokeInMonitorLock<T>(Action<T> action, T arg)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                action(arg);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking an <seealso cref="Action" /> delegate method.
        /// </summary>
        /// <param name="action">The delegate method to invoke.</param>
        protected void InvokeInMonitorLock(Action action)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                action();
            }
            finally { Monitor.Exit(SyncRoot); }
        }
        private TResult GetInChange<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2) => GetInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(arg1, arg2); }
            finally { _collectionChangableEvent.Set(); }
        });
        private TResult GetInChange<TArg, TResult>(Func<TArg, TResult> func, TArg arg) => GetInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(arg); }
            finally { _collectionChangableEvent.Set(); }
        });
        private T GetInChange<T>(Func<T> func) => GetInMonitorLock(() =>
        {
            if (!_collectionChangableEvent.WaitOne(0))
                throw new InvalidOperationException(ErrorMessage_ChangeInProgress);
            _collectionChangableEvent.Reset();
            try { return func(); }
            finally { _collectionChangableEvent.Set(); }
        });

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking a <seealso cref="Func{TArg1, TArg2, TResult}" /> delegate and returning the result.
        /// </summary>
        /// <param name="func">The delegate function to invoke.</param>
        /// <param name="arg1">The first parameter to pass to the <seealso cref="Func{TArg1, TArg2, TResult}" /> delegate method.</param>
        /// <param name="arg2">The second parameter to pass to the <seealso cref="Func{TArg1, TArg2, TResult}" /> delegate method.</param>
        /// <typeparam name="T1">The type of the first parameter to pass to the <seealso cref="Func{TArg1, TArg2, TResult}" /> delegate method.</typeparam>
        /// <typeparam name="T2">The type of the second parameter to pass to the <seealso cref="Func{TArg1, TArg2, TResult}" /> delegate method.</typeparam>
        /// <typeparam name="TResult">The type of value returned by the delegate function.</typeparam>
        /// <returns>The value returned by the delegate function.</returns>
        protected TResult GetInMonitorLock<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                return func(arg1, arg2);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking a <seealso cref="Func{TArg, TResult}" /> delegate and returning the result.
        /// </summary>
        /// <param name="func">The delegate function to invoke.</param>
        /// <param name="arg">The parameter to pass to the <seealso cref="Func{TArg, TResult}" /> delegate method.</param>
        /// <typeparam name="TArg">The type of parameter to pass to the <seealso cref="Func{TArg, TResult}" /> delegate method.</typeparam>
        /// <typeparam name="TResult">The type of value returned by the delegate function.</typeparam>
        /// <returns>The value returned by the delegate function.</returns>
        protected TResult GetInMonitorLock<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                return func(arg);
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// Acquires a <seealso cref="Monitor" /> lock for the current <see cref="LinkedComponentList{TList, TElement}" />
        /// while invoking a <seealso cref="Func{T}" /> delegate and returning the result.
        /// </summary>
        /// <param name="func">The delegate function to invoke.</param>
        /// <typeparam name="T">The type of value returned by the delegate function.</typeparam>
        /// <returns>The value returned by the delegate function.</returns>
        protected T GetInMonitorLock<T>(Func<T> func)
        {
            Monitor.Enter(SyncRoot);
            try
            {
                if (_collectionChangableEvent == null)
                    throw new ObjectDisposedException(typeof(TList).AssemblyQualifiedName);
                return func();
            }
            finally { Monitor.Exit(SyncRoot); }
        }

        /// <summary>
        /// This gets called before an element is about to replace another in the current list.
        /// </summary>
        /// <param name="existing">The element being replaced.</param>
        /// <param name="newElement">The element that will be added in place of the <paramref name="existing" /> element.</param>
        /// <param name="index">The zero-based index of the element being replaced.</param>
        /// <remarks>After this method is invoked, <seealso cref="OnRemovingItem(TElement, index)" /> and <seealso cref="ElementBase.OnRemoving(long)" /> will be called for the <paramref name="existing" /> element,
        /// and then <sealso cref="OnAddingItem(TElement, long)" /> and <seealso cref="ElementBase.OnAdding(TList, long)" /> will be called for <paramref name="newElement" />.
        /// <para>While this method is being invoked, no items should be added to or removed from the parent <see cref="LinkedComponentList{TList, TElement}" />.</para></remarks>
        protected virtual void OnReplacingItem(TElement existing, TElement newElement, long index) { }

        /// <summary>
        /// This gets called before an element is about to added to the current list.
        /// </summary>
        /// <param name="item">The element being added.</param>
        /// <param name="index">The zero-based index at which the element will be inserted or added.</param>
        /// <remarks>After this method is invoked, <seealso cref="ElementBase.OnAdding(TList, long)" /> will be called.
        /// <para>While this method is being invoked, no items should be added to or removed from the current <see cref="LinkedComponentList{TList, TElement}" />.</para></remarks>
        protected virtual void OnAddingItem(TElement item, long index) { }

        /// <summary>
        /// This gets called before an element is about to removed from the current list.
        /// </summary>
        /// <param name="existing">The element that is being removed.</param>
        /// <param name="index">The zero-based index of the element being removed.</param>
        /// <remarks>After this method is invoked, <seealso cref="ElementBase.OnRemoving(long)" /> will be called.
        /// <para>While this method is being invoked, no items should be added to or removed from the parent <see cref="LinkedComponentList{TList, TElement}" />.</para></remarks>
        protected virtual void OnRemovingItem(TElement existing, long index) { }

        /// <summary>
        /// This gets called after an element has replaced another in the current list.
        /// </summary>
        /// <param name="orphaned">The element that was replaced.</param>
        /// <param name="added">The elemment that replaced the previous element.</param>
        /// <param name="index">The zero-based index at which the elemnt was replaced.</param>
        /// <remarks>Before this method is invoked, <sealso cref="OnItemAdded(TElement, long)" /> and <seealso cref="ElementBase.OnAdded(long)" /> will be called for <paramref name="added" /> element,
        /// and then <seealso cref="OnItemRemoved(TElement, index)" /> and <seealso cref="ElementBase.OnRemoved(TList, long)" /> will be called for the <paramref name="orphaned" /> element.</remarks>
        protected virtual void OnItemReplaced(TElement orphaned, TElement added, long index) { }

        /// <summary>
        /// This gets called after an element has been added to the current list.
        /// </summary>
        /// <param name="item">The element that was added to the current list.</param>
        /// <param name="index">The index at which the element was added or inserted.</param>
        /// <remarks>After this method is invoked, <seealso cref="ElementBase.OnAdded(long)" /> will be called.</remarks>
        protected virtual void OnItemAdded(TElement item, long index) { }

        /// <summary>
        /// This gets called after an element has been removed from the current list.
        /// </summary>
        /// <param name="item">The element that has been removed from the current list.</param>
        /// <param name="index">The zero-based index at which the element was removed.</param>
        /// <remarks>After this method is invoked, <seealso cref="ElementBase.OnRemoved(TList, long)" /> will be called.</remarks>
        protected virtual void OnItemRemoved(TElement item, long index) { }

        #region Explicit Members

        TElement IList<TElement>.this[int index] { get => ElementBase.Get((TList)this, index); set => throw new NotSupportedException(); }
        object IList.this[int index] { get => ElementBase.Get((TList)this, index); set => throw new NotSupportedException(); }
        int ICollection<TElement>.Count => Count32;
        int ICollection.Count => Count32;
        bool ICollection<TElement>.IsReadOnly => true;
        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => SyncRoot;
        bool ICollection.IsSynchronized => true;
        void ICollection<TElement>.Add(TElement item) => throw new NotSupportedException();
        int IList.Add(object value) => throw new NotSupportedException();
        void ICollection<TElement>.Clear() => Clear();
        void IList.Clear() => Clear();
        bool IList.Contains(object value) => value != null && value is TElement && Contains((TElement)value);
        void ICollection.CopyTo(Array array, int index)
        {
            try { ElementBase.CopyTo((TList)this, array, index); }
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
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        int IList<TElement>.IndexOf(TElement item) => IndexOf32(item);
        int IList.IndexOf(object value) => (value != null && value is TElement) ? IndexOf32((TElement)value) : -1;
        void IList<TElement>.Insert(int index, TElement item) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        bool ICollection<TElement>.Remove(TElement item) => Remove(item);
        void IList.Remove(object value)
        {
            if (value != null && value is TElement)
                Remove((TElement)value);
        }
        void IList<TElement>.RemoveAt(int index) => RemoveAt(index);
        void IList.RemoveAt(int index) => RemoveAt(index);

        #endregion
    }
}