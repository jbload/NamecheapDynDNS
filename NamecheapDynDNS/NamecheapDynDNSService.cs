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
using Newtonsoft.Json;

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

        private string CurrentIPAddress { get; set; }

        private bool IsUpdating { get; set; }

        protected override async void OnStart(string[] args)
        {
            var options = new CommandLineOptions();
            Parser.Default.ParseArguments(args, options);

            Timer.Interval = GetUpdateInterval(options);
            Domains = GetDomains(options);

            await UpdateIPAsync();
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

            if(string.IsNullOrWhiteSpace(options.DomainConfigFile))
            {
                var message = string.Format(
                    "A domain config file was not specified using the -{0} or -{1} options.",
                    CommandLineOptions.DomainConfigFileShortName,
                    CommandLineOptions.DomainConfigFileLongName);
                EventLog.WriteEntry(message, EventLogEntryType.Error);
            }
            else
            {
                var configFilePath = Path.GetFullPath(options.DomainConfigFile);

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
            }

            return domains ?? new List<NamecheapDomain>();
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
                await DynDNSClient.UpdateIPIfChanged(Domains);
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}
