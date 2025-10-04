using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.SettlePayment
{
    public abstract class SettlePaymentBaseViewModel : ISettlePaymentViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public AccCollectionPaymentMode[] Bank { get; set; }
        public SettlePaymentBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _appConfig = appConfig;
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            Bank = new AccCollectionPaymentMode[0];
        }
        public async virtual Task GetChequeDetails(string UID, string TargetUID)
        {
            await GetChequeDetails_Data(UID, TargetUID);
        }
        public async virtual Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickApproveReject(string UID, string Button, string Comments1, string SessionUserCode, string ReceiptNumber, string ChequeNo1)
        {
            return await OnClickApproveReject_Data(UID, Button, Comments1, SessionUserCode, ReceiptNumber, ChequeNo1);
        }
        protected abstract Task GetChequeDetails_Data(string UID, string TargetUID);
        public abstract Task<bool> UpdateFields_Data(string UID, string BankName, string Branch, string ReferenceNumber);
        protected abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickApproveReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string ReceiptNumber, string ChequeNo1);
    }
}
