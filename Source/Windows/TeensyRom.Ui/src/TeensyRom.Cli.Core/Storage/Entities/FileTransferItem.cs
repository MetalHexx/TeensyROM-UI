namespace TeensyRom.Cli.Core.Storage.Entities
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
            using var reader = new BinaryReader(File.Open(filePath, FileMode.Open));
            StreamLength = (uint)reader.BaseStream.Length;
            Buffer = reader.ReadBytes((int)StreamLength);
            reader.Close();

            for (uint num = 0; num < StreamLength; num++)
            {
                Checksum += Buffer[num];
            }
        }
    }
}