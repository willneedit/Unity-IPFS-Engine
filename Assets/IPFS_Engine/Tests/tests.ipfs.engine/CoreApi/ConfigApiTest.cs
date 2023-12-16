using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Engine
{

    [TestFixture]
    public class ConfigApiTest
    {
        const string apiAddress = "/ip4/127.0.0.1/tcp/";
        const string gatewayAddress = "/ip4/127.0.0.1/tcp/";

        [Test]
        public async void Get_Entire_Config()
        {
            var ipfs = TestFixture.Ipfs;
            var config = await ipfs.Config.GetAsync();
            StringAssert.StartsWith(config["Addresses"]["API"].Value<string>(), apiAddress);
        }

        [Test]
        public async void Get_Scalar_Key_Value()
        {
            var ipfs = TestFixture.Ipfs;
            var api = await ipfs.Config.GetAsync("Addresses.API");
            StringAssert.StartsWith(api.Value<string>(), apiAddress);
        }

        [Test]
        public async void Get_Object_Key_Value()
        {
            var ipfs = TestFixture.Ipfs;
            var addresses = await ipfs.Config.GetAsync("Addresses");
            StringAssert.StartsWith(addresses["API"].Value<string>(), apiAddress);
            StringAssert.StartsWith(addresses["Gateway"].Value<string>(), gatewayAddress);
        }

        [Test]
        public void Keys_are_Case_Sensitive()
        {
            var ipfs = TestFixture.Ipfs;
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            StringAssert.StartsWith(api.Value<string>(), apiAddress);

            ExceptionAssert.Throws<Exception>(() => { var x = ipfs.Config.GetAsync("Addresses.api").Result; });
        }

        [Test]
        public async void Set_String_Value()
        {
            const string key = "foo";
            const string value = "foobar";
            var ipfs = TestFixture.Ipfs;
            await ipfs.Config.SetAsync(key, value);
            Assert.AreEqual(value, await ipfs.Config.GetAsync(key));
        }

        [Test]
        public async void Set_JSON_Value()
        {
            const string key = "API.HTTPHeaders.Access-Control-Allow-Origin";
            JToken value = JToken.Parse("['http://example.io']");
            var ipfs = TestFixture.Ipfs;
            await ipfs.Config.SetAsync(key, value);
            Assert.AreEqual("http://example.io", ipfs.Config.GetAsync(key).Result[0]);
        }

    }
}
