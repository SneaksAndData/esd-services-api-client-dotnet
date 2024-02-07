using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    [InlineData(new[] { "GET", "POST" }, "^(GET|POST)$")]
    [InlineData(new[] { "GET", "POST", "PUT" }, "^(GET|POST|PUT)$")]
    [InlineData(new[] { "GET", "POST", "PUT", "PATCH" }, "^(GET|POST|PUT|PATCH)$")]
    [InlineData(new[] { "GET", "POST", "PUT", "PATCH", "DELETE" }, "^(GET|POST|PUT|PATCH|DELETE)$")]
    public void CreateWithSomeMethods(string[] apiMethods, string expectedValue)
    {
        var boxerJwtClaim = BoxerJwtClaim.Create("path", new HashSet<HttpMethod>(apiMethods.Select(s=>new HttpMethod(s))));
        Assert.Equal("path", boxerJwtClaim.Type);
        Assert.True(apiMethods.All(s => boxerJwtClaim.ApiMethods.Contains(new HttpMethod(s))));
    }

    [Fact]
    public void DeserializationTest()
    {
        var apiResponse = File.ReadAllText("Boxer/ApiSamples/GetUserSample.json");
        var boxerJwtClaims = BoxerJwtClaim.FromBoxerClaimsApiResponse(apiResponse).ToArray();
        Assert.Equal(2, boxerJwtClaims.Length);
        Assert.Equal("myapi1.com/.*", boxerJwtClaims.First().Path);
        Assert.Contains(HttpMethod.Get, boxerJwtClaims.First().ApiMethods);
    }
}
