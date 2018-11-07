namespace FsDedunderator
{
    /// <summary>
    /// Type of last check performed on a file.
    /// </summary>
    public enum FsCheckType
    {
        /// <summary>
        /// Only the file size was verified.
        /// </summary>
        SizeOnly = 0,

        /// <summary>
        /// File size and checksum was verified.
        /// </summary>
        Checksum,

        /// <summary>
        /// File group comparison was verified.
        /// </summary>
        GroupCompare
    }
}