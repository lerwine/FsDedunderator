using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileCompareStats : IFileCompareStats, IEquatable<FileCompareStats>
    {
        private FileMD5Stats _md5;
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

        DateTime IFileStats.LengthLastVerifedUTC => _md5.LastCalculatedUTC;

        public FileCompareStats(long length, DateTime lastCompared, MD5Checksum checksum, Guid groupID)
        {
            _md5 = new FileMD5Stats(length, lastCompared, checksum);
            _groupID = groupID;
        }

        public FileCompareStats(long length, DateTime lastCompared, byte[] checksum, Guid groupID)
        {
            _md5 = new FileMD5Stats(length, lastCompared, checksum);
            _groupID = groupID;
        }

        public FileCompareStats(IFileStats stats, MD5Checksum checksum, Guid groupID)
        {
            _md5 = new FileMD5Stats(stats, checksum);
            _groupID = groupID;

        }

        public FileCompareStats(IFileStats stats, byte[] checksum, Guid groupID)
        {
            _md5 = new FileMD5Stats(stats, checksum);
            _groupID = groupID;
        }

        public FileCompareStats(IFileMD5Stats stats, Guid groupID)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _md5 = (stats is FileMD5Stats) ? (FileMD5Stats)stats : new FileMD5Stats(stats.Length, stats.LengthLastVerifedUTC, stats.Checksum);
            _groupID = groupID;
        }

        public bool Equals(FileCompareStats other) => _groupID.Equals(other._groupID) && _md5.Equals(other._md5);

        public bool Equals(IFileCompareStats other) => other != null && other is FileCompareStats && Equals((FileCompareStats)other);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileCompareStats && Equals((FileCompareStats)other);

        public bool Equals(IFileStats other) => other != null && other is FileCompareStats && Equals((FileCompareStats)other);

        public override bool Equals(object obj) => obj != null && obj is FileCompareStats && Equals((FileCompareStats)obj);

        public override int GetHashCode() => _groupID.GetHashCode();

        public override string ToString() => _groupID.ToString("b") + " " + _md5.ToString();

        public IFileCompareStats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatsVerifiedLength(this, lastValidated);
        }

        IFileStats IFileMD5Stats.ValidateLength(long length, DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (length == _md5.Length)
                return (lastValidated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatsVerifiedLength(this, lastValidated);
            return new FileStats(_md5.Length, lastValidated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        public IFileStats ValidateChecksum(long length, MD5Checksum checksum, DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (length == _md5.Length && checksum.Equals(_md5.Checksum))
                return (lastCalculated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatsVerifiedLength(this, lastCalculated);
            return new FileMD5Stats(_md5.Length, lastCalculated, _md5.Checksum);
        }

        public IFileCompareStats ValidateChecksum(DateTime lastCalculated)
        {
            lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            return (lastCalculated.Equals(_md5.LastCalculatedUTC)) ? (IFileCompareStats)this : new FileCompareStatsVerifiedMD5(this, lastCalculated);
        }

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => ValidateChecksum(lastCalculated);

        public FileCompareStats ToLastCompared(DateTime lastCompared) => new FileCompareStats(_md5.Length, _md5.LastCalculatedUTC, _md5.Checksum, _groupID);

        IFileCompareStats IFileCompareStats.ToLastCompared(DateTime lastCompared) => ToLastCompared(lastCompared);

        public FileMD5Stats WithoutGroupID() => _md5;

        IFileMD5Stats IFileCompareStats.WithoutGroupID() => _md5;

        public FileStats WithoutMD5() => _md5.WithoutMD5();

        IFileStats IFileMD5Stats.WithoutMD5() => _md5.WithoutMD5();

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum) => new FileCompareStats(_md5.Length, calculated, checksum, _groupID);

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID) => new FileCompareStats(_md5.Length, compared, checksum, groupID);

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID) => new FileCompareStats(_md5.Length, compared, _md5.Checksum, groupID);
    }
}
