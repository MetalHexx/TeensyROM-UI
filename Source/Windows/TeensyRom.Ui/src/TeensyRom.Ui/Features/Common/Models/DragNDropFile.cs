using System.IO;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class DragNDropFile
    {
        public FileInfo File { get; set; }
        public bool InSubdirectory { get; set; }

        public DragNDropFile(string filePath) 
        {
            File = new FileInfo(filePath);
        }

        public DragNDropFile(string filePath, bool inSubdirectory)
        {
            File = new FileInfo(filePath);
            InSubdirectory = inSubdirectory;
        }
    }
}
