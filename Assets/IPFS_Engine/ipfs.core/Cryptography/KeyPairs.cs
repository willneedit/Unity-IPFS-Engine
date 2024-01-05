
using Ipfs.Core.Cryptography.Proto;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;

namespace Ipfs.Core.Cryptography
{
    public class KeyPair
    {
        const string RsaSigningAlgorithmName = "SHA-256withRSA";
        const string EcSigningAlgorithmName = "SHA-256withECDSA";
        const string Ed25519SigningAlgorithmName = "Ed25519";

        private AsymmetricKeyParameter privateKey = null;
        private PublicKey publicKey = null;
        private string signingAlgorithmName;

        public PublicKey PublicKey
        { 
            get {
                if (publicKey == null) DerivePublicKey();
                return publicKey; 
            } 
        }

        private void DerivePublicKey()
        {
            AsymmetricKeyParameter parmPublicKey;

            // Get the public key from the private key.
            if (privateKey is RsaPrivateCrtKeyParameters rsa)
            {
                parmPublicKey = new RsaKeyParameters(false, rsa.Modulus, rsa.PublicExponent);
                signingAlgorithmName = RsaSigningAlgorithmName;
            }
            else if (privateKey is Ed25519PrivateKeyParameters ed)
            {
                parmPublicKey = ed.GeneratePublicKey();
                signingAlgorithmName = Ed25519SigningAlgorithmName;
            }
            else if (privateKey is ECPrivateKeyParameters ec)
            {
                var q = ec.Parameters.G.Multiply(ec.D);
                parmPublicKey = new ECPublicKeyParameters(q, ec.Parameters);
                signingAlgorithmName = EcSigningAlgorithmName;
            }
            else
                throw new NotSupportedException($"The key type {privateKey.GetType().Name} is not supported.");

            publicKey = PublicKey.Import(parmPublicKey);
        }

        public static KeyPair Generate(string keyType, int size = 2048)
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
                publicKey = PublicKey.Import(keyPair.Public),
            };
        }

        public static KeyPair Import(AsymmetricKeyParameter privateKey)
        {
            return new()
            {
                privateKey = privateKey,
                publicKey = null,
                signingAlgorithmName = null
            };
        }

        public byte[] Sign(byte[] data)
        {
            if (signingAlgorithmName == null) DerivePublicKey();

            var signer = SignerUtilities.GetSigner(signingAlgorithmName);
            signer.Init(true, privateKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }
    }
}