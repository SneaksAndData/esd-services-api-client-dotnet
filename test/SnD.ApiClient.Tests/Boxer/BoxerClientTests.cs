using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using Xunit;

namespace SnD.ApiClient.Tests.Boxer;

public class BoxerClientTests: IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly IBoxerClient boxerClient;

    public BoxerClientTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var crystalOptions = new BoxerClientOptions
            { BaseUri = "https://boxer.example.com" };
        
        this.boxerClient = new BoxerClient(Options.Create(crystalOptions),
            mockServiceFixture.BoxerMockHttpClient,
            CreateBoxerClient(mockServiceFixture),
            loggerFixture.Factory.CreateLogger<BoxerClient>());
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
    
    // TODO: Add tests here

}
