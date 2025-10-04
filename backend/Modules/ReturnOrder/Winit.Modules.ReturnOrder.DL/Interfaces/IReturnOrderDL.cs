using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Interfaces;

public interface IReturnOrderDL
{
    Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>> SelectAllReturnOrderDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

    Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder> SelectReturnOrderByUID(string UID);
    Task<int> CreateReturnOrder(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder returnOrderDetails);

    Task<int> UpdateReturnOrder(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder returnOrderDetails);
    Task<int> DeleteReturnOrder(String UID);
    Task<int> CreateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMaster);
    Task<int> UpdateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO updateReturnOrderMaster);
    Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView>> GetReturnSummaryItemView(DateTime startDate, DateTime endDate, string storeUID = "",List<FilterCriteria>? filterCriterias = null);
    Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderMasterByUID(string UID);
    Task<(List<Model.Interfaces.IReturnOrder>, List<Model.Interfaces.IReturnOrderLine>)>
      SelectReturnOrderMasterByUID(string UID);
    Task<int> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string Status);
    Task<IReturnOrderInvoiceMaster> GetReturnOrderInvoiceMasterByUID(string returnOrderUID);
}
