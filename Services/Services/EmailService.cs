using Services.DbContext;
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
            var response = SendSMS(mobile, contents, "923466043805", "3186");
            if(response!=null && response.Contains("OK"))
            {
                using (var dbContext = new DeliversEntities()) {
                    var sms = new Sm {
                        Contetns= contents,
                        Time= CommonService.GetSystemTime(),
                        ToNumber=mobile
                    };
                    dbContext.Sms.Add(sms);
                    dbContext.SaveChanges();
                }
            }
            return response;
            //const string url = "http://www.sms4connect.com/api/sendsms.php/sendsms/url";
            //String result = "";
            //String message = System.Web.HttpUtility.UrlEncode(contents);
            //String strPost = "id=kissaneng&pass=pakistan6&msg=" + message +
            //                 "&to=" + mobile + "&mask=KamShamHelp&type=json&lang=English";
            //// "&to=923084449991" + "&mask=SMS4CONNECT&type=xml&lang=English";
            //StreamWriter myWriter = null;
            //HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            //objRequest.Method = "POST";
            //objRequest.ContentLength = Encoding.UTF8.GetByteCount(strPost);
            //objRequest.ContentType = "application/x-www-form-urlencoded";
            //try
            //{
            //    myWriter = new StreamWriter(objRequest.GetRequestStream());
            //    myWriter.Write(strPost);
            //}
            //catch (Exception e)
            //{
            //    return e.Message;
            //}
            //finally
            //{
            //    myWriter.Close();
            //}
            //HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            //using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            //{
            //    result = sr.ReadToEnd();
            //    // Close and clean up the StreamReader
            //    sr.Close();
            //}
            //return result;
        }

        public static string SendSMS(string toNumber, string MessageText, string MyUsername, string MyPassword)
        {
            String URI = "http://sendpk.com" +
            "/api/sms.php?" +
            "username=" + MyUsername +
            "&sender=" + "yolo" +
            "&password=" + MyPassword +
            "&mobile=" + toNumber +
            "&message=" + Uri.UnescapeDataString(MessageText); // Visual Studio 10-15 
            try
            {
                WebRequest req = WebRequest.Create(URI);
                WebResponse resp = req.GetResponse();
                var sr = new System.IO.StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                var httpWebResponse = ex.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            return "404:URL not found :" + URI;
                            break;
                        case HttpStatusCode.BadRequest:
                            return "400:Bad Request";
                            break;
                        default:
                            return httpWebResponse.StatusCode.ToString();
                    }
                }
            }
            return null;
        }
    }
}
