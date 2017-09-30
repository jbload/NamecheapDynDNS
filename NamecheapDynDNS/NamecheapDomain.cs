using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NamecheapDynDNS
{
    public class NamecheapDomain
    {
        public string DomainName { get; set; }

        public string DynamicDNSPassword { get; set; }

        public ISet<string> Hosts { get; set; }

        public static IList<NamecheapDomain> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return FromJson(json);
        }

        public static IList<NamecheapDomain> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<IList<NamecheapDomain>>(json);
        }
    }
}
