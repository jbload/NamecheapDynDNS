using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NamecheapDynDNS
{
    public class NamecheapDynDNSClient
    {
        private static readonly string GetIPUrl;
        private static readonly string UpdateIPUrl;

        static NamecheapDynDNSClient()
        {
            GetIPUrl = ConfigurationManager.AppSettings["Url:GetIP"];
            UpdateIPUrl = ConfigurationManager.AppSettings["Url:UpdateIP"];
        }
        
        private string LastUpdatedIPAddress { get; set; }

        public async Task<bool> UpdateIPIfChanged(IEnumerable<NamecheapDomain> domains)
        {
            bool updated = false;

            using(var client = new HttpClient())
            {
                var ip = await GetIPAsync(client);

                if(ip != LastUpdatedIPAddress)
                {
                    await UpdateIPAsync(client, ip, domains);
                    updated = true;
                }
            }

            return updated;
        }

        public async Task<string> GetIPAsync()
        {
            using(var client = new HttpClient())
            {
                return await GetIPAsync(client);
            }
        }

        private async Task<string> GetIPAsync(HttpClient client)
        {
            var response = await client.GetAsync(GetIPUrl);
            ThrowIfErrorStatusCode(GetIPUrl, response);

            return await GetValidIPAddress(response);
        }

        private async Task<string> GetValidIPAddress(HttpResponseMessage response)
        {
            var ip = await response.Content.ReadAsStringAsync();

            if(!IPAddress.TryParse(ip, out IPAddress ipAddress))
            {
                throw new Exception($"Request to {GetIPUrl} return invalid IP: {ip}");
            }

            return ip;
        }

        public async Task UpdateIPAsync(string ip, IEnumerable<NamecheapDomain> domains)
        {
            using(var client = new HttpClient())
            {
                await UpdateIPAsync(client, ip, domains);
            }
        }

        private async Task UpdateIPAsync(HttpClient client, string ip, IEnumerable<NamecheapDomain> domains)
        {
            var failures = new SortedSet<string>();

            foreach(var domain in domains)
            {
                foreach(var host in domain.Hosts)
                {
                    var url = UpdateIPUrl
                        + $"?host={host}&domain={domain.DomainName}&password={domain.DynamicDNSPassword}&ip{ip}";
                    var response = await client.GetAsync(url);

                    if(!response.IsSuccessStatusCode)
                    {
                        var failure = $"{host}.{domain.DomainName} ({response.StatusCode} {response.ReasonPhrase})";
                        failures.Add(failure);
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if(TryAreErrorsInResponseXml(content, out string errorMessage))
                        {
                            var failure = $"{host}.{domain.DomainName} ({errorMessage})";
                            failures.Add(failure);
                        }
                    }
                }
            }

            if(failures.Count > 0)
            {
                var message = $"Request to {UpdateIPUrl} failed for: {string.Join(", ", failures)}";
                throw new Exception(message);
            }
            else
            {
                LastUpdatedIPAddress = ip;
            }
        }

        private bool TryAreErrorsInResponseXml(string content, out string errorMessage)
        {
            errorMessage = null;

            var errors = new List<string>();
            var doc = new XmlDocument();
            doc.LoadXml(content);

            var errorNodes = doc.SelectSingleNode("/interface-response/errors");

            if(errorNodes != null)
            {
                foreach(XmlNode errorNode in errorNodes.ChildNodes)
                {
                    errors.Add(errorNode.InnerText);
                }

                if(errors.Count > 0)
                {
                    errorMessage = string.Join(", ", errors);
                }
            }

            return errors.Count > 0;
        }

        private static void ThrowIfErrorStatusCode(string requestedUrl, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var status = $"{response.StatusCode} {response.ReasonPhrase}";
                throw new Exception($"Request to {requestedUrl} returned status: {status}");
            }
        }
    }
}
