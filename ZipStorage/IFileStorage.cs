using System;

namespace FileContentArchive
{
    public class FileStorageEntry
    {
        public string Location { get; set; }
        public DateTime Created { get; set; }
    }
    public interface IFileStorage
    {
        string GetContent(string loc);
        void UpdateContent(string loc, string content);
        FileStorageEntry[] GetAllEntrys();
    }
}
