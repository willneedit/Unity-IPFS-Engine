using Common.Logging;
using Semver;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PeerTalk.Protocols;
using System;

namespace PeerTalk.Muxer
{
    /// <summary>
    ///    A Stream Multiplexer protocol.
    /// </summary>
    /// <seealso href="https://github.com/ChainSafe/js-libp2p-yamux"/>
    public class Yamux1 : IMuxerProtocol
    {
        static ILog log = LogManager.GetLogger(typeof(Yamux1));

        public string Name { get; } = "yamux";

        public SemVersion Version { get; } = new SemVersion(1, 0);

        public override string ToString()
        {
            return $"/{Name}/{Version}";
        }

        public async Task ProcessMessageAsync(PeerConnection connection, Stream stream, CancellationToken cancel = default)
        {
            throw new NotImplementedException();
            log.Debug("start processing requests from " + connection.RemoteAddress);
            MplexMuxer muxer = new MplexMuxer
            {
                Channel = stream,
                Connection = connection,
                Initiator = !connection.IsIncoming
            };
            muxer.SubstreamCreated += (s, e) => _ = connection.ReadMessagesAsync(e, CancellationToken.None);

            // Attach muxer to the connection.  It now becomes the message reader.
            connection.MuxerEstablished.SetResult(muxer);
            await muxer.ProcessRequestsAsync().ConfigureAwait(false);

            log.Debug("stop processing from " + connection.RemoteAddress);
        }
    }
}
