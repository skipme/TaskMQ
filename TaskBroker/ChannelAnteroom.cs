using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker.Statistics;
using TaskQueue.Providers;
using TaskUniversum;

namespace TaskBroker
{
    public class ChannelAnteroom
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public string ChannelName { get; set; }
        public BrokerStat ChannelStatistic;

        public ChannelAnteroom()
        {
            anteroom = new Queue<TaskQueue.Providers.TaskMessage>();
        }
        public Queue<TaskQueue.Providers.TaskMessage> anteroom;
        public TaskQueue.ITQueue Queue;

        public bool InternalEmptyFlag { get; private set; }
        public void ResetEmptyFlag()
        {
            InternalEmptyFlag = false;
        }

        public bool Push(TaskMessage item)
        {
            try
            {
                Queue.Push(item);
                InternalEmptyFlag = false;
            }
            catch (Exception e)
            {
                logger.Exception(e, "Message Push", "anteroom push error");
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
                logger.Exception(e, "Message Update", "anteroom update error");
                return false;
            }
            return true;
        }
        public long CountNow
        {
            get
            {
                return Queue.GetQueueLength();
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
                        items = Queue.GetItemTuple();
                    }
                    catch (Exception e)
                    {
                        logger.Exception(e, "Message Take", "anteroom take error");
                        return null;
                    }

                    if (items != null && items.Length > 0)
                    {
                        for (int i = 1; i < items.Length; i++)
                        {
                            anteroom.Enqueue(items[i]);
                        }
                        return items[0];
                    }
                    // the channel queue is empty
                    InternalEmptyFlag = true;
                    return null;
                }
                else
                {
                    TaskQueue.Providers.TaskMessage result = anteroom.Dequeue();
                    InternalEmptyFlag = result == null;
                    return result;
                }
            }
        }
    }
}
