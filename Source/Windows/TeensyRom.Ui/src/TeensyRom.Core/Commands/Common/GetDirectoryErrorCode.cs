using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Commands.Common
{
    public enum GetDirectoryErrorCodeType
    {
        StorageTypeParamError = 1,
        SkipParamError = 2,
        TakeParamError = 3,
        PathParamError = 4,
        DirectoryNotFoundError = 5,
        NotADirectoryError = 6,
        UnknownError = 7
    }

    public static class SerialErrorCodeHelper
    {
        public static GetDirectoryErrorCodeType GetDirectoryErrorCode(this string errorMessage)
        {
            return errorMessage switch
            {
                string msg when msg.Contains("Error 1") => GetDirectoryErrorCodeType.StorageTypeParamError,
                string msg when msg.Contains("Error 2") => GetDirectoryErrorCodeType.SkipParamError,
                string msg when msg.Contains("Error 3") => GetDirectoryErrorCodeType.TakeParamError,
                string msg when msg.Contains("Error 4") => GetDirectoryErrorCodeType.PathParamError,
                string msg when msg.Contains("Error 5") => GetDirectoryErrorCodeType.DirectoryNotFoundError,
                string msg when msg.Contains("Error 6") => GetDirectoryErrorCodeType.NotADirectoryError,
                _ => GetDirectoryErrorCodeType.UnknownError
            };
        }
    }
}
