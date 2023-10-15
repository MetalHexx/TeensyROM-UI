namespace TeensyRom.Core.Files
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
        public string DestinationPath { get; set; } = string.Empty;
        public StorageType DestinationType { get; set; } = StorageType.SD;
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
    }
}