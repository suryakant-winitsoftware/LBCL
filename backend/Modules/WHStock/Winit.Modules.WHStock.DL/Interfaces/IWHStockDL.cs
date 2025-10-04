using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.DL.Interfaces
{
    public interface IWHStockDL
    {
        Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel);
        Task<PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StockType);
        Task<(List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>,List<IWHStockRequestLineItemView>)> SelectLoadRequestDataByUID(string UID);
        Task<int> CUDWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null);
    }
}
