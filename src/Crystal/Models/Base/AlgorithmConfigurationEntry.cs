using System.Text.Json.Serialization;

namespace ESD.ApiClient.Crystal.Models.Base
{
    public enum AlgorithmConfigurationValueType
    {
        PLAIN,
        RELATIVE_REFERENCE
    }

    /// <summary>
    /// Named configuration entry.
    /// </summary>
    public class AlgorithmConfigurationEntry
    {
        /// <summary>
        /// Name of the entry.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the entry. Can be a plain text or a reference, thus actual value is provided via configuration provider service.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Type of the entrie's value. Null is added for backwards-compatibility, behaviour is the same as PLAIN.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlgorithmConfigurationValueType? ValueType { get; set; }
    }
}