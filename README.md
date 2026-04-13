# SnD.ApiClient

SDK for the following Sneaks & Data Platform Services:

- Nexus, a high-performance runner for containerized machine learning algorithms based on Akka framework and Kubernetes Jobs API. 
- Boxer, an authorization API based on Json Web Tokens.

# Usage
To use the SnD.ApiClient, you can follow these steps:
1. Install the SnD.ApiClient package from NuGet.
This package is hosted on GitHub Packages, so you need to add the GitHub Packages NuGet source to your project. 
You can do this by running the following command in your terminal, replacing `your-token` and `your-username` with your GitHub token and username:
```bash
export GITHUB_TOKEN=your-token
export USERNAME=your-username
dotnet nuget add source --username $USERNAME --password $GITHUB_TOKEN \
  --store-password-in-clear-text \
  --name github "https://nuget.pkg.github.com/sneaksAndData/index.json"
```

2. Inject the Nexus Client into application dependency injection container:

```csharp
    using SnD.ApiClient.Config;
    using SnD.ApiClient.Extensions;
    
    var services = new ServiceCollection();
    
    // Add Http Client for Nexus Client
    services.AddHttpClient()
        
    // Add ILoggerFactory for logging
    services.AddLogging();
    
    // Assuming that we have a configuration object that contains the necessary settings for the Nexus Client
    // named configurationRoot
    services.Configure<NexusClientOptions>(configurationRoot.GetSection(nameof(NexusClientOptions)))

    // Add retry policy for transient errors
    // For now, only RetryAllErrors is supported, which retries on all exceptions. In the future, we may add more specific retry policies.
    services.AddNexusRetryPolicy(sp => new RetryAllErrors(sp.GetRequiredService<ILogger<RetryAllErrors>>(), sp.GetRequiredService<IOptions<NexusClientOptions>>()))
    
    // Add the Nexus Client to the dependency injection container
    services.AddNexusClient();
    
    // Add the Boxer Client to the dependency injection container
    services.AddBoxerClient(configurationRoot.GetSection(nameof(BoxerClientOptions)));
    
    // Add azure credential provider for authentication
    // This is required for Nexus Client to authenticate with Azure Entra ID to obtain the Access token for calling Nexus API.
    // This will add the authenticaiton provider with the "https://management.core.windows.net/.default" scope.
    // Addionaly it will alsot inject Boxer authentication provider to the dependency injection container,
    // which will be used by the Nexus Client to authenticate with Boxer API.
    services.AuthorizeWithBoxerOnAzure().AddAuthenticationProvider()
```

## Creating and Awaiting Nexus Jobs

The acceptance test `test/SnD.ApiClient.Tests.Acceptance/Nexus/NexusAcceptancetests.cs` shows the full runtime flow:

1. Resolve `INexusClient` from DI.
2. Build a `NexusAlgorithmRequest` (typically from `AlgorithmRequest` + JSON payload).
3. Call `CreateRunAsync(...)` to submit the run.
4. Use `AwaitRunAsync(...)` with a polling interval and cancellation timeout.
5. Check final state with `IsFinished(...)` and optionally `HasSucceeded(...)`.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using KiotaPosts.Client.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;

// Assuning that we resolved INexusClient from the dependency injection container and have the algorithm name and payload ready.
INexusClient nexusClient;

// Start from generated request model, then attach serialized payload.
var request = NexusAlgorithmRequest.Create(
            this.configuration.AlgorithmPayload,
            customConfiguration: null,
            parentRequest: null,
            payloadValidFor: null,
            requestApiVersion: null,
            "example"
);

// Use a bounded timeout so await does not run forever.
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

// Create the run and get the request ID.
var createResponse = await nexusClient.CreateRunAsync(request, algorithmName, dryRun: false, cancellationToken: cts.Token); 

// Await the run completion with polling, and get the final result.
var result = await nexusClient.AwaitRunAsync(
    createResponse.RequestId,
    algorithmName,
    pollInterval: TimeSpan.FromSeconds(2),
    cancellationToken: cts.Token);

if (!nexusClient.IsFinished(result))
{
    throw new TimeoutException("Nexus run did not reach a terminal state in time.");
}

var succeeded = nexusClient.HasSucceeded(result);
Console.WriteLine($"Run {createResponse.RequestId} finished with status: {result.Status}");

if (succeeded == false)
{
    throw new Exception($"Nexus run failed with status: {result.Status}");
}
```
