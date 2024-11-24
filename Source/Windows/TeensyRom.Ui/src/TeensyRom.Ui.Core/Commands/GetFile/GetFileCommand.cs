using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using System.Diagnostics;
using System.IO;

namespace TeensyRom.Ui.Core.Commands.GetFile
{
    public class GetFileCommand(TeensyStorageType storageType, string filePath) : IRequest<GetFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string FilePath { get; } = filePath;
    }
}