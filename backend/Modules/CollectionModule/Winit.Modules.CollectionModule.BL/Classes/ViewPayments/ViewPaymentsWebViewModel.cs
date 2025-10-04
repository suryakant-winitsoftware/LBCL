using Azure;
using Nest;
using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.BL.Classes.Cashier;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.ViewPayments
{
    public class ViewPaymentsWebViewModel : ViewPaymentsBaseViewModel
    {
        private readonly ApiService _apiService;

        public ViewPaymentsWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, ApiService apiService) : base(serviceProvider, appConfig)
        {
            _apiService = apiService;
        }

        public override async Task GetCustomerCodeName()
        {
            Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> responsed = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetCustomerCode?CustomerCode=" + "", HttpMethod.Get, null);
            if (responsed != null && responsed.Data != null)
            {
                CustomerCode = responsed.Data;
            }
            foreach (var store in CustomerCode)
            {
                storeData[store.UID] = store.Name;
            }
        }
        public override async Task<List<IAccCollection>> GetReceiptDetails_Data()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;

                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollection>> response = await _apiService.FetchDataAsync<PagedResponse<AccCollection>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/ViewPayments", HttpMethod.Post, pagingRequest);
                if (response != null && response.IsSuccess)
                {
                    TotalCount = response.Data.TotalCount;
                    return response.Data.PagedData.ToList<IAccCollection>();
                }
                
                
                return default;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " viewpayments.razor exception");
            }
        }
        public override async Task CollectionTabs()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<AccCollection>> responseTab = await _apiService.FetchDataAsync<PagedResponse<AccCollection>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetCollectionTabsDetails?PageName=" + "ViewPayments", HttpMethod.Post, pagingRequest);
                if (responseTab != null && responseTab.IsSuccess)
                {
                    CollectionTabDetails = responseTab.Data.PagedData.ToList<IAccCollection>();
                }
            }
            catch(Exception ex)
            {

            }
        }
        public override async Task ViewReceiptDetails(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AccCollectionAllotment>> response = await _apiService.FetchDataAsync<List<AccCollectionAllotment>>($"{_appConfig.ApiBaseUrl}CollectionModule/ViewPaymentsDetails?UID=" + UID, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    _viewDetailsList = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " viewpaymentdetails.razor exception");
            }
        }
    }
}
