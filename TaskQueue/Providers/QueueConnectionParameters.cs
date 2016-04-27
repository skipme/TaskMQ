using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class QueueConnectionParameters
    {
        public QueueConnectionParameters(string name)
        {
            this.Name = name;
        }
        //Unique name for queue(important if persistant)
        public string Name { get; set; }
        
        ///// <summary>
        ///// table name
        ///// </summary>
        //public string Collection { get; set; }
        //public string Database { get; set; }
        //public string ConnectionString { get; set; }

        public string QueueTypeName;
        public TaskQueue.ITQueue QueueInstance;
        public QueueSpecificParameters specParams;

        public bool Temporary { get; set; }

        public void SetInstance(TaskQueue.ITQueue instance, QueueSpecificParameters parameters)
        {
            this.QueueTypeName = instance.QueueType;
            this.QueueInstance = instance;
#if DEBUG
            System.Diagnostics.Debug.Assert(instance.GetParametersModel().GetType() == parameters.GetType());
#endif
            this.specParams = parameters;

            string chkresult;
            if (!parameters.Validate(out chkresult))
                throw new Exception("Invalid parameters passed to queue: " + Name);
        }
    }
    public abstract class QueueSpecificParameters : TItemModel
    {
        public QueueSpecificParameters() { }
        public QueueSpecificParameters(TItemModel tm) : base(tm.GetHolder()) { }

        public abstract bool Validate(out string result);
    }
}
