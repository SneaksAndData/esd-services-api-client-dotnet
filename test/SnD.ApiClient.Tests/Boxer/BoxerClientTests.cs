using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

public class BoxerClientTests : IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
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


    [Fact]
    public void CreateWithAllMethods()
    {
        // Arrange
        var path = "path";
        var apiMethods = new HashSet<ApiMethodElement>();
        apiMethods.Add(ApiMethodElement.GET);
        apiMethods.Add(ApiMethodElement.PUT);
        apiMethods.Add(ApiMethodElement.POST);
        apiMethods.Add(ApiMethodElement.PATCH);
        apiMethods.Add(ApiMethodElement.DELETE);
        // Act
        var boxerJwtClaim = BoxerJwtClaim.Create(path, apiMethods);
        // Assert
        Assert.Equal(path, boxerJwtClaim.Type);
        Assert.Equal(".*", boxerJwtClaim.Value);
    }

    [Fact]
    public void CreateWithSomeMethods()
    {
        // Arrange
        var path = "path";
        var apiMethods = new HashSet<ApiMethodElement>();
        apiMethods.Add(ApiMethodElement.GET);
        apiMethods.Add(ApiMethodElement.POST);
        // Act
        var boxerJwtClaim = BoxerJwtClaim.Create(path, apiMethods);
        // Assert
        Assert.Equal(path, boxerJwtClaim.Type);
        Assert.Equal("^(GET|POST)$", boxerJwtClaim.Value);
        Assert.True(Regex.Match("POST", boxerJwtClaim.Value, RegexOptions.IgnoreCase).Success);
        Assert.False(Regex.Match("PUT", boxerJwtClaim.Value, RegexOptions.IgnoreCase).Success);
    }

    [Fact]
    public void GetClaimsAsEnums()
    {
        var apiMethods = new HashSet<ApiMethodElement>();
        apiMethods.Add(ApiMethodElement.GET);
        apiMethods.Add(ApiMethodElement.POST);
        var boxerJwtClaim = BoxerJwtClaim.Create("path", apiMethods);
        
        Assert.Contains(ApiMethodElement.GET, boxerJwtClaim.ApiMethods);
        Assert.Contains(ApiMethodElement.POST, boxerJwtClaim.ApiMethods);
        Assert.DoesNotContain(ApiMethodElement.PUT, boxerJwtClaim.ApiMethods);
    }
    
    [Fact]
    public void GetAllClaimsAsEnums()
    {
        var apiMethods = new HashSet<ApiMethodElement>();
        apiMethods.Add(ApiMethodElement.GET);
        apiMethods.Add(ApiMethodElement.PUT);
        apiMethods.Add(ApiMethodElement.POST);
        apiMethods.Add(ApiMethodElement.PATCH);
        apiMethods.Add(ApiMethodElement.DELETE);
        
        var boxerJwtClaim = BoxerJwtClaim.Create("path", apiMethods);
        
        Assert.Contains(ApiMethodElement.GET, boxerJwtClaim.ApiMethods);
        Assert.Contains(ApiMethodElement.PUT, boxerJwtClaim.ApiMethods);
        Assert.Contains(ApiMethodElement.POST, boxerJwtClaim.ApiMethods);
        Assert.Contains(ApiMethodElement.PATCH, boxerJwtClaim.ApiMethods);
        Assert.Contains(ApiMethodElement.DELETE, boxerJwtClaim.ApiMethods);
    }
}
