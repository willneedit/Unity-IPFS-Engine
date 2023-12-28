
namespace PeerTalk.Muxer.Yamux
{
    public static class Constants
    {
        public const string ERR_INVALID_FRAME = "ERR_INVALID_FRAME";
        public const string ERR_UNREQUESTED_PING = "ERR_UNREQUESTED_PING";
        public const string ERR_NOT_MATCHING_PING = "ERR_NOT_MATCHING_PING";
        public const string ERR_STREAM_ALREADY_EXISTS = "ERR_STREAM_ALREADY_EXISTS";
        public const string ERR_DECODE_INVALID_VERSION = "ERR_DECODE_INVALID_VERSION";
        public const string ERR_BOTH_CLIENTS = "ERR_BOTH_CLIENTS";
        public const string ERR_RECV_WINDOW_EXCEEDED = "ERR_RECV_WINDOW_EXCEEDED";


        // local errors
        public const string ERR_INVALID_CONFIG = "ERR_INVALID_CONFIG";
        public const string ERR_MUXER_LOCAL_CLOSED = "ERR_MUXER_LOCAL_CLOSED";
        public const string ERR_MUXER_REMOTE_CLOSED = "ERR_MUXER_REMOTE_CLOSED";
        public const string ERR_STREAM_RESET = "ERR_STREAM_RESET";
        public const string ERR_STREAM_ABORT = "ERR_STREAM_ABORT";
        public const string ERR_MAX_OUTBOUND_STREAMS_EXCEEDED = "ERROR_MAX_OUTBOUND_STREAMS_EXCEEDED";
        public const string ERR_DECODE_IN_PROGRESS = "ERR_DECODE_IN_PROGRESS";

        /**
         * INITIAL_STREAM_WINDOW is the initial stream window size.
         *
         * Not an implementation choice, this is defined in the specification
         */
        public const int INITIAL_STREAM_WINDOW = 256 * 1024;

        /**
         * Default max stream window
         */
        public const int MAX_STREAM_WINDOW = 16 * 1024 * 1024;

        public const int YAMUX_VERSION = 0;

        public const int HEADEER_LENGTH = 12;
    }

    public enum FrameType
    {
        /** Used to transmit data. May transmit zero length payloads depending on the flags. */
        Data = 0x0,
        /** Used to updated the senders receive window size. This is used to implement per-session flow control. */
        WindowUpdate = 0x1,
        /** Used to measure RTT. It can also be used to heart-beat and do keep-alives over TCP. */
        Ping = 0x2,
        /** Used to close a session. */
        GoAway = 0x3,
    }

    public enum Flag
    {
        /** Signals the start of a new stream. May be sent with a data or window update message. Also sent with a ping to indicate outbound. */
        SYN = 0x1,
        /** Acknowledges the start of a new stream. May be sent with a data or window update message. Also sent with a ping to indicate response. */
        ACK = 0x2,
        /** Performs a half-close of a stream. May be sent with a data message or window update. */
        FIN = 0x4,
        /** Reset a stream immediately. May be sent with a data or window update message. */
        RST = 0x8,
    }

    public enum GoAwayCode
    {
        NormalTermination = 0x0,
        ProtocolError = 0x1,
        InternalError = 0x2,
    }
}