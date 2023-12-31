﻿using NUnit.Framework;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ipfs.Engine
{
    [TestFixture]
    public class RandomWalkTest
    {
        [Test]
        public void CanStartAndStopAsync()
		{
			Task.Run(CanStartAndStop).Wait();
		}

		public async Task CanStartAndStop()
        {
            var walk = new RandomWalk();
            await walk.StartAsync();
            await walk.StopAsync();

            await walk.StartAsync();
            await walk.StopAsync();
        }

        [Test]
        public void CannotStartTwice()
        {
            var walk = new RandomWalk();
            walk.StartAsync().Wait();
            ExceptionAssert.Throws<Exception>(() =>
            {
                walk.StartAsync().Wait();
            });
        }

        [Test]
        public void CanStopMultipletimesAsync()
		{
			Task.Run(CanStopMultipletimes).Wait();
		}

		public async Task CanStopMultipletimes()
        {
            var walk = new RandomWalk();
            await walk.StartAsync();
            await walk.StopAsync();
            await walk.StopAsync();
            await walk.StartAsync();
            await walk.StopAsync();
        }
    }
}
