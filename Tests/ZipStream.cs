using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class ZipStreamTest
    {

        [Test]
        public void SetContent()
        {
            MemoryStream ms = new MemoryStream();
            FileContentArchive.ZipStream zip = new FileContentArchive.ZipStream(ms);
            zip.UpdateContent("testentry/subentry", "some content");

            string content = zip.GetContent("testentry/subentry");

            Assert.AreEqual(content, "some content");
        }
        [Test]
        public void OverwriteContent()
        {
            MemoryStream ms = new MemoryStream();
            FileContentArchive.ZipStream zip = new FileContentArchive.ZipStream(ms);
            zip.UpdateContent("testentry/subentry", "some content");
            zip.UpdateContent("testentry/subentry", "another content");
            string content = zip.GetContent("testentry/subentry");

            Assert.AreEqual(content, "another content");
        }
    }
}
