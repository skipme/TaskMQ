using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class InMemoryQueue
    {
        public class SomeExtMessage : TaskQueue.Providers.TaskMessage
        {
            const string MType = "_MSGTYPE_TEST";
            public SomeExtMessage()
                : base(MType)
            {

            }
            public SomeExtMessage(TaskQueue.Providers.TaskMessage holder)
                : base(holder.MType)
            {
                this.SetHolder(holder.GetHolder());
            }
            public SomeExtMessage(Dictionary<string, object> holder)
                : base(MType)
            {
                this.SetHolder(holder);
            }
            public int field1 { get; set; }
            public long field2 { get; set; }
        }
        public delegate int InternalComparable(SomeExtMessage one, SomeExtMessage other);
        [Test]
        public void MemQueue_Fifo()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector();
            mq.Push(new SomeExtMessage() { field1 = 1 });
            mq.Push(new SomeExtMessage() { field1 = 2 });
            mq.Push(new SomeExtMessage() { field1 = 3 });
            Assert.AreEqual(mq.GetItem().Holder["field1"], 1);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 2);
            TaskQueue.Providers.TaskMessage first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);// push will be ignored by Processed flag
            mq.Push(new SomeExtMessage() { field1 = 4 });
            Assert.AreEqual(mq.GetItem().Holder["field1"], 4);
            mq.Push(new SomeExtMessage() { field1 = 5 });
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            mq.Push(new SomeExtMessage() { field1 = 6 });
            Assert.AreEqual(mq.GetItem().Holder["field1"], 6);
            Assert.AreEqual(mq.GetItem(), null);
        }
        [Test]
        public void MemQueue_CustomOrder()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector(
                new TaskQueue.TQItemSelector("Processed", false)
                .Rule("field1", TaskQueue.TQItemSelectorSet.Descending)
                );
            mq.Push(new SomeExtMessage() { field1 = 1 });
            mq.Push(new SomeExtMessage() { field1 = 3 });
            mq.Push(new SomeExtMessage() { field1 = 2 });       
            Assert.AreEqual(mq.GetItem().Holder["field1"], 3);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 2);
            TaskQueue.Providers.TaskMessage first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            mq.Push(new SomeExtMessage() { field1 = 8 });
            mq.Push(new SomeExtMessage() { field1 = 9 });
            mq.Push(new SomeExtMessage() { field1 = 10 }); 
            Assert.AreEqual(mq.GetItem().Holder["field1"], 10);
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 8);
            Assert.AreEqual(mq.GetItem(), null);
        }
        [Test]
        public void MemQueue_Duplications()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector(
                new TaskQueue.TQItemSelector("Processed", false)
                .Rule("field1", TaskQueue.TQItemSelectorSet.Descending)
                );
            mq.Push(new SomeExtMessage() { field1 = 1 });
            mq.Push(new SomeExtMessage() { field1 = 3 });
            mq.Push(new SomeExtMessage() { field1 = 2 });
            mq.Push(new SomeExtMessage() { field1 = 1 });
            Assert.AreEqual(mq.GetItem().Holder["field1"], 3);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 2);
            TaskQueue.Providers.TaskMessage first = mq.GetItem();
            first.Processed = false;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 1);
        }
        [Test]
        public void MemQueue_Update()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector(
                new TaskQueue.TQItemSelector("Processed", false)
                .Rule("field1", TaskQueue.TQItemSelectorSet.Descending)
                );
            SomeExtMessage msg = new SomeExtMessage { field1 = 1 };
            mq.Push(msg);
            Assert.AreEqual(mq.GetQueueLength(), 1);
            mq.UpdateItem(mq.GetItem());
            Assert.AreEqual(mq.GetQueueLength(), 1);
        }
    }
}
