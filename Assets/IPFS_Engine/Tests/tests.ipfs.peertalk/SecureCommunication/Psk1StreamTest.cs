using NUnit.Framework;
using PeerTalk.Cryptography;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace PeerTalk.SecureCommunication
{
    [TestFixture]
    public class Psk1StreamTest
    {
        [Test]
        public void BadKeyLength()
        {
            var psk = new PreSharedKey();
            Assert.Throws<Exception>(() => new Psk1Stream(Stream.Null, psk));
        }

        [Test]
        public void FirstWriteSendsNonce()
        {
            var psk = new PreSharedKey().Generate();

            var insecure = new MemoryStream();
            var secure = new Psk1Stream(insecure, psk);
            secure.WriteByte(0x10);
            Assert.AreEqual(24 + 1, insecure.Length);

            insecure = new MemoryStream();
            secure = new Psk1Stream(insecure, psk);
            secure.Write(new byte[10], 0, 10);
            Assert.AreEqual(24 + 10, insecure.Length);

            insecure = new MemoryStream();
            secure = new Psk1Stream(insecure, psk);
            secure.WriteAsync(new byte[12], 0, 12).Wait();
            Assert.AreEqual(24 + 12, insecure.Length);
        }

        [Test]
        public void Roundtrip()
        {
            var psk = new PreSharedKey().Generate();
            var plain = new byte[] { 1, 2, 3 };
            var plain1 = new byte[3];
            var plain2 = new byte[3];

            var insecure = new MemoryStream();
            var secure = new Psk1Stream(insecure, psk);
            secure.Write(plain, 0, plain.Length);
            secure.Flush();

            insecure.Position = 0;
            secure = new Psk1Stream(insecure, psk);
            secure.Read(plain1, 0, plain1.Length);
            CollectionAssert.AreEqual(plain, plain1);

            insecure.Position = 0;
            secure = new Psk1Stream(insecure, psk);
            secure.ReadAsync(plain2, 0, plain2.Length).Wait();
            CollectionAssert.AreEqual(plain, plain2);
        }

        [Test]
        public void ReadingInvalidNonce()
        {
            var psk = new PreSharedKey().Generate();
            var secure = new Psk1Stream(Stream.Null, psk);
            Assert.Throws<EndOfStreamException>(() => secure.ReadByte());
        }
    }
}
