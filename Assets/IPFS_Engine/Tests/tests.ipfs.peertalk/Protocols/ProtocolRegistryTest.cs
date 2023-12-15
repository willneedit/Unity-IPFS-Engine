using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Protocols
{
    [TestFixture]
    public class ProtocolRegistryTest
    {
        [Test]
        public void PreRegistered()
        {
            CollectionAssert.Contains(ProtocolRegistry.Protocols.Keys, "/multistream/1.0.0");
            CollectionAssert.Contains(ProtocolRegistry.Protocols.Keys, "/plaintext/1.0.0");
        }

    }
}
