using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Protocols
{
    [TestFixture]
    public class MessageTest
    {
        [Test]
        public void EncodingAsync()
		{
			Task.Run(Encoding).Wait();
		}

		public async Task Encoding()
        {
            var ms = new MemoryStream();
            await Message.WriteAsync("a", ms);
            var buf = ms.ToArray();
            Assert.AreEqual(3, buf.Length);
            Assert.AreEqual(2, buf[0]);
            Assert.AreEqual((byte)'a', buf[1]);
            Assert.AreEqual((byte)'\n', buf[2]);
        }

        [Test]
		public void RoundTripAsync()
		{
			Task.Run(RoundTrip).Wait();
		}

		public async Task RoundTrip()
        {
            var msg = "/foobar/0.42.0";
            var ms = new MemoryStream();
            await Message.WriteAsync(msg, ms);
            ms.Position = 0;
            var result = await Message.ReadStringAsync(ms);
            Assert.AreEqual(msg, result);
        }
    }
}
