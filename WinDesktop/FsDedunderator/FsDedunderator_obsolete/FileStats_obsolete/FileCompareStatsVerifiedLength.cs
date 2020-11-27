using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileCompareStatsVerifiedLength : IFileCompareStats, IEquatable<FileCompareStatsVerifiedLength>
    {
        private FileMD5StatsVerifiedLength _md5;
        private Guid _groupID;

        public Guid GroupID => _groupID;

        Guid? IFileStats.GroupID => _groupID;

        public DateTime LastCompared => _md5.LastCalculatedUTC;

        DateTime? IFileStats.LastCompared => _md5.LastCalculatedUTC;

        public MD5Checksum Checksum => _md5.Checksum;

        MD5Checksum? IFileStats.Checksum => _md5.Checksum;

        DateTime IFileMD5Stats.LastCalculatedUTC => _md5.LastCalculatedUTC;

        DateTime? IFileStats.LastCalculatedUTC => _md5.LastCalculatedUTC;

        public long Length => _md5.Length;

        public DateTime LengthLastVerifedUTC => _md5.LengthLastVerifedUTC;

        public FileCompareStatsVerifiedLength(long length, DateTime lastVerified, MD5Checksum checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5StatsVerifiedLength(length, lastVerified, checksum, lastCompared);
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedLength(long length, DateTime lastVerified, byte[] checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5StatsVerifiedLength(length, lastVerified, checksum, lastCompared);
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedLength(FileMD5StatsVerifiedLength stats, Guid groupID)
        {
            _md5 = stats;
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedLength(IFileCompareStats stats, DateTime lastVerified)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _md5 = new FileMD5StatsVerifiedLength(stats, lastVerified);
            _groupID = stats.GroupID;
        }

        public bool Equals(FileCompareStatsVerifiedLength other) => _groupID.Equals(other._groupID) && _md5.Equals(other._md5);

        public bool Equals(IFileCompareStats other) => other != null && other is FileCompareStatsVerifiedLength && Equals((FileCompareStatsVerifiedLength)other);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileCompareStatsVerifiedLength && Equals((FileCompareStatsVerifiedLength)other);

        public bool Equals(IFileStats other) => other != null && other is FileCompareStatsVerifiedLength && Equals((FileCompareStatsVerifiedLength)other);

        public override bool Equals(object obj) => obj != null && obj is FileCompareStatsVerifiedLength && Equals((FileCompareStatsVerifiedLength)obj);

        public override int GetHashCode() => _groupID.GetHashCode();

        public override string ToString() => _groupID.ToString("b") + " " + _md5.ToString();

        public IFileCompareStats ToLastCompared(DateTime lastCompared)
        {
            lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (lastCompared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStats(_md5.Length, lastCompared, _md5.Checksum, _groupID);
            if (lastCompared.Equals(_md5.LengthLastVerifedUTC))
                return new FileCompareStatusReverified(_md5, _groupID, lastCompared);
            return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, _groupID, lastCompared);
        }

        public FileMD5StatsVerifiedLength WithoutGroupID() => _md5;

        IFileMD5Stats IFileCompareStats.WithoutGroupID() => _md5;

        public IFileStats ValidateLength(long length, DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (_md5.Length == length)
                return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)(new FileCompareStats(length, lastValidated, _md5.Checksum, _groupID)) : new FileCompareStatsVerifiedLength(this, lastValidated);
            return new FileStats(length, lastValidated);
        }

        public IFileCompareStats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)(new FileCompareStats(_md5.Length, lastValidated, _md5.Checksum, _groupID)) : new FileCompareStatsVerifiedLength(this, lastValidated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        public IFileStats ValidateChecksum(long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            if (_md5.Length == length)
            {
                if (_md5.Checksum.Equals(checksum))
                    return new FileCompareStats(_md5.Length, lastCalculated, _md5.Checksum, _groupID);
                return new FileMD5Stats(_md5.Length, lastCalculated, _md5.Checksum);
            }
            return new FileStats(length, lastCalculated);
        }

        public IFileCompareStats ValidateChecksum(DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (lastCalculated.Equals(_md5.LengthLastVerifedUTC))
                return new FileCompareStats(_md5.Length, lastCalculated, _md5.Checksum, _groupID);
            if (lastCalculated.Equals(_md5.LastCalculatedUTC))
                return this;
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, lastCalculated, _groupID, _md5.LastCalculatedUTC);
        }

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => ValidateChecksum(lastCalculated);

        public FileStats WithoutMD5() => _md5.WithoutMD5();

        IFileStats IFileMD5Stats.WithoutMD5() => _md5.WithoutMD5();

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum)
        {
            calculated = FileStats.NormalizeDateTime(calculated);
            if (calculated.Equals(_md5.LengthLastVerifedUTC))
                return new FileCompareStats(_md5.Length, calculated, checksum, _groupID);
            if (calculated.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, checksum, _groupID, _md5.LastCalculatedUTC);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, checksum, calculated, _groupID, _md5.LastCalculatedUTC);
        }

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LengthLastVerifedUTC))
                return new FileCompareStats(_md5.Length, compared, checksum, groupID);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, checksum, groupID, _md5.LastCalculatedUTC);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, checksum, _md5.LastCalculatedUTC, groupID, compared);
        }

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LengthLastVerifedUTC))
                return new FileCompareStats(_md5.Length, compared, _md5.Checksum, groupID);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, groupID, _md5.LastCalculatedUTC);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, _md5.LastCalculatedUTC, groupID, compared);
        }
    }
}