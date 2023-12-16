using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class NameApiTest
    {
#if true
        [Test]
        public void CrashesNeedFixing()
        {
            Assert.AreEqual(true, false);
        }
#else
        IpfsEngine ipfs = TestFixture.Ipfs;

        [Test]
        public void Resolve_DnsLinkAsync()
		{
			Task.Run(Resolve_DnsLink).Wait();
		}

		public async Task Resolve_DnsLink()
        {
            var iopath = await ipfs.Name.ResolveAsync("ipfs.io");
            Assert.IsNotNull(iopath);

            var path = await ipfs.Name.ResolveAsync("/ipns/ipfs.io");
            Assert.AreEqual(iopath, path);
        }

        [Test]
        public void Resolve_DnsLink_RecursiveAsync()
		{
			Task.Run(Resolve_DnsLink_Recursive).Wait();
		}

		public async Task Resolve_DnsLink_Recursive()
        {
            var path = await ipfs.Name.ResolveAsync("/ipns/ipfs.io/media", true);
            StringAssert.StartsWith(path, "/ipfs/");
            StringAssert.EndsWith(path, "/media");

            path = await ipfs.Name.ResolveAsync("ipfs.io/media", true);
            StringAssert.StartsWith(path, "/ipfs/");
            StringAssert.EndsWith(path, "/media");

            path = await ipfs.Name.ResolveAsync("/ipfs.io/media", true);
            StringAssert.StartsWith(path, "/ipfs/");
            StringAssert.EndsWith(path, "/media");
        }

        [Test]
        public void Resolve_NoDnsLink()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                var _ = ipfs.Dns.ResolveAsync("google.com").Result;
            });
        }
#endif
    }
}
