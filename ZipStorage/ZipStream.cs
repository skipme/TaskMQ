using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskUniversum;

namespace FileContentArchive
{
    public class ZipStream : FileContentArchive.IFileStorage
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();
        Stream zipArchive;

        public ZipStream(Stream stream)
        {
            this.zipArchive = stream;
            if (!CheckArchive())
            {
                if (!ResetFile())
                {
                    logger.Debug("zip reset failure...");
                }
            }
        }
        public ZipStream(byte[] zipdata)
            : this(new MemoryStream(zipdata))
        {

        }
        public void Close()
        {
            zipArchive.Close();
        }
        public byte[] GetContentRaw(string loc)
        {
            byte[] cont = null;
            ZipFile zipFile = new ZipFile(this.zipArchive);
            int i = zipFile.FindEntry(loc, false);
            if (i >= 0)
            {
                ZipEntry ze = zipFile[i];
                Stream s = zipFile.GetInputStream(ze);
                byte[] buff = new byte[(int)ze.Size];
                s.Read(buff, 0, buff.Length);
                cont = buff;
            }
            zipFile.IsStreamOwner = false; zipFile.Close();
            this.zipArchive.Position = 0;
            return cont;
        }
        public byte[] GetContentRaw(string loc, out DateTime CreationTime)
        {
            byte[] cont = null; CreationTime = DateTime.MinValue;
            ZipFile zipFile = new ZipFile(this.zipArchive);

            int i = zipFile.FindEntry(loc, false);
            if (i >= 0)
            {
                ZipEntry ze = zipFile[i];
                CreationTime = ze.DateTime;
                Stream s = zipFile.GetInputStream(ze);
                byte[] buff = new byte[(int)ze.Size];
                s.Read(buff, 0, buff.Length);
                cont = buff;
            }
            zipFile.IsStreamOwner = false; zipFile.Close();

            this.zipArchive.Position = 0;
            return cont;
        }
        public string GetContent(string loc)
        {
            return Encoding.Unicode.GetString(GetContentRaw(loc));
        }
        public void UpdateContent(string loc, string content)
        {
            SetContent(loc, content);
        }
        public void UpdateContent(string loc, byte[] content)
        {
            SetContent(loc, content);
        }
        void SetContent(string loc, string content)
        {
            SetContent(loc, Encoding.Unicode.GetBytes(content));
        }
        void AddDirectory(string loc)
        {
            ZipFile zipFile = new ZipFile(this.zipArchive);

            // Must call BeginUpdate to start, and CommitUpdate at the end.
            zipFile.BeginUpdate();
            zipFile.AddDirectory(loc);
            // Both CommitUpdate and Close must be called.
            zipFile.CommitUpdate();
            zipFile.IsStreamOwner = false; zipFile.Close();

            this.zipArchive.Position = 0;
        }
        void SetContent(string loc, byte[] content)
        {
            ZipFile zipFile = new ZipFile(this.zipArchive);

            // Must call BeginUpdate to start, and CommitUpdate at the end.
            zipFile.BeginUpdate();
            if (loc.Contains('/'))
            {
                int i = loc.IndexOf('/');
                string dir = loc.Remove(i + 1, loc.Length - i - 1);
                if (zipFile.FindEntry(dir, true) < 0)
                {
                    zipFile.AddDirectory(dir);
                }
            }
            CustomStaticDataSource sds = new CustomStaticDataSource();
            using (MemoryStream ms = new MemoryStream(content))
            {
                sds.SetStream(ms);

                // If an entry of the same name already exists, it will be overwritten; otherwise added.
                zipFile.Add(sds, loc);

                // Both CommitUpdate and Close must be called.
                zipFile.CommitUpdate();
                zipFile.IsStreamOwner = false; zipFile.Close();

                this.zipArchive.Position = 0;
            }
        }
        void DeleteContent(string loc)
        {
            ZipFile zipFile = new ZipFile(this.zipArchive);

            // Must call BeginUpdate to start, and CommitUpdate at the end.
            zipFile.BeginUpdate();
            zipFile.Delete(loc);
            // Both CommitUpdate and Close must be called.
            zipFile.CommitUpdate();
            zipFile.IsStreamOwner = false; zipFile.Close();


            this.zipArchive.Position = 0;
        }
        bool CheckArchive()
        {
            try
            {
                ZipFile zipFile = new ZipFile(this.zipArchive);
                bool v = zipFile.TestArchive(false);
                // Must call BeginUpdate to start, and CommitUpdate at the end.
                //zipFile.BeginUpdate();
                zipFile.IsStreamOwner = false; zipFile.Close();

                this.zipArchive.Position = 0;

                return v;
            }
            catch (Exception e)
            {
                logger.Exception(e, "CheckArchive", "zip corrupted...");
                return false;
            }
        }
        bool ResetFile()
        {
            try
            {
                this.zipArchive.SetLength(0);
                ZipOutputStream zipStream = new ZipOutputStream(this.zipArchive);

                zipStream.SetLevel(2); //0-9, 9 being the highest level of compression

                //zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                zipStream.IsStreamOwner = false; zipStream.Close();

                this.zipArchive.Position = 0;
            }
            catch (Exception e)
            {
                logger.Exception(e, "ResetFile", "can't reset zip archive...");
                return false;
            }
            return true;
        }

        public class CustomStaticDataSource : IStaticDataSource
        {
            private Stream _stream;
            // Implement method from IStaticDataSource
            public Stream GetSource()
            {
                return _stream;
            }

            // Call this to provide the memorystream
            public void SetStream(Stream inputStream)
            {
                _stream = inputStream;
                _stream.Position = 0;
            }
        }


        public FileStorageEntry[] GetAllEntrys()
        {
            List<FileStorageEntry> es = new List<FileStorageEntry>();
            ZipFile zipFile = new ZipFile(this.zipArchive);
            foreach (ZipEntry e in zipFile)
            {
                es.Add(new FileStorageEntry()
                    {
                        Created = e.DateTime,
                        Location = e.Name,
                        IsDir = e.IsDirectory
                    });
            }

            zipFile.IsStreamOwner = false; zipFile.Close();

            this.zipArchive.Position = 0;
            return es.ToArray();
        }
    }
}
