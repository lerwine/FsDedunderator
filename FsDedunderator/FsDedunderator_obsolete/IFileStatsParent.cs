using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FsDedunderator_obsolete.FileStats
{
    public interface IFileStatsParent : IDictionary, IList
    {
    }

    public interface IFileStatsParent<TKey, TValue> : IFileStatsParent, IDictionary<TKey, TValue>, IList<TValue>
        where TValue : IFileStats
    {
    }
}