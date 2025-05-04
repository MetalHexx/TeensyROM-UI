using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Api.Tests.Integration.Common
{
    public class EndpointFixture : IDisposable
    {
        public HttpClient Client => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        public Fixture DataGenerator { get; private set; } = new();

        private readonly WebApplicationFactory<Program> _factory;

        public EndpointFixture()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        public void Dispose() => _factory.Dispose();
    }

    [CollectionDefinition("Endpoint")]
    public class EndpointCollection : ICollectionFixture<EndpointFixture> { }
}
