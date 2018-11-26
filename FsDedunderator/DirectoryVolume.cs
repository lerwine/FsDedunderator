using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FsDedunderator
{
    public class DirectoryVolume : LinkedComponentElement<DirectoryVolume, VolumeComponentList>, IFileDirectory, IEquatable<DirectoryVolume>
    {
        private StringComparer _nameComparer;
        private DirectoryNode.DirectoryList _contents = null;
        public const int InteropStringCapacity = 261;

        public DirectoryNode.DirectoryList Contents => _contents;

        /// <summary>
        /// Gets the full path name of the volume root directory.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the volume.
        /// </summary>
        public string VolumeName { get; }
        
        /// <summary>
        /// Gets the name of the file system.
        /// </summary>
        public string FileSystemName { get; }
        
        /// <summary>
        /// Gets the volume serial number.
        /// </summary>
        public uint SerialNumber { get; }

        /// <summary>
        /// Gets the maximum length for file/directory names.
        /// </summary>
        public uint MaxNameLength { get; }

        /// <summary>
        /// Gets a value that indicates the volume capabilities and attributes.
        /// </summary>
        public FileSystemFeature Flags { get; }

        /// <summary>
        /// Indicates whether the current volume file names are case-sensitive.
        /// </summary>
        public bool IsCaseSensitive => Flags.HasFlag(FileSystemFeature.CaseSensitiveSearch);

        IFileDirectory IFileDirectory.Parent => null;

        public VolumeComponentList VolumeList => Container;

        public DirectoryVolume(DirectoryInfo directory)
        {
            _contents = new DirectoryNode.DirectoryList(this);
            if (directory == null)
                Name = new DirectoryInfo(Environment.SystemDirectory).Root.FullName;
           
            StringBuilder volumeNameBuffer = new StringBuilder(InteropStringCapacity);
            StringBuilder fsn = new StringBuilder(InteropStringCapacity);
            if (!GetVolumeInformation(Name, volumeNameBuffer, InteropStringCapacity, out uint sn, out uint maxNameLength, out FileSystemFeature flags, fsn, InteropStringCapacity))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            VolumeName = volumeNameBuffer.ToString();
            FileSystemName = fsn.ToString();
            Flags = flags;
            SerialNumber = sn;
            MaxNameLength = maxNameLength;
            _nameComparer = (IsCaseSensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
        }

        public DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            throw new NotImplementedException();
        }

        public DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length)
        {
            throw new NotImplementedException();
        }

        public FileDirectory Add(string name)
        {
            throw new NotImplementedException();
        }

        public bool Equals(DirectoryVolume other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IFileDirectory other)
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

        DirectoryVolume IFileDirectory.GetVolume() => this;
    }
}