namespace TeensyRom.Core.Entities.Storage
{
    /// <summary>
    /// A wrapper around FileInfo that automatically provides information necessary
    /// to transfer files to TeensyROM.
    /// </summary>
    public class FileTransferItem
    {
        private readonly FileInfo _fileInfo;

        public string Name => _fileInfo.Name;
        public string SourcePath => _fileInfo.FullName;
        public long Size => _fileInfo.Length;
        public TeensyFileType Type => _fileInfo.Extension.GetFileType();
        public string TargetPath { get; private set; } = string.Empty;
        public TeensyStorageType TargetStorage { get; set; } = TeensyStorageType.SD;
        public byte[] Buffer { get; set; } = new byte[0];
        public uint StreamLength { get; private set; }
        public ushort Checksum { get; private set; }

        public FileTransferItem(string sourcePath, string targetPath, TeensyStorageType targetStorage)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"A file was not found at: {sourcePath}");
            }
            TargetPath = targetPath;
            TargetStorage = targetStorage;
            _fileInfo = new FileInfo(sourcePath);
            GetBinaryFileData(SourcePath);
        }

        public FileTransferItem(FileInfo fileInfo, string targetPath, TeensyStorageType targetStorage)
        {
            TargetPath = targetPath;
            TargetStorage = targetStorage;
            _fileInfo = fileInfo;
            GetBinaryFileData(SourcePath);
        }

        public void GetBinaryFileData(string filePath)
        {
            const int maxRetries = 5;
            const int delayMs = 200;

            //TODO: This method does retries in a situation where 2 UI processes are trying to handle the same file.  Could be improved.

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new BinaryReader(stream);

                    var length = (uint)reader.BaseStream.Length;
                    var buffer = reader.ReadBytes((int)length);

                    if (buffer.Length != length)
                    {
                        throw new IOException("File read incomplete.");
                    }

                    StreamLength = length;
                    Buffer = buffer;
                    Checksum = 0;

                    for (uint i = 0; i < StreamLength; i++)
                    {
                        Checksum += Buffer[i];
                    }

                    return;
                }
                catch (IOException ex)
                {
                    if (attempt == maxRetries - 1)
                        throw;

                    Thread.Sleep(delayMs);
                }
            }
        }
    }
}