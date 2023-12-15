using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;
using Google.Protobuf;

namespace Ipfs
{
    [TestFixture]
    public class NamedContentTest
    {
        [Test]
        public void Properties()
        {
            var nc = new NamedContent
            {
                ContentPath = "/ipfs/...",
                NamePath = "/ipns/..."
            };
            Assert.AreEqual("/ipfs/...", nc.ContentPath);
            Assert.AreEqual("/ipns/...", nc.NamePath);
        }

    }
}
