using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;

namespace PeerTalk.Cryptography.Marshaling
{
    [TestFixture]
    public class MarshalPublicKey
    {
        [Test]
        public void Marshal_RSA()
        {
            Key key = Key.GenerateKeyPair("rsa");
            byte[] data = key.MarshalPublicKey();
        }

        [Test]
        public void Marshal_secp256k1()
        {
            Key key = Key.GenerateKeyPair("secp256k1");
            byte[] data = key.MarshalPublicKey();
        }

        [Test]
        public void Marshal_ed25519()
        {
            Key key = Key.GenerateKeyPair("ed25519");
            byte[] data = key.MarshalPublicKey();
            Assert.AreEqual(data.Length, 36); // Constant 32 bits key material with 4 bytes Protobuf header
        }
    }
}