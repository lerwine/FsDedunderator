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
    public sealed class FileDirectory : DirectoryNode, IFileDirectory, IEquatable<FileDirectory>
    {
        private DirectoryList _contents = null;

        /// <summary>
        /// 
        /// </summary>
        public DirectoryList Contents => _contents;

        /// <summary>
        /// 
        /// </summary>
        public FileDirectory()
        {
            _contents = new DirectoryList(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="creationTime"></param>
        /// <param name="lastModificationTime"></param>
        /// <param name="length"></param>
        /// <param name="checksum"></param>
        /// <param name="lastCalculated"></param>
        /// <returns></returns>
        public DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="creationTime"></param>
        /// <param name="lastModificationTime"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FileDirectory Add(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(FileDirectory other)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(DirectoryNode other)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IFileDirectory other)
        {
            throw new NotImplementedException();
        }
    }
}