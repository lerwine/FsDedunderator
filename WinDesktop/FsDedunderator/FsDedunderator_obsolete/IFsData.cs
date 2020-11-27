using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsData
    {
        ICollection<IFsStructureChild> Links { get; }
        void AddLink(IFsStructureChild structureFile);
        void RemoveLink(IFsStructureChild structureFile);
    }
}
