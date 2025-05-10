using MediatR;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.Terminal;
using TeensyRom.Ui.Features.Terminal.SerialCommand;
using TeensyRom.Ui.Services;

namespace TeensyRom.Tests.Unit
{
    public class ConnectViewModelTests
    {
        private readonly IMediator _mediatorMock = Substitute.For<IMediator>();
        private readonly ISerialStateContext _serialMock = Substitute.For<ISerialStateContext>();
        private readonly ILoggingService _logMock = Substitute.For<ILoggingService>();
        private readonly IObservableSerialPort _observableSerialPort = Substitute.For<IObservableSerialPort>();
        private readonly IAlertService _alertService = Substitute.For<IAlertService>();
        private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
        private readonly ISettingsService _settingsService = Substitute.For<ISettingsService>();
        private readonly ISerialCommandViewModel _serialCommandVm = Substitute.For<ISerialCommandViewModel>();


        [Fact]
        public void Given_SerialPortsDoNotExist_SerialPortsEmpty()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.Ports.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Given_SerialPortsDoNotExist_NotConnected_And_NotConnectable()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.IsConnected.Should().BeFalse();
            viewModel.IsConnectable.Should().BeFalse();
        }

        [Fact]
        public void Given_SerialPortsExist_SerialPortsDisplayed()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.Ports.Should().BeEquivalentTo(new[] { "Auto-detect", "COM3", "COM4" });
        }

        [Fact]
        public void Given_SerialPortsExist_AutoIsSelected()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.SelectedPort.Should().BeEquivalentTo("Auto-detect");
        }

        [Fact]
        public void Given_SerialPortsExist_WhenSerialPortSelected_SerialPortSet()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Act
            viewModel.SelectedPort = "COM4";

            //Assert
            _serialMock.Received(1).SetPort("COM4");
        }

        [Fact]
        public void Given_SerialPortsExist_WhenAutoSelect_PortIsNotSet()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM3", "COM4"]).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Act
            viewModel.SelectedPort = "Auto-detect";

            //Assert
            _serialMock.Received(0).SetPort(Arg.Any<string>());
        }



        [Fact]
        public void Given_SerialConnectedState_IsConnectedTrue()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.IsConnected.Should().BeTrue();
        }

        [Fact]
        public void Given_NoSerialPortsExist_IsConnectableFalse()
        {
            //Arrange
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(Array.Empty<string>()).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.IsConnectable.Should().BeFalse();
        }

        [Fact]
        public void Given_SerialConnectableState_IsConnectableTrue()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectableState(_observableSerialPort)).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Assert
            viewModel.IsConnectable.Should().BeTrue();
        }

        [Fact]
        public void Given_IsConnectable_When_ConnectCommandExecuted_And_SinglePortSelected_AndConnectionSucceeds_Great()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectableState(_observableSerialPort)).AsObservable());
            _serialMock.Ports.Returns(new BehaviorSubject<string[]>(["COM1", "COM2", "COM3", "COM4"]).AsObservable());

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);
            viewModel.SelectedPort = "COM3";

            //Act
            viewModel.ConnectCommand.Execute().Subscribe();

            //Assert
            _serialMock.Received(1).OpenPort();
            _serialMock.Received(1).SetPort(Arg.Any<string>());
        }

        [Fact]
        public void Given_IsConnected_When_DisconnectCommandExecuted_SerialPortClosed()
        {
            //Arrange
            _serialMock.CurrentState.Returns(new BehaviorSubject<SerialState>(new SerialConnectedState(_observableSerialPort)).AsObservable());
            var logMock = Substitute.For<ILoggingService>();

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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

            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Act
            var canExecute = viewModel.ResetCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Given_LogsExist_When_LogCommandIsCalled_LogsAreCleared()
        {
            //Arrange
            var logSubject = new BehaviorSubject<string>("log1");
            _logMock.Logs.Returns(logSubject.AsObservable());
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Act
            logSubject.OnNext("log2");
            logSubject.OnNext("log3");
            viewModel.ClearLogsCommand.Execute().Subscribe();


            //Assert
            viewModel.Log.Logs.Should().BeEquivalentTo([]);
        }

        [Fact]
        public void Given_ThereAreNoLogs_Then_ClearLogsCommand_CannotBeExecuted()
        {
            //Arrange
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

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
            var viewModel = new TerminalViewModel(_mediatorMock, _serialMock, _logMock, _alertService, _dialogService, _serialCommandVm, _settingsService);

            //Act
            var canExecute = viewModel.ClearLogsCommand.CanExecute.FirstAsync().Wait();

            //Assert
            canExecute.Should().BeTrue();
        }
    }
    //TODO: Add tests to check dialog service behavior.
}