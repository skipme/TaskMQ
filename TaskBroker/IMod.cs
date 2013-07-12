using System;
using System.Collections.Generic;
using TaskQueue.Providers;

namespace TaskBroker
{

    public interface IMod
    {
        void Exit();
        void Initialise(Broker context, TaskBroker.ModMod thisModule);
        ModuleSelfTask[] RegisterTasks(TaskBroker.ModMod thisModule);
        
        string Name { get; }
        string Description { get; }
    }
    public interface IModConsumer : IMod
    {
        bool Push(Dictionary<string, object> parameters, ref TaskQueue.Providers.TaskMessage q_parameter);
        TaskQueue.Providers.TItemModel ParametersModel
        {
            get;
        }
        TaskQueue.Providers.TItemModel AcceptsModel
        {
            get;
        }
    }
    public interface IModProducer : IMod
    {
        void Iterate(TItemModel parameters);
    }
    public interface IModIsolatedProducer : IMod
    {
        void IsolatedProducer(Dictionary<string, object> parameters);
        void IsolatedProducerStop();
    }
}
