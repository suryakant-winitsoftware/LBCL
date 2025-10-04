using Microsoft.Extensions.Localization;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Route.BL.Classes
{
    public class AddEditRouteLoadViewModel: AddEditRouteLoadWebViewModel
    {
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        public AddEditRouteLoadViewModel(IServiceProvider serviceProvider,
    IFilterHelper filter,
    ISortHelper sorter,
    IListHelper listHelper,
    IAppUser appUser,
       IAppConfig appConfigs,
    Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService
         ) : base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            _localizer = Localizer;
            _languageService = languageService;


        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public override async Task PopulateViewModel(string apiParam = null)
        {
            LoadResources(null, _languageService.SelectedCulture);
            await GetRouteLoadTruckLineByUID(RouteLoadTruckTemplateUID);
        }
        public async Task GetRouteLoadTruckLineByUID(string uid)
        {
            try
            {
                string apiUrl = $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/SelectRouteLoadTruckTemplateAndLineByUID?UID={uid}";

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);


                    FilterRouteLoadTruckTemplateViewDTO = JsonConvert.DeserializeObject<RouteLoadTruckTemplateViewDTO>(data);
                    DisplayRouteLoadTruckTemplateViewDTO = JsonConvert.DeserializeObject<RouteLoadTruckTemplateViewDTO>(data);
                   
                    if (FilterRouteLoadTruckTemplateViewDTO != null && FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null)
                    {

                        SelectedRouteUID = DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.RouteUID;
                        TemplateName = DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateName;
                        TemplateDescription = DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateDescription;


                        if (DisplayRouteLoadTruckTemplateViewDTO != null && DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null)
                        {
                            foreach (var row in DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList)
                            {
                                row.ActionTypes = Winit.Shared.Models.Enums.ActionType.Update;
                            }
                        }
                        
                    }


                    else
                    {
                        // Log or handle the case where the response data or PagedData is null
                        Console.WriteLine(@_localizer["invalid_response_format:_data_or_paged_data_is_null."]);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"{@_localizer["error_fetching_data_from_api:"]} {ex.Message}");
            }
            //return null;
        }

        public override void MatchingNewExistingUIDs()
        {
            FindAllUIDs();
            FindExistingUIDs();
        }
        public override async Task<bool> DeleteSelectedTemplates()
        {
            var commonUIDs = ExistingUIDs.Intersect(AllUIDs).ToList();
            string apiUrl = $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/DeleteRouteLoadTruckTemplateLine";
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Delete, commonUIDs);
           if(apiResponse.IsSuccess)
            {
                return true;
            }
           else
            {
                return false;
            }
        }
        
        public void FindAllUIDs()
        {                                    
            AllUIDs = DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList
                 .Where(item => item.IsSelected)
                 .Select(item => item.UID)
                 .ToList();
        }

        public void FindExistingUIDs()
        {
           
            if (FilterRouteLoadTruckTemplateViewDTO != null && FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null && FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Count > 0)
            {
                ExistingUIDs = FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList
                .Select(item => item.UID)
                .ToList();

            }
        }
        public override async Task<bool> CreateUpdateIRouteLoadTruckTemplateDTO()
        {
            try
            {
                if (RouteLoadTruckTemplateUID != null)
                {
                    UpdateChangedProperties();
                    return await CreateUpdateLoadTemplate(false);
                }
                else
                {
                   return  await CreateUpdateLoadTemplate(true);
                }
                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
           
            return false;
        }

        public async Task<bool> CreateUpdateLoadTemplate(bool IsCreate)
        {
            ApiResponse<string> apiResponse = null;
            if(IsCreate)
            {
                apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/CreateRouteLoadTruckTemplateAndLine", HttpMethod.Post, DisplayRouteLoadTruckTemplateViewDTO);

            }
            else
            {
                apiResponse = await _apiService.FetchDataAsync(
                 $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/UpdateRouteLoadTruckTemplateAndLine", HttpMethod.Put, DisplayRouteLoadTruckTemplateViewDTO);

            }
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                {
                    return apiResponse.IsSuccess;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public void UpdateChangedProperties()
        {

            //DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedBy = _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedBy;
            DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedTime = DateTime.Now;
            DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateName = TemplateName;
            DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateDescription = TemplateDescription;

        }
        public override async Task GetSKUMasterData()
        {
            try
            {
                if (SkuList == null || SkuList.Count <= 0)
                {
                    PagingRequest pagingRequest = new PagingRequest();
                    pagingRequest.PageSize = 10;
                    pagingRequest.PageNumber = 1;
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",

                        HttpMethod.Post, pagingRequest);

                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                        PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData> selectionSKUs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>(data);
                        if (selectionSKUs.PagedData != null)
                        {
                            SkuMasterData = selectionSKUs.PagedData.ToList();
                            SkuAttributesList = await FindCompleteSKUAttributes(SkuMasterData);
                            SkuList = await FindCompleteSKU(SkuMasterData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
            
        }

        public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> FindCompleteSKUAttributes(IEnumerable<SKUMasterData> sKUMasters)
        {
            var skuAttributesList = new List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(); // Declare the list here

            try
            {
                if (sKUMasters != null)
                {
                    foreach (var skuMaster in sKUMasters)
                    {
                        foreach (var skuAttributes in skuMaster.SKUAttributes)
                        {
                            skuAttributesList.Add(skuAttributes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }

            return skuAttributesList;
        }
        public override async Task OnselectRoue(DropDownEvent dropDown)
        {
            SelectedRouteUID = dropDown.SelectionItems[0].UID;
            SelectedRouteName = dropDown.SelectionItems[0].Label;
            SelectedRoute = dropDown.SelectionItems[0].Label;
        }
        public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> FindCompleteSKU(IEnumerable<SKUMasterData> sKUMasters)
        {
            var skuList = new List<Winit.Modules.SKU.Model.Interfaces.ISKU>(); // Declare the list here

            try
            {
                if (sKUMasters != null)
                {
                    foreach (var skuMaster in sKUMasters)
                    {
                        //foreach (var skuAttributes in skuMaster.SKU)
                        //{
                        skuList.Add(skuMaster.SKU);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }

            return skuList;
        }
        protected async Task FetchRoutesForSelection()
        {
            if (RouteListForSelection == null)
            {
                RouteListForSelection = new List<ISelectionItem>();
                if (RouteList != null)
                {
                    foreach (var route in RouteList)
                    {
                        if (SelectedRouteUID != null && SelectedRouteUID == route.UID)
                        {
                            RouteListForSelection.Add(new SelectionItem { UID = route.UID, IsSelected = true, Code = route.Code, Label = route.Name });
                            SelectedRoute = route.Name;
                        }
                        else
                        {
                            RouteListForSelection.Add(new SelectionItem { UID = route.UID, IsSelected = false, Code = route.Code, Label = route.Name });
                        }
                    }
                }

            }
        }
        public override async Task GetRoutes()
        {
            //var newOrgUID = _appUser.SelectedJobPosition?.OrgUID ?? "";
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={_appUser.SelectedJobPosition?.OrgUID ?? ""}",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Route.Model.Classes.Route> fetchedapiData = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>(data);
                    if (fetchedapiData.PagedData != null)
                    {
                        RouteList = fetchedapiData.PagedData.ToList();

                        await FetchRoutesForSelection();



                    }

                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }

        }
        public override async Task ApplySearch(string searchString)
        {
            try
            {

                _propertiesToSearch.Add("SKUCode");
                _propertiesToSearch.Add("Name");
                if (_filter != null)
                {

                    DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList = await _filter.ApplySearch<RouteLoadTruckTemplateLine>(
                            FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList, searchString, _propertiesToSearch);
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

        }
        public override void CreateRouteLoadTruckTemplate()
        {
            var routeLoadTruckTemplate = new Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplate
            {
                UID = Guid.NewGuid().ToString(),
                CreatedBy = _appUser.Emp.UID,
                CreatedTime = DateTime.Now,
                ModifiedBy = _appUser.Emp.UID,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                RouteUID = SelectedRouteUID,
                TemplateName = TemplateName,
                TemplateDescription = TemplateDescription,
                CompanyUID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F",
                //OrgUID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7F",
                OrgUID = _appUser.SelectedJobPosition.OrgUID,
            };


            DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate = routeLoadTruckTemplate;
        }

        public override void CreateInstancesOfTemplateDTO()
        {
            if (DisplayRouteLoadTruckTemplateViewDTO == null)
            { DisplayRouteLoadTruckTemplateViewDTO = new RouteLoadTruckTemplateViewDTO(); }
            if (DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList == null)
            { DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList = new List<RouteLoadTruckTemplateLine>(); }
        }
        public override void CreateRouteLoadTruckTemplateLine(ISelectionItem selectionItem)
        {

            var newRowProduct = new RouteLoadTruckTemplateLine
            {
                UID = Guid.NewGuid().ToString(),
                CreatedBy = _appUser.Emp.UID,
                LineNumber = LineNumber + 1,
                CreatedTime = DateTime.Now,
                ModifiedBy = _appUser.Emp.UID,
                ModifiedTime = DateTime.Now, 
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                OrgUID = _appUser.SelectedJobPosition.OrgUID,
                CompanyUID = "WINIT",
                RouteLoadTruckTemplateUID = RouteLoadTruckTemplateUID != null ? RouteLoadTruckTemplateUID : DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.UID,
                SKUCode = selectionItem.Code,
                UOM = "OU",
                MondayQty = 0,
                MondaySuggestedQty = 0,
                TuesdayQty = 0,
                TuesdaySuggestedQty = 0,
                WednesdayQty = 0,
                WednesdaySuggestedQty = 0,
                ThursdayQty = 0,
                ThursdaySuggestedQty = 0,
                FridayQty = 0,
                FridaySuggestedQty = 0,
                SaturdayQty = 0,
                SaturdaySuggestedQty = 0,
                SundayQty = 0,
                SundaySuggestedQty = 0,// Set the value for the new property

                ActionTypes = Winit.Shared.Models.Enums.ActionType.Add
            };
            DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Add(newRowProduct);
        }
    }
}
