using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FsDedunderator
{
    /// <summary>
    /// Contains information about a filesystem volume.
    /// </summary>
    public class VolumeInformation : IEquatable<VolumeInformation>, IEqualityComparer<string>
    {
        #region Fields

        public const int InteropStringCapacity = 261;

        private StringComparer _nameComparer;
        private static VolumeInformation _default = null;
        private readonly DirectoryInfo _root;

        #endregion

        #region Properties

        /// <summary>
        /// Volume information for system drive.
        /// </summary>
        public static VolumeInformation Default
        {
            get
            {
                if (_default == null)
                    _default = new VolumeInformation();
                return _default;
            }
        }

        /// <summary>
        /// Gets the name of the volume root directory.
        /// </summary>
        public string Name => _root.Name;

        /// <summary>
        /// Gets a value indicating whether the volume / root directory exists
        /// </summary>
        public bool Exists => _root.Exists;

        /// <summary>
        /// Gets the full path name of the volume root directory.
        /// </summary>
        public string RootPathName => _root.FullName;

        /// <summary>
        /// Gets the name of the file system.
        /// </summary>
        public string FileSystemName { get; private set; }

        /// <summary>
        /// Gets teh name of the volume.
        /// </summary>
        public string VolumeName { get; private set; }

        /// <summary>
        /// Gets the volume serial number.
        /// </summary>
        public uint SerialNumber { get; private set; }

        /// <summary>
        /// Gets the maximum length for file/directory names.
        /// </summary>
        public uint MaxNameLength { get; private set; }

        /// <summary>
        /// Gets a value that indicates the volume capabilities and attributes.
        /// </summary>
        public FileSystemFeature Flags { get; private set; }

        /// <summary>
        /// Indicates whether the current volume file names are case-sensitive.
        /// </summary>
        public bool IsCaseSensitive => Flags.HasFlag(FileSystemFeature.CaseSensitiveSearch);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="VolumeInformation"/>.
        /// </summary>
        /// <param name="directory">A child <seealso cref="DirectoryInfo"/> of the volume information to retrieve or null for the volume that contains the system directory.</param>
        public VolumeInformation(DirectoryInfo directory)
        {
            if (directory == null)
                _root = new DirectoryInfo(Environment.SystemDirectory).Root;
            else
            {
                _root = directory.Root;
                if (!_root.Exists)
                {
                    Flags = (new VolumeInformation()).Flags;
                    FileSystemName = "";
                    VolumeName = "";
                }
            }
            _Refresh();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="VolumeInformation"/> describing the volume that contains system directory.
        /// </summary>
        public VolumeInformation() : this(null) { }

        #endregion

        #region Methods

        private void _Refresh()
        {
            if (_root.Exists)
            {
                StringBuilder volumeNameBuffer = new StringBuilder(InteropStringCapacity);
                StringBuilder fsn = new StringBuilder(InteropStringCapacity);
                if (!GetVolumeInformation(_root.FullName, volumeNameBuffer, InteropStringCapacity, out uint sn, out uint maxNameLength, out FileSystemFeature flags, fsn, InteropStringCapacity))
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                VolumeName = volumeNameBuffer.ToString();
                FileSystemName = fsn.ToString();
                Flags = flags;
                SerialNumber = sn;
                MaxNameLength = maxNameLength;
            }
            _nameComparer = (IsCaseSensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
        }

        /// <summary>
        /// Refreshes the state of the object.
        /// </summary>
        public void Refresh()
        {
            _root.Refresh();
            _Refresh();
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetVolumeInformation(string rootPathName, StringBuilder volumeNameBuffer, int volumeNameSize, out uint volumeSerialNumber,
            out uint maximumComponentLength, out FileSystemFeature fileSystemFlags, StringBuilder fileSystemNameBuffer, int nFileSystemNameSize);

        /// <summary>
        /// Determines whether the current <see cref="VolumeInformation"/> object is equal to another.
        /// </summary>
        /// <param name="other">Other <see cref="VolumeInformation"/> to compare.</param>
        /// <returns><c>true</c> if the current <see cref="VolumeInformation"/> is equal to <paramref name="other"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(VolumeInformation other) => other != null && (ReferenceEquals(this, other) || _root.FullName == other._root.FullName ||
            (!(Flags.HasFlag(FileSystemFeature.CaseSensitiveSearch) || other.Flags.HasFlag(FileSystemFeature.CaseSensitiveSearch)) &&
            _nameComparer.Equals(_root.FullName, other._root.FullName)));

        /// <summary>
        /// Determines whether the current <see cref="VolumeInformation"/> object is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns><c>true</c> if <paramref name="other"/> is a <see cref="VolumeInformation"/> and is equal to the current <see cref="VolumeInformation"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj != null && obj is VolumeInformation && Equals((VolumeInformation)obj);

        /// <summary>
        /// Returns the hash code for the current <see cref="VolumeInformation"/> object.
        /// </summary>
        /// <returns>The hash code for the current <see cref="VolumeInformation"/> object.</returns>
        public override int GetHashCode() => (VolumeName == null) ? Name.GetHashCode() : (int)SerialNumber;

        /// <summary>
        /// Gets a string representation of the current <see cref="VolumeInformation"/> object.
        /// </summary>
        /// <returns>A string version of the current object.</returns>
        public override string ToString() => (FileSystemName.Length == 0) ? Path.DirectorySeparatorChar + _root.Name : (SerialNumber >> 16).ToString("X4") + "-" + (SerialNumber & 0xFFFF).ToString("X4") + Path.DirectorySeparatorChar + VolumeName;

        private static readonly char[] PathSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };

        /// <summary>
        /// Gets a normalized path string.
        /// </summary>
        /// <param name="path">Path string to normalize.</param>
        /// <param name="keelRelative">If <c>true</c>, then path is not resolved to a full path name; otherwise, the path is resolved and normalized to a fully qualified path name.</param>
        /// <returns>The normalized path string.</returns>
        public string NormalizePath(string path, bool keelRelative)
        {
            if (!keelRelative || Path.IsPathFullyQualified(path))
                return NormalizePath(path, null);

            if (string.IsNullOrEmpty(path) || path == ".")
                return ".";
            
            int index = path.IndexOfAny(PathSeparators);
            if (index < 0)
                return path;
            

            if (path[index] == Path.VolumeSeparatorChar)
            {
                if (index == path.Length - 1)
                    return path + Path.DirectorySeparatorChar;
                string p = NormalizePath(path.Substring(index + 1));
                return (p[0] == Path.DirectorySeparatorChar) ? path.Substring(0, index) + p : ((p == ".") ? path + Path.DirectorySeparatorChar :
                    ((p.Length > 1 && p[0] == '.' && p[1] == Path.DirectorySeparatorChar) ? path + p.Substring(1) : path + Path.DirectorySeparatorChar + p));
            }
            
            List<string> elements = new List<string>(path.Split(PathSeparators));
            for (int i = 1; i < elements.Count; i++)
            {
                switch (elements[i])
                {
                    case "":
                    case ".":
                        elements.RemoveAt(i);
                        i--;
                        break;
                    case "..":
                        if (i == 1)
                        {
                            switch (elements[0])
                            {
                                case ".":
                                    elements[0] = "..";
                                    elements.RemoveAt(1);
                                    i--;
                                    break;
                                case "..":
                                case "":
                                    break;
                                default:
                                    elements[0] = ".";
                                    elements.RemoveAt(1);
                                    i--;
                                    break;
                            }
                        }
                        else
                        {
                            elements.RemoveAt(i);
                            elements.RemoveAt(i - 1);
                            i -= 2;
                        }
                        break;
                }
            }

            if (elements.Count > 1 && elements[0] == ".")
                elements.RemoveAt(0);

            return String.Join(Path.DirectorySeparatorChar, elements);
        }

        /// <summary>
        /// Gets a normalized path string.
        /// </summary>
        /// <param name="path">Path string to normalize.</param>
        /// <param name="basePath">If not null or empty and <paramref name="path"/> is relative, then the <paramref name="path"/> will be considered relative to this.</param>
        /// <returns>The fully qualified, normalized path string.</returns>
        public string NormalizePath(string path, string basePath)
        {
            if (string.IsNullOrEmpty(path) || path == ".")
                path = ".";
            if (Path.IsPathRooted(path) || String.IsNullOrEmpty(basePath))
                path = Path.GetFullPath(path);
            else
                path = Path.GetFullPath(Path.Combine(basePath, path));
            if (string.IsNullOrEmpty(Path.GetFileName(path)))
            {
                string d = Path.GetDirectoryName(path);
                if (!String.IsNullOrEmpty(d))
                    return d;
            }
            return path;
        }

        /// <summary>
        /// Gets a normalized path string.
        /// </summary>
        /// <param name="path">Path string to normalize.</param>
        /// <returns>The fully qualified, normalized path string.</returns>
        public string NormalizePath(string path) => NormalizePath(path, null);

        /// <summary>
        /// Determines whether two path strings are equal.
        /// </summary>
        /// <param name="x">Path string being compared.</param>
        /// <param name="y">Path string being compared to.</param>
        /// <param name="doNotNormalize">If <c>true</c>, then path strings are compared as-is; otherwise their fully qualified, normalized equivalents are compared.</param>
        /// <returns>><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
        public bool ArePathsEqual(string x, string y, bool doNotNormalize) => string.IsNullOrEmpty(x) ? string.IsNullOrEmpty(y) :
            (!string.IsNullOrEmpty(y) && (doNotNormalize ? _nameComparer.Equals(x, y) : _nameComparer.Equals(NormalizePath(x), NormalizePath(y))));

        /// <summary>
        /// Determines whether the two fully qualifed, normalized versions of the path strings are equal.
        /// </summary>
        /// <param name="x">Path string being compared.</param>
        /// <param name="y">Path string being compared to.</param>
        /// <returns>><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
        public bool ArePathsEqual(string x, string y) => ArePathsEqual(x, y, false);
        
        /// <summary>
        /// Determines whether a <seealso cref="DirectoryInfo"/> object represents a subdirectory within the current volume.
        /// </summary>
        /// <param name="directory"><seealso cref="DirectoryInfo"/> to test.</param>
        /// <returns><c>true</c> if <seealso cref="DirectoryInfo"/> object represents a subdirectory within the current volume; otherwise, <c>false</c>.</returns>
        public bool Contains(DirectoryInfo directory) => directory != null && ArePathsEqual(_root.FullName, directory.Root.FullName);
        
        /// <summary>
        /// Determines whether two file names are equals.
        /// </summary>
        /// <param name="x">File name being compared.</param>
        /// <param name="y">File name being compared to.</param>
        /// <returns>><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(string x, string y) => (string.IsNullOrEmpty(x)) ? string.IsNullOrEmpty(y) : !string.IsNullOrEmpty(y) && _nameComparer.Equals(x, y);

        /// <summary>
        /// Gets the hash code for a file name.
        /// </summary>
        /// <param name="obj">Value representing a file name.</param>
        /// <returns>Hashcode value for a file name.</returns>
        public int GetHashCode(string obj) => _nameComparer.GetHashCode(obj ?? "");

        #endregion
    }
}
