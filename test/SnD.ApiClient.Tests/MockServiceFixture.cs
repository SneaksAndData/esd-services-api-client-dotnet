using System.Net.Http;

namespace SnD.ApiClient.Tests
{
    public class MockServiceFixture
    {
        public HttpClient CrystalMockHttpClient { get; }
        public HttpClient BoxerMockHttpClient { get; }

        public MockServiceFixture()
        {
            CrystalMockHttpClient = new HttpClient(new CrystalMockHttpHandler());
            BoxerMockHttpClient = new HttpClient(new BoxerMockHttpHandler());
        }
    }
    
}