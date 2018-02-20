using Ovh.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ACMESharp.Providers.OVH
{
    /// <summary>
    /// Helper class to interface with the OVH API endpoint.
    /// </summary>
    /// <remarks>
    /// See <see cref="https://api.ovh.com/"/>
    /// for more details.
    /// </remarks>
    public class OvhHelper
    {
        private readonly Client _client;

        private string _zone;

        private string _subDomain;

        public OvhHelper(string endpoint, string applicationKey, string applicationSecret, string consumerKey)
        {
            _client = new Client(endpoint, applicationKey, applicationSecret, consumerKey);
        }

        private void SetZoneAndSubDomain(string recordName)
        {
            var parts = recordName.Split('.');
            _zone = string.Join(".", parts.Skip(parts.Length - 2));
            _subDomain = string.Join(".", parts.Take(parts.Length - 2));
        }

        public void AddOrUpdateDnsRecord(string recordName, string value)
        {
            SetZoneAndSubDomain(recordName);

            var recordsId = GetRecordsId();
            if (recordsId.Any())
            {
                UpdateRecords(recordsId, value);
            }
            else
            {
                AddRecords(value);
            }

            Refresh();

            Thread.Sleep(10000);
        }

        public void DeleteDnsRecord(string recordName)
        {
            SetZoneAndSubDomain(recordName);

            var recordsId = GetRecordsId();
            if (recordsId.Any())
            {
                DeleteRecords(recordsId);
            }
        }

        private long[] GetRecordsId()
        {
            return _client.Get<long[]>($"/domain/zone/{_zone}/record?fieldType=TXT&subDomain={_subDomain}");
        }

        private void Refresh()
        {
            _client.Post($"/domain/zone/{_zone}/refresh", null);
        }

        private void AddRecords(string value)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                { "fieldType", "TXT" },
                { "subDomain", _subDomain },
                { "ttl", 60 },
                { "target", value }
            };

            _client.Post($"/domain/zone/{_zone}/record", payload);
        }

        private void UpdateRecords(long[] recordsId, string value)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                { "target", value }
            };

            foreach (var id in recordsId)
            {
                _client.Put($"/domain/zone/{_zone}/record/{id}", payload);
            }
        }

        private void DeleteRecords(long[] recordsId)
        {
            foreach (var id in recordsId)
            {
                var res = _client.Delete($"/domain/zone/{_zone}/record/{id}");
            }
        }

    }
}
