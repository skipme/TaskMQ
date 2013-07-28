using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileContentArchive
{
    public class ZipStorage : FileContentArchive.IFileStorage
    {
        //ZipFile zipFile;
        string FileLoc;

        public ZipStorage(string fileloc)
        {
            this.FileLoc = fileloc;
            if (!CheckFile())
            {
                if (ResetFile())
                {
                    Console.WriteLine("zip reset ok...");
                }
            }
        }
        public string GetContent(string loc)
        {
            string cont = null;
            ZipFile zipFile = new ZipFile(FileLoc);
            int i = zipFile.FindEntry(loc, false);
            if (i >= 0)
            {
                ZipEntry ze = zipFile[i];
                Stream s = zipFile.GetInputStream(ze);
                byte[] buff = new byte[(int)ze.Size];
                s.Read(buff, 0, buff.Length);
                cont = Encoding.Unicode.GetString(buff);
            }
            zipFile.Close();
            return cont;
        }
        public void UpdateContent(string loc, string content)
        {
            string c = GetContent(loc);
            if (c != null)
            {
                DeleteContent(loc);
            }
            SetContent(loc, content);
        }
        void SetContent(string loc, string content)
        {
            ZipFile zipFile = new ZipFile(FileLoc);

            // Must call BeginUpdate to start, and CommitUpdate at the end.
            zipFile.BeginUpdate();

            CustomStaticDataSource sds = new CustomStaticDataSource();
            sds.SetStream(new MemoryStream(Encoding.Unicode.GetBytes(content)));

            // If an entry of the same name already exists, it will be overwritten; otherwise added.
            zipFile.Add(sds, loc);

            // Both CommitUpdate and Close must be called.
            zipFile.CommitUpdate();
            zipFile.Close();
        }
        void DeleteContent(string loc)
        {
            ZipFile zipFile = new ZipFile(FileLoc);

            // Must call BeginUpdate to start, and CommitUpdate at the end.
            zipFile.BeginUpdate();
            zipFile.Delete(loc);
            // Both CommitUpdate and Close must be called.
            zipFile.CommitUpdate();
            zipFile.Close();
        }
        bool CheckFile()
        {
            if (File.Exists(FileLoc))
            {
                try
                {
                    ZipFile zipFile = new ZipFile(FileLoc);
                    bool v = zipFile.TestArchive(false);
                    // Must call BeginUpdate to start, and CommitUpdate at the end.
                    //zipFile.BeginUpdate();
                    zipFile.Close();

                    return v;
                }
                catch (Exception e)
                {
                    Console.WriteLine("zip corrupted...");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        bool ResetFile()
        {
            try
            {
                FileStream fsOut = File.Create(FileLoc);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);

                zipStream.SetLevel(2); //0-9, 9 being the highest level of compression



                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                zipStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("can't reset zip archive...");
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
            ZipFile zipFile = new ZipFile(FileLoc);
            foreach (ZipEntry e in zipFile)
            {
                es.Add(new FileStorageEntry()
                    {
                        Created = e.DateTime,
                        Location = e.Name
                    });
            }

            zipFile.Close();

            return es.ToArray();
        }
    }
}
