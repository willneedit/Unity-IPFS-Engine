using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class BlockRepositoryApiTest
    {
        IpfsEngine ipfs = TestFixture.Ipfs;

        [Test]
        public void Exists()
        {
            Assert.IsNotNull(ipfs.BlockRepository);
        }

        [Test]
		public void StatsAsync()
		{
			Task.Run(Stats).Wait();
		}

		public async Task Stats()
        {
            var stats = await ipfs.BlockRepository.StatisticsAsync();
            var version = await ipfs.BlockRepository.VersionAsync();
            Assert.AreEqual(stats.Version, version);
        }

        [Test]
		public void GarbageCollectionAsync()
		{
			Task.Run(GarbageCollection).Wait();
		}

		public async Task GarbageCollection()
        {
            var pinned = await ipfs.Block.PutAsync(new byte[256], pin: true);
            var unpinned = await ipfs.Block.PutAsync(new byte[512], pin: false);
            Assert.AreNotEqual(pinned, unpinned);
            Assert.IsNotNull(await ipfs.Block.StatAsync(pinned));
            Assert.IsNotNull(await ipfs.Block.StatAsync(unpinned));

            await ipfs.BlockRepository.RemoveGarbageAsync();
            Assert.IsNotNull(await ipfs.Block.StatAsync(pinned));
            Assert.IsNull(await ipfs.Block.StatAsync(unpinned));
        }

        [Test]
        public void VersionFileMissingAsync()
		{
			Task.Run(VersionFileMissing).Wait();
		}

		public async Task VersionFileMissing()
        {
            var versionPath = Path.Combine(ipfs.Options.Repository.ExistingFolder(), "version");
            var versionBackupPath = versionPath + ".bak";

            try
            {
                if (File.Exists(versionPath))
                {
                    File.Move(versionPath, versionBackupPath);
                }

                Assert.AreEqual("0", await ipfs.BlockRepository.VersionAsync());
            }
            finally
            {
                if (File.Exists(versionBackupPath))
                {
                    File.Move(versionBackupPath, versionPath);
                }
            }
        }

    }
}
