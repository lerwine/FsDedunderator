using System;

namespace FsDedunderator.FileStats_obsolete
{
    public struct FileStats : IFileStats, IEquatable<FileStats>
    {
        private DateTime _lengthLastVerifedUTC;
        private long _length;

        public static DateTime NormalizeDateTime(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                value = ((value.Kind == DateTimeKind.Local) ? value : DateTime.SpecifyKind(value, DateTimeKind.Local)).ToUniversalTime();
            return (value.Millisecond == 0) ? value : new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, 0, DateTimeKind.Utc);
        }

        public long Length => _length;

        Guid? IFileStats.GroupID => null;

        MD5Checksum? IFileStats.Checksum => null;

        public DateTime LengthLastVerifedUTC => _lengthLastVerifedUTC;

        DateTime? IFileStats.LastCalculatedUTC => null;

        DateTime? IFileStats.LastCompared => null;

        public FileStats(long length, DateTime lastVerified)
        {
            _length = length;
            _lengthLastVerifedUTC = NormalizeDateTime(lastVerified);
        }

        public bool Equals(FileStats other) => _length.Equals(other._length) && _lengthLastVerifedUTC.Equals(other._lengthLastVerifedUTC);

        public bool Equals(IFileStats other) => other != null && other is FileStats && Equals((FileStats)other);

        public override bool Equals(object obj) => obj != null && obj is FileStats && Equals((FileStats)obj);

        public override int GetHashCode() => (int)(_length & 0xFFFFFF) | (_lengthLastVerifedUTC.GetHashCode() << 24);

        public override string ToString() => _length.ToString() + " [" + _lengthLastVerifedUTC.ToString("yyyy-MM-dd HH:mm:ss") + "]";

        public FileStats ValidateLength(DateTime lastValidated) => new FileStats(_length, lastValidated);

        IFileStats IFileStats.ValidateLength(DateTime lastValidated) => new FileStats(_length, lastValidated);

        public FileMD5Stats WithMD5(DateTime calculated, MD5Checksum checksum) => new FileMD5Stats(_length, calculated, checksum);

        IFileMD5Stats IFileStats.WithMD5(DateTime calculated, MD5Checksum checksum) => new FileMD5Stats(_length, calculated, checksum);

        public FileCompareStats AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID) => new FileCompareStats(_length, compared, checksum, groupID);

        IFileCompareStats IFileStats.AsCompared(DateTime compared, MD5Checksum checksum, Guid groupID) => new FileCompareStats(_length, compared, checksum, groupID);
    }
}
