using PeerTalk.Multiplex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Protocols
{
    /// <summary>
    ///   Applies a stream multiplexer to a <see cref="PeerConnection"/>.
    /// </summary>
    public interface IMuxerProtocol : IPeerProtocol
    {
        public Task AttachMuxerAsync(PeerConnection connection, Stream stream, CancellationToken cancel = default);

    }
}
