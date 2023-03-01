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
            services.AddSingleton<HttpClient>()
            services.AddBoxerAuthorization(async cancellationToken =>
            {
                var token = ""; /* call a Rest API to obtain a valid token, dependent on the identity provider */
                return token;
            });
            services.AddCrystalConnector()
            services.Configure<BoxerConnectorOptions>(configurationRoot.GetSection(nameof(BoxerConnectorOptions)))
            services.Configure<CrystalConnectorOptions>(configurationRoot.GetSection(nameof(CrystalConnectorOptions)))
        }
    }
    
```

Also, you can inject clients directly if you need to change injection scope or custom configuration:
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

## Crystal connector

To run a Crystal algorithm request:

```csharp
       var response = await crystalConnector.CreateRunAsync(
                "algorithm-name",
                /* algorithm payload */,
                /* algorithm configuration */,
                CancellationToken.None
       );
```

To get a request status:

```csharp
    var runResult = await crystalConnector.GetResultAsync("algorithm-name", response.RequestId);
```
