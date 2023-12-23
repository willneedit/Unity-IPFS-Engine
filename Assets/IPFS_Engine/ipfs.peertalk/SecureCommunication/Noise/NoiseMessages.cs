using ProtoBuf;
using System.Collections.Generic;

/*
syntax = "proto3";
package pb;

message NoiseHandshakePayload {
	bytes identity_key = 1;
	bytes identity_sig = 2;
	bytes data = 3;
}
*/

namespace PeerTalk.SecureCommunication
{
    [ProtoContract]
    class NoiseExtensions
    {
        [ProtoMember(1)]
        public List<byte[]> WebtransportCerthashes;

        [ProtoMember(2)]
        public List<string> StreamMuxers;

    }

    [ProtoContract]
    class NoiseHandshakePayload
    {
        #pragma warning disable 0649
        [ProtoMember(1)]
        public byte[] IdentityKey;

        [ProtoMember(2)]
        public byte[] IdentitySig;

        // [ProtoMember(3)]
        // public byte[] Data;

        [ProtoMember(4)]
        public NoiseExtensions Extensions;
        #pragma warning restore 0649
    }
}
