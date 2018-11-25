using System.Collections.Generic;

namespace FsDedunderator
{
    /// <summary>
    /// Interface for an object that groups <seealso cref="IFileReference" /> objects according to their content characteristics.
    /// </summary>
    public interface IContentGroup : IContentInfo
    {
        /// <summary>
        /// Collection of files that are directly contained within the current <see cref="IContentGroup" />.
        /// </summary>
        IList<IFileReference> FileReferences { get; }

        /// <summary>
        /// Collection of files that are contained (directly or indirectly) within the current <see cref="IContentGroup" />.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IFileReference> GetAllFileReferences();
    }
}