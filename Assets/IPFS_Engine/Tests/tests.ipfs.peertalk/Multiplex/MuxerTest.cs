using NUnit.Framework;
using Ipfs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Muxer
{
    [TestFixture]
    public class MplexMuxerTest
    {
        [Test]
        public void Defaults()
        {
            var muxer = new MplexMuxer();
            Assert.AreEqual(true, muxer.Initiator);
        }

        [Test]
        public void InitiatorReceiver()
        {
            var muxer = new MplexMuxer { Initiator = true };
            Assert.AreEqual(true, muxer.Initiator);
            Assert.AreEqual(0, muxer.NextStreamId & 1);

            muxer.Initiator = false;
            Assert.AreEqual(false, muxer.Initiator);
            Assert.AreEqual(1, muxer.NextStreamId & 1);
        }

        [Test]
        public void NewStream_SendAsync()
		{
			Task.Run(NewStream_Send).Wait();
		}

		public async Task NewStream_Send()
        {
            var channel = new MemoryStream();
            var muxer = new MplexMuxer { Channel = channel, Initiator = true };
            var nextId = muxer.NextStreamId;
            var stream = await muxer.CreateStreamAsync("foo");

            // Correct stream id is assigned.
            // Assert.AreEqual(nextId, stream.Id);
            Assert.AreEqual(nextId + 2, muxer.NextStreamId);
            // Assert.AreEqual("foo", stream.Name);

            // Substreams are managed.
            Assert.AreEqual(1, muxer.Substreams.Count);
            // Assert.AreSame(stream, muxer.Substreams[stream.Id]);

            // NewStream message is sent.
            channel.Position = 0;
            Assert.AreEqual(8000, channel.ReadVarint32());
            Assert.AreEqual(3, channel.ReadVarint32());
            var name = new byte[3];
            channel.Read(name, 0, 3);
            Assert.AreEqual("foo", Encoding.UTF8.GetString(name));
            Assert.AreEqual(channel.Length, channel.Position);
        }

        [Test]
        public void NewStream_ReceiveAsync()
		{
			Task.Run(NewStream_Receive).Wait();
		}

		public async Task NewStream_Receive()
        {
            var channel = new MemoryStream();
            var muxer1 = new MplexMuxer { Channel = channel, Initiator = true };
            var foo = await muxer1.CreateStreamAsync("foo");
            var bar = await muxer1.CreateStreamAsync("bar");

            channel.Position = 0;
            var muxer2 = new MplexMuxer { Channel = channel };
            int n = 0;
            muxer2.SubstreamCreated += (s, e) => ++n;
            await muxer2.ProcessRequestsAsync();
            Assert.AreEqual(2, n);
        }

        [Test]
        public void NewStream_AlreadyAssignedAsync()
		{
			Task.Run(NewStream_AlreadyAssigned).Wait();
		}

		public async Task NewStream_AlreadyAssigned()
        {
            var channel = new MemoryStream();
            var muxer1 = new MplexMuxer { Channel = channel, Initiator = true };
            var foo = await muxer1.CreateStreamAsync("foo");
            var muxer2 = new MplexMuxer { Channel = channel, Initiator = true };
            var bar = await muxer2.CreateStreamAsync("bar");

            channel.Position = 0;
            var muxer3 = new MplexMuxer { Channel = channel };
            await muxer3.ProcessRequestsAsync(new CancellationTokenSource(500).Token);
       
            // The channel is closed because of 2 new streams with same id.
            Assert.IsFalse(channel.CanRead);
            Assert.IsFalse(channel.CanWrite);
        }

        [Test]
        public void NewStream_EventAsync()
		{
			Task.Run(NewStream_Event).Wait();
		}

		public async Task NewStream_Event()
        {
            var channel = new MemoryStream();
            var muxer1 = new MplexMuxer { Channel = channel, Initiator = true };
            var foo = await muxer1.CreateStreamAsync("foo");
            var bar = await muxer1.CreateStreamAsync("bar");

            channel.Position = 0;
            var muxer2 = new MplexMuxer { Channel = channel };
            int createCount = 0;
            muxer2.SubstreamCreated += (s, e) =>
            {
                ++createCount;
            };
            await muxer2.ProcessRequestsAsync();
            Assert.AreEqual(2, createCount);
        }

        [Test]
        public void CloseStream_EventAsync()
		{
			Task.Run(CloseStream_Event).Wait();
		}

		public async Task CloseStream_Event()
        {
            var channel = new MemoryStream();
            var muxer1 = new MplexMuxer { Channel = channel, Initiator = true };
            using (var foo = await muxer1.CreateStreamAsync("foo"))
            using (var bar = await muxer1.CreateStreamAsync("bar"))
            {
                // open and close a stream.
            }

            channel.Position = 0;
            var muxer2 = new MplexMuxer { Channel = channel };
            int closeCount = 0;
            muxer2.SubstreamClosed += (s, e) =>
            {
                ++closeCount;
            };
            await muxer2.ProcessRequestsAsync();
            Assert.AreEqual(2, closeCount);
        }

        [Test]
        public void AcquireWriteAsync()
		{
			Task.Run(AcquireWrite).Wait();
		}

		public async Task AcquireWrite()
        {
            var muxer = new MplexMuxer();
            var tasks = new List<Task<string>>
            {
                Task<string>.Run(async () =>
                {
                    using (await muxer.AcquireWriteAccessAsync())
                    {
                        await Task.Delay(100);
                    }
                    return "step 1";
                }),
                Task<string>.Run(async () =>
                {
                    using (await muxer.AcquireWriteAccessAsync())
                    {
                        await Task.Delay(50);
                    }
                    return "step 2";
                }),
            };

            var done = await Task.WhenAll(tasks);
            Assert.AreEqual("step 1", done[0]);
            Assert.AreEqual("step 2", done[1]);
        }
    }
}
