using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk
{
    [TestFixture]
    public class BlackListTest
    {
        [Test]
        public void Allowed()
        {
            var policy = new BlackList<string>();
            policy.Add("c");
            policy.Add("d");
            Assert.IsTrue(policy.IsAllowed("a"));
            Assert.IsTrue(policy.IsAllowed("b"));
            Assert.IsFalse(policy.IsAllowed("c"));
            Assert.IsFalse(policy.IsAllowed("d"));
        }

        [Test]
        public void Empty()
        {
            var policy = new BlackList<string>();
            Assert.IsTrue(policy.IsAllowed("a"));
        }
    }
}
