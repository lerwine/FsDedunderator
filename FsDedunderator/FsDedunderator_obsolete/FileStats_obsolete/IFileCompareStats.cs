using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    /// <summary>
    /// File stats for an object that has been compared to another file.
    /// </summary>
    public interface IFileCompareStats : IFileMD5Stats, IEquatable<IFileCompareStats>
    {
        /// <summary>
        /// Identifies groups of duplicate files.
        /// </summary>
        new Guid GroupID { get; }

        /// <summary>
        /// Date when file was last compared to another file in the same group.
        /// </summary>
        new DateTime LastCompared { get; }

        /// <summary>
        /// Sets the date when the file length was last validated.
        /// </summary>
        /// <param name="lastValidated">Date when file length was last validated.</param>
        /// <returns>New object with the specified file length validation date.</returns>
        new IFileCompareStats ValidateLength(DateTime lastValidated);

        /// <summary>
        /// Validates the length and checksum.
        /// </summary>
        /// <param name="length">Length of file.</param>
        /// <param name="checksum">Checksum of file.</param>
        /// <param name="lastCalculated">Date when checksum of file was calculated.</param>
        /// <returns>If length is different, then an object without the group ID or MD5 hashcode is returned; If the checkusm is different, then an object without the group ID is returned; otherwise an object with an updated validation value is returneed.</returns>
        IFileStats ValidateChecksum(long length, MD5Checksum checksum, DateTime lastCalculated);

        /// <summary>
        /// Sets the date when the checksum was last calculated.
        /// </summary>
        /// <param name="lastCalculated">Date when checksum was last calculated.</param>
        /// <returns>A new object the specified checksum calculation date.</returns>
        new IFileCompareStats ValidateChecksum(DateTime lastCalculated);

        /// <summary>
        /// Sets date when file was last compared.
        /// </summary>
        /// <param name="lastCompared">Date when file was last compared.</param>
        /// <returns>A new object with the specified comparison date.</returns>
        IFileCompareStats ToLastCompared(DateTime lastCompared);

        /// <summary>
        /// Gets an object without the Group ID.
        /// </summary>
        /// <returns>A new object without the group ID.</returns>
        IFileMD5Stats WithoutGroupID();
    }
}
