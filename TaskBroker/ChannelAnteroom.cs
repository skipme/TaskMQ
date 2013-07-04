using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class ChannelAnteroom
    {
        public string ChannelName { get; set; }
        public ChannelAnteroom(TaskQueue.TQItemSelector selector)
        {
            this.selector = selector;
            anteroom = new Queue<TaskQueue.Providers.TaskMessage>();
        }
        public Queue<TaskQueue.Providers.TaskMessage> anteroom { get; set; }
        public TaskQueue.ITQueue Queue { get; set; }
        public TaskQueue.TQItemSelector  selector { get; set; }

        public bool Push(TaskMessage item)
        {
            try
            {
                Queue.Push(item);
            }
            catch (Exception e)
            {
                Console.WriteLine("anteroom push error {0}", e.Message);
                return false;
            }
            return true;
        }

        public bool Update(TaskMessage item)
        {
            try
            {
                Queue.UpdateItem(item);
            }
            catch (Exception e)
            {
                Console.WriteLine("anteroom update error {0}", e.Message);
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
        public TaskQueue.Providers.TaskMessage Next()
        {
            lock (anteroom)
            {
                // the internal tuple is empty
                if (anteroom.Count == 0)
                {
                    TaskQueue.Providers.TaskMessage[] items = null;
                    try
                    {
                        items = Queue.GetItemTuple(selector);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("anteroom take error: {0}", e.Message);
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
