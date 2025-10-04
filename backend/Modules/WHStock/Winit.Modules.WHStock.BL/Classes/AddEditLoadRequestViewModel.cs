using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class AddEditLoadRequestViewModel: AddEditLoadRequestWebViewModel
    {
        public AddEditLoadRequestViewModel(IServiceProvider serviceProvider,

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
        //public override async Task PopulateViewModel(string apiParam=null)
        //{
        //    await GetLoadRequestByUID(apiParam);
        //}
        public override async Task PopulateViewModel(string apiParam = null)
        {
            OrgUID = _appUser.SelectedJobPosition.OrgUID;
            await GetLoadRequestByUID(apiParam);
        }
        public  async Task GetLoadRequestByUID(string uid)
        {
            try
            {


                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageSize = 10;
                pagingRequest.PageNumber = 1;
                ApiResponse<ViewLoadRequestItemViewUI> apiResponse =
                 await _apiService.FetchDataAsync<ViewLoadRequestItemViewUI>(
                 $"{_appConfigs.ApiBaseUrl}WHStock/SelectLoadRequestDataByUID?UID={uid}",
                 HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {

                    WHStockRequestItemview = (IWHStockRequestItemViewUI)apiResponse.Data.WHStockRequest;
                    DisplayWHStockRequestLineItemview = apiResponse.Data.WHStockRequestLines.Cast<IWHStockRequestLineItemViewUI>().ToList();


                }


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    
}
