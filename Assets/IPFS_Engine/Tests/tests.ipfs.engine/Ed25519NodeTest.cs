using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Ipfs.Engine
{
    [TestFixture]
    public class Ed25519NodeTest
    {
        [Test]
        public async void Can_Create()
        {
            var ed = await CreateNode();
            try
            {
                Assert.IsNotNull(ed);
                var node = await ed.LocalPeer;
                Assert.IsNotNull(node);
            }
            finally
            {
                DeleteNode(ed);
            }
        }

        [Test]
        public async void CanConnect()
        {
            var ed = await CreateNode();
            try
            {
                await ed.StartAsync();
                var node = await ed.LocalPeer;
                Assert.AreNotEqual(0, node.Addresses.Count());
                var addr = node.Addresses.First();

                var ipfs = TestFixture.Ipfs;
                await ipfs.StartAsync();
                try
                {
                    await ipfs.Swarm.ConnectAsync(addr);
                    var peers = await ipfs.Swarm.PeersAsync();
                    Assert.IsTrue(peers.Any(p => p.Id == addr.PeerId));
                    await ipfs.Swarm.DisconnectAsync(addr);
                }
                finally
                {
                    await ipfs.StopAsync();
                }
            }
            finally
            {
                await ed.StopAsync();
                DeleteNode(ed);
            }
        }

        async Task<IpfsEngine> CreateNode()
        {
            const string passphrase = "this is not a secure pass phrase";
            var ipfs = new IpfsEngine(passphrase.ToCharArray());
            ipfs.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), "ipfs-ed255129-test");
            ipfs.Options.KeyChain.DefaultKeyType = "ed25519";
            await ipfs.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/0" })
            );
            return ipfs;
        }

        void DeleteNode(IpfsEngine ipfs)
        {
            if (Directory.Exists(ipfs.Options.Repository.Folder))
            {
                Directory.Delete(ipfs.Options.Repository.Folder, true);
            }
        }

    }
}
