using System.Net.Http;

namespace ESD.ApiClient.Tests
{
    public class MockServiceFixture
    {
        public HttpClient CrystalMockHttpClient { get; private set; }
        public HttpClient BoxerMockHttpClient { get; private set; }

        public MockServiceFixture()
        {
            CrystalMockHttpClient = new HttpClient(new CrystalMockHttpHandler());
            BoxerMockHttpClient = new HttpClient(new BoxerMockHttpHandler());
        }
    }
    
}