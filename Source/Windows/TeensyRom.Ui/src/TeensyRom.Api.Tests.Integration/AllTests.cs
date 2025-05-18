//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TeensyRom.Api.Endpoints.ClosePort;
//using TeensyRom.Api.Endpoints.Files.Index;
//using TeensyRom.Api.Endpoints.Files.IndexAll;
//using TeensyRom.Api.Endpoints.Files.LaunchFile;
//using TeensyRom.Api.Endpoints.FindCarts;
//using TeensyRom.Api.Endpoints.GetDirectory;
//using TeensyRom.Api.Endpoints.OpenPort;
//using TeensyRom.Api.Endpoints.Serial.GetPorts;
//using TeensyRom.Core.Common;
//using TeensyRom.Core.Entities.Storage;

//namespace TeensyRom.Api.Tests.Integration
//{
//    [Collection("Endpoint")]
//    public class AllTests(EndpointFixture f) : IDisposable
//    {
//        [Fact]
//        public async Task When_Closing_ValidDevice_ResponseSuccessful()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();

//            // Act
//            var closeRequest = new ClosePortRequest { DeviceId = deviceId };
//            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ClosePortResponse>(closeRequest);

//            var findResponse = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

//            // Assert
//            r.Should().BeSuccessful<ClosePortResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            findResponse.Content.ConnectedCarts.Count.Should().Be(0);

//            r.Content.Message.Should().Be("Success!");
//        }

//        [Fact]
//        public async Task When_Closing_WithInvalidFormatDeviceId_ReturnsBadRequest()
//        {
//            // Act
//            var closeRequest = new ClosePortRequest { DeviceId = "!!!BAD!!!" };
//            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ValidationProblemDetails>(closeRequest);

//            // Assert
//            r.Should().BeValidationProblem()
//                .WithStatusCode(HttpStatusCode.BadRequest);

//            r.Content.Errors.Should().ContainKey("DeviceId");
//            r.Content.Errors["DeviceId"].First().Should().Be("Device ID must be a valid filename-safe hash of 8 characters long.");
//        }

//        [Fact]
//        public async Task When_Closing_UnknownDeviceId_ReturnsNotFound()
//        {
//            // Act
//            var deviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();
//            var closeRequest = new ClosePortRequest { DeviceId = deviceId };
//            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ProblemDetails>(closeRequest);

//            // Assert
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound);

//            r.Content.Title.Should().Contain($"The device {deviceId} was not found.");
//        }

//        [Fact]
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

//        public static IEnumerable<object[]> ValidPaths =>
//        new List<object[]>
//        {
//            new object[] { "/music/MUSICIANS/L/LukHash/" },
//            new object[] { "/music/MUSICIANS/J/Jammic/" },
//            new object[] { "/games/Large/" }
//        };


//        //TODO: Come back and create a view model or solution for deserializing Interface types.
//        [Theory]
//        [MemberData(nameof(ValidPaths))]
//        public async Task When_ValidRequest_DirectoryReturned(string path)
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            var directoryPath = Path.GetDirectoryName(path)!.Replace('\\', '/');

//            // Act
//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, dynamic>(new GetDirectoryRequest
//            {
//                DeviceId = deviceId,
//                Path = directoryPath,
//                StorageType = TeensyStorageType.SD
//            });

//            // Assert
//            r.Should().BeSuccessful<dynamic>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            r.Should().NotBeNull();
//        }


//        [Fact]
//        public async Task When_DeviceIdInvalid_ReturnsBadRequest()
//        {
//            // Act
//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = "!!invalid",
//                Path = "/music",
//                StorageType = TeensyStorageType.SD
//            });

//            // Assert
//            r.Should().BeValidationProblem()
//                .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
//        }

//        [Fact]
//        public async Task When_PathInvalid_ReturnsBadRequest()
//        {
//            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = validDeviceId,
//                Path = "@!bad-path!!",
//                StorageType = TeensyStorageType.SD
//            });

