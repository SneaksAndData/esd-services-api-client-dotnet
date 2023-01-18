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

public class CrystalClientTests: IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly ICrystalConnector crystalConnector;

    public CrystalClientTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var crystalOptions = new CrystalClientOptions
            { BaseUri = "https://crystal.example.com", ApiVersion = ApiVersions.v1_2 };
        
        this.crystalConnector = new CrystalClient(Options.Create(crystalOptions),
            mockServiceFixture.CrystalMockHttpClient,
            CreateBoxerClient(mockServiceFixture),
            loggerFixture.Factory.CreateLogger<CrystalClient>());
    }

    private static BoxerTokenProvider CreateBoxerClient(MockServiceFixture mockServiceFixture)
    {
        var boxerOptions = new BoxerTokenProviderOptions
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
        Assert.Equal("00000000-0000-0000-0000-000000000000", result!.RequestId);
    }

}