using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderBL : IReturnOrderBL
{
    protected readonly DL.Interfaces.IReturnOrderDL _returnOrderDL = null;
    IServiceProvider _serviceProvider = null;
    public ReturnOrderBL(DL.Interfaces.IReturnOrderDL returnOrderDL, IServiceProvider serviceProvider)
    {
        _returnOrderDL = returnOrderDL;
        _serviceProvider = serviceProvider;
    }
    public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>> SelectAllReturnOrderDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _returnOrderDL.SelectAllReturnOrderDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<Model.Interfaces.IReturnOrder> SelectReturnOrderByUID(string UID)
    {
        return await _returnOrderDL.SelectReturnOrderByUID(UID);
    }
    public async Task<int> CreateReturnOrder(Model.Interfaces.IReturnOrder returnOrder)
    {
        return await _returnOrderDL.CreateReturnOrder(returnOrder);
    }
    public async Task<int> UpdateReturnOrder(Model.Interfaces.IReturnOrder ReturnOrder)
    {
        return await _returnOrderDL.UpdateReturnOrder(ReturnOrder);
    }
    public async Task<int> DeleteReturnOrder(string UID)
    {
        return await _returnOrderDL.DeleteReturnOrder(UID);
    }

    public async Task<int> CreateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMaster)
    {
        return await _returnOrderDL.CreateReturnOrderMaster(returnOrderMaster);
    }
    public async Task<int> UpdateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO updateReturnOrderMaster)
    {
        return await _returnOrderDL.UpdateReturnOrderMaster(updateReturnOrderMaster);
    }
    public async Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView>> GetReturnSummaryItemView
        (DateTime startDate, DateTime endDate, string storeUID = "", List<FilterCriteria>? filterCriterias = null)
    {
        return await _returnOrderDL.GetReturnSummaryItemView(startDate, endDate, storeUID,filterCriterias);
    }
    public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderMasterByUID(string UID)
    {
        return await _returnOrderDL.GetReturnOrderMasterByUID(UID);
    }
    public async Task<Model.Interfaces.IReturnOrderMaster> SelectReturnOrderMasterByUID(string UID)
    {
        var (returnOrdersList, returnOrderLineList) = await _returnOrderDL.SelectReturnOrderMasterByUID(UID);
        Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = _serviceProvider.CreateInstance<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster>();
        returnOrderMaster.ReturnOrder = returnOrdersList.FirstOrDefault();
        if(returnOrderLineList != null && returnOrderLineList.Count > 0)
        {
            returnOrderMaster.ReturnOrderLineList = returnOrderLineList;
        }
        return returnOrderMaster;
    }

    public async Task<int> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string Status)
    {
        return await _returnOrderDL.UpdateReturnOrderStatus(returnOrderUIDs, Status);
    }
    public async Task<IReturnOrderInvoiceMaster> GetReturnOrderInvoiceMasterByUID(string returnOrderUID)
    {
        return await _returnOrderDL.GetReturnOrderInvoiceMasterByUID(returnOrderUID);
    }
}
