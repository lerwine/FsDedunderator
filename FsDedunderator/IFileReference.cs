using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FsDedunderator
{
    public interface IFileReference : IContentInfo, IEquatable<IFileReference>
    {
        IContentGroup Content { get; }
        DirectoryFile File { get; }
    }
}