using Common.Services;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Contracts;
using Nethereum.Web3;
using System.Numerics;

namespace ContractIndexer.Services;
public class SmartContractService : Singleton
{
    [Inject]
    private readonly Web3 _ethereumRpc = null!;

    public async Task<ulong> GetPeakBlockHeightAsync() 
        => (ulong) (await _ethereumRpc.Eth.Blocks.GetBlockNumber.SendRequestAsync()).ToLong();

    public async Task<T> DecodeContractCallAsync<T>(string transactionHash)
        where T : FunctionMessage, new()
    {

        var receipt = await _ethereumRpc.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
        return receipt.DecodeTransactionToFunctionMessage<T>();
    }

    public async Task<TResult> CallAsync<TFunction, TResult>(string contractAddress, TFunction? message = null)
        where TFunction : FunctionMessage, new()
    {
        var handler = _ethereumRpc.Eth.GetContractQueryHandler<TFunction>();
        return await handler.QueryAsync<TResult>(contractAddress, message!);
    }
}
