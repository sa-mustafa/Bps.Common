namespace Bps.Common
{
    using System.IO;

    public static class FailSafeCopy
    {
        public static void FileCopy(string Source, string Destination)
        {
            using (var fs = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                using (var sw = new BinaryWriter(fs))
                {
                    sw.Write(File.ReadAllBytes(Source));
                    sw.Flush();
                }
            }
        }

        public static void DirectoryCopy(string Source, string Destination, bool Recursive = true)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(Source);
            if (!dir.Exists) return;

            // If the destination directory doesn't exist, create it.       
            if (!Directory.Exists(Destination))
                Directory.CreateDirectory(Destination);

            // Get the files in the directory and copy them to the new location.
            foreach (var file in dir.GetFiles())
            {
                string tempPath = Path.Combine(Destination, file.Name);
                FileCopy(file.FullName, tempPath);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (Recursive)
            {
                foreach (var subdir in dir.GetDirectories())
                {
                    string tempPath = Path.Combine(Destination, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, Recursive);
                }
            }
        }
    }
}