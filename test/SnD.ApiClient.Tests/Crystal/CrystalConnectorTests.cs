using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SnD.ApiClient.Crystal.Models;
using SnD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal;
using SnD.ApiClient.Crystal.Base;
using Xunit;

namespace SnD.ApiClient.Tests.Crystal;

public class CrystalConnectorTests: IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly ICrystalConnector crystalConnector;

    public CrystalConnectorTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var crystalOptions = new CrystalConnectorOptions
            { BaseUri = "https://crystal.example.com", ApiVersion = ApiVersions.v1_2 };
        
        this.crystalConnector = new CrystalConnector(Options.Create(crystalOptions),
            mockServiceFixture.CrystalMockHttpClient,
            CreateBoxerConnector(mockServiceFixture),
            loggerFixture.Factory.CreateLogger<CrystalConnector>());
    }

    private static BoxerTokenProvider CreateBoxerConnector(MockServiceFixture mockServiceFixture)
    {
        var boxerOptions = new BoxerConnectorOptions
            { IdentityProvider = "example.com", BaseUri = "https://boxer.example.com" };
        var boxerConnector = new BoxerTokenProvider(
            Options.Create(boxerOptions),
            mockServiceFixture.BoxerMockHttpClient,
            Mock.Of<ILogger<BoxerTokenProvider>>(),
            _ => Task.FromResult(string.Empty));
        return boxerConnector;
    }

    [Fact]
    public async Task TestShouldRenewTokenAsync()
    {
        var payload = JsonDocument.Parse("null").RootElement;
        var result = await crystalConnector.CreateRunAsync(
            "algorithm",
            payload,
            new AlgorithmConfiguration(),
            CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("1", result!.RequestId);
    }

}