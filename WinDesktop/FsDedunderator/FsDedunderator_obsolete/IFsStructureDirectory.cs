using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsStructureDirectory : IFsContainer, IFsStructureChild
    {
        new IFsDirectoryData Items { get; }
    }
}
