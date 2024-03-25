namespace SnD.ApiClient.Beast.Models;

public sealed class JobDataSocket
{
    /// <summary>
    /// Alias of the data socket
    /// </summary>
    public string Alias { get; set; }
    
    /// <summary>
    /// Fully qualified path to actual data, i.e. abfss://..., s3://... etc.
    /// </summary>
    public string DataPath { get; set; }
    
    /// <summary>
    /// Data format, i.e. csv, json, delta etc.
    /// </summary>
    public string DataFormat { get; set; }
}
