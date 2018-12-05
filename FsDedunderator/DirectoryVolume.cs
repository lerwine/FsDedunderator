using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FsDedunderator
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectoryVolume : LinkedComponentElement<DirectoryVolume, VolumeComponentList>, IFileDirectory, IEquatable<DirectoryVolume>
    {
        private StringComparer _nameComparer;
        private DirectoryNode.DirectoryList _contents = null;

        /// <summary>
        /// 
        /// </summary>
        public const int InteropStringCapacity = 261;

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public VolumeComponentList VolumeList => Container;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="ArgumentException"><paramref name=""/></exception>
        public DirectoryVolume(DirectoryInfo directory)
        {
            _contents = new DirectoryNode.DirectoryList(this);
            if (directory == null)
                Name = new DirectoryInfo(Environment.SystemDirectory).Root.FullName;
           
            StringBuilder volumeNameBuffer = new StringBuilder(InteropStringCapacity);
            StringBuilder fsn = new StringBuilder(InteropStringCapacity);
            if (!VolumeComponentList.GetVolumeInformation(Name, volumeNameBuffer, InteropStringCapacity, out uint sn, out uint maxNameLength, out FileSystemFeature flags, fsn, InteropStringCapacity))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            VolumeName = volumeNameBuffer.ToString();
            FileSystemName = fsn.ToString();
            Flags = flags;
            SerialNumber = sn;
            MaxNameLength = maxNameLength;
            _nameComparer = (IsCaseSensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
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
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name=""/></exception>
        /// <exception cref="ArgumentException"><paramref name=""/></exception>
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
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name=""/></exception>
        /// <exception cref="ArgumentException"><paramref name=""/></exception>
        public DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="ArgumentException"><paramref name=""/></exception>
        public FileDirectory Add(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DirectoryVolume other)
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

        DirectoryVolume IFileDirectory.GetVolume() => this;
    }
}