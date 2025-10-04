using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLStoreAdditionalInfoDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreAdditionalInfoDL
    {
        public MSSQLStoreAdditionalInfoDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT sai.id AS Id, sai.uid AS UID, sai.created_by AS CreatedBy, sai.created_time AS CreatedTime, sai.modified_by AS ModifiedBy,
                                            sai.modified_time AS ModifiedTime, sai.server_add_time AS ServerAddTime, sai.server_modified_time AS ServerModifiedTime, sai.store_uid AS StoreUID,
                                            sai.order_type AS OrderType, sai.is_promotions_block AS IsPromotionsBlock, sai.customer_start_date AS CustomerStartDate,
                                            sai.customer_end_date AS CustomerEndDate, sai.purchase_order_number AS PurchaseOrderNumber,
                                            sai.delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                                            sai.is_with_printed_invoices AS IsWithPrintedInvoices, sai.is_capture_signature_required AS IsCaptureSignatureRequired,
                                            sai.is_always_printed AS IsAlwaysPrinted, sai.building_delivery_code AS BuildingDeliveryCode,
                                            sai.delivery_information AS DeliveryInformation, sai.is_stop_delivery AS IsStopDelivery, 
                                            sai.is_forecast_top_up_qty AS IsForeCastTopUpQty, sai.is_temperature_check AS IsTemperatureCheck,
                                            sai.invoice_start_date AS InvoiceStartDate, sai.invoice_end_date AS InvoiceEndDate, 
                                            sai.invoice_format AS InvoiceFormat, sai.invoice_delivery_method AS InvoiceDeliveryMethod,
                                            sai.display_delivery_docket AS DisplayDeliveryDocket, sai.display_price AS DisplayPrice, 
                                            sai.show_cust_po AS ShowCustPO, sai.invoice_text AS InvoiceText, sai.invoice_frequency AS InvoiceFrequency, 
                                            sai.stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, sai.admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                                            sai.admin_fee_per_delivery AS AdminFeePerDelivery, sai.late_payment_fee AS LatePaymentFee, sai.drawer AS Drawer, 
                                            sai.bank_uid AS BankUID, sai.bank_account AS BankAccount, sai.mandatory_po_number AS MandatoryPONumber, 
                                            sai.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, sai.store_credit_always_printed AS StoreCreditAlwaysPrinted,
                                            sai.is_dummy_customer AS IsDummyCustomer, sai.default_run AS DefaultRun, sai.is_foc_customer AS IsFOCCustomer,
                                            sai.rss_show_price AS RSSShowPrice, sai.rss_show_payment AS RSSShowPayment, sai.rss_show_credit AS RSSShowCredit,
                                            sai.rss_show_invoice AS RSSShowInvoice, sai.rss_is_active AS RSSIsActive, sai.rss_delivery_instruction_status AS RSSDeliveryInstructionStatus,
                                            sai.rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, sai.rss_order_placed_in_rss AS RSSOrderPlacedInRSS,
                                            sai.rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, sai.rss_total_order_value AS RSSTotalOrderValue, sai.allow_force_check_in AS AllowForceCheckIn,
                                            sai.is_manual_edit_allowed AS IsManualEditAllowed, sai.can_update_lat_long AS CanUpdateLatLong, sai.allow_good_return AS AllowGoodReturn,
                                            sai.allow_bad_return AS AllowBadReturn,sai.allow_replacement AS AllowReplacement, sai.is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed,
                                            sai.is_delivery_note_required AS IsDeliveryNoteRequired, sai.e_invoicing_enabled AS EInvoicingEnabled, sai.image_recognition_enabled AS ImageRecognizationEnabled,
                                            sai.max_outstanding_invoices AS MaxOutstandingInvoices, sai.negative_invoice_allowed AS NegativeInvoiceAllowed, sai.delivery_mode AS DeliveryMode,
                                            sai.visit_frequency AS VisitFrequency, sai.shipping_contact_same_as_store AS ShippingContactSameAsStore,
                                            sai.billing_address_same_as_shipping AS BillingAddressSameAsShipping, sai.payment_mode AS PaymentMode, sai.price_type AS PriceType, 
                                            sai.average_monthly_income AS AverageMonthlyIncome, sai.default_bank_uid AS DefaultBankUID, sai.account_number AS AccountNumber, 
                                            sai.no_of_cash_counters AS NoOfCashCounters, sai.custom_field1 AS CustomField1, sai.custom_field2 AS CustomField2, sai.custom_field3 AS CustomField3,
                                            sai.custom_field4 AS CustomField4, sai.custom_field5 AS CustomField5, sai.custom_field6 AS CustomField6, sai.custom_field7 AS CustomField7,
                                            sai.custom_field8 AS CustomField8, sai.custom_field9 AS CustomField9, sai.custom_field10 AS CustomField10,sai.is_asset_enabled AS IsAssetEnabled,
                                            sai.is_survey_enabled AS IsSurveyEnabled,sai.allow_return_against_invoice AS AllowReturnAgainstInvoice, sai.allow_return_with_sales_order AS AllowReturnWithSalesOrder,
                                            sai.week_off_sun AS WeekOffSun, sai.week_off_mon AS WeekOffMon, sai.week_off_tue AS WeekOffTue, sai.week_off_wed AS WeekOffWed,
                                            sai.week_off_thu AS WeekOffThu, sai.week_off_fri AS WeekOffFri, sai.week_off_sat AS WeekOffSat, sai.aging_cycle AS AgingCycle,
                                            sai.depot AS Depot,sai.default_route_uid AS DefaultRouteUID,sai.firm_reg_no AS FirmRegNo,sai.company_reg_no AS CompanyRegNo,sai.is_mcme AS IsMSME,sai.firm_type AS FirmType,
                                            sai.acc_soft_name AS AccSoftName,sai.acc_soft_license_no AS AccSoftLicenseNo,sai.acc_soft_version_no AS AccSoftVersionNo, 
                                            sai.website AS  WebSite FROM store_additional_info sai) as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT sai.id AS Id, sai.uid AS UID, sai.created_by AS CreatedBy, sai.created_time AS CreatedTime, sai.modified_by AS ModifiedBy,
                                            sai.modified_time AS ModifiedTime, sai.server_add_time AS ServerAddTime, sai.server_modified_time AS ServerModifiedTime, sai.store_uid AS StoreUID,
                                            sai.order_type AS OrderType, sai.is_promotions_block AS IsPromotionsBlock, sai.customer_start_date AS CustomerStartDate,
                                            sai.customer_end_date AS CustomerEndDate, sai.purchase_order_number AS PurchaseOrderNumber,
                                            sai.delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                                            sai.is_with_printed_invoices AS IsWithPrintedInvoices, sai.is_capture_signature_required AS IsCaptureSignatureRequired,
                                            sai.is_always_printed AS IsAlwaysPrinted, sai.building_delivery_code AS BuildingDeliveryCode,
                                            sai.delivery_information AS DeliveryInformation, sai.is_stop_delivery AS IsStopDelivery, 
                                            sai.is_forecast_top_up_qty AS IsForeCastTopUpQty, sai.is_temperature_check AS IsTemperatureCheck,
                                            sai.invoice_start_date AS InvoiceStartDate, sai.invoice_end_date AS InvoiceEndDate, 
                                            sai.invoice_format AS InvoiceFormat, sai.invoice_delivery_method AS InvoiceDeliveryMethod,
                                            sai.display_delivery_docket AS DisplayDeliveryDocket, sai.display_price AS DisplayPrice, 
                                            sai.show_cust_po AS ShowCustPO, sai.invoice_text AS InvoiceText, sai.invoice_frequency AS InvoiceFrequency, 
                                            sai.stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, sai.admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                                            sai.admin_fee_per_delivery AS AdminFeePerDelivery, sai.late_payment_fee AS LatePaymentFee, sai.drawer AS Drawer, 
                                            sai.bank_uid AS BankUID, sai.bank_account AS BankAccount, sai.mandatory_po_number AS MandatoryPONumber, 
                                            sai.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, sai.store_credit_always_printed AS StoreCreditAlwaysPrinted,
                                            sai.is_dummy_customer AS IsDummyCustomer, sai.default_run AS DefaultRun, sai.is_foc_customer AS IsFOCCustomer,
                                            sai.rss_show_price AS RSSShowPrice, sai.rss_show_payment AS RSSShowPayment, sai.rss_show_credit AS RSSShowCredit,
                                            sai.rss_show_invoice AS RSSShowInvoice, sai.rss_is_active AS RSSIsActive, sai.rss_delivery_instruction_status AS RSSDeliveryInstructionStatus,
                                            sai.rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, sai.rss_order_placed_in_rss AS RSSOrderPlacedInRSS,
                                            sai.rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, sai.rss_total_order_value AS RSSTotalOrderValue, sai.allow_force_check_in AS AllowForceCheckIn,
                                            sai.is_manual_edit_allowed AS IsManualEditAllowed, sai.can_update_lat_long AS CanUpdateLatLong, sai.allow_good_return AS AllowGoodReturn,
                                            sai.allow_bad_return AS AllowBadReturn,sai.allow_replacement AS AllowReplacement, sai.is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed,
                                            sai.is_delivery_note_required AS IsDeliveryNoteRequired, sai.e_invoicing_enabled AS EInvoicingEnabled, sai.image_recognition_enabled AS ImageRecognizationEnabled,
                                            sai.max_outstanding_invoices AS MaxOutstandingInvoices, sai.negative_invoice_allowed AS NegativeInvoiceAllowed, sai.delivery_mode AS DeliveryMode,
                                            sai.visit_frequency AS VisitFrequency, sai.shipping_contact_same_as_store AS ShippingContactSameAsStore,
                                            sai.billing_address_same_as_shipping AS BillingAddressSameAsShipping, sai.payment_mode AS PaymentMode, sai.price_type AS PriceType, 
                                            sai.average_monthly_income AS AverageMonthlyIncome, sai.default_bank_uid AS DefaultBankUID, sai.account_number AS AccountNumber, 
                                            sai.no_of_cash_counters AS NoOfCashCounters, sai.custom_field1 AS CustomField1, sai.custom_field2 AS CustomField2, sai.custom_field3 AS CustomField3,
                                            sai.custom_field4 AS CustomField4, sai.custom_field5 AS CustomField5, sai.custom_field6 AS CustomField6, sai.custom_field7 AS CustomField7,
                                            sai.custom_field8 AS CustomField8, sai.custom_field9 AS CustomField9, sai.custom_field10 AS CustomField10,sai.is_asset_enabled AS IsAssetEnabled,
                                            sai.is_survey_enabled AS IsSurveyEnabled,sai.allow_return_against_invoice AS AllowReturnAgainstInvoice, sai.allow_return_with_sales_order AS AllowReturnWithSalesOrder,
                                            sai.week_off_sun AS WeekOffSun, sai.week_off_mon AS WeekOffMon, sai.week_off_tue AS WeekOffTue, sai.week_off_wed AS WeekOffWed,
                                            sai.week_off_thu AS WeekOffThu, sai.week_off_fri AS WeekOffFri, sai.week_off_sat AS WeekOffSat, sai.aging_cycle AS AgingCycle,
                                            sai.depot AS Depot,sai.default_route_uid AS DefaultRouteUID,sai.firm_reg_no AS FirmRegNo,sai.company_reg_no AS CompanyRegNo,sai.is_mcme AS IsMSME,sai.firm_type AS FirmType,
                                            sai.acc_soft_name AS AccSoftName,sai.acc_soft_license_no AS AccSoftLicenseNo,sai.acc_soft_version_no AS AccSoftVersionNo,
                                            sai.website AS  WebSite FROM store_additional_info sai) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IStoreAdditionalInfo>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                //Data
                IEnumerable<Model.Interfaces.IStoreAdditionalInfo> StoreAdditionalInfos = await ExecuteQueryAsync<Model.Interfaces.IStoreAdditionalInfo>(sql.ToString(), parameters);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>
                {
                    PagedData = StoreAdditionalInfos,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Task<IStoreAdditionalInfo> SelectStoreAdditionalInfoByStoreUID(string storeUID)
        {
            throw new NotImplementedException();
        }
        public async Task<Model.Interfaces.IStoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT sai.id AS Id, sai.uid AS UID, sai.created_by AS CreatedBy, sai.created_time AS CreatedTime, sai.modified_by AS ModifiedBy,
                                            sai.modified_time AS ModifiedTime, sai.server_add_time AS ServerAddTime, sai.server_modified_time AS ServerModifiedTime, sai.store_uid AS StoreUID,
                                            sai.order_type AS OrderType, sai.is_promotions_block AS IsPromotionsBlock, sai.customer_start_date AS CustomerStartDate,
                                            sai.customer_end_date AS CustomerEndDate, sai.purchase_order_number AS PurchaseOrderNumber,
                                            sai.delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                                            sai.is_with_printed_invoices AS IsWithPrintedInvoices, sai.is_capture_signature_required AS IsCaptureSignatureRequired,
                                            sai.is_always_printed AS IsAlwaysPrinted, sai.building_delivery_code AS BuildingDeliveryCode,
                                            sai.delivery_information AS DeliveryInformation, sai.is_stop_delivery AS IsStopDelivery, 
                                            sai.is_forecast_top_up_qty AS IsForeCastTopUpQty, sai.is_temperature_check AS IsTemperatureCheck,
                                            sai.invoice_start_date AS InvoiceStartDate, sai.invoice_end_date AS InvoiceEndDate, 
                                            sai.invoice_format AS InvoiceFormat, sai.invoice_delivery_method AS InvoiceDeliveryMethod,
                                            sai.display_delivery_docket AS DisplayDeliveryDocket, sai.display_price AS DisplayPrice, 
                                            sai.show_cust_po AS ShowCustPO, sai.invoice_text AS InvoiceText, sai.invoice_frequency AS InvoiceFrequency, 
                                            sai.stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, sai.admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                                            sai.admin_fee_per_delivery AS AdminFeePerDelivery, sai.late_payment_fee AS LatePaymentFee, sai.drawer AS Drawer, 
                                            sai.bank_uid AS BankUID, sai.bank_account AS BankAccount, sai.mandatory_po_number AS MandatoryPONumber, 
                                            sai.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, sai.store_credit_always_printed AS StoreCreditAlwaysPrinted,
                                            sai.is_dummy_customer AS IsDummyCustomer, sai.default_run AS DefaultRun, sai.is_foc_customer AS IsFOCCustomer,
                                            sai.rss_show_price AS RSSShowPrice, sai.rss_show_payment AS RSSShowPayment, sai.rss_show_credit AS RSSShowCredit,
                                            sai.rss_show_invoice AS RSSShowInvoice, sai.rss_is_active AS RSSIsActive, sai.rss_delivery_instruction_status AS RSSDeliveryInstructionStatus,
                                            sai.rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, sai.rss_order_placed_in_rss AS RSSOrderPlacedInRSS,
                                            sai.rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, sai.rss_total_order_value AS RSSTotalOrderValue, sai.allow_force_check_in AS AllowForceCheckIn,
                                            sai.is_manual_edit_allowed AS IsManualEditAllowed, sai.can_update_lat_long AS CanUpdateLatLong, sai.allow_good_return AS AllowGoodReturn,
                                            sai.allow_bad_return AS AllowBadReturn,sai.allow_replacement AS AllowReplacement, sai.is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed,
                                            sai.is_delivery_note_required AS IsDeliveryNoteRequired, sai.e_invoicing_enabled AS EInvoicingEnabled, sai.image_recognition_enabled AS ImageRecognizationEnabled,
                                            sai.max_outstanding_invoices AS MaxOutstandingInvoices, sai.negative_invoice_allowed AS NegativeInvoiceAllowed, sai.delivery_mode AS DeliveryMode,
                                            sai.visit_frequency AS VisitFrequency, sai.shipping_contact_same_as_store AS ShippingContactSameAsStore,
                                            sai.billing_address_same_as_shipping AS BillingAddressSameAsShipping, sai.payment_mode AS PaymentMode, sai.price_type AS PriceType, 
                                            sai.average_monthly_income AS AverageMonthlyIncome, sai.default_bank_uid AS DefaultBankUID, sai.account_number AS AccountNumber, 
                                            sai.no_of_cash_counters AS NoOfCashCounters, sai.custom_field1 AS CustomField1, sai.custom_field2 AS CustomField2, sai.custom_field3 AS CustomField3,
                                            sai.custom_field4 AS CustomField4, sai.custom_field5 AS CustomField5, sai.custom_field6 AS CustomField6, sai.custom_field7 AS CustomField7,
                                            sai.custom_field8 AS CustomField8, sai.custom_field9 AS CustomField9, sai.custom_field10 AS CustomField10,sai.is_asset_enabled AS IsAssetEnabled,
                                            sai.is_survey_enabled AS IsSurveyEnabled,sai.allow_return_against_invoice AS AllowReturnAgainstInvoice, sai.allow_return_with_sales_order AS AllowReturnWithSalesOrder,
                                            sai.week_off_sun AS WeekOffSun, sai.week_off_mon AS WeekOffMon, sai.week_off_tue AS WeekOffTue, sai.week_off_wed AS WeekOffWed,
                                            sai.week_off_thu AS WeekOffThu, sai.week_off_fri AS WeekOffFri, sai.week_off_sat AS WeekOffSat, sai.aging_cycle AS AgingCycle,
                                            sai.depot AS Depot,sai.default_route_uid AS DefaultRouteUID,sai.firm_reg_no AS FirmRegNo,sai.company_reg_no AS CompanyRegNo,sai.is_mcme AS IsMSME,sai.firm_type AS FirmType,
                                            sai.acc_soft_name AS AccSoftName,sai.acc_soft_license_no AS AccSoftLicenseNo,sai.acc_soft_version_no AS AccSoftVersionNo,
                                            sai.website AS  WebSite FROM store_additional_info sai WHERE sai.uid = @UID";

            Model.Interfaces.IStoreAdditionalInfo? StoreAdditionalInfo = await ExecuteSingleAsync<Model.Interfaces.IStoreAdditionalInfo>(sql, parameters);
            return StoreAdditionalInfo;
        }

        public async Task<int> CreateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {

                var sql = @"INSERT INTO store_additional_info (
                             uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                            store_uid, order_type, is_promotions_block, customer_start_date, customer_end_date, purchase_order_number,
                            delivery_docket_is_purchase_order_required, is_with_printed_invoices, is_capture_signature_required,
                            is_always_printed, building_delivery_code, delivery_information, is_stop_delivery, is_forecast_top_up_qty,
                            is_temperature_check, invoice_start_date, invoice_end_date, invoice_format, invoice_delivery_method,
                            display_delivery_docket, display_price, show_cust_po, invoice_text, invoice_frequency,
                            stock_credit_is_purchase_order_required, admin_fee_per_billing_cycle, admin_fee_per_delivery, late_payment_fee,
                            drawer, bank_uid, bank_account, mandatory_po_number, is_store_credit_capture_signature_required,
                            store_credit_always_printed, is_dummy_customer, default_run, is_foc_customer, rss_show_price, rss_show_payment,
                            rss_show_credit, rss_show_invoice, rss_is_active, rss_delivery_instruction_status, rss_time_spent_on_rss_portal,
                            rss_order_placed_in_rss, rss_avg_orders_per_week, rss_total_order_value, allow_force_check_in, is_manual_edit_allowed,
                            can_update_lat_long, allow_good_return, allow_bad_return, allow_replacement, is_invoice_cancellation_allowed,
                            is_delivery_note_required, e_invoicing_enabled, image_recognition_enabled, max_outstanding_invoices,
                            negative_invoice_allowed, delivery_mode, visit_frequency, shipping_contact_same_as_store, billing_address_same_as_shipping,
                            payment_mode, price_type, average_monthly_income, default_bank_uid, account_number, no_of_cash_counters,
                            custom_field1, custom_field2, custom_field3, custom_field4, custom_field5, custom_field6, custom_field7,
                            custom_field8, custom_field9, custom_field10, is_asset_enabled, is_survey_enabled, allow_return_against_invoice,
                            allow_return_with_sales_order, week_off_sun, week_off_mon, week_off_tue, week_off_wed, week_off_thu,
                            week_off_fri, week_off_sat, aging_cycle, depot, default_route_uid,firm_reg_no,company_reg_no,is_mcme,firm_type,
                            acc_soft_name,acc_soft_license_no,acc_soft_version_no,website,gst_owner_name,gst_gstin_status,gst_nature_of_business,gst_pan,
                            gst_pin_code,gst_registration_date,gst_registration_type,gst_tax_payment_type,gst_hsn_description,gst_gst_address,is_vendor,gst_state,gst_address1,gst_address2,district) 
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                            @StoreUID, @OrderType, @IsPromotionsBlock, @CustomerStartDate, @CustomerEndDate, @PurchaseOrderNumber,
                            @DeliveryDocketIsPurchaseOrderRequired, @IsWithPrintedInvoices, @IsCaptureSignatureRequired, @IsAlwaysPrinted,
                            @BuildingDeliveryCode, @DeliveryInformation, @IsStopDelivery, @IsForeCastTopUpQty, @IsTemperatureCheck,
                            @InvoiceStartDate, @InvoiceEndDate, @InvoiceFormat, @InvoiceDeliveryMethod, @DisplayDeliveryDocket,
                            @DisplayPrice, @ShowCustPO, @InvoiceText, @InvoiceFrequency, @StockCreditIsPurchaseOrderRequired,
                            @AdminFeePerBillingCycle, @AdminFeePerDelivery, @LatePaymentFee, @Drawer, @BankUID, @BankAccount,
                            @MandatoryPONumber, @IsStoreCreditCaptureSignatureRequired, @StoreCreditAlwaysPrinted, @IsDummyCustomer,
                            @DefaultRun, @IsFOCCustomer, @RSSShowPrice, @RSSShowPayment, @RSSShowCredit, @RSSShowInvoice, @RSSIsActive,
                            @RSSDeliveryInstructionStatus, @RSSTimeSpentOnRSSPortal, @RSSOrderPlacedInRSS, @RSSAvgOrdersPerWeek,
                            @RSSTotalOrderValue, @AllowForceCheckIn, @IsManualEditAllowed, @CanUpdateLatLong, @AllowGoodReturn,
                            @AllowBadReturn, @AllowReplacement, @IsInvoiceCancellationAllowed, @IsDeliveryNoteRequired, @EInvoicingEnabled,
                            @ImageRecognizationEnabled, @MaxOutstandingInvoices, @NegativeInvoiceAllowed, @DeliveryMode, @VisitFrequency,
                            @ShippingContactSameAsStore, @BillingAddressSameAsShipping, @PaymentMode, @PriceType, @AverageMonthlyIncome,
                            @DefaultBankUID, @AccountNumber, @NoOfCashCounters, @CustomField1, @CustomField2, @CustomField3, @CustomField4,
                            @CustomField5, @CustomField6, @CustomField7, @CustomField8, @CustomField9, @CustomField10, @IsAssetEnabled,
                            @IsSurveyEnabled, @AllowReturnAgainstInvoice, @AllowReturnWithSalesOrder, @WeekOffSun, @WeekOffMon,
                            @WeekOffTue, @WeekOffWed, @WeekOffThu, @WeekOffFri, @WeekOffSat, @AgingCycle, @Depot, @DefaultRouteUID,@FirmRegNo,@CompanyRegNo,
                            @IsMSME,@FirmType,@AccSoftName,@AccSoftLicenseNo,@AccSoftVersionNo,@WebSite,@OwnerName,@GSTINStatus,@NatureOfBusiness,@PAN,
                            @PinCode,@DateOfRegistration,@RegistrationType,@TaxPaymentType,@HSNDescription,@GSTAddress,@IsVendor, @GSTState,@GSTAddress1,@GSTAddress2,@GSTDistrict)";


                return await ExecuteNonQueryAsync(sql, storeAdditionalInfo);

            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while inserting store additional info.", ex);
            }
        }
        public async Task<int> UpdateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {
                storeAdditionalInfo.ModifiedBy = "ADMIN";
                storeAdditionalInfo.ModifiedTime = DateTime.Now;
                storeAdditionalInfo.ServerModifiedTime = DateTime.Now;
                var sql = @"UPDATE store_additional_info
                            SET 
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            firm_reg_no=@FirmRegNo,
                            company_reg_no=@CompanyRegNo,
                            is_mcme=@IsMSME,
                            firm_type=@FirmType,
                            acc_soft_name=@AccSoftName,
                            acc_soft_license_no=@AccSoftLicenseNo,
                            acc_soft_version_no=@AccSoftVersionNo,
                            website=@WebSite,
                            gst_owner_name = @OwnerName,
                            gst_gstin_status = @GSTINStatus,
                            gst_nature_of_business = @NatureOfBusiness,
                            gst_pan = @PAN,
                            gst_pin_code = @PinCode,
                            gst_registration_date = @DateOfRegistration,
                            gst_registration_type = @RegistrationType,
                            gst_tax_payment_type = @TaxPaymentType,
                            gst_hsn_description = @HSNDescription,
                            gst_gst_address = @GSTAddress
                            WHERE 
                            uid = @UID";

                return await ExecuteNonQueryAsync(sql, storeAdditionalInfo);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreAdditionalInfo(string storeUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",storeUID}
            };
            var sql = "DELETE  FROM store_additional_info WHERE store_uid = @storeUID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreatePaymentForMobile(IPayment payment)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdatePaymentForMobile(IPayment payment)
        {
            throw new NotImplementedException();
        }

        public Task<IPayment> SelectPaymentByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateWeekDaysForMobile(IWeekDays WeeDays)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateWeekDaysForMobile(IWeekDays WeeDays)
        {
            throw new NotImplementedException();
        }

        public Task<IWeekDays> SelectWeekDaysByUID(string UID)
        {
            throw new NotImplementedException();
        }
        public async Task<int> CUDStoreAdditionalInfo(IStoreAdditionalInfo storeAdditional)
        {
            int count = -1;
            try
            {
                string? existingUID = await CheckIfUIDExistsInDB(DbTableName.StoreAdditionalInfo, storeAdditional.UID);
                if (existingUID != null)
                {
                    count = await UpdateStoreAdditionalInfo(storeAdditional);
                }
                else
                {
                    count = await CreateStoreAdditionalInfo(storeAdditional);
                }
                return count;
            }
            catch
            {
                throw;
            }
        }

    }
}








