using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.RejectPayment
{
    public abstract class RejectPaymentBaseViewModel : IRejectPaymentViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public AccCollectionPaymentMode[] Bank { get; set; }
        public RejectPaymentBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
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
        protected abstract Task GetChequeDetails_Data(string UID, string TargetUID);
    }
}
