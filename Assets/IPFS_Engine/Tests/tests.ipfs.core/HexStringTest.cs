using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;

namespace Ipfs
{
    [TestFixture]
    public class HexStringTest
    {
        [Test]
        public void Encode()
        {
            var buffer = Enumerable.Range(byte.MinValue, byte.MaxValue).Select(b => (byte) b).ToArray();
            var lowerHex = string.Concat(buffer.Select(b => b.ToString("x2")).ToArray());
            var upperHex = string.Concat(buffer.Select(b => b.ToString("X2")).ToArray());

            Assert.AreEqual(lowerHex, buffer.ToHexString(), "encode default");
            Assert.AreEqual(lowerHex, buffer.ToHexString("G"), "encode general");
            Assert.AreEqual(lowerHex, buffer.ToHexString("x"), "encode lower");
            Assert.AreEqual(upperHex, buffer.ToHexString("X"), "encode upper");
        }

        [Test]
        public void Decode()
        {
            var buffer = Enumerable.Range(byte.MinValue, byte.MaxValue).Select(b => (byte)b).ToArray();
            var lowerHex = string.Concat(buffer.Select(b => b.ToString("x2")).ToArray());
            var upperHex = string.Concat(buffer.Select(b => b.ToString("X2")).ToArray());

            CollectionAssert.AreEqual(buffer, lowerHex.ToHexBuffer(), "decode lower");
            CollectionAssert.AreEqual(buffer, upperHex.ToHexBuffer(), "decode upper");
        }

        [Test]
        public void InvalidFormatSpecifier()
        {
            ExceptionAssert.Throws<FormatException>(() => HexString.Encode(new byte[0], "..."));
        }

        [Test]
        public void InvalidHexStrings()
        {
            ExceptionAssert.Throws<InvalidDataException>(() => HexString.Decode("0"));
            ExceptionAssert.Throws<InvalidDataException>(() => HexString.Decode("0Z"));
        }
    }
}
