using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Engine
{
    [TestFixture]
    public class GenericApiTest
    {

        [Test]
        public void Local_InfoAsync()
		{
			Task.Run(Local_Info).Wait();
		}

		public async Task Local_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var peer = await ipfs.Generic.IdAsync();
            Assert.AreEqual(peer.GetType(), typeof(Peer));
            Assert.IsNotNull(peer.Addresses);
            StringAssert.StartsWith(peer.AgentVersion, "net-ipfs/");
            Assert.IsNotNull(peer.Id);
            StringAssert.StartsWith(peer.ProtocolVersion, "ipfs/");
            Assert.IsNotNull(peer.PublicKey);

            Assert.IsTrue(peer.IsValid());
        }

        [Test]
		public void Mars_InfoAsync()
		{
			Task.Run(Mars_Info).Wait();
		}

		public async Task Mars_Info()
        {
            var marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
            var marsAddr = $"/ip6/::1/p2p/{marsId}";
            var ipfs = TestFixture.Ipfs;
            var swarm = await ipfs.SwarmService;
            var mars = swarm.RegisterPeerAddress(marsAddr);

            var peer = await ipfs.Generic.IdAsync(marsId);
            Assert.AreEqual(mars.Id, peer.Id);
            Assert.AreEqual(mars.Addresses.First(), peer.Addresses.First());
        }

        [Test]
        public void Version_InfoAsync()
		{
			Task.Run(Version_Info).Wait();
		}

		public async Task Version_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var versions = await ipfs.Generic.VersionAsync();
            Assert.IsNotNull(versions);
            Assert.IsTrue(versions.ContainsKey("Version"));
            Assert.IsTrue(versions.ContainsKey("Repo"));
        }

        [Test]
        public void ShutdownAsync()
		{
			Task.Run(Shutdown).Wait();
		}

		public async Task Shutdown()
        {
            var ipfs = TestFixture.Ipfs;
            await ipfs.StartAsync();
            await ipfs.Generic.ShutdownAsync();
        }

        [Test]
        public void Resolve_CidAsync()
		{
			Task.Run(Resolve_Cid).Wait();
		}

		public async Task Resolve_Cid()
        {
            var ipfs = TestFixture.Ipfs;
            var actual = await ipfs.Generic.ResolveAsync("QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao");
            Assert.AreEqual("/ipfs/QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao", actual);

            actual = await ipfs.Generic.ResolveAsync("/ipfs/QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao");
            Assert.AreEqual("/ipfs/QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao", actual);
        }

        [Test]
        public void Resolve_Cid_PathAsync()
		{
			Task.Run(Resolve_Cid_Path).Wait();
		}

		public async Task Resolve_Cid_Path()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = FileSystemApiTest.MakeTemp();
            try
            {
                var dir = await ipfs.FileSystem.AddDirectoryAsync(temp, true);
                var name = "/ipfs/" + dir.Id.Encode() + "/x/y/y.txt";
                Assert.AreEqual("/ipfs/QmTwEE2eSyzcvUctxP2negypGDtj7DQDKVy8s3Rvp6y6Pc", await ipfs.Generic.ResolveAsync(name));
            }
            finally
            {
                Directory.Delete(temp, true);
            }
        }

        [Test]
        public void Resolve_Cid_Invalid()
        {
            var ipfs = TestFixture.Ipfs;
            ExceptionAssert.Throws<FormatException>(() =>
            {
                var _= ipfs.Generic.ResolveAsync("QmHash").Result;
            });
        }

        [Test]
        public void Resolve_DnsLinkAsync()
		{
			Task.Run(Resolve_DnsLink).Wait();
		}

		public async Task Resolve_DnsLink()
        {
            var ipfs = TestFixture.Ipfs;
            var path = await ipfs.Generic.ResolveAsync("/ipns/ipfs.io");
            Assert.IsNotNull(path);
        }

        [Test]
        [Ignore("Need a working IPNS")]
        public void Resolve_DnsLink_RecursiveAsync()
		{
			Task.Run(Resolve_DnsLink_Recursive).Wait();
		}

		public async Task Resolve_DnsLink_Recursive()
        {
            var ipfs = TestFixture.Ipfs;

            var media = await ipfs.Generic.ResolveAsync("/ipns/ipfs.io/media");
            var actual = await ipfs.Generic.ResolveAsync("/ipns/ipfs.io/media", recursive: true);
            Assert.AreNotEqual(media, actual);
        }
    }
}

