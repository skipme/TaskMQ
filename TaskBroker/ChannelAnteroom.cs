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
        
        public bool Push(TaskQueue.ITItem item)
        {
            try
            {
                Queue.Push(item);
            }
            catch (Exception e)
            {
                Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        public bool Update(TaskQueue.ITItem item)
        {
            try
            {
                Queue.UpdateItem(item);
            }
            catch (Exception e)
            {
                Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
                return false;
            }
            return true;
        }
        public long CountNow
        {
            get
            {
                return Queue.GetQueueLength(selector);
            }
        }
        public TaskQueue.ITItem Next()
        {
            lock (anteroom)
            {
                // the internal tuple is empty
                if (anteroom.Count == 0)
                {
                    TaskQueue.ITItem[] items = null;
                    try
                    {
                        items = Queue.GetItemTuple(selector);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
                        return null;
                    }

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
