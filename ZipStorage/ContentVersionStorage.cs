using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileContentArchive
{
    public class VersionData
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public byte[] data;
    }
    public class ContentVersionStorage
    {
        IFileStorage storage;
        Dictionary<string, List<FileStorageEntry>> History = new Dictionary<string, List<FileStorageEntry>>();
        public string key_most_fresh { get; private set; }

        public ContentVersionStorage(IFileStorage istorage = null)
        {
            if (istorage == null)
            {
                istorage = new ZipStorage("fv.zip");
            }
            this.storage = istorage;

            PopulateContents();
        }
        public List<FileStorageEntry> GetVersions()
        {
            if (History.Count == 0)
                return null;

            List<FileStorageEntry> versions = History[key_most_fresh];
            return versions;
        }
        //public byte[] GetLatestVersion(ref string location)
        //{
        //    if (History.Count == 0)
        //        return null;

        //    List<FileStorageEntry> versions = History.Values.First();
        //    FileStorageEntry actual = (from v in versions
        //                               orderby v.Created descending
        //                               select v).First();
        //    location = actual.Location;
        //    return storage.GetContentRaw(actual.Location);
        //}
        public List<VersionData> GetLatestVersion(out string revision)
        {
            List<VersionData> vresult = new List<VersionData>(); 
            revision = null;
            if (History.Count == 0)
                return null;

            List<FileStorageEntry> versions = History[revision = key_most_fresh];

            foreach (FileStorageEntry v in versions)
            {
                if (v.IsDir) continue;
                vresult.Add(new VersionData
                {
                    Key = key_most_fresh,
                    Name = v.Location.Remove(0, key_most_fresh.Length + 1),
                    Created = v.Created,
                    data = storage.GetContentRaw(v.Location)
                });
            }
            return vresult;
        }
        public IEnumerable<VersionData> GetSpecificVersion(string key)
        {
            if (History.Count == 0 || !History.ContainsKey(key))
                yield break;
            
            List<FileStorageEntry> versions = History[key];

            foreach (FileStorageEntry v in versions)
            {
                if (v.IsDir) continue;
                yield return new VersionData
                {
                    Key = key_most_fresh,
                    Name = v.Location.Remove(0, key_most_fresh.Length + 1),
                    Created = v.Created,
                    data = storage.GetContentRaw(v.Location)
                };
            }
        }
        public string GetLatestVersion(string key)
        {
            if (History.ContainsKey(key))
            {
                List<FileStorageEntry> versions = History[key];
                FileStorageEntry actual = (from v in versions
                                           where !v.IsDir
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
        public void AddVersion(string name, byte[] contents)
        {
            storage.UpdateContent(name, contents);
            PopulateContents();// TODO: just add to dictionary instant
        }
        string locPostfixNew(string key)
        {
            if (!History.ContainsKey(key))
            {
                return key + "/1_" + DateTime.UtcNow.ToString("dd.MM.yyyy_HH-mm-ss");
            }
            else
            {
                return key + "/" + (History[key].Count + 1) + "_" + DateTime.UtcNow.ToString("dd.MM.yyyy_HH-mm-ss");
            }
        }
        string KeyPrefix(string locFull)
        {
            int index = locFull.IndexOf('/');
            if (index > 0)
            {
                return locFull.Substring(0, index);
            }
            return locFull;
        }

        void PopulateContents()
        {
            History.Clear();
            key_most_fresh = null;
            DateTime mostfresh = DateTime.MinValue;
            foreach (FileStorageEntry fi in storage.GetAllEntrys())
            {
                if (fi.Created > mostfresh)
                {
                    mostfresh = fi.Created;
                    key_most_fresh = KeyPrefix(fi.Location);
                }
                string k = KeyPrefix(fi.Location);
                if (!History.ContainsKey(k))
                {
                    History.Add(k, new List<FileStorageEntry>() { fi });
                }
                else
                {
                    History[k].Add(fi);
                }
            }
        }
    }
}
