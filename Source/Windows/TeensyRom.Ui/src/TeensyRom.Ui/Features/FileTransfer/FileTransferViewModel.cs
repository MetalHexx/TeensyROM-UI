using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using TeensyRom.Core.File;

namespace TeensyRom.Ui.Features.FileTransfer
{
    public class FileTransferViewModel : ReactiveObject
    {
        [Reactive]
        public string Logs { get; set; } = string.Empty;
                
        private readonly ITeensyFileService _fileService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public FileTransferViewModel(ITeensyFileService fileService) 
        {
            _fileService = fileService;

            _fileService.Logs.Subscribe(log =>
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });
        }
    }
}
