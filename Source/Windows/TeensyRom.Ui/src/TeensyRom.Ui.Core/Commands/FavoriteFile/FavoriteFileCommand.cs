using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class FavoriteFileCommand(TeensyStorageType storageType, string sourcePath, string targetPath) : IRequest<FavoriteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string SourcePath { get; } = sourcePath;
        public string TargetPath { get; } = targetPath;
    }
}