using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class CopyFileCommand(TeensyStorageType storageType, string sourcePath, string destPath) : IRequest<CopyFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string SourcePath { get; } = sourcePath;
        public string DestPath { get; } = destPath;
    }
}