using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EmailConsumer
{
    public class Sender
    {
        public static bool Send(MailModel model, SmtpModel con)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(model.To));
                msg.From = new MailAddress(model.From, model.FromAlias);
                msg.Body = model.Body;
                msg.IsBodyHtml = true;
                msg.Subject = model.Subject;

                return Send(msg, con);
            }
            catch (Exception e)
            {

                return false;
            }
        }
        public static bool Send(MailMessage message, SmtpModel con)
        {
            if (con.OverrideCertificateSecurity)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    if (con.OverrideX509CertificateSerial == certificate.GetSerialNumberString())
                        return true;
                    return false;
                };
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
            SmtpClient SmtpServer = new SmtpClient(con.Server);
            SmtpServer.Port = con.Port;
            SmtpServer.Credentials = new System.Net.NetworkCredential(con.Login, con.Password);
            SmtpServer.EnableSsl = con.UseSSL;

            SmtpServer.Send(message);

            return true;
        }
    }
}
