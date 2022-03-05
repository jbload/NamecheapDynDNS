using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml;

namespace NamecheapDynDNS.Namecheap;

public class NamecheapClient
{
	public NamecheapClient(HttpClient httpClient, ILogger<NamecheapClient> logger)
	{
		Client = httpClient;
		Logger = logger;
	}

	private HttpClient Client { get; }

	private ILogger Logger { get; }

	private string? PreviousIPAddress { get; set; }

	public async Task<bool> UpdateIPIfChangedAsync(IEnumerable<NamecheapDomain> domains)
	{
		var updated = false;

		var ip = await GetIPAsync();

		if(ip != PreviousIPAddress)
		{
			await UpdateIPAsync(ip, domains);
			updated = true;
		}

		return updated;
	}

	private async Task<string> GetIPAsync()
	{
		var response = await Client.GetAsync("getip");
		response.EnsureSuccessStatusCode();

		return await GetValidIPAddressAsync(response);
	}

	private async Task<string> GetValidIPAddressAsync(HttpResponseMessage response)
	{
		var ip = await response.Content.ReadAsStringAsync();

		if(!IPAddress.TryParse(ip, out _))
		{
			throw new Exception($"Request to {nameof(GetIPAsync)} returned invalid IP: {ip}");
		}

		return ip;
	}

	private async Task UpdateIPAsync(string ip, IEnumerable<NamecheapDomain> domains)
	{
		var failures = new SortedSet<string>();

		foreach(var domain in domains)
		{
			foreach(var host in domain.Hosts)
			{
				var url = GetUpdateIPUrl(domain, ip, host);
				var response = await Client.GetAsync(url);

				if(response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();

					if(TryAreErrorsInResponseXml(content, out var errorMessage))
					{
						var failure = $"{host}.{domain.DomainName} ({errorMessage})";
						failures.Add(failure);
					}
					else
					{
						Logger.LogInformation(
							"Updated IP address of {host}.{domain} to {ip}.",
							host,
							domain.DomainName,
							ip);
					}
				}
				else
				{
					var failure = $"{host}.{domain.DomainName} ({response.StatusCode} {response.ReasonPhrase})";
					failures.Add(failure);
				}
			}
		}

		if(failures.Count > 0)
		{
			var message = $"Request to {nameof(UpdateIPAsync)} failed for: {string.Join(", ", failures)}";
			throw new Exception(message);
		}
		else
		{
			PreviousIPAddress = ip;
		}
	}

	private static string GetUpdateIPUrl(NamecheapDomain domain, string ip, string host)
	{
		return $"update?host={host}&domain={domain.DomainName}&password={domain.Password}&ip{ip}";
	}

	private static bool TryAreErrorsInResponseXml(string content, [NotNullWhen(true)] out string? errorMessage)
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
}