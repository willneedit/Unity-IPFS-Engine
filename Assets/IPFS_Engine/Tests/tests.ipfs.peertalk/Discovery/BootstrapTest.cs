﻿using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Discovery
{
    
    [TestFixture]
    public class BootstrapTest
    {
        [Test]
        public void NullListAsync()
		{
			Task.Run(NullList).Wait();
		}

		public async Task NullList()
        {
            var bootstrap = new Bootstrap { Addresses = null };
            int found = 0;
            bootstrap.PeerDiscovered += (s, e) =>
            {
                ++found;
            };
            await bootstrap.StartAsync();
            Assert.AreEqual(0, found);
        }

        [Test]
        public void DiscoveredAsync()
		{
			Task.Run(Discovered).Wait();
		}

		public async Task Discovered()
        {
            var bootstrap = new Bootstrap
            {
                Addresses = new MultiAddress[]
                {
                    "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ",
                    "/ip4/104.131.131.83/tcp/4001/p2p/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ"
                }
            };
            int found = 0;
            bootstrap.PeerDiscovered += (s, peer) =>
            {
                Assert.IsNotNull(peer);
                Assert.AreEqual("QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ", peer.Id.ToBase58());
                CollectionAssert.AreEqual(bootstrap.Addresses.ToArray(), peer.Addresses.ToArray());
                ++found;
            };
            await bootstrap.StartAsync();
            Assert.AreEqual(1, found);
        }

        [Test]
        public void Discovered_Multiple_PeersAsync()
		{
			Task.Run(Discovered_Multiple_Peers).Wait();
		}

		public async Task Discovered_Multiple_Peers()
        {
            var bootstrap = new Bootstrap
            {
                Addresses = new MultiAddress[]
                {
                    "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ",
                    "/ip4/127.0.0.1/tcp/4001/ipfs/QmdpwjdB94eNm2Lcvp9JqoCxswo3AKQqjLuNZyLixmCM1h",
                    "/ip4/104.131.131.83/tcp/4001/p2p/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ",
                    "/ip6/::/tcp/4001/p2p/QmdpwjdB94eNm2Lcvp9JqoCxswo3AKQqjLuNZyLixmCM1h"
                }
            };
            int found = 0;
            bootstrap.PeerDiscovered += (s, peer) =>
            {
                Assert.IsNotNull(peer);
                ++found;
            };
            await bootstrap.StartAsync();
            Assert.AreEqual(2, found);
        }

        [Test]
        public void Stop_Removes_EventHandlersAsync()
		{
			Task.Run(Stop_Removes_EventHandlers).Wait();
		}

		public async Task Stop_Removes_EventHandlers()
        {
            var bootstrap = new Bootstrap
            {
                Addresses = new MultiAddress[]
                {
                    "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ"
                }
            };
            int found = 0;
            bootstrap.PeerDiscovered += (s, e) =>
            {
                Assert.IsNotNull(e);
                ++found;
            };
            await bootstrap.StartAsync();
            Assert.AreEqual(1, found);
            await bootstrap.StopAsync();

            await bootstrap.StartAsync();
            Assert.AreEqual(1, found);
        }

        [Test]
        public void Missing_ID_Is_IgnoredAsync()
		{
			Task.Run(Missing_ID_Is_Ignored).Wait();
		}

		public async Task Missing_ID_Is_Ignored()
        {
            var bootstrap = new Bootstrap
            {
                Addresses = new MultiAddress[]
                {
                    "/ip4/104.131.131.82/tcp/4002",
                    "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ"
                }
            };
            int found = 0;
            bootstrap.PeerDiscovered += (s, e) =>
            {
                Assert.IsNotNull(e);
                Assert.IsNotNull(e.Addresses);
                Assert.AreEqual(bootstrap.Addresses.Last(), e.Addresses.First());
                ++found;
            };
            await bootstrap.StartAsync();
            Assert.AreEqual(1, found);
        }
    }
}
