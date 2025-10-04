using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.PendingPayment
{
    public abstract class PendingPaymentBaseViewModel : IPendingPaymentViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public AccCollectionPaymentMode[] Bank { get; set; }
        public PendingPaymentBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
            Bank = new AccCollectionPaymentMode[0];
        }

        public async virtual Task GetChequeDetails(string UID, string TargetUID)
        {
            await GetChequeDetails_Data(UID, TargetUID);
        }
        
        public async virtual Task UpdateFields(string UID, string BankName, string Branch, string ReferenceNumber)
        {
            await UpdateFields_Data(UID, BankName, Branch, ReferenceNumber);
        }

        public async virtual Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickSettleReject(string UID, string Button, string Comments1, string SessionUserCode, string CashNumber)
        {
            return await OnClickSettleReject_Data(UID, Button, Comments1, SessionUserCode, CashNumber);
        }

        public abstract Task<bool> UpdateFields_Data(string UID, string BankName, string Branch, string ReferenceNumber);
        protected abstract Task GetChequeDetails_Data(string UID, string TargetUID);
        protected abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickSettleReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string CashNumber);
    }
}
