using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnOrderViewModel
{
    public int Id { get; set; }
    public string ReturnOrderUID { get; set; }
    public string ReturnOrderNumber { get; set; }
    public string DraftOrderNumber { get; set; }
    public string DistributionChannelOrgUID { get; set; }
    public string OrderType { get; set; }
    public string JobPositionUID { get; set; }
    public string Status { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViewsRawdata { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViews { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> FilteredReturnOrderItemViews { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> DisplayedReturnOrderItemViews { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUList { get; set; }
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
    public bool IsTaxable { get; set; }
    public string StoreUID { get; set; }
    public string CurrencyUID { get; set; }
    public string OrgUID { get; set; }
    public string SalesOrderUID { get; set; }
    public bool IsTaxApplicable { get; set; }
    public bool IsViewMode { get; set; }
    public string RouteUID { get; set; }
    public string BeatHistoryUID { get; set; }
    public string StoreHistoryUID { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalLineTax { get; set; }
    public decimal TotalHeaderTax { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreViewModel { get; set; }
    /// <summary>
    /// Will be used to show Currency Label
    /// </summary>
    public string CurrencyLabel { get; set; }
    public decimal LineCount { get; set; }
    public decimal QtyCount { get; set; }
    public string Notes { get; set; }
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
    public decimal LineTaxAmount { get; set; }
    /// <summary>
    /// Header Tax Amount
    /// </summary>
    public decimal HeaderTaxAmount { get; set; }
    /// <summary>
    /// TotalTax = LineTaxAmount + HeaderTaxAmount
    /// </summary>
    public decimal TotalTax { get; }
    /// <summary>
    /// NetAmount = TotalAmount - TotalDiscount + ExciseDuty + TotalTax
    /// </summary>
    public decimal NetAmount { get; }
    public Dictionary<StockType, List<ISelectionItem>> ReasonMap { get; set; }
    //Loaylty
    //SKU Replaced
    //MCL Status
    //Wastage Value
    /// <summary>
    /// Populate Initial Data
    /// </summary>
    Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "");
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
    //Signature Fields
    List<IFileSys> SignatureFileSysList { get; set; }
    bool IsSignaturesCaptured { get; set; }
    string CustomerSignatureFileName { get; set; }
    string CustomerSignatureFolderPath { get; set; }
    string UserSignatureFileName { get; set; }
    string UserSignatureFolderPath { get; set; }
    Task ApplySort(List<SortCriteria> sortCriterias);
    List<ISelectionItem> GetReasons(StockType stockType);
    Task OnRouteSelect(string routeUID);
    Task AddItemToList(List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViews,
    ReturnOrder.Model.Interfaces.IReturnOrderItemView item, bool addAtEnd = true);
    Task<bool> SaveOrder();
    List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForClone(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForDDL(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    void DeleteClonedItem(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    void DeleteItem(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    void UpdateItemPrice(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    Task OnQtyChange(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView);
    void AddProductToGrid(List<IReturnOrderItemView> selectionItems);
    Task<List<ISelectionItem>> GetSkuListAsSelectionItems();
    Task OnStoreItemViewSelected(string storeItemViewUID);
    void OnSignatureProceedClick();
    void Validate();

}
