# SnD.ApiClient

SDK for the following Sneaks & Data Platform Services:

- Crystal, a high-performance runner for containerized machine learning algorithms based on Akka framework and Kubernetes Jobs API. 
- Boxer, an authorization API based on Json Web Tokens.
- Acceptance tests that contains end-to-end usage examples for this services.
> **Warning**
> Acceptance tests run on the real infrastructure and costs money to run

# Examples

## Dependency injection
To setup DI container for running Crystal requests using extension methods provided by `Snd.ApiClient` package:

```csharp
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BoxerConnectorOptions>(configurationRoot.GetSection(nameof(BoxerConnectorOptions)));
            services.Configure<CrystalConnectorOptions>(configurationRoot.GetSection(nameof(CrystalConnectorOptions)));
            services.AddSingleton<HttpClient>();
            
            //  -- BEGIN -- Inject Boxer Auth provider for token exchange
            // any OAuth provider
            services.AddBoxerAuthorization(async cancellationToken =>
            {
                var token = ""; /* call a Rest API to obtain a valid token, dependent on the identity provider */
                return token;
            });
            
            // Kubernetes Auth
            // services.AuthorizeWithBoxerOnKubernetes();
            
            // Azure AD Auth
            // services.AuthorizeWithBoxerOnAzure(scopes: new [] { ".default" });
            
            //  -- END -- Inject Boxer Auth provider for token exchange
            
            // Add Crystal client to the DI
            services.AddCrystalClient();
        }
    }
```

The following configurations are needed in `appsettings.json` for the injections:
```json
{
  "BoxerTokenProviderOptions": {
    "BaseUri": "https://boxer-token-exchange-url",
    "IdentityProvider": "azuread,auth0 or anything else that is connected and has configured claims for Boxer-Crystal"
  },
  "CrystalClientOptions": {
    "BaseUri": "https://crystal-base-url",
    "ApiVersion": "v1.2.3 - version to execute requests against"
  }
}
```

Also, you can instantiate clients directly if you need to change injection scope or custom configuration:
```csharp
        var boxerConnectorOptions = new BoxerConnectorOptions();
        var logger = LoggerFactory.Create(conf => conf.AddConsole());
        var boxerTokenProvider new BoxerTokenProvider(boxerConnectorOptions, new HttpClient(), logger, async cancellationToken => {
            var token = ""; /* call a Rest API to obtain a valid token, dependent on the identity provider */
            return token;
        });
        var crystalConnectorOptions = new CrystalConnectorOptions();
        var crystalConnector = new CrystalConnector(crystalConnectorOptions, new HttpClient(), boxerTokenProvider, logger);
```

## Crystal Client

To run a Crystal algorithm request:

```csharp
       var response = await crystalClient.CreateRunAsync(
                "algorithm-name",
                /* algorithm payload */,
                /* algorithm configuration */,
                CancellationToken.None
       );
```

To get a request status:

```csharp
    var runResult = await crystalClient.GetResultAsync("algorithm-name", response.RequestId);
```

To await the run and cancel using external timeout:
```csharp
   var response = await crystalClient.CreateRunAsync(
                "algorithm-name",
                /* algorithm payload */,
                /* algorithm configuration */,
                CancellationToken.None
       );
  
  using var cts = new CancellationTokenSource();
  cts.CancelAfter(TimeSpan.FromMinutes(5);
  var result = await crystalClient.AwaitRunAsync("algorithm-name", response.RequestId, cts.Token);
  
  if (result.Status == RequestLifeCycleStage.FAILED || result.Status == RequestLifeCycleStage.CLIENT_TIMEOUT)
  {
      // run fallback code
  }
```
