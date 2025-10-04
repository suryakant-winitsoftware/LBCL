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
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class LoadRequestAppViewModel: LoadRequestBaseViewModel
    {

        public LoadRequestAppViewModel(IServiceProvider serviceProvider,

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
            await GetLoadRequestData(apiParam);
        }
        public async Task GetLoadRequestData(string activeTab)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                if (_IWHStockBL != null)
                {
                    var pagedResponse = await _IWHStockBL.SelectLoadRequestData(
                        pagingRequest?.SortCriterias,
                        pagingRequest?.PageNumber ?? 1,
                        pagingRequest?.PageSize ?? 10,
                        pagingRequest?.FilterCriterias,
                        pagingRequest?.IsCountRequired ?? false,
                        activeTab
                    );
                    if (pagedResponse.PagedData != null)
                    {
                        var displayWHStockRequestItemView = pagedResponse.PagedData.ToList();
                        DisplayWHStockRequestItemView = ConvertLoadRequestData(displayWHStockRequestItemView);

                        DisplayWHStockRequestItemView = DisplayWHStockRequestItemView
                                                    .OrderByDescending(line => line.ModifiedTime)
                                                    .ToList();
                    }
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public List<IWHStockRequestItemViewUI> ConvertLoadRequestData(List<IWHStockRequestItemView> wHStockRequestItemView)
        {
            List<IWHStockRequestItemViewUI> result = new List<IWHStockRequestItemViewUI>();

            if (wHStockRequestItemView != null)
            {
                foreach (var item in wHStockRequestItemView)
                {
                    IWHStockRequestItemViewUI itemViewUI = new Winit.Modules.WHStock.Model.Classes.WHStockRequestItemViewUI
                    {
                        UID = item.UID,
                        RequestCode = item.RequestCode,
                        RequestType = item.RequestType,
                        RouteCode = item.RouteCode,
                        RouteName = item.RouteName,
                        SourceCode = item.SourceCode,
                        SourceName = item.SourceName,
                        TargetCode = item.TargetCode,
                        TargetName = item.TargetName,
                        Status = item.Status,
                        Remarks = item.Remarks,
                        RequestedTime = item.RequestedTime,
                        RequiredByDate = item.RequiredByDate,
                        ModifiedTime = item.ModifiedTime
                    };

                    result.Add(itemViewUI);
                }
            }

            return result;
        }

    }
}
