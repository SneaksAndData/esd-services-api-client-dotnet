using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SnD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal.Base;
using SnD.ApiClient.Extensions;
using SnD.ApiClient.Azure;
using SnD.ApiClient.Tests.Acceptance.Config;
using Xunit;

namespace SnD.ApiClient.Tests.Acceptance.Crystal;


public class AcceptanceTests
{
    private readonly AcceptanceTestsConfiguration configuration = new();
    private readonly ServiceProvider services;

    public AcceptanceTests()
    {

        var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
        configurationRoot.GetSection(nameof(AcceptanceTestsConfiguration)).Bind(this.configuration);

        this.services = new ServiceCollection()
            .Configure<BoxerTokenProviderOptions>(configurationRoot.GetSection(nameof(BoxerTokenProviderOptions)))
            .AddSingleton<HttpClient>()
            .AddLogging(conf => conf.AddConsole())
            .AuthorizeWithBoxerOnAzure()
            .Configure<CrystalClientOptions>(configurationRoot.GetSection(nameof(CrystalClientOptions)))
            .AddCrystalClient()
            .BuildServiceProvider();
    }
    
    [SkippableFact]
    public async Task TestCanRunAlgorithm()
    {
        Skip.If(string.IsNullOrEmpty(configuration.AlgorithmName) || configuration.AlgorithmPayload == null, "Algorithm payload and/or name is empty.");
        
        var crystalConnector = this.services.GetRequiredService<ICrystalClient>();
        var response = await crystalConnector.CreateRunAsync(
            configuration.AlgorithmName,
            JsonDocument.Parse(configuration.AlgorithmPayload).RootElement,
            configuration.AlgorithmConfiguration,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.NotNull(response!.RequestId);
        
        RunResult runResult;
        do
        {
            await Task.Delay(5000);
            runResult = await crystalConnector.GetResultAsync(configuration.AlgorithmName, response!.RequestId);
        } while (runResult != null && runResult.Status != RequestLifeCycleStage.COMPLETED);
    }
}
