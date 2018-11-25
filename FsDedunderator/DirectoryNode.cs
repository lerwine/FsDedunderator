using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public abstract class DirectoryNode : IEquatable<DirectoryNode>
    {
        private FileDirectory _parent;

        public FileDirectory Parent
        {
            get => _parent;
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name { get; }

        public virtual bool Equals(DirectoryNode other)
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