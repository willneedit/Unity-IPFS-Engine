using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Routing
{
    
    [TestFixture]
    public class DistributedQueryTest
    {
        [Test]
        public async void Cancelling()
        {
            var dquery = new DistributedQuery<Peer>
            {
                Dht = new Dht1()
            };
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await dquery.RunAsync(cts.Token);
            Assert.AreEqual(0, dquery.Answers.Count());
        }

        [Test]
        public void UniqueId()
        {
            var q1 = new DistributedQuery<Peer>();
            var q2 = new DistributedQuery<Peer>();
            Assert.AreNotEqual(q1.Id, q2.Id);
        }

    }
}
