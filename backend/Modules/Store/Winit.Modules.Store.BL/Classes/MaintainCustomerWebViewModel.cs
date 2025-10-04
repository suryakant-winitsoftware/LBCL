using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class MaintainCustomerWebViewModel : MaintainCustomerBaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly IAppConfig _appConfig;
        private readonly IAppUser _appUser;

        public MaintainCustomerWebViewModel(IAppConfig appConfig, ApiService apiService, IAppUser appUser)
        {
            _appConfig = appConfig;
            _apiService = apiService;
            _appUser = appUser;
        }
        public async Task PopulateViewModel()
        {
            await getListItemsAsync();
        }

        protected override async Task GetStores()
        {
            try
            {
                PagingRequest.FilterCriterias?.Add(new FilterCriteria("Type", "FRC", FilterType.Equal));
                PagingRequest.FilterCriterias?.Add(new FilterCriteria("FranchiseeOrgUid", _appUser.SelectedJobPosition.OrgUID, FilterType.Equal));
                if (PagingRequest.SortCriterias != null)
                {
                    PagingRequest.SortCriterias?.Add(new SortCriteria(nameof(IStore.ModifiedTime), SortDirection.Desc));
                }
                ApiResponse<PagedResponse<IStore>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<IStore>>(
                    $"{_appConfig.ApiBaseUrl}Store/SelectAllStore",
                    HttpMethod.Post, PagingRequest);
                Stores.Clear();
                TotalItems = 0;
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    Stores.AddRange(apiResponse.Data.PagedData);
                    TotalItems = apiResponse.Data.TotalCount;
                }
            }
            catch (Exception ex)
            {
                TotalItems = 0;
            }
        }
        private async Task getListItemsAsync()
        {
            try
            {

                ListItemRequest request = new()
                {
                    Codes = new()
                {
                    "PriceType",
                }
                    ,
                    isCountRequired = true
                };

                ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> apiResponse = await _apiService.
                    FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>>(
                 $"{_appConfig.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                 HttpMethod.Post, request);
                if (apiResponse.Data != null)
                {
                    PriceTypeSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<Winit.Modules.ListHeader.Model.Interfaces.IListItem>
                        (apiResponse.Data.PagedData.ToList(), e => e.UID, e => e.Code, e => e.Name));
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
