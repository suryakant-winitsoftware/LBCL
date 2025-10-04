using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Classes;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
namespace Winit.Modules.Store.Model.Interfaces
{
   
    public interface IStoreItemView : IBaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string AddressUID { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public bool IsPlanned { get; set; }
        public bool IsStopDelivery { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockedReasonDescription { get; set; }
        public bool IsAwayPeriod { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { set; get; }
        public bool IsPromotionsBlock { get; set; }
        public string SelectedOrgUID { get; set; }
        public string SelectedDistributionChannelUID { get; set; }
        public bool IsTaxApplicable { get; set; }
        public string TaxDocNumber { get; set; }
        public decimal TotalCreditLimit { get; set; }
        public decimal UsedCreditLimit { get; set; }
        public decimal AvailableCreditLimit { get; set; }
        public bool IsTemperatureCheckEnabled { get; set; }
        public int AlwaysPrintedFlag { get; set; }
        public int PurchaseOrderNoRequiredFlag { get; set; }
        public bool IsWithPrintedInvoicesFlag { get; set; }
        public List<object> ApplicableStoreCreditUIDS { get; set; }
        public string CurrentVisitStatus { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public bool IsVisited { get; }
        public string DeliveryTime { get; set; }
        public string StoreRating { get; set; }
        public string StoreClass { get; set; }
        public string StoreUID { get; set; }
        public string StoreCode { get; set; }
        public string StoreNumber { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        //public decimal StoreDistanceInKM { get; set; }
        //public StandardListSource SelectedStoreOrgSource { get; set; }
        //public List<StandardListSource> StoreOrgListSource { get; set; }
        //public StandardListSource SelectedStoreDistributionChannelSource { get; set; }
        //public List<StandardListSource> StoreDistributionChannelListSource { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreCredit SelectedStoreCredit { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> StoreCredits { get; set; }

        public string CurrencySymbol { get; set; }
        public int SerialNo { get; set; }
        public string StoreHistoryUID { get; set; }
        public string Notes { get; set; }
        public string AdditionalNotes { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string BeatHistoryUID { get; set; }
        public int NoOfVisits { get; set; }
        public bool IsProductive { get; set; }
        public bool IsGreen { get; set; }
        public decimal LastMonthSalesValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal TargetVolume { get; set; }
        public decimal TargetLines { get; set; }
        public decimal ActualValue { get; set; }
        public decimal ActualVolume { get; set; }
        public decimal ActualLines { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsTemperatureCheck { get; set; }
        public bool IsWithPrintedInvoices { get; set; }
        public decimal PrintEmailOption { get; set; }
        public bool IsBackgroundDataLoaded { get; set; }
        public bool IsPriceDataLoaded { get; set; }
        public int DeliveryDocketIsPurchaseOrderRequired { get; set; }
        public string ApplicableOrgHeirarchyCommaSeparated { get; set; }
        public string ApplicablePromotioListCommaSeparated { get; set; }
        public string ApplicablePlanogramListCommaSeparated { get; set; }
        public string ApplicableSKUClassificationCommaSeparated { get; set; }
        public Dictionary<string, string> ApplicableSKUClassificationDictionary { get; set; }
        public string ApplicablePriceListCommaSeparated { get; set; }
        public string BlockedItemCommaSeparatedList { get; set; }
        public string MinOrderQuantityClassGroupCommaSeparated { get; set; }
        public List<StandardListSource> SkippingCustomers { get; set; }
        public List<StandardListSource> ExceptionReasonList { get; set; }
        public string ExceptionType { get; set; }
        public int PlannedTimeSpendInMinutes { get; set; }
        public string ExceptionReason { get; set; }
        public string StoreHistoryStatsUID { get; set; }
        public bool IsTop10Avg3MonthsSales { get; set; }
        public decimal LMAvgSaleValue { get; set; }
        public decimal LMNoofInvoices { get; set; }
        public decimal LMInvoiceValue { get; set; }
        public decimal Last3MonthsAvgSalesValue { get; set; }
        public string TIN { get; set; }
        public bool IsTaxCustomer { get; }
        public bool IsTop10Customer { get; set; }
        public string OrderProfile { get; set; }
        public string Type { get; set; }
        public string Drawer { get; set; }
        public string Bank { get; set; }
        public string BuildingDeliveryCode { get; set; }
        public string ChannelCode { get; set; }
        public string ChannelName { get; set; }
        public string Price { get; set; }
        public bool MandatoryPONumber { get; set; }
        public bool IsStoreCreditCaptureSignatureRequired { get; set; }
        public bool StoreCreditAlwaysPrinted { get; set; }
        public bool IsDummyCustomer { get; set; }
        public bool IsAddedInJP { get; set; }
        public int NoOfOrderForDelivery { get; set; }
        public bool IsCaptureSignatureRequired { get; set; }
        public string SoldToStoreUID { get; set; }
        public string BillToStoreUID { get; set; }
        public bool StockCreditIsPurchaseOrderRequired { get; set; }
        public bool HasForwardOrder { get; set; }
        public bool IsOBCustomerAddItemRestricted { get; set; }
        public string StoreLevel3Code { get; set; }

        public List<string> ApplicablePromotionList { get; set; }
        public Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion> DMSPromotionDictionary { get; set; }
        public List<IItemPromotionMap>? ItemPromotionMapList { get; set; }
        public List<string>? AllowedSKUs { get; set; }
    }


}
