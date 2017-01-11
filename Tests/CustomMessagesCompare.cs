using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class CustomMessagesCompare
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
        public void Compare()
        {
            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", TaskQueue.TQItemSelectorSet.Descending);
            SomeExtMessage inst = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 15
            };
            InternalComparable cmp = (InternalComparable)inst.MakeComparator(sel, typeof(InternalComparable)/*, typeof(SomeExtMessage)*/);
            SomeExtMessage inst_CE = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 18
            };
            SomeExtMessage inst_CL = new SomeExtMessage()
            {
                field1 = 4,
                field2 = 18
            };

            Assert.AreEqual(cmp(inst, inst_CE), -1);
            Assert.AreEqual(cmp(inst, inst_CL), 1);
            Assert.AreEqual(cmp(inst_CL, inst_CE), -1);
        }
        [Test]
        public void CompareDict()
        {
            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", TaskQueue.TQItemSelectorSet.Descending);
            SomeExtMessage inst = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 15
            };
            TaskQueue.Providers.TaskMessage.InternalComparableDictionary cmp = (TaskQueue.Providers.TaskMessage.InternalComparableDictionary)
                TaskQueue.Providers.TaskMessage.MakeComparatorDictionary(sel);
            SomeExtMessage inst_CE = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 18
            };
            SomeExtMessage inst_CL = new SomeExtMessage()
            {
                field1 = 4,
                field2 = 18
            };

            Assert.AreEqual(cmp(inst.GetHolder(), inst_CE.GetHolder()), -1);
            Assert.AreEqual(cmp(inst.GetHolder(), inst_CL.GetHolder()), 1);
            Assert.AreEqual(cmp(inst_CL.GetHolder(), inst_CE.GetHolder()), -1);
        }
        [Test]
        public void CheckDict()
        {
            TaskQueue.TQItemSelector sel = new
                TaskQueue.TQItemSelector("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", 15L, true);
            SomeExtMessage inst = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 15
            };
            TaskQueue.Providers.TaskMessage.InternalCheckDictionary chk = (TaskQueue.Providers.TaskMessage.InternalCheckDictionary)
                TaskQueue.Providers.TaskMessage.MakeCheckerDictionary(sel);
            SomeExtMessage inst_CE = new SomeExtMessage()
            {
                field1 = 5,
                field2 = 18
            };

            Assert.AreEqual(chk(inst.GetHolder()), true);
            Assert.AreEqual(chk(inst_CE.GetHolder()), false);

            Assert.AreEqual(chk(new Dictionary<string, object>() { { "field1", 5 }, { "field2", 15 } }), true);
            Assert.AreEqual(chk(new Dictionary<string, object>() { { "field1", 5 }}), false);
        }
    }
}
