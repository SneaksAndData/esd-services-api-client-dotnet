using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ESD.ApiClient.Tests;

    public class BoxerMockHttpHandler : DelegatingHandler
    {
        private int callCount;

        private readonly List<HttpResponseMessage> responses = new()
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("boxerToken")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("boxerToken")
            },
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var response = responses[callCount];
            callCount++;
            return Task.FromResult(response);
        }
    }