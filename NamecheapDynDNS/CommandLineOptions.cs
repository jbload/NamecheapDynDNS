using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace NamecheapDynDNS
{
    public class CommandLineOptions
    {
        public const char DomainConfigFileShortName = 'c';
        public const string DomainConfigFileLongName = "config";
        public const string DomainConfigFileDefault = "domain-config.json";
        public const char UpdateIntervalShortName = 'u';
        public const string UpdateIntervalLongName = "update";
        public const string UpdateIntervalDefault = "15";

        [Option(DomainConfigFileShortName, DomainConfigFileLongName, DefaultValue = DomainConfigFileDefault)]
        public string DomainConfigFile { get; set; }

        [Option(UpdateIntervalShortName, UpdateIntervalLongName, DefaultValue = UpdateIntervalDefault)]
        public string UpdateInterval { get; set; }
    }
}
