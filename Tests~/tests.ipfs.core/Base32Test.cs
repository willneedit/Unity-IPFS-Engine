using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Ipfs
{
    [TestFixture]
    public class Base32EncodeTests
    {
        byte[] GetStringBytes(string x)
        {
            return Encoding.ASCII.GetBytes(x);
        }

        [Test]
        public void Vector1()
        {
            Assert.AreEqual(string.Empty, Base32.Encode(GetStringBytes(string.Empty)));
        }

        [Test]
        public void Vector2()
        {
            Assert.AreEqual("my", Base32.Encode(GetStringBytes("f")));
        }

        [Test]
        public void Vector3()
        {
            Assert.AreEqual("mzxq", Base32.Encode(GetStringBytes("fo")));
        }

        [Test]
        public void Vector4()
        {
            Assert.AreEqual("mzxw6", Base32.Encode(GetStringBytes("foo")));
        }

        [Test]
        public void Vector5()
        {
            Assert.AreEqual("mzxw6yq", Base32.Encode(GetStringBytes("foob")));
        }

        [Test]
        public void Vector6()
        {
            Assert.AreEqual("mzxw6ytb", Base32.Encode(GetStringBytes("fooba")));
        }

        [Test]
        public void Vector7()
        {
            Assert.AreEqual("mzxw6ytboi", Base32.Encode(GetStringBytes("foobar")));
        }
    }

    [TestFixture]
    public class Base32DecodeTests
    {
        byte[] GetStringBytes(string x)
        {
            return Encoding.ASCII.GetBytes(x);
        }

        [Test]
        public void Vector1()
        {
            CollectionAssert.AreEqual(GetStringBytes(string.Empty), Base32.Decode(string.Empty));
        }

        [Test]
        public void Vector2()
        {
            CollectionAssert.AreEqual(GetStringBytes("f"), Base32.Decode("MY======"));
        }

        [Test]
        public void Vector3()
        {
            CollectionAssert.AreEqual(GetStringBytes("fo"), Base32.Decode("MZXQ===="));
        }

        [Test]
        public void Vector4()
        {
            CollectionAssert.AreEqual(GetStringBytes("foo"), Base32.Decode("MZXW6==="));
        }

        [Test]
        public void Vector5()
        {
            CollectionAssert.AreEqual(GetStringBytes("foob"), Base32.Decode("MZXW6YQ="));
        }

        [Test]
        public void Vector6()
        {
            CollectionAssert.AreEqual(GetStringBytes("fooba"), Base32.Decode("MZXW6YTB"));
        }

        [Test]
        public void Vector7()
        {
            CollectionAssert.AreEqual(GetStringBytes("foobar"), Base32.Decode("MZXW6YTBOI======"));
        }
    }
}
