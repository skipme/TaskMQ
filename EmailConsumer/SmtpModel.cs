using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailConsumer
{
    public class SmtpModel : TaskQueue.Providers.TItemModel
    {
        public SmtpModel() { }
        public SmtpModel(Dictionary<string, object> holder)
        {
            SetHolder(holder);
        }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        [TaskQueue.TQModelProp("Use secure connection over ssl layer")]
        public bool UseSSL { get; set; }
        [TaskQueue.TQModelProp("Use custom secure certificate checking by serial number only(ssl)")]
        public bool OverrideCertificateSecurity { get; set; }
        [TaskQueue.TQModelProp("See OverrideCertificateSecurity prop")]
        public string OverrideX509CertificateSerial { get; set; }
    }
}
