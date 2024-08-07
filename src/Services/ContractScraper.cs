using Common.Services;
using ContractIndexer.Configuration;
using ContractIndexer.Models;
using ContractIndexer.Services.Contracts;
using Microsoft.Extensions.Logging;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;
using System.Text;

namespace ContractIndexer.Services;
public class ContractScraper : Singleton
{
    [Inject]
    private readonly Web3 _ethereumRpc = null!;
    [Inject]
    private readonly PbDbContext _dbContext = null!;
    [Inject]
    private readonly SmartContractService _smartContractService = null!;

    [Inject]
    private readonly ContractTargetOptions _targetOptions = null!;
    [Inject]
    private readonly EthereumRPCOptions _ethereumRPCOptions = null!;

    private readonly PeriodicTimer _updateTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

    protected override ValueTask InitializeAsync()
    {
        _updateTimer.Period = TimeSpan.FromMilliseconds(_targetOptions.RefreshIntervalMs);
        return ValueTask.CompletedTask;
    }

    protected override async ValueTask RunAsync()
    {
        while(await _updateTimer.WaitForNextTickAsync())
        {
            try
            {
                await UpdateContractStateAsync();
            }
            catch(Exception ex)
            {
                Logger.LogCritical(ex, "An exception occured while scraping contract.");
            }
        }
    }

    private async Task UpdateContractStateAsync()
    {
        Logger.LogDebug("Refreshing contract state...");

        ulong peakBlockHeight = await _smartContractService.GetPeakBlockHeightAsync();
        var lastBlockHeightStatus = await _dbContext.GetStatusItemAsync("last_block_height");
        ulong lastUpdatedHeight = lastBlockHeightStatus.Value;

        if(peakBlockHeight - lastBlockHeightStatus.Value < _targetOptions.FinalizationBlocks + 1)
        {
            Logger.LogDebug("Skipping, no new finalized blocks found");
            return;
        }

        var configuredEvent = _ethereumRpc.Eth.GetEvent<SocialSharesContract.TradeEventDTO>(_targetOptions.ContractAddress);

        ulong maxBlock = Math.Min(
                lastUpdatedHeight + _ethereumRPCOptions.MaxBlocksPerCall,
                peakBlockHeight - _targetOptions.FinalizationBlocks);

        if (lastUpdatedHeight + 1 >= maxBlock)
        {
            return;
        }

        Logger.LogInformation("Scanning blocks {startBlock} to {endBlock}", lastUpdatedHeight + 1, maxBlock);

        var filter = configuredEvent.CreateFilterInput(
            fromBlock: new BlockParameter(new HexBigInteger(new BigInteger(lastUpdatedHeight + 1))),
            toBlock: new BlockParameter(new HexBigInteger(maxBlock)));
        var logs = await configuredEvent.GetAllChangesAsync(filter);

        if(logs.Count > 0)
        {
            Logger.LogInformation("Found {eventCount} new events. Processing...", logs.Count);
        }

        foreach(var log in logs.OrderBy(x => x.Log.BlockNumber.Value))
        {
            await ProcessEventLogAsync(log);
        }

        lastBlockHeightStatus.Value = maxBlock;
        await _dbContext.UpdateStatusItemAsync(lastBlockHeightStatus);
    }

    private async Task ProcessEventLogAsync(EventLog<SocialSharesContract.TradeEventDTO> eventLog)
    {
        var block = await _ethereumRpc.Eth.Blocks.GetBlockWithTransactionsByHash.SendRequestAsync(eventLog.Log.BlockHash);

        var trade = new Trade()
        {
            TxHash = eventLog.Log.TransactionHash,
            EventIndex = eventLog.Log.LogIndex.ToUlong(),
            EpochCreated = DateTimeOffset.FromUnixTimeSeconds(block.Timestamp.ToLong()),
            Trader = eventLog.Event.Trader,
            Subject = eventLog.Event.Subject,
            PoolIndex = (ulong) eventLog.Event.PoolIndex,
            IsBuy = eventLog.Event.IsBuy,
            KeyAmount = (ulong) eventLog.Event.KeyAmount,
            EthAmount = (ulong) eventLog.Event.EthAmount,
            ProtocolEthAmount = (ulong) eventLog.Event.ProtocolEthAmount,
            SubjectEthAmount = (ulong) eventLog.Event.SubjectEthAmount,
            Supply = (ulong) eventLog.Event.Supply,
        };

        var insertResult = await _dbContext.Trades.CreateAsync(trade);

        if(insertResult.IsSuccess)
        {
            return;
        }

        var errorSb = new StringBuilder();

        foreach(var reason in insertResult.Reasons)
        {
            _ = errorSb.AppendLine(reason.ToString());
        }

        Logger.LogCritical("Inserting Trade failed! {reason}", errorSb);
    }
}