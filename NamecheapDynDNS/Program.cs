using System.Text.Json;
using Microsoft.Extensions.Options;
using NamecheapDynDNS;
using NamecheapDynDNS.Namecheap;

using IHost host = Host.CreateDefaultBuilder(args)
	.UseWindowsService(options =>
	{
		options.ServiceName = "Namecheap Dynamic DNS Client";
	})
	.ConfigureServices((context, services) =>
	{
		var settingsFile = Path.Combine(context.HostingEnvironment.ContentRootPath, "namecheap-settings.json");
		var namecheapOptionsJson = File.ReadAllText(settingsFile);
		var namecheapOptions = JsonSerializer.Deserialize<NamecheapOptions>(namecheapOptionsJson);

		services.AddSingleton(Options.Create(namecheapOptions!));
		services.AddHttpClient<NamecheapClient>(
			(serviceProvider, client) =>
			{
				var options = serviceProvider.GetRequiredService<IOptions<NamecheapOptions>>();
				client.BaseAddress = new Uri(options.Value.BaseAddress);
			});
		services.AddSingleton<NamecheapService>();
		services.AddHostedService<WindowsBackgroundService>();
	})
	.Build();

await host.RunAsync();
