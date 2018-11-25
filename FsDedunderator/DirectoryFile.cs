using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public sealed class DirectoryFile : DirectoryNode, IEquatable<DirectoryFile>
    {
        private long _creationTime;
        private long _lastModificationTime;
        private IFileReference _contentReference;

        public long CreationTime
        {
            get => _creationTime;
            set
            {
                throw new NotImplementedException();
            }
        }

        public long LastModificationTime
        {
            get => _lastModificationTime;
            set
            {
                throw new NotImplementedException();
            }
        }

        public IFileReference ContentReference => _contentReference;

        public bool Equals(DirectoryFile other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(DirectoryNode other)
        {
            throw new NotImplementedException();
        }
    }
}