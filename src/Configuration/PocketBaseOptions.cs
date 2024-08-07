using Common.Configuration;
using System.ComponentModel.DataAnnotations;

namespace ContractIndexer.Configuration;
public class PocketBaseOptions : Option
{
    public required string BaseUrl { get; set; }
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    [MinLength(2)]
    public required string Password { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var url) || (url.Scheme != "http" && url.Scheme != "https"))
        {
            yield return new ValidationResult("Invalid Pocketbase BaseUrl. Must be an http or https url.");
        }
    }
}
