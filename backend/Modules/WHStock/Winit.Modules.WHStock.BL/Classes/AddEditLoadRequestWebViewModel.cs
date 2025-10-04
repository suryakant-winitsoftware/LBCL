using Nest;
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
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class AddEditLoadRequestWebViewModel : AddEditLoadRequestBaseViewModel
    {
        public AddEditLoadRequestWebViewModel(IServiceProvider serviceProvider,
     IFilterHelper filter,
     ISortHelper sorter,
     IListHelper listHelper,
     IAppUser appUser,
     IWHStockBL iWHStockBL,
       ISKUBL sKUBL,
     Winit.Shared.Models.Common.IAppConfig appConfigs,
     IRouteBL iRouteBL,
     Base.BL.ApiService apiService,
     Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
     StockUpdater.BL.Interfaces.IStockUpdaterBL stockUpdaterBL) : base(serviceProvider, filter, sorter, listHelper,
         appUser, iWHStockBL, sKUBL, appConfigs, iRouteBL,
         apiService, appSetting, stockUpdaterBL)
        {


        }
        
        public override async Task<bool> CUDWHStock(string btnText)
        {

            await UpdateQuantities(btnText);
            string jsonBody = JsonConvert.SerializeObject(pendingLoadRequestTemplateModel);
            string apiUrl = $"{_appConfigs.ApiBaseUrl}WHStock/CUDWHStock";
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, pendingLoadRequestTemplateModel);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                //SelectedWHStockRequestLineItemViewUI = null;
                //WHRequestTempletemodel = null;

                return apiResponse.IsSuccess;
            }

            return false;
        }
        public override async Task GetSKUMasterData()
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

                        await FetchSKUAttributes(SkuMasterData);
                        await FetchSKUUOMs(SkuMasterData);
                        await FetchSKUs(SkuMasterData);
                        await GetRoutesByOrgUID(_appUser.SelectedJobPosition.OrgUID);
                        await GetOrgByUID(_appUser.SelectedJobPosition.OrgUID);
                        await FetchRouteForSelection();
                    }
                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }

        }
        public async Task GetRoutesByOrgUID(string OrgUID)
        {
            try
            {


                PagingRequest pagingRequest = new PagingRequest();
                //pagingRequest.PageSize = 10;
                //pagingRequest.PageNumber = 1;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={OrgUID}",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Route.Model.Classes.Route> fetchApiData = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>(data);
                    if (fetchApiData.PagedData != null)
                    {
                        RouteList = fetchApiData.PagedData.Cast<Winit.Modules.Route.Model.Interfaces.IRoute>().ToList();

                    }
                }


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public async Task GetOrgByUID(string uid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageSize = 10;
                pagingRequest.PageNumber = 1;

                ApiResponse<Winit.Modules.Org.Model.Classes.Org> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.Org.Model.Classes.Org>(
                $"{_appConfigs.ApiBaseUrl}Org/GetOrgByUID?UID={uid}",
                HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {

                    OrgsList = apiResponse.Data;
                }


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        private readonly IViewLoadRequestItemView _viewLoadRequestItemView;
        public void SetSelectedRoute(IRoute route)
        {
            SelectedRoute = route;
        }
    }
}
