using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator
{
    public struct FileComparisonInfo : IFileComparisonInfo, IEquatable<FileComparisonInfo>
    {
        #region Fields

        private MD5Checksum _checksum;
        private Guid _comparisonGroupID;

        #endregion

        #region Properties

        public MD5Checksum Checksum => _checksum;

        public Guid ComparisonGroupID => _comparisonGroupID;

        Guid? IFileComparisonInfo.ComparisonGroupID => _comparisonGroupID;

        long IFileComparisonInfo.MD5LowBits => _checksum.HighBits;

        long IFileComparisonInfo.MD5HighBits => _checksum.LowBits;

        #endregion

        #region Constructors

        public FileComparisonInfo(Guid comparisonGroupID, MD5Checksum checksum)
        {
            _comparisonGroupID = comparisonGroupID;
            _checksum = checksum;
        }

        #endregion

        #region Methods

        #region Equals

        public bool Equals(FileComparisonInfo other) => _comparisonGroupID.Equals(other._comparisonGroupID) && _checksum.Equals(other._checksum);

        public bool Equals(IFileComparisonInfo other) => other != null && other is FileComparisonInfo && Equals((FileComparisonInfo)other);

        public override bool Equals(object obj) => obj != null && obj is FileComparisonInfo && Equals((FileComparisonInfo)obj);

        #endregion

        public override int GetHashCode() => _comparisonGroupID.GetHashCode();

        public override string ToString() => _checksum.ToString();

        #endregion
    }
}
