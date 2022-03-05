using System.Diagnostics;

namespace NamecheapDynDNS.Namecheap;

[DebuggerDisplay("{" + nameof(DomainName) + "}")]
public class NamecheapDomain
{
	public string DomainName { get; set; } = null!;

	public string Password { get; set; } = null!;

	public ISet<string> Hosts { get; set; } = null!;
}