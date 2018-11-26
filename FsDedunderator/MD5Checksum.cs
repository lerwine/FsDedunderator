using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FsDedunderator
{
    /// <summary>
    /// Represents a 128-bit MD5 checksumm value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct MD5Checksum : IEquatable<MD5Checksum>
    {
        /// <summary>
        /// 
        /// </summary>
        public const int MD5ByteSize = 16;

        #region Fields

        private static readonly Regex HexPattern = new Regex(@"\s*([^a-fA-F\d\s]+)?([a-fA-F\d][a-fA-F\d])\s*([\-:,;]\s*)?", RegexOptions.Compiled);

        private static readonly Regex HexDigit = new Regex(@"(?:[\s\-:,;]|([a-fA-F\d])|\s*$)", RegexOptions.Compiled);

        [FieldOffset(0)]
        private readonly long _lowBits;

        [FieldOffset(0)]
        private readonly int _key0;

        [FieldOffset(0)]
        private readonly byte _b0;

        [FieldOffset(1)]
        private readonly byte _b1;

        [FieldOffset(2)]
        private readonly byte _b2;

        [FieldOffset(3)]
        private readonly byte _b3;

        [FieldOffset(4)]
        private readonly int _key1;

        [FieldOffset(4)]
        private readonly byte _b4;

        [FieldOffset(5)]
        private readonly byte _b5;

        [FieldOffset(6)]
        private readonly byte _b6;

        [FieldOffset(7)]
        private readonly byte _b7;

        [FieldOffset(8)]
        private readonly long _highBits;

        [FieldOffset(8)]
        private readonly int _key2;

        [FieldOffset(8)]
        private readonly byte _b8;

        [FieldOffset(9)]
        private readonly byte _b9;

        [FieldOffset(10)]
        private readonly byte _b10;

        [FieldOffset(11)]
        private readonly byte _b11;

        [FieldOffset(12)]
        private readonly int _key3;

        [FieldOffset(12)]
        private readonly byte _b12;

        [FieldOffset(13)]
        private readonly byte _b13;

        [FieldOffset(14)]
        private readonly byte _b14;

        [FieldOffset(15)]
        private readonly byte _b15;

        #endregion

        #region Properties

        /// <summary>
        /// Contains the lower 64 bits of the MD5 checksum value.
        /// </summary>
        public long LowBits => _lowBits;

        /// <summary>
        /// Contains the lower 64 bits of the MD5 checksum value.
        /// </summary>
        public int Key0 => _key0;

        /// <summary>
        /// Contains the lower 64 bits of the MD5 checksum value.
        /// </summary>
        public int Key1 => _key1;

        /// <summary>
        /// Contains the upper 64 bits of the MD5 checksum value.
        /// </summary>
        public long HighBits => _highBits;

        /// <summary>
        /// Contains the upper 64 bits of the MD5 checksum value.
        /// </summary>
        public int Key2 => _key2;

        /// <summary>
        /// Contains the upper 64 bits of the MD5 checksum value.
        /// </summary>
        public int Key3 => _key3;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key0"></param>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        public MD5Checksum(int key0, int key1, int key2, int key3)
        {
            _highBits = _lowBits = 0L;
            _b0 = _b1 = _b2 = _b3 = _b4 = _b5 = _b6 = _b7 = _b8 = _b9 = _b10 = _b11 = _b12 = _b13 = _b14 = _b15 = (byte)0;
            _key0 = key0;
            _key1 = key1;
            _key2 = key2;
            _key3 = key3;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MD5Checksum"/>.
        /// </summary>
        /// <param name="buffer">A <seealso cref="Byte"/> array representing the 128-bit MD5 checksum. A null value or empty array initializes a zero checksum value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Buffer was not null, empty or 16 bytes in length.</exception>
        public MD5Checksum(byte[] buffer)
        {
            _highBits = _lowBits = 0L;
            _key0 = _key1 = _key2 = _key3 = 0;
            if (buffer == null || buffer.Length == 0)
                _b0 = _b1 = _b2 = _b3 = _b4 = _b5 = _b6 = _b7 = _b8 = _b9 = _b10 = _b11 = _b12 = _b13 = _b14 = _b15 = (byte)0;
            else
            {
                if (buffer.Length != MD5ByteSize)
                    throw new ArgumentOutOfRangeException("buffer", "Buffer length must be 16 bytes");
                _b0 = buffer[0];
                _b1 = buffer[1];
                _b2 = buffer[2];
                _b3 = buffer[3];
                _b4 = buffer[4];
                _b5 = buffer[5];
                _b6 = buffer[6];
                _b7 = buffer[7];
                _b8 = buffer[8];
                _b9 = buffer[9];
                _b10 = buffer[10];
                _b11 = buffer[11];
                _b12 = buffer[12];
                _b13 = buffer[13];
                _b14 = buffer[14];
                _b15 = buffer[15];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the 16-byte array representing the MD5 checksum.
        /// </summary>
        /// <returns>A 16-byte array representing the MD5 checksum.</returns>
        public byte[] GetBuffer() { return new byte[] { _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7, _b8, _b9, _b10, _b11, _b12, _b13, _b14, _b15 }; }

        #region Equals

        /// <summary>
        /// Determines whether the current <see cref="MD5Checksum"/> is equal to another.
        /// </summary>
        /// <param name="other"><see cref="MD5Checksum"/> to compare to.</param>
        /// <returns><c>true</c> if the current <see cref="MD5Checksum"/> is equal to <paramref name="other"/>; othwerise, <c>false</c>.</returns>
        public bool Equals(MD5Checksum other) => _highBits == other._highBits && _lowBits == other._lowBits;

        /// <summary>
        /// Determines whether the current <see cref="MD5Checksum"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns><c>true</c> if <paramref name="other"/> is an <see cref="MD5Checksum"/> and is equal to the current <see cref="MD5Checksum"/>; othwerise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj != null && obj is MD5Checksum && Equals((MD5Checksum)obj);

        #endregion

        /// <summary>
        /// Gets the hashcode for the current <see cref="MD5Checksum"/>.
        /// </summary>
        /// <returns>The hashcode for the current <see cref="MD5Checksum"/>.</returns>
        public override int GetHashCode() => (int)(_lowBits & 0xFFFF) | ((int)(_highBits & 0xFFFF) << MD5ByteSize);

        /// <summary>
        /// Gets a hexidecimal string representation of the <see cref="MD5Checksum"/>.
        /// </summary>
        /// <returns>A hexidecimal string representation of the <see cref="MD5Checksum"/>.</returns>
        public override string ToString() => _highBits.ToString("x16") + _lowBits.ToString("x16");

        /// <summary>
        /// Parses a hexidecimal string into a <see cref="MD5Checksum"/> object.
        /// </summary>
        /// <param name="s">Hexidecimal string to parse.</param>
        /// <returns>The parsed <see cref="MD5Checksum"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> contains an invalid hexidecmal pair or does not represent a 128-bit hexidecimal value.</exception>
        public static MD5Checksum Parse(string s)
        {
            if (s == null || (s = s.TrimStart()).Length == 0)
                return new MD5Checksum();
            int startAt = 0;

            StringBuilder sb = new StringBuilder();
            while (startAt < s.Length)
            {
                Match m = HexPattern.Match(s, startAt);
                if (!m.Success || m.Groups[1].Success)
                {
                    if (sb.Length == 32)
                    {
                        m = HexDigit.Match(s, startAt);
                        if (m.Success && !m.Groups[1].Success)
                            break;
                    }
                    throw new FormatException("Invalid hexidecimal pair at offset " + startAt.ToString());
                }
                if (sb.Length == 32 && m.Groups[2].Index == startAt)
                    throw new FormatException("Too many hexidecimal characters at offset " + startAt.ToString());
                sb.Append(m.Groups[2].Value);
                startAt = m.Index + m.Length;
            }
            if (sb.Length < 32)
                throw new FormatException("Expected 16 hexidecimal pairs; Actual: " + (sb.Length / 2).ToString());
            
            return new MD5Checksum(BitConverter.GetBytes(long.Parse(sb.ToString(MD5ByteSize, MD5ByteSize), System.Globalization.NumberStyles.HexNumber)).Concat(BitConverter.GetBytes(long.Parse(sb.ToString(0, MD5ByteSize), System.Globalization.NumberStyles.HexNumber))).ToArray());
        }

        /// <summary>
        /// Attempts to parses a hexidecimal string into a <see cref="MD5Checksum"/> object.
        /// </summary>
        /// <param name="s">Hexidecimal string to parse.</param>
        /// <param name="value">The parsed <see cref="MD5Checksum"/>.</param>
        /// <returns><c>true</c> if <paramref name="s"/> could be parsed as a <see cref="MD5Checksum"/>; otherwise, false.</returns>
        public static bool TryParse(string s, out MD5Checksum value)
        {
            if (s == null || (s = s.TrimStart()).Length == 0)
            {
                value = new MD5Checksum();
                return false;
            }
            int startAt = 0;
            StringBuilder sb = new StringBuilder();
            while (startAt < s.Length)
            {
                Match m = HexPattern.Match(s, startAt);
                if (!m.Success || m.Groups[1].Success)
                {
                    if (sb.Length == 32)
                    {
                        m = HexDigit.Match(s, startAt);
                        if (m.Success && !m.Groups[1].Success)
                            break;
                    }
                    value = new MD5Checksum();
                    return false;
                }
                if (sb.Length == 32 && m.Groups[2].Index == startAt)
                {
                    value = new MD5Checksum();
                    return false;
                }
                sb.Append(m.Groups[2].Value);
                startAt = m.Index + m.Length;
            }
            if (sb.Length < 32)
            {
                value = new MD5Checksum();
                return false;
            }
            value = new MD5Checksum(BitConverter.GetBytes(long.Parse(sb.ToString(MD5ByteSize, MD5ByteSize), System.Globalization.NumberStyles.HexNumber)).Concat(BitConverter.GetBytes(long.Parse(sb.ToString(0, MD5ByteSize), System.Globalization.NumberStyles.HexNumber))).ToArray());
            return true;
        }

        #endregion
    }
}