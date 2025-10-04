using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnOrderBL
{
    Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>> SelectAllReturnOrderDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<Model.Interfaces.IReturnOrder> SelectReturnOrderByUID(string UID);
    Task<int> CreateReturnOrder(Model.Interfaces.IReturnOrder returnOrder);
    Task<int> UpdateReturnOrder(Model.Interfaces.IReturnOrder returnOrder);
    Task<int> DeleteReturnOrder(string UID);
    Task<int> CreateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMaster);
    Task<int> UpdateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO updateReturnOrderMaster);
    Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView>> GetReturnSummaryItemView(DateTime startDate,
        DateTime endDate, string storeUID = "",List<FilterCriteria>? filterCriterias = null);
    Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderMasterByUID(string UID);
    Task<Model.Interfaces.IReturnOrderMaster> SelectReturnOrderMasterByUID(string UID);
    Task<int> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string Status);
    Task<IReturnOrderInvoiceMaster> GetReturnOrderInvoiceMasterByUID(string returnOrderUID);
}
