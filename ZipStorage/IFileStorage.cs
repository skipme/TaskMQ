using System;

namespace FileContentArchive
{
    public class FileStorageEntry
    {
        public string Location { get; set; }
        public DateTime Created { get; set; }
        public bool IsDir { get; set; }
    }
    public interface IFileStorage
    {
        string GetContent(string loc);
        byte[] GetContentRaw(string loc);
        byte[] GetContentRaw(string loc, out DateTime dateCreate);

        void UpdateContent(string loc, string content);
        void UpdateContent(string loc, byte[] content);
        FileStorageEntry[] GetAllEntrys();
    }
}
