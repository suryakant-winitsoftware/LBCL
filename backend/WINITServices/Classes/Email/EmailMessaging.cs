using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using WINITSharedObjects.Models;

namespace WINITServices.Classes.Email
{
    public class EmailMessaging
    {
        IConfiguration _configuration;
        EmailMessage _emailMessage;
        public EmailMessaging(IConfiguration configuration)
        {
            _configuration = configuration;
            _emailMessage = new EmailMessage();
            var _mailSetting = _configuration.GetSection("smtpsetting");
            _emailMessage.smtpserver = _mailSetting.GetSection("smtpserver").Value;
            _emailMessage.smtpport = Convert.ToInt32(_mailSetting.GetSection("smtpport").Value ?? "25");
            _emailMessage.username = _mailSetting.GetSection("username").Value;
            _emailMessage.password = _mailSetting.GetSection("password").Value;
            _emailMessage.displayName = _mailSetting.GetSection("displayname").Value;
            _emailMessage.IsBodyHtml = Convert.ToBoolean(_mailSetting.GetSection("IsBodyHtml").Value ?? "True");
            _emailMessage.EnableSsl = Convert.ToBoolean(_mailSetting.GetSection("EnableSsl").Value ?? "True");
            //_emailMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailMessage>(_configuration.GetSection("smtpsetting").GetChildren().ToString());
        }
        public async Task sendEmailasync(EmailMessage emailMsg)
        {
            await Task.Run(() =>
            {
                try
                {
                    //var _mailSetting = _configuration.GetSection("smtpsetting");
                    string smtp = _emailMessage.smtpserver;// _mailSetting.GetSection("smtpserver").Value;
                    int port = _emailMessage.smtpport;// _mailSetting.GetSection("smtpport").Value;
                    string user = _emailMessage.username;// _mailSetting.GetSection("username").Value;
                    string pwd = _emailMessage.password;// _mailSetting.GetSection("password").Value;
                    string displayName = _emailMessage.displayName;// _mailSetting.GetSection("displayname").Value;

                    MailMessage msg = new MailMessage();
                    msg.From = new MailAddress(user, displayName);
                   // msg.Bcc.Add("amit.86k@gmail.com");
                    msg.Subject = emailMsg.subject;
                    msg.Body = emailMsg.message;
                    msg.IsBodyHtml = _emailMessage.IsBodyHtml;
                    if (emailMsg.attachments != null)
                    {
                        foreach (var attachment in emailMsg.attachments)
                        {
                            msg.Attachments.Add(new Attachment(attachment));
                        }
                    }
                    emailMsg.to.ForEach(el =>
                    {
                        try
                        {

                            msg.To.Add(new MailAddress(el));

                            using (var client = new SmtpClient(smtp, port))
                            {
                                // client.UseDefaultCredentials = false;
                                client.EnableSsl = _emailMessage.EnableSsl;
                                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                                try
                                {
                                    client.Send(msg);
                                    //client.SendAsync(msg, null);
                                   

                                }
                                catch (Exception ex) { }
                                msg.To.Clear();
                            }

                        }
                        catch (Exception ex) { }


                    });





                }
                catch (Exception exc)
                {

                }
            });
        }
    }
}
