using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker;
using TaskQueue.Providers;

namespace QueueService
{
    public class MailModel : TaskQueue.Providers.TaskMessage
    {
        public const string Name = "EMail";
        public MailModel()
            : base(Name)
        {

        }
    }
    [Serializable]
    class ThroughputTest : MarshalByRefObject, IModConsumer
    {
        public bool Push(TItemModel parameters, ref TaskMessage q_parameter)
        {
            //Send

            return true;
        }

        public void Exit()
        {

        }

        public void Initialise(ModMod thisModule)
        {
            thisModule.Role = TaskBroker.ExecutionType.Consumer;
            thisModule.AcceptsModel = new MailModel();
        }

        public string Name
        {
            get { return "ThroughputTest"; }
        }

        public string Description
        {
            get { return "ThroughputTest"; }
        }


        public ModuleSelfTask[] RegisterTasks(ModMod thisModule)
        {
            return null;
        }
    }
}
