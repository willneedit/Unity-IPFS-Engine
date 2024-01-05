
using System;

namespace PeerTalk.Muxer.Yamux
{
    public class FrameHeader
    {
        public int version = Constants.YAMUX_VERSION;
        public FrameType type;
        public int flag;
        public int streamID;
        public int length;


        public static FrameHeader DecodeHeader(byte[] data)
        {
            if (data[0] != Constants.YAMUX_VERSION)
                throw new Exception("Invalid frame version");

            return new FrameHeader()
            {
                version = 0,
                type = (FrameType)data[1],
                flag = (data[2] << 8) + data[3],
                streamID = (data[4] << 24) + (data[5] << 16) + (data[6] << 8) + data[7],
                length = (data[8] << 24) + (data[9] << 16) + (data[10] << 8) + data[11]
            };
        }

        public byte[] EncodeHeader()
        {
            byte[] frame = new byte[Constants.HEADEER_LENGTH];
            frame[0] = (byte)version;
            frame[1] = (byte)type;

            frame[2] = (byte)(flag >> 8);
            frame[3] = (byte)flag;

            frame[4] = (byte)(streamID >> 24);
            frame[5] = (byte)(streamID >> 16);
            frame[6] = (byte)(streamID >> 8);
            frame[7] = (byte)streamID;

            frame[8] = (byte)(length >> 24);
            frame[9] = (byte)(length >> 16);
            frame[10] = (byte)(length >> 8);
            frame[11] = (byte)length;

            return frame;
        }
    }
}