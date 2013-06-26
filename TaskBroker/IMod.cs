using System;
using TaskQueue.Providers;

namespace TaskBroker
{
    public interface IMod
    {
        void Exit();
        void Initialise(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule);
        void RegisterTasks(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule);  
    }
    public interface IModConsumer : IMod
    {
        bool Push(TItemModel parameters, ref TaskQueue.Providers.TaskMessage q_parameter);
    }
    public interface IModProducer : IMod
    {
        void Iterate(TItemModel parameters);
    }
    public interface IModIsolatedProducer : IMod
    {
        void IsolatedProducer(TItemModel parameters);
    }
}
