using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    /// <summary>
    /// A wrapper around FileInfo that automatically provides information necessary
    /// to transfer files to TeensyROM.
    /// </summary>
    public class FileTransferItem
    {
        private readonly FileInfo _fileInfo;
        public string FullSourcePath => _fileInfo.FullName;
        public string SourceDirectory => _fileInfo.DirectoryName ?? string.Empty;
        public long Size => _fileInfo.Length;
        public TeensyFileType Type => _fileInfo.Extension.GetFileType();
        public FilePath TargetPath { get; private set; } = new FilePath(string.Empty);
        public TeensyStorageType TargetStorage { get; private set; } = TeensyStorageType.SD;
        public byte[] Buffer { get; private set; } = [];
        public uint StreamLength { get; private set; }
        public ushort Checksum { get; private set; }

        public FileTransferItem(string sourcePath, FilePath targetFilePath, TeensyStorageType targetStorage)
        {
            if (!File.Exists(sourcePath)) 
            {
                throw new FileNotFoundException($"A file was not found at: {sourcePath}");
            }
            _fileInfo = new FileInfo(sourcePath);
            TargetPath = targetFilePath;
            TargetStorage = targetStorage;

            var buffer = ReadFileWithRetry(sourcePath);
            InitializeFromBuffer(buffer);
        }

        public FileTransferItem(FileInfo fileInfo, FilePath targetFilePath, TeensyStorageType targetStorage)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            TargetPath = targetFilePath ?? throw new ArgumentNullException(nameof(targetFilePath));
            TargetStorage = targetStorage;

            var buffer = ReadFileWithRetry(fileInfo.FullName);
            InitializeFromBuffer(buffer);
        }

        public FileTransferItem(byte[] buffer, FilePath targetFilePath, TeensyStorageType targetStorage)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer is null or empty.", nameof(buffer));

            _fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), targetFilePath.FileName));
            TargetPath = targetFilePath;
            TargetStorage = targetStorage;

            InitializeFromBuffer(buffer);
        }

        private void InitializeFromBuffer(byte[] buffer)
        {
            Buffer = buffer;
            StreamLength = (uint)buffer.Length;
            Checksum = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                Checksum += buffer[i];
            }
        }

        private static byte[] ReadFileWithRetry(string localFilePath)
        {
            const int maxRetries = 5;
            const int delayMs = 200;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new BinaryReader(stream);

                    var length = (int)reader.BaseStream.Length;
                    var buffer = reader.ReadBytes(length);

                    if (buffer.Length != length)
                        throw new IOException("File read incomplete.");

                    return buffer;
                }
                catch (IOException)
                {
                    if (attempt == maxRetries - 1)
                        throw;

                    Thread.Sleep(delayMs);
                }
            }

            throw new IOException("Failed to read file after multiple attempts.");
        }
    }
}
