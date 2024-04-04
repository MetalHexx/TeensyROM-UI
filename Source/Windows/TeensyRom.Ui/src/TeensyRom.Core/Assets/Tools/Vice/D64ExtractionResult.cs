namespace TeensyRom.Core.Assets.Tools.Vice
{
    public class D64ExtractionResult
    {
        public string D64Name { get; }
        public List<FileInfo> ExtractedFiles { get; }
        

        public D64ExtractionResult(string d64Name, List<FileInfo> extractedFiles)
        {
            D64Name = d64Name;
            ExtractedFiles = extractedFiles;
                    
        }
    }
}