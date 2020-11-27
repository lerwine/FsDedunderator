using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// Represents a collection of filesystem volumes.
    /// </summary>
    public class VolumeComponentList : LinkedComponentList<VolumeComponentList, DirectoryVolume>
    {
        /// <summary>
        /// Imports file information.
        /// </summary>
        /// <param name="file">File information to import.</param>
        /// <returns>The imported directory file object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name=""/></exception>
        /// <exception cref="FileNotFoundException"><paramref name=""/></exception>
        public DirectoryFile Import(FileInfo file)
        {
            throw new NotImplementedException();
        }

        private FileDirectory Import(DirectoryInfo directory)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootPathName"></param>
        /// <param name="volumeNameBuffer"></param>
        /// <param name="volumeNameSize"></param>
        /// <param name="volumeSerialNumber"></param>
        /// <param name="maximumComponentLength"></param>
        /// <param name="fileSystemFlags"></param>
        /// <param name="fileSystemNameBuffer"></param>
        /// <param name="nFileSystemNameSize"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetVolumeInformation(string rootPathName, StringBuilder volumeNameBuffer, int volumeNameSize, out uint volumeSerialNumber,
            out uint maximumComponentLength, out FileSystemFeature fileSystemFlags, StringBuilder fileSystemNameBuffer, int nFileSystemNameSize);

    }
}