using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    /// <summary>
    /// Interface for file stats with an MD5 checksum.
    /// </summary>
    public interface IFileMD5Stats : IFileStats, IEquatable<IFileMD5Stats>
    {
        /// <summary>
        /// 128-bit MD5 checksum.
        /// </summary>
        new MD5Checksum Checksum { get; }

        /// <summary>
        /// Date when checksum was last calculated.
        /// </summary>
        new DateTime LastCalculatedUTC { get; }

        /// <summary>
        /// Validates the file length.
        /// </summary>
        /// <param name="length">Length of file.</param>
        /// <param name="lastValidated">Date when file length was validated.</param>
        /// <returns>If length is different, then an object without the MD5 hashcode is returned; otherwise an object with an updated validation value is returneed.</returns>
        IFileStats ValidateLength(long length, DateTime lastValidated);

        /// <summary>
        /// Sets the date when the file length was last validated.
        /// </summary>
        /// <param name="lastValidated">Date when file length was last validated.</param>
        /// <returns>New object with the specified file length validation date.</returns>
        new IFileMD5Stats ValidateLength(DateTime lastValidated);

        /// <summary>
        /// Sets the date when the checksum was last calculated.
        /// </summary>
        /// <param name="lastCalculated">Date when checksum was last calculated.</param>
        /// <returns>A new object the specified checksum calculation date.</returns>
        IFileMD5Stats ValidateChecksum(DateTime lastCalculated);

        /// <summary>
        /// Gets an object without the MD5 hash code.
        /// </summary>
        /// <returns>An object without the MD5 hash code.</returns>
        IFileStats WithoutMD5();

        /// <summary>
        /// Creates a new object with a comparison group ID.
        /// </summary>
        /// <param name="compared">Date when file was compared and MD5 hash code was calculated.</param>
        /// <param name="groupID">Unique identifier which identifies duplicate files.</param>
        /// <returns>A new object with the specified comparison group ID.</returns>
        IFileCompareStats AsCompared(DateTime compared, Guid groupID);
    }
}
