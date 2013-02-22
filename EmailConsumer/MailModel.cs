using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailConsumer
{
    public class MailModel : TaskQueue.Providers.TaskMessage
    {
        public MailModel(Dictionary<string, object> holder)
            : base("EMail")
        {
            this.SetHolder(holder);
        }
        public string From { get; set; }
        public string FromAlias { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}
