using Common.Logging;
using Ipfs;
using Ipfs.Core.Cryptography;
using PeerTalk.Cryptography;
using PeerTalk.Muxer;
using PeerTalk.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk
{
    /// <summary>
    ///   A connection between two peers.
    /// </summary>
    /// <remarks>
    ///   A connection is used to exchange messages between peers.
    /// </remarks>
    public class PeerConnection : IDisposable
    {
        static ILog log = LogManager.GetLogger(typeof(PeerConnection));

        Stream stream;
        StatsStream statsStream;

        /// <summary>
        ///   The local peer.
        /// </summary>
        public Peer LocalPeer { get; set; }

        /// <summary>
        ///   The remote peer.
        /// </summary>
        public Peer RemotePeer { get; set; }

        /// <summary>
        ///   The local peer's end point.
        /// </summary>
        public MultiAddress LocalAddress { get; set; }

        /// <summary>
        ///   The remote peer's end point.
        /// </summary>
        public MultiAddress RemoteAddress { get; set; }

        /// <summary>
        ///   The private key of the local peer.
        /// </summary>
        /// <value>
        ///   Used to prove the identity of the <see cref="LocalPeer"/>.
        /// </value>
        public KeyPair LocalPeerKey { get; set; }

        /// <summary>
        ///   Determine which peer (local or remote) initiated the connection.
        /// </summary>
        /// <value>
        ///   <b>true</b> if the <see cref="RemotePeer"/> initiated the connection;
        ///   otherwise, <b>false</b>.
        /// </value>
        public bool IsIncoming { get; set; }

        /// <summary>
        ///   Determines if the connection to the remote can be used.
        /// </summary>
        /// <value>
        ///   <b>true</b> if the connection is active.
        /// </value>
        public bool IsActive
        {
            get { return Stream != null && Stream.CanRead && Stream.CanWrite;  }
        }

        /// <summary>
        ///   The duplex stream between the two peers.
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
            set
            {
                if (value != null && statsStream == null)
                {
                    statsStream = new StatsStream(value);
                    value = statsStream;
                }
                stream = value;
            }
        }

        /// <summary>
        ///   The protocols that the connection will handle.
        /// </summary>
        /// <value>
        ///   The key is a protocol name, such as "/mplex/6.7.0".  The value
        ///   is a function that will process the protocol message.
        /// </value>
        /// <seealso cref="AddProtocol"/>
        /// <seealso cref="AddProtocols"/>
        public Dictionary<string, Func<PeerConnection, Stream, CancellationToken, Task>> Protocols { get; }
            = new Dictionary<string, Func<PeerConnection, Stream, CancellationToken, Task>>();

        /// <summary>
        ///   Add a protocol that the connection will handle.
        /// </summary>
        /// <param name="protocol">
        ///   A peer protocol to add.
        /// </param>
        public void AddProtocol(IPeerProtocol protocol)
        {
            Protocols.Add(protocol.ToString(), protocol.ProcessMessageAsync);
        }

        /// <summary>
        ///   Add a seequence of protocols that the connection will handle.
        /// </summary>
        /// <param name="protocols">
        ///   The peer protocols to add.
        /// </param>
        public void AddProtocols(IEnumerable<IPeerProtocol> protocols)
        {
            foreach (var protocol in protocols)
            {
                if (protocol != null)
                {
                    Protocols.Add(protocol.ToString(), protocol.ProcessMessageAsync);
                }
            }
        }

        /// <summary>
        ///   Signals that the security for the connection is established.
        /// </summary>
        /// <remarks>
        ///   This can be awaited.
        /// </remarks>
        public TaskCompletionSource<bool> SecurityEstablished { get; } = new();

        /// <summary>
        ///   Signals that the muxer for the connection is established.
        /// </summary>
        /// <remarks>
        ///   This can be awaited.
        /// </remarks>
        public TaskCompletionSource<IMuxerControl> MuxerEstablished { get; } = new();

        /// <summary>
        ///   Signals that the identity of the remote endpoint is established.
        /// </summary>
        /// <remarks>
        ///   This can be awaited.
        /// </remarks>
        /// <remarks>
        ///   The data in <see cref="RemotePeer"/> is not complete until
        ///   the identity is establish.
        /// </remarks>
        public TaskCompletionSource<Peer> IdentityEstablished { get; } = new();

        /// <summary>
        ///   When the connection was last used.
        /// </summary>
        public DateTime LastUsed => statsStream.LastUsed;

        /// <summary>
        ///   Number of bytes read over the connection.
        /// </summary>
        public long BytesRead => statsStream.BytesRead;

        /// <summary>
        ///   Number of bytes written over the connection.
        /// </summary>
        public long BytesWritten => statsStream.BytesWritten;

        /// <summary>
        ///  Establish the connection with the remote node.
        /// </summary>
        /// <param name="securityProtocols"></param>
        /// <param name="cancel"></param>
        /// <remarks>
        ///   This should be called when the local peer wants a connection with
        ///   the remote peer.
        /// </remarks>
        public async Task InitiateAsync(
            IEnumerable<IPeerProtocol> protocols,
            CancellationToken cancel = default(CancellationToken))
        {
            IEncryptionProtocol[] securityProtocols = null;
            IMuxerProtocol[] muxerProtocols = null;

            lock (protocols)
            {
                securityProtocols = protocols.OfType<IEncryptionProtocol>().ToArray();
                muxerProtocols = protocols.OfType<IMuxerProtocol>().ToArray();
            }

            await EstablishProtocolAsync("/multistream/", cancel).ConfigureAwait(false);

            // Find an excryption protocol and start encrypting
            IEncryptionProtocol match = await MatchProtocolAsync(securityProtocols, Stream, cancel).ConfigureAwait(false);
            await match.EncryptAsync(this, cancel).ConfigureAwait(false);

            await EstablishProtocolAsync("/multistream/", cancel).ConfigureAwait(false);

            // TODO IMuxerProtocol, merge Muxer <--> Mplex67, implement Yamux

            IMuxerProtocol muxerMatch = await MatchProtocolAsync(muxerProtocols, Stream, cancel).ConfigureAwait(false);
            _ = muxerMatch.ProcessMessageAsync(this, Stream, cancel).ConfigureAwait(false);
        }

        /// <summary>
        ///   TODO:
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task EstablishProtocolAsync(string name, CancellationToken cancel)
        {
            return EstablishProtocolAsync(name, Stream, cancel);
        }

        /// <summary>
        ///  Offers the remote a suitable list of protocols to pick one.
        /// </summary>
        /// <param name="protocols">Protocols to be offered</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns>The protocol the server and client agreed on</returns>
        /// <exception cref="AggregateException">Error, or no protocol matched</exception>
        public async Task<T> MatchProtocolAsync<T>(IEnumerable<T> protocols, Stream stream, CancellationToken cancel) where T : IPeerProtocol
        {
            var exceptions = new List<Exception>();
            foreach (var protocol in protocols)
            {
                try
                {
                    log.Debug($"Offering protocol: {protocol}");
                    await EstablishProtocolAsync(protocol.ToString(), stream, cancel).ConfigureAwait(false);
                    log.Debug($"Agreed on {protocol}");
                    return protocol;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    continue;
                }
            }

            // Ran out of options.
            throw new AggregateException("Could not agree on the offered protocols", exceptions);
        }

        /// <summary>
        ///   TODO:
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public async Task EstablishProtocolAsync(string name, Stream stream, CancellationToken cancel = default(CancellationToken))
        {
            var protocols = ProtocolRegistry.Protocols.Keys
                .Where(k => k == name || k.StartsWith(name));
            foreach (var protocol in protocols)
            {
                await Message.WriteAsync(protocol, stream, cancel).ConfigureAwait(false);
                var result = await Message.ReadStringAsync(stream, cancel).ConfigureAwait(false);
                if (result == protocol)
                {
                    return;
                }
            }
            if (protocols.Count() == 0)
            {
                throw new Exception($"Protocol '{name}' is not registered.");
            }
            throw new Exception($"{RemotePeer.Id} does not support protocol '{name}'.");
        }

        /// <summary>
        ///   Starts reading messages from the remote peer.
        /// </summary>
        public async Task ReadMessagesAsync(CancellationToken cancel)
        {
            log.Debug($"start reading messsages from {RemoteAddress}");

            // TODO: Only a subset of protocols are allowed until
            // the remote is authenticated.
            IPeerProtocol protocol = new Multistream1();
            try
            {
                while (!cancel.IsCancellationRequested && Stream != null)
                {
                    await protocol.ProcessMessageAsync(this, Stream, cancel).ConfigureAwait(false);
                }
            }
            catch (IOException e)
            {
                log.Error("reading message failed " + e.Message);
                // eat it.
            }
            catch (Exception e)
            {
                if (!cancel.IsCancellationRequested && Stream != null)
                {
                    log.Error("reading message failed", e);
                }
            }

            // Ignore any disposal exceptions.
            try
            {
                Stream?.Dispose();
            }
            catch (Exception)
            {
                // eat it.
            }

            log.Debug($"stop reading messsages from {RemoteAddress}");
        }

        /// <summary>
        ///   Starts reading messages from the remote peer on the specified stream.
        /// </summary>
        public async Task ReadMessagesAsync(Stream stream, CancellationToken cancel)
        {
            IPeerProtocol protocol = new Multistream1();
            try
            {
                while (!cancel.IsCancellationRequested && stream != null && stream.CanRead)
                {
                    await protocol.ProcessMessageAsync(this, stream, cancel).ConfigureAwait(false);
                }
            }
            catch (EndOfStreamException)
            {
                // eat it.
            }
            catch (Exception e)
            {
                if (!cancel.IsCancellationRequested && stream != null)
                {
                    log.Error($"reading message failed {RemoteAddress} {RemotePeer}", e);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///   Signals that the connection is closed (disposed).
        /// </summary>
        public event EventHandler<PeerConnection> Closed;

        /// <summary>
        ///  TODO
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;
            disposedValue = true;

            if (disposing)
            {
                log.Debug($"Closing connection to {RemoteAddress}");
                if (Stream != null)
                {
                    try
                    {
                        Stream.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignore stream already closed.
                    }
                    catch (Exception e)
                    {
                        log.Warn($"Failed to close connection to {RemoteAddress}", e);
                        // eat it.
                    }
                    finally
                    {
                        Stream = null;
                        statsStream = null;
                    }
                }
                SecurityEstablished.TrySetCanceled();
                IdentityEstablished.TrySetCanceled();
                IdentityEstablished.TrySetCanceled();
                Closed?.Invoke(this, this);
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PeerConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

       /// <summary>
       /// 
       /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
