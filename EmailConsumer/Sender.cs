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
                if (model.From != null)
                    msg.From = new MailAddress(model.From, model.FromAlias);
                else msg.From = new MailAddress(loginAddress(con.Login, con.Server));
                msg.Body = model.Body;
                msg.IsBodyHtml = true;
                msg.Subject = model.Subject;

                return Send(msg, con);
            }
            catch (Exception e)
            {
                Console.WriteLine("Email sender:: " + e.Message);
                return false;
            }
        }
        static string loginAddress(string login, string server)
        {
            if (login.Contains('@'))
                return login;

            string result = login + '@';
            string[] ds = server.Split('.');
            for (int i = 1; i < ds.Length; i++)
            {
                result += ds[i] + (i == ds.Length - 1 ? "" : ".");
            }
            return result;
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
