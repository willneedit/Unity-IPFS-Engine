using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Engine.Cryptography
{
    [TestFixture]
    public class SecureStringExtensionsTest
    {
        [Test]
        public void UseBytes()
        {
            var secret = new SecureString();
            var expected = new char[] { 'a', 'b', 'c' };
            foreach (var c in expected) secret.AppendChar(c);
            secret.UseSecretBytes(bytes =>
            {
                Assert.AreEqual(expected.Length, bytes.Length);
                for (var i = 0; i < expected.Length; ++i)
                    Assert.AreEqual((int)expected[i], (int)bytes[i]);
            });
        }


    }
}
