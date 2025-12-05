using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal.Models.Base;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;
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
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
        this.nexusClient = new NexusClient(adapter);
    }

    [InlineData("algorithm", "file:///algorithm/v1/algorithm/v1/run/algorithm?dryRun=False")]
    [Theory]
    public async Task TestShouldRenewTokenAsync(string algorithm, string expectedUrl)
    {
        var payload = JsonDocument.Parse("null").RootElement;
        var result = await nexusClient.CreateRunAsync(
            payload,
            algorithm,
            null,
            null,
            null,
            null,
            false,
            CancellationToken.None);

        this.handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}