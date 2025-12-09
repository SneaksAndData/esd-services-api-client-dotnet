using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Polly;

namespace SnD.ApiClient.Nexus;

public class RetryAdapter: IRequestAdapter, IDisposable
{
    private readonly IRequestAdapter baseAdapter;

    public RetryAdapter(IRequestAdapter baseAdapter)
    {
        this.baseAdapter = baseAdapter;
    }
    
    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory)
    {
        this.baseAdapter.EnableBackingStore(backingStoreFactory);
    }

    public async Task<TModelType?> SendAsync<TModelType>(RequestInformation requestInfo,
        ParsableFactory<TModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where TModelType : IParsable
    {
        return await this
            .GetRetryPolicy<TModelType?>()
            .ExecuteAsync(async () =>
                await this.baseAdapter.SendAsync(requestInfo, factory, errorMapping, cancellationToken));
    }

    public async Task<IEnumerable<TModelType>?> SendCollectionAsync<TModelType>(RequestInformation requestInfo,
        ParsableFactory<TModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where TModelType : IParsable
    {
        return await this.baseAdapter.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken);
    }

    public async Task<TModelType?> SendPrimitiveAsync<TModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        return await this.baseAdapter.SendPrimitiveAsync<TModelType>(requestInfo, errorMapping, cancellationToken);
    }

    public async Task<IEnumerable<TModelType>?> SendPrimitiveCollectionAsync<TModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        return await this.baseAdapter.SendPrimitiveCollectionAsync<TModelType>(requestInfo, errorMapping, cancellationToken);
    }

    public async Task SendNoContentAsync(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        await this.baseAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken);
    }

    public async Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo,
        CancellationToken cancellationToken = default)
    {
        return await this.baseAdapter.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);
    }

    public ISerializationWriterFactory SerializationWriterFactory => this.baseAdapter.SerializationWriterFactory;

    public string? BaseUrl
    {
        get => this.baseAdapter.BaseUrl;
        set => this.baseAdapter.BaseUrl = value;
    }

    public void Dispose()
    {
        if (this.baseAdapter is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
    private IAsyncPolicy<TModelType> GetRetryPolicy<TModelType>()
    {
        return Policy<TModelType>
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}