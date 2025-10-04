using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.OutStandingInvoice
{
    public class OutStandingInvoicesWebViewModel : OutStandingInvoicesBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();


        public OutStandingInvoicesWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appUser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService
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

        public override async Task<List<IAccPayableCMI>> GetAccPayableCMIFromAPI()
        {
            return await GetAccPayableCMIFromAPIAsync();
        }

        public override async Task GetAccPayableMasterFirCMIItemFromAPI(string uID)
        {
            await GetAccPayableMasterFirCMIItemFromAPIAsync(uID);
        }

        private async Task<List<IAccPayableCMI>> GetAccPayableCMIFromAPIAsync()
        {
            try
            {
                ApiResponse<PagedResponse<Winit.Modules.CollectionModule.Model.Classes.AccPayableCMI>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.CollectionModule.Model.Classes.AccPayableCMI>>(
                    $"{_appConfigs.ApiBaseUrl}AccPayableCMI/GetAccPayableCMIDetails/{_appUser.SelectedJobPosition.UID}",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    TotalItems = apiResponse.Data.TotalCount;
                    return apiResponse.Data.PagedData.ToList<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>();
                }
            }
            catch (Exception ex)
            {
            }

            return [];
        }


        private async Task GetAccPayableMasterFirCMIItemFromAPIAsync(string storeUID)
        {
            try
            {
                ApiResponse<Winit.Modules.CollectionModule.Model.Classes.AccPayableMaster> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.CollectionModule.Model.Classes.AccPayableMaster>(
                        $"{_appConfigs.ApiBaseUrl}AccPayableCMI/GetAccPayableMasterByUID/{storeUID}",
                        HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null &&
                    apiResponse.Data.AccPayableCMI != null)
                {
                    AccPayableMasterForCMIItem.AccPayableList.Clear();
                    AccPayableMasterForCMIItem.AccPayableCMI = apiResponse.Data.AccPayableCMI;
                    AccPayableMasterForCMIItem.AccPayableList.AddRange(apiResponse.Data.AccPayableList);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public async Task<List<ISelectionItem>> GetOU()
        {
            var pagingRequest = new PagingRequest()
            {
                FilterCriterias = [new FilterCriteria(name: nameof(IOrg.OrgTypeUID), value: "OU", type: FilterType.Equal)]
            };

            ApiResponse<PagedResponse<IOrg>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<IOrg>>(
                        $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
                        HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                var items = apiResponse.Data.PagedData.Select(p =>

                    new SelectionItem()
                    {
                        UID = p.Code,
                        Code = p.Code,
                        Label = $"{p.Name}"
                    }
                ).ToList<ISelectionItem>();

                return items;
            }

            return [];
        }

        public async Task<List<OutstandingInvoiceView>> GetOutstandingInvoiceDetailsByStoreCode(string storeCode, int pageNumber, int pageSize)
        {
            ApiResponse<List<OutstandingInvoiceView>> apiResponse =
                    await _apiService.FetchDataAsync<List<OutstandingInvoiceView>>(
                        $"{_appConfigs.ApiBaseUrl}AccPayableCMI/OutSTandingInvoicesByStoreCode/{storeCode}/{pageNumber}/{pageSize}",
                        HttpMethod.Get);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data : [];
        }
    }
}