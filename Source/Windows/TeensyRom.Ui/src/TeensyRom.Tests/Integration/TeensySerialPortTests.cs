using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class TeensySerialPortTests
    {
        public TeensySerialPortTests()
        {

        }

        //[Fact]
        //public void Given_OneLoad64DirectoryExists_When_ContentReceived_ReturnsCorrectNumOfItems()
        //{
        //    //Arrange
        //    var logService = new LoggingService();
        //    var teensySerial = new TeensyObservableSerialPort(logService);

        //    //Act
        //    teensySerial.SetPort("COM3");
        //    teensySerial.OpenPort();
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var directoryContent = teensySerial.GetDirectoryContent("/oneload64", TeensyStorageType.SD, 3000, 50);
        //    stopwatch.Stop();

        //    //Assert
        //    directoryContent.TotalCount.Should().Be(0);
        //}

        //[Fact]
        //public void Experiment()
        //{
        //    //Arrange
        //    var logService = new LoggingService();
        //    var teensySerial = new TeensyObservableSerialPort(logService);

        //    var logs = string.Empty;

        //    logService.Logs.Subscribe(log =>
        //    {
        //        logs += log;
        //    });

        //    //Act
        //    teensySerial.SetPort("COM3");
        //    teensySerial.OpenPort();
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    var directoryContent = teensySerial.GetDirectoryContentTest("/oneload64", TeensyStorageType.SD, 50, 300);
        //    stopwatch.Stop();

            

        //    //Assert
        //    directoryContent.Should().Be(string.Empty);
        //}
    }
}
