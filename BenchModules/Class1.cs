using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using TaskUniversum;
using TaskUniversum.Task;

namespace BenchModules
{
    public class BenchModel : TaskQueue.Providers.TaskMessage
    {
        public const string Name = "benchMessage";
        public BenchModel()
            : base(Name)
        {

        }
        public BenchModel(TaskQueue.Providers.TaskMessage holder)
            : base(holder.MType)
        {
            this.SetHolder(holder.GetHolder());
        }
        public BenchModel(Dictionary<string, object> holder)
            : base(Name)
        {
            this.SetHolder(holder);
        }
        [TaskQueue.FieldDescription("random parameter for consumer to set @", true)]
        public string ParameterA { get; set; }
        [TaskQueue.FieldDescription("random parameter for consumer to set", true)]
        public string ParameterB { get; set; }

    }
    public class ModConsumer : IModConsumer
    {

        public bool Push(Dictionary<string, object> parameters, ref TaskMessage q_parameter)
        {
            BenchModel pocket = new BenchModel(q_parameter);
            pocket.ParameterA = "setA";
            pocket.ParameterB = "setB";
            //System.Threading.Thread.Sleep(2000);
            
            //q_parameter = pocket; // for persistant only queues
            //return true;

            return false;
        }

        public void Exit()
        {

        }

        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
       
        }

        public string Name
        {
            get { return "Benchmark pAB"; }
        }

        public string Description
        {
            get { return "Benchmark module just sets a and b parameters within model"; }
        }


        public MetaTask[] RegisterTasks(IBrokerModule thisModule)
        {
            return null;
        }

        public TaskQueue.Providers.TItemModel ParametersModel
        {
            get { return null; }
        }

        public TaskQueue.Providers.TItemModel AcceptsModel
        {
            get { return new BenchModel(); }
        }


        public TaskQueue.TQItemSelector ConfigureSelector()
        {
            return TaskQueue.TQItemSelector.DefaultFifoSelector;
        }
    }
}
