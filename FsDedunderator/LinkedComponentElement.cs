namespace FsDedunderator
{
    /// <summary>
    /// Child element object type for a an object that derrives from <seealso cref="LinkedComponentList{TList, TElement}" />.
    /// </summary>
    /// <typeparam name="TElement">The type of object which is inheriting from this class.</typeparam>
    /// <typeparam name="TList">The type of list, derriving from <seealso cref="LinkedComponentList{TList, TElement}" />, that will contain <typeparamref name="TElement" /> objects.</typeparam>
    public abstract class LinkedComponentElement<TElement, TList> : LinkedComponentList<TList, TElement>.ElementBase
        where TElement : LinkedComponentElement<TElement, TList>
        where TList : LinkedComponentList<TList, TElement>
    {
        /// <summary>
        /// Initializes a new <see cref="LinkedComponentElement{TElement, TList}" /> that does not belong to a <seealso cref="LinkedComponentList{TList, TElement}" />.
        /// </summary>
        protected LinkedComponentElement() : base() { }
    }
}