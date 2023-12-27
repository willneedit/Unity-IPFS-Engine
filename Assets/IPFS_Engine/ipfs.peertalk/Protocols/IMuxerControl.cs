using PeerTalk.Multiplex;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.Muxer
{
    public interface IMuxerControl
    {
        Stream Channel { get; set; }

        event EventHandler<Substream> SubstreamClosed;
        event EventHandler<Substream> SubstreamCreated;

        Task<IDisposable> AcquireWriteAccessAsync();
        Task<Substream> CreateStreamAsync(string name = "", CancellationToken cancel = default);
        Task ProcessRequestsAsync(CancellationToken cancel = default);
        Task<Substream> RemoveStreamAsync(Substream stream, CancellationToken cancel = default);
    }
}