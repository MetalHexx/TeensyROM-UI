using TeensyRom.Core.Common;

namespace TeensyRom.Core.ValueObjects
{
    public sealed class DirectoryPath : PathValueObject<DirectoryPath>
    {
        public string DirectoryName => Value == "/" ? "/" : Value.GetLastDirectoryFromPath();
        public DirectoryPath? ParentPath => Value.GetParentPath() is null ? null : new (Value.GetParentPath()!);
        public string DirectoryTitle => IsEmpty ? string.Empty : $"{DirectoryName.Substring(0, 1).ToUpper()}{DirectoryName.Substring(1, DirectoryName.Length - 1)}";
        public bool IsRoot => Value.Equals("/");

        public DirectoryPath(string directoryPath) : base(directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return;

            Value = Value
                .RemoveLeadingAndTrailingSlash()
                .EnsureUnixPathEnding()
                .EnsureUnixPathStart();
            
            if (Value.IsValidUnixPath()) return;

            throw new ArgumentException("Must be valid Unix-style path", nameof(directoryPath));
        }

        public override bool Equals(DirectoryPath? other)
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

            return other
                .RemoveLeadingAndTrailingSlash()
                .Equals(Value.RemoveLeadingAndTrailingSlash(), StringComparison.OrdinalIgnoreCase);
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


        public DirectoryPath Combine(params DirectoryPath[] directoryPaths)
        {
            var combinedValue = Value;

            var directoryPathStrings = directoryPaths
                .Select(d => d.ToString())
                .ToArray();

            combinedValue = combinedValue.UnixPathCombine(directoryPathStrings);

            return new DirectoryPath(combinedValue);
        }

        public FilePath Combine(FilePath filePath)
        {
            return new FilePath(Value.UnixPathCombine(filePath.ToString()));
        }

        public override bool Equals(object? obj) => Equals(obj as DirectoryPath);
    }
}
