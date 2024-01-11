using Ipfs;
using PeerTalk.Muxer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PeerTalk.Muxer
{
    public class Yamux1Substream : Stream
    {
        private enum StreamState
        {
            Init,
            SYNSent,
            SYNReceived,
            Established,
            Finished,
        }

        public string Name { get; set; }

        private StreamState state;
        private int _id;

        /** The number of available bytes to send */
        private int sendWindowCapacity;
        /** Callback to notify that the sendWindowCapacity has been updated */
        private Action sendWindowCapacityUpdate = null;

        /** The number of bytes available to receive in a full window */
        private int recvWindow;
        /** The number of available bytes to receive */
        private int recvWindowCapacity;

        /**
         * An 'epoch' is the time it takes to process and read data
         *
         * Used in conjunction with RTT to determine whether to increase the recvWindow
         */
        private DateTime epochStart;


        BufferBlock<byte[]> inBlocks = new BufferBlock<byte[]>();
        byte[] inBlock;
        int inBlockOffset;
        bool eos;

        Stream outStream = new MemoryStream();

        public IMuxerControl Muxer { get; set; } = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Muxer.RemoveStreamAsync(this);

                eos = true;
                if (outStream != null)
                {
                    outStream.Dispose();
                    outStream = null;
                }
            }
            base.Dispose(disposing);
        }

        public override bool CanRead => !eos;

        public override bool CanSeek => false;

        public override bool CanWrite => outStream != null;

        public override bool CanTimeout => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException(); 
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            outStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return outStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Flush()
        {
            FlushAsync().GetAwaiter().GetResult();
        }

        public override Task FlushAsync(CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            outStream.WriteByte(value);
        }
    }
}
