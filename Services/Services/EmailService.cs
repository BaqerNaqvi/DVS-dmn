using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;


namespace Services.Services
{
    public static class EmailService
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                const string fromEmail = "support@kamsham.pk";
                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    To = { toEmail },
                    Subject = subject,
                    Body = body,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                };
                using (SmtpClient smtpClient = new SmtpClient("webmail.kamsham.pk"))
                {
                    smtpClient.Credentials = new NetworkCredential("support@kamsham.pk", "vakR69~0");
                    smtpClient.Port = 25;
                    smtpClient.EnableSsl = false;
                    smtpClient.Send(message);
                }
            }
            catch (Exception ffg)
            {

            }
        }

        public static string SendSms(string mobile, string contents)
        {
            const string url = "http://www.sms4connect.com/api/sendsms.php/sendsms/url";
            String result = "";
            String message = System.Web.HttpUtility.UrlEncode(contents);
            String strPost = "id=kissaneng&pass=pakistan6&msg=" + message +
                             "&to=" + mobile + "&mask=KamShamHelp&type=json&lang=English";
            // "&to=923084449991" + "&mask=SMS4CONNECT&type=xml&lang=English";
            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Method = "POST";
            objRequest.ContentLength = Encoding.UTF8.GetByteCount(strPost);
            objRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(strPost);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                myWriter.Close();
            }
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            return result;
        }
    }
}
