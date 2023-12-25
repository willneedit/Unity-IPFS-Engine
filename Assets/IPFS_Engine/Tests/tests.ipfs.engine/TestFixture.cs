using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Common.Logging;
using Common.Logging.XSimple;

namespace Ipfs.Engine
{
    [TestFixture]
    public class TestFixture
    {
        const string passphrase = "this is not a secure pass phrase";
        public static IpfsEngine Ipfs = new IpfsEngine(passphrase.ToCharArray());
        public static IpfsEngine IpfsOther = new IpfsEngine(passphrase.ToCharArray());

        static TestFixture()
        {
            Ipfs.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), "ipfs-test");
            Ipfs.Options.KeyChain.DefaultKeyType = "ed25519";
            Ipfs.Options.KeyChain.DefaultKeySize = 2048;
            Ipfs.Config.SetAsync(
                "Addresses.Swarm", 
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/10234" })
            ).Wait();

            IpfsOther.Options.Repository.Folder = Path.Combine(Path.GetTempPath(), "ipfs-other");
            IpfsOther.Options.KeyChain.DefaultKeySize = 512;
            IpfsOther.Config.SetAsync(
                "Addresses.Swarm",
                JToken.FromObject(new string[] { "/ip4/0.0.0.0/tcp/10235" })
            ).Wait();
        }

        [Test]
        public void Engine_Exists()
        {
            Assert.IsNotNull(Ipfs);
            Assert.IsNotNull(IpfsOther);
        }

        [OneTimeSetUp]
        public static void AssemblyInitialize()
        {
            // set logger factory
            var properties = new Common.Logging.Configuration.NameValueCollection
            {
                ["level"] = "DEBUG",
                ["showLogName"] = "true",
                ["showDateTime"] = "true",
                ["dateTimeFormat"] = "HH:mm:ss.fff"

            };
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            if (Directory.Exists(Ipfs.Options.Repository.Folder))
            {
                Directory.Delete(Ipfs.Options.Repository.Folder, true);
            }
            if (Directory.Exists(IpfsOther.Options.Repository.Folder))
            {
                Directory.Delete(IpfsOther.Options.Repository.Folder, true);
            }
        }
    }
}
