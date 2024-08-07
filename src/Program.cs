using Common.Extensions;
using ContractIndexer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nethereum.Web3;
using pocketbase_csharp_sdk;
using System.Reflection;

public class Program
{
    public static readonly Assembly AppAssembly = Assembly.GetExecutingAssembly();
    private static IHost _host = null!;

    public static async Task Main(string[] args)
    {
        _host = CreateHost(args);

        // ValidateOptions();

        await _host.Services.InitializeApplicationAsync(AppAssembly);

        await _host.StartAsync();

        _host.Services.RunApplication(AppAssembly);

        await _host.WaitForShutdownAsync();
    }

    private static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

    // Add these lines to explicitly load the configuration file
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        ConfigureServices(builder.Services, builder.Configuration);

        return builder.Build();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication(configuration, AppAssembly);

        services.AddSingleton(provider =>
        {
            var rpcOptions = provider.GetRequiredService<EthereumRPCOptions>();
            return new Web3(url: rpcOptions.ProviderURL);
        });

        services.AddSingleton(provider =>
        {
            var pocketBaseOptions = provider.GetRequiredService<PocketBaseOptions>();
            return new PocketBase(pocketBaseOptions.BaseUrl);
        });
    }

    private static void ValidateOptions() 
        => _ = AppAssembly.GetApplicationOptionTypes()
            .Select(_host.Services.GetRequiredService)
            .ToArray();
}
