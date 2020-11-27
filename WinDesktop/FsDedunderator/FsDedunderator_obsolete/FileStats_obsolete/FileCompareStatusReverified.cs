using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileCompareStatusReverified : IFileCompareStats, IEquatable<FileCompareStatusReverified>
    {
        private FileMD5StatsVerifiedLength _md5;
        private DateTime _lastCompared;
        private Guid _groupID;

        public Guid GroupID => _groupID;

        Guid? IFileStats.GroupID => _groupID;

        public DateTime LastCompared => _lastCompared;

        DateTime? IFileStats.LastCompared => _lastCompared;

        public MD5Checksum Checksum => _md5.Checksum;

        MD5Checksum? IFileStats.Checksum => _md5.Checksum;

        public DateTime LastCalculatedUTC => _md5.LastCalculatedUTC;

        DateTime? IFileStats.LastCalculatedUTC => _md5.LastCalculatedUTC;

        public long Length => _md5.Length;

        public DateTime LengthLastVerifedUTC => _md5.LengthLastVerifedUTC;

        public FileCompareStatusReverified(long length, DateTime lastVerified, MD5Checksum checksum, DateTime lastCalculated, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5StatsVerifiedLength(length, lastVerified, checksum, lastCalculated);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Calculated date must differ from verified and compared dates");
            _groupID = groupID;
        }

        public FileCompareStatusReverified(long length, DateTime lastVerified, byte[] checksum, DateTime lastCalculated, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5StatsVerifiedLength(length, lastVerified, checksum, lastCalculated);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Calculated date must differ from verified and compared dates");
            _groupID = groupID;
        }

        public FileCompareStatusReverified(FileMD5Stats stats, DateTime lastVerified, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5StatsVerifiedLength(stats, lastVerified);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Calculated date must differ from verified and compared dates");
            _groupID = groupID;
        }

        public FileCompareStatusReverified(FileMD5StatsVerifiedLength stats, Guid groupID, DateTime lastCompared)
        {
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (stats.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Calculated date must differ from verified and compared dates");
            _md5 = stats;
            _groupID = groupID;
        }

        public FileCompareStatusReverified(FileCompareStatsVerifiedMD5 stats, DateTime lastValidated)
        {
            _md5 = new FileMD5StatsVerifiedLength(stats, lastValidated);
            if (_md5.LastCalculatedUTC.Equals(stats.LastCompared))
                throw new InvalidOperationException("Calculated date must differ from verified and compared dates");
            _lastCompared = stats.LastCompared;
            _groupID = stats.GroupID;
        }

        public bool Equals(FileCompareStatusReverified other) => _groupID.Equals(other._groupID) && _lastCompared.Equals(other._lastCompared) && _md5.Equals(other._md5);

        public bool Equals(IFileCompareStats other) => other != null && other is FileCompareStatusReverified && Equals((FileCompareStatusReverified)other);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileCompareStatusReverified && Equals((FileCompareStatusReverified)other);

        public bool Equals(IFileStats other) => other != null && other is FileCompareStatusReverified && Equals((FileCompareStatusReverified)other);

        public override bool Equals(object obj) => obj != null && obj is FileCompareStatusReverified && Equals((FileCompareStatusReverified)obj);

        public override int GetHashCode() => _groupID.GetHashCode();

        public override string ToString() => _groupID.ToString("b") + " (" + _lastCompared.ToString("yyyy-MM-dd HH:mm:ss") + ") " + _md5.ToString();

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, groupID, compared);
            return new FileCompareStatusReverified(_md5, groupID, compared);
        }

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, checksum, groupID, compared);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, checksum, _md5.LastCalculatedUTC, groupID, compared);
        }

        public IFileCompareStats ToLastCompared(DateTime lastCompared)
        {
            lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (lastCompared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, _groupID, lastCompared);
            return new FileCompareStatusReverified(_md5, _groupID, lastCompared);
        }

        public IFileStats ValidateChecksum(long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (length == _md5.Length)
            {
                if (checksum.Equals(_md5.Checksum))
                {
                    if (lastCalculated.Equals(_lastCompared))
                        return new FileCompareStats(length, lastCalculated, checksum, _groupID);
                    return new FileCompareStatsVerifiedMD5(length, lastCalculated, checksum, _groupID, _lastCompared);
                }
                return new FileMD5Stats(length, lastCalculated, checksum);
            }
            return new FileStats(length, lastCalculated);
        }

        public IFileCompareStats ValidateChecksum(DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (lastCalculated.Equals(_lastCompared))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, _groupID, lastCalculated);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, _md5.Checksum, lastCalculated, _groupID, _lastCompared);

        }

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => ValidateChecksum(lastCalculated);

        public IFileCompareStats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (lastValidated.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStatsVerifiedMD5(_md5.Length, lastValidated, _md5.Checksum, _groupID, _lastCompared);
            return new FileCompareStatusReverified(_md5.Length, lastValidated, _md5.Checksum, _md5.LastCalculatedUTC, _groupID, _lastCompared);
        }

        public IFileStats ValidateLength(long length, DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (length == _md5.Length)
            {
                lastValidated = FileStats.NormalizeDateTime(lastValidated);
                if (lastValidated.Equals(_md5.LastCalculatedUTC))
                    return new FileCompareStatsVerifiedMD5(length, lastValidated, _md5.Checksum, _groupID, _lastCompared);
                return new FileCompareStatusReverified(length, lastValidated, _md5.Checksum, _md5.LastCalculatedUTC, _groupID, _lastCompared);
            }
            return new FileStats(length, lastValidated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum)
        {
            calculated = FileStats.NormalizeDateTime(calculated);
            if (calculated.Equals(_lastCompared))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LengthLastVerifedUTC, checksum, _groupID, calculated);
            return new FileCompareStatusReverified(_md5.Length, _md5.LengthLastVerifedUTC, checksum, calculated, _groupID, _lastCompared);
        }

        public FileMD5StatsVerifiedLength WithoutGroupID() => _md5;

        IFileMD5Stats IFileCompareStats.WithoutGroupID() => _md5;

        public FileStats WithoutMD5() => _md5.WithoutMD5();

        IFileStats IFileMD5Stats.WithoutMD5() => _md5.WithoutMD5();
    }
}