namespace TeensyRom.Core.Serial.Commands.Common
{
    public enum GetDirectoryErrorCode
    {
        StorageTypeParamError = 1,
        SkipParamError = 2,
        TakeParamError = 3,
        PathParamError = 4,
        DirectoryNotFoundError = 5,
        NotADirectoryError = 6,
        UnknownError = 7
    }
}
