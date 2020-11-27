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
    /// <typeparam name="DirectoryNode"></typeparam>
    /// <typeparam name="DirectoryNode.DirectoryList"></typeparam>
    public abstract class DirectoryNode : LinkedComponentElement<DirectoryNode, DirectoryNode.DirectoryList>, IEquatable<DirectoryNode>
    {
        private IFileDirectory _parent;

        /// <summary>
        /// 
        /// </summary>
        public IFileDirectory Parent => _parent;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DirectoryVolume GetVolume()
        {
            IFileDirectory parent = _parent;
            return (parent == null) ? null : parent.GetVolume();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(DirectoryNode other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class DirectoryList : LinkedComponentList<DirectoryList, DirectoryNode>
        {
            private IFileDirectory _owner;

            internal DirectoryList(IFileDirectory owner)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                if (owner.Contents != null)
                    throw new InvalidOperationException();
                _owner = owner;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <param name="index"></param>
            protected override void OnAddingItem(DirectoryNode item, long index)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <param name="index"></param>
            protected override void OnItemAdded(DirectoryNode item, long index)
            {
                item._parent = _owner;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <param name="index"></param>
            protected override void OnItemRemoved(DirectoryNode item, long index)
            {
                item._parent = null;
            }
        }
    }
}