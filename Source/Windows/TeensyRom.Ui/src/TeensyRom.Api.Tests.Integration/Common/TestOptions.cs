using RadEndpoints.Testing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeensyRom.Api.Tests.Integration.Common
{
    /// <summary>
    /// Shared configuration for RadEndpoints testing with enum support
    /// </summary>
    public static class TestOptions
    {
        /// <summary>
        /// Default RadHttpClientOptions with JsonStringEnumConverter for proper enum serialization
        /// </summary>
        public static readonly RadHttpClientOptions WithEnumSupport = new()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                NumberHandling = JsonNumberHandling.Strict
            }
        };
    }
}