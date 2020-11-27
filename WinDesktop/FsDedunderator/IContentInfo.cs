namespace FsDedunderator
{
    /// <summary>
    /// Interface for an object that contains characteristics of file contents.
    /// </summary>
    public interface IContentInfo
    {
        /// <summary>
        /// Length of file in bytes.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// MD5 Checksum value of file contents.
        /// </summary>
        MD5Checksum? Checksum { get; }
    }
}