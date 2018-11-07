using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    /// <summary>
    /// Interface for file stats
    /// </summary>
    public interface IFileStats : IEquatable<IFileStats>
    {
        /// <summary>
        /// Length of file.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Comparison group ID.
        /// </summary>
        Guid? GroupID { get; }

        /// <summary>
        /// 128-bit MD5 checksum.
        /// </summary>
        MD5Checksum? Checksum { get; }

        /// <summary>
        /// Date when file length was last verified.
        /// </summary>
        DateTime LengthLastVerifedUTC { get; }

        /// <summary>
        /// Date when checksum was last calculated.
        /// </summary>
        DateTime? LastCalculatedUTC { get; }

        /// <summary>
        /// Date when file was last compared to another file in the same group.
        /// </summary>
        DateTime? LastCompared { get; }

        /// <summary>
        /// Sets the date when the file length was last validated.
        /// </summary>
        /// <param name="lastValidated">Date when file length was last validated.</param>
        /// <returns>New object with the specified file length validation date.</returns>
        IFileStats ValidateLength(DateTime lastValidated);

        /// <summary>
        /// Creates a new object with an MD5 calculation.
        /// </summary>
        /// <param name="calculated">Date when MD5 hash code was calculated.</param>
        /// <param name="checksum">128-bit MD5 hash code.</param>
        /// <returns>A new object with the specified MD5 calculation.</returns>
        IFileMD5Stats WithMD5(DateTime calculated, MD5Checksum checksum);

        /// <summary>
        /// Creates a new object with a comparison group ID.
        /// </summary>
        /// <param name="compared">Date when file was compared and MD5 hash code was calculated.</param>
        /// <param name="checksum">128-bit MD5 hash code.</param>
        /// <param name="groupID">Unique identifier which identifies duplicate files.</param>
        /// <returns>A new object with the specified checksum and comparison group ID.</returns>
        IFileCompareStats AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID);
    }
}
