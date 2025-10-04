using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Distributor.BL.Classes
{
    public class MaintainDistributorBaseViewModel : Interfaces.IMaintainDistributorBaseViewModel
    {
        public MaintainDistributorBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
            Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            _loadingService = loadingService;
        }
        ILoadingService _loadingService;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }
        public bool IsLoad { get; set; }
        public int TotalItems { get; set; }
        public PagingRequest PagingRequest { get; set; } = new()
        {
            FilterCriterias = [],
            SortCriterias = [],
            PageNumber = 1,
            PageSize = 20,
            IsCountRequired = true
        };
        public List<Model.Classes.Distributor> DispayDistributorsList { get; set; } = [];



        public async Task PopulateViewModel()
        {


            //await GetDataFromAPIAsync();
        }
        public async Task OnPageChange(int pageNo)
        {
            PagingRequest.PageNumber = pageNo;
            await GetDataFromAPIAsync();
        }
        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var item in filterCriteria)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    FilterCriteria filter = new FilterCriteria(item.Key, item.Value, item.Key == "Status" ? FilterType.Equal : FilterType.Like);
                    filterCriterias.Add(filter);
                }
            }
            PagingRequest.FilterCriterias = filterCriterias;
            await GetDataFromAPIAsync();
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            _loadingService.ShowLoading();
            PagingRequest.SortCriterias = new() { sortCriteria };
            await GetDataFromAPIAsync();
            _loadingService.HideLoading();
        }

        public async Task GetDataFromAPIAsync()
        {
            try
            {

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Distributor/SelectAllDistributors",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Distributor.Model.Classes.Distributor>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Distributor.Model.Classes.Distributor>>(data);
                    if (pagedResponse != null)
                    {
                        if (pagedResponse.PagedData != null)
                        {
                            DispayDistributorsList.Clear();
                            DispayDistributorsList.AddRange(pagedResponse.PagedData);
                            TotalItems = pagedResponse.TotalCount;
                        }
                    }
                }
                else
                {
                    TotalItems = 0;
                }
            }
            catch (Exception ex)
            {
                TotalItems = 0;
            }
        }
    }
}
