using Azure;
using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.NonCashSettlement
{
    public class NonCashSettlementWebViewModel : NonCashSettlementBaseViewModel
    {
        protected readonly ApiService _apiService;
        public NonCashSettlementWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        public override async Task GetCustomerCodeName()
        {
            Winit.Shared.Models.Common.ApiResponse<AccCustomer[]> responsed = await _apiService.FetchDataAsync<AccCustomer[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAllCustomersBySalesOrgCode?SessionUserCode=" + _appUser.SelectedJobPosition.OrgUID, HttpMethod.Get, null);
            if (responsed != null && responsed.Data != null)
            {
                await GetBank();
                CustomerCode = responsed.Data;
            }
            foreach (var store in CustomerCode)
            {
                storeData[store.UID] = store.Name;
            }
        }
        public async Task GetBank()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.Bank.Model.Classes.Bank[]> response = await _apiService.FetchDataAsync<Winit.Modules.Bank.Model.Classes.Bank[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetBankNames", HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    BankNames = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " recceipt.razor exception");
            }
        }
        public override async Task ShowAllTabssRecords(int Count = 0)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = _currentPage;
                pagingRequest.PageSize = _pageSize;
                pagingRequest.SortCriterias = null;
                pagingRequest.FilterCriterias = filterCriterias;
                pagingRequest.IsCountRequired = true;
                await PopulateTabs(pagingRequest, Count);
                await CollectionTabs();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
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
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetCollectionTabsDetails?PageName=" + "NonCashSettlement", HttpMethod.Post, pagingRequest);
                if (responseTab != null && responseTab.IsSuccess)
                {
                    CollectionTabDetails = responseTab.Data.PagedData.ToList<IAccCollection>();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task PopulateTabs(PagingRequest pagingRequest, int Count)
        {
            try
            {
                await ShowPendingRecords(pagingRequest, Count);
                await ShowSettledRecords(pagingRequest, Count);
                await ShowApprovedRecords(pagingRequest, Count);
                await ShowBouncedRecords(pagingRequest, Count);
                await ShowRejectedRecords(pagingRequest, Count);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message + " settlement.razor exception"}", ex);
            }
        }
        public async Task ShowPendingRecords(PagingRequest pagingRequest, int Count)
        {
            try
            {
                Pendingresponse = await _apiService.FetchDataAsync<PagedResponse<AccCollectionPaymentMode>>($"{_appConfig.ApiBaseUrl}CollectionModule/ShowPending", HttpMethod.Post, pagingRequest);
                if (Pendingresponse.StatusCode != 404)
                {
                    if (Pendingresponse.Data != null)
                    {
                        elem = Pendingresponse.Data.PagedData.ToArray();
                        pendingCount = Pendingresponse.Data.TotalCount;
                    }
                    _elemPending = elem.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
            }
        }
        public async Task ShowSettledRecords(PagingRequest pagingRequest, int Count)
        {
            try
            {
                Settledresponse = await _apiService.FetchDataAsync<PagedResponse<AccCollectionPaymentMode>>($"{_appConfig.ApiBaseUrl}CollectionModule/ShowSettled", HttpMethod.Post, pagingRequest);
                if (Settledresponse.StatusCode != 404)
                {
                    if (Settledresponse.Data != null)
                    {
                        elemen = Settledresponse.Data.PagedData.ToArray();
                        settlementCount = Settledresponse.Data.TotalCount;
                    }
                    _elemSettled = elemen.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
            }
        }
        public async Task ShowApprovedRecords(PagingRequest pagingRequest, int Count)
        {
            try
            {
                Approvedresponse = await _apiService.FetchDataAsync<PagedResponse<AccCollectionPaymentMode>>($"{_appConfig.ApiBaseUrl}CollectionModule/ShowApproved", HttpMethod.Post, pagingRequest);
                if (Approvedresponse.StatusCode != 404)
                {
                    if (Approvedresponse.Data != null)
                    {
                        elemt = Approvedresponse.Data.PagedData.ToArray();
                        approvedCount = Approvedresponse.Data.TotalCount;
                    }
                    _elemApproved = elemt.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
            }
        }
        public async Task ShowBouncedRecords(PagingRequest pagingRequest, int Count)
        {
            try
            {
                Bouncedresponse = await _apiService.FetchDataAsync<PagedResponse<AccCollectionPaymentMode>>($"{_appConfig.ApiBaseUrl}CollectionModule/ShowBounced", HttpMethod.Post, pagingRequest);
                if (Bouncedresponse.StatusCode != 404)
                {
                    if (Bouncedresponse.Data != null)
                    {
                        elemtBounc = Bouncedresponse.Data.PagedData.ToArray();
                        bouncedCount = Bouncedresponse.Data.TotalCount;
                    }
                    _elemBounced = elemtBounc.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
            }
        }
        public async Task ShowRejectedRecords(PagingRequest pagingRequest, int Count)
        {
            try
            {
                Rejectedresponse = await _apiService.FetchDataAsync<PagedResponse<AccCollectionPaymentMode>>($"{_appConfig.ApiBaseUrl}CollectionModule/ShowRejected", HttpMethod.Post, pagingRequest);
                if (Rejectedresponse.StatusCode != 404)
                {
                    if (Rejectedresponse.Data != null)
                    {
                        elemtRej = Rejectedresponse.Data.PagedData.ToArray();
                        rejectedCount = Rejectedresponse.Data.TotalCount;
                    }
                    _elemRejected = elemtRej.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " settlement.razor exception");
            }
        }
    }
}
