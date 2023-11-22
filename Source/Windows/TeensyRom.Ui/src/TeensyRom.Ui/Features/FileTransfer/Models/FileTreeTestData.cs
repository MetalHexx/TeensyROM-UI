using System.Collections.Generic;
using System.Collections.ObjectModel;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Some test data for the view
    /// </summary>
    internal static class FileTreeTestData
    {
        public static List<StorageItem> InitializeTestStorageItems()
        {
            return new()
            {
                new DirectoryItem
                {
                    Name = "Test DirectoryTest DirectoryTest DirectoryTest DirectoryTest DirectoryTest Directory",
                    Path = "Test Path"
                },
                new DirectoryItem
                {
                    Name = "Test Directory 2",
                    Path = "Test Path 2"
                },
                new DirectoryItem
                {
                    Name = "Test Directory 3",
                    Path = "Test Path 3"
                },
                new FileItem
                {
                    Name = "Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest ",
                    Path = "Test Path"
                },
                new FileItem
                {
                    Name = "Test File 2",
                    Path = "Test Path 2"
                },
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItem
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
            };
        }
    }
}
