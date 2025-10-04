using System.ComponentModel.DataAnnotations.Schema;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreAdditionalInfo : Base.Model.IBaseModel
    {
        [Column("store_uid")]
        public string StoreUID { get; set; }

        [Column("order_type")]
        public string OrderType { get; set; }

        [Column("is_promotions_block")]
        public bool? IsPromotionsBlock { get; set; }

        [Column("customer_start_date")]
        public DateTime? CustomerStartDate { get; set; }

        [Column("customer_end_date")]
        public DateTime? CustomerEndDate { get; set; }

        [Column("purchase_order_number")]
        public string PurchaseOrderNumber { get; set; }

        [Column("delivery_docket_is_purchase_order_required")]
        public int DeliveryDocketIsPurchaseOrderRequired { get; set; }

        [Column("is_with_printed_invoices")]
        public bool? IsWithPrintedInvoices { get; set; }

        [Column("is_capture_signature_required")]
        public bool? IsCaptureSignatureRequired { get; set; }

        [Column("is_always_printed")]
        public int IsAlwaysPrinted { get; set; }

        [Column("building_delivery_code")]
        public string BuildingDeliveryCode { get; set; }

        [Column("delivery_information")]
        public string DeliveryInformation { get; set; }

        [Column("is_stop_delivery")]
        public bool? IsStopDelivery { get; set; }

        [Column("is_forecast_top_up_qty")]
        public bool? IsForeCastTopUpQty { get; set; }

        [Column("is_temperature_check")]
        public bool? IsTemperatureCheck { get; set; }

        [Column("invoice_start_date")]
        public DateTime? InvoiceStartDate { get; set; }

        [Column("invoice_end_date")]
        public DateTime? InvoiceEndDate { get; set; }

        [Column("invoice_format")]
        public string InvoiceFormat { get; set; }

        [Column("invoice_delivery_method")]
        public string InvoiceDeliveryMethod { get; set; }

        [Column("display_delivery_docket")]
        public bool? DisplayDeliveryDocket { get; set; }

        [Column("display_price")]
        public bool? DisplayPrice { get; set; }

        [Column("show_cust_po")]
        public bool? ShowCustPO { get; set; }

        [Column("invoice_text")]
        public string InvoiceText { get; set; }

        [Column("invoice_frequency")]
        public string InvoiceFrequency { get; set; }

        [Column("stock_credit_is_purchase_order_required")]
        public bool? StockCreditIsPurchaseOrderRequired { get; set; }

        [Column("admin_fee_per_billing_cycle")]
        public decimal AdminFeePerBillingCycle { get; set; }

        [Column("admin_fee_per_delivery")]
        public decimal AdminFeePerDelivery { get; set; }

        [Column("late_payment_fee")]
        public decimal LatePaymentFee { get; set; }

        [Column("drawer")]
        public string Drawer { get; set; }

        [Column("bank_uid")]
        public string BankUID { get; set; }

        [Column("bank_account")]
        public string BankAccount { get; set; }

        [Column("mandatory_po_number")]
        public bool? MandatoryPONumber { get; set; }

        [Column("is_store_credit_capture_signature_required")]
        public bool? IsStoreCreditCaptureSignatureRequired { get; set; }

        [Column("store_credit_always_printed")]
        public int StoreCreditAlwaysPrinted { get; set; }

        [Column("is_dummy_customer")]
        public bool? IsDummyCustomer { get; set; }

        [Column("default_run")]
        public string DefaultRun { get; set; }

        [Column("is_foc_customer")]
        public bool? IsFOCCustomer { get; set; }

        [Column("rss_show_price")]
        public bool? RSSShowPrice { get; set; }

        [Column("rss_show_payment")]
        public bool? RSSShowPayment { get; set; }

        [Column("rss_show_credit")]
        public bool? RSSShowCredit { get; set; }

        [Column("rss_show_invoice")]
        public bool? RSSShowInvoice { get; set; }

        [Column("rss_is_active")]
        public bool? RSSIsActive { get; set; }

        [Column("rss_delivery_instruction_status")]
        public string RSSDeliveryInstructionStatus { get; set; }

        [Column("rss_time_spent_on_rss_portal")]
        public int RSSTimeSpentOnRSSPortal { get; set; }

        [Column("rss_order_placed_in_rss")]
        public int RSSOrderPlacedInRSS { get; set; }

        [Column("rss_avg_orders_per_week")]
        public int RSSAvgOrdersPerWeek { get; set; }

        [Column("rss_total_order_value")]
        public decimal RSSTotalOrderValue { get; set; }

        [Column("allow_force_check_in")]
        public bool? AllowForceCheckIn { get; set; }

        [Column("is_manual_edit_allowed")]
        public bool? IsManualEditAllowed { get; set; }

        [Column("can_update_lat_long")]
        public bool? CanUpdateLatLong { get; set; }

        [Column("allow_good_return")]
        public bool? AllowGoodReturn { get; set; }

        [Column("allow_bad_return")]
        public bool? AllowBadReturn { get; set; }

        [Column("allow_replacement")]
        public bool? AllowReplacement { get; set; }

        [Column("is_invoice_cancellation_allowed")]
        public bool? IsInvoiceCancellationAllowed { get; set; }

        [Column("is_delivery_note_required")]
        public bool? IsDeliveryNoteRequired { get; set; }

        [Column("e_invoicing_enabled")]
        public bool? EInvoicingEnabled { get; set; }

        [Column("image_recognition_enabled")]
        public bool? ImageRecognizationEnabled { get; set; }

        [Column("max_outstanding_invoices")]
        public int MaxOutstandingInvoices { get; set; }

        [Column("negative_invoice_allowed")]
        public bool? NegativeInvoiceAllowed { get; set; }

        [Column("delivery_mode")]
        public string DeliveryMode { get; set; }

        [Column("visit_frequency")]
        public string VisitFrequency { get; set; }

        [Column("shipping_contact_same_as_store")]
        public bool? ShippingContactSameAsStore { get; set; }

        [Column("billing_address_same_as_shipping")]
        public bool? BillingAddressSameAsShipping { get; set; }

        [Column("payment_mode")]
        public string PaymentMode { get; set; }

        [Column("price_type")]
        public string PriceType { get; set; }

        [Column("average_monthly_income")]
        public decimal AverageMonthlyIncome { get; set; }

        [Column("default_bank_uid")]
        public string DefaultBankUID { get; set; }

        [Column("account_number")]
        public string AccountNumber { get; set; }

        [Column("no_of_cash_counters")]
        public int NoOfCashCounters { get; set; }

        [Column("custom_field1")]
        public string CustomField1 { get; set; }

        [Column("custom_field2")]
        public string CustomField2 { get; set; }

        [Column("custom_field3")]
        public string CustomField3 { get; set; }

        [Column("custom_field4")]
        public string CustomField4 { get; set; }

        [Column("custom_field5")]
        public string CustomField5 { get; set; }

        [Column("custom_field6")]
        public string CustomField6 { get; set; }

        [Column("custom_field7")]
        public string CustomField7 { get; set; }

        [Column("custom_field8")]
        public string CustomField8 { get; set; }

        [Column("custom_field9")]
        public string CustomField9 { get; set; }

        [Column("custom_field10")]
        public string CustomField10 { get; set; }

        [Column("is_asset_enabled")]
        public bool? IsAssetEnabled { get; set; }

        [Column("is_survey_enabled")]
        public bool? IsSurveyEnabled { get; set; }

        [Column("allow_return_against_invoice")]
        public bool? AllowReturnAgainstInvoice { get; set; }

        [Column("allow_return_with_sales_order")]
        public bool? AllowReturnWithSalesOrder { get; set; }

        [Column("week_off_sun")]
        public bool? WeekOffSun { get; set; }

        [Column("week_off_mon")]
        public bool? WeekOffMon { get; set; }

        [Column("week_off_tue")]
        public bool? WeekOffTue { get; set; }

        [Column("week_off_wed")]
        public bool? WeekOffWed { get; set; }

        [Column("week_off_thu")]
        public bool? WeekOffThu { get; set; }

        [Column("week_off_fri")]
        public bool? WeekOffFri { get; set; }

        [Column("week_off_sat")]
        public bool? WeekOffSat { get; set; }

        [Column("aging_cycle")]
        public string AgingCycle { get; set; }

        [Column("depot")]
        public string Depot { get; set; }

        [Column("default_route_uid")]
        public string DefaultRouteUID { get; set; }

        [Column("firm_reg_no")]
        public string FirmRegNo { get; set; }

        [Column("company_reg_no")]
        public string CompanyRegNo { get; set; }

        [Column("is_mcme")]
        public bool IsMCME { get; set; }

        [Column("is_vendor")]
        public bool IsVendor { get; set; }

        [Column("firm_type")]
        public string FirmType { get; set; }

        [Column("acc_soft_name")]
        public string AccSoftName { get; set; }

        [Column("acc_soft_license_no")]
        public string AccSoftLicenseNo { get; set; }

        [Column("acc_soft_version_no")]
        public string AccSoftVersionNo { get; set; }

        [Column("website")]
        public string WebSite { get; set; }

        [Column("gst_owner_name")]
        public string OwnerName { get; set; }

        [Column("gst_gstin_status")]
        public string GSTINStatus { get; set; }

        [Column("gst_nature_of_business")]
        public string NatureOfBusiness { get; set; }

        [Column("gst_pan")]
        public string PAN { get; set; }

        [Column("gst_pin_code")]
        public string PinCode { get; set; }

        [Column("is_mcme")]
        public bool IsMSME { get; set; }
        //By Selva
        [Column("gst_registration_date")]
        public string DateOfRegistration { get; set; }
        [Column("gst_registration_type")]
        public string RegistrationType { get; set; }
        [Column("gst_tax_payment_type")]
        public string TaxPaymentType { get; set; }
        [Column("gst_hsn_description")]
        public string HSNDescription { get; set; }
        [Column("gst_gst_address")]
        public string GSTAddress { get; set; }
        [Column("gst_address1")]
        public string GSTAddress1 { get; set; }
        [Column("gst_address2")]
        public string GSTAddress2 { get; set; }
        [Column("gst_state")]
        public string GSTState { get; set; }
        [Column("district")]
        public string GSTDistrict { get; set; }
    }
}
