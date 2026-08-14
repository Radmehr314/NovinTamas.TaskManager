using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Options;
using System.Net.Http.Json;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;
        private readonly MicroserviceOptions _microserviceOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientService(
            HttpClient httpClient,
            ILogger<HttpClientService> logger,
            IOptions<MicroserviceOptions> microserviceOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _microserviceOptions = microserviceOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse?> GetAsync<TResponse>(string serviceName, string endpoint, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(HttpMethod.Get, serviceName, endpoint, null);
            return await SendAsync<TResponse>(request, cancellationToken);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string serviceName, string endpoint, TRequest data, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(HttpMethod.Post, serviceName, endpoint, JsonContent.Create(data));
            return await SendAsync<TResponse>(request, cancellationToken);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string serviceName, string endpoint, HttpContent? content)
        {
            var baseUrl = serviceName.ToLowerInvariant() switch
            {
                "voip" => _microserviceOptions.VoipService,
                "iam" => _microserviceOptions.IAMService,
                "userprofile" => _microserviceOptions.UserProfileService,
                _ => throw new ArgumentException($"Service '{serviceName}' not found")
            };

            var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
            var request = new HttpRequestMessage(method, url) { Content = content };

            // توکن کاربر جاری را به سرویس مقصد پاس می‌دهد، وگرنه سرویس‌های داخلی 401 برمی‌گردانند.
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(authHeader))
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);

            return request;
        }

        private async Task<TResponse?> SendAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("HTTP {StatusCode} from {Url}: {Error}", response.StatusCode, request.RequestUri, error);
                throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {error}");
            }

            if (typeof(TResponse) == typeof(string))
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                return (TResponse)(object)raw;
            }

            if (response.Content.Headers.ContentLength == 0)
                return default;

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
        }
    }
}
