using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces;

public interface ISalesOrderDataHelper
{
    Task<Model.Interfaces.ISalesOrder?> GetSalesOrderByUID(string salesOrderUID);
    Task<List<Model.Interfaces.ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID);
    Task<List<ISKUMaster>> PopulateSKUMaster(List<string> orgUIDs);
    Task<List<ISKUV1>> GetSKUs(PagingRequest pagingRequest)
    {
        return null;
    }
    Task<List<ISKUMaster>> PopulateSKUMaster(SKUMasterRequest request)
    {
        return null;
    }
    Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(List<SortCriteria>? sortCriterias = null, int? pageNumber = null,
        int? pageSize = null, List<FilterCriteria>? filterCriterias = null, bool? isCountRequired = null, string skuPriceList = null);
    Task<bool> SaveSalesOrder_Order(SalesOrderViewModelDCO salesOrderViewModelDCO);
    Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>>
        GetVanStockItems(string vanUID, string orgUID, StockType stockType);
    Task<List<IFileSys>?> GetFileSys(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs = null);
    Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderHeaderDetails(string salesOrderUID);
    Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderPrintViewslines(string salesOrderUID);
    List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView,
            Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority);
    Task<int> CreateFileSysForBulk(List<IFileSys> fileSysList);
    Task<List<Winit.Modules.Route.Model.Interfaces.IRoute>> GetAllRoutesAPIAsync();
    Task<SalesOrderViewModelDCO?> GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID);
    Task<List<IStoreItemView>> GetStoreItemViewsDataFromAPIAsync(string routeUID);
    Task<List<StoreMasterDTO>> GetStoreMastersbyStoreUIDs(List<string> storeUIDs);
    Task<IStoreHistory> GetStoreHistoryAPI(string routeUID, string visitDate, string storeUID);
    Task<bool> UpdateSalesOrderStatusApiAsync(Winit.Modules.SalesOrder.Model.Classes.SalesOrderStatusModel salesOrderStatusModel);
    Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID);
    Task<List<IOrg>?> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
    {
        return default;
    }

    Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData();
    Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID);
    Task<List<IOrg>> GetWareHouseData(string orgTypeUID);
    Task<IEnumerable<IOrg>> GetDistributorslistByType(string type);
}
