using System;
using System.Collections.Generic;
using TaskQueue.Providers;

namespace TaskUniversum
{
    /// <summary>
    /// Absolute module interface, this interface skipped by broker while configuration, use IModConsumer, IModProducer or IModIsolatedProducer
    /// </summary>
    public interface IMod
    {
        void Exit();
        void Initialise(IBroker context, IBrokerModule moduleInstance);
        Task[] RegisterTasks(IBrokerModule moduleInstance);
        
        string Name { get; }
        string Description { get; }
    }
    /// <summary>
    /// Consumer module interface, invokes by execution plan and not empty queue
    /// </summary>
    public interface IModConsumer : IMod
    {
        /// <summary>
        /// Main consumer job entry
        /// </summary>
        /// <param name="optionalParameters">Represent worker parameters, for example -- smtp server connection string</param>
        /// <param name="message">Message taken from queue, sent by client</param>
        /// <returns></returns>
        bool Push(Dictionary<string, object> optionalParameters, ref TaskQueue.Providers.TaskMessage message);
        /// <summary>
        /// Consist a parameters model which passed to consumer by platform configuration
        /// </summary>
        TaskQueue.Providers.TItemModel ParametersModel
        {
            get;
        }
        /// <summary>
        /// Consist a message model, this could be used in validation
        /// </summary>
        TaskQueue.Providers.TItemModel AcceptsModel
        {
            get;
        }
    }
    /// <summary>
    /// Use it if module executing only by scheduler plan<br/>plan configured by tasks in broker
    /// </summary>
    public interface IModProducer : IMod
    {
        void Iterate(TItemModel parameters);
    }
    /// <summary>
    /// Use it if module role required isolated thread for prevent main loop suspending
    /// </summary>
    public interface IModIsolatedProducer : IMod
    {
        void IsolatedProducer(Dictionary<string, object> parameters);
        void IsolatedProducerStop();
    }
}
