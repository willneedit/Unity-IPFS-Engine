using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk
{
    [TestFixture]
    public class PolicyTest
    {
        [Test]
        public void Always()
        {
            var policy = new PolicyAlways<string>();
            Assert.IsTrue(policy.IsAllowed("foo"));
        }

        [Test]
        public void Never()
        {
            var policy = new PolicyNever<string>();
            Assert.IsFalse(policy.IsAllowed("foo"));
        }
    }
}
