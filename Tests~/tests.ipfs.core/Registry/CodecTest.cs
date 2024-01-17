using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Google.Protobuf;

namespace Ipfs.Registry
{
    [TestFixture]
    public class CodecTest
    {
        [Test]
        public void Bad_Name()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register(null, 1));
            ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register("", 1));
            ExceptionAssert.Throws<ArgumentNullException>(() => Codec.Register("   ", 1));
        }

        [Test]
        public void Name_Already_Exists()
        {
            ExceptionAssert.Throws<ArgumentException>(() => Codec.Register("raw", 1));
        }

        [Test]
        public void Code_Already_Exists()
        {
            ExceptionAssert.Throws<ArgumentException>(() => Codec.Register("raw-x", 0x55));
        }

        [Test]
        public void Algorithms_Are_Enumerable()
        {
            Assert.AreNotEqual(0, Codec.All.Count());
        }

        [Test]
        public void Register()
        {
            var codec = Codec.Register("something-new", 0x0bad);
            try
            {
                Assert.AreEqual("something-new", codec.Name);
                Assert.AreEqual("something-new", codec.ToString());
                Assert.AreEqual(0x0bad, codec.Code);
            }
            finally
            {
                Codec.Deregister(codec);
            }
        }
    }
}
