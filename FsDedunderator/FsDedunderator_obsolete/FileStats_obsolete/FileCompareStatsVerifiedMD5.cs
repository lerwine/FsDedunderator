using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileCompareStatsVerifiedMD5 : IFileCompareStats, IEquatable<FileCompareStatsVerifiedMD5>
    {
        private FileMD5Stats _md5;
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

        DateTime IFileStats.LengthLastVerifedUTC => _md5.LastCalculatedUTC;

        public FileCompareStatsVerifiedMD5(long length, DateTime lastVerified, MD5Checksum checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5Stats(length, lastVerified, checksum);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedMD5(long length, DateTime lastVerified, byte[] checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5Stats(length, lastVerified, checksum);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedMD5(IFileStats stats, MD5Checksum checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5Stats(stats, checksum);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedMD5(IFileStats stats, byte[] checksum, Guid groupID, DateTime lastCompared)
        {
            _md5 = new FileMD5Stats(stats, checksum);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedMD5(IFileMD5Stats stats, Guid groupID, DateTime lastCompared)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _md5 = (stats is FileMD5Stats) ? (FileMD5Stats)stats : new FileMD5Stats(stats.Length, stats.LengthLastVerifedUTC, stats.Checksum);
            _lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = groupID;
        }

        public FileCompareStatsVerifiedMD5(IFileCompareStats stats, DateTime lastCalculated)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _md5 = new FileMD5Stats(stats.Length, lastCalculated, stats.Checksum);
            _lastCompared = FileStats.NormalizeDateTime(stats.LastCompared);
            if (_md5.LastCalculatedUTC.Equals(_lastCompared))
                throw new InvalidOperationException("Last verified date must be different from last calculated date");
            _groupID = stats.GroupID;
        }

        public bool Equals(FileCompareStatsVerifiedMD5 other) => _groupID.Equals(other._groupID) && _lastCompared.Equals(other._lastCompared) && _md5.Equals(other._md5);

        public bool Equals(IFileCompareStats other) => other != null && other is FileCompareStatsVerifiedMD5 && Equals((FileCompareStatsVerifiedMD5)other);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileCompareStatsVerifiedMD5 && Equals((FileCompareStatsVerifiedMD5)other);

        public bool Equals(IFileStats other) => other != null && other is FileCompareStatsVerifiedMD5 && Equals((FileCompareStatsVerifiedMD5)other);

        public override bool Equals(object obj) => obj != null && obj is FileCompareStatsVerifiedMD5 && Equals((FileCompareStatsVerifiedMD5)obj);

        public override int GetHashCode() => _groupID.GetHashCode();

        public override string ToString() => _groupID.ToString("b") + " (" + _lastCompared.ToString("yyyy-MM-dd HH:mm:ss") + ") " + _md5.ToString();

        public IFileStats ValidateChecksum(long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (length == _md5.Length && checksum.Equals(_md5.Checksum))
                return (lastCalculated.Equals(_lastCompared)) ? (IFileCompareStats)(new FileCompareStats(_md5.Length, lastCalculated, checksum, _groupID)) : new FileCompareStatsVerifiedMD5(this, lastCalculated);
            return new FileMD5Stats(_md5.Length, lastCalculated, _md5.Checksum);
        }

        public IFileCompareStats ValidateChecksum(DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (lastCalculated.Equals(_lastCompared))
                return new FileCompareStats(_md5.Length, lastCalculated, _md5.Checksum, _groupID);
            return new FileCompareStatsVerifiedMD5(this, lastCalculated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => ValidateChecksum(lastCalculated);

        public IFileCompareStats ToLastCompared(DateTime lastCompared)
        {
            lastCompared = FileStats.NormalizeDateTime(lastCompared);
            if (lastCompared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStats(_md5.Length, lastCompared, _md5.Checksum, _groupID);
            return new FileCompareStatsVerifiedMD5(_md5, _groupID, lastCompared);
        }

        public FileMD5Stats WithoutGroupID() => _md5;
        
        IFileMD5Stats IFileCompareStats.WithoutGroupID() => _md5;

        public IFileStats ValidateLength(long length, DateTime lastValidated)
        {
            if (_md5.Length == length)
                return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatusReverified(this, lastValidated);
            return new FileStats(length, lastValidated);
        }

        public IFileCompareStats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatusReverified(this, lastValidated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        public FileStats WithoutMD5() => _md5.WithoutMD5();

        IFileStats IFileMD5Stats.WithoutMD5() => _md5.WithoutMD5();

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum)
        {
            calculated = FileStats.NormalizeDateTime(calculated);
            if (_md5.LastCalculatedUTC.Equals(calculated))
            {
                if (checksum.Equals(_md5.Checksum))
                    return this;
                return new FileCompareStatsVerifiedMD5(_md5.Length, calculated, checksum, _groupID, _lastCompared);
            }
            if (_lastCompared.Equals(calculated))
                return new FileCompareStatsVerifiedLength(_md5.Length, _md5.LastCalculatedUTC, checksum, _groupID, calculated);
            return new FileCompareStatusReverified(_md5.Length, _md5.LastCalculatedUTC, checksum, calculated, _groupID, _lastCompared);
        }

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStats(_md5.Length, compared, checksum, groupID);
            return new FileCompareStatsVerifiedMD5(_md5.Length, _md5.LastCalculatedUTC, checksum, groupID, compared);
        }

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID)
        {
            compared = FileStats.NormalizeDateTime(compared);
            if (compared.Equals(_md5.LastCalculatedUTC))
                return new FileCompareStats(_md5.Length, compared, _md5.Checksum, groupID);
            return new FileCompareStatsVerifiedMD5(_md5.Length, _md5.LastCalculatedUTC, _md5.Checksum, groupID, compared);
        }
    }
}