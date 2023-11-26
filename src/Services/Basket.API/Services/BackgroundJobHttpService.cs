using Shared.Configurations;

namespace Basket.API.Services
{
    public class BackgroundJobHttpService
    {
        public HttpClient _client { get; }

        public BackgroundJobHttpService(HttpClient client, BackgroundJobSettings backgroundJobSettings)
        {
            _client = client;
            client.BaseAddress = new Uri(backgroundJobSettings.HangfireUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }
}
