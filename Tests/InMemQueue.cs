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
        public void Fifo()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector();
            mq.Push(new SomeExtMessage() { field1 = 1 });
            mq.Push(new SomeExtMessage() { field1 = 2 });
            mq.Push(new SomeExtMessage() { field1 = 3 });
            Assert.AreEqual(mq.GetItem().Holder["field1"], 1);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 1);
            TaskQueue.Providers.TaskMessage first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 2);
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 3);
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem(), null);
        }
        [Test]
        public void CustomOrder()
        {
            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector(
                new TaskQueue.TQItemSelector("Processed", true, false)
                .Rule("field1", TaskQueue.TQItemSelectorSet.Descending)
                );
            mq.Push(new SomeExtMessage() { field1 = 1 });
            mq.Push(new SomeExtMessage() { field1 = 3 });
            mq.Push(new SomeExtMessage() { field1 = 2 });       
            Assert.AreEqual(mq.GetItem().Holder["field1"], 3);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 3);
            TaskQueue.Providers.TaskMessage first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 2);
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem().Holder["field1"], 1);
            first = mq.GetItem();
            first.Processed = true;
            mq.UpdateItem(first);
            Assert.AreEqual(mq.GetItem(), null);
        }
    }
}
