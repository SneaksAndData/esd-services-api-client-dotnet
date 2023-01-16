using Microsoft.Extensions.Logging;

namespace ESD.ApiClient.Tests
{
    public class LoggerFixture
    {
        public ILoggerFactory Factory { get; }
        
        public LoggerFixture()
        {
            this.Factory = LoggerFactory.Create(conf => conf.AddConsole());
        }
    }
}