using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.SnackBar.Enum;
using Winit.UIComponents.SnackBar;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class AddEditSKUMappingTemplateWebViewModel: AddEditSKUMappingTemplateBaseViewModel
    {
        ApiService _apiService;
        IAppConfig _appConfigs;
        IAlertService _alertService;
        public AddEditSKUMappingTemplateWebViewModel(ApiService apiService, IAppConfig appConfigs,
            CommonFunctions commonFunctions, IAlertService alertService, IDataManager dataManager, IToast toast, IAppUser appUser)
        {
            _dataManager = dataManager;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _commonFunctions = commonFunctions;
            _alertService = alertService;
            _toast = toast;
            _appUser = appUser;
        }

        #region API Calling>
        protected override async Task GetSKUGroupTypeFromAPI()
        {
            SKUGroupsTypes = new List<ISelectionItem>();
            PagingRequest pagingRequest = new PagingRequest()
            {
                PageNumber = 1,
                PageSize = 10000,
                IsCountRequired = true,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUGroupType/SelectAllSKUGroupTypeDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                    if (pagedResponse != null)
                    {
                        //LocationTypes = pagedResponse.PagedData.ToList<ILocationType>();
                        foreach (var locationType in pagedResponse.PagedData)
                        {
                            SKUGroupsTypes.Add(new SelectionItem() { Code = locationType.Code, UID = locationType.UID, Label = locationType.Name });
                        }
                    }
                }
            }
        }
        protected override async Task GetSKUGroupBySkuGroupTypeUIDFromAPI(string skuGroupTypeUID)
        { 
            SKUGroups = new List<ISelectionItem>();
           
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUGroup/GetAllSKUGroupBySKUGroupTypeUID?skuGroupTypeUID={skuGroupTypeUID}", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<Winit.Modules.SKU.Model.Classes.SKUGroup>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Winit.Modules.SKU.Model.Classes.SKUGroup>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {

                        foreach (var sku in pagedResponse.Data)
                        {
                            //LocationsByType.Add(location);
                            SKUGroups.Add(new SelectionItem() { Code = sku.ParentName, UID = sku.UID, Label = sku.Name });
                        }
                    }
                }
            }
        }
        protected override async Task GetSKUTemplateLinesFromAPI(string templateUID)
        {
            PagingRequest pagingRequest = new()
            {
                PageNumber = 1,
                PageSize = 1000,
                FilterCriterias = new()
                {
                    new FilterCriteria("SKUTemplateUID",templateUID,FilterType.Equal)
                }
                ,IsCountRequired = true,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUTemplateLine/SelectSKUTemplateLineDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    PagedResponse<SKUTemplateLine>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<SKUTemplateLine>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                    if (pagedResponse != null&& pagedResponse.PagedData!=null)
                    {
                        SKUTemplateLineList = pagedResponse.PagedData.ToList();
                    }
                }
            }
        }


        public async Task FinalizeMapping()
        {
            var validate = IsValidated();
            if (!validate.Item1)
            {
                await _alertService.ShowErrorAlert("Error", validate.Item2);
                return;
            }
            SKUTemplate.ModifiedBy= _appUser.Emp.UID;
            SKUTemplate.ModifiedTime= DateTime.Now;
            SKUTemplateMaster skuTemplateMaster = new SKUTemplateMaster()
            {
                SKUTemplate = SKUTemplate,
                SKUTemplateLineList = SKUTemplateLineList,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUTemplateLine/CUDSKUTemplateAndLine", HttpMethod.Post, skuTemplateMaster);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    _toast.Add("Success", "Saved Successfully", Severity.Success);
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", apiResponse.ErrorMessage);
                }
            }
        }

       
        public async Task DeleteSelectedTemplateLines(ISKUTemplateLine SKUTemplateLine = null)
        {
            List<SKUTemplateLine> UnSelectedSKUTemplateLineList = new();
            //var validate = IsValidated();
            //if (!validate.Item1)
            //{
            //    await _alertService.ShowErrorAlert("Error", validate.Item2);
            //    return;
            //}
            List<string> requestData = new List<string>();
            if (SKUTemplateLine == null)
            {
                for (int i = 0; i < SKUTemplateLineList.Count; i++)
                {
                    if (SKUTemplateLineList[i].IsSelected && SKUTemplateLineList[i].Id != 0)
                    {
                        requestData.Add(SKUTemplateLineList[i].UID);
                    }
                    else
                    {
                        UnSelectedSKUTemplateLineList.Add(SKUTemplateLineList[i]);
                    }

                }

            }
            else
            {
                requestData.Add(SKUTemplateLine.UID);
                UnSelectedSKUTemplateLineList = SKUTemplateLineList.FindAll(p => p.UID != SKUTemplateLine.UID);
            }

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUTemplateLine/DeleteSKUTemplateLines",
                HttpMethod.Delete, requestData);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    SKUTemplateLineList = UnSelectedSKUTemplateLineList;
                    _toast.Add("Success", "Deleted Successfully", Severity.Success);
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", apiResponse.ErrorMessage);
                }
            }
        }
        #endregion

    }
}
