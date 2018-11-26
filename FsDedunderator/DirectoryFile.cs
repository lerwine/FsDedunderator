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
    public sealed class DirectoryFile : DirectoryNode, IEquatable<DirectoryFile>
    {
        private long _creationTime;
        private long _lastModificationTime;
        private IFileReference _contentReference;

        /// <summary>
        /// 
        /// </summary>
        public long CreationTime
        {
            get => _creationTime;
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long LastModificationTime
        {
            get => _lastModificationTime;
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IFileReference ContentReference => _contentReference;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DirectoryFile other)
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
    }
}