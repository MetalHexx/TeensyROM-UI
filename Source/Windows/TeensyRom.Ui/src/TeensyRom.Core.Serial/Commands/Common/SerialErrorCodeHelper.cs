namespace TeensyRom.Core.Serial.Commands.Common
{
    public static class SerialErrorCodeHelper 
    {
        public static GetDirectoryErrorCode GetDirectoryErrorCode(this string errorMessage)
        {
            return errorMessage switch
            {
                string msg when msg.Contains("Error 1") => Serial.Commands.Common.GetDirectoryErrorCode.StorageTypeParamError,
                string msg when msg.Contains("Error 2") => Serial.Commands.Common.GetDirectoryErrorCode.SkipParamError,
                string msg when msg.Contains("Error 3") => Serial.Commands.Common.GetDirectoryErrorCode.TakeParamError,
                string msg when msg.Contains("Error 4") => Serial.Commands.Common.GetDirectoryErrorCode.PathParamError,
                string msg when msg.Contains("Error 5") => Serial.Commands.Common.GetDirectoryErrorCode.DirectoryNotFoundError,
                string msg when msg.Contains("Error 6") => Serial.Commands.Common.GetDirectoryErrorCode.NotADirectoryError,
                _ => Serial.Commands.Common.GetDirectoryErrorCode.UnknownError
            };
        }
    }
}
