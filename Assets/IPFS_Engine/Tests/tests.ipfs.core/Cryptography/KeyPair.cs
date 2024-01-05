using Ipfs.Core.Cryptography;
using NUnit.Framework;
using System;
using System.Text;

namespace Ipfs.Cryptography
{
    public class KeyPairTest
    {
        [Test]
        public void Generate_RSA()
        {
            KeyPair kp = KeyPair.Generate("rsa", 512);

            Assert.IsNotNull(kp);
            Assert.IsNotNull(kp.PublicKey);
        }

        [Test]
        public void Generate_Ed25519()
        {
            KeyPair kp = KeyPair.Generate("ed25519");

            Assert.IsNotNull(kp);
            Assert.IsNotNull(kp.PublicKey);
        }

        [Test]
        public void SignVerify_Roundtrip_Ed25519()
        {
            byte[] toSign = Encoding.ASCII.GetBytes("Something to be signed");

            KeyPair kp = KeyPair.Generate("ed25519");

            
            Assert.IsNotNull(kp);
            Assert.IsNotNull(kp.PublicKey);

            byte[] signature = kp.Sign(toSign);

            Assert.IsNotNull(signature);

            Assert.DoesNotThrow(() =>
            {
                kp.PublicKey.Verify(toSign, signature);
            });
        }

        [Test]
        public void SignVerify_Roundtrip_Ed25519_negative1()
        {
            byte[] toSign = Encoding.ASCII.GetBytes("Something to be signed");

            KeyPair kp = KeyPair.Generate("ed25519");


            Assert.IsNotNull(kp);
            Assert.IsNotNull(kp.PublicKey);

            byte[] signature = kp.Sign(toSign);

            // Manipulated data
            toSign[0] += 1;

            Assert.IsNotNull(signature);

            ExceptionAssert.Throws<Exception>(() =>
            {
                kp.PublicKey.Verify(toSign, signature);
            });
        }

        [Test]
        public void SignVerify_Roundtrip_Ed25519_negative2()
        {
            byte[] toSign = Encoding.ASCII.GetBytes("Something to be signed");

            KeyPair kp = KeyPair.Generate("ed25519");


            Assert.IsNotNull(kp);
            Assert.IsNotNull(kp.PublicKey);

            byte[] signature = kp.Sign(toSign);

            // Garbled signature
            signature[1] += 1;

            Assert.IsNotNull(signature);

            ExceptionAssert.Throws<Exception>(() =>
            {
                kp.PublicKey.Verify(toSign, signature);
            });
        }

        [Test]
        public void SignVerify_Roundtrip_Ed25519_negative3()
        {
            byte[] toSign = Encoding.ASCII.GetBytes("Something to be signed");

            KeyPair kp1 = KeyPair.Generate("ed25519");
            KeyPair kp2 = KeyPair.Generate("ed25519");

            Assert.IsNotNull(kp1);
            Assert.IsNotNull(kp1.PublicKey);

            byte[] signature = kp1.Sign(toSign);

            Assert.IsNotNull(signature);

            ExceptionAssert.Throws<Exception>(() =>
            {
                // Faked Public Key (Attempt of a MITM attack)
                kp2.PublicKey.Verify(toSign, signature);
            });
        }

    }
}