using System;

namespace FsDedunderator
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileDirectory : IEquatable<IFileDirectory>
    {
        /// <summary>
        /// 
        /// </summary>
        DirectoryNode.DirectoryList Contents { get; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        IFileDirectory Parent { get; }

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
        DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length, MD5Checksum checksum, DateTime lastCalculated);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="creationTime"></param>
        /// <param name="lastModificationTime"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        DirectoryFile Add(string name, DateTime creationTime, DateTime lastModificationTime, long length);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        FileDirectory Add(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DirectoryVolume GetVolume();
    }
}