using pocketbase_csharp_sdk.Models;
using System.Numerics;
using System.Text.Json.Serialization;

namespace ContractIndexer.Models;
public class StatusItem : BaseModel
{
    public required string Type { get; set; }
    public required ulong Value { get; set; }
}
