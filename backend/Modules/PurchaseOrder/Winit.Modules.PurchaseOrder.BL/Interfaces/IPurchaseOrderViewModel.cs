using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Classes;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderViewModel
{
    public string UserCode { get; set; }
    public bool IsNewOrder { get; set; }
    public bool IsQtyDisabled { get; }
    public bool IsDraftOrder { get; set; }
    IPurchaseOrderHeader PurchaseOrderHeader { get; }
    public List<ISKUV1> SKUs { get; }
    public List<string> PurchaseOrderUIDs { get; set; }
    public string PurchaseOrderUID { get; set; }
    List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    public List<ISelectionItem> WareHouseSelectionItems { get; }
    public List<ISelectionItem> TemplateSelectionItems { get; }
    public List<ISelectionItem> ShippingAddressSelectionItems { get; }
    public List<ISelectionItem> BillingAddressSelectionItems { get; }
    public List<ISelectionItem> OrgUnitSelectionItems { get; }
    public List<ISelectionItem> OrganizationUnitSelectionItems { get; }
    public List<ISelectionItem> DivisionSelectionItems { get; }
    public List<ISelectionItem> ProductCategorySelectionItems { get; }
    public List<IPurchaseOrderItemView> PurchaseOrderItemViews { get; }
    public List<IPurchaseOrderItemView> FilteredPurchaseOrderItemViews { get; }
    public IStoreItemView? SelectedStoreViewModel { get; set; }
    public List<IAppliedTax> AppliedTaxes { get; set; }
    public Dictionary<string, ITax> TaxDictionary { get; set; }
    public List<string> InvoiceApplicableTaxes { get; set; }
    public IPurchaseOrderTaxCalculator _purchaseOrderTaxCalculator { get; set; }
    public string StoreSearchString { get; set; }
    public string ProductSearchString { get; set; }
    public bool IsReassignButtonNeeded { get; set; }
    public IEnumerable<IStore> FilteredStores { get; }
    public string? SelectedDistributor { get; set; }
    public string? CurrencyUID { get; set; }
    public string? CurrencyLabel { get; set; }
    #region Approval Fields
    public string ApprovalCreatedBy { get; set; }
    public int RuleId { get; set; }
    public int RequestId { get; set; }
    public ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
    #endregion
    public string UserRoleCode { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableLimit { get; set; }
    public decimal CurrentOutStanding { get; set; }
    public IStoreMaster? SelectedStoreMaster { get; set; }
    public List<string> CreatedPurchaseOrderNumbers { get; set; }
    public int ApprovalLevel { get; set; }
    Dictionary<string, string> PurchaseOrderUIDEmpKVPair { get; set; }
    public IStoreCreditLimit? StoreCreditLimit { get; set; }
    public bool IsPoEdited { get; set; }
    public bool IsPoCreatedByCP { get; set; }
    decimal CrediLimitBufferPercentage { get; }
    Task PopulateViewModel(string source, string orderUID = "");
    Task OnDistributorSelect();
    void ClearOrgUnitSelection();
    Task OnOrgUnitSelect(string orgUid, bool isDraftNotNeed = false);
    Task OnShipToAddressSelect(string uid);
    void ClearShippingAddressSelection();
    Task OnTemplateSelect(string templateUID);
    Task ClearTemplateItems();
    Task PrepareDistributors();
    Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID);
    bool FilterAction(List<FilterCriteria> filterCriterias, ISKUV1 sKUV1);
    void ApplyGridFilter();
    Task AddProductsToGridBySKUUIDs(List<string> sKUs);
    Task OnQtyChange(IPurchaseOrderItemView purchaseOrderItemView);
    void UpdateNetAmount(IPurchaseOrderItemView purchaseOrderItemView);
    void Validate();
    Task DeleteSelectedItems();
    Task SaveOrder(string purchaseOrderStatus = PurchaseOrderStatusConst.Draft);
    Task InsertDataInIntegrationDB();
    void ApplyApprovedQty();
    Task<bool> SaveAllApprovalRequestDetails(string RequestId);
    Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID);
    Task<List<IUserHierarchy>?> GetAllUserHierarchyFromAPIAsync(string hierarchyType, string hierarchyUID, int ruleId);
    Task CreateApproval();
    Task ValidateCreditLimit();
    void ApplySellInScheme(IPurchaseOrderItemView purchaseOrderItemView);
    Task SendEmail(List<string> smsTemplates);
    Task SendSms(List<string> smsTemplates);
}
