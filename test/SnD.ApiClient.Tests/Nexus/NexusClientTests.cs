using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using KiotaPosts.Client.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;
using Xunit;

namespace SnD.ApiClient.Tests.Nexus;

public class NexusClientTests : IClassFixture<MockServiceFixture>, IClassFixture<LoggerFixture>
{
    private readonly INexusClient nexusClient;
    private readonly Mock<HttpMessageHandler> handlerMock;

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

        var httpClient = new HttpClient(handlerMock.Object);
        httpClient.BaseAddress = new Uri("http://www.example.com/");
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

        this.nexusClient = new NexusClient(adapter, loggerFixture.Factory.CreateLogger<NexusClient>());
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
                Content = JsonContent.Create(new { status = "COMPLETED" })
            });
        
        // Act
        await nexusClient.CreateRunAsync(
            new AlgorithmRequest_algorithmParameters(),
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
        await nexusClient.AwaitTaggedRunsAsync(
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
}