using Common.Services;
using ContractIndexer.Configuration;
using ContractIndexer.Models;
using Microsoft.Extensions.Logging;
using pocketbase_csharp_sdk;
using pocketbase_csharp_sdk.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractIndexer.Services;
public class PbDbContext : Singleton
{
    private const string _statusTableName = "Status";
    private const string _tradesTableName = "Trades";

    public RecordService Trades { get; private set; } = null!;

    [Inject]
    private readonly PocketBase _pocketBase = null!;

    [Inject]
    private readonly PocketBaseOptions _pbOptions = null!;

    protected override async ValueTask InitializeAsync()
    {
        try
        {
            Logger.LogInformation("Logging into PocketBase...");
            var res = await _pocketBase.Admin.AuthenticateWithPasswordAsync(_pbOptions.Email, _pbOptions.Password);

            if (!res.IsSuccess)
            {
                var sb = new StringBuilder();
                foreach(var reason in res.Reasons)
                {
                    sb.AppendLine(reason.Message);
                }
                Logger.LogCritical("Pocketbase Authentication Failed! {reason}", sb);
                throw new InvalidOperationException();
            }

            Logger.LogInformation("Login successful");

            Trades = _pocketBase.Collection(_tradesTableName);
        }
        catch(Exception ex)
        {
            Logger.LogCritical(ex, "Pocketbase Authentication Failed!");
            throw;
        }
    }

    public async Task<StatusItem> GetStatusItemAsync(string itemName, ulong defaultValue = 0)
    {
        var statusItemResult = await _pocketBase.Collection(_statusTableName)
            .GetFullListAsync<StatusItem>(filter: $"""
                type = "{itemName}"
            """);
        var statusItem = statusItemResult.ValueOrDefault.SingleOrDefault();

        if (statusItem is not null)
        {
            return statusItem;
        }

        statusItem = new StatusItem()
        {
            Type = itemName,
            Value = defaultValue
        };

        var createResult = await _pocketBase.Collection(_statusTableName)
            .CreateAsync(statusItem);
        _ = createResult.Value;

        return await GetStatusItemAsync(itemName, defaultValue);
    }

    public async Task UpdateStatusItemAsync(StatusItem value)
    {
        var res = await _pocketBase.Collection(_statusTableName)
            .UpdateAsync(value);

        _ = res.Value;
    }

    public RecordService Collection(string collectionName) 
        => _pocketBase.Collection(collectionName);
}
