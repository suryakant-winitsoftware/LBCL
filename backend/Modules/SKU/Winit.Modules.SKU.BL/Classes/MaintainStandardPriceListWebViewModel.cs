using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.SKU.BL.Classes
{
    public class MaintainStandardPriceListWebViewModel : MaintainStandardPriceListBaseViewModel
    {
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private readonly Winit.Modules.Base.BL.ApiService _apiService;
        private readonly IAppUser _appUser;
        private readonly CommonFunctions _commonFunctions;
        private readonly IAlertService _alertService;
        public MaintainStandardPriceListWebViewModel(IServiceProvider serviceProvider,
               IAppUser appUser,
               IAppConfig appConfigs,
               Base.BL.ApiService apiService,
               CommonFunctions commonFunctions,
               IAlertService alertService, ILoadingService loadingService, IAppSetting appSetting) :
            base(serviceProvider, appUser, alertService, loadingService, appSetting)
        {
            _appConfigs = appConfigs;
            _apiService = apiService;
            _appUser = appUser;
            _commonFunctions = commonFunctions;
            _alertService = alertService;

            FRPrice = new SKUPriceList();
        }


        public void OnSort(SortCriteria criteria)
        {

        }


        #region Api calling methods

        protected override async Task GetStandardPriceListDetails()
        {
            try
            {
                Winit.Shared.Models.Common.PagingRequest pagingRequest = new Winit.Shared.Models.Common.PagingRequest()
                {
                    FilterCriterias = new()
                               {
                                   new Winit.Shared.Models.Enums.FilterCriteria("SelectionUID", _appUser.SelectedJobPosition.OrgUID, Winit.Shared.Models.Enums.FilterType.Equal),
                                   new Winit.Shared.Models.Enums.FilterCriteria("Type", "FR", Winit.Shared.Models.Enums.FilterType.Equal)
                               },
                    IsCountRequired = true
                };


                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKUPriceList/SelectAllSKUPriceListDetails",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);

                    Winit.Shared.Models.Common.PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPriceList> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPriceList>>(data);
                    if (pagedResponse != null)
                    {

                        if (pagedResponse.PagedData != null)
                        {
                            FRPrice = pagedResponse.PagedData.FirstOrDefault();
                            if (FRPrice == null)
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", $"{apiResponse.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {

            }
        }


        protected override async Task GetSKUPriceDetails()
        {
            try
            {
                PagingRequest.FilterCriterias?.Add(DefaultFilter);
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<ISKUPrice>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISKUPrice>>(
                    $"{_appConfigs.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetailsV1",
                    HttpMethod.Post, PagingRequest);


                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    SKUPriceList.Clear();
                    SerchedSKUPriceList.Clear();
                    SKUPriceList.AddRange(apiResponse.Data?.PagedData ?? []);
                    SerchedSKUPriceList.AddRange(apiResponse.Data?.PagedData ?? []);
                    _totalItems = apiResponse.Data.TotalCount;
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", $"{apiResponse.ErrorMessage}");
                    _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                _totalItems = 0;
            }
        }

        public override async Task<ISKUAttributeLevel> GetSKUAttributeType()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                   $"{_appConfigs.ApiBaseUrl}DataPreparation/GetAllSKUAttributeDDL",
                   HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<SKUAttributeLevelDTO>? Response = JsonConvert.DeserializeObject<ApiResponse<SKUAttributeLevelDTO>>(apiResponse.Data);
                    if (Response != null && Response.IsSuccess)
                    {
                        var skudata = Response.Data;
                        ISKUAttributeLevel skuAttributeLevel = new SKUAttributeLevel();
                        skuAttributeLevel.SKUGroupTypes = skudata.SKUGroupTypes?.Cast<ISelectionItem>().ToList();
                        if (skudata.SKUGroups != null)
                        {
                            skuAttributeLevel.SKUGroups = new Dictionary<string, List<ISelectionItem>>();
                            foreach (var kvp in skudata.SKUGroups)
                            {
                                skuAttributeLevel.SKUGroups[kvp.Key] = kvp.Value.Cast<ISelectionItem>().ToList();
                            }
                        }
                        return skuAttributeLevel;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        #endregion
    }
}
