namespace TeensyRom.Core.Common
{
    public static class FileHelper 
    {
        public static void DeleteAllFilesStartingWith(string name, string directory)
        {
            if (Directory.Exists(directory))
            {
                var logFiles = Directory.GetFiles(directory, $"{name}*")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.Exists);

                foreach (var file in logFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete {file.FullName}: {ex.Message}");
                    }
                }
            }
        }

        public static void DeleteFilesOlderThan(DateTime date, string directory)
        {
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.Exists && f.LastWriteTime < date);

                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete {file.FullName}: {ex.Message}");
                    }
                }
            }
        }

        public static string GetFileDateTimeStamp(DateTime date)
        {
            return date.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
}
