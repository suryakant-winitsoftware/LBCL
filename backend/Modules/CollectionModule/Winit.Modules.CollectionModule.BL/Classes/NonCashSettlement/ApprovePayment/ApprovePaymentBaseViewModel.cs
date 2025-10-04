using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.ApprovePayment
{
    public abstract class ApprovePaymentBaseViewModel : IApprovePaymentViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public AccCollectionPaymentMode[] Bank { get; set; }
        public AccCollection[] ReversalData { get; set; } 
        public ApprovePaymentBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
            Bank = new AccCollectionPaymentMode[0];
            ReversalData = new AccCollection[0];
        }
        public async virtual Task GetChequeDetails(string UID, string TargetUID)
        {
            await GetChequeDetails_Data(UID, TargetUID);
        }

        public async virtual Task CheckReversalPossible(string UID)
        {
            await CheckReversalPossible_Data(UID);
        }

        
        public async virtual Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReversal(string UID, decimal ChequeAmount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
        {
            return await ReceiptReversal_Data(UID, ChequeAmount, ChequeNo, SessionUserCode, ReasonforCancelation);
        }


        protected abstract Task GetChequeDetails_Data(string UID, string TargetUID);
        protected abstract Task CheckReversalPossible_Data(string UID);
        protected abstract Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReversal_Data(string UID, decimal ChequeAmount, string ChequeNo, string SessionUserCode, string ReasonforCancelation);
    }
}
