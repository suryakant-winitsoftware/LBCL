using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINITSharedObjects.Enums;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class LoadRequestWebViewModel: LoadRequestBaseViewModel
    {
       
        public LoadRequestWebViewModel(IServiceProvider serviceProvider,

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

            FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
        }
        public override async Task GetRoutesByOrgUID(string OrgUID)
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

    }
}
