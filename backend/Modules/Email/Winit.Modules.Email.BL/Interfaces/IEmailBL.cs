using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;

namespace Winit.Modules.Email.BL.Interfaces
{
    public interface IEmailBL
    {
        Task<bool> SendEmail(IMailRequest MailFormat);
        Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID);
        Task<List<IEmailRequestModel>> GetEmailRequestFromJob();
        Task<int> UpdateSuccessEmailRequest(IMailRequest model);
        Task<int> UpdateFailureEmailRequest(IMailRequest model);
        Task<IMailRequest> GetMailRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID);
        Task<int> CreateEmailRequest(IMailRequest mailRequest);
        Task<int> CheckExistsOrNot(IMailRequest mailRequest);
    }
}
