using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using ESD.ApiClient.Config;
using ESD.ApiClient.Crystal.Base;
using ESD.ApiClient.Crystal.Models.Base;
using ESD.ApiClient.Extensions;
using ESD.ApiClient.Tests.Acceptance.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ESD.ApiClient.Tests.Acceptance.Crystal;


public class AcceptanceTests
{
    private readonly AcceptanceTestsConfiguration configuration = new();
    private readonly ServiceProvider services;

    public AcceptanceTests()
    {

        var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
        configurationRoot.GetSection(nameof(AcceptanceTestsConfiguration)).Bind(this.configuration);

        this.services = new ServiceCollection()
            .Configure<BoxerConnectorOptions>(configurationRoot.GetSection(nameof(BoxerConnectorOptions)))
            .AddSingleton<HttpClient>()
            .AddLogging(conf => conf.AddConsole())
            .AddBoxerAuthorization(async cancellationToken =>
            {
                var credential = new DefaultAzureCredential();
                var context = new TokenRequestContext(scopes: new[] { "https://management.core.windows.net/.default" });
                var tokenResponse = await credential.GetTokenAsync(context, cancellationToken);
                return tokenResponse.Token;
            })
            .Configure<CrystalConnectorOptions>(configurationRoot.GetSection(nameof(CrystalConnectorOptions)))
            .AddCrystalConnector()
            .BuildServiceProvider();
    }
    
    [Fact]
    public async Task TestCanRunAlgorithm()
    {
        var crystalConnector = this.services.GetRequiredService<ICrystalConnector>();
        var response = await crystalConnector.CreateRunAsync(
            configuration.AlgorithmName,
            JsonDocument.Parse(configuration.AlgorithmPayload).RootElement,
            configuration.AlgorithmConfiguration,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.NotNull(response!.RequestId);
        
        RunResult? runResult;
        do
        {
            await Task.Delay(5000);
            runResult = await crystalConnector.QueryResultAsync(configuration.AlgorithmName, response!.RequestId);
        } while (runResult != null && runResult.Status != RequestLifeCycleStage.COMPLETED);
    }
}