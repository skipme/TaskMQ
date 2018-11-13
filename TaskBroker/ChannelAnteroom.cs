﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.Linq;
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

        private readonly object anteroomSync = new object();

        public ChannelAnteroom()
        {
            anteroom = new Queue<TaskQueue.Providers.TaskMessage>();
            //anteroom = new ConcurrentQueue<TaskMessage>();
        }
        public Queue<TaskQueue.Providers.TaskMessage> anteroom;
        //public ConcurrentQueue<TaskQueue.Providers.TaskMessage> anteroom;
        public TaskQueue.ITQueue Queue;
        /// <summary>
        /// True: queue is empty
        /// </summary>
        public volatile bool InternalEmptyFlag;
        /// <summary>
        /// True: queue is hit the limit (out of memory exception for non persistant queues)
        /// </summary>
        public volatile bool InternalOverflowFlag;
        /// <summary>
        /// Set what channel queue have items
        /// </summary>
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

            catch (QueueOverflowException e)
            {
                logger.Exception(e, "Message Push", "anteroom push overflow queue error: {0}", e.baseException.Message);
                InternalOverflowFlag = true;
                return false;
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
            lock (anteroomSync)
            {
                // the internal tuple is empty
                if (anteroom.Count == 0)
                {
                    TaskQueue.Providers.TaskMessage result = Queue.GetItem();
                    if (result != null)
                    {
                        long qlen = Queue.GetQueueLength();
                        if (qlen >= 100)
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
                                anteroom = new Queue<TaskMessage>(items);
                                //anteroom = new ConcurrentQueue<TaskMessage>(items);
                                return result;
                            }
                            // the channel queue is empty
                            InternalEmptyFlag = true;
                            return null;
                        }
                        InternalEmptyFlag = result == null;
                        return result;
                    }
                    else
                    {
                        InternalEmptyFlag = true;
                        return null;
                    }
                }
                else
                {
                    TaskQueue.Providers.TaskMessage result = anteroom.Dequeue();
                    //TaskQueue.Providers.TaskMessage result;
                    //anteroom.TryDequeue(out result);
                    InternalEmptyFlag = result == null;
                    return result;
                }
            }
        }
        /// <summary>
        /// processed messages
        /// </summary>
        public BrokerStat ChannelStatsOut;
        /// <summary>
        /// pushed messages to queue
        /// </summary>
        public BrokerStat ChannelStatsIn;
    }
}
