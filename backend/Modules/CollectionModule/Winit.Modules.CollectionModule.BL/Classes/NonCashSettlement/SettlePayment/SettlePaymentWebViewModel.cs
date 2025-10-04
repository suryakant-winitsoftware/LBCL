using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.SettlePayment
{
    public class SettlePaymentWebViewModel : SettlePaymentBaseViewModel
    {
        protected readonly ApiService _apiService;
        public SettlePaymentWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
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
            catch (Exception)
            {
                throw;
            }
        }
        protected override async Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickApproveReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string ReceiptNumber, string ChequeNo1)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/ValidateChequeSettlement?UID=" + UID + "&Comments=" + Comments1 + "&Status=" + Button + "&SessionUserCode=" + SessionUserCode + "&ReceiptUID=" + ReceiptNumber + "&ChequeNo=" + ChequeNo1, HttpMethod.Post, null);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message + " settled.razor exception"}");
            }
        }
    }
}
