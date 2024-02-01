using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Boxer.Models;
using SnD.ApiClient.Config;
using Xunit;

namespace SnD.ApiClient.Tests.Boxer;

public class BoxerClaimsClientTests : IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly IBoxerClaimsClient boxerClaimsClient;

    public BoxerClaimsClientTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        var crystalOptions = new BoxerClaimsClientOptions
            { BaseUri = "https://boxer.example.com" };

        boxerClaimsClient = new BoxerClaimsClient(Options.Create(crystalOptions),
            mockServiceFixture.BoxerMockHttpClient,
            CreateBoxerClient(mockServiceFixture),
            loggerFixture.Factory.CreateLogger<BoxerClaimsClient>());
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

    [Theory]
    [InlineData(new[] { ApiMethodElement.GET, ApiMethodElement.POST }, "^(GET|POST)$")]
    [InlineData(new[] { ApiMethodElement.GET, ApiMethodElement.POST, ApiMethodElement.PUT }, "^(GET|POST|PUT)$")]
    [InlineData(new[] { ApiMethodElement.GET, ApiMethodElement.POST, ApiMethodElement.PUT, ApiMethodElement.PATCH },
        "^(GET|POST|PUT|PATCH)$")]
    [InlineData(new[] { ApiMethodElement.GET, ApiMethodElement.POST, ApiMethodElement.PUT, ApiMethodElement.PATCH, ApiMethodElement.DELETE },
        ".*")]
    public void CreateWithSomeMethods(ApiMethodElement[] apiMethods, string expectedValue)
    {
        var boxerJwtClaim = BoxerJwtClaim.Create("path", new HashSet<ApiMethodElement>(apiMethods));
        Assert.Equal("path", boxerJwtClaim.Type);
        Assert.Equal(expectedValue, boxerJwtClaim.Value);
    }
}
