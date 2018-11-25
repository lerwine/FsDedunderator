using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileMD5StatsVerifiedLength : IFileMD5Stats, IEquatable<FileMD5StatsVerifiedLength>
    {
        private FileStats _fileStats;
        private DateTime _lastCalculated;
        private MD5Checksum _checksum;

        public MD5Checksum Checksum => _checksum;

        MD5Checksum? IFileStats.Checksum => _checksum;

        public DateTime LastCalculatedUTC => _lastCalculated;

        DateTime? IFileStats.LastCalculatedUTC => _lastCalculated;

        public long Length => _fileStats.Length;

        public DateTime LengthLastVerifedUTC => _fileStats.LengthLastVerifedUTC;

        Guid? IFileStats.GroupID => null;

        DateTime? IFileStats.LastCompared => null;

        public FileMD5StatsVerifiedLength(long length, DateTime lastVerified, MD5Checksum checksum, DateTime lastCalculated)
        {
            _fileStats = new FileStats(length, lastVerified);
            _lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (_fileStats.LengthLastVerifedUTC.Equals(_lastCalculated))
                throw new InvalidOperationException("Last calculated date must be different than the last verified date");
            _checksum = checksum;
        }

        public FileMD5StatsVerifiedLength(long length, DateTime lastVerified, byte[] checksum, DateTime lastCalculated)
        {
            if (checksum == null)
                throw new ArgumentNullException("checksum");
            if (checksum.Length == 0)
                throw new ArgumentException("Checksum cannot have zero bytes", "checksum");
            _fileStats = new FileStats(length, lastVerified);
            _lastCalculated = FileStats.NormalizeDateTime(lastCalculated);
            if (_fileStats.LengthLastVerifedUTC.Equals(_lastCalculated))
                throw new InvalidOperationException("Last calculated date must be different than the last verified date");
            try { _checksum = new MD5Checksum(checksum); }
            catch (Exception e) { throw new ArgumentException(e.Message, "checksum", e); }
        }
        
        public FileMD5StatsVerifiedLength(IFileMD5Stats stats, DateTime lastVerified)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _fileStats = new FileStats(stats.Length, lastVerified);
            _lastCalculated = FileStats.NormalizeDateTime(stats.LastCalculatedUTC);
            if (_fileStats.LengthLastVerifedUTC.Equals(_lastCalculated))
                throw new InvalidOperationException("Last calculated date must be different than the last verified date");
            _checksum = stats.Checksum;
        }

        public bool Equals(FileMD5StatsVerifiedLength other) => _checksum.Equals(other._checksum) && _lastCalculated.Equals(other._lastCalculated) && _fileStats.Equals(other._fileStats);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileMD5StatsVerifiedLength && Equals((FileMD5StatsVerifiedLength)other);

        public bool Equals(IFileStats other) => other != null && other is FileMD5StatsVerifiedLength && Equals((FileMD5StatsVerifiedLength)other);

        public override bool Equals(object obj) => obj != null && obj is FileMD5StatsVerifiedLength && Equals((FileMD5StatsVerifiedLength)obj);

        public override int GetHashCode() => (_checksum.GetHashCode() & 0xFFF) | ((_lastCalculated.GetHashCode() & 0xFF) << 12) | (_fileStats.GetHashCode() << 20);

        public override string ToString() => _checksum.ToString() + " ("  + _lastCalculated.ToString("yyyy-MM-dd HH:mm:ss") + "):" + _fileStats.ToString();

        public IFileStats ValidateLength(long length, DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (length == _fileStats.Length)
                return (lastValidated.Equals(_lastCalculated)) ? (IFileMD5Stats)(new FileMD5Stats(_fileStats.Length, lastValidated, _checksum)) : this;
            return new FileStats(length, lastValidated);
        }

        public IFileMD5Stats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            return (lastValidated.Equals(_lastCalculated)) ? (IFileMD5Stats)(new FileMD5Stats(_fileStats.Length, lastValidated, _checksum)) : this;
        }

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        public FileMD5Stats ValidateChecksum(DateTime lastCalculated) => new FileMD5Stats(_fileStats.Length, lastCalculated, _checksum);

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => ValidateChecksum(lastCalculated);

        public FileStats WithoutMD5() => _fileStats;

        IFileStats IFileMD5Stats.WithoutMD5() => WithoutMD5();

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum)
        {
            calculated = FileStats.NormalizeDateTime(calculated);
            if (calculated.Equals(_fileStats.LengthLastVerifedUTC))
                return new FileMD5Stats(_fileStats.Length, calculated, _checksum);
            return new FileMD5StatsVerifiedLength(_fileStats.Length, _fileStats.LengthLastVerifedUTC, _checksum, calculated);
        }

        public FileCompareStats AsCompared(DateTime compared, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, _checksum, groupID);

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, _checksum, groupID);

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, checksum, groupID);
    }
}