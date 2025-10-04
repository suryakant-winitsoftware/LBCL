using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Promotion.BL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesOrderDataAppHelper : ISalesOrderDataHelper
{
    private readonly Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL;
    private readonly Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL _salesOrderBL;
    private readonly ISKUBL _sKUBL;
    private readonly ISKUPriceBL _sKUPriceBL;
    private readonly Winit.Modules.Org.BL.Interfaces.IOrgBL _orgBl;
    private readonly IPromotionBL _promotionBL;

    public SalesOrderDataAppHelper(
        IFileSysBL fileSysBL,
        Interfaces.ISalesOrderBL salesOrderBL,
        ISKUBL sKUBL,
        ISKUPriceBL sKUPriceBL,
        Org.BL.Interfaces.IOrgBL orgBL,
        IPromotionBL promotionBL)

    {
        _fileSysBL = fileSysBL;
        _salesOrderBL = salesOrderBL;
        _sKUBL = sKUBL;
        _sKUPriceBL = sKUPriceBL;
        _orgBl = orgBL;
        _promotionBL = promotionBL;
    }

    public async Task<Model.Interfaces.ISalesOrder?> GetSalesOrderByUID(string salesOrderUID)
    {
        return await _salesOrderBL.GetSalesOrderByUID(salesOrderUID);
    }

    public async Task<List<Model.Interfaces.ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        return await _salesOrderBL.GetSalesOrderLinesBySalesOrderUID(salesOrderUID);
    }
    public async Task<List<ISKUMaster>> PopulateSKUMaster(List<string> orgUID)
    {
        return await _sKUBL.GetWinitCache("skumaster");
    }
    public async Task<bool> SaveSalesOrder_Order(SalesOrderViewModelDCO salesOrderViewModelDCO)
    {
        return await _salesOrderBL.SaveSalesOrder(salesOrderViewModelDCO) > 0;
    }
    public Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>>
        GetVanStockItems(string vanUID, string orgUID, StockType stockType)
    {
        return _orgBl.GetVanStockItems(vanUID, orgUID, stockType);
    }

    public async Task<List<IFileSys>?> GetFileSys(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs = null)
    {
        return await _fileSysBL.GetFileSysByLinkedItemType(linkedItemType, fileSysType, linkedItemUIDs);
    }

    public async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderHeaderDetails(
        string salesOrderUID)
    {
        return await _salesOrderBL.GetSalesOrderPrintView(salesOrderUID);
    }
    public async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderPrintViewslines(
        string salesOrderUID)
    {
        return (List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>)
            await _salesOrderBL.GetSalesOrderLinePrintView(salesOrderUID);
    }

    public Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(List<SortCriteria>? sortCriterias = null, int? pageNumber = null,
        int? pageSize = null, List<FilterCriteria>? filterCriterias = null, bool? isCountRequired = null, string skuPriceList = null)
    {
        return _sKUPriceBL.SelectAllSKUPriceDetails(sortCriterias, pageNumber ?? 0, pageSize ?? 0, filterCriterias, isCountRequired ?? false, skuPriceList);
    }

    public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView,
            Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
    {
        return _promotionBL.ApplyPromotion(applicablePromotionUIDs, promoHeaderView, promoDictionary, promotionPriority);
    }

    public Task<int> CreateFileSysForBulk(List<IFileSys> fileSysList)
    {
        return _fileSysBL.CreateFileSysForBulk(fileSysList);
    }

    public async Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID)
    {
        return await _orgBl.GetOrgHierarchyParentUIDsByOrgUID([orgUID]);
    }

    public Task<List<IRoute>> GetAllRoutesAPIAsync()
    {
        throw new NotImplementedException();
    }

    public Task<SalesOrderViewModelDCO?> GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID)
    {
        throw new NotImplementedException();
    }

    public Task<List<IStoreItemView>> GetStoreItemViewsDataFromAPIAsync(string routeUID)
    {
        throw new NotImplementedException();
    }

    public Task<List<StoreMasterDTO>> GetStoreMastersbyStoreUIDs(List<string> storeUIDs)
    {
        throw new NotImplementedException();
    }

    public Task<IStoreHistory> GetStoreHistoryAPI(string routeUID, string visitDate, string storeUID)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateSalesOrderStatusApiAsync(SalesOrderStatusModel salesOrderStatusModel)
    {
        throw new NotImplementedException();
    }

    public Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData()
    {
        throw new NotImplementedException();
    }

    public Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
    {
        throw new NotImplementedException();
    }

    public Task<List<IOrg>> GetWareHouseData(string orgTypeUID)
    {
        throw new NotImplementedException();
    }
    public Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDistributorslistByType(string type)
    {
        return _orgBl.GetOrgByOrgTypeUID(type, null);
    }
}
