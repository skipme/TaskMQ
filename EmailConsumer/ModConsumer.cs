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

        }

        public void Initialise(ModMod thisModule)
        {
            thisModule.Role = TaskBroker.ExecutionType.Consumer;
            thisModule.AcceptsModel = new MailModel();
            thisModule.ParametersModel = new SmtpModel();
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
    }
}
