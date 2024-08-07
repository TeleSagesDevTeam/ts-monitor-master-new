using Common.Configuration;
using System.ComponentModel.DataAnnotations;

namespace ContractIndexer.Configuration;
public class EthereumRPCOptions : Option
{
    public required string ProviderURL { get; set; }
    public required ulong MaxBlocksPerCall { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(ProviderURL, UriKind.Absolute, out var url) || url.Scheme != "https")
        {
            yield return new ValidationResult("Invalid EthereumRPC ProviderURL. Must be an https url.");
        }
        if (MaxBlocksPerCall < 100)
        {
            yield return new ValidationResult("MaxBlocksPerCall too low. Must be at least 100.");
        }
    }
}
