using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public abstract class FileReference<T> : IFileReference
        where T : IContentGroup
    {
        public T Content { get; }

        IContentGroup IFileReference.Content => Content;

        public DirectoryFile File { get; }

        public long Length => Content.Length;

        public MD5Checksum? Checksum => Content.Checksum;

        protected FileReference(DirectoryFile file, T content)
        {
            File = file;
            Content = content;
        }
        public virtual bool Equals(IFileReference other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}