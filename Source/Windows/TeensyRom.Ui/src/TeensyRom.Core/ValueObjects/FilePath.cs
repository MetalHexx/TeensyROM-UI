using TeensyRom.Core.Common;

namespace TeensyRom.Core.ValueObjects
{
    public sealed class FilePath : PathValueObject<FilePath>
    {
        public string FileName => Value.GetFileNameFromUnixPath();
        public string Extension => Value.GetFileExtension();
        public string FileNameWithoutExtension => GetFileName();
        public string FileTitle => IsEmpty ? string.Empty : $"{FileNameWithoutExtension.Substring(0, 1).ToUpper()}{FileNameWithoutExtension.Substring(1, FileNameWithoutExtension.Length - 1)}";
        public DirectoryPath Directory => new(Value.GetParentPath());

        public FilePath(string filePath) : base(filePath)
        {
            if (filePath.Equals(string.Empty)) return;

            Value = Value
                .RemoveLeadingAndTrailingSlash()                
                .EnsureUnixPathStart();

            if (!Value.IsValidUnixFilePath())
            {
                throw new ArgumentException("Must be valid Unix-style path", nameof(filePath));
            }
        }

        private string GetFileName() 
        {
            if (Extension.Equals(string.Empty)) return FileName;
            
            return FileName.Replace(Extension, "");
        }

        public override bool Equals(FilePath? other)
        {
            if (other is null) return false;

            return string.Equals(
                Value,
                other.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string? other)
        {
            if (other is null) return false;

            return string.Equals(
                Value.RemoveLeadingSlash(),
                other.RemoveLeadingSlash(),
                StringComparison.OrdinalIgnoreCase);
        }
        public bool Contains(DirectoryPath path)
        {
            if (path is null) return false;

            return Value.Contains(path.Value, StringComparison.OrdinalIgnoreCase);
        }

        public bool Contains(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            return Value.Contains(path.RemoveLeadingAndTrailingSlash(), StringComparison.OrdinalIgnoreCase);
        }

        public FilePath Combine(params DirectoryPath[] directoryPaths)
        {
            var directoryPathStrings = directoryPaths
                .Select(d => d.ToString())
                .ToArray();

            var combinedValue = Value.UnixPathCombineFilePath(directoryPathStrings);

            return new FilePath(combinedValue);
        }

        public override bool Equals(object? obj) => Equals(obj as FilePath);

        public override int GetHashCode() => Value.GetHashCode();
    }
}
