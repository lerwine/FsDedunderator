using FsDedunderator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FsDedunderatorXUnitTest
{   
    public class MD5ChecksumTest
    {
        static byte[] GetByteArray(long lowBits, long highBits) => BitConverter.GetBytes(lowBits).Concat(BitConverter.GetBytes(highBits)).ToArray();

        [Fact]
        public void EmptyConstructorTest()
        {
            MD5Checksum target = new MD5Checksum();
            byte[] buffer = target.GetBuffer();
            Assert.Equal(GetByteArray(0L, 0L), buffer);
            Assert.Equal(0L, target.HighBits);
            Assert.Equal(0L, target.LowBits);
        }

        [Fact]
        public void NullArgConstructorTest()
        {
            MD5Checksum target = new MD5Checksum(null);
            byte[] buffer = target.GetBuffer();
            Assert.Equal(GetByteArray(0L, 0L), buffer);
            Assert.Equal(0L, target.HighBits);
            Assert.Equal(0L, target.LowBits);
        }


        [Fact]
        public void EmptyArgConstructorTest()
        {
            MD5Checksum target = new MD5Checksum(new byte[0]);
            byte[] buffer = target.GetBuffer();
            Assert.Equal(GetByteArray(0L, 0L), buffer);
            Assert.Equal(0L, target.HighBits);
            Assert.Equal(0L, target.LowBits);
        }

        [Fact]
        public void Array16ArgConstructorTest()
        {
            foreach (long highBits in new long[] { 0L, 0x12345678L, 0x32L })
            {
                foreach (long lowBits in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    MD5Checksum target = new MD5Checksum(GetByteArray(lowBits, highBits));
                    byte[] buffer = target.GetBuffer();
                    Assert.Equal(GetByteArray(lowBits, highBits), buffer);
                    Assert.Equal(highBits, target.HighBits);
                    Assert.Equal(lowBits, target.LowBits);
                }
            }
        }

        [Fact]
        public void ParseTest()
        {
            foreach (long highBits in new long[] { 0L, 0x12345678L, 0x32L })
            {
                string h = highBits.ToString("x16");
                foreach (long lowBits in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    MD5Checksum target = MD5Checksum.Parse(h + lowBits.ToString("x16"));
                    byte[] buffer = target.GetBuffer();
                    Assert.Equal(GetByteArray(lowBits, highBits), buffer);
                    Assert.Equal(highBits, target.HighBits);
                    Assert.Equal(lowBits, target.LowBits);
                }
            }
        }

        [Fact]
        public void TryParseTest()
        {
            foreach (long highBits in new long[] { 0L, 0x12345678L, 0x32L })
            {
                string h = highBits.ToString("x16");
                foreach (long lowBits in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    MD5Checksum target;
                    bool result = MD5Checksum.TryParse(h + lowBits.ToString("x16"), out target);
                    Assert.True(result);
                    byte[] buffer = target.GetBuffer();
                    Assert.Equal(GetByteArray(lowBits, highBits), buffer);
                    Assert.Equal(highBits, target.HighBits);
                    Assert.Equal(lowBits, target.LowBits);
                }
            }
        }

        [Fact]
        public void ToStringTest()
        {
            foreach (long highBits in new long[] { 0L, 0x12345678L, 0x32L })
            {
                string h = highBits.ToString("x16");
                foreach (long lowBits in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    MD5Checksum target = new MD5Checksum(GetByteArray(lowBits, highBits));
                    string s = target.ToString();
                    Assert.Equal(h + lowBits.ToString("x16"), s);
                }
            }
        }

        [Fact]
        public void TypedEqualsTest()
        {
            foreach (long highBitsX in new long[] { 0L, 0x12345678L, 0x32L })
            {
                foreach (long lowBitsX in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    MD5Checksum targetX = new MD5Checksum(GetByteArray(lowBitsX, highBitsX));
                    foreach (long highBitsY in new long[] { 0L, 0x12345678L, 0x32L })
                    {
                        foreach (long lowBitsY in new long[] { 0L, 0x12345678L, 0x32L })
                        {
                            MD5Checksum targetY = new MD5Checksum(GetByteArray(lowBitsY, highBitsY));
                            if (highBitsX == highBitsY && lowBitsX == lowBitsY)
                            {
                                Assert.True(targetX.Equals(targetY));
                                Assert.True(targetY.Equals(targetX));
                                Assert.Equal(targetX, targetY);
                            }
                            else
                            {
                                Assert.False(targetX.Equals(targetY));
                                Assert.False(targetY.Equals(targetX));
                                Assert.NotEqual(targetX, targetY);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void AssumptionTest()
        {// 10, 11, 12
            long value = 0x123456789a;
            byte[] buffer = BitConverter.GetBytes(value);
            Assert.Equal(8, buffer.Length);
            byte b4 = 0x12;
            byte b3 = 0x34;
            byte b2 = 0x56;
            byte b1 = 0x78;
            byte b0 = 0x9a;
            byte[] expected = new byte[] { b0, b1, b2, b3, b4, 0, 0, 0 };
            Assert.Equal(expected.Skip(4), buffer.Skip(4));
            Assert.Equal(expected.Take(4), buffer.Take(4));
        }

        [Fact]
        public void ObjectEqualsTest()
        {
            foreach (long highBitsX in new long[] { 0L, 0x12345678L, 0x32L })
            {
                foreach (long lowBitsX in new long[] { 0L, 0x12345678L, 0x32L })
                {
                    object targetX = new MD5Checksum(GetByteArray(lowBitsX, highBitsX));
                    foreach (long highBitsY in new long[] { 0L, 0x12345678L, 0x32L })
                    {
                        foreach (long lowBitsY in new long[] { 0L, 0x12345678L, 0x32L })
                        {
                            object targetY = new MD5Checksum(GetByteArray(lowBitsY, highBitsY));
                            if (highBitsX == highBitsY && lowBitsX == lowBitsY)
                            {
                                Assert.True(targetX.Equals(targetY));
                                Assert.True(targetY.Equals(targetX));
                                Assert.Equal(targetX, targetY);
                            }
                            else
                            {
                                Assert.False(targetX.Equals(targetY));
                                Assert.False(targetY.Equals(targetX));
                                Assert.NotEqual(targetX, targetY);
                            }
                        }
                    }
                }
            }
        }
    }
}
