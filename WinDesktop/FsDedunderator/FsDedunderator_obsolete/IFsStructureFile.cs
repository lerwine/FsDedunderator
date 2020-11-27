using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsStructureFile : IFsStructureChild, IFsFileProperties
    {
        new IFsFileData Data { get; }
        new IFsStructureDirectory Parent { get; set; }
    }
}