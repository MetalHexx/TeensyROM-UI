using System.Net.Http.Headers;
using System.Reflection;
using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Endpoints.Files.Index;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ResetDevice;
using TeensyRom.Api.Models;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration.Common
{
    public class EndpointFixture : IDisposable
    {
        public HttpClient Client
        {
            get
            {
                var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

                client.Timeout = TimeSpan.FromMinutes(10);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                return client;
            }
        }
        public Fixture DataGenerator { get; private set; } = new();

        private readonly WebApplicationFactory<Program> _factory;

        public EndpointFixture()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        public async Task<string> GetConnectedDevice() 
        {
            var findResponse = await Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest()
            {
                AutoConnectNew = true
            });
            var deviceId = findResponse.Content.Devices.First().DeviceId!;

            return deviceId;
        }

        public async Task<List<CartDto>> ConnectToDevices()
        {
            var findResponse = await Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest()
            {
                AutoConnectNew = true
            });
            return findResponse.Content.Devices;
        }


        public async Task DisconnectDevice(string deviceId) 
        {
            var request = new DisconnectDeviceRequest
            {
                DeviceId = deviceId
            };
            await Client.DeleteAsync<DisconnectDeviceEndpoint, DisconnectDeviceRequest, DisconnectDeviceResponse>(request);
        }

        public async Task Preindex(string deviceId, TeensyStorageType storageType, string path) 
        {
            var request = new IndexRequest
            {
                DeviceId = deviceId,
                StorageType = storageType,
                StartingPath = path
            };
            var response = await Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);
            await Task.Delay(3000);
        }

        public void Reset() 
        {
            var initialCarts = Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest()
            {
                AutoConnectNew = true
            }).Result;

            var connectDevices = initialCarts.Content.Devices
                .Where(d => d.IsConnected)
                .ToList();

            connectDevices.ForEach(d =>
            {
                _ = Client.DeleteAsync<DisconnectDeviceEndpoint, Endpoints.ClosePort.DisconnectDeviceRequest, DisconnectDeviceResponse>(new Endpoints.ClosePort.DisconnectDeviceRequest
                {
                    DeviceId = d.DeviceId!
                }).Result;
            });
        }
        public async Task ResetDevice(string deviceId)
        {
            await Client.PutAsync<ResetDeviceEndpoint, ResetDeviceRequest, ResetDeviceResponse>(new ResetDeviceRequest
            {
                DeviceId = deviceId
            });
            await Task.Delay(3000);
        }

        public string DeleteCache(string deviceId, TeensyStorageType storageType)
        {
            var path = GetCachePath(deviceId, storageType);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }

        public bool CacheExists(string deviceId, TeensyStorageType storageType)
        {
            var path = GetCachePath(deviceId, storageType);
            return File.Exists(path);
        }

        public string GetCachePath(string deviceId, TeensyStorageType storageType)
        {
            var path = string.Empty;

            if (storageType == TeensyStorageType.SD)
            {
                path = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Sd_Cache_File_Relative_Path,
                    $"{StorageHelper.Sd_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
            }
            else
            {
                path = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Usb_Cache_File_Relative_Path,
                    $"{StorageHelper.Usb_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
            }
            return path;
        }

        public void Dispose() => _factory.Dispose();
    }
}
