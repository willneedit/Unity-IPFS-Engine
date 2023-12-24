using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PeerTalk.Cryptography
{
    /// <summary>
    ///   An asymmetric key.
    /// </summary>
    public class Key
    {
        const string RsaSigningAlgorithmName = "SHA-256withRSA";
        const string EcSigningAlgorithmName = "SHA-256withECDSA";
        const string Ed25519SigningAlgorithmName = "Ed25519";

        private AsymmetricKeyParameter publicKey;
        private AsymmetricKeyParameter privateKey;
        private string signingAlgorithmName;

        private Key()
        {

        }

        /// <summary>
        ///   Verify that signature matches the data.
        /// </summary>
        /// <param name="data">
        ///   The data to check.
        /// </param>
        /// <param name="signature">
        ///   The supplied signature of the <paramref name="data"/>.
        /// </param>
        /// <exception cref="InvalidDataException">
        ///   The <paramref name="data"/> does match the <paramref name="signature"/>.
        /// </exception>
        public void Verify(byte[] data, byte[] signature)
        {
            var signer = SignerUtilities.GetSigner(signingAlgorithmName);
            signer.Init(false, publicKey);
            signer.BlockUpdate(data, 0, data.Length);
            if (!signer.VerifySignature(signature))
                throw new InvalidDataException("Data does not match the signature.");
        }

        /// <summary>
        ///   Create a signature for the data.
        /// </summary>
        /// <param name="data">
        ///   The data to sign.
        /// </param>
        /// <returns>
        ///   The signature.
        /// </returns>
        public byte[] Sign(byte[] data)
        {
            var signer = SignerUtilities.GetSigner(signingAlgorithmName);
            signer.Init(true, privateKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        /// <summary>
        ///   Unmarshal the public key from the transmission package, see
        ///   https://github.com/libp2p/specs/tree/master/noise
        ///   https://github.com/libp2p/specs/blob/master/peer-ids/peer-ids.md
        /// </summary>
        /// <param name="bytes">
        ///   The Iencoded protobuf PublicKey message.
        /// </param>
        /// <returns>
        ///   The public key.
        /// </returns>
        public static Key UnmarshalPublicKey(byte[] bytes)
        {
            var key = new Key();

            var ms = new MemoryStream(bytes, false);
            var ipfsKey = Serializer.Deserialize<PublicKeyMessage>(ms);

            switch (ipfsKey.Type)
            {
                case KeyType.RSA: // PKIX DER format
                    key.publicKey = PublicKeyFactory.CreateKey(ipfsKey.Data);
                    key.signingAlgorithmName = RsaSigningAlgorithmName;
                    break;
                case KeyType.Ed25519:
                    key.UnmarshalEd25519key(ipfsKey);
                    break;
                case KeyType.Secp256k1:
                    throw new NotImplementedException(); // FIXME unknown encoding. Secp256k1 takes 520 bits (65 bytes), not 32.
                    //key.publicKey = PublicKeyFactory.CreateKey(ipfsKey.Data);
                    //key.signingAlgorithmName = EcSigningAlgorithmName;
                    //break;
                default:
                    throw new InvalidDataException($"Unknown key type of {ipfsKey.Type}.");
            }
            
            return key;
        }

        private void UnmarshalEd25519key(PublicKeyMessage ipfsKey)
        {
            // Envelope the naked binary public key material to satisfy BouncyCastle
            byte[] pkixKeyData = Convert.FromBase64String("MCowBQYDK2VwAyEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
            ipfsKey.Data.CopyTo(pkixKeyData, 12);
            publicKey = PublicKeyFactory.CreateKey(pkixKeyData);
            signingAlgorithmName = Ed25519SigningAlgorithmName;
        }

        public byte[] MarshalPublicKey()
        {
            byte[] spki = SubjectPublicKeyInfoFactory
                .CreateSubjectPublicKeyInfo(publicKey)
                .GetDerEncoded();
            AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(spki);

            KeyType type;
            if (pk is RsaKeyParameters) type = KeyType.RSA;
            else if (pk is Ed25519PublicKeyParameters) type = KeyType.Ed25519;
            else if (pk is ECPublicKeyParameters) type = KeyType.Secp256k1;
            else throw new NotImplementedException();

            byte[] data;
            switch (type)
            {
                case KeyType.RSA: data = spki; break;
                case KeyType.Ed25519: data = spki[12..]; break; // Strip PKIX ASN1 boilerplate
                case KeyType.Secp256k1: throw new NotImplementedException(); // See above
                default: throw new InvalidDataException();
            }

            PublicKeyMessage pkm = new()
            {
                Type = type,
                Data = data,
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, pkm);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///   Create the key from the Bouncy Castle private key.
        /// </summary>
        /// <param name="privateKey">
        ///   The Bouncy Castle private key.
        /// </param>
        public static Key CreatePrivateKey(AsymmetricKeyParameter privateKey)
        {
            var key = new Key();
            key.privateKey = privateKey;

            // Get the public key from the private key.
            if (privateKey is RsaPrivateCrtKeyParameters rsa)
            {
                key.publicKey = new RsaKeyParameters(false, rsa.Modulus, rsa.PublicExponent);
                key.signingAlgorithmName = RsaSigningAlgorithmName;
            }
            else if (privateKey is Ed25519PrivateKeyParameters ed)
            {
                key.publicKey = ed.GeneratePublicKey();
                key.signingAlgorithmName = Ed25519SigningAlgorithmName;
            }
            else if (privateKey is ECPrivateKeyParameters ec)
            {
                var q = ec.Parameters.G.Multiply(ec.D);
                key.publicKey = new ECPublicKeyParameters(q, ec.Parameters);
                key.signingAlgorithmName = EcSigningAlgorithmName;
            }
            if (key.publicKey == null)
                throw new NotSupportedException($"The key type {privateKey.GetType().Name} is not supported.");

            return key;
        }

        public static Key GenerateKeyPair(string keyType, int size = 2048)
        {
            IAsymmetricCipherKeyPairGenerator g;
            string san;
            switch (keyType)
            {
                case "rsa":
                    g = GeneratorUtilities.GetKeyPairGenerator("RSA");
                    g.Init(new RsaKeyGenerationParameters(
                        BigInteger.ValueOf(0x10001), new SecureRandom(), size, 25));
                    san = RsaSigningAlgorithmName;
                    break;
                case "ed25519":
                    g = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
                    g.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
                    san = Ed25519SigningAlgorithmName;
                    break;
                case "secp256k1":
                    g = GeneratorUtilities.GetKeyPairGenerator("EC");
                    g.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256k1, new SecureRandom()));
                    san = EcSigningAlgorithmName;
                    break;
                default:
                    throw new Exception($"Invalid key type '{keyType}'.");
            }

            var keyPair = g.GenerateKeyPair();

            return new()
            {
                signingAlgorithmName = san,
                privateKey = keyPair.Private,
                publicKey = keyPair.Public,
            };
        }

        public static Key ImportPublicKey(string publicKey)
        {
            byte[] encapsulated = Convert.FromBase64String(publicKey);
            MemoryStream ms = new(encapsulated, false);
            PublicKeyMessage pkm = Serializer.Deserialize<PublicKeyMessage>(ms);

            AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(pkm.Data);

            string algName;
            if (pk is RsaKeyParameters) algName = RsaSigningAlgorithmName;
            else if (pk is Ed25519PublicKeyParameters) algName = Ed25519SigningAlgorithmName;
            else if (pk is ECPublicKeyParameters) algName = EcSigningAlgorithmName;
            else throw new NotImplementedException();

            return new()
            {
                publicKey = pk,
                signingAlgorithmName = algName
            };
        }

        internal enum KeyType
        {
            RSA = 0,
            Ed25519 = 1,
            Secp256k1 = 2,
            ECDH = 4,
        }

        [ProtoContract]
        class PublicKeyMessage
        {
            [ProtoMember(1, IsRequired = true)]
            public KeyType Type { get; set; }
            [ProtoMember(2, IsRequired = true)]
            public byte[] Data { get; set; }
        }

        [ProtoContract]
        class PrivateKeyMessage
        {
            [ProtoMember(1, IsRequired = true)]
            public KeyType Type;
            [ProtoMember(2, IsRequired = true)]
            public byte[] Data;
        }
    }
}
