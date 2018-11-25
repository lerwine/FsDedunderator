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
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TParent"></typeparam>
    public abstract class LongLinkedNode<TNode, TParent> : LongLinkedList<TParent, TNode>.NodeLink
        where TNode : LongLinkedNode<TNode, TParent>
        where TParent : LongLinkedList<TParent, TNode>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected LongLinkedNode(TParent parent) : base(parent) { }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected LongLinkedNode() : base(null) { }
    }
}