using Azure;
using iTextSharp.text;
using Nest;
using Newtonsoft.Json;
using System.Linq;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.CashSettlement
{
    public class CashSettlementWebViewModel : CashSettlementBaseViewModel
    {
        protected readonly ApiService _apiService;

        public CashSettlementWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }

        public override async Task GetCustomerCodeName()
        {
            Winit.Shared.Models.Common.ApiResponse<AccCustomer[]> responsed = await _apiService.FetchDataAsync<AccCustomer[]>(
                $"{_appConfig.ApiBaseUrl}CollectionModule/GetAllCustomersBySalesOrgCode?SessionUserCode=" + _appUser.SelectedJobPosition.OrgUID, HttpMethod.Get, null);
            if(responsed != null && responsed.Data != null)
            {
                CustomerCode  = responsed.Data;
            }
            foreach (var store in CustomerCode)
            {
                storeData[store.UID] = store.Name;
            }
        }
        

        public override async Task GetCashierDetails()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = _currentPage;
                pagingRequest.PageSize = _pageSize;
                pagingRequest.SortCriterias = null;
                pagingRequest.FilterCriterias = filterCriterias;
                pagingRequest.IsCountRequired = true;


                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccElement>> response = await _apiService.FetchDataAsync<PagedResponse<AccElement>>($"{_appConfig.ApiBaseUrl}CollectionModule/CashierSettlement", HttpMethod.Post, pagingRequest);
                if (response != null && response.Data != null)
                {
                    pageResponse = response.Data;
                }
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccElement>> response1 = await _apiService.FetchDataAsync<PagedResponse<AccElement>>($"{_appConfig.ApiBaseUrl}CollectionModule/CashierSettlementVoid", HttpMethod.Post, pagingRequest);
                if (response1 != null && response1.Data != null)
                {
                    pagedResponse1 = response1.Data;
                }
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccElement>> response2 = await _apiService.FetchDataAsync<PagedResponse<AccElement>>($"{_appConfig.ApiBaseUrl}CollectionModule/CashierSettlementSettled", HttpMethod.Post, pagingRequest);
                if (response2 != null && response2.Data != null)
                {
                    pagedResponse2 = response2.Data;
                }
                await CollectionTabs();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " cashier.razor exception");
            }
        }
        public async Task CollectionTabs()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 0;
                pagingRequest.PageSize = 0;
                pagingRequest.SortCriterias = null;
                pagingRequest.FilterCriterias = filterCriterias;
                pagingRequest.IsCountRequired = true;
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollection>> responseTab = await _apiService.FetchDataAsync<PagedResponse<AccCollection>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetCollectionTabsDetails?PageName=" + "CashSettlement", HttpMethod.Post, pagingRequest);
                if (responseTab != null && responseTab.IsSuccess)
                {
                    CollectionTabDetails = responseTab.Data.PagedData.ToList<IAccCollection>();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task<Winit.Shared.Models.Common.ApiResponse<string>> Clicked(string Status)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/SettledDetails?AccCollectionUID=" + Status, HttpMethod.Get, null);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " cashier.razor exception");
            }
        }

        public override async Task<Winit.Shared.Models.Common.ApiResponse<string>> SettleRecords(List<string> Multiple)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CashCollectionSettlement", HttpMethod.Post, Multiple);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " cashier.razor exception");
            }
        }

        public override async Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReverseByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CreateReversalReceiptByReceiptNumber?ReceiptNumber=" + ReceiptNumber + "&TargetUID=" + "Cash" + "&Amount=" + Amount + "&ChequeNo=" + ChequeNo + "&SessionUserCode=" + "1001" + "&ReasonforCancelation=" + ReasonforCancelation, HttpMethod.Post, null);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " reversaldetails.razor exception");
            }
        }

        public override async Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptVOIDByCash(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/VOIDCollectionByReceiptNumber?ReceiptNumber=" + ReceiptNumber + "&TargetUID=" + "Cash" + "&Amount=" + Amount + "&ChequeNo=" + ChequeNo + "&SessionUserCode=" + "1001" + "&ReasonforCancelation=" + ReasonforCancelation, HttpMethod.Post, null);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " reversaldetails.razor exception");
            }
        }
    }
}
