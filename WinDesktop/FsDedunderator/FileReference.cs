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
    /// <typeparam name="T"></typeparam>
    public abstract class FileReference<T> : IFileReference
        where T : IContentGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public T Content { get; }

        IContentGroup IFileReference.Content => Content;

        /// <summary>
        /// 
        /// </summary>
        public DirectoryFile File { get; }

        /// <summary>
        /// 
        /// </summary>
        public long Length => Content.Length;

        /// <summary>
        /// 
        /// </summary>
        public MD5Checksum? Checksum => Content.Checksum;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="ArgumentException"><paramref name=""/></exception>
        protected FileReference(DirectoryFile file, T content)
        {
            File = file;
            Content = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(IFileReference other)
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
    }
}