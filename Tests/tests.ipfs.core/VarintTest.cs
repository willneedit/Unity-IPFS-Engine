using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs
{
    [TestFixture]
    public class VarintTest
    {
        [Test]
        public void Zero()
        {
            var x = new byte[] { 0 };
            Assert.AreEqual(1, Varint.RequiredBytes(0));
            CollectionAssert.AreEqual(x, Varint.Encode(0));
            Assert.AreEqual(0, Varint.DecodeInt32(x));
        }
        
        [Test]
        public void ThreeHundred()
        {
            var x = new byte[] { 0xAC, 0x02 };
            Assert.AreEqual(2, Varint.RequiredBytes(300));
            CollectionAssert.AreEqual(x, Varint.Encode(300));
            Assert.AreEqual(300, Varint.DecodeInt32(x));
        }

        [Test]
        public void Decode_From_Offset()
        {
            var x = new byte[] { 0x00, 0xAC, 0x02 };
            Assert.AreEqual(300, Varint.DecodeInt32(x, 1));
        }

        [Test]
        public void MaxLong()
        {
            var x = "ffffffffffffffff7f".ToHexBuffer();
            Assert.AreEqual(9, Varint.RequiredBytes(long.MaxValue));
            CollectionAssert.AreEqual(x, Varint.Encode(long.MaxValue));
            Assert.AreEqual(long.MaxValue, Varint.DecodeInt64(x));
        }

        [Test]
        public void Encode_Negative()
        {
            ExceptionAssert.Throws<NotSupportedException>(() => Varint.Encode(-1));
        }

        [Test]
        public void TooBig_Int32()
        {
            var bytes = Varint.Encode((long)Int32.MaxValue + 1);
            ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt32(bytes));
        }

        [Test]
        public void TooBig_Int64()
        {
            var bytes = "ffffffffffffffffff7f".ToHexBuffer();
            ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt64(bytes));
        }

        [Test]
        public void Unterminated()
        {
            var bytes = "ff".ToHexBuffer();
            ExceptionAssert.Throws<InvalidDataException>(() => Varint.DecodeInt64(bytes));
        }

        [Test]
        public void Empty()
        {
            var bytes = new byte[0];
            ExceptionAssert.Throws<EndOfStreamException>(() => Varint.DecodeInt64(bytes));
        }

        [Test]
        public async void WriteAsync()
        {
            using (var ms = new MemoryStream())
            {
                await ms.WriteVarintAsync(long.MaxValue);
                ms.Position = 0;
                Assert.AreEqual(long.MaxValue, ms.ReadVarint64());
            }
        }

        [Test]
        public void WriteAsync_Negative()
        {
            var ms = new MemoryStream();
            ExceptionAssert.Throws<Exception>(() => ms.WriteVarintAsync(-1).Wait());
        }

        [Test]
        public void WriteAsync_Cancel()
        {
            var ms = new MemoryStream();
            var cs = new CancellationTokenSource();
            cs.Cancel();
            ExceptionAssert.Throws<TaskCanceledException>(() => ms.WriteVarintAsync(0, cs.Token).Wait());
        }

        [Test]
        public async void ReadAsync()
        {
            using (var ms = new MemoryStream("ffffffffffffffff7f".ToHexBuffer()))
            {
                var v = await ms.ReadVarint64Async();
                Assert.AreEqual(long.MaxValue, v);
            }
        }

        [Test]
        public void ReadAsync_Cancel()
        {
            var ms = new MemoryStream(new byte[] { 0 });
            var cs = new CancellationTokenSource();
            cs.Cancel();
            ExceptionAssert.Throws<TaskCanceledException>(() => ms.ReadVarint32Async(cs.Token).Wait());
        }

        [Test]
        public void Example()
        {
            for (long v = 1; v <= 0xFFFFFFFL; v = v << 4)
            {
                Console.Write($"| {v} (0x{v.ToString("x")}) ");
                Console.WriteLine($"| {Varint.Encode(v).ToHexString()} |");
            }
        }
    }
}
