namespace Bps.Common
{
    using System;
    using System.IO;
    using System.Text;

    public class FailSafeFile
    {
        public FailSafeFile(string SourceFile)
        {
            this.SourceFile = SourceFile;
        }

        public FailSafeFile(string SourceFile, string BackupFile, bool PreserveBackup = false)
        {
            this.SourceFile = SourceFile;
            this.BackupFile = BackupFile;
            this.PreserveBackup = PreserveBackup;
        }

        public string BackupFile { get; set; }

        public string SourceFile { get; set; }

        public int BufferSize { get; set; } = 1024;

        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public FileMode Mode { get; set; } = FileMode.Create;

        public bool PreserveBackup { get; set; }

        public FileShare Share { get; set; } = FileShare.None;

        public void Write(byte[] Content)
        {
            bool hasBackup = !string.IsNullOrEmpty(BackupFile);
            string file = hasBackup ? BackupFile : SourceFile;
            // In case of failures up to now, SourceFile is the credible source for the next load.
            WriteContent(file, Content);
            if (hasBackup)
                CopyBackup();
        }

        public void Write(string Content)
        {
            Write(Encoding.GetBytes(Content));
        }

        public void Write(Action<FileStream, StreamWriter> Callback)
        {
            bool hasBackup = !string.IsNullOrEmpty(BackupFile);
            string file = hasBackup ? BackupFile : SourceFile;
            // In case of failures up to now, SourceFile is the credible source for the next load.
            WriteContent(file, Callback);
            if (hasBackup)
                CopyBackup();
        }

        private void CopyBackup()
        {
            File.Delete(SourceFile);
            // Up to this point, BackupFile is the credible source.
            FailSafeCopy.FileCopy(BackupFile, SourceFile);
            // Again, SourceFile is the credible source for the next load.
            if (!PreserveBackup)
                File.Delete(BackupFile);
        }

        private void WriteContent(string File, byte[] Content)
        {
            using (var fs = new FileStream(File, Mode, FileAccess.Write, Share, BufferSize, FileOptions.WriteThrough))
            {
                using (var sw = new BinaryWriter(fs, Encoding))
                {
                    sw.Write(Content);
                    sw.Flush();
                }
            }
        }

        private void WriteContent(string File, Action<FileStream, StreamWriter> Callback)
        {
            using (var fs = new FileStream(File, Mode, FileAccess.Write, Share, BufferSize, FileOptions.WriteThrough))
            {
                using (var sw = new StreamWriter(fs, Encoding))
                {
                    Callback?.Invoke(fs, sw);
                    sw.Flush();
                }
            }
        }
    }
}