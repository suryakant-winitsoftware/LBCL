using System.Text.Json;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Classes.Common
{
    public class NotificationSMSServiceBL : INotificationSMSServiceBL
    {
        private readonly ISMSDL _smsDL;
        public NotificationSMSServiceBL(ISMSDL sMSDL)
        {
            _smsDL = sMSDL;
        }
        public async Task SendSMSAsync(ISms smsRequest)
        {
            // Logic to send an email
            Console.WriteLine($"Sending sms with data: {JsonSerializer.Serialize(smsRequest)}");
            await _smsDL.SendOtp(smsRequest);
            await Task.CompletedTask;
        }
    }
}
