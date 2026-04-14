using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KiotaPosts.Client.Models.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using SnD.ApiClient.Azure;
using SnD.ApiClient.Config;
using SnD.ApiClient.Extensions;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;
using SnD.ApiClient.Tests.Acceptance.Config;
using Xunit;

namespace SnD.ApiClient.Tests.Acceptance.Nexus;


public class NexusAcceptanceTests
{
    private readonly AcceptanceTestsConfiguration configuration = new();
    private readonly ServiceProvider services;

    public NexusAcceptanceTests()
    {

        var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
        configurationRoot.GetSection(nameof(AcceptanceTestsConfiguration)).Bind(this.configuration);

        var serviceCollection =  new ServiceCollection()
            .Configure<BoxerTokenProviderOptions>(configurationRoot.GetSection(nameof(BoxerTokenProviderOptions)))
            .AddSingleton<HttpClient>()
            .AddLogging(conf => conf.AddConsole())
            .Configure<NexusClientOptions>(configurationRoot.GetSection(nameof(NexusClientOptions)))
            .AddNexusRetryPolicy(sp => new RetryAllErrors(sp.GetRequiredService<ILogger<RetryAllErrors>>(),
                sp.GetRequiredService<IOptions<NexusClientOptions>>()))
            .AddNexusClient();

        serviceCollection = configuration.UseBoxerTokenProviderOnAzure
            ? serviceCollection.AuthorizeWithBoxerOnAzure().AddAuthenticationProvider()
            : serviceCollection.AddSingleton<IAuthenticationProvider, EmptyAuthenticationProvider>();
            
        this.services = serviceCollection.BuildServiceProvider();
    }
    
    [SkippableFact]
    public async Task TestCanRunAlgorithm()
    {
        Skip.If(string.IsNullOrEmpty(configuration.AlgorithmName) || configuration.AlgorithmPayload == null, "Algorithm payload and/or name is empty.");

        var request = NexusAlgorithmRequest.Create(
            this.configuration.AlgorithmPayload,
            customConfiguration: null,
            parentRequest: null,
            payloadValidFor: "6h",
            requestApiVersion: null,
            tag: "example"
        );
        var nexusClient = this.services.GetRequiredService<INexusClient>();
        var response = await nexusClient.CreateRunAsync(
            request,
            configuration.AlgorithmName,
            false,
            CancellationToken.None
        );

        Assert.NotNull(response);
        Assert.NotNull(response!.RequestId);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var runResult = await nexusClient.AwaitRunAsync(
            response!.RequestId,
            configuration.AlgorithmName,
            TimeSpan.FromSeconds(2),
            cts.Token);
        Assert.NotNull(runResult);
        Assert.True(nexusClient.IsFinished(runResult), "Run did not finish in expected time.");
    }
}
