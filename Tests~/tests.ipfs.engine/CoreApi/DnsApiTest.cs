using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class DnsApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;

        [Test]
        public void ResolveAsync()
		{
			Task.Run(Resolve).Wait();
		}

		public async Task Resolve()
        {
            var path = await ipfs.Dns.ResolveAsync("ipfs.io");
            Assert.IsNotNull(path);
        }

        [Test]
        public void Resolve_NoLinkAsync()
        {
            Task.Run(Resolve_NoLink).Wait();
        }

        public void Resolve_NoLink()
        {
            ExceptionAssert.Throws<Exception>(() => ipfs.Dns.ResolveAsync("google.com").Wait());
        }

        [Test]
        public void Resolve_RecursiveAsync()
		{
			Task.Run(Resolve_Recursive).Wait();
		}

		public async Task Resolve_Recursive()
        {
            var path = await ipfs.Dns.ResolveAsync("ipfs.io", true);
            StringAssert.StartsWith("/ipfs/", path);
        }
    }
}
