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
        public bool Push(Dictionary<string, object> parameters, ref TaskMessage q_parameter)
        {
            return true;
            //Send
            SmtpModel smtp_p = new SmtpModel(parameters);
            MailModel mail = new MailModel(q_parameter);

            // send email
            bool result = Sender.Send(mail, smtp_p);
            if (mail.SendErrors > 5)
                result = true;
            q_parameter = mail;
            return result;
        }

        public void Exit()
        {

        }

        public void Initialise(Broker context, ModMod thisModule)
        {
        }

        public string Name
        {
            get { return "EmailSender"; }
        }

        public string Description
        {
            get { return "Email common sender"; }
        }


        public ModuleSelfTask[] RegisterTasks(ModMod thisModule)
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
    }
}
