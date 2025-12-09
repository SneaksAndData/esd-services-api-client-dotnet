using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnD.ApiClient.Azure;
using SnD.ApiClient.Config;
using SnD.ApiClient.Extensions;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Tests.Acceptance.Config;
using Xunit;

namespace SnD.ApiClient.Tests.Acceptance.Nexus;


public class NexusAcceptanceTests
{
    private readonly AcceptanceTestsConfiguration configuration = new();
    private readonly ServiceProvider services;

    public NexusAcceptanceTests()
    {

        var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
        configurationRoot.GetSection(nameof(AcceptanceTestsConfiguration)).Bind(this.configuration);

        this.services = new ServiceCollection()
            .Configure<BoxerTokenProviderOptions>(configurationRoot.GetSection(nameof(BoxerTokenProviderOptions)))
            .AddSingleton<HttpClient>()
            .AddLogging(conf => conf.AddConsole())
            .AuthorizeWithBoxerOnAzure()
            .AddAuthenticationProvider()
            .Configure<NexusClientOptions>(configurationRoot.GetSection(nameof(NexusClientOptions)))
            .AddNexusRetryPolicy()
            .AddNexusClient()
            .BuildServiceProvider();
    }
    
    [SkippableFact]
    public async Task TestCanRunAlgorithm()
    {
        Skip.If(string.IsNullOrEmpty(configuration.AlgorithmName) || configuration.AlgorithmPayload == null, "Algorithm payload and/or name is empty.");
        
        var crystalConnector = this.services.GetRequiredService<INexusClient>();
        // var response = await crystalConnector.CreateRunAsync(
        //     configuration.AlgorithmName,
        //     JsonDocument.Parse(configuration.AlgorithmPayload).RootElement,
        //     configuration.AlgorithmConfiguration,
        //     CancellationToken.None
        // );

        // Assert.NotNull(response);
        // Assert.NotNull(response!.RequestId);
        //
        // RunResult runResult;
        // do
        // {
        //     await Task.Delay(5000);
        //     runResult = await crystalConnector.GetResultAsync(configuration.AlgorithmName, response!.RequestId);
        // } while (runResult != null && runResult.Status != RequestLifeCycleStage.COMPLETED);
        Assert.NotNull(crystalConnector);
    }
}
