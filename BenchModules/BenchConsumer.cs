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

        [TaskQueue.FieldDescription("sleep thread for msecs", true, DefaultValue = 0)]
        public int WaitThreadMS { get; set; }

    }
    public class ModConsumer : IModConsumer
    {
        public const string ModuleName = "Benchmark_common";
        ILogger logger;
        public bool Push(Dictionary<string, object> parameters, ref TaskMessage q_parameter)
        {
            //Console.WriteLine(q_parameter.AddedTime.ToString());
            //return false;
            BenchModel pocket = new BenchModel(q_parameter);
            pocket.ParameterA = "setA";
            pocket.ParameterB = "setB";
            if (pocket.WaitThreadMS > 0)//100 secmax
            {
                if (pocket.WaitThreadMS < 100000)
                    System.Threading.Thread.Sleep(pocket.WaitThreadMS);
                else
                    logger.Warning("message has try to wait thread for more than 100sec, check arguments");
            }

            //q_parameter = pocket; // for persistant only queues
            //return true;

            return false;
        }

        public void Exit()
        {

        }

        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
            logger = context.APILogger();
        }

        public string Name
        {
            get { return ModuleName; }
        }

        public string Description
        {
            get { return "Benchmark module sets a and b parameters within model, suspend channel thread for a \"WaitThreadMS\" milliseconds param in message"; }
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
