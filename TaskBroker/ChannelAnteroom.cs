using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public class ChannelAnteroom
    {
        public ChannelAnteroom(TaskQueue.TQItemSelector selector)
        {
            this.selector = selector;
            anteroom = new Queue<TaskQueue.ITItem>();
        }
        public Queue<TaskQueue.ITItem> anteroom { get; set; }
        public TaskQueue.ITQueue Queue { get; set; }
        public TaskQueue.TQItemSelector  selector { get; set; }
        
        public void Push(TaskQueue.ITItem item)
        {
            Queue.Push(item);
        }

        public void Update(TaskQueue.ITItem item)
        {
            Queue.UpdateItem(item);
        }
        public TaskQueue.ITItem Next()
        {
            lock (anteroom)
            {
                // the internal tuple is empty
                if (anteroom.Count == 0)
                {
                    TaskQueue.ITItem[] items = Queue.GetItemTuple(selector);
                    if (items.Length > 0)
                    {
                        for (int i = 1; i < items.Length; i++)
                        {
                            anteroom.Enqueue(items[i]);
                        }
                        return items[0];
                    }
                    // the channel queue is empty
                    return null;
                }
                else
                {
                    return anteroom.Dequeue();
                }
            }
        }
    }
}
