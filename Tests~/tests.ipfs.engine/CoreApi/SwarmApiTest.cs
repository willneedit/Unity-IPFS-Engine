using NUnit.Framework;
using Newtonsoft.Json.Linq;
using PeerTalk.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class SwarmApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;
        readonly MultiAddress somewhere = "/ip4/127.0.0.1";

        [Test]
        public void Filter_Add_RemoveAsync()
		{
			Task.Run(Filter_Add_Remove).Wait();
		}

		public async Task Filter_Add_Remove()
        {
            var addr = await ipfs.Swarm.AddAddressFilterAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            var addrs = await ipfs.Swarm.ListAddressFiltersAsync();
            Assert.IsTrue(addrs.Any(a => a == somewhere));

            addr = await ipfs.Swarm.RemoveAddressFilterAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            addrs = await ipfs.Swarm.ListAddressFiltersAsync();
            Assert.IsFalse(addrs.Any(a => a == somewhere));
        }

        [Test]
        [Ignore("https://github.com/richardschneider/net-ipfs-engine/issues/74")]
        public void Connect_Disconnect_MarsAsync()
		{
			Task.Run(Connect_Disconnect_Mars).Wait();
		}

		public async Task Connect_Disconnect_Mars()
        {
            var mars = "/dns/mars.i.ipfs.io/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ";
            await ipfs.StartAsync();
            try
            {
                await ipfs.Swarm.ConnectAsync(mars);
                var peers = await ipfs.Swarm.PeersAsync();
                Assert.IsTrue(peers.Any(p => p.Id == "QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ"));
                await ipfs.Swarm.DisconnectAsync(mars);
            }
            finally
            {
                await ipfs.StopAsync();
            }
        }

        [Test]
        [Ignore("TODO: Move to interop tests")]
        public void JsIPFS_ConnectAsync()
		{
			Task.Run(JsIPFS_Connect).Wait();
		}

		public async Task JsIPFS_Connect()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var remoteId = "QmXFX2P5ammdmXQgfqGkfswtEVFsZUJ5KeHRXQYCTdiTAb";
            var remoteAddress = $"/ip4/127.0.0.1/tcp/4002/ipfs/{remoteId}";

            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
            await ipfs.Swarm.ConnectAsync(remoteAddress, cts.Token);
            Assert.AreEqual(1, (await ipfs.Swarm.PeersAsync()).Count());

            await ipfs.Swarm.DisconnectAsync(remoteAddress);
            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
        }

        [Test]
        [Ignore("TODO: Move to interop tests")]
        public void GoIPFS_ConnectAsync()
		{
			Task.Run(GoIPFS_Connect).Wait();
		}

		public async Task GoIPFS_Connect()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var remoteId = "QmdoxrwszT6b9srLXHYBPFVRXmZSFAosWLXoQS9TEEAaix";
            var remoteAddress = $"/ip4/127.0.0.1/tcp/4001/ipfs/{remoteId}";

            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
            await ipfs.Swarm.ConnectAsync(remoteAddress, cts.Token);
            Assert.AreEqual(1, (await ipfs.Swarm.PeersAsync()).Count());

            await ipfs.Swarm.DisconnectAsync(remoteAddress);
            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
        }

        [Test]
        [Ignore("TODO: Move to interop tests")]
        public void GoIPFS_Connect_v0_4_17Async()
		{
			Task.Run(GoIPFS_Connect_v0_4_17).Wait();
		}

		public async Task GoIPFS_Connect_v0_4_17()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var remoteId = "QmSoLer265NRgSp2LA3dPaeykiS1J6DifTC88f5uVQKNAd";
            var remoteAddress = $"/ip4/178.62.158.247/tcp/4001/ipfs/{remoteId}";

            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
            await ipfs.Swarm.ConnectAsync(remoteAddress, cts.Token);
            Assert.AreEqual(1, (await ipfs.Swarm.PeersAsync()).Count());

            await ipfs.Swarm.DisconnectAsync(remoteAddress);
            Assert.AreEqual(0, (await ipfs.Swarm.PeersAsync()).Count());
        }

        [Test]
        public void PrivateNetwork_WithOptionsKeyAsync()
		{
			Task.Run(PrivateNetwork_WithOptionsKey).Wait();
		}

		public async Task PrivateNetwork_WithOptionsKey()
        {
            using (var ipfs = CreateNode())
            {
                try
                {
                    ipfs.Options.Swarm.PrivateNetworkKey = new PreSharedKey().Generate();
                    var swarm = await ipfs.SwarmService;
                    Assert.IsNotNull(swarm.NetworkProtector);
                }
                finally
                {
                    if (Directory.Exists(ipfs.Options.Repository.Folder))
                    {
                        Directory.Delete(ipfs.Options.Repository.Folder, true);
                    }
                }
            }
        }

        [Test]
        public void PrivateNetwork_WithSwarmKeyFileAsync()
		{
			Task.Run(PrivateNetwork_WithSwarmKeyFile).Wait();
		}

		public async Task PrivateNetwork_WithSwarmKeyFile()
        {
            using (var ipfs = CreateNode())
            {
                try
                {
                    var key = new PreSharedKey().Generate();
                    var path = Path.Combine(ipfs.Options.Repository.ExistingFolder(), "swarm.key");
                    using (var x = File.CreateText(path))
                    {
                        key.Export(x);
                    }

                    var swarm = await ipfs.SwarmService;
                    Assert.IsNotNull(swarm.NetworkProtector);
                }
                finally
                {
                    if (Directory.Exists(ipfs.Options.Repository.Folder))
                    {
                        Directory.Delete(ipfs.Options.Repository.Folder, true);
                    }
                }
            }
        }

        static int nodeNumber = 0;
        IpfsEngine CreateNode()
        {
            const string passphrase = "this is not a secure pass phrase";
            var ipfs = new IpfsEngine(passphrase.ToCharArray());
            ipfs.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), $"swarm-{nodeNumber++}");
            ipfs.Options.KeyChain.DefaultKeySize = 512;
            ipfs.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/4007" })
            ).Wait();

            return ipfs;
        }

    }
}
