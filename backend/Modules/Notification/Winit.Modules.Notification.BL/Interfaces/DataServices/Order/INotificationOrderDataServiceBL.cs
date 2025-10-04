using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Notification.Model.Classes.Email;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Interfaces.DataServices.Order
{
    public interface INotificationOrderDataServiceBL
    {
        Task<SMS.Model.Interfaces.INotificationPOData> GetDataAsyncForSms(INotificationRequest notificationRequest);
        Task<Email.Model.Interfaces.INotificationPOData> GetDataAsyncForEmail(INotificationRequest notificationRequest);
        Task<IMailRequest> GetMessageByTemplateForEmail(Email.Model.Interfaces.INotificationPOData notificationPOData, string templateName, string uniqueUID);
        Task<int> CreateMailRequest(IMailRequest mailRequest);
        Task<int> UpdateFailureEmailRequest(IMailRequest mailRequest);
        Task<int> UpdateSuccessEmailRequest(IMailRequest mailRequest);
        Task<ISms> GetMessageByTemplateForSms(SMS.Model.Interfaces.INotificationPOData notificationPOData, string templateName, string uniqueUID);
        Task<int> CreateSmsRequest(ISms smsRequest);
        Task<int> UpdateSmsRequest(ISms smsRequest);
    }
}
