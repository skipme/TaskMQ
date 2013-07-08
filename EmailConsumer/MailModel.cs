using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailConsumer
{
    public class MailModel : TaskQueue.Providers.TaskMessage
    {
        public const string Name = "EMail";
        public MailModel()
            : base(Name)
        {

        }
        public MailModel(TaskQueue.Providers.TaskMessage holder)
            : base(holder.MType)
        {
            this.SetHolder(holder.GetHolder());
        }
        public MailModel(Dictionary<string, object> holder)
            : base(Name)
        {
            this.SetHolder(holder);
        }
        public string From { get; set; }
        public string FromAlias { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }

        public int SendErrors { get; set; }
        public string LastSError { get; set; }
    }
}
