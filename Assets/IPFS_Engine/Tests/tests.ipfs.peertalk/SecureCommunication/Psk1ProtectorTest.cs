using NUnit.Framework;
using PeerTalk.Cryptography;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace PeerTalk.SecureCommunication
{
    [TestFixture]
    public class Psk1ProtectorTest
    {
        [Test]
        public async Task Protect()
        {
            var psk = new PreSharedKey().Generate();
            var protector = new Psk1Protector { Key = psk };
            var connection = new PeerConnection { Stream = Stream.Null };
            var protectedStream = await protector.ProtectAsync(connection);
            Assert.AreSame(protectedStream.GetType(), typeof(Psk1Stream));
        }

    }
}