//            r.Should().BeValidationProblem()
//                .WithKeyAndValue("Path", "Path must be a valid Unix-style file path.");
//        }

//        [Fact]
//        public async Task When_MissingPath_ReturnsBadRequest()
//        {
//            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = validDeviceId,
//                Path = "",
//                StorageType = TeensyStorageType.SD
//            });

//            r.Should().BeValidationProblem()
//                .WithKeyAndValue("Path", "Path is required.");
//        }

//        [Fact]
//        public async Task When_InvalidStorageType_ReturnsBadRequest()
//        {
//            // Arrange
//            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

//            // Act
//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = validDeviceId,
//                Path = "/music",
//                StorageType = (TeensyStorageType)999
//            });

//            // Assert
//            r.Should().BeValidationProblem()
//                .WithKeyAndValue("StorageType", "Storage type must be a valid enum value.");
//        }

//        [Fact]
//        public async Task When_StorageNotAvailable_ReturnsNotFound()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            var expectedPath = "/music";

//            // Act
//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = deviceId,
//                Path = expectedPath,
//                StorageType = TeensyStorageType.USB
//            });

//            // Assert
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound)
//                .WithMessage($"The directory {expectedPath} was not found.");
//        }

//        //TODO: Fix a bug that causes directories that are not found to be added as an empty directory in the cache.
//        [Fact]
//        public async Task When_DirectoryNotFound_ReturnsNotFound()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            var expectedPath = "/fake/path";

//            // Act
//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = deviceId,
//                Path = expectedPath,
//                StorageType = TeensyStorageType.SD
//            });

//            // Assert
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound)
//                .WithMessage($"The directory {expectedPath} was not found.");
//        }

//        [Fact]
//        public async Task When_Directory_IsAFilePath_BadRequestReturned()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            var expectedPath = "/music/MUSICIANS/L/LukHash/Alpha.sid";

//            // Act        

//            var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
//            {
//                DeviceId = deviceId,
//                StorageType = TeensyStorageType.SD,
//                Path = expectedPath
//            });

//            // Assert
//            r.Should().BeProblem()
//                .WithStatusCode(HttpStatusCode.NotFound)
//                .WithMessage($"The directory {expectedPath} was not found.");
//        }

////        [Fact]
////        public async void When_Called_PortsReturned()
////        {
////            // Act
////            var r = await f.Client.GetAsync<GetPortsEndpoint, GetPortsResponse>();

////            // Assert
////            r.Should().BeSuccessful<GetPortsResponse>()
////                .WithStatusCode(HttpStatusCode.OK)
////                .WithContentNotNull();

////            r.Content.Message.Should().Be("Ports found");
////            r.Content.Ports.Should().NotBeNullOrEmpty();
////        }

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
//                f.DeleteCache(item.DeviceId!, TeensyStorageType.SD);
//                f.DeleteCache(item.DeviceId!, TeensyStorageType.USB);
//            }

//            // Act
//            var response = await f.Client.PostAsync<IndexAllEndpoint, IndexResponse>();

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            var availableSdDevices = deviceResult.Content.AvailableCarts
//                .Where(d => d.SdStorage.Available)
//                .ToList();

//            if (availableSdDevices.Count > 0)
//            {
//                availableSdDevices
//                .Should()
//                .AllSatisfy(item =>
//                {
//                    f.CacheExists(item.DeviceId!, TeensyStorageType.SD).Should().BeTrue();
//                });
//            }

//            var availableUsbDevices = deviceResult.Content.AvailableCarts
//                    .Where(d => d.UsbStorage.Available)
//                    .ToList();

//            if (availableUsbDevices.Count > 0)
//            {
//                availableUsbDevices
//                    .Should()
//                    .AllSatisfy(item =>
//                    {
//                        f.CacheExists(item.DeviceId!, TeensyStorageType.USB).Should().BeTrue();
//                    });
//                response.Content.Message.Should().Contain("Success");
//            }
//        }

//        [Fact]
//        public async Task When_Indexing_WithoutPath_SuccessReturned()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            f.DeleteCache(deviceId, TeensyStorageType.SD);

