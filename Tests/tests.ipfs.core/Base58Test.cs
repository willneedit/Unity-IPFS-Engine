using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Ipfs
{
    [TestFixture]
    public class Base58Test
    {
        [Test]
        public void Encode()
        {
            Assert.AreEqual("jo91waLQA1NNeBmZKUF", Base58.Encode(Encoding.UTF8.GetBytes("this is a test")));
            Assert.AreEqual("jo91waLQA1NNeBmZKUF", Encoding.UTF8.GetBytes("this is a test").ToBase58());
        }

        [Test]
        public void Decode()
        {
            Assert.AreEqual("this is a test", Encoding.UTF8.GetString(Base58.Decode("jo91waLQA1NNeBmZKUF")));
            Assert.AreEqual("this is a test", Encoding.UTF8.GetString("jo91waLQA1NNeBmZKUF".FromBase58()));
        }

        /// <summary>
        ///    C# version of base58Test in <see href="https://github.com/ipfs/java-ipfs-api/blob/master/test/org/ipfs/Test.java"/>
        /// </summary>
        [Test]
        public void Java()
        {
            String input = "QmPZ9gcCEpqKTo6aq61g2nXGUhM4iCL3ewB6LDXZCtioEB";
            byte[] output = Base58.Decode(input);
            String encoded = Base58.Encode(output);
            Assert.AreEqual(input, encoded);
        }

        [Test]
        public void Decode_Bad()
        {
            ExceptionAssert.Throws<InvalidOperationException>(() =>  Base58.Decode("jo91waLQA1NNeBmZKUF=="));
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual("1111111", Base58.Encode(new byte[7]));
            Assert.AreEqual(7, Base58.Decode("1111111").Length);
            Assert.IsTrue(Base58.Decode("1111111").All(b => b == 0));
        }
    }
}
