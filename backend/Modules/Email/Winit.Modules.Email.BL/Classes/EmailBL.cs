using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.BL.Interfaces;
using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;

namespace Winit.Modules.Email.BL.Classes
{
    public class EmailBL : IEmailBL
    {
        private readonly IEmailDL EmailDL;
        public EmailBL(IEmailDL emailDL)
        {
            EmailDL = emailDL;
        }
        public async Task<bool> SendEmail(IMailRequest MailFormat)
        {
            return await EmailDL.SendEmail(MailFormat);
        }
        public async Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID)
        {
            return await EmailDL.GetNotificationDataForPO(TemplateName, OrderUID);
        }
        public async Task<List<IEmailRequestModel>> GetEmailRequestFromJob()
        {
            return await EmailDL.GetEmailRequestFromJob();
        }
        public async Task<int> UpdateFailureEmailRequest(IMailRequest model)
        {
            return await EmailDL.UpdateFailureEmailRequest(model);
        }
        public async Task<int> UpdateSuccessEmailRequest(IMailRequest model)
        {
            return await EmailDL.UpdateSuccessEmailRequest(model);
        }
        public async Task<int> CreateEmailRequest(IMailRequest mailRequest)
        {
            return await EmailDL.CreateEmailRequest(mailRequest);
        }
        public async Task<int> CheckExistsOrNot(IMailRequest mailRequest)
        {
            return await EmailDL.CheckExistsOrNot(mailRequest);
        }
        public async Task<IMailRequest> GetMailRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID)
        {
            return await EmailDL.GetMailRequestForPO(notificationTemplateNames, smsFields, UniqueUID);
        }
    }
}
