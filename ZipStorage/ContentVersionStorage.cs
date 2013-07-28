using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileContentArchive
{
    public class ContentVersionStorage
    {
        IFileStorage storage;
        Dictionary<string, List<FileStorageEntry>> History = new Dictionary<string, List<FileStorageEntry>>();

        public ContentVersionStorage(IFileStorage istorage = null)
        {
            if (istorage == null)
            {
                istorage = new ZipStorage("fv.zip");
            }
            this.storage = istorage;

            PopulateContents();
        }
        public string GetLatestVersion(string key)
        {
            if (History.ContainsKey(key))
            {
                List<FileStorageEntry> versions = History[key];
                FileStorageEntry actual = (from v in versions
                                           orderby v.Created descending
                                           select v).First();
                return storage.GetContent(actual.Location);
            }
            else
            {
                return null;
            }
        }
        public void AddVersion(string key, string contents)
        {
            string loc = locPostfixNew(key);
            storage.UpdateContent(loc, contents);
            PopulateContents();// TODO: just add to dictionary instant
        }
        string locPostfixNew(string key)
        {
            if (!History.ContainsKey(key))
            {
                return key + "$1" + DateTime.UtcNow.ToString("dd.MM.yyyy_HH-mm-ss");
            }
            else
            {
                return key + "$" + (History[key].Count + 1) + "_" + DateTime.UtcNow.ToString("dd.MM.yyyy_HH-mm-ss");
            }
        }
        string KeyPrefix(string locFull)
        {
            int index = locFull.IndexOf('$');
            if (index > 0)
            {
                return locFull.Substring(0, index);
            }
            return locFull;
        }

        void PopulateContents()
        {
            History.Clear();

            foreach (FileStorageEntry item in storage.GetAllEntrys())
            {
                string k = KeyPrefix(item.Location);
                if (!History.ContainsKey(k))
                {
                    History.Add(k, new List<FileStorageEntry>() { item });
                }
                else
                {
                    History[k].Add(item);
                }
            }
        }
    }
}
