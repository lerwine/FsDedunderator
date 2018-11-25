using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsFileData : IFsData, IFsFileProperties
    {
        new ICollection<IFsStructureFile> Links { get; }
        void AddLink(IFsStructureFile structureFile);
        void RemoveLink(IFsStructureFile structureFile);
    }
}