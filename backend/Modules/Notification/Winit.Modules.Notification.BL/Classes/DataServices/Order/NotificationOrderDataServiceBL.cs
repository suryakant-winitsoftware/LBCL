using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Order;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Classes.DataServices.Order
{
    public class NotificationOrderDataServiceBL : INotificationOrderDataServiceBL
    {
        private IEmailDL _emailDL { get; }
        private ISMSDL _smsDL { get; }
        public NotificationOrderDataServiceBL(IEmailDL emailDL, ISMSDL sMSDL)
        {
            _emailDL = emailDL;
            _smsDL = sMSDL;
        }
        public async Task<SMS.Model.Interfaces.INotificationPOData> GetDataAsyncForSms(INotificationRequest notificationRequest)
        {
            // Logic to fetch data based on the notificationRequest
            Console.WriteLine("Fetching data for request...");
            return await _smsDL.GetNotificationDataForPO(notificationRequest.TemplateName, notificationRequest.LinkedItemUID);
        }
        public async Task<Email.Model.Interfaces.INotificationPOData> GetDataAsyncForEmail(INotificationRequest notificationRequest)
        {
            // Logic to fetch data based on the notificationRequest
            Console.WriteLine("Fetching data for request...");
            return await _emailDL.GetNotificationDataForPO(notificationRequest.TemplateName, notificationRequest.LinkedItemUID);
        }
        public async Task<IMailRequest> GetMessageByTemplateForEmail(Email.Model.Interfaces.INotificationPOData notificationPOData, string templateName, string uniqueUID)
        {
            return await _emailDL.GetMailRequestForPO(templateName, notificationPOData, uniqueUID);
        }
        public async Task<int> CreateMailRequest(IMailRequest mailRequest)
        {
            return await _emailDL.CheckExistsOrNot(mailRequest);
        }
        public async Task<int> UpdateSuccessEmailRequest(IMailRequest mailRequest)
        {
            return await _emailDL.UpdateSuccessEmailRequest(mailRequest);
        }
        public async Task<int> UpdateFailureEmailRequest(IMailRequest mailRequest)
        {
            return await _emailDL.UpdateFailureEmailRequest(mailRequest);
        }
        public async Task<ISms> GetMessageByTemplateForSms(SMS.Model.Interfaces.INotificationPOData notificationPOData, string templateName, string uniqueUID)
        {
            return await _smsDL.GetSmsRequestForPO(templateName, notificationPOData, uniqueUID);
        }
        public async Task<int> CreateSmsRequest(ISms smsRequest)
        {
            return await _smsDL.CheckExistsOrNot(smsRequest);
        }
        public async Task<int> UpdateSmsRequest(ISms smsRequest)
        {
            return await _smsDL.UpdateSmsRequest(smsRequest);
        }
    }
}
