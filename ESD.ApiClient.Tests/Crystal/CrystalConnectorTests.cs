using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ESD.ApiClient.Boxer;
using ESD.ApiClient.Config;
using ESD.ApiClient.Crystal;
using ESD.ApiClient.Crystal.Base;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ESD.ApiClient.Tests.Crystal;

public class CrystalConnectorTests: IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly ICrystalConnector crystalConnector;

    public CrystalConnectorTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var boxerConnector = new BoxerConnector(
            Options.Create(new BoxerConnectorOptions
            {
                AuthorizationProvider = "example.com",
                BaseUri = "https://boxer.example.com"
            }),
            mockServiceFixture.BoxerMockHttpClient,
            Mock.Of<ILogger<BoxerConnector>>(),
            _ => Task.FromResult(string.Empty));
        this.crystalConnector = new CrystalConnector(Options.Create(new CrystalConnectorOptions
            {
                BaseUri = "https://crystal.example.com",
                ApiVersion = ApiVersions.v1_2
                
            }),
            mockServiceFixture.CrystalMockHttpClient, boxerConnector,
            loggerFixture.Factory.CreateLogger(nameof(CrystalConnectorTests)));
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