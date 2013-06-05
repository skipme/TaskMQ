using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker;
using TaskQueue.Providers;

namespace EmailConsumer
{
    public class ModConsumer : IModConsumer
    {
        public static TaskBroker.Broker broker;

        public bool Push(TItemModel parameters, ref TaskMessage q_parameter)
        {
            //Send
            SmtpModel smtp_p = new SmtpModel(parameters);
            MailModel mail = new MailModel(q_parameter);

            // send email
            bool result = Sender.Send(mail, smtp_p);

            return result;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void Initialise(Broker brokerInterface, ModMod thisModule)
        {
            broker = brokerInterface;
            broker.RegistarateMessageModel(new TaskBroker.MessageType(new EmailConsumer.MailModel()));

            thisModule.UniqueName = "EmailSender";
            thisModule.Description = "Email common sender";
            thisModule.InvokeAs = TaskBroker.ExecutionType.Consumer;
            thisModule.AcceptsModel = new TaskQueue.QueueItemModel(typeof(MailModel));
            thisModule.Parameters = new TaskQueue.QueueItemModel(typeof(SmtpModel));
            //thisModule.Consumer = ModConsumer.Send;
        }

    }
}
