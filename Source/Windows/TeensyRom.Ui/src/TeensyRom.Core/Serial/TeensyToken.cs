﻿using Ardalis.SmartEnum;

namespace TeensyRom.Core.Serial
{
    public sealed class TeensyToken : SmartEnum<TeensyToken, ushort>
    {
        public static readonly TeensyToken PauseMusic = new(0x6466, nameof(PauseMusic));
        public static readonly TeensyToken SetMusicSpeedLinear = new(0x6499, nameof(SetMusicSpeedLinear));
        public static readonly TeensyToken SetMusicSpeedLog = new(0x649A, nameof(SetMusicSpeedLog));
        public static readonly TeensyToken SIDVoiceMuting = new(0x6433, nameof(SIDVoiceMuting));
        public static readonly TeensyToken ListDirectory = new(0x64DD, nameof(ListDirectory));
        public static readonly TeensyToken StartDirectoryList = new(0x5A5A, nameof(StartDirectoryList));
        public static readonly TeensyToken EndDirectoryList = new(0xA5A5, nameof(EndDirectoryList));
        public static readonly TeensyToken LaunchFile = new(0x6444, nameof(LaunchFile));
        public static readonly TeensyToken PlaySubtune = new(0x6488, nameof(PlaySubtune));
        public static readonly TeensyToken SendFile = new(0x64BB, nameof(SendFile));
        public static readonly TeensyToken CopyFile = new(0x64FF, nameof(CopyFile));
        public static readonly TeensyToken GetFile = new(0x64B0, nameof(GetFile));
        public static readonly TeensyToken DeleteFile = new(0x64CF, nameof(DeleteFile));
        public static readonly TeensyToken LegacySendFile = new(0x64AA, nameof(LegacySendFile));
        public static readonly TeensyToken Ack = new(0x64CC, nameof(Ack));
        public static readonly TeensyToken Fail = new(0x9B7F, nameof(Fail));
        public static readonly TeensyToken GoodSIDToken = new(0x9B81, nameof(GoodSIDToken));
        public static readonly TeensyToken BadSIDToken = new(0x9B80, nameof(BadSIDToken));
        public static readonly TeensyToken Unnknown = new(0x0000, nameof(Unnknown));
        public static readonly TeensyToken RetryLaunch = new(0x9B7E, nameof(RetryLaunch));
        public static readonly TeensyToken Ping = new(0x6455, nameof(Ping));
        public static readonly TeensyToken Reset = new(0x64EE, nameof(Reset));
        public static readonly TeensyToken VersionCheck = new('v', nameof(VersionCheck));

        private TeensyToken(ushort value, string name) : base(name, value) { }
    }
}
