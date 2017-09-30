using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NamecheapDynDNS.Tests
{
    public class NamecheapDNSClientTests
    {
        public NamecheapDNSClientTests()
        {
            Client = new NamecheapDynDNSClient();
        }

        private NamecheapDynDNSClient Client { get; }

        [Fact]
        public async Task GetIPAsync_ReturnsValidIPAddress()
        {
            var ip = await Client.GetIPAsync();
            Assert.NotNull(ip);
            Assert.True(IPAddress.TryParse(ip, out IPAddress ipAddress));
        }

        [Fact]
        public async Task UpdateIPIfChanged_UpdatesTheIPTheFirstTime()
        {
            var domains = NamecheapDomain.FromJsonFile("domain-test-config.json");
            var updated = await Client.UpdateIPIfChanged(domains);
            Assert.True(updated);
        }

        [Fact]
        public async Task UpdateIPIfChanged_OnlyUpdatesTheIPTheFirstTime()
        {
            var domains = NamecheapDomain.FromJsonFile("domain-test-config.json");
            var updatedFirstTry = await Client.UpdateIPIfChanged(domains);
            var updatedSecondTry = await Client.UpdateIPIfChanged(domains);
            Assert.True(updatedFirstTry);
            Assert.False(updatedSecondTry);
        }
    }
}
