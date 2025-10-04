using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Email.BL.Interfaces;
using Winit.Modules.Email.DL.UtilityClasses;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Notification.BL.Interfaces.Common;

namespace Winit.Modules.Notification.BL.Classes.Common
{
    public class NotificationEmailServiceBL : INotificationEmailServiceBL
    {
        private readonly EmailUtility _emailUtility;
        private readonly IEmailBL _EmailBL;
        public NotificationEmailServiceBL(EmailUtility emailUtility, IEmailBL emailBL)
        {
            _emailUtility = emailUtility;
            _EmailBL = emailBL;
        }
        public async Task SendEmailAsync(IMailRequest mailRequest)
        {
            try
            {
                // Logic to send an email
                Console.WriteLine($"Sending email with data: {JsonSerializer.Serialize(mailRequest)}");
                await _EmailBL.SendEmail(mailRequest);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
    }
}
