using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public interface IFsStructureChild : IFsStructureElement, IEquatable<IFsStructureChild>
    {
        string Name { get; }
        IFsData Data { get; }
        IFsContainer Parent { get; set; }
    }
}