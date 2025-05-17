//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TeensyRom.Api.Endpoints.Files.Index;
//using TeensyRom.Api.Endpoints.Files.IndexAll;
//using TeensyRom.Api.Endpoints.Files.LaunchFile;
//using TeensyRom.Api.Endpoints.FindCarts;
//using TeensyRom.Api.Endpoints.OpenPort;
//using TeensyRom.Api.Endpoints.Serial.GetPorts;
//using TeensyRom.Core.Common;
//using TeensyRom.Core.Entities.Storage;

//namespace TeensyRom.Api.Tests.Integration
//{
//    [Collection("Endpoint")]
//    public class AllTests(EndpointFixture f) : IDisposable
//    {
//        public async void When_Called_AvailableCartsReturned()
//        {
//            // Act
//            var r = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

//            // Assert
//            r.Should()
//                .BeSuccessful<FindCartsResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Content.Message.Should().Be("Success!");
//            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();
//            r.Content.ConnectedCarts.Should().BeEmpty();
//        }

//        [Fact]
//        public async void Given_CartWasOpened_When_FindCalled_ConnectedCartsReturned()
//        {
//            // Arrange
//            var initialCarts = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var expectedConnectedCart = initialCarts.Content.AvailableCarts.First();
//            var expectedAvailableCount = initialCarts.Content.AvailableCarts.Count;
//            var openRequest = new OpenPortRequest
//            {
//                DeviceId = expectedConnectedCart.DeviceId
//            };
//            var openResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openRequest);

//            // Act
//            var r = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

//            // Assert
//            r.Should().BeSuccessful<FindCartsResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();
//            r.Content.AvailableCarts.Count.Should().Be(expectedAvailableCount);
//            r.Content.ConnectedCarts.Count.Should().Be(1);
//            r.Content.ConnectedCarts.First().DeviceId.Should().Be(expectedConnectedCart.DeviceId);
//        }

//        [Fact]
//        public async void When_TeensyRomsDeactivated_NotFoundReturned()
//        {
//            // Act
//            var r = await f.Client.GetAsync<FindCartsEndpoint, ProblemDetails>();

//            // Assert
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound);

//            r.Content.Title.Should().Be("No TeensyRom devices found.");
//        }

//        [Fact]
//        public async void When_Called_PortsReturned()
//        {
//            // Act
//            var r = await f.Client.GetAsync<GetPortsEndpoint, GetPortsResponse>();

//            // Assert
//            r.Should().BeSuccessful<GetPortsResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Content.Message.Should().Be("Ports found");
//            r.Content.Ports.Should().NotBeNullOrEmpty();
//        }

//        [Fact]
//        public async void When_Called_ResponseSuccessful()
//        {
//            // Arrange
//            var devices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

//            // Act
//            var openRequest = new OpenPortRequest
//            {
//                DeviceId = devices.Content.AvailableCarts.First().DeviceId
//            };
//            var r = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openRequest);

//            // Assert
//            r.Should().BeSuccessful<OpenPortResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Content.Should().NotBeNull();
//            r.Content.ConnectedCart.Should().NotBeNull();
//            r.Content.ConnectedCart.DeviceId.Should().NotBeNullOrEmpty();
//            r.Content.ConnectedCart.DeviceId.Should().Be(openRequest.DeviceId);
//            r.Content.ConnectedCart.ComPort.Should().NotBeNullOrEmpty();

//            r.Content.Message.Should().Contain("Connection successful!");
//        }

//        private const string NonExistentPath = "/something/that/doesnt/exist.sid";

//        [Fact]
//        public async void When_LaunchingVariousFiles_ReturnsSuccess()
//        {
//            // Arrange              
//            var availableDevices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;
//            var openPortRequest = new OpenPortRequest { DeviceId = deviceId };

//            await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);

//            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                StorageType = TeensyStorageType.SD
//            });

//            await Task.Delay(3000);

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
//                StorageType = TeensyStorageType.SD
//            });

//            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                StorageType = TeensyStorageType.SD
//            });

//            // Assert  
//            r.Should().BeSuccessful<LaunchFileResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Content.Message.Should().Contain("Success");
//        }

//        [Fact]
//        public async void When_LaunchCalled_WithInvalidPath_ReturnsNotFound()
//        {
//            // Arrange              
//            var availableDevices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;
//            var openPortRequest = new OpenPortRequest { DeviceId = deviceId! };

//            await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);

//            // Act  
//            var request = new LaunchFileRequest
//            {
//                DeviceId = deviceId!,
//                FilePath = NonExistentPath,
//                StorageType = TeensyStorageType.SD
//            };

//            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
//            // Assert  
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound);
//        }

//        [Fact]
//        public async void When_LaunchCalled_WithInvalidDeviceId_ReturnsNotFound()
//        {
//            // Act  
//            var request = new LaunchFileRequest
//            {
//                DeviceId = "invalid-device-id",
//                FilePath = NonExistentPath,
//                StorageType = TeensyStorageType.SD
//            };
//            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ValidationProblemDetails>(request);
//            // Assert  
//            r.Should().BeValidationProblem()
//                .WithStatusCode(HttpStatusCode.BadRequest);
//        }

//        [Fact]
//        public async void When_LaunchCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
//        {
//            // Act  
//            var request = new LaunchFileRequest
//            {
//                DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash(),
//                FilePath = NonExistentPath
//            };
//            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
//            // Assert  
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound);
//        }

