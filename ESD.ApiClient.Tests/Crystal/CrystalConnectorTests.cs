using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ESD.ApiClient.Boxer;
using ESD.ApiClient.Crystal;
using ESD.ApiClient.Crystal.Base;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;
using Xunit;

namespace ESD.ApiClient.Tests.Crystal;

public class CrystalConnectorTests: IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly ICrystalConnector crystalConnector;

    public CrystalConnectorTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var boxerConnector = new BoxerConnector(new Uri("https://boxer.example.com"), 
            "provider", mockServiceFixture.BoxerMockHttpClient);
        this.crystalConnector = new CrystalConnector(new Uri("https://crystal.example.com"), ApiVersions.v1_2,
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
            () => Task.FromResult(string.Empty),
            CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("1", result!.RequestId);
    }

}