using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete
{
    public interface IFsFileProperties
    {
        long Length { get; }
        IFileComparisonInfo ComparisonInfo { get; }
    }
}