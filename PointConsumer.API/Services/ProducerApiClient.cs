using Microsoft.Extensions.Options;
using PointConsumer.API.Configurations;
using Polly;
using Polly.Retry;
using System.Net;

namespace PointConsumer.API.Services
{
    public class ProducerApiClient : IProducerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _producerApiUrl;
        private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public ProducerApiClient(HttpClient httpClient, IOptions<ProducerApiOptions> producerApiUrl)
        {
            _httpClient = httpClient;
            _producerApiUrl = producerApiUrl.Value.Url;
            this.ConfigureRetryPolicy();
        }
        public async Task<string> GetInitialPointsAsync()
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(_producerApiUrl));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new HttpRequestException($"Data did not get from the Producer Service. Status code: {response.StatusCode}");
        }

        private void ConfigureRetryPolicy()
        {
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    r.StatusCode >= HttpStatusCode.InternalServerError ||
                    r.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Producer API request is unsuccessfull! Retry: ({retryCount}.) {timeSpan.TotalSeconds} saniye sonra.");
                    }
                );
        }
    }
}
