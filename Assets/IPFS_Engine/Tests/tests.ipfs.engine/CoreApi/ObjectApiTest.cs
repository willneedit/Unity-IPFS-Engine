using Ipfs.CoreApi;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine.CoreApi
{

    [TestFixture]
    public class ObjectApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;

        [Test]
		public void New_Template_NullAsync()
		{
			Task.Run(New_Template_Null).Wait();
		}

		public async Task New_Template_Null()
        {
            var node = await ipfs.Object.NewAsync();
            Assert.AreEqual("QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n", (string)node.Id);
        }

        [Test]
		public void New_Template_UnixfsDirAsync()
		{
			Task.Run(New_Template_UnixfsDir).Wait();
		}

		public async Task New_Template_UnixfsDir()
        {
            var node = await ipfs.Object.NewAsync("unixfs-dir");
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);

            node = await ipfs.Object.NewDirectoryAsync();
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);
        }

        [Test]
        public void New_Template_Unknown()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                var node = ipfs.Object.NewAsync("unknown-template").Result;
            });
        }

        [Test]
        public void Put_Get_DagAsync()
		{
			Task.Run(Put_Get_Dag).Wait();
		}

		public async Task Put_Get_Dag()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = new DagNode(bdata, new[] { alpha.ToLink() });
            var x = await ipfs.Object.PutAsync(beta);
            var node = await ipfs.Object.GetAsync(x.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [Test]
        public void Put_Get_DataAsync()
		{
			Task.Run(Put_Get_Data).Wait();
		}

		public async Task Put_Get_Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var node = await ipfs.Object.GetAsync(beta.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [Test]
        public void DataAsync()
		{
			Task.Run(Data).Wait();
		}

		public async Task Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var node = await ipfs.Object.PutAsync(adata);
            using (var stream = await ipfs.Object.DataAsync(node.Id))
            {
                var bdata = new byte[adata.Length];
                stream.Read(bdata, 0, bdata.Length);
                CollectionAssert.AreEqual(adata, bdata);
            }
        }

        [Test]
        public void LinksAsync()
		{
			Task.Run(Links).Wait();
		}

		public async Task Links()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var links = await ipfs.Object.LinksAsync(beta.Id);
            Assert.AreEqual(beta.Links.Count(), links.Count());
            Assert.AreEqual(beta.Links.First().Id, links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, links.First().Size);
        }

        [Test]
        public void StatAsync()
		{
			Task.Run(Stat).Wait();
		}

		public async Task Stat()
        {
            var data1 = Encoding.UTF8.GetBytes("Some data 1");
            var data2 = Encoding.UTF8.GetBytes("Some data 2");
            var node2 = new DagNode(data2);
            var node1 = await ipfs.Object.PutAsync(data1,
                new[] { node2.ToLink("some-link") });
            var info = await ipfs.Object.StatAsync(node1.Id);
            Assert.AreEqual(1, info.LinkCount);
            Assert.AreEqual(11, info.DataSize);
            Assert.AreEqual(64, info.BlockSize);
            Assert.AreEqual(53, info.LinkSize);
            Assert.AreEqual(77, info.CumulativeSize);
        }

        [Test]
        public void Get_NonexistentAsync()
		{
			Task.Run(Get_Nonexistent).Wait();
		}

		public async Task Get_Nonexistent()
        {
            var data = Encoding.UTF8.GetBytes("Some data for net-ipfs-engine-test that cannot be found");
            var node = new DagNode(data);
            var id = node.Id;
            var cs = new CancellationTokenSource(500);
            try
            {
                var _ = await ipfs.Object.GetAsync(id, cs.Token);
                Assert.Fail("Did not throw TaskCanceledException");
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        [Test]
        /// <seealso href="https://github.com/ipfs/js-ipfs/issues/2084"/>
        public void Get_InlinefileAsync()
		{
			Task.Run(Get_Inlinefile).Wait();
		}

		public async Task Get_Inlinefile()
        {
            var original = ipfs.Options.Block.AllowInlineCid;
            try
            {
                ipfs.Options.Block.AllowInlineCid = true;

                var node = await ipfs.FileSystem.AddTextAsync("hiya");
                Assert.AreEqual(1, node.Id.Version);
                Assert.IsTrue(node.Id.Hash.IsIdentityHash);

                var dag = await ipfs.Object.GetAsync(node.Id);
                Assert.AreEqual(12, dag.Size);
            }
            finally
            {
                ipfs.Options.Block.AllowInlineCid = original;
            }
        }

        [Test]
        public void Links_InlineCidAsync()
		{
			Task.Run(Links_InlineCid).Wait();
		}

		public async Task Links_InlineCid()
        {
            var original = ipfs.Options.Block.AllowInlineCid;
            try
            {
                ipfs.Options.Block.AllowInlineCid = true;

                var node = await ipfs.FileSystem.AddTextAsync("hiya");
                Assert.AreEqual(1, node.Id.Version);
                Assert.IsTrue(node.Id.Hash.IsIdentityHash);

                var links = await ipfs.Object.LinksAsync(node.Id);
                Assert.AreEqual(0, links.Count());
            }
            finally
            {
                ipfs.Options.Block.AllowInlineCid = original;
            }
        }

        [Test]
        public void Links_RawCidAsync()
		{
			Task.Run(Links_RawCid).Wait();
		}

		public async Task Links_RawCid()
        {
            var blob = new byte[2048];
            var cid = await ipfs.Block.PutAsync(blob, contentType: "raw");

            var links = await ipfs.Object.LinksAsync(cid);
            Assert.AreEqual(0, links.Count());
        }
    }
}