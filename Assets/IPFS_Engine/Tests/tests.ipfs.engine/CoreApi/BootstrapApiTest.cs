using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class BootstapApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;
        MultiAddress somewhere = "/ip4/127.0.0.1/tcp/4009/ipfs/QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rAQ";

        [Test]
        public async void Add_Remove()
        {
            var addr = await ipfs.Bootstrap.AddAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsTrue(addrs.Any(a => a == somewhere));

            addr = await ipfs.Bootstrap.RemoveAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsFalse(addrs.Any(a => a == somewhere));
        }

        [Test]
        public async void List()
        {
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsNotNull(addrs);
            Assert.AreNotEqual(0, addrs.Count());
        }

        [Test]
        public async void Remove_All()
        {
            var original = await ipfs.Bootstrap.ListAsync();
            await ipfs.Bootstrap.RemoveAllAsync();
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.AreEqual(0, addrs.Count());
            foreach (var addr in original)
            {
                await ipfs.Bootstrap.AddAsync(addr);
            }
        }

        [Test]
        public async void Add_Defaults()
        {
            var original = await ipfs.Bootstrap.ListAsync();
            await ipfs.Bootstrap.RemoveAllAsync();
            try
            {
                await ipfs.Bootstrap.AddDefaultsAsync();
                var addrs = await ipfs.Bootstrap.ListAsync();
                Assert.AreNotEqual(0, addrs.Count());
            }
            finally
            {
                await ipfs.Bootstrap.RemoveAllAsync();
                foreach (var addr in original)
                {
                    await ipfs.Bootstrap.AddAsync(addr);
                }
            }
        }

        [Test]
        public async void Override_FactoryDefaults()
        {
            var original = ipfs.Options.Discovery.BootstrapPeers;
            try
            {
                ipfs.Options.Discovery.BootstrapPeers = new MultiAddress[0];
                var addrs = await ipfs.Bootstrap.ListAsync();
                Assert.AreEqual(0, addrs.Count());

                ipfs.Options.Discovery.BootstrapPeers = new MultiAddress[1]
                    { somewhere };
                addrs = await ipfs.Bootstrap.ListAsync();
                Assert.AreEqual(1, addrs.Count());
                Assert.AreEqual(somewhere, addrs.First());
            }
            finally
            {
                ipfs.Options.Discovery.BootstrapPeers = original;
            }
        }

    }
}
