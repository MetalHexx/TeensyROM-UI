namespace TeensyRom.Core.Storage.Entities
{
    /// <summary>
    /// A wrapper around FileInfo that automatically provides information necessary
    /// to transfer files to TeensyROM.
    /// </summary>
    public class TeensyFileInfo
    {
        private readonly FileInfo _fileInfo;
        public string Name => _fileInfo.Name;
        public string FullPath => _fileInfo.FullName;
        public long Size => _fileInfo.Length;
        public TeensyFileType Type => _fileInfo.Extension.GetFileType();
        public string TargetPath { get; set; } = string.Empty;
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public byte[] Buffer { get; set; } = new byte[0];
        public uint StreamLength { get; private set; }
        public ushort Checksum { get; private set; }

        public TeensyFileInfo(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"A file was not found at: {fullPath}");
            }
            _fileInfo = new FileInfo(fullPath);
            GetBinaryFileData(FullPath);
        }

        public TeensyFileInfo(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            GetBinaryFileData(FullPath);
        }

        private void GetBinaryFileData(string filePath)
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

        public override string ToString()
        {
            var storageType = StorageType == TeensyStorageType.USB ? "USB" : "SD";
            return $"\r\nName: {Name}\r\n Type: {Type}\r\n Source Path: {FullPath}\r\n Storage Type: {storageType} Target Path: {TargetPath}\r\n Stream Length: {StreamLength}\r\n Checksum: {Checksum}\r\n";
        }
    }
}