//        [Fact]
//        public async void When_LaunchCalled_WithInvalidStorageType_ReturnsBadRequest()
//        {
//            // Act  
//            var request = new LaunchFileRequest
//            {
//                DeviceId = "invalid-device-id",
//                FilePath = NonExistentPath,
//                StorageType = (TeensyStorageType)999
//            };
//            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
//            // Assert  
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.BadRequest);
//        }

//        [Fact]
//        public async Task When_Indexing_WithoutPath_SuccessReturned()
//        {
//            // Arrange
//            var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var deviceId = deviceResult.Content.AvailableCarts.First().DeviceId;
//            var openPortRequest = new OpenPortRequest
//            {
//                DeviceId = deviceId!
//            };
//            var openPortResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);
//            DeleteCache(deviceId!, TeensyStorageType.SD);

//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = deviceId!,
//                StorageType = TeensyStorageType.SD,
//                Path = null
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            CacheExists(deviceId!, TeensyStorageType.SD).Should().BeTrue();
//            response.Content.Message.Should().Contain("Success");
//        }

//        [Fact]
//        public async Task When_Indexing_WithGamePath_SuccessReturned()
//        {
//            // Arrange
//            var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var deviceId = deviceResult.Content.AvailableCarts.First().DeviceId;
//            var openPortRequest = new OpenPortRequest
//            {
//                DeviceId = deviceId!
//            };
//            var openPortResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);
//            DeleteCache(deviceId!, TeensyStorageType.SD);

//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = deviceId!,
//                StorageType = TeensyStorageType.SD,
//                Path = "/games"
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            CacheExists(deviceId!, TeensyStorageType.SD).Should().BeTrue();
//            response.Content.Message.Should().Contain("Success");
//        }

//        [Fact]
//        public async Task When_Indexing_WithBadPath_BadRequestReturned()
//        {
//            // Arrange
//            var availableDevices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
//            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;

//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = deviceId,
//                StorageType = TeensyStorageType.SD,
//                Path = "#(*&_#_()*&$"
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

//            // Assert
//            response.Should()
//                .BeValidationProblem()
//                .WithKeyAndValue("Path", "Path must be a valid Unix path.");
//        }

//        [Fact]
//        public async Task When_Indexing_WithoutDeviceId_BadRequestReturned()
//        {
//            // Act
//            var request = new IndexRequest
//            {
//                StorageType = TeensyStorageType.SD,
//                Path = "/"
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

//            // Assert
//            response.Should()
//                .BeValidationProblem()
//                .WithKeyAndValue("DeviceId", "Device ID is required.");
//        }

//        [Fact]
//        public async Task When_Indexing_WithBadDeviceId_BadRequestReturned()
//        {
//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = "12345!!&^#",
//                StorageType = TeensyStorageType.SD,
//                Path = "/"
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

//            // Assert
//            response.Should()
//                .BeValidationProblem()
//                .WithKeyAndValue("DeviceId", "Device ID must be a valid deviceId.  Only a string of 8 numbers and letters are supported.  No other special characters or spaces.");
//        }


//        [Fact]
//        public async Task When_IndexingAll_SuccessReturned()
//        {
//            // Arrange
//            var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

//            foreach (var item in deviceResult.Content.AvailableCarts)
//            {
//                await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(new OpenPortRequest
//                {
//                    DeviceId = item.DeviceId!
//                });
//                DeleteCache(item.DeviceId!, TeensyStorageType.SD);
//                DeleteCache(item.DeviceId!, TeensyStorageType.USB);
//            }

//            // Act
//            var response = await f.Client.PostAsync<IndexAllEndpoint, IndexResponse>();

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            deviceResult.Content.AvailableCarts
//                .Where(d => d.SdStorage.Available)
//                .Should()
//                .AllSatisfy(item =>
//                {
//                    CacheExists(item.DeviceId!, TeensyStorageType.SD).Should().BeTrue();
//                });

//            deviceResult.Content.AvailableCarts
//                .Where(d => d.UsbStorage.Available)
//                .Should()
//                .AllSatisfy(item =>
//                {
//                    CacheExists(item.DeviceId!, TeensyStorageType.USB).Should().BeTrue();
//                });
//            response.Content.Message.Should().Contain("Success");
//        }






















//        private string DeleteCache(string deviceId, TeensyStorageType storageType)
//        {
//            var path = GetCachePath(deviceId, storageType);
//            if (File.Exists(path))
//            {
//                File.Delete(path);
//            }
//            return path;
//        }

//        private bool CacheExists(string deviceId, TeensyStorageType storageType)
//        {
//            var path = GetCachePath(deviceId, storageType);
//            return File.Exists(path);
//        }

//        private string GetCachePath(string deviceId, TeensyStorageType storageType)
//        {
//            var path = string.Empty;

//            if (storageType == TeensyStorageType.SD)
//            {
//                path = Path.Combine(
//                    Assembly.GetExecutingAssembly().GetPath(),
//                    StorageHelper.Sd_Cache_File_Relative_Path,
//                    $"{StorageHelper.Sd_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
//            }
//            else
//            {
//                path = Path.Combine(
//                    Assembly.GetExecutingAssembly().GetPath(),
//                    StorageHelper.Usb_Cache_File_Relative_Path,
//                    $"{StorageHelper.Usb_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
//            }
//            return path;
//        }

//        public void Dispose() => f.Reset();
//    }
//}
