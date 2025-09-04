using RadEndpoints;

namespace TeensyRom.Api.Tests.Integration.Common
{
    /// <summary>
    /// TeensyRom Test Client - A wrapper around RadEndpoints.Testing HttpClient that automatically applies enum serialization support
    /// </summary>
    public class TrClient 
    {
        private readonly HttpClient _httpClient;

        public TrClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region GET Methods

        // GET methods with request - Default enum support
        public async Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TRequest, TResponse>(TRequest request)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.GetAsync<TEndpoint, TRequest, TResponse>(request, TestOptions.WithEnumSupport);
        }

        // GET methods with request - Custom options support
        public async Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TRequest, TResponse>(TRequest request, RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.GetAsync<TEndpoint, TRequest, TResponse>(request, options);
        }

        // GET methods without request - Default enum support
        public async Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TResponse>()
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.GetAsync<TEndpoint, TResponse>(TestOptions.WithEnumSupport);
        }

        // GET methods without request - Custom options support
        public async Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TResponse>(RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.GetAsync<TEndpoint, TResponse>(options);
        }

        #endregion

        #region POST Methods

        // POST methods with request - Default enum support
        public async Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TRequest, TResponse>(TRequest request)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PostAsync<TEndpoint, TRequest, TResponse>(request, TestOptions.WithEnumSupport);
        }

        // POST methods with request - Custom options support
        public async Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TRequest, TResponse>(TRequest request, RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PostAsync<TEndpoint, TRequest, TResponse>(request, options);
        }

        // POST methods without request - Default enum support
        public async Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TResponse>()
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PostAsync<TEndpoint, TResponse>(TestOptions.WithEnumSupport);
        }

        // POST methods without request - Custom options support
        public async Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TResponse>(RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PostAsync<TEndpoint, TResponse>(options);
        }

        #endregion

        #region DELETE Methods

        // DELETE methods with request - Default enum support
        public async Task<RadTestResult<TResponse>> DeleteAsync<TEndpoint, TRequest, TResponse>(TRequest request)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.DeleteAsync<TEndpoint, TRequest, TResponse>(request, TestOptions.WithEnumSupport);
        }

        // DELETE methods with request - Custom options support
        public async Task<RadTestResult<TResponse>> DeleteAsync<TEndpoint, TRequest, TResponse>(TRequest request, RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.DeleteAsync<TEndpoint, TRequest, TResponse>(request, options);
        }

        #endregion

        #region PUT Methods

        // PUT methods with request - Default enum support
        public async Task<RadTestResult<TResponse>> PutAsync<TEndpoint, TRequest, TResponse>(TRequest request)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PutAsync<TEndpoint, TRequest, TResponse>(request, TestOptions.WithEnumSupport);
        }

        // PUT methods with request - Custom options support
        public async Task<RadTestResult<TResponse>> PutAsync<TEndpoint, TRequest, TResponse>(TRequest request, RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PutAsync<TEndpoint, TRequest, TResponse>(request, options);
        }

        #endregion

        #region PATCH Methods

        // PATCH methods with request - Default enum support
        public async Task<RadTestResult<TResponse>> PatchAsync<TEndpoint, TRequest, TResponse>(TRequest request)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PatchAsync<TEndpoint, TRequest, TResponse>(request, TestOptions.WithEnumSupport);
        }

        // PATCH methods with request - Custom options support
        public async Task<RadTestResult<TResponse>> PatchAsync<TEndpoint, TRequest, TResponse>(TRequest request, RadHttpClientOptions? options)
            where TEndpoint : RadEndpoint
        {
            return await _httpClient.PatchAsync<TEndpoint, TRequest, TResponse>(request, options);
        }

        #endregion
    }
}
