using System;

namespace FsDedunderator
{
    public interface IFileComparisonInfo : IEquatable<IFileComparisonInfo>
    {
        long MD5LowBits { get; }
        long MD5HighBits { get; }
        Guid? ComparisonGroupID { get; }
    }
}