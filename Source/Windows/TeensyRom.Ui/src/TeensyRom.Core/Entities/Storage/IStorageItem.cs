﻿using System.ComponentModel;

namespace TeensyRom.Core.Entities.Storage
{
    public interface IStorageItem
    {
        bool IsCompatible { get; set; }
        bool IsFavorite { get; set; }
        bool IsSelected { get; set; }

        string Name { get; set; }
        string Path { get; set; }
        long Size { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;
    }
}