using ACMESharp.ACME;
using ACMESharp.Providers.OVH;
using ACMESharp.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ovh.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACMESharp.Providers.OVH_test
{
    [TestClass]
    public class OvhTest
    {
        public static readonly IReadOnlyDictionary<string, object> EMPTY_PARAMS =
                new Dictionary<string, object>
                {
                    ["DomainName"] = "",
                    ["Endpoint"] = "",
                    ["ApplicationKey"] = "",
                    ["ApplicationSecret"] = "",
                    ["ConsumerKey"] = "",
                };

        private static IReadOnlyDictionary<string, object> _handlerParams = EMPTY_PARAMS;

        private static IReadOnlyDictionary<string, object> GetParams()
        {
            return _handlerParams;
        }

        [ClassInitialize]
        public static void Init(TestContext tctx)
        {
            var file = new FileInfo("Config\\OvhHandlerParams.json");
            if (file.Exists)
            {
                using (var fs = new FileStream(file.FullName, FileMode.Open))
                {
                    _handlerParams = JsonHelper.Load<Dictionary<string, object>>(fs);
                }
            }
        }

        public static OvhChallengeHandlerProvider GetProvider()
        {
            return new OvhChallengeHandlerProvider();
        }

        public static OvhChallengeHandler GetHandler(Challenge challenge)
        {
            return (OvhChallengeHandler)GetProvider().GetHandler(challenge, null);
        }

        public static OvhHelper GetHelper()
        {
            var p = GetParams();
            var h = new OvhHelper(
                    (string)p["Endpoint"],
                    (string)p["ApplicationKey"],
                    (string)p["ApplicationSecret"],
                    (string)p["ConsumerKey"]
                );
            return h;
        }

        [TestMethod]
        public void TestConfigParameters()
        {
            var parameters = GetParams();
            if (string.IsNullOrWhiteSpace((string)parameters["Endpoint"]))
            {
                throw new Exception("\"Endpoint\" is not defined in config\\OvhHandlerParams.json. Visit https://github.com/ovh/csharp-ovh#2-configure-your-application to get the complete list of end points and how to create your ApplicationKey and get your ApplicationSecret.");
            }
            else if (string.IsNullOrWhiteSpace((string)parameters["ApplicationKey"]) || string.IsNullOrWhiteSpace((string)parameters["ApplicationSecret"]))
            {
                var createApiUrl = OvhChallengeHandler.GetCreateApiUrl((string)parameters["Endpoint"]);
                throw new Exception($"\"ApplicationKey\" or \"ApplicationSecret\" is not defined in config\\OvhHandlerParams.json. Visit {createApiUrl} to create your ApplicationKey and get your ApplicationSecret.");
            }
            else if (string.IsNullOrWhiteSpace((string)parameters["ConsumerKey"]))
            {
                CredentialRequestResult requestConsumer = OvhChallengeHandler.RequestConsumerKey((string)parameters["Endpoint"],
                    (string)parameters["ApplicationKey"], (string)parameters["ApplicationSecret"],
                    null, "https://eu.api.ovh.com/");
                throw new Exception($"\"ConsumerKey\" is not defined in config\\OvhHandlerParams.json. Go to {requestConsumer.ValidationUrl} to validate your application credentials and set your ConsumerKey with \"{requestConsumer.ConsumerKey}\".");
            }

        }

        [TestMethod]
        public void TestParameterDescriptions()
        {
            var p = GetProvider();
            var dp = p.DescribeParameters();

            Assert.IsNotNull(dp);
            Assert.IsTrue(dp.Any());
        }

        [TestMethod]
        public void TestSupportedChallenges()
        {
            var p = GetProvider();

            Assert.IsTrue(p.IsSupported(TestCommon.DNS_CHALLENGE));
            Assert.IsFalse(p.IsSupported(TestCommon.HTTP_CHALLENGE));
            Assert.IsFalse(p.IsSupported(TestCommon.TLS_SNI_CHALLENGE));
            Assert.IsFalse(p.IsSupported(TestCommon.FAKE_CHALLENGE));
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void TestRequiredParams()
        {
            var p = GetProvider();
            var c = TestCommon.DNS_CHALLENGE;
            var h = p.GetHandler(c, new Dictionary<string, object>());
        }

        [TestMethod]
        public void TestHandlerLifetime()
        {
            var p = GetProvider();
            var c = TestCommon.DNS_CHALLENGE;
            var h = p.GetHandler(c, GetParams());

            Assert.IsNotNull(h);
            Assert.IsFalse(h.IsDisposed);
            h.Dispose();
            Assert.IsTrue(h.IsDisposed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestHandlerDisposedAccess()
        {
            var p = GetProvider();
            var c = TestCommon.DNS_CHALLENGE;
            var h = p.GetHandler(c, GetParams());

            h.Dispose();
            h.Handle(null);
        }

        [TestMethod]
        public void TestAddDnsRecord()
        {
            var h = GetHelper();
            var rrName = "acmesharp-test." + GetParams()["DomainName"];
            var rrValue = "testrr-" + DateTime.Now.ToString("yyyyMMddHHmmss #1");

            h.AddOrUpdateDnsRecord(rrName, rrValue);
        }

        [TestMethod]
        public void TestUpdateDnsRecord()
        {
            var h = GetHelper();
            var rrName = "acmesharp-test." + GetParams()["DomainName"];
            var rrValue = "testrr-" + DateTime.Now.ToString("yyyyMMddHHmmss #2");

            h.AddOrUpdateDnsRecord(rrName, rrValue);
        }

        [TestMethod]
        public void TestDeleteDnsRecord()
        {
            var h = GetHelper();
            var rrName = "acmesharp-test." + GetParams()["DomainName"];

            h.DeleteDnsRecord(rrName);
        }
    }
}
