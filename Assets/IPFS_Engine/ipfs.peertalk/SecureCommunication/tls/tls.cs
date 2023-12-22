using System.Collections;
using Common.Logging;
using Ipfs;
using Noise;
using PeerTalk.Cryptography;
using PeerTalk.Protocols;
using Semver;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk.SecureCommunication
{
    /// <summary>
    ///   Creates a secure connection with a peer.
    /// </summary>
    public class TLS : IEncryptionProtocol
    {
        static ILog log = LogManager.GetLogger(typeof(TLS));

        /// <inheritdoc />
        public string Name { get; } = "tls";

        /// <inheritdoc />
        public SemVersion Version { get; } = new SemVersion(1, 0);


        /// <inheritdoc />
        public override string ToString()
        {
            return $"/{Name}/{Version}";
        }


        /// <inheritdoc />
        public async Task ProcessMessageAsync(PeerConnection connection, Stream stream, CancellationToken cancel = default(CancellationToken))
        {
            await EncryptAsync(connection, cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<Stream> EncryptAsync(PeerConnection connection, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}