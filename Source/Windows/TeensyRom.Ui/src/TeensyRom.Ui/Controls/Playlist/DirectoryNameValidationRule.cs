using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TeensyRom.Core.Common;
using TeensyRom.Ui.Controls.Playlist;

namespace TeensyRom.Ui.Controls.Playlist
{
    public class DirectoryNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {   
            var name = value.ToString() ?? "";

            if (string.IsNullOrEmpty(name)) return ValidationResult.ValidResult;

            return name.IsSafeUnixDirectoryName()
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Must be a valid directory name.");
        }
    }
}