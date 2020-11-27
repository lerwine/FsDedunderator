using System;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileMD5Stats : IFileMD5Stats, IEquatable<FileMD5Stats>
    {
        private FileStats _fileStats;
        private MD5Checksum _checksum;

        public MD5Checksum Checksum => _checksum;

        MD5Checksum? IFileStats.Checksum => _checksum;

        public DateTime LastCalculatedUTC => _fileStats.LengthLastVerifedUTC;

        DateTime? IFileStats.LastCalculatedUTC => _fileStats.LengthLastVerifedUTC;

        public long Length => _fileStats.Length;

        DateTime IFileStats.LengthLastVerifedUTC => _fileStats.LengthLastVerifedUTC;

        Guid? IFileStats.GroupID => null;

        DateTime? IFileStats.LastCompared => null;

        public FileMD5Stats(long length, DateTime lastVerified, MD5Checksum checksum)
        {
            _fileStats = new FileStats(length, lastVerified);
            _checksum = checksum;
        }

        public FileMD5Stats(long length, DateTime lastVerified, byte[] checksum)
        {
            if (checksum == null)
                throw new ArgumentNullException("checksum");
            if (checksum.Length == 0)
                throw new ArgumentException("Checksum cannot have zero bytes", "checksum");
            _fileStats = new FileStats(length, lastVerified);
            try { _checksum = new MD5Checksum(checksum); }
            catch (Exception e) { throw new ArgumentException(e.Message, "checksum", e); }
        }

        public FileMD5Stats(IFileStats stats, MD5Checksum checksum)
        {
            if (stats == null)
                throw new ArgumentNullException("stats");
            _fileStats = (stats is FileStats) ? (FileStats)stats : new FileStats(stats.Length, stats.LengthLastVerifedUTC);
            _checksum = checksum;

        }

        public FileMD5Stats(IFileStats stats, byte[] checksum)
        {
            if (stats == null)
                throw new ArgumentNullException("status");
            if (checksum == null)
                throw new ArgumentNullException("checksum");
            if (checksum.Length == 0)
                throw new ArgumentException("Checksum cannot have zero bytes", "checksum");
            _fileStats = (stats is FileStats) ? (FileStats)stats : new FileStats(stats.Length, stats.LengthLastVerifedUTC);
            try { _checksum = new MD5Checksum(checksum); }
            catch (Exception e) { throw new ArgumentException(e.Message, "checksum", e); }

        }

        public bool Equals(FileMD5Stats other) => _checksum.Equals(other._checksum) && _fileStats.Equals(other._fileStats);

        public bool Equals(IFileMD5Stats other) => other != null && other is FileMD5Stats && Equals((FileMD5Stats)other);

        public bool Equals(IFileStats other) => other != null && other is FileMD5Stats && Equals((FileMD5Stats)other);

        public override bool Equals(object obj) => obj != null && obj is FileMD5Stats && Equals((FileMD5Stats)obj);

        public override int GetHashCode() => (_checksum.GetHashCode() & 0xFFFF) | (_fileStats.GetHashCode() << 16);

        public override string ToString() => _checksum.ToString() + ":" + _fileStats.ToString();

        public IFileStats ValidateLength(long length, DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            if (length == _fileStats.Length)
                return (lastValidated.Equals(_fileStats.LengthLastVerifedUTC)) ? (IFileMD5Stats)this: new FileMD5StatsVerifiedLength(this, lastValidated);
            return new FileStats(length, lastValidated);
        }

        public IFileMD5Stats ValidateLength(DateTime lastValidated)
        {
            lastValidated = FileStats.NormalizeDateTime(lastValidated);
            return (lastValidated.Equals(_fileStats.LengthLastVerifedUTC)) ? (IFileMD5Stats)this : new FileMD5StatsVerifiedLength(this, lastValidated);
        }

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => ValidateLength(lastValidated);

        public FileMD5Stats ValidateChecksum(DateTime lastCalculated) => new FileMD5Stats(_fileStats.Length, lastCalculated, _checksum);

        IFileMD5Stats IFileMD5Stats.ValidateChecksum(DateTime lastCalculated) => new FileMD5Stats(_fileStats.Length, lastCalculated, _checksum);

        public FileStats WithoutMD5() => _fileStats;

        IFileStats IFileMD5Stats.WithoutMD5() => _fileStats;

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum) => new FileMD5Stats(_fileStats.Length, calculated, checksum);

        public FileCompareStats AsCompared(DateTime compared, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, _checksum, groupID);

        IFileCompareStats IFileMD5Stats.AsCompared(DateTime compared, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, _checksum, groupID);

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID) => new FileCompareStats(_fileStats.Length, compared, checksum, groupID);
    }
}