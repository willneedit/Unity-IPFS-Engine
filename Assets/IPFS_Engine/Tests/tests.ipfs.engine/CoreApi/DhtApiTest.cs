using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine
{
    [TestFixture]
    public class DhtApiTest
    {

        [Test]
        public void Local_InfoAsync()
		{
			Task.Run(Local_Info).Wait();
		}

		public async Task Local_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var locaId = (await ipfs.LocalPeer).Id;
            var peer = await ipfs.Dht.FindPeerAsync(locaId);

            Assert.AreEqual(peer.GetType(), typeof(Peer));
            Assert.AreEqual(locaId, peer.Id);
            Assert.IsNotNull(peer.Addresses);
            Assert.IsTrue(peer.IsValid());
        }

        [Test]
        // [Ignore("https://github.com/richardschneider/net-ipfs-engine/issues/74#issuecomment-500668261")]
        public void Mars_InfoAsync()
		{
			Task.Run(Mars_Info).Wait();
		}

		public async Task Mars_Info()
        {
            // var marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3"; // Outdated ID?
            var marsId = "QmSoLer265NRgSp2LA3dPaeykiS1J6DifTC88f5uVQKNAd";
            var ipfs = TestFixture.Ipfs;
            await ipfs.StartAsync();
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var mars = await ipfs.Dht.FindPeerAsync(marsId, cts.Token);
                Assert.AreEqual(marsId, mars.Id.ToString());
                Assert.IsNotNull(mars.Addresses);
                Assert.IsTrue(mars.IsValid());
            }
            finally
            {
                await ipfs.StopAsync();
            }
        }

        [Test]
        // [Ignore("https://github.com/richardschneider/net-ipfs-engine/issues/74 - Bootstrap servers don't relay folders")]
        public void FindProviderAsync()
		{
			Task.Run(FindProvider).Wait();
		}

		public async Task FindProvider()
        {
            void ProvidersFound(Peer peer)
            {
                /**/
            }

            var folder = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv";
            var ipfs = TestFixture.Ipfs;
            await ipfs.StartAsync();
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                var providers = await ipfs.Dht.FindProvidersAsync(folder, 1, ProvidersFound, cts.Token);
                Assert.AreEqual(1, providers.Count());
            }
            finally
            {
                await ipfs.StopAsync();
            }
        }
    }
}

