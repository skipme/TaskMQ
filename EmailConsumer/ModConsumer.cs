using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using TaskUniversum;
using TaskUniversum.Task;

namespace EmailConsumer
{
    public class ModConsumer : IModConsumer
    {
        Sender emailSender;

        public bool Push(Dictionary<string, object> parameters, ref TaskMessage q_parameter)
        {
            System.Threading.Thread.Sleep(10);
            return true;
            //Send
            SmtpModel smtp_p = new SmtpModel(parameters);
            MailModel mail = new MailModel(q_parameter);

            // send email
            bool result = emailSender.Send(mail, smtp_p);

            if (mail.SendErrors > 5)
                result = true;
            q_parameter = mail;
            return result;
        }

        public void Exit()
        {

        }

        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
            emailSender = new Sender(context.APILogger());
        }

        public string Name
        {
            get { return "EmailSender"; }
        }

        public string Description
        {
            get { return "Email common sender"; }
        }


        public MetaTask[] RegisterTasks(IBrokerModule thisModule)
        {
            return null;
        }

        public TaskQueue.Providers.TItemModel ParametersModel
        {
            get { return new SmtpModel(); }
        }

        public TaskQueue.Providers.TItemModel AcceptsModel
        {
            get { return new MailModel(); }
        }


        public TaskQueue.TQItemSelector ConfigureSelector()
        {
            return new TaskQueue.TQItemSelector("Processed", false, true)
                .Rule("SendErrors", TaskQueue.TQItemSelectorSet.Ascending);
        }
    }
}
