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
#if true
        [Test]
        public void CrashesNeedFixing() 
        {
            Assert.AreEqual(true, false);
        }
#else
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
        public void Resolve_NoLink()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                var _ = ipfs.Dns.ResolveAsync("google.com").Result;
            });
        }

        [Test]
        public void Resolve_RecursiveAsync()
		{
			Task.Run(Resolve_Recursive).Wait();
		}

		public async Task Resolve_Recursive()
        {
            var path = await ipfs.Dns.ResolveAsync("ipfs.io", true);
            StringAssert.StartsWith(path, "/ipfs/");
        }
#endif
    }
}
