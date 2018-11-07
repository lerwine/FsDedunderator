using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public interface IFsContainer : IFsStructureElement
    {
        IFsCollection Items { get; }
        VolumeInformation Volume { get; }
        IFsStructureDirectory NewDirectory(string name);
        IFsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime, IFileComparisonInfo comparisonInfo);
        IFsStructureFile NewFile(string name, long length, DateTime creationTime, DateTime lastWriteTime);
    }
}
