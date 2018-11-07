using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public interface IFsFileProperties
    {
        long Length { get; }
        IFileComparisonInfo ComparisonInfo { get; }
    }
}