//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = deviceId,
//                StorageType = TeensyStorageType.SD,
//                Path = null
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            f.CacheExists(deviceId, TeensyStorageType.SD).Should().BeTrue();
//            response.Content.Message.Should().Contain("Success");
//        }

//        [Fact]
//        public async Task When_Indexing_WithGamePath_SuccessReturned()
//        {
//            // Arrange
//            var deviceId = await f.ConnectToFirstDevice();
//            f.DeleteCache(deviceId!, TeensyStorageType.SD);

//            // Act
//            var request = new IndexRequest
//            {
//                DeviceId = deviceId,
//                StorageType = TeensyStorageType.SD,
//                Path = "/games"
//            };
//            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

//            // Assert
//            response.Should().BeSuccessful<IndexResponse>()
//                .WithStatusCode(HttpStatusCode.OK)
//                .WithContentNotNull();

//            f.CacheExists(deviceId, TeensyStorageType.SD).Should().BeTrue();
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
//                .WithKeyAndValue("DeviceId", "Invalid Device Id.");
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
//                .WithKeyAndValue("DeviceId", "Invalid Device Id.");
//        }
            
//        private const string NonExistentPath = "/something/that/doesnt/exist.sid";

//            [Fact]
//            public async void When_LaunchingVariousFiles_ReturnsSuccess()
//            {
//                // Arrange              
//                var deviceId = await f.ConnectToFirstDevice();

//                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/images/Dio2.kla",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                    StorageType = TeensyStorageType.SD
//                });

//                await Task.Delay(3000);

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
//                    StorageType = TeensyStorageType.SD
//                });

//                r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
//                    StorageType = TeensyStorageType.SD
//                });

//                // Assert  
//                r.Should().BeSuccessful<LaunchFileResponse>()
//                    .WithStatusCode(HttpStatusCode.OK)
//                    .WithContentNotNull();

//                r.Content.Message.Should().Contain("Success");
//            }

//            [Fact]
//            public async void When_LaunchCalled_WithInvalidPath_ReturnsNotFound()
//            {
//                // Arrange              
//                var deviceId = await f.ConnectToFirstDevice();

//                // Act  
//                var request = new LaunchFileRequest
//                {
//                    DeviceId = deviceId,
//                    FilePath = NonExistentPath,
//                    StorageType = TeensyStorageType.SD
//                };

//                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
//                // Assert  
//                r.Should().BeProblem()
//                    .WithStatusCode(HttpStatusCode.NotFound);
//            }

//            [Fact]
//            public async void When_LaunchCalled_WithInvalidDeviceId_ReturnsNotFound()
//            {
//                // Act  
//                var request = new LaunchFileRequest
//                {
//                    DeviceId = "invalid-device-id",
//                    FilePath = NonExistentPath,
//                    StorageType = TeensyStorageType.SD
//                };
//                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ValidationProblemDetails>(request);

//                // Assert  
//                r.Should().BeValidationProblem()
//                    .WithStatusCode(HttpStatusCode.BadRequest);
//            }

//            [Fact]
//            public async void When_LaunchCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
//            {
//                // Act  
//                var request = new LaunchFileRequest
//                {
//                    DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash(),
//                    FilePath = NonExistentPath
//                };
//                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

//                // Assert  
//                r.Should().BeProblem()
//                    .WithStatusCode(HttpStatusCode.NotFound);
//            }

//            [Fact]
//            public async void When_LaunchCalled_WithInvalidStorageType_ReturnsBadRequest()
//            {
//                // Act  
//                var request = new LaunchFileRequest
//                {
//                    DeviceId = "invalid-device-id",
//                    FilePath = NonExistentPath,
//                    StorageType = (TeensyStorageType)999
//                };
//                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

//                // Assert  
//                r.Should().BeProblem()
//                    .WithStatusCode(HttpStatusCode.BadRequest);
//            }

//        public void Dispose() => f.Reset();
//    }
//}
