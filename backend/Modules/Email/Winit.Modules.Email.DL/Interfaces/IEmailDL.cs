
using Winit.Modules.Email.Model.Interfaces;

namespace Winit.Modules.Email.DL.Interfaces
{
    public interface IEmailDL
    {
        Task<bool> SendEmail(IMailRequest MailFormat);
        Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID);
        Task<List<IEmailRequestModel>> GetEmailRequestFromJob();
        Task<int> UpdateFailureEmailRequest(IMailRequest model);
        Task<int> UpdateSuccessEmailRequest(IMailRequest model);
        Task<IMailRequest> GetMailRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID);
        Task<int> CreateEmailRequest(IMailRequest mailRequest);
        Task<int> CheckExistsOrNot(IMailRequest mailRequest);
    }
}
