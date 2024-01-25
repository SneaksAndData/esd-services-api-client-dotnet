using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SnD.ApiClient.Tests;

    public class CrystalMockHttpHandler : DelegatingHandler
    {
        private int callCount;

        private readonly List<HttpResponseMessage> responses = new()
        {
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"RequestId\": \"00000000-0000-0000-0000-000000000000\" }")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"RequestId\": \"00000000-0000-0000-0000-000000000002\" }")
            },
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"RequestId\": \"00000000-0000-0000-0000-000000000003\" }")
            },
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var response = responses[callCount];
            callCount++;
            return Task.FromResult(response);
        }
    }