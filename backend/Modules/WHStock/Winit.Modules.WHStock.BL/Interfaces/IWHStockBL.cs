using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.BL.Interfaces
{
    public interface IWHStockBL
    {
        Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel);
        Task<PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StockType);
        Task<Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView> SelectLoadRequestDataByUID(string UID);
        Task<int> CUDWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines);
    }
}
