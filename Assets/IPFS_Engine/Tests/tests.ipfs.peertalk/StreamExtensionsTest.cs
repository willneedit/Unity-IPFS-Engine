﻿using NUnit.Framework;
using Ipfs;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PeerTalk
{
    [TestFixture]
    public class StreamExtensionsTest
    {
        [Test]
        public void ReadAsyncAsync()
		{
			Task.Run(ReadAsync).Wait();
		}

		public async Task ReadAsync()
        {
            var expected = new byte[] { 1, 2, 3, 4 };
            using (var ms = new MemoryStream(expected))
            {
                var actual = new byte[expected.Length];
                await ms.ReadExactAsync(actual, 0, actual.Length);
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void ReadAsync_EOS()
        {
            var expected = new byte[] { 1, 2, 3, 4 };
            var actual = new byte[expected.Length + 1];

            using (var ms = new MemoryStream(expected))
            {
                ExceptionAssert.Throws<EndOfStreamException>(() =>
                {
                    ms.ReadExactAsync(actual, 0, actual.Length).Wait();
                });
            }

            var cancel = new CancellationTokenSource();
            using (var ms = new MemoryStream(expected))
            {
                ExceptionAssert.Throws<EndOfStreamException>(() =>
                {
                    ms.ReadExactAsync(actual, 0, actual.Length, cancel.Token).Wait();
                });
            }
        }

        [Test]
        public void ReadAsync_CancelAsync()
		{
			Task.Run(ReadAsync_Cancel).Wait();
		}

		public async Task ReadAsync_Cancel()
        {
            var expected = new byte[] { 1, 2, 3, 4 };
            var actual = new byte[expected.Length];
            var cancel = new CancellationTokenSource();
            using (var ms = new MemoryStream(expected))
            {
                await ms.ReadExactAsync(actual, 0, actual.Length, cancel.Token);
                CollectionAssert.AreEqual(expected, actual);
            }

            cancel.Cancel();
            using (var ms = new MemoryStream(expected))
            {
                ExceptionAssert.Throws<TaskCanceledException>(() =>
                {
                    ms.ReadExactAsync(actual, 0, actual.Length, cancel.Token).Wait();
                });
            }
        }
    }
}
