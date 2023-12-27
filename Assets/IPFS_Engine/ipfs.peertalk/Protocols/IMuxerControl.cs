using PeerTalk.Multiplex;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Muxer
{
    public interface IMuxerControl
    {
        Task<Stream> CreateStreamAsync(string name = "", CancellationToken cancel = default);
    }
}