using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CommandLine;

namespace NamecheapDynDNS
{
    public partial class NamecheapDynDNSService : ServiceBase
    {
        public NamecheapDynDNSService()
        {
            InitializeComponent();
            DynDNSClient = new NamecheapDynDNSClient();
            Timer = new Timer()
            {
                AutoReset = true,
                Enabled = false
            };
            Timer.Elapsed += async (sender, args) => await UpdateIPAsync();
        }

        private NamecheapDynDNSClient DynDNSClient { get; }

        private Timer Timer { get; }
        
        private IList<NamecheapDomain> Domains { get; set; }
        
        private bool IsUpdating { get; set; }

        private DateTime? LastLogTimeOfUpdatesWithoutIPChange { get; set; }

        private int UnloggedUpdatesWithoutIPChange { get; set; }

        protected override void OnStart(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(
                    async options =>
                    {
                        Timer.Interval = GetUpdateInterval(options);
                        Domains = GetDomains(options);

                        await UpdateIPAsync();
                    });
        }

        protected override void OnStop()
        {
        }
        
        private int GetUpdateInterval(CommandLineOptions options)
        {
            if(!Int32.TryParse(options.UpdateInterval, out int intervalInMinutes))
            {
                intervalInMinutes = 15;
            }

            var intervalInMilliseconds = intervalInMinutes * 1000 * 60;

            return intervalInMilliseconds;
        }

        private IList<NamecheapDomain> GetDomains(CommandLineOptions options)
        {
            IList<NamecheapDomain> domains = null;
            
            var configFilePath = GetFullPath(options.DomainConfigFile);

            if(File.Exists(configFilePath))
            {
                try
                {
                    domains = NamecheapDomain.FromJsonFile(configFilePath);
                }
                catch(Exception ex)
                {
                    EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                }
            }
            else
            {
                EventLog.WriteEntry($"Domain config file does not exist: {configFilePath}");
            }

            return domains ?? new List<NamecheapDomain>();
        }

        private string GetFullPath(string path)
        {
            string fullPath;

            if(Path.IsPathRooted(path))
            {
                fullPath = Path.GetFullPath(path);
            }
            else
            {
                fullPath = Path.Combine(AppContext.BaseDirectory, path);
            }

            return fullPath;
        }
        private async Task UpdateIPAsync()
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
                var updated = await DynDNSClient.UpdateIPIfChanged(Domains);

                if(updated)
                {
                    UnloggedUpdatesWithoutIPChange = 0;
                    LastLogTimeOfUpdatesWithoutIPChange = null;
                    EventLog.WriteEntry(
                        $"Updated IP Address for {Domains.Count} domain(s).",
                        EventLogEntryType.SuccessAudit);
                }
                else
                {
                    UnloggedUpdatesWithoutIPChange++;

                    var elapsed = DateTime.Now - (LastLogTimeOfUpdatesWithoutIPChange ?? DateTime.Now);

                    if(elapsed.TotalHours >= 1.0)
                    {
                        var message =
                            $"Update was not needed because the IP address has not changed. Attempts: {UnloggedUpdatesWithoutIPChange:N}";
                        EventLog.WriteEntry(message, EventLogEntryType.Information);
                        UnloggedUpdatesWithoutIPChange = 0;
                    }
                }
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}
