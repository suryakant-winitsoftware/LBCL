using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreCredit : BaseModel, IStoreCredit
    {
        public string StoreUID { get; set; }
        public string DivisionName { get; set; }
        public string AsmEmpName { get; set; }
        public string PaymentTermUID { get; set; }
        public string PaymentTermLabel{ get; set; }
        public string CreditType { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal TemporaryCredit { get; set; }
        public string OrgUID { get; set; }
        public string? OrgLabel { get; set; }
        public string DistributionChannelUID { get; set; }
        public string PreferredPaymentMode { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockingReasonCode { get; set; }
        public string BlockingReasonDescription { get; set; }
        public string DCLabel { get; set; }
        public string PriceList { get; set; }
        public string AuthorizedItemGRPKey { get; set; }
        public string MessageKey { get; set; }
        public string TaxKeyField { get; set; }
        public string PromotionKey { get; set; }
        public bool Disabled { get; set; }
        public string BillToAddressUID { get; set; }
        public string ShipToAddressUID { get; set; }
        public int OutstandingInvoices { get; set; }
        public string PreferredPaymentMethod { get; set; }
        public string PaymentType { get; set; }
        public string PaymentTypeLabel { get; set; }
        public decimal InvoiceAdminFeePerBillingCycle { get; set; }
        public decimal InvoiceAdminFeePerDelivery { get; set; }
        public decimal InvoiceLatePaymentFee { get; set; }
        public bool IsCancellationOfInvoiceAllowed { get; set; }
        public bool IsAllowCashOnCreditExceed { get; set; }
        public bool IsOutstandingBillControl { get; set; }
        public bool IsNegativeInvoiceAllowed { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal OverdueBalance { get; set; }
        public decimal AvailableBalance
        {
            get
            {
                return CommonFunctions.GetDecimalValue(CreditLimit) - CommonFunctions.GetDecimalValue(TotalBalance);
            }
        }
        public string StoreGroupDataUID {  get; set; }
        public int CreditDays { get; set; }
        public int TemporaryCreditDays { get; set; }
        public string DivisionOrgUID { get; set; }
        public DateTime TemporaryCreditApprovalDate { get; set; }
    }
}
