using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.WHStock.BL.Classes
{
    public class LoadRequestViewModel: LoadRequestWebViewModel
    {
        public LoadRequestViewModel(IServiceProvider serviceProvider,

     IFilterHelper filter,
     ISortHelper sorter,
     IListHelper listHelper,
     IAppUser appUser,
     IWHStockBL iWHStockBL,
       ISKUBL sKUBL,
     Winit.Shared.Models.Common.IAppConfig appConfigs,
     IRouteBL iRouteBL,
     Base.BL.ApiService apiService,
     Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting) : base(serviceProvider, filter, sorter, listHelper,
         appUser, iWHStockBL, sKUBL, appConfigs, iRouteBL,
         apiService, appSetting)
        {


        }
        public override async Task PopulateViewModel(string apiParam = null)
        {
            await GetLoadRequestDataByStatus(apiParam);
        }

        public override async Task ApplyFilter(List<FilterCriteria> filterCriterias,string ActiveTab)
        {
            try
            {
                FilterCriterias.Clear();
                FilterCriterias.AddRange(filterCriterias);
                await GetLoadRequestDataByStatus(ActiveTab);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
       
        public async Task GetLoadRequestDataByStatus(string ActiveTab)
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;

                var jsonData = JsonConvert.SerializeObject(pagingRequest);

                //PagingRequest pagingRequest = new PagingRequest();
                ////pagingRequest.PageSize = 10; var jsonData = JsonConvert.SerializeObject(filterCriteria);
                ////pagingRequest.PageNumber = 1;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}WHStock/SelectLoadRequestData?StockType={ActiveTab}",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<WHStockRequestItemViewUI> fetchedApiData = JsonConvert.DeserializeObject<PagedResponse<WHStockRequestItemViewUI>>(data);
                    if (fetchedApiData.PagedData != null)
                    {
                        // DisplayWHStockRequestItemView = new List<IWHStockRequestItemView>();
                        DisplayWHStockRequestItemView = fetchedApiData.PagedData.Cast<IWHStockRequestItemViewUI>().ToList();
                        
                    }
                }

            }

            catch (Exception ex)
            {

            }

        }
        public override async Task GetVehicleDropDown()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetVehicleDropDown?parentUID={_appUser.SelectedJobPosition.OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string jsonData = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                   var displayRequestToDDL = JsonConvert.DeserializeObject<List<SelectionItem>>(jsonData);
                    DisplayRequestToDDL = displayRequestToDDL.Cast<ISelectionItem>().ToList();
                }
                else
                {
                 
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override async Task GetRequestFromDropDown()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetRequestFromDropDown?parentUID={_appUser.SelectedJobPosition.OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string jsonData = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    var displayRequestFromDDL = JsonConvert.DeserializeObject<List<SelectionItem>>(jsonData);
                    DisplayRequestFromDDL = displayRequestFromDDL.Cast<ISelectionItem>().ToList();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
