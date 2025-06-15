using Microsoft.AspNetCore.Http;
using RadEndpoints;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeensyRom.Api.Tests.Integration.Common
{

    public class RadHttpClientOptions
    {
        public HeaderDictionary Headers { get; set; } = [];
    }
    public static class TestClientExtensions
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            NumberHandling = JsonNumberHandling.Strict
        };

        public async static Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TResponse>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TResponse>(HttpMethod.Get, options);
        }

        public async static Task<RadTestResult<TResponse>> GetAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest, TResponse>(request, HttpMethod.Get, options);
        }

        public async static Task<HttpResponseMessage> GetAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest>(request, HttpMethod.Get, options);
        }

        public async static Task<HttpResponseMessage> GetAsync<TEndpoint>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint>(HttpMethod.Get, options);
        }

        public async static Task<RadTestResult<TResponse>> DeleteAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest, TResponse>(request, HttpMethod.Delete, options);
        }

        public async static Task<HttpResponseMessage> DeleteAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest>(request, HttpMethod.Delete, options);
        }

        public async static Task<HttpResponseMessage> DeleteAsync<TEndpoint>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint>(HttpMethod.Delete, options);
        }

        public async static Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest, TResponse>(request, HttpMethod.Post, options);
        }

        public async static Task<HttpResponseMessage> PostAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest>(request, HttpMethod.Post, options);
        }

        public async static Task<RadTestResult<TResponse>> PostAsync<TEndpoint, TResponse>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TResponse>(HttpMethod.Post, options);
        }

        public async static Task<HttpResponseMessage> PostAsync<TEndpoint>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint>(HttpMethod.Post, options);
        }

        public async static Task<RadTestResult<TResponse>> PutAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest, TResponse>(request, HttpMethod.Put, options);
        }

        public async static Task<HttpResponseMessage> PutAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest>(request, HttpMethod.Put, options);
        }

        public async static Task<HttpResponseMessage> PutAsync<TEndpoint>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint>(HttpMethod.Put, options);
        }

        public async static Task<RadTestResult<TResponse>> PatchAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest, TResponse>(request, HttpMethod.Patch, options);
        }

        public async static Task<HttpResponseMessage> PatchAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint, TRequest>(request, HttpMethod.Patch, options);
        }

        public async static Task<HttpResponseMessage> PatchAsync<TEndpoint>(this HttpClient client, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            return await client.SendAsync<TEndpoint>(HttpMethod.Patch, options);
        }

        public async static Task<RadTestResult<TResponse>> SendAsync<TEndpoint, TResponse>(this HttpClient client, HttpMethod method, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            var httpRequest = TestRequestBuilder.BuildRequest<TEndpoint>(client, method, options);

            var httpResponse = await client.SendAsync(httpRequest);
            client.Dispose();

            return new(httpResponse, await httpResponse.DeserializeJson<TResponse>());
        }

        public async static Task<RadTestResult<TResponse>> SendAsync<TEndpoint, TRequest, TResponse>(this HttpClient client, TRequest request, HttpMethod method, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            var httpRequest = TestRequestBuilder.BuildRequest<TEndpoint, TRequest>(client, request, method, options);

            var httpResponse = await client.SendAsync(httpRequest);
            client.Dispose();

            return new(httpResponse, await httpResponse.DeserializeJson<TResponse>());
        }

        private async static Task<HttpResponseMessage> SendAsync<TEndpoint, TRequest>(this HttpClient client, TRequest request, HttpMethod method, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            var httpRequest = TestRequestBuilder.BuildRequest<TEndpoint, TRequest>(client, request, method);

            var httpResponse = await client.SendAsync(httpRequest);
            client.Dispose();

            return httpResponse;
        }

        private async static Task<HttpResponseMessage> SendAsync<TEndpoint>(this HttpClient client, HttpMethod method, RadHttpClientOptions? options = null)
            where TEndpoint : RadEndpoint
        {
            var httpRequest = TestRequestBuilder.BuildRequest<TEndpoint>(client, method, options);
            var httpResponse = await client.SendAsync(httpRequest);
            client.Dispose();
            return httpResponse;
        }

        private static async Task<TResponse> DeserializeJson<TResponse>(this HttpResponseMessage response)
        {
            try
            {
                // Use the custom JsonOptions that include JsonStringEnumConverter
                return (await response!.Content!.ReadFromJsonAsync<TResponse>(JsonOptions))!;
            }
            catch (JsonException ex)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                throw new RadTestException(stringResponse, response, ex);
            }
        }
    }
}