using NamecheapDynDNS.Namecheap;

namespace NamecheapDynDNS;

public class WindowsBackgroundService : BackgroundService
{
	public WindowsBackgroundService(NamecheapService namecheapService)
	{
		Service = namecheapService;
	}

	private NamecheapService Service { get; }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var interval = TimeSpan.FromMinutes(15);

		while(!stoppingToken.IsCancellationRequested)
		{
			await Service.UpdateDomainsAsync();
			await Task.Delay(interval, stoppingToken);
		}
	}
}