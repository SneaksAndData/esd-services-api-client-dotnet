using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ESD.ApiClient.Boxer;
using ESD.ApiClient.Crystal;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;
using Xunit;

namespace ESD.ApiClient.Tests.Crystal;

public class CrystalConnectorTests: IClassFixture<MockServiceFixture>
{
    private readonly ICrystalConnector crystalConnector;

    public CrystalConnectorTests(MockServiceFixture mockServiceFixture)
    {
        var boxerConnector = new BoxerConnector(new Uri("https://boxer.example.com"), "provider",
            mockServiceFixture.BoxerMockHttpClient);
        this.crystalConnector = new CrystalConnector(new Uri("https://crystal.example.com"), ApiVersions.Api12,
            mockServiceFixture.CrystalMockHttpClient, boxerConnector);
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