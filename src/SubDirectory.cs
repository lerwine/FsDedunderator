using System;
using System.ComponentModel;

namespace FsDedunderator
{
    public class SubDirectory : Component
    {
        public DirectoryContainer SubDirectories { get; }

        public SubDirectory()
        {
            SubDirectories = new DirectoryContainer(this);
        }

        internal void AddFile(FileStructureNode structureNode, string name)
        {
            throw new NotImplementedException();
        }

        public sealed class DirectoryContainer : NestedValueContainer<string>
        {
            public new SubDirectory Owner { get; }

            internal DirectoryContainer(SubDirectory owner) : base(owner)
            {
                if (null != owner.Container)
                    throw new InvalidOperationException();
            }

            protected override bool AreValuesEqual(string x, string y)
            {
                throw new NotImplementedException();
            }

            protected override Site CreateSite(IComponent component, string name)
            {
                throw new NotImplementedException();
            }

            protected override ValueSite CreateValueSite(IComponent component, string value)
            {
                throw new NotImplementedException();
            }

            protected override string ValidateName(IComponent component, string name, out string value)
            {
                throw new NotImplementedException();
            }

            protected override string ValidateValue(IComponent component, string value, out string name)
            {
                throw new NotImplementedException();
            }
        }
    }
}