namespace ESD.ApiClient.Crystal.Models.Base
{
    /// <summary>
    /// Static algorithm configuration.
    /// </summary>
    public class AlgorithmConfiguration
    {
        /// <summary>
        /// Container image used by this algorithm.
        /// </summary>
        public string ImageRepository { get; set; }

        /// <summary>
        /// Image tag (version) used by this algorithm.
        /// </summary>
        public string ImageTag { get; set; }

        /// <summary>
        /// Computation deadline in seconds.
        /// </summary>
        public int? DeadlineSeconds { get; set; }

        /// <summary>
        /// Total number of allowed retries within a deadline.
        /// </summary>
        public int? MaximumRetries { get; set; }

        /// <summary>
        /// Environment variables to be mapped on container.
        /// </summary>
        public AlgorithmConfigurationEntry[] Env { get; set; }

        /// <summary>
        /// Secrets to be mapped as environment variables on the container.
        /// </summary>
        public string[] Secrets { get; set; }

        /// <summary>
        /// Arguments for container entrypoint.
        /// </summary>
        public AlgorithmConfigurationEntry[] Args { get; set; }

        /// <summary>
        /// Max CPU share allowed for this container.
        /// </summary>
        public string CpuLimit { get; set; }

        /// <summary>
        /// Max memory allocation for this container.
        /// </summary>
        public string MemoryLimit { get; set; }

        /// <summary>
        /// Sets the workgroup that an algorithm should run on. Null value will default to a value provided from app config.
        /// </summary>
        public string Workgroup { get; set; }

        /// <summary>
        /// Git tag associated with this configuration.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Parameters for monitoring of algorithm
        /// </summary>
        public HashSet<string> MonitoringParameters { get; set; }
        
        /// <summary>
        /// Custom resources for this container (e.g. GPU)
        /// </summary>
        public Dictionary<string, string> CustomResources { get; set; }

    }
}
