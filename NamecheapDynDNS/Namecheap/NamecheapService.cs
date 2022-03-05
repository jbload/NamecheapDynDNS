using Microsoft.Extensions.Options;

namespace NamecheapDynDNS.Namecheap;

public class NamecheapService
{
	public NamecheapService(
		NamecheapClient namecheapClient, 
		IOptions<NamecheapOptions> options,
		ILogger<NamecheapService> logger)
	{
		NamecheapClient = namecheapClient;
		Domains = options.Value.Domains.ToList();
		Logger = logger;
	}

	private NamecheapClient NamecheapClient { get; }

	private ICollection<NamecheapDomain> Domains { get; }
        
	private ILogger Logger { get; }

	private bool IsUpdating { get; set; }
	
	public async Task UpdateDomainsAsync()
	{
		lock(this)
		{
			if(IsUpdating)
			{
				return;
			}

			IsUpdating = true;
		}

		try
		{
			var updated = await NamecheapClient.UpdateIPIfChangedAsync(Domains);

			if(updated)
			{
				Logger.LogInformation("Updated IP Address for {count} domain(s).", Domains.Count);
			}
			else
			{
				Logger.LogDebug("Update was not necessary because IP address has not changed.");
			}
		}
		catch(Exception ex)
		{
			Logger.LogError(ex, "IP address update failed.");
		}
		finally
		{
			IsUpdating = false;
		}
	}
}