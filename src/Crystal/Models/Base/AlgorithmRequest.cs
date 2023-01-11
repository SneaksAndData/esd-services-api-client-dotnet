using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Crystal.Models.Base
{
    /// <summary>
    /// Data to be submitted by a client, when doing an algorithm invocation.
    /// </summary>
    public class AlgorithmRequest
    {
        /// <summary>
        /// Algorithm-specific configuration. Must be a JSON-serializable object.
        /// </summary>
        [Required]
        public JsonElement AlgorithmParameters { get; init; }
        
        /// <summary>
        /// Optional custom configuration to be applied when running this algorithm.
        /// </summary>
        public ESD.ApiClient.Crystal.Models.Base.AlgorithmConfiguration? CustomConfiguration { get; init; }
        
        /// <summary>
        /// Tag for tracking request status
        /// </summary>
        public string Tag { get; init; }
    }
}
