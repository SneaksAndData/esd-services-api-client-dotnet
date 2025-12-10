using System.Text;
using KiotaPosts.Client.Models.Models;
using KiotaPosts.Client.Models.V1;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

namespace SnD.ApiClient.Nexus.Models;

public class NexusAlgorithmRequest : AlgorithmRequest
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="NexusAlgorithmRequest"/> class.
    ///  This constructor is private to enforce the use of factory methods for object creation.
    /// </summary>
    private NexusAlgorithmRequest()
    {
    }

    public override void Serialize(ISerializationWriter writer)
    {
        // base.Serialize(writer);
        if (ReferenceEquals(writer, null)) throw new ArgumentNullException(nameof(writer));

        this.SerializeCustomAdditionalDataProperties(writer);
        writer.WriteObjectValue("customConfiguration", CustomConfiguration);
        writer.WriteObjectValue("parentRequest", ParentRequest);
        writer.WriteStringValue("payloadValidFor", PayloadValidFor);
        writer.WriteStringValue("requestApiVersion", RequestApiVersion);
        writer.WriteStringValue("tag", Tag);
        writer.WriteAdditionalData(AdditionalData);
    }

    private void SerializeCustomAdditionalDataProperties(ISerializationWriter writer)
    {
        var json = this.AlgorithmParameters?.AdditionalData["payload"]?.ToString()
                   ?? throw new ArgumentException(
                       $"${nameof(AlgorithmParameters.AdditionalData)} must contain the key 'payload' with a valid JSON string value.");

        // Create the parse node factory
        var factory = new JsonParseNodeFactory();

        // Create the root parse node (the input MUST be valid JSON)
#pragma warning disable CS0618 // Type or member is obsolete
        // Obsolete API is used here because the new async API cannot be used in a synchronous method.
        var root = factory.GetRootParseNode("application/json", new MemoryStream(Encoding.UTF8.GetBytes(json)));
#pragma warning restore CS0618 // Type or member is obsolete

        // Deserialize as UntypedNode
        var node = root.GetObjectValue(UntypedNode.CreateFromDiscriminatorValue);


        writer.WriteObjectValue("algorithmParameters", node);
    }

    /// <summary>
    /// The factory method to create a NexusAlgorithmRequest instance.
    /// </summary>
    /// <param name="algorithmParameters">A JSON string representing the algorithm parameters payload.</param>
    /// <param name="customConfiguration">An optional custom configuration for the algorithm.</param>
    /// <param name="parentRequest">An optional reference to the parent algorithm request.</param>
    /// <param name="payloadValidFor">The duration for which the payload is valid.</param>
    /// <param name="requestApiVersion">The API version for the request.</param>
    /// <param name="tag">A tag to identify the request.</param>
    /// <returns>A new <see cref="NexusAlgorithmRequest"/> instance initialized with the provided values.</returns>
    public static NexusAlgorithmRequest Create(string algorithmParameters,
        NexusAlgorithmSpec? customConfiguration,
        AlgorithmRequestRef? parentRequest,
        string? payloadValidFor,
        string? requestApiVersion,
        string? tag)
    {
        return new NexusAlgorithmRequest
        {
            AlgorithmParameters = new AlgorithmRequest_algorithmParameters
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "payload", algorithmParameters }
                }
            },
            CustomConfiguration = customConfiguration,
            ParentRequest = parentRequest,
            PayloadValidFor = payloadValidFor,
            RequestApiVersion = requestApiVersion,
            Tag = tag,
            AdditionalData = new Dictionary<string, object>()
        };
    }
}