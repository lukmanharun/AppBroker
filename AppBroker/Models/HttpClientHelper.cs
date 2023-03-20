using Polly.Extensions.Http;
using Polly;

namespace AppBroker.Models
{
    public static class HttpClientHelper
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)));
        }
    }
}
