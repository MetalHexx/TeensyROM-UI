using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using System.Diagnostics;
using System.IO;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommand : IRequest<GetFileResult>
    {
        public string FilePath { get; init; } = default!;
        public TeensyStorageType StorageType { get; set; }
    }
}