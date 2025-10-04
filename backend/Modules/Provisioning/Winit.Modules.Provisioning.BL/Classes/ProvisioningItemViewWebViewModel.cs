using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public class ProvisioningItemViewWebViewModel : ProvisioningItemViewBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ProvisioningItemViewWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task<List<IProvisionItemView>> GetProvisionItemDataList(string UID)
        {
            return await GetProvisionItemDataListFromApiAsync(UID);
        }
        public override async Task<IProvisionItemView> GetProvisioningItemDetailsByUid(string provisionItemUID)
        {
            return await GetProvisioningItemDetailsByUidFromApiAsync(provisionItemUID);
        }
        private async Task<List<IProvisionItemView>> GetProvisionItemDataListFromApiAsync(string UID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningLineItem/SelectProvisioningLineItemsDetailsByUID/{UID}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionItemView>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionItemView>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<IProvisionItemView> GetProvisioningItemDetailsByUidFromApiAsync(string UID)
        {
            try
            {
                ApiResponse<Winit.Modules.Provisioning.Model.Classes.ProvisionItemView> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Provisioning.Model.Classes.ProvisionItemView>(
                    $"{_appConfigs.ApiBaseUrl}ProvisioningLineItem/GetProvisioningLineItemDetailsByUID/{UID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return  null;
        }
    }
}
