using Ipfs.Engine.Cryptography;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine
{
    
    [TestFixture]
    public class DiscoveryOptionsTest
    {
        [Test]
        public void Defaults()
        {
            var options = new DiscoveryOptions();
            Assert.IsNull(options.BootstrapPeers);
            Assert.IsFalse(options.DisableMdns);
        }
    }
}
