using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
namespace Winit.Modules.WHStock.BL.Classes
{
    public class WHStockBL : IWHStockBL
    {
        protected readonly DL.Interfaces.IWHStockDL _whStockBL = null;
        IServiceProvider _serviceProvider = null;
        public WHStockBL(DL.Interfaces.IWHStockDL whStockBL, IServiceProvider serviceProvider)
        {
            _whStockBL = whStockBL;
            _serviceProvider = serviceProvider;
        }
        public async Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel)
        {
            return await _whStockBL.CUDWHStock(wHRequestTempleteModel);
        }

        public async Task<PagedResponse<IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StockType)
        {
            return await _whStockBL.SelectLoadRequestData(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, StockType);
        }

        public async Task<Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView> SelectLoadRequestDataByUID(string UID)
        {
            var (WHStockRequest, WHStockLineList) = await _whStockBL.SelectLoadRequestDataByUID(UID);
            Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView LoadRequestItemView = _serviceProvider.CreateInstance<Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView>();
            if (WHStockRequest != null && WHStockRequest.Count > 0)
            {
                LoadRequestItemView.WHStockRequest = WHStockRequest.FirstOrDefault();
            }
            if (WHStockLineList != null && WHStockLineList.Count > 0)
            {
                LoadRequestItemView.WHStockRequestLines= WHStockLineList;
            }
            return LoadRequestItemView;
        }
        public async Task<int> CUDWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines)
        {
            return await _whStockBL.CUDWHStockRequestLine(wHStockRequestLines);
        }

    }
}
