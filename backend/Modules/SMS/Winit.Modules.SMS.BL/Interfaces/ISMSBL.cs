using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.BL.Interfaces
{
    public interface ISMSBL
    {
        Task<SMSApiResponse> SendOtp(ISms smsRequest);
        Task<int> CreateSmsRequest(ISms model);
        Task<List<ISmsModel>> GetSmsRequest(string UID);
        Task<List<ISmsRequestModel>> GetSmsRequestFromJob();
        Task<int> UpdateSmsRequest(ISms model);
        Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID);
        Task<ISms> GetSmsRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID);
        Task<int> CheckExistsOrNot(ISms smsRequest);
    }
}
