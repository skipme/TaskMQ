using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;
    using TaskQueue.Providers;

    //[TestFixture]
    //public class MongoQueueTest
    //{
    //    private class MSGT : TaskMessage
    //    {
    //        public const string Name = "testMSG";
    //        public MSGT() : base(Name) { }

    //        public string someExtProp { get; set; }
    //    }

    //    [Test]
    //    public void Insert_Update()
    //    {
    //        MongoQueue.MongoDbQueue q = new MongoQueue.MongoDbQueue();
    //        q.InitialiseFromModel(null, new TaskQueue.Providers.QueueConnectionParameters()
    //            {
    //                Database = "test",
    //                Collection = "queuetests",
    //                ConnectionString = "mongodb://localhost:27017/"
    //            });
    //        q.Purge();
    //        MSGT tmi = new MSGT()
    //          {
    //              someExtProp = "Insert"
    //          };
    //        q.Push(tmi);
    //        MSGT tmTi = q.GetItemFifo<MSGT>();

    //        Assert.AreEqual(tmTi.someExtProp, tmi.someExtProp);
    //        // ok update
    //        tmTi.someExtProp= "Update";
    //        q.UpdateItem(tmTi);

    //        MSGT tmTu = q.GetItemFifo<MSGT>();
    //        Assert.AreEqual(tmTu.someExtProp, "Update");
    //    }
    //}
}
