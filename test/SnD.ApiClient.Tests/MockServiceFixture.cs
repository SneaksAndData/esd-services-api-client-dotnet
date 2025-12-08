using System;
using System.Net.Http;
using Moq;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;

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
        
        public Mock<HttpMessageHandler> GetMockedHttpClientHandler(Action<Mock<HttpMessageHandler>> setup)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            setup(handlerMock);
            return handlerMock;
        }
        
        public Mock<IJwtTokenExchangeProvider> GetMockedJwtTokenExchangeProvider(Action<Mock<IJwtTokenExchangeProvider>> setup)
        {
            var jwtTokenExchangeProviderMock = new Mock<IJwtTokenExchangeProvider>();
            setup(jwtTokenExchangeProviderMock);
            return jwtTokenExchangeProviderMock;
        }
    }
    
}