using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FsDedunderator
{
    public sealed partial class CrawlData : Component
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        public static readonly StringComparer NameComparer = StringComparer.InvariantCultureIgnoreCase;
        public delegate void ProgressUpdateHandler(ProgressUpdateEventArgs e);
        public delegate void CompletionHandler(CrawlCompletionEventArgs e);
        public RootDirectoriesContainer RootDirectories { get; }
        public SizeMappingContainer SizeMappings { get; }

        public CrawlData()
        {
            RootDirectories = new RootDirectoriesContainer(this);
            SizeMappings = new SizeMappingContainer(this);
        }

        public FileNode AddFile(RootDirectory parent, string name, long length)
        {
            FileNode result = new FileNode();
            SingleFile singleFile = SizeMappings.GetComponentByValue<SingleFile>(length);
            FilesBySize filesBySize;
            if (null != singleFile)
            {
                SizeMappings.Remove(singleFile);
                filesBySize = new FilesBySize();
                filesBySize.AddFile(singleFile);
                filesBySize.AddFile(result.Grouped);
            }
            else
            {
                filesBySize = SizeMappings.GetComponentByValue<FilesBySize>(length);
                if (null != filesBySize)
                    filesBySize.AddFile(result.Grouped);
                else
                    SizeMappings.AddValue(result.Grouped, length);
            }
            parent.AddFile(result.StructureNode, name);
            return result;
        }
        
        public FileNode AddFile(SubDirectory parent, string name, long length)
        {
            FileNode result = new FileNode();
            SingleFile singleFile = SizeMappings.GetComponentByValue<SingleFile>(length);
            FilesBySize filesBySize;
            if (null != singleFile)
            {
                SizeMappings.Remove(singleFile);
                filesBySize = new FilesBySize();
                filesBySize.AddFile(singleFile);
                filesBySize.AddFile(result.Grouped);
            }
            else
            {
                filesBySize = SizeMappings.GetComponentByValue<FilesBySize>(length);
                if (null != filesBySize)
                    filesBySize.AddFile(result.Grouped);
                else
                    SizeMappings.AddValue(result.Grouped, length);
            }
            parent.AddFile(result.StructureNode, name);
            return result;
        }
        
        public RootDirectory AddDirectory(string path)
        {
            RootDirectory result = new RootDirectory();
            RootDirectories.AddValue(result, path);
            return result;
        }

        public static string AssertValidPathName(string name)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            if (name.TrimEnd().Length == 0)
                throw new ArgumentException("Path name cannot be empty", "name");
            if (!Path.IsPathRooted(name))
                throw new ArgumentException("Path name must be an absolute path", "name");
            foreach (char c in InvalidPathChars)
            {
                if (name.Contains(c))
                    throw new ArgumentException("Invalid file name", "name");
            }
            try
            {
                name = Path.GetFullPath(name);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            if (String.IsNullOrEmpty(Path.GetFileName(name)))
            {
                string d = Path.GetDirectoryName(name);
                if (!String.IsNullOrEmpty(d))
                    return d;
            }
            return name;
        }
        
        public static bool TryGetValidPathName(string name, out string result)
        {
            if (null == name || name.TrimEnd().Length == 0 || !Path.IsPathRooted(name) || !String.IsNullOrEmpty(Path.GetDirectoryName(name)))
            {
                result = name;
                return false;
            }
            foreach (char c in InvalidPathChars)
            {
                if (name.Contains(c))
                {
                    result = name;
                    return false;
                }
            }
            try
            {
                result = Path.GetFullPath(name);
            }
            catch
            {
                result = name;
                return false;
            }
            if (String.IsNullOrEmpty(Path.GetFileName(name)))
            {
                string d = Path.GetDirectoryName(name);
                result = (String.IsNullOrEmpty(d)) ? name : d;
            }
            else
                result = name;
            return true;
        }

        public class SizeMappingContainer : NestedValueContainer<long>
        {
            public new CrawlData Owner { get { return (CrawlData)base.Owner; } }

            internal SizeMappingContainer(CrawlData owner) : base(owner)
            {
                if (null != owner.SizeMappings)
                    throw new InvalidOperationException();
            }
            
            protected override string ValidateName(IComponent component, string name, out long value)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (null == name || !long.TryParse(name.Trim(), out long newValue) || newValue < 0L)
                    throw new ArgumentException("Invalid name", "name");
                name = newValue.ToString();
                IComponent c = GetAnyComponentByName(name);
                if (null != c && !Object.ReferenceEquals(c, component))
                    throw new ArgumentException("Duplicate name not allowed", "name");
                value = newValue;
                return name;
            }

            protected override long ValidateValue(IComponent component, long value, out string name)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (value < 0L)
                    throw new ArgumentOutOfRangeException("value");
                string newName = value.ToString();
                IComponent c = this[value];
                if (null != c && !Object.ReferenceEquals(c, component))
                    throw new ArgumentException("Duplicate value not allowed", "name");
                c = GetAnyComponentByName(newName);
                if (null != c && !Object.ReferenceEquals(c, component))
                    throw new ArgumentException("Duplicate name not allowed", "name");
                name = newName;
                return value;
            }

            protected override bool AreValuesEqual(long x, long y) => x == y;

            protected override Site CreateSite(IComponent component, string name)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (component is SingleFile || component is FilesBySize)
                {
                    if (null == name)
                        throw new ArgumentNullException("name");
                    if (!Int64.TryParse(name.Trim(), out long value) || value < 0L)
                        throw new ArgumentException("Invalid name", "name");
                    if (component is SingleFile)
                        return new SingleFileSite((SingleFile)component, this, value);
                    return new FileGroupSite((FilesBySize)component, this, value);
                }
                return new OtherSite(component, this, name);
            }

            protected override ValueSite CreateValueSite(IComponent component, long value)
            {
                if (component is SingleFile)
                    return new SingleFileSite((SingleFile)component, this, value);
                if (component is FilesBySize)
                    return new FileGroupSite((FilesBySize)component, this, value);
                throw new InvalidOperationException();
            }

            private class SingleFileSite : ValueSite
            {
                public new SingleFile Component { get { return (SingleFile)base.Component; } }

                internal SingleFileSite(SingleFile component, SizeMappingContainer container, long length) : base(component, container, length)
                {
                    if (length < 0L)
                        throw new ArgumentOutOfRangeException("length");
                }
            }

            private class FileGroupSite : ValueSite
            {
                public new FilesBySize Component { get { return (FilesBySize)base.Component; } }

                internal FileGroupSite(FilesBySize component, SizeMappingContainer container, long length) : base(component, container, length)
                {
                    if (length < 0L)
                        throw new ArgumentOutOfRangeException("length");
                }
            }
        }

        public sealed class RootDirectoriesContainer : NestedValueContainer<string>
        {
            public new CrawlData Owner { get { return (CrawlData)base.Owner; } }

            internal RootDirectoriesContainer(CrawlData owner) : base(owner)
            {
                if (null != owner.RootDirectories)
                    throw new InvalidOperationException();
            }

            protected override string ValidateName(IComponent component, string name, out string value)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (null == name)
                {
                    if (component is RootDirectory)
                        throw new ArgumentNullException("name");
                    value = null;
                    return null;
                }
                string newValue = DecodeName(name);
                if (!EncodeName(newValue).Equals(name))
                    newValue = name;
                value = AssertValidPathName(newValue);
                return EncodeName(value);
            }

            protected override string ValidateValue(IComponent component, string value, out string name)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (null == (value = AssertValidPathName(value)))
                {
                    if (component is RootDirectory)
                        throw new ArgumentNullException("name");
                    name = null;
                }
                else
                    name = EncodeName(value);
                return value;
            }

            protected override bool AreValuesEqual(string x, string y)
            {
                return (null == x) ? null == y : null != y && NameComparer.Equals(x, y);
            }

            protected override Site CreateSite(IComponent component, string name)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                if (component is RootDirectory)
                {
                    if (null == name)
                        throw new ArgumentNullException("name");
                    string value = DecodeName(name);
                    if (EncodeName(value).Equals(name))
                        return new SiteImpl(component, this, value);
                    return new SiteImpl(component, this, name);
                }
                return new OtherSite(component, this, name);
            }

            protected override ValueSite CreateValueSite(IComponent component, string value)
            {
                if (null == component)
                    throw new ArgumentNullException("component");
                return new SiteImpl(component, this, value);
            }

            private class SiteImpl : ValueSite
            {
                internal SiteImpl(IComponent component, RootDirectoriesContainer container, string value) : base(component, container, AssertValidPathName(value))
                {
                    if (null == Value)
                        throw new ArgumentNullException("value");
                }
            }
        }

    }
}