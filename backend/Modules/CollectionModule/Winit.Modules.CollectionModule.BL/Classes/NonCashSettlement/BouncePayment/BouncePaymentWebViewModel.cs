using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.BouncePayment
{
    public class BouncePaymentWebViewModel : BouncePaymentBaseViewModel
    {
        protected readonly ApiService _apiService;
        public BouncePaymentWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        protected override async Task GetChequeDetails_Data(string UID, string TargetUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccCollectionPaymentMode[]> response = await _apiService.FetchDataAsync<AccCollectionPaymentMode[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetChequeDetails?UID=" + UID + "&TargetUID=" + TargetUID, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    Bank = response.Data;
                }
                await Task.CompletedTask;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
