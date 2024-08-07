using Common.Configuration;
using Nethereum.Util;
using System.ComponentModel.DataAnnotations;

namespace ContractIndexer.Configuration;
public class ContractTargetOptions : Option
{
    public required string ContractAddress { get; set; }
    public required int RefreshIntervalMs { get; set; }
    public required ulong FinalizationBlocks { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!AddressUtil.Current.IsValidEthereumAddressHexFormat(ContractAddress))
        {
            yield return new ValidationResult("Invalid target contractAddress");
        }
        if (RefreshIntervalMs < 2500)
        {
            yield return new ValidationResult("RefreshInterval too short. Must be more than 2500ms!");
        }
    }

}
