using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces;

public interface ISalesOrderViewModel
{
 
    public bool IsInitialized { get; set; }
    public Int64 Id { get; set; }
    public string SalesOrderUID { get; set; }
    public string SalesOrderNumber { get; set; }
    public string DraftOrderNumber { get; set; }
    public string OrderType { get; set; }
    public string OrgUID { get; set; }
    public string DistributionChannelOrgUID { get; set; }
    public string RouteUID { get; set; }
    public string StoreUID { get; set; }
    public string Status { get;set; }
    public DateTime OrderDate { get; set; }
    public string CustomerPO { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime DeliveredDateTime { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> SalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> FilteredSalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> DisplayedSalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> SelectedSalesOrderItemViews { get; set; }        
    public List<Model.UIInterfaces.ISalesOrderItemView> DisplayedSalesOrderItemViews_Preview { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> FOCSalesOrderItemViews { get; set; }
    public List<ISelectionItem> DistributorsList { get; set; }
    public ISelectionItem SelectedDistributor { get; set; }
    public string CustomerSignatureFolderPath { get; set; }
    public string UserSignatureFolderPath { get; set; }
    public string CustomerSignatureFileName { get; set; }
    public string UserSignatureFileName { get; set; }
    public string CustomerSignatureFilePath { get; set; }
    public string UserSignatureFilePath { get; set; }
    public bool IsSignaturesCaptured { get; set; }
    public List<IFileSys> SignatureFileSysList { get; set; }
    List<ISKUV1> SKUs { get; set; }
    List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    //Web Specific Fields
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
    public List<ISelectionItem> SalesTypeSelectionItems { get; set; }

    public string CurrencyUID { get; set; }
    /// <summary>
    /// Will be used to show Currency Label
    /// </summary>
    public string CurrencyLabel { get; set; }
    public int LineCount { get; set; }
    public decimal QtyCount { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public int CreditDays { get; set; }
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// Line Level Discount
    /// </summary>
    public decimal TotalLineDiscount { get; set; }
    /// <summary>
    /// TotalCashDiscount. Should be splitted to lines
    /// </summary>
    public decimal TotalCashDiscount { get; set; }
    /// <summary>
    /// Header Level Discount
    /// </summary>
    public decimal TotalHeaderDiscount { get; set; }
    /// <summary>
    /// TotalDiscount = TotalLineDiscount + TotalCashDiscount + TotalHeaderDiscount
    /// </summary>
    public decimal TotalDiscount { get; }
    /// <summary>
    /// Total Excise Duty
    /// </summary>
    public decimal TotalExciseDuty { get; set; }
    /// <summary>
    /// Line Tax Amount
    /// </summary>
    public decimal TotalLineTax { get; set; }
    /// <summary>
    /// Header Tax Amount
    /// </summary>
    public decimal TotalHeaderTax { get; set; }
    /// <summary>
    /// TotalTax = LineTaxAmount + HeaderTaxAmount
    /// </summary>
    public decimal TotalTax { get; }
    /// <summary>
    /// NetAmount = TotalAmount - TotalDiscount + ExciseDuty + TotalTax
    /// </summary>
    public decimal NetAmount { get; }
    public string Source { get; set; }
    public string ReferenceNumber { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreViewModel { get; set; }
    public List<FilterCriteria> FilterCriteriaList { get; set; } 
    public List<SortCriteria> SortCriteriaList { get; set; }
    public string Notes { get; set; }
    public string DeliveryInstructions { get; set; }
    public string Remarks { get; set; }
    public string Address { get; set; }
    public string CustomerName { get; set; }
    public List<string>? ApplicablePromotionList { get; set; }
    public Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>? DMSPromotionDictionary { get; set; }
    public Dictionary<string, List<ISelectionItem>>? PromotionItemMapDictionary { get; set; }
    public List<AppliedPromotionView>? AppliedPromotionViewList { get; set; }
    public string SelectedPromotionUID { get; set; }
    public Dictionary<string, ITax> TaxDictionary { get; set; }
    public List<string> InvoiceApplicableTaxes { get; set; }
    public List<IAppliedTax> AppliedTaxes { get; set; }
    public string VehicleUID { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public string BeatHistoryUID { get; set; }
    public string StoreHistoryUID { get; set; }
    public bool IsStockUpdateRequired { get; set; }
    public bool IsInvoiceGenerationRequired { get; set; }
    IJSRuntime _JS
    {
        get; set;
    }
    public ISalesOrderTaxCalculator _salesOrderTaxCalculator { get; set; }
    //Loaylty
    //SKU Replaced
    //MCL Status
    //Wastage Value
    /// <summary>
    /// Populate Initial Data
    /// </summary>
    Task PopulateViewModel(string source, Winit.Modules.Store.Model.Interfaces.IStoreItemView? storeViewModel = null, bool isNewOrder = true, string salesOrderUID = "");
    /// <summary>
    /// Dispose object Reference
    /// </summary>
    void Dispose();
    /// <summary>
    /// Apply Filter
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    Task ApplyFilter(List<FilterCriteria> filterCriterias, FilterMode filterMode);
    /// <summary>
    /// Apply Search
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns></returns>
    Task ApplySearch(string searchString);
    /// <summary>
    /// Apply Sort
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    Task ApplySort(List<SortCriteria> sortCriterias);
    public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForDDL(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView);
    public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForClone(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView);
    Task AddClonedItemToList(Model.UIInterfaces.ISalesOrderItemView item);
    Model.Interfaces.ISalesOrder ConvertToISalesOrder(Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModel viewModel);
    Model.Interfaces.ISalesOrderLine ConvertToISalesOrderLine(ISalesOrderItemView salesOrderItemView, int lineNumber);
    List<Model.Interfaces.ISalesOrderLine> ConvertToISalesOrderLine(List<ISalesOrderItemView> salesOrderItemViewList);
    ISKUUOMView ConvertToISKUUOMView(ISKUUOM sKUUOM);
    List<ISKUUOMView> ConvertToISKUUOMView(List<ISKUUOM> sKUUOMs);
    ISKUAttributeView ConvertToISKUAttributeView(ISKUAttributes sKUAttribute);
    Dictionary<string, ISKUAttributeView>? ConvertToISKUAttributeView(List<ISKUAttributes> sKUAttributes);
    Model.UIInterfaces.ISalesOrderItemView ConvertToISalesOrderItemView(ISKUMaster viewModel, int lineNumber, List<string>? skuImages = null);
    List<ISalesOrderItemView> ConvertToISalesOrderItemView(List<ISKUMaster> sKUMasters, List<IFileSys>? skuImages = null);
    Task OnQtyChange(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView);
    Task ApplyPromotion();
    Task UpdateHeader();
    Task RemoveItemFromList(ISalesOrderItemView salesOrderItemView);
    void SetSelectedSalesOrderItemViews();
    Task ApplySearch_Preview(string searchString);
    Task<bool> SaveSalesOrder(string StatusType = SalesOrderStatus.DRAFT);
    Task CalculateCashDiscount(decimal discountValue);
    void UpdateNotes(string notes);
    void UpdateCustomerPO(string customerPO);
    Task OnRouteSelect(string routeUID);
    Task OnStoreItemViewSelected(string storeItemViewUID);
    Task AddSelectedProducts(List<ISalesOrderItemView> salesOrderItemViews);
    Task ClearCartItems(List<ISalesOrderItemView> salesOrderItemViews);
    Task UpdateUnitPriceByUOMChange(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView);
    Task UpdateUnitPrice(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView, decimal unitPrice);
    Task<bool> UpdateSalesOrderStatus(string status);
    void OnSignatureProceedClick();
    List<ISelectionItem>? GetPromotionListForItem(ISalesOrderItemView salesOrderItemView);
    Task ApplySpecialPromotionFilter();
    Task RemoveSpecialPromotionFilter();
    Task<(List<ISelectionItem>, Dictionary<string, List<ISelectionItem>>?)> PrepareMissingPromotion();
    Task UpdateNetAmount(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView);
}
