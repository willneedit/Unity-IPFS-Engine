using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Ipfs.CoreApi
{
    [TestFixture]
    public class PingResultTest
    {
        [Test]
        public void Properties()
        {
            var time = TimeSpan.FromSeconds(3);
            var r = new PingResult
            {
                Success = true,
                Text = "ping",
                Time = time
            };
            Assert.AreEqual(true, r.Success);
            Assert.AreEqual("ping", r.Text);
            Assert.AreEqual(time, r.Time);
        }

    }
}
