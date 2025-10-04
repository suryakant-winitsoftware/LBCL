using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.SnackBar;
using Winit.UIComponents.SnackBar.Enum;

namespace Winit.Modules.Location.BL.Classes
{
    public class AddEditLocationMappingTemplateWebViewModel : AddEditLocationMappingTemplateBaseViewModel
    {
        ApiService _apiService;
        IAppConfig _appConfigs;
        IAlertService _alertService;
        public AddEditLocationMappingTemplateWebViewModel(ApiService apiService, IAppConfig appConfigs,
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

        #region API Calling
        protected override async Task GetLocationTypeFromAPI()
        {
            LocationTypesForDD = new List<ISelectionItem>();
            PagingRequest pagingRequest = new PagingRequest()
            {
                PageNumber = 1,
                PageSize = 10000,
                IsCountRequired = true,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    PagedResponse<LocationType>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<LocationType>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                    if (pagedResponse != null)
                    {
                        LocationTypes = pagedResponse.PagedData.ToList<ILocationType>();
                        foreach (var locationType in LocationTypes)
                        {
                            LocationTypesForDD.Add(new SelectionItem() { Code = locationType.Code, UID = locationType.UID, Label = locationType.Name });
                        }
                    }
                }
            }
        }
        protected override async Task GetLocationFromAPI(string locationTypeCode)
        {
            LocationsByType = new();
            Locations = new List<ISelectionItem>();
            List<string> locations = new List<string>()
            {
                locationTypeCode,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes", HttpMethod.Post, locations);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<Model.Classes.Location>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Model.Classes.Location>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {

                        foreach (var location in pagedResponse.Data)
                        {
                            LocationsByType.Add(location);
                            Locations.Add(new SelectionItem() { Code = location.Code, UID = location.UID, Label = location.Name });
                        }
                    }
                }
            }
        }
        protected override async Task GetLocationTemplateLinesFromAPI(string templateUID)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationTemplate/GetAllLocationTemplateLinesBytemplateUID?templateUID={templateUID}", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<List<LocationTemplateLine>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<LocationTemplateLine>>>(apiResponse.Data);
                    if (pagedResponse != null)
                    {
                        LocationTemplateLineList = pagedResponse.Data;
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
            LocationTemplate.ModifiedBy = _appUser.Emp.UID;
            LocationTemplate.ModifiedTime = DateTime.Now;
            LocationTemplateMaster locationTemplateMaster = new LocationTemplateMaster()
            {
                LocationTemplate = LocationTemplate,
                LocationMappingLineList = LocationTemplateLineList.FindAll(p => p.Id == 0),
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationTemplate/CUDLocationMappingAndLine", HttpMethod.Post, locationTemplateMaster);
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

        public async Task DeleteSelectedTemplateLines(ILocationTemplateLine locationTemplateLine = null)
        {
            List<LocationTemplateLine> UnSelectedLocationTemplateLineList = new();
            bool IsDelete=false;
            List<string> requestData = new List<string>();
            if (locationTemplateLine == null)
            {
                for (int i = 0; i < LocationTemplateLineList.Count; i++)
                {
                    if (LocationTemplateLineList[i].IsSelected)
                    {
                        if (LocationTemplateLineList[i].Id != 0)
                        requestData.Add(LocationTemplateLineList[i].UID);
                        else
                            IsDelete = true;
                    }
                    else 
                    {
                        UnSelectedLocationTemplateLineList.Add(LocationTemplateLineList[i]);
                    }

                }

            }
            else
            {
                requestData.Add(locationTemplateLine.UID);
                UnSelectedLocationTemplateLineList = LocationTemplateLineList.FindAll(p => p.UID != locationTemplateLine.UID);
            }
            if (requestData.Count == 0)
            {
                _toast.Add("Success", "Deleted Successfully");
            }
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationTemplate/DeleteLocationTemplateLines",
                HttpMethod.Delete, requestData);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    LocationTemplateLineList = UnSelectedLocationTemplateLineList;
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
