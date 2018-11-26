using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileReference : IContentInfo, IEquatable<IFileReference>
    {
        /// <summary>
        /// 
        /// </summary>
        IContentGroup Content { get; }

        /// <summary>
        /// 
        /// </summary>
        DirectoryFile File { get; }
    }
}