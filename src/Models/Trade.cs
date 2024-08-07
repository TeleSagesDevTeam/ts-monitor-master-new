using ContractIndexer.Json;
using pocketbase_csharp_sdk.Models;
using System.Text.Json.Serialization;

namespace ContractIndexer.Models;
public class Trade : BaseModel
{
    public required string TxHash { get; init; }
    public required ulong EventIndex { get; init; }
    [JsonConverter(typeof(DateTimeOffsetConverter))]
    public required DateTimeOffset EpochCreated { get; init; }
    public required string Trader {  get; init; }
    public required string Subject { get; init; }
    public required ulong PoolIndex { get; init; }
    public required bool IsBuy { get; init; }
    public required ulong KeyAmount { get; init; }
    public required ulong EthAmount { get; init; }
    public required ulong ProtocolEthAmount { get; init; }
    public required ulong SubjectEthAmount { get; init; }
    public required ulong Supply {  get; init; }

    public Trade()
    {
    }
}
