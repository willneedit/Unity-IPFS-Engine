using Ipfs;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PeerTalk.Protocols
{
    [TestFixture]
    public class VersionedNameTest
    {
        [Test]
        public void Parsing()
        {
            var vn = VersionedName.Parse("/multistream/1.0.0");
            Assert.AreEqual("multistream", vn.Name);
            Assert.AreEqual("1.0.0", vn.Version.ToString());

            vn = VersionedName.Parse("/ipfs/id/1.0.0");
            Assert.AreEqual("ipfs/id", vn.Name);
            Assert.AreEqual("1.0.0", vn.Version.ToString());
        }

        [Test]
        public void Stringing()
        {
            var vn = new VersionedName { Name = "x", Version = new Semver.SemVersion(0, 42) };
            Assert.AreEqual("/x/0.42.0", vn.ToString());
        }

        [Test]
        public void Value_Equality()
        {
            var a0 = VersionedName.Parse("/x/1.0.0");
            var a1 = VersionedName.Parse("/x/1.0.0");
            var b = VersionedName.Parse("/x/1.1.0");
            VersionedName c = null;
            VersionedName d = null;

            Assert.IsTrue(c == d);
            Assert.IsFalse(c == b);
            Assert.IsFalse(b == c);

            Assert.IsFalse(c != d);
            Assert.IsTrue(c != b);
            Assert.IsTrue(b != c);

#pragma warning disable 1718
            Assert.IsTrue(a0 == a0);
            Assert.IsTrue(a0 == a1);
            Assert.IsFalse(a0 == b);

#pragma warning disable 1718
            Assert.IsFalse(a0 != a0);
            Assert.IsFalse(a0 != a1);
            Assert.IsTrue(a0 != b);

            Assert.IsTrue(a0.Equals(a0));
            Assert.IsTrue(a0.Equals(a1));
            Assert.IsFalse(a0.Equals(b));

            Assert.AreEqual(a0, a0);
            Assert.AreEqual(a0, a1);
            Assert.AreNotEqual(a0, b);

            Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
            Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
            Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Comparing()
        {
            var a0 = VersionedName.Parse("/x/1.0.0");
            var a1 = VersionedName.Parse("/x/1.0.0");
            var b = VersionedName.Parse("/x/1.1.0");
            var c = VersionedName.Parse("/y/0.42.0");

            Assert.AreEqual(0, a0.CompareTo(a1));
            Assert.AreEqual(0, a1.CompareTo(a0));

            Assert.AreEqual(1, b.CompareTo(a0));
            Assert.AreEqual(-1, a0.CompareTo(b));

            Assert.AreEqual(1, c.CompareTo(b));
            Assert.AreEqual(-1, b.CompareTo(c));
        }

        [Test]
        public void Ordering()
        {
            var names = new List<VersionedName>
            {
                VersionedName.Parse("/x/1.0.0"),
                VersionedName.Parse("/x/1.1.0"),
                VersionedName.Parse("/y/0.42.0"),
            };
            var ordered = names.OrderByDescending(n => n).ToArray();
            Assert.AreEqual("/y/0.42.0", ordered[0].ToString());
            Assert.AreEqual("/x/1.1.0", ordered[1].ToString());
            Assert.AreEqual("/x/1.0.0", ordered[2].ToString());
        }
    }
}
