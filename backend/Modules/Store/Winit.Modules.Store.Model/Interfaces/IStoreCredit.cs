using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreCredit : IBaseModel
    {
        string DivisionName { get; set; }
        string AsmEmpName { get; set; }
        string StoreUID { get; set; }
        string PaymentTermUID { get; set; }
        string PaymentTermLabel { get; set; }
        string CreditType { get; set; }
        decimal CreditLimit { get; set; }
        decimal TemporaryCredit { get; set; }
        string OrgUID { get; set; }
        string? OrgLabel { get; set; }
        string DistributionChannelUID { get; set; }
        string PreferredPaymentMode { get; set; }
        bool IsActive { get; set; }
        bool IsBlocked { get; set; }
        string BlockingReasonCode { get; set; }
        string BlockingReasonDescription { get; set; }
        string DCLabel { get; set; }
        string PriceList { get; set; }
        string AuthorizedItemGRPKey { get; set; }
        string MessageKey { get; set; }
        string TaxKeyField { get; set; }
        string PromotionKey { get; set; }
        bool Disabled { get; set; }
        string BillToAddressUID { get; set; }
        string ShipToAddressUID { get; set; }
        int OutstandingInvoices { get; set; }
        string PreferredPaymentMethod { get; set; }
        string PaymentType { get; set; }
        string PaymentTypeLabel { get; set; }
        decimal InvoiceAdminFeePerBillingCycle { get; set; }
        decimal InvoiceAdminFeePerDelivery { get; set; }
        decimal InvoiceLatePaymentFee { get; set; }
        bool IsCancellationOfInvoiceAllowed { get; set; }
        bool IsAllowCashOnCreditExceed { get; set; }
        bool IsOutstandingBillControl { get; set; }
        bool IsNegativeInvoiceAllowed { get; set; }
        decimal TotalBalance { get; set; }
        decimal OverdueBalance { get; set; }
        decimal AvailableBalance { get; }
        string StoreGroupDataUID { get; set; }
        int CreditDays { get; set; }
        int TemporaryCreditDays { get; set; }
        string DivisionOrgUID { get; set; }
        DateTime TemporaryCreditApprovalDate { get; set; }
    }
}
