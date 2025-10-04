using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Winit.Modules.Common.BL.Classes;
using System.Security.Cryptography;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Enums;
using Microsoft.Extensions.Localization;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using System.Globalization;
using System.Resources;
namespace Winit.Modules.Route.BL.Classes
{
    
    public class RouteLoadBaseViewModel : IRouteLoadViewModel
    {

        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<RouteLoadTruckTemplateViewUI> LoadTemplateOriginalList = new List<RouteLoadTruckTemplateViewUI>();
        public List<IRouteLoadTruckTemplateUI> FilterRouteLoadTruckTemplateView { get; set; } = new List<IRouteLoadTruckTemplateUI>();
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }

        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        List<Winit.Modules.SKU.Model.Classes.SKUMasterData> SkuMasterData;
        public List<Modules.Route.Model.Classes.Route> RouteList { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public List<IRouteLoadTruckTemplateUI> DisplayRouteLoadTruckTemplateView { get; set; }
        public IServiceProvider _serviceProvider;
        public  IFilterHelper _filter;
        public  ISortHelper _sorter;
        //private readonly IPromotionManager _promotionManager;
        //private readonly Interfaces.IReturnOrderAmountCalculator _amountCalculator;
        public IListHelper _listHelper;
        IEnumerable<ISKUMaster> SKUMasterList;
        public IAppUser _appUser;
        public List<string> _propertiesToSearch = new List<string>();
        public IAppConfig _appConfigs;
        public Base.BL.ApiService _apiService;
        public List<Winit.Modules.Route.Model.Classes.Route> TemplateRouteList = new List<Winit.Modules.Route.Model.Classes.Route>();
        // private Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL _returnOrderBL;
        public SKU.BL.Interfaces.ISKUBL _sKUBL;
       
        public List<SkuSequence> SkuSequenceList { get; private set; }
        //public RouteLoadTruckTemplateViewDTO fetchedapiData { get; private set; }
        public RouteLoadTruckTemplateViewDTO DisplayRouteLoadTruckTemplateViewDTO { get; set; }
        public RouteLoadTruckTemplateViewDTO FilterRouteLoadTruckTemplateViewDTO { get; set; }
        public string SelectedRouteUID { get; set; }
        public string SelectedRouteName { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }

      
        public RouteLoadBaseViewModel(IServiceProvider serviceProvider,
       IFilterHelper filter,
       ISortHelper sorter,
       IListHelper listHelper,
       IAppUser appUser,

       IAppConfig appConfigs,
       Base.BL.ApiService apiService
   )
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            
        }
       
        public virtual async Task PopulateViewModel(string apiParam=null)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> DeleteRouteLoadTruckTemplate(string selectedUID)  
        {
            throw new NotImplementedException();
        }

        public virtual async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        { 
            throw new NotImplementedException();
        }

//if(pageName == PageNames.LoadTemplate)
//{
//    await GetRouteLoadAPIAsync();
//   await GetRouteAPIAsync("FR001");
//}
//else if(pageName == PageNames.AddEditLoadTemplate)
//{




//    await GetSKUMasterData();
//    if (apiParam != null)
//    { await GetRouteLoadTruckLineByUID(apiParam); }
//    await GetRouteAPIAsync("FR001");
//}





protected async Task BtnSelectRoute()
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

                        }
                        else
                        {
                            RouteListForSelection.Add(new SelectionItem { UID = route.UID, IsSelected = false, Code = route.Code, Label = route.Name });
                        }
                    }
                }

            }
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
                        //}
                        // return fetchedapiData;
                    }


                    else
                    {
                        // Log or handle the case where the response data or PagedData is null
                        Console.WriteLine("Invalid response format: Data or Paged Data is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"Error fetching data from API: {ex.Message}");
            }
            //return null;
        }
      

        public async Task<List<Winit.Modules.SKU.Model.Classes.SKUMasterData>> GetSKUMasterData()
        {
            try
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

            catch (Exception ex)
            {
                // Handle exceptions
            }
            return SkuMasterData;
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
        public async Task OnselectRoue(DropDownEvent dropDown)
        {
            SelectedRouteUID = dropDown.SelectionItems[0].UID;
            SelectedRouteName = dropDown.SelectionItems[0].Label;
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

        public async Task<bool> CreateUpdateIRouteLoadTruckTemplateLineDataFromAPIAsync(RouteLoadTruckTemplateViewDTO iRuteLodTrukTmplateLine, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(iRuteLodTrukTmplateLine);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/CreateRouteLoadTruckTemplateAndLine", HttpMethod.Post, iRuteLodTrukTmplateLine);
                }
                else
                {
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/UpdateRouteLoadTruckTemplateAndLine", HttpMethod.Put, iRuteLodTrukTmplateLine);
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
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

     

    }
}
