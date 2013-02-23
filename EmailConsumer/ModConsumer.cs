using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace EmailConsumer
{
    public class ModConsumer
    {
        public static TaskBroker.Broker broker;

        public static void Initialise(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule)
        {
            broker = brokerInterface;
            broker.RegistarateMessageModel(new TaskBroker.MessageType(new EmailConsumer.MailModel()));

            thisModule.UniqueName = "EmailSender";
            thisModule.Description = "Email common sender";
            thisModule.InvokeAs = TaskBroker.ExecutionType.Consumer;
            thisModule.AcceptedModel = new TaskQueue.QueueItemModel(typeof(MailModel));
            thisModule.AcceptedParameters = new TaskQueue.QueueItemModel(typeof(SmtpModel));
            thisModule.Consumer = ModConsumer.Send;
        }
        public static bool Send(TItemModel parameters, ref TaskMessage q_parameter)
        {
            SmtpModel smtp_p = new SmtpModel(parameters);
            MailModel mail = new MailModel(q_parameter);

            // send email
            bool result = Sender.Send(mail, smtp_p);

            return result;
        }
        public static void Exit()
        {
        }
    }
}
