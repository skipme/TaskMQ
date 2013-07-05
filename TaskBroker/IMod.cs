using System;
using TaskQueue.Providers;

namespace TaskBroker
{

    public interface IMod
    {
        void Exit();
        void Initialise(TaskBroker.ModMod thisModule);
        ModuleSelfTask[] RegisterTasks(TaskBroker.ModMod thisModule);
        
        string Name { get; }
        string Description { get; }
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
        void IsolatedProducerStop();
    }
}
