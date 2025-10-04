using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.BL.Interfaces;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.BL.Classes
{   
    public class SMSBL : ISMSBL
    {
        private readonly Winit.Modules.SMS.DL.Interfaces.ISMSDL _SMSDl;
        public SMSBL(ISMSDL sMSDL)
        {
            _SMSDl = sMSDL;
        }
        public async Task<int> CreateSmsRequest(ISms model)
        {
            return await _SMSDl.CreateSmsRequest(model);
        }
        public async Task<SMSApiResponse> SendOtp(ISms smsRequest)
        {
            return await _SMSDl.SendOtp(smsRequest);
        }
        public async Task<List<ISmsModel>> GetSmsRequest(string UID)
        {
            return await _SMSDl.GetSmsRequest(UID);
        }
        public async Task<List<ISmsRequestModel>> GetSmsRequestFromJob()
        {
            return await _SMSDl.GetSmsRequestFromJob();
        }
        public async Task<int> UpdateSmsRequest(ISms model)
        {
            return await _SMSDl.UpdateSmsRequest(model);
        }
        public async Task<INotificationPOData> GetNotificationDataForPO(string TemplateName, string OrderUID)
        {
           return await _SMSDl.GetNotificationDataForPO(TemplateName, OrderUID);
        }
        public async Task<ISms> GetSmsRequestForPO(string notificationTemplateNames, INotificationPOData smsFields, string UniqueUID)
        {
            return await _SMSDl.GetSmsRequestForPO(notificationTemplateNames, smsFields, UniqueUID);
        }
        public async Task<int> CheckExistsOrNot(ISms smsRequest)
        {
            return await _SMSDl.CheckExistsOrNot(smsRequest);
        }
    }
}
