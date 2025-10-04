using Azure;
using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement.ApprovePayment
{
    public class ApprovePaymentWebViewModel : ApprovePaymentBaseViewModel
    {
        protected readonly ApiService _apiService;
        public ApprovePaymentWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
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

        protected override async Task CheckReversalPossible_Data(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccCollection[]> response1 = await _apiService.FetchDataAsync<AccCollection[]>($"{_appConfig.ApiBaseUrl}CollectionModule/IsReversal?UID=" + UID, HttpMethod.Get, null);
                if (response1 != null && response1.Data != null)
                {
                    ReversalData = response1.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " approved.razor exception");
            }
        }

        protected override async Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReversal_Data(string UID, decimal ChequeAmount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CreateReversalReceiptByReceiptNumber?ReceiptNumber=" + UID + "&TargetUID=" + "Cheque" + "&Amount=" + ChequeAmount + "&ChequeNo=" + ChequeNo + "&SessionUserCode=" + SessionUserCode + "&ReasonforCancelation=" + ReasonforCancelation, HttpMethod.Post, null);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " approved.razor exception");
            }
        }
    }
}
