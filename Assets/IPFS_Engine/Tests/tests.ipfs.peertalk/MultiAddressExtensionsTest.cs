﻿using NUnit.Framework;
using Ipfs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PeerTalk
{
    [TestFixture]
    public class MultiAddressExtensionsTest
    {
        [Test]
        public void Cloning()
        {
            var a = new MultiAddress("/dns/libp2p.io/tcp/5001");
            var b = a.Clone();
            Assert.AreEqual(a, b);
            Assert.AreNotSame(a.Protocols, b.Protocols);
        }

        [Test]
        public void ResolvingAsync()
		{
			Task.Run(Resolving).Wait();
		}

		public async Task Resolving()
        {
            var local = new MultiAddress("/ip4/127.0.0.1/tcp/5001");
            var r0 = await local.ResolveAsync();
            Assert.AreEqual(1, r0.Count);
            Assert.AreEqual(local, r0[0]);
        }

        [Test]
        public void Resolving_DnsAsync()
		{
			Task.Run(Resolving_Dns).Wait();
		}

		public async Task Resolving_Dns()
        {
            var dns = await new MultiAddress("/dns/libp2p.io/tcp/5001").ResolveAsync();
            Assert.AreNotEqual(0, dns.Count);
            var dns4 = await new MultiAddress("/dns4/libp2p.io/tcp/5001").ResolveAsync();
            var dns6 = await new MultiAddress("/dns6/libp2p.io/tcp/5001").ResolveAsync();
            Assert.AreEqual(dns.Count, dns4.Count + dns6.Count);
        }

        [Test]
        public void Resolving_HTTPAsync()
		{
			Task.Run(Resolving_HTTP).Wait();
		}

		public async Task Resolving_HTTP()
        {
            var r = await new MultiAddress("/ip4/127.0.0.1/http").ResolveAsync();
            Assert.AreEqual("/ip4/127.0.0.1/http/tcp/80", r.First().ToString());

            r = await new MultiAddress("/ip4/127.0.0.1/http/tcp/8080").ResolveAsync();
            Assert.AreEqual("/ip4/127.0.0.1/http/tcp/8080", r.First().ToString());
        }

        [Test]
        public void Resolving_HTTPSAsync()
		{
			Task.Run(Resolving_HTTPS).Wait();
		}

		public async Task Resolving_HTTPS()
        {
            var r = await new MultiAddress("/ip4/127.0.0.1/https").ResolveAsync();
            Assert.AreEqual("/ip4/127.0.0.1/https/tcp/443", r.First().ToString());

            r = await new MultiAddress("/ip4/127.0.0.1/https/tcp/4433").ResolveAsync();
            Assert.AreEqual("/ip4/127.0.0.1/https/tcp/4433", r.First().ToString());
        }

        [Test]
        public void Resolving_Unknown()
        {
            ExceptionAssert.Throws<SocketException>(() =>
            {
                var _ = new MultiAddress("/dns/does.not.exist/tcp/5001")
                    .ResolveAsync()
                    .Result;
            });
        }

    }
}
