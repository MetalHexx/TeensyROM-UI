using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests.Unit
{
    public class ConnectViewModelTests
    {
        private readonly IMediator _mediatorMock = Substitute.For<IMediator>();
        private readonly ISerialStateContext _serialMock = Substitute.For<ISerialStateContext>();
        private readonly ILoggingService _logMock = Substitute.For<ILoggingService>();
        private readonly IObservableSerialPort _observableSerialPort = Substitute.For<IObservableSerialPort>();


        [Fact]
        public void Given_SerialPortsDoNotExist_SerialPortsEmpty()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.Ports.Should().BeEmpty();
        }

        [Fact]
        public void Given_SerialPortsDoNotExist_NotConnected()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void Given_SerialPortsExist_SerialPortsDisplayed()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.Ports.Should().BeEquivalentTo(new[] { "COM3", "COM4" });
        }

        [Fact]
        public void Given_SerialPortsExist_FirstSerialPortSelected()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.SelectedPort.Should().BeEquivalentTo("COM3");
        }

        [Fact]
        public void Given_SerialPortsExist_WhenSerialPortSelected_SerialPortSet()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            viewModel.SelectedPort = "COM4";

            //Assert
            _serialMock.Received(1).SetPort("COM4");
        }



        [Fact]
        public void Given_SerialConnectedState_IsConnectedTrue()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.IsConnected.Should().BeTrue();
        }

        [Fact]
        public void Given_NoSerialPortsExist_IsConnectableFalse()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.IsConnectable.Should().BeFalse();
        }

        [Fact]
        public void Given_SerialConnectableState_IsConnectableTrue()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectableState(_observableSerialPort)).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Assert
            viewModel.IsConnectable.Should().BeTrue();
        }

        [Fact]
        public void Given_IsConnectable_When_ConnectCommandExecuted_SerialPortOpened_And_ResetCommandCalled()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectableState(_observableSerialPort)).AsObservable());
            var logMock = Substitute.For<ILoggingService>();

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, logMock);

            //Act
            viewModel.ConnectCommand.Execute().Subscribe();

            //Assert
            _serialMock.Received(1).OpenPort();
            _mediatorMock.Received(1).Send(Arg.Any<ResetCommand>());
        }

        [Fact]
        public void Given_IsConnected_When_DisconnectCommandExecuted_SerialPortClosed_AndResetCommandCalled()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var logMock = Substitute.For<ILoggingService>();

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, logMock);

            //Act
            viewModel.DisconnectCommand.Execute().Subscribe();

            //Assert
            _serialMock.Received(1).ClosePort();
        }

        [Fact]
        public void Given_IsConnected_When_PingCommandExecuted_PingCommandCalled()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var logMock = Substitute.For<ILoggingService>();

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, logMock);

            //Act
            viewModel.PingCommand.Execute().Subscribe();

            //Assert
            _mediatorMock.Received(1).Send(Arg.Any<PingCommand>());
        }

        [Fact]
        public void Given_IsConnected_When_ResetCommandExecuted_ResetCommandCalled()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var logMock = Substitute.For<ILoggingService>();

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, logMock);

            //Act
            viewModel.ResetCommand.Execute().Subscribe();

            //Assert
            _mediatorMock.Received(1).Send(Arg.Any<ResetCommand>());
        }


        [Fact]
        public void Given_IsConnected_WhenDisconnectCommandExecuted_SerialPortClosed()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());

            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            viewModel.DisconnectCommand.Execute().Subscribe();

            //Assert
            _serialMock.Received(1).ClosePort();
        }


        [Fact]
        public void Given_IsConnectable_Then_ConnectCommand_CanBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectableState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.ConnectCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void Given_IsNotConnectable_Then_ConnectCommand_CannotBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialStartState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.ConnectCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Given_IsConnected_Then_DisconnectCommand_CanBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.DisconnectCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void Given_IsNotConnected_Then_DisconnectCommand_CannotBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialStartState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.DisconnectCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Given_IsConnected_Then_PingCommand_CanBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.PingCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void Given_IsNotConnected_Then_PingCommand_CannotBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialStartState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.PingCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Given_IsConnected_Then_ResetCommand_CanBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.ResetCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void Given_IsNotConnected_Then_ResetCommand_CannotBeExecuted()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialStartState(_observableSerialPort)).AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.ResetCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void When_LogServiceEmitsLogs_LogsAreAvailable()
        {
            //Arrange
            var logSubject = new BehaviorSubject<string>("log1");
            _logMock.Logs.Returns(logSubject.AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            logSubject.OnNext("log2");
            logSubject.OnNext("log3");


            //Assert
            viewModel.Logs.Should().BeEquivalentTo(new[] { "log1", "log2", "log3" });
        }

        [Fact]
        public void Given_LogsExist_When_LogCommandIsCalled_LogsAreCleared()
        {
            //Arrange
            var logSubject = new BehaviorSubject<string>("log1");
            _logMock.Logs.Returns(logSubject.AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            logSubject.OnNext("log2");
            logSubject.OnNext("log3");
            viewModel.ClearLogsCommand.Execute().Subscribe();


            //Assert
            viewModel.Logs.Should().BeEquivalentTo([]);
        }

        [Fact]
        public void Given_ThereAre1000Logs_When_NextLogAdded_FirstLogRemoved()
        {
            //Arrange
            var logSubject = new BehaviorSubject<string>("log0");
            _logMock.Logs.Returns(logSubject.AsObservable());
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            foreach (var i in Enumerable.Range(1, 999))
            {
                logSubject.OnNext($"log{i}");
            }

            //Act
            logSubject.OnNext("log1000");

            //Assert
            viewModel.Logs.First().Should().Be("log1");
            viewModel.Logs.Last().Should().Be("log1000");
        }

        [Fact]
        public void Given_ThereAreNoLogs_Then_ClearLogsCommand_CannotBeExecuted()
        {
            //Arrange
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            viewModel.ClearLogsCommand.Execute().Subscribe();
            var canExecute = viewModel.ClearLogsCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Given_ThereAreLogs_Then_ClearLogsCommand_CannotBeExecuted()
        {
            //Arrange
            var viewModel = new ConnectViewModel(_mediatorMock, _serialMock, _logMock);

            //Act
            var canExecute = viewModel.ClearLogsCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }
    }
}