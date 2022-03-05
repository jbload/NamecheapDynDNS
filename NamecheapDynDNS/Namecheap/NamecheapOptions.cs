using System.Diagnostics;

namespace NamecheapDynDNS.Namecheap;

[DebuggerDisplay("{" + nameof(BaseAddress) + "}")]
public class NamecheapOptions
{
	public string BaseAddress { get; set; } = null!;

	public ICollection<NamecheapDomain> Domains { get; set; } = null!;
}
