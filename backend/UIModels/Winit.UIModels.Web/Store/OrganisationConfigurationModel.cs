using System.ComponentModel.DataAnnotations;
using Winit.Modules.Store.Model.Classes;

namespace Winit.UIModels.Web.Store
{
    public class OrganisationConfigurationModel
    {
        public string? SalesOrg { get; set; }
        public string? SalesOrgLabel { get; set; }
        public string? PriceList { get; set; }
        public string? Channel { get; set; }
        public string? SubChannel { get; set; }
        public string? SubSubChannel { get; set; }
        public decimal CreditLimit { get; set; }
        public string? CreditDay { get; set; }
        public int OutstandingInvoices { get; set; }
        public string? CustomerType { get; set; }
        public string? CustomerChain { get; set; }
        public string? ChainLabel { get; set; }
        public string? CustomerGroup { get; set; }
        public string? CustomerGroupLabel { get; set; }
        public string? CustomerClassification { get; set; }
        public string? CustomerClassificationLabel { get; set; }
        public string? ChannelSubChannelLabel { get; set; }
        public string? ChannelSubChannelUID { get; set; }
        public List<StoreGroupDataFromJson> StoreGroupDataFromJsons {  get; set; }
        public string? JsonData { get; set; }
        public string? Distributor { get; set; }
        public string? AuthorizedItemGRPKey { get; set; }
        public string? MessageKey { get; set; }
        public string? TaxKeyField { get; set; }
        public string? PromotionKey { get; set; }
        public bool Disabled { get; set; }
        public bool IsActive { get; set; }
        public string? BillTo { get; set; }
        public string? ShipTo { get; set; }
        public string? PreferredPaymentMethod { get; set; }
        public string? PreferredPaymentMethodLabel { get; set; }
        public string? PaymentType { get; set; }
        public string? PaymentTypeLabel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentMethodLabel { get; set; }
        public string? PaymentTerm { get; set; }
        public bool IsPaymentTermDisable { get; set; }
        public string? PaymentTermLabel { get; set; }
        public string? TaxKeyField2 { get; set; }
        public string? CreditLimit2 { get; set; }
        public bool IsAllowCashOnCreditExceed { get; set; }
        public bool IsOutStandingBillControl { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockedReason { get; set; }
        public bool IsCancellationOfInvoiceAllowed { get; set; }
        public bool IsNegativeInvoiceAllowed { get; set; }
        public decimal InvoiceAdminFeePerBillingCycle { get; set; }
        public decimal InvoiceAdminFeePerDelivery { get; set; }
        public decimal InvoiceLatePaymentFeePer { get; set; }
        public bool AllowForceCheckIn { get; set; }
        public bool CanUpdateLatLong { get; set; }
    }
}
