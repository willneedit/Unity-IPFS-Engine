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
    public class SwarmOptionsTest
    {
        [Test]
        public void Defaults()
        {
            var options = new SwarmOptions();
            Assert.IsNull(options.PrivateNetworkKey);
            Assert.AreEqual(8, options.MinConnections);
        }

    }
}
