﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public interface IFsStructureElement
    {
        FsRootContainer Root { get; }
    }
}
