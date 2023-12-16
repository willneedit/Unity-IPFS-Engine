using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class StatsApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;

        [Test]
        public void Exists()
        {
            Assert.IsNotNull(ipfs.Stats);
        }

        [Test]
        public async void SmokeTest()
        {
            var bandwidth = await ipfs.Stats.BandwidthAsync();
            var bitswap = await ipfs.Stats.BitswapAsync();
            var repository = await ipfs.Stats.RepositoryAsync();
        }

    }
}
