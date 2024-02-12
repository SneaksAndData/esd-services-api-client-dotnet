using System.Text.Json.Serialization;

namespace SnD.ApiClient.Beast.Models;

public sealed class JobDataSocket
{
    /// <summary>
    /// Alias of the data socket
    /// </summary>
    [JsonPropertyName("alias")]
    public string Alias { get; set; }
    
    /// <summary>
    /// Fully qualified path to actual data, i.e. abfss://..., s3://... etc.
    /// </summary>
    [JsonPropertyName("dataPath")]
    public string DataPath { get; set; }
    
    /// <summary>
    /// Data format, i.e. csv, json, delta etc.
    /// </summary>
    [JsonPropertyName("dataFormat")]
    public string DataFormat { get; set; }
}