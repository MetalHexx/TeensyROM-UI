﻿using System.Reactive;

namespace TeensyRom.Core.Settings
{
    public interface ISettingsService
    {
        IObservable<TeensySettings> Settings { get; }
        bool SaveSettings(TeensySettings settings);
        TeensySettings GetSettings();
        void SetCart(string comPort);
    }
}