using Common.MyInfo;
using Newtonsoft.Json;
using NUnit.Framework;
using sg.gov.ndi.MyInfoConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ir.Common.UnitTest.Common.MyInfo
{
    [TestFixture]
    public class MyInfoClientFixture
    {
        MyInfoConnectorConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = MyInfoConnectorConfigFixture.GetFromAppConfig();
        }

        private MyInfoClient GetSystemUnderTest()
        {
            var connector = MyInfoConnector.Create(_config);
            return new MyInfoClient(connector);
        }

        [Test]
        public void GetAuthoriseUrl()
        {
            var sut = GetSystemUnderTest();
            var state = "randomString";

            var url = sut.GetAuthoriseUrl(state);
            Console.WriteLine($"With State: {url}");

            Assert.That(url.Contains("v3/authorise"));
            Assert.That(url.Contains(state));
            Assert.That(url.Contains(Uri.EscapeDataString(MyInfoClient.RedirectUriWeb)));

            url = sut.GetAuthoriseUrl();
            Assert.IsNotEmpty(url, "test when no state passed");
            Console.WriteLine($"No State: {url}");
        }

        [Test]
        public void ParseJson()
        {
            var json = Encoding.UTF8.GetString(MyInfoResource.SampleS3100052A);
            
            Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<SingpassRootData>(json));
        }
    }
}
