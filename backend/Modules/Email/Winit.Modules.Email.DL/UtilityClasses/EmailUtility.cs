using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Shared.Models.Common;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace Winit.Modules.Email.DL.UtilityClasses
{
    public class EmailUtility
    {
        private readonly IConfiguration _configuration;
        private string? SMTPType { get; set; }
        private string? SMTPServer { get; set; }
        private string? UserName { get; set; }
        private string? PASSWORD { get; set; }
        private string? MailSenderName { get; set; }
        private string? FromEmailConfig { get; set; }
        private string? PORT { get; set; }
        private string? BasePhysicalPath { get; set; }
        public EmailUtility(IConfiguration configuration)
        {
            _configuration = configuration;
            BasePhysicalPath = _configuration.GetSection("AppSettings:BasePhysicalPath").Value;
            SMTPServer = _configuration.GetSection("MailSettings:SMTPServer").Value;
            SMTPType = _configuration.GetSection("MailSettings:SMTPType").Value;
            UserName = _configuration.GetSection("MailSettings:UserName").Value;
            PASSWORD = _configuration.GetSection("MailSettings:PASSWORD").Value;
            MailSenderName = _configuration.GetSection("MailSettings:SenderName").Value;
            FromEmailConfig = _configuration.GetSection("MailSettings:FromEmail").Value;
            PORT = _configuration.GetSection("MailSettings:PORT").Value;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
        public IList<Attachment> GetReportAttachment(string FilePath, string FileName)
        {
            IList<Attachment> attchments = null;
            Attachment attachment = null;
            attchments = new List<Attachment>();
            attachment = new Attachment(FilePath);
            attachment.Name = FileName;
            attchments.Add(attachment);
            return attchments;
        }
        public bool SendEmail(string fromEmail, string toEmail, string subject, string message, bool isHTML, ref string response, string cc, IList<Attachment> attachments)
        {
            MailMessage msg = new MailMessage(fromEmail, toEmail);
            SmtpClient objMail = new SmtpClient();
            objMail.Host = SMTPServer;
            /*if (GetIntValue(ConfigurationManager.AppSettings["Port"]) > 0)
                objMail.Port = GetIntValue(ConfigurationManager.AppSettings["Port"]);*/
            objMail.Port = Convert.ToInt32(PORT);
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = isHTML;

            if (attachments != null)
            {
                if (attachments.Count > 0)
                {
                    foreach (Attachment attachment in attachments)
                    {
                        msg.Attachments.Add(attachment);
                    }
                }
            }

            if (cc.Length > 0)
            {
                msg.CC.Add(new MailAddress(cc));
            }

            try
            {
                objMail.Send(msg);
            }
            catch (Exception exception)
            {
                response = exception.Message;
                return false;
            }

            return true;
        }
        public bool SendDefaultMail(IMailRequest MailDetails)
        {
            try
            {
                string FileName = MailDetails.MailFormat + ".html";
                string strcontent = string.Empty;
                strcontent = GetEmailTemplateContent(FileName);
                strcontent = strcontent.Replace("{{body}}", MailDetails.Content);
                //strcontent = strcontent.Replace("{{OrderDate}}", "01-01-2025");
                //strcontent = strcontent.Replace("{{DeliveryDate}}", "01-01-2025");
                //strcontent = strcontent.Replace("{{StockOrderNumber}}", "5610247");
                //strcontent = strcontent.Replace("{{StockReciptNumber}}", "8720145");
                StringBuilder ASNString = new StringBuilder(strcontent);
                return SendEmailUtils(Guid.NewGuid().ToString(), MailDetails.Receivers.FirstOrDefault().ToEmail, MailDetails.FromEmail, MailDetails.Receivers.FirstOrDefault().CcEmail, MailDetails.Subject, ASNString.ToString(), true, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public bool SendEmailUtils(string UID, string toEmail, string FromEmail, string CcEmail, string subject, string message, bool isHTML, IList<Attachment> attachments)
        {
            try
            {
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string fromEmail = FromEmail;
                if (!string.IsNullOrEmpty(FromEmail))
                    fromEmail = FromEmail;
                string smtpAddress = SMTPServer;
                string sMTPType = SMTPType;
                string userName = UserName;
                string password = PASSWORD;
                string SenderName = MailSenderName;
                int portNumber = Convert.ToInt16(PORT);
                bool enableSSL = true;
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, SenderName);
                    mail.To.Add(toEmail);
                    if (!string.IsNullOrEmpty(CcEmail))
                        mail.CC.Add(CcEmail);
                    if (subject != null && !subject.Contains("DO NOT REPLY"))
                        subject = subject + " DO NOT REPLY";
                    mail.Subject = subject;
                    mail.Body = message;
                    mail.IsBodyHtml = isHTML;

                    // mail.Bcc.Add("abcdf@gmail.com,aabbcc@gmail.com");
                    if (attachments != null)
                    {
                        if (attachments.Count > 0)
                        {
                            foreach (Attachment attachment in attachments)
                            {
                                mail.Attachments.Add(attachment);
                            }
                        }
                    }
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.UseDefaultCredentials = false;
                        //smtp.UseDefaultCredentials = true;
                        if (sMTPType == "SendGrid")
                        {
                            smtp.Credentials = new System.Net.NetworkCredential(userName, password);
                        }
                        else
                        {
                            smtp.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                        }

                        smtp.EnableSsl = enableSSL;
                        //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //CommonFunctions.LogErrorInFile(DateTime.Now.Ticks.ToString(), FromEmail, ex);
                //MailDO objDO = new MailDO();
                //int len = 499;
                //if (ex.Message.Length < 499)
                //{
                //    len = ex.Message.Length;
                //}
                //objDO.UpdateMailQueueStatus(UID, false, ex.Message.Substring(0, len), FromEmail);
                throw;
                //throw new Exception(ex.Message);

            }

        }
        public string GetEmailTemplateContent(string strEmailFilepath)
        {
            string fileName = strEmailFilepath;

            // Default directory
            string defaultDirectory = "Data/MailTemplate";
            string fullPath = Path.Combine(BasePhysicalPath??"~", defaultDirectory, fileName);
            return GetFileContent(fullPath);
        }
        public static string GetFileContent(string strFilepath)
        {
            string functionReturnValue = null;
            StreamReader swfile = null;
            string strContent = string.Empty;
            try
            {
                swfile = File.OpenText(strFilepath);
                strContent = swfile.ReadToEnd();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (swfile != null)
                {
                    swfile.Close();
                }
            }
            if (strContent.Length > 0)
            {
                functionReturnValue = strContent;
            }
            else
            {
                functionReturnValue = "";
            }
            return functionReturnValue;
        }
    }
}
