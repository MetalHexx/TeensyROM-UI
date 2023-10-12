using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Media;
using TeensyRom.Core.Files.Abstractions;

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> TestFileCopyCommand { get; set; }

        private readonly ITeensyFileService _fileService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FileTransferViewModel(ITeensyFileService fileService) 
        {
            _fileService = fileService;

            TestFileCopyCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                TestFileCopy(), outputScheduler: ImmediateScheduler.Instance);

            _fileService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

        }

        private Unit TestFileCopy()
        {
            string sourcePath = @"C:\test\New folder (3)\Calliope_John_13.sid";
            string destinationPath = @$"C:\Users\Metal\Downloads\{Guid.NewGuid()}.sid";
            File.Copy(sourcePath, destinationPath, overwrite: true);
            Console.WriteLine($"File copied from {sourcePath} to {destinationPath}");
            return Unit.Default;
        }
    }
}
