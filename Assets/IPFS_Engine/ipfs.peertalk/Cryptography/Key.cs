﻿using Org.BouncyCastle.Asn1.Sec;
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
    }
}
