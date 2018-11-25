using System;

namespace FsDedunderator_obsolete
{
    public interface IFileComparisonInfo : IEquatable<IFileComparisonInfo>
    {
        long MD5LowBits { get; }
        long MD5HighBits { get; }
        Guid? ComparisonGroupID { get; }
    }
}