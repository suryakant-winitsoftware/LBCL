using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class AdditionalInfoModel : BaseModel
    {
        public string StoreUID { get; set; }
        public string OrderType { get; set; }
        public bool IsPromotionsBlock { get; set; }
        public DateTime CustomerStartDate { get; set; }
        public DateTime CustomerEndDate { get; set; }
        public string SchoolWarehouse { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int DeliveryDocketIsPurchaseOrderRequired { get; set; }
        public bool IsWithPrintedInvoices { get; set; }
        public int IsAlwaysPrinted { get; set; }
        public string BuildingDeliveryCode { get; set; }
        public string DeliveryInformation { get; set; }
        public bool IsStopDelivery { get; set; }
        public bool IsForeCastTopUpQty { get; set; }
        public bool IsTemperatureCheck { get; set; }
        public DateTime InvoiceStartDate { get; set; }
        public DateTime InvoiceEndDate { get; set; }
        public string InvoiceFormat { get; set; }
        public string InvoiceDeliveryMethod { get; set; }
        public bool DisplayDeliveryDocket { get; set; }
        public bool DisplayPrice { get; set; }
        public bool ShowCustPO { get; set; }
        public string InvoiceText { get; set; }
        public string InvoiceFrequency { get; set; }
        public bool StockCreditIsPurchaseOrderRequired { get; set; }
        public decimal AdminFeePerBillingCycle { get; set; }
        public decimal AdminFeePerDelivery { get; set; }
        public decimal LatePayementFee { get; set; }
        public string Drawer { get; set; }
        public string AgeingCycle { get; set; }
        public string BankUID { get; set; }
        public string BankAccount { get; set; }
        public bool MandatoryPONumber { get; set; }
        public bool IsStoreCreditCaptureSignatureRequired { get; set; }
        public int StoreCreditAlwaysPrinted { get; set; }
        public bool IsDummyCustomer { get; set; }
        public string DefaultRun { get; set; }
        public string ProspectEmpUID { get; set; }
        public bool IsFOCCustomer { get; set; }
        public bool RSSShowPrice { get; set; }
        public bool RSSShowPayment { get; set; }
        public bool RSSShowCredit { get; set; }
        public bool RSSShowInvoice { get; set; }
        public bool RSSIsActive { get; set; }
        public string RSSDeliveryInstructionStatus { get; set; }
        public int RSSTimeSpentOnRSSPortal { get; set; }
        public int RSSOrderPlacedInRSS { get; set; }
        public int RSSAvgOrdersPerWeek { get; set; }
        public decimal RSSTotalOrderValue { get; set; }
        public bool AllowForceCheckIn { get; set; }
        public bool IsManaualEditAllowed { get; set; }
        public bool CanUpdateLatLong { get; set; }
        public bool IsTaxApplicable { get; set; }
        public string TaxDocNumber { get; set; }
        public bool IsTaxDocVerified { get; set; }
        public bool AllowGoodReturn { get; set; }
        public bool AllowBadReturn { get; set; }
        public bool EnableAsset { get; set; }
        public bool EnableSurvey { get; set; }
        public bool AllowReplacement { get; set; }
        public bool IsInvoiceCancellationAllowed { get; set; }
        public bool IsDeliveryNoteRequired { get; set; }
        public bool EInvoicingEnabled { get; set; }
        public bool ImageRecognizationEnabled { get; set; }
        public int MaxOutstandingInvoices { get; set; }
        public bool NegativeInvoiceAllowed { get; set; }
        public string DeliveryMode { get; set; }
        public decimal StoreSize { get; set; }
        public string VisitFrequency { get; set; }
        public bool ShippingContactSameAsStore { get; set; }
        public bool BillingAddressSameAsShipping { get; set; }
        public string PaymentMode { get; set; }
        public string PriceType { get; set; }
        public decimal AverageMonthlyIncome { get; set; }
        public string DefaultBankUID { get; set; }
        public string AccountNumber { get; set; }
        public int NoOfCashCounters { get; set; }
        public bool IsCaptureSignatureRequired { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }
        public string CustomField6 { get; set; }
        public string CustomField7 { get; set; }
        public string CustomField8 { get; set; }
        public string CustomField9 { get; set; }
        public string CustomField10 { get; set; }
        public string TaxType { get; set; }
        public string TaxKeyField { get; set; }
        public string StoreImage { get; set; }
        public bool IsVATQRCaptureMandatory { get; set; }
        public bool IsAssetEnabled { get; set; }
        public bool IsSurveyEnabled { get; set; }
        public bool AllowGoodReturns { get; set; }
        public bool AllowBadReturns { get; set; }
        public bool AllowReturnAgainstInvoice { get; set; }
        public bool AllowReturnWithSalesOrder { get; set; }
        public bool WeekOffSun { get; set; }
        public bool WeekOffMon { get; set; }
        public bool WeekOffTue { get; set; }
        public bool WeekOffWed { get; set; }
        public bool WeekOffThu { get; set; }
        public bool WeekOffFri { get; set; }
        public bool WeekOffSat { get; set; }
    }
}
