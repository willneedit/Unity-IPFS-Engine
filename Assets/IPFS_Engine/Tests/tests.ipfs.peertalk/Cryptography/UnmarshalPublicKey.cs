using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;

namespace PeerTalk.Cryptography.Marshaling
{
    [TestFixture]
    public class UnmarshalPublicKey
    {
        private void test_key(string marshaledKey)
        {
            byte[] data = SimpleBase.Base16.Decode(marshaledKey);
            Key key = Key.UnmarshalPublicKey(data);
        }

        // Test vectors seen in
        // https://github.com/libp2p/specs/blob/master/peer-ids/peer-ids.md

        [Test]
        public void Unmarshal_RSA()
        {
            test_key(
                "080012a60430820222300d06092a864886f70d01010105000382020f003082" +
                "020a0282020100e1beab071d08200bde24eef00d049449b07770ff9910257b" +
                "2d7d5dda242ce8f0e2f12e1af4b32d9efd2c090f66b0f29986dbb645dae988" +
                "0089704a94e5066d594162ae6ee8892e6ec70701db0a6c445c04778eb3de12" +
                "93aa1a23c3825b85c6620a2bc3f82f9b0c309bc0ab3aeb1873282bebd3da03" +
                "c33e76c21e9beb172fd44c9e43be32e2c99827033cf8d0f0c606f4579326c9" +
                "30eb4e854395ad941256542c793902185153c474bed109d6ff5141ebf9cd25" +
                "6cf58893a37f83729f97e7cb435ec679d2e33901d27bb35aa0d7e20561da08" +
                "885ef0abbf8e2fb48d6a5487047a9ecb1ad41fa7ed84f6e3e8ecd5d98b3982" +
                "d2a901b4454991766da295ab78822add5612a2df83bcee814cf50973e80d7e" +
                "f38111b1bd87da2ae92438a2c8cbcc70b31ee319939a3b9c761dbc13b5c086" +
                "d6b64bf7ae7dacc14622375d92a8ff9af7eb962162bbddebf90acb32adb5e4" +
                "e4029f1c96019949ecfbfeffd7ac1e3fbcc6b6168c34be3d5a2e5999fcbb39" +
                "bba7adbca78eab09b9bc39f7fa4b93411f4cc175e70c0a083e96bfaefb04a9" +
                "580b4753c1738a6a760ae1afd851a1a4bdad231cf56e9284d832483df215a4" +
                "6c1c21bdf0c6cfe951c18f1ee4078c79c13d63edb6e14feaeffabc90ad317e" +
                "4875fe648101b0864097e998f0ca3025ef9638cd2b0caecd3770ab54a1d9c6" +
                "ca959b0f5dcbc90caeefc4135baca6fd475224269bbe1b0203010001");
        }

        [Test]
        [Ignore("No source for the 'standard Bitcoin EC encoding for Secp256k1 public and private keys'")]
        public void Unmarshal_secp256k1()
        {
            test_key(
                "08021221037777e994e452c21604f91de093ce415f5432f701dd8cd1a7a6fea0e630bfca99");
        
        }

        [Test]
        public void Unmarshal_ed25519()
        {
            test_key("080112201ed1e8fae2c4a144b8be8fd4b47bf3d3b34b871c3cacf6010f0e42d474fce27e");
        }
    }
}