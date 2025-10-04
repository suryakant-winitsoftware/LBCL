using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.PendingPayment
{
    public class PendingPaymentWebViewModel : PendingPaymentBaseViewModel
    {
        private readonly ApiService _apiService;
        public PendingPaymentWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
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
        public override async Task<bool> UpdateFields_Data(string UID, string BankName, string Branch, string ReferenceNumber)
        {
            try
            {
                ApiResponse<bool> response = await _apiService.FetchDataAsync<bool>($"{_appConfig.ApiBaseUrl}CollectionModule/UpdateBankDetails?UID=" + UID + "&BankName=" + BankName + "&Branch=" + Branch + "&ReferenceNumber=" + ReferenceNumber, HttpMethod.Post, null);
                return response.Data;
            }
            catch(Exception)
            {
                throw;
            }
        }

        protected override async Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickSettleReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string CashNumber)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/ValidateChequeReceiptByPaymentMode?UID=" + UID + "&Button=" + Button + "&Comments=" + Comments1 + "&SessionUserCode=" + SessionUserCode + "&CashNumber=" + CashNumber, HttpMethod.Post, null);
                return response;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
