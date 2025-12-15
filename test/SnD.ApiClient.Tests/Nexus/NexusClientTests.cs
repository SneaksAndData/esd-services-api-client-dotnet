using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using KiotaPosts.Client.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;
using Xunit;

namespace SnD.ApiClient.Tests.Nexus;

public class NexusClientTests : IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly INexusClient nexusClient;
    private readonly Mock<HttpMessageHandler> handlerMock;
    private readonly Mock<IJwtTokenExchangeProvider> tokenProviderMock;

    public NexusClientTests(MockServiceFixture mockServiceFixture, LoggerFixture loggerFixture)
    {
        this.handlerMock = mockServiceFixture.GetMockedHttpClientHandler(m =>
        {
            m.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        });

        this.tokenProviderMock = mockServiceFixture.GetMockedJwtTokenExchangeProvider(m =>
        {
            m.Setup(tp => tp.GetTokenAsync(It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("token");
        });

        var options = Options.Create(new NexusClientOptions
        {
            BaseUri = "http://www.example.com/",
            MaxRetryAttempts = 5,
            RetryIntervalSeconds = 1
        });
        var boxerAuthenticationProvider = new BoxerAuthenticationProvider(this.tokenProviderMock.Object);
        var retryStrategy = new RetryAllErrors(loggerFixture.Factory.CreateLogger<RetryAllErrors>(), options);

        var retryOption = retryStrategy.ToRetryHandlerOption();
        var httpClient =
            KiotaClientFactory.Create(optionsForHandlers: [retryOption], finalHandler: this.handlerMock.Object);
        httpClient.BaseAddress = new Uri(options.Value.BaseUri);
        var httpAdapter = new HttpClientRequestAdapter(boxerAuthenticationProvider, httpClient: httpClient);
        this.nexusClient = new NexusClient(httpAdapter, loggerFixture.Factory.CreateLogger<NexusClient>());
    }

    [InlineData("algorithm", "http://www.example.com/algorithm/v1/run/algorithm?dryRun=False")]
    [Theory]
    public async Task TestCreateAlgorithmAsync(string algorithm, string expectedUrl)
    {
        // Arrange
        this.handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { requestId = "12345" })
            });

        // Act
        await nexusClient.CreateRunAsync(
            NexusAlgorithmRequest.Create(
                "{}",
                null,
                null,
                null,
                null,
                null),
            algorithm,
            null,
            null,
            null,
            null,
            false,
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [InlineData("algorithm", "http://www.example.com/algorithm/v1/results/algorithm/requests/12345")]
    [Theory]
    public async Task TestAwaitRunAsync(string algorithm, string expectedUrl)
    {
        // Arrange
        this.handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { status = "COMPLETED" })
            });

        // Act
        await nexusClient.AwaitRunAsync(
            "12345",
            algorithm,
            TimeSpan.Zero,
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [InlineData("algorithm",
        "http://www.example.com/algorithm/v1/results/tags/tag1",
        "http://www.example.com/algorithm/v1/results/tags/tag2")]
    [Theory]
    public async Task TestAwaitTaggedRunsAsync(string algorithm, string expectedUrl1, string expectedUrl2)
    {
        // Arrange
        this.handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => msg.RequestUri.ToString().Contains("algorithm/v1/results/tags")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new List<TaggedRequestResult>
                    { new() { RequestId = "12345", AlgorithmName = algorithm } })
            })
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new List<TaggedRequestResult>
                    { new() { RequestId = "6789", AlgorithmName = algorithm } })
            })
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                // This response is for testing an unexpected algorithm name scenario
                // Should not try to await this run
                Content = JsonContent.Create(new List<TaggedRequestResult> { new() { RequestId = "6789", AlgorithmName = "UnknownAlgorithm" } })
            });
        
            this.handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => msg.RequestUri.ToString().Contains("results/algorithm/requests/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { status = "COMPLETED" })
            })
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { status = "COMPLETED" })
            });

        // Act
        var result = await nexusClient.AwaitTaggedRunsAsync(
            new List<string>
            {
                "tag1",
                "tag2"
            },
            algorithm,
            TimeSpan.Zero,
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.ToString() == expectedUrl1),
            ItExpr.IsAny<CancellationToken>()
        );
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.ToString() == expectedUrl2),
            ItExpr.IsAny<CancellationToken>()
        );
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.ToString().Contains("results/algorithm/requests/")),
            ItExpr.IsAny<CancellationToken>()
        );
        this.handlerMock.VerifyNoOtherCalls();
        Assert.Equal(2, result.Count);
    }

    [InlineData("algorithm", "http://www.example.com/algorithm/v1/metadata/algorithm/requests/12345")]
    [Theory]
    public async Task TestGetRequestMetadataAsync(string algorithm, string expectedUrl)
    {
        // Act
        await nexusClient.GetRequestMetadataAsync(
            "12345",
            algorithm,
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [InlineData("algorithm", "http://www.example.com/algorithm/v1/cancel/algorithm/requests/12345")]
    [Theory]
    public async Task TestCancelRunAsync(string algorithm, string expectedUrl)
    {
        // Act
        await nexusClient.CancelRunAsync(
            "12345",
            algorithm,
            "initiator",
            "reason",
            "Foreground",
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [InlineData("algorithm", "http://www.example.com/algorithm/v1/cancel/algorithm/requests/12345")]
    [Theory]
    public async Task TestUsesAuthenticationProvider(string algorithm, string expectedUrl)
    {
        // Arrange
        this.handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { status = "COMPLETED" })
            });

        // Act
        await nexusClient.CancelRunAsync(
            "12345",
            algorithm,
            "initiator",
            "reason",
            "Foreground",
            CancellationToken.None);

        await nexusClient.CancelRunAsync(
            "12345",
            algorithm,
            "initiator",
            "reason",
            "Foreground",
            CancellationToken.None);

        // Assert
        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl &&
                                                 req.Headers.Authorization != null &&
                                                 req.Headers.Authorization.Scheme == "Bearer" &&
                                                 req.Headers.Authorization.Parameter == "token"),
            ItExpr.IsAny<CancellationToken>()
        );

        // Assert that the provider was called twice
        // Verifies that token provider issues a new token every time when called by the Nexus client
        tokenProviderMock.Verify(m =>
            m.GetTokenAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}