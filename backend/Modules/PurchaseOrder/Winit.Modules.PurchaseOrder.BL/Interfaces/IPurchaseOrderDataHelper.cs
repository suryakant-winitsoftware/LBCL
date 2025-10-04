using Microsoft.Identity.Client;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderDataHelper
{
    Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData();
    Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID);
    Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>?> GetWareHouseData(string orgTypeUID, string parentUID);
    Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest);
    Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(List<SortCriteria>? sortCriterias = null,
        int? pageNumber = null, int? pageSize = null, List<FilterCriteria>? filterCriterias = null,
        bool? isCountRequired = null, string? type = null);
    Task<IStoreMaster?> GetStoreMasterByStoreUID(string storeUID);
    Task<bool> SaveOrder(List<IPurchaseOrderMaster> purchaseOrderMaster, ApprovalRequestItem? approvalRequestItem = null);
    Task<bool> CreateMailRequest(List<string> OrderUIDs);
    Task<List<IStore>?> GetChannelPartner(string jobPositionUid);
    Task<IPurchaseOrderMaster?> GetPurchaseOrderMasterByUID(string uID);
    Task<bool> DeletePurchaseOrderLinesByUIDs(IEnumerable<string> purchaseOrderLineItems);
    Task<List<ISelectionItem>?> GetProductOrgSelectionItems();
    Task<List<ISelectionItem>?> GetProductDivisionSelectionItems();
    Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID);
    Task<List<IPurchaseOrderTemplateHeader>?> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(string storeUID, string? createdBy = null);
    Task<List<IPurchaseOrderTemplateLine>?> GetAllPurchaseOrderTemplateLines(string purchaseOrderTemplateHeaderUID);
    Task<List<ISellInSchemePO>?> GetSellInSchemesByOrgUidAndSKUUid(string OrgUid, List<string>? skus = null);
    Task<List<ISellInSchemePO>?> GetSellInSchemesByPOUid(string POUid);
    Task<List<IQPSSchemePO>?> GetQPSSchemesByStoreUIDAndSKUUID(string StoreUid, DateTime order_date, List<ISKUFilter>? skus = null);
    Task<List<IQPSSchemePO>?> GetQPSSchemesByPOUid(string pouid, List<ISKUFilter>? skus = null);
    Task<bool> SaveApprovalRequestDetails(string RequestId, string PurchaseOrderUID);
    Task<List<IAllApprovalRequest>> GetAllApproveListDetailsFromAPIAsync(string UID);
    Task<bool> InsertDataInIntegrationDB(IPendingDataRequest pendingDataRequest);
    Task<List<IUserHierarchy>?> GetAllUserHierarchyFromAPIAsync(string hierarchyType, string hierarchyUID, int ruleId);
    Task<IOrg?> GetOrgByUID(string orgUID);
    Task<List<IOrg>?> GetOrgByOrgTypeUID(string orgTypeUID);

    Task<List<IAsmDivisionMapping>?> GetAsmDivisionMappingByUID(string linkedItemType, string linkedItemUID, string? asmEmpUID = null);
    Task<string?> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID);
    Task<IStoreCreditLimit?> GetCurrentLimitByStoreAndDivision(string storeUID, string divisionUID);

    Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem? approvalRequestItem);
    Task<List<IPurchaseOrderCreditLimitBufferRange>?> GetPurchaseOrderCreditLimitBufferRanges();
    Task<Dictionary<string, StandingSchemeResponse>?> GetStandingSchemesByOrgUidAndSKUUid(string orgUID, DateTime orderDate, List<ISKUFilter> skuFilterList);
    Task<Dictionary<string, StandingSchemeResponse>?> GetStandingSchemesByPOUid(string pouid, List<ISKUFilter> skuFilterList);
    Task<bool> InsertEmailIntoRabbitMQ(List<string> smsTemplates, List<string> OrerUIDs);
    Task<bool> InsertSmsIntoRabbitMQ(List<string> smsTemplates, List<string> OrerUIDs);
    Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate);
}
