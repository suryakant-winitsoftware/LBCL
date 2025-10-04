using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLStoreAdditionalInfoDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreAdditionalInfoDL
    {
        public PGSQLStoreAdditionalInfoDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT id AS Id, 
                           uid AS UID, 
                           created_by AS CreatedBy, 
                           created_time AS CreatedTime, 
                           modified_by AS ModifiedBy, 
                           modified_time AS ModifiedTime, 
                           server_add_time AS ServerAddTime, 
                           server_modified_time AS ServerModifiedTime, 
                           store_uid AS StoreUID, 
                           order_type AS OrderType, 
                           is_promotions_block AS IsPromotionsBlock, 
                           customer_start_date AS CustomerStartDate, 
                           customer_end_date AS CustomerEndDate, 
                           purchase_order_number AS PurchaseOrderNumber, 
                           delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                           is_with_printed_invoices AS IsWithPrintedInvoices, 
                           is_capture_signature_required AS IsCaptureSignatureRequired, 
                           is_always_printed AS IsAlwaysPrinted, 
                           building_delivery_code AS BuildingDeliveryCode, 
                           delivery_information AS DeliveryInformation, 
                           is_stop_delivery AS IsStopDelivery, 
                           is_forecast_top_up_qty AS IsForeCastTopUpQty, 
                           is_temperature_check AS IsTemperatureCheck, 
                           invoice_start_date AS InvoiceStartDate, 
                           invoice_end_date AS InvoiceEndDate, 
                           invoice_format AS InvoiceFormat, 
                           invoice_delivery_method AS InvoiceDeliveryMethod, 
                           display_delivery_docket AS DisplayDeliveryDocket, 
                           display_price AS DisplayPrice, 
                           show_cust_po AS ShowCustPO, 
                           invoice_text AS InvoiceText, 
                           invoice_frequency AS InvoiceFrequency, 
                           stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, 
                           admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                           admin_fee_per_delivery AS AdminFeePerDelivery, 
                           late_payment_fee AS LatePaymentFee, 
                           drawer AS Drawer, 
                           bank_uid AS BankU, 
                           bank_account AS BankAccount, 
                           mandatory_po_number AS MandatoryPONumber, 
                           is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                           store_credit_always_printed AS StoreCreditAlwaysPrinted, 
                           is_dummy_customer AS IsDummyCustomer, 
                           default_run AS DefaultRun, 
                           is_foc_customer AS IsFOCCustomer, 
                           rss_show_price AS RSSShowPrice, 
                           rss_show_payment AS RSSShowPayment, 
                           rss_show_credit AS RSSShowCredit, 
                           rss_show_invoice AS RSSShowInvoice, 
                           rss_is_active AS RSSIsActive, 
                           rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                           rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, 
                           rss_order_placed_in_rss AS RSSOrderPlacedInRSS, 
                           rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, 
                           rss_total_order_value AS RSSTotalOrderValue, 
                           allow_force_check_in AS AllowForceCheckIn, 
                           is_manual_edit_allowed AS IsManualEditAllowed, 
                           can_update_lat_long AS CanUpdateLatLong, 
                           allow_good_return AS AllowGoodReturn, 
                           allow_bad_return AS AllowBadReturn, 
                           allow_replacement AS AllowReplacement, 
                           is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed, 
                           is_delivery_note_required AS IsDeliveryNoteRequired, 
                           e_invoicing_enabled AS EInvoicingEnabled, 
                           image_recognition_enabled AS ImageRecognizationEnabled, 
                           max_outstanding_invoices AS MaxOutstandingInvoices, 
                           negative_invoice_allowed AS NegativeInvoiceAllowed, 
                           delivery_mode AS DeliveryMode, 
                           visit_frequency AS VisitFrequency, 
                           shipping_contact_same_as_store AS ShippingContactSameAsStore, 
                           billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                           payment_mode AS PaymentMode, 
                           price_type AS PriceType, 
                           average_monthly_income AS AverageMonthlyIncome, 
                           default_bank_uid AS DefaultBankUid, 
                           account_number AS AccountNumber, 
                           no_of_cash_counters AS NoOfCashCounters, 
                           custom_field1 AS CustomField1, 
                           custom_field2 AS CustomField2, 
                           custom_field3 AS CustomField3, 
                           custom_field4 AS CustomField4, 
                           custom_field5 AS CustomField5, 
                           custom_field6 AS CustomField6, 
                           custom_field7 AS CustomField7, 
                           custom_field8 AS CustomField8, 
                           custom_field9 AS CustomField9, 
                           custom_field10 AS CustomField10, 
                           is_asset_enabled AS IsAssetEnabled, 
                           is_survey_enabled AS IsSurveyEnabled, 
                           allow_return_against_invoice AS AllowReturnAgainstInvoice, 
                           allow_return_with_sales_order AS AllowReturnWithSalesOrder, 
                           week_off_sun AS WeekOffSun, 
                           week_off_mon AS WeekOffMon, 
                           week_off_tue AS WeekOffTue, 
                           week_off_wed AS WeekOffWed, 
                           week_off_thu AS WeekOffThu, 
                           week_off_fri AS WeekOffFri, 
                           week_off_sat AS WeekOffSat, 
                           aging_cycle AS AgingCycle, 
                           depot AS Depot ,default_route_uid as DefaultRouteUID,
                           firm_reg_no AS FirmRegNo,
                           company_reg_no AS CompanyRegNo,
                           is_mcme AS IsMSME,
                           firm_type AS FirmType,
                           acc_soft_name AS AccSoftName,
                           acc_soft_license_no AS AccSoftLicenseNo,
                           acc_soft_version_no AS AccSoftVersionNo, 
                           website AS  WebSite
                           FROM store_additional_info) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id, 
                           uid AS UID, 
                           created_by AS CreatedBy, 
                           created_time AS CreatedTime, 
                           modified_by AS ModifiedBy, 
                           modified_time AS ModifiedTime, 
                           server_add_time AS ServerAddTime, 
                           server_modified_time AS ServerModifiedTime, 
                           store_uid AS StoreUID, 
                           order_type AS OrderType, 
                           is_promotions_block AS IsPromotionsBlock, 
                           customer_start_date AS CustomerStartDate, 
                           customer_end_date AS CustomerEndDate, 
                           purchase_order_number AS PurchaseOrderNumber, 
                           delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                           is_with_printed_invoices AS IsWithPrintedInvoices, 
                           is_capture_signature_required AS IsCaptureSignatureRequired, 
                           is_always_printed AS IsAlwaysPrinted, 
                           building_delivery_code AS BuildingDeliveryCode, 
                           delivery_information AS DeliveryInformation, 
                           is_stop_delivery AS IsStopDelivery, 
                           is_forecast_top_up_qty AS IsForeCastTopUpQty, 
                           is_temperature_check AS IsTemperatureCheck, 
                           invoice_start_date AS InvoiceStartDate, 
                           invoice_end_date AS InvoiceEndDate, 
                           invoice_format AS InvoiceFormat, 
                           invoice_delivery_method AS InvoiceDeliveryMethod, 
                           display_delivery_docket AS DisplayDeliveryDocket, 
                           display_price AS DisplayPrice, 
                           show_cust_po AS ShowCustPO, 
                           invoice_text AS InvoiceText, 
                           invoice_frequency AS InvoiceFrequency, 
                           stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, 
                           admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                           admin_fee_per_delivery AS AdminFeePerDelivery, 
                           late_payment_fee AS LatePaymentFee, 
                           drawer AS Drawer, 
                           bank_uid AS BankU, 
                           bank_account AS BankAccount, 
                           mandatory_po_number AS MandatoryPONumber, 
                           is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                           store_credit_always_printed AS StoreCreditAlwaysPrinted, 
                           is_dummy_customer AS IsDummyCustomer, 
                           default_run AS DefaultRun, 
                           is_foc_customer AS IsFOCCustomer, 
                           rss_show_price AS RSSShowPrice, 
                           rss_show_payment AS RSSShowPayment, 
                           rss_show_credit AS RSSShowCredit, 
                           rss_show_invoice AS RSSShowInvoice, 
                           rss_is_active AS RSSIsActive, 
                           rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                           rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, 
                           rss_order_placed_in_rss AS RSSOrderPlacedInRSS, 
                           rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, 
                           rss_total_order_value AS RSSTotalOrderValue, 
                           allow_force_check_in AS AllowForceCheckIn, 
                           is_manual_edit_allowed AS IsManualEditAllowed, 
                           can_update_lat_long AS CanUpdateLatLong, 
                           allow_good_return AS AllowGoodReturn, 
                           allow_bad_return AS AllowBadReturn, 
                           allow_replacement AS AllowReplacement, 
                           is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed, 
                           is_delivery_note_required AS IsDeliveryNoteRequired, 
                           e_invoicing_enabled AS EInvoicingEnabled, 
                           image_recognition_enabled AS ImageRecognizationEnabled, 
                           max_outstanding_invoices AS MaxOutstandingInvoices, 
                           negative_invoice_allowed AS NegativeInvoiceAllowed, 
                           delivery_mode AS DeliveryMode, 
                           visit_frequency AS VisitFrequency, 
                           shipping_contact_same_as_store AS ShippingContactSameAsStore, 
                           billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                           payment_mode AS PaymentMode, 
                           price_type AS PriceType, 
                           average_monthly_income AS AverageMonthlyIncome, 
                           default_bank_uid AS DefaultBankUid, 
                           account_number AS AccountNumber, 
                           no_of_cash_counters AS NoOfCashCounters, 
                           custom_field1 AS CustomField1, 
                           custom_field2 AS CustomField2, 
                           custom_field3 AS CustomField3, 
                           custom_field4 AS CustomField4, 
                           custom_field5 AS CustomField5, 
                           custom_field6 AS CustomField6, 
                           custom_field7 AS CustomField7, 
                           custom_field8 AS CustomField8, 
                           custom_field9 AS CustomField9, 
                           custom_field10 AS CustomField10, 
                           is_asset_enabled AS IsAssetEnabled, 
                           is_survey_enabled AS IsSurveyEnabled, 
                           allow_return_against_invoice AS AllowReturnAgainstInvoice, 
                           allow_return_with_sales_order AS AllowReturnWithSalesOrder, 
                           week_off_sun AS WeekOffSun, 
                           week_off_mon AS WeekOffMon, 
                           week_off_tue AS WeekOffTue, 
                           week_off_wed AS WeekOffWed, 
                           week_off_thu AS WeekOffThu, 
                           week_off_fri AS WeekOffFri, 
                           week_off_sat AS WeekOffSat, 
                           aging_cycle AS AgingCycle, 
                           depot AS Depot ,default_route_uid as DefaultRouteUID,
                            firm_reg_no AS FirmRegNo,
                           company_reg_no AS CompanyRegNo,
                           is_mcme AS IsMSME,
                           firm_type AS FirmType,
                           acc_soft_name AS AccSoftName,
                           acc_soft_license_no AS AccSoftLicenseNo,
                           acc_soft_version_no AS AccSoftVersionNo, 
                           website AS  WebSite
                           FROM store_additional_info) as SubQuery");
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
                    AppendSortCriteria(sortCriterias, sql, true);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAdditionalInfo>().GetType();
                IEnumerable<Model.Interfaces.IStoreAdditionalInfo> StoreAdditionalInfos = await ExecuteQueryAsync<Model.Interfaces.IStoreAdditionalInfo>(sql.ToString(), parameters, type);
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
        public async Task<Model.Interfaces.IStoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT id AS Id, 
                           uid AS UID, 
                           created_by AS CreatedBy, 
                           created_time AS CreatedTime, 
                           modified_by AS ModifiedBy, 
                           modified_time AS ModifiedTime, 
                           server_add_time AS ServerAddTime, 
                           server_modified_time AS ServerModifiedTime, 
                           store_uid AS StoreUID, 
                           order_type AS OrderType, 
                           is_promotions_block AS IsPromotionsBlock, 
                           customer_start_date AS CustomerStartDate, 
                           customer_end_date AS CustomerEndDate, 
                           purchase_order_number AS PurchaseOrderNumber, 
                           delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                           is_with_printed_invoices AS IsWithPrintedInvoices, 
                           is_capture_signature_required AS IsCaptureSignatureRequired, 
                           is_always_printed AS IsAlwaysPrinted, 
                           building_delivery_code AS BuildingDeliveryCode, 
                           delivery_information AS DeliveryInformation, 
                           is_stop_delivery AS IsStopDelivery, 
                           is_forecast_top_up_qty AS IsForeCastTopUpQty, 
                           is_temperature_check AS IsTemperatureCheck, 
                           invoice_start_date AS InvoiceStartDate, 
                           invoice_end_date AS InvoiceEndDate, 
                           invoice_format AS InvoiceFormat, 
                           invoice_delivery_method AS InvoiceDeliveryMethod, 
                           display_delivery_docket AS DisplayDeliveryDocket, 
                           display_price AS DisplayPrice, 
                           show_cust_po AS ShowCustPO, 
                           invoice_text AS InvoiceText, 
                           invoice_frequency AS InvoiceFrequency, 
                           stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, 
                           admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                           admin_fee_per_delivery AS AdminFeePerDelivery, 
                           late_payment_fee AS LatePaymentFee, 
                           drawer AS Drawer, 
                           bank_uid AS BankU, 
                           bank_account AS BankAccount, 
                           mandatory_po_number AS MandatoryPONumber, 
                           is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                           store_credit_always_printed AS StoreCreditAlwaysPrinted, 
                           is_dummy_customer AS IsDummyCustomer, 
                           default_run AS DefaultRun, 
                           is_foc_customer AS IsFOCCustomer, 
                           rss_show_price AS RSSShowPrice, 
                           rss_show_payment AS RSSShowPayment, 
                           rss_show_credit AS RSSShowCredit, 
                           rss_show_invoice AS RSSShowInvoice, 
                           rss_is_active AS RSSIsActive, 
                           rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                           rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, 
                           rss_order_placed_in_rss AS RSSOrderPlacedInRSS, 
                           rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, 
                           rss_total_order_value AS RSSTotalOrderValue, 
                           allow_force_check_in AS AllowForceCheckIn, 
                           is_manual_edit_allowed AS IsManualEditAllowed, 
                           can_update_lat_long AS CanUpdateLatLong, 
                           allow_good_return AS AllowGoodReturn, 
                           allow_bad_return AS AllowBadReturn, 
                           allow_replacement AS AllowReplacement, 
                           is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed, 
                           is_delivery_note_required AS IsDeliveryNoteRequired, 
                           e_invoicing_enabled AS EInvoicingEnabled, 
                           image_recognition_enabled AS ImageRecognizationEnabled, 
                           max_outstanding_invoices AS MaxOutstandingInvoices, 
                           negative_invoice_allowed AS NegativeInvoiceAllowed, 
                           delivery_mode AS DeliveryMode, 
                           visit_frequency AS VisitFrequency, 
                           shipping_contact_same_as_store AS ShippingContactSameAsStore, 
                           billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                           payment_mode AS PaymentMode, 
                           price_type AS PriceType, 
                           average_monthly_income AS AverageMonthlyIncome, 
                           default_bank_uid AS DefaultBankUid, 
                           account_number AS AccountNumber, 
                           no_of_cash_counters AS NoOfCashCounters, 
                           custom_field1 AS CustomField1, 
                           custom_field2 AS CustomField2, 
                           custom_field3 AS CustomField3, 
                           custom_field4 AS CustomField4, 
                           custom_field5 AS CustomField5, 
                           custom_field6 AS CustomField6, 
                           custom_field7 AS CustomField7, 
                           custom_field8 AS CustomField8, 
                           custom_field9 AS CustomField9, 
                           custom_field10 AS CustomField10, 
                           is_asset_enabled AS IsAssetEnabled, 
                           is_survey_enabled AS IsSurveyEnabled,
                           allow_return_against_invoice AS AllowReturnAgainstInvoice, 
                           allow_return_with_sales_order AS AllowReturnWithSalesOrder, 
                           week_off_sun AS WeekOffSun, 
                           week_off_mon AS WeekOffMon, 
                           week_off_tue AS WeekOffTue, 
                           week_off_wed AS WeekOffWed, 
                           week_off_thu AS WeekOffThu, 
                           week_off_fri AS WeekOffFri, 
                           week_off_sat AS WeekOffSat, 
                           aging_cycle AS AgingCycle, 
                           depot AS Depot ,default_route_uid as DefaultRouteUID,
                           firm_reg_no AS FirmRegNo,
                           company_reg_no AS CompanyRegNo,
                           is_mcme AS IsMSME,
                           firm_type AS FirmType,
                           acc_soft_name AS AccSoftName,
                           acc_soft_license_no AS AccSoftLicenseNo,
                           acc_soft_version_no AS AccSoftVersionNo, 
                           website AS  WebSite
                           FROM store_additional_info where uid=@UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAdditionalInfo>().GetType();
            Model.Interfaces.IStoreAdditionalInfo StoreList = await ExecuteSingleAsync<Model.Interfaces.IStoreAdditionalInfo>(sql, parameters, type);
            return StoreList;
        }
        public async Task<Model.Interfaces.IStoreAdditionalInfo> SelectStoreAdditionalInfoByStoreUID(string storeUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",  storeUID}
            };

            var sql = @"SELECT id AS Id, 
                           uid AS UID, 
                           created_by AS CreatedBy, 
                           created_time AS CreatedTime, 
                           modified_by AS ModifiedBy, 
                           modified_time AS ModifiedTime, 
                           server_add_time AS ServerAddTime, 
                           server_modified_time AS ServerModifiedTime, 
                           store_uid AS StoreUID, 
                           order_type AS OrderType, 
                           is_promotions_block AS IsPromotionsBlock, 
                           customer_start_date AS CustomerStartDate, 
                           customer_end_date AS CustomerEndDate, 
                           purchase_order_number AS PurchaseOrderNumber, 
                           delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                           is_with_printed_invoices AS IsWithPrintedInvoices, 
                           is_capture_signature_required AS IsCaptureSignatureRequired, 
                           is_always_printed AS IsAlwaysPrinted, 
                           building_delivery_code AS BuildingDeliveryCode, 
                           delivery_information AS DeliveryInformation, 
                           is_stop_delivery AS IsStopDelivery, 
                           is_forecast_top_up_qty AS IsForeCastTopUpQty, 
                           is_temperature_check AS IsTemperatureCheck, 
                           invoice_start_date AS InvoiceStartDate, 
                           invoice_end_date AS InvoiceEndDate, 
                           invoice_format AS InvoiceFormat, 
                           invoice_delivery_method AS InvoiceDeliveryMethod, 
                           display_delivery_docket AS DisplayDeliveryDocket, 
                           display_price AS DisplayPrice, 
                           show_cust_po AS ShowCustPO, 
                           invoice_text AS InvoiceText, 
                           invoice_frequency AS InvoiceFrequency, 
                           stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, 
                           admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                           admin_fee_per_delivery AS AdminFeePerDelivery, 
                           late_payment_fee AS LatePaymentFee, 
                           drawer AS Drawer, 
                           bank_uid AS BankU, 
                           bank_account AS BankAccount, 
                           mandatory_po_number AS MandatoryPONumber, 
                           is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                           store_credit_always_printed AS StoreCreditAlwaysPrinted, 
                           is_dummy_customer AS IsDummyCustomer, 
                           default_run AS DefaultRun, 
                           is_foc_customer AS IsFOCCustomer, 
                           rss_show_price AS RSSShowPrice, 
                           rss_show_payment AS RSSShowPayment, 
                           rss_show_credit AS RSSShowCredit, 
                           rss_show_invoice AS RSSShowInvoice, 
                           rss_is_active AS RSSIsActive, 
                           rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                           rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, 
                           rss_order_placed_in_rss AS RSSOrderPlacedInRSS, 
                           rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, 
                           rss_total_order_value AS RSSTotalOrderValue, 
                           allow_force_check_in AS AllowForceCheckIn, 
                           is_manual_edit_allowed AS IsManualEditAllowed, 
                           can_update_lat_long AS CanUpdateLatLong, 
                           allow_good_return AS AllowGoodReturn, 
                           allow_bad_return AS AllowBadReturn, 
                         
                           allow_replacement AS AllowReplacement, 
                           is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed, 
                           is_delivery_note_required AS IsDeliveryNoteRequired, 
                           e_invoicing_enabled AS EInvoicingEnabled, 
                           image_recognition_enabled AS ImageRecognizationEnabled, 
                           max_outstanding_invoices AS MaxOutstandingInvoices, 
                           negative_invoice_allowed AS NegativeInvoiceAllowed, 
                           delivery_mode AS DeliveryMode, 
                           visit_frequency AS VisitFrequency, 
                           shipping_contact_same_as_store AS ShippingContactSameAsStore, 
                           billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                           payment_mode AS PaymentMode, 
                           price_type AS PriceType, 
                           average_monthly_income AS AverageMonthlyIncome, 
                           default_bank_uid AS DefaultBankUid, 
                           account_number AS AccountNumber, 
                           no_of_cash_counters AS NoOfCashCounters, 
                           custom_field1 AS CustomField1, 
                           custom_field2 AS CustomField2, 
                           custom_field3 AS CustomField3, 
                           custom_field4 AS CustomField4, 
                           custom_field5 AS CustomField5, 
                           custom_field6 AS CustomField6, 
                           custom_field7 AS CustomField7, 
                           custom_field8 AS CustomField8, 
                           custom_field9 AS CustomField9, 
                           custom_field10 AS CustomField10, 
                           is_asset_enabled AS IsAssetEnabled, 
                           is_survey_enabled AS IsSurveyEnabled, 
                          
                           allow_return_against_invoice AS AllowReturnAgainstInvoice, 
                           allow_return_with_sales_order AS AllowReturnWithSalesOrder, 
                           week_off_sun AS WeekOffSun, 
                           week_off_mon AS WeekOffMon, 
                           week_off_tue AS WeekOffTue, 
                           week_off_wed AS WeekOffWed, 
                           week_off_thu AS WeekOffThu, 
                           week_off_fri AS WeekOffFri, 
                           week_off_sat AS WeekOffSat, 
                           aging_cycle AS AgingCycle, 
                           depot AS Depot ,default_route_uid as DefaultRouteUID,
                           firm_reg_no AS FirmRegNo,
                           company_reg_no AS CompanyRegNo,
                           is_mcme AS IsMSME,
                           firm_type AS FirmType,
                           acc_soft_name AS AccSoftName,
                           acc_soft_license_no AS AccSoftLicenseNo,
                           acc_soft_version_no AS AccSoftVersionNo, 
                           website AS  WebSite
                           FROM public.store_additional_info where store_uid=@storeUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAdditionalInfo>().GetType();
            Model.Interfaces.IStoreAdditionalInfo StoreList = await ExecuteSingleAsync<Model.Interfaces.IStoreAdditionalInfo>(sql, parameters, type);
            return StoreList;
        }
        public async Task<int> CreateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {
                var sql = @"INSERT INTO public.store_additional_info (
                                    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, store_uid, order_type, is_promotions_block, 
                                    customer_start_date, customer_end_date, purchase_order_number, delivery_docket_is_purchase_order_required, is_with_printed_invoices, 
                                    is_capture_signature_required, is_always_printed, building_delivery_code, delivery_information, is_stop_delivery, is_forecast_top_up_qty, 
                                    is_temperature_check, invoice_start_date, invoice_end_date, invoice_format, invoice_delivery_method, display_delivery_docket, display_price, 
                                    show_cust_po, invoice_text, invoice_frequency, stock_credit_is_purchase_order_required, admin_fee_per_billing_cycle, admin_fee_per_delivery, 
                                    late_payment_fee, drawer, bank_uid, bank_account, mandatory_po_number, is_store_credit_capture_signature_required, store_credit_always_printed, 
                                    is_dummy_customer, default_run, is_foc_customer, rss_show_price, rss_show_payment, rss_show_credit, rss_show_invoice, rss_is_active, 
                                    rss_delivery_instruction_status, rss_time_spent_on_rss_portal, rss_order_placed_in_rss, rss_avg_orders_per_week, rss_total_order_value, 
                                    allow_force_check_in, is_manual_edit_allowed, can_update_lat_long, allow_good_return, allow_bad_return,
                                    allow_replacement, is_invoice_cancellation_allowed, is_delivery_note_required, e_invoicing_enabled, image_recognition_enabled, max_outstanding_invoices, 
                                    negative_invoice_allowed, delivery_mode, visit_frequency, shipping_contact_same_as_store, billing_address_same_as_shipping, payment_mode, price_type, 
                                    average_monthly_income, default_bank_uid, account_number, no_of_cash_counters, custom_field1, custom_field2, custom_field3, custom_field4, 
                                    custom_field5, custom_field6, custom_field7, custom_field8, custom_field9, custom_field10, is_asset_enabled, is_survey_enabled, 
                                    allow_return_against_invoice, allow_return_with_sales_order, week_off_sun, week_off_mon, week_off_tue, week_off_wed, week_off_thu, 
                                    week_off_fri, week_off_sat, aging_cycle, depot,default_route_uid,firm_reg_no,company_reg_no,is_mcme,firm_type,
                            acc_soft_name,acc_soft_license_no,acc_soft_version_no,website
                                )
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime, @StoreUID, @OrderType, @IsPromotionsBlock, @CustomerStartDate, @CustomerEndDate, @PurchaseOrderNumber, 
                        @DeliveryDocketIsPurchaseOrderRequired, @IsWithPrintedInvoices, @IsCaptureSignatureRequired, @IsAlwaysPrinted, @BuildingDeliveryCode,
                        @DeliveryInformation, @IsStopDelivery, @IsForeCastTopUpQty, @IsTemperatureCheck, @InvoiceStartDate, @InvoiceEndDate, @InvoiceFormat,
                        @InvoiceDeliveryMethod, @DisplayDeliveryDocket, @DisplayPrice, @ShowCustPO, @InvoiceText, @InvoiceFrequency, 
                        @StockCreditIsPurchaseOrderRequired, @AdminFeePerBillingCycle, @AdminFeePerDelivery, @LatePaymentFee, @Drawer, @BankUID, 
                        @BankAccount, @MandatoryPONumber, @IsStoreCreditCaptureSignatureRequired, @StoreCreditAlwaysPrinted, @IsDummyCustomer, @DefaultRun, 
                        @IsFOCCustomer, @RSSShowPrice, @RSSShowPayment, @RSSShowCredit, @RSSShowInvoice, @RSSIsActive, @RSSDeliveryInstructionStatus, 
                        @RSSTimeSpentOnRSSPortal, @RSSOrderPlacedInRSS, @RSSAvgOrdersPerWeek, @RSSTotalOrderValue, @AllowForceCheckIn, @IsManualEditAllowed, 
                        @CanUpdateLatLong, @AllowGoodReturn, @AllowBadReturn, @AllowReplacement, @IsInvoiceCancellationAllowed,
                        @IsDeliveryNoteRequired, @EInvoicingEnabled, @ImageRecognizationEnabled, @MaxOutstandingInvoices, @NegativeInvoiceAllowed, @DeliveryMode,
                        @VisitFrequency, @ShippingContactSameAsStore, @BillingAddressSameAsShipping, @PaymentMode, @PriceType, @AverageMonthlyIncome, 
                        @DefaultBankUID, @AccountNumber, @NoOfCashCounters, @CustomField1, @CustomField2, @CustomField3, @CustomField4, @CustomField5, 
                        @CustomField6, @CustomField7, @CustomField8, @CustomField9, @CustomField10,@IsAssetEnabled, @IsSurveyEnabled,
                        @AllowReturnAgainstInvoice, @AllowReturnWithSalesOrder, 
                        @WeekOffSun, @WeekOffMon, @WeekOffTue, @WeekOffWed, @WeekOffThu, @WeekOffFri, @WeekOffSat,@AgingCycle,
                        @Depot,@DefaultRouteUID,@FirmRegNo,@CompanyRegNo,
                        @IsMSME,@FirmType,@AccSoftName,@AccSoftLicenseNo,@AccSoftVersionNo,@WebSite)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAdditionalInfo.UID},
                    {"CreatedBy",storeAdditionalInfo.CreatedBy},
                    {"CreatedTime",storeAdditionalInfo.CreatedTime},
                    {"ModifiedBy",storeAdditionalInfo.ModifiedBy},
                    {"ModifiedTime",storeAdditionalInfo.ModifiedTime},
                    {"ServerAddTime",storeAdditionalInfo.ServerAddTime},
                    {"ServerModifiedTime",storeAdditionalInfo.ServerModifiedTime},
                    {"StoreUID",storeAdditionalInfo.StoreUID},
                    {"OrderType",storeAdditionalInfo.OrderType},
                    {"IsPromotionsBlock",storeAdditionalInfo.IsPromotionsBlock},
                    {"CustomerStartDate",storeAdditionalInfo.CustomerStartDate},
                    {"CustomerEndDate",storeAdditionalInfo.CustomerEndDate},
                    {"PurchaseOrderNumber",storeAdditionalInfo.PurchaseOrderNumber},
                    {"DeliveryDocketIsPurchaseOrderRequired",storeAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired},
                    {"IsWithPrintedInvoices",storeAdditionalInfo.IsWithPrintedInvoices},
                    {"IsCaptureSignatureRequired",storeAdditionalInfo.IsCaptureSignatureRequired},
                    {"IsAlwaysPrinted",storeAdditionalInfo.IsAlwaysPrinted},
                    {"BuildingDeliveryCode",storeAdditionalInfo.BuildingDeliveryCode},
                    {"DeliveryInformation",storeAdditionalInfo.DeliveryInformation},
                    {"IsStopDelivery",storeAdditionalInfo.IsStopDelivery},
                    {"IsForeCastTopUpQty",storeAdditionalInfo.IsForeCastTopUpQty},
                    {"IsTemperatureCheck",storeAdditionalInfo.IsTemperatureCheck},
                    {"InvoiceStartDate",storeAdditionalInfo.InvoiceStartDate},
                    {"InvoiceEndDate",storeAdditionalInfo.InvoiceEndDate},
                    {"InvoiceFormat",storeAdditionalInfo.InvoiceFormat},
                    {"InvoiceDeliveryMethod",storeAdditionalInfo.InvoiceDeliveryMethod},
                    {"DisplayDeliveryDocket",storeAdditionalInfo.DisplayDeliveryDocket},
                    {"DisplayPrice",storeAdditionalInfo.DisplayPrice},
                    {"ShowCustPO",storeAdditionalInfo.ShowCustPO},
                    {"InvoiceText",storeAdditionalInfo.InvoiceText},
                    {"InvoiceFrequency",storeAdditionalInfo.InvoiceFrequency},
                    {"StockCreditIsPurchaseOrderRequired",storeAdditionalInfo.StockCreditIsPurchaseOrderRequired},
                    {"AdminFeePerBillingCycle",storeAdditionalInfo.AdminFeePerBillingCycle},
                    {"AdminFeePerDelivery",storeAdditionalInfo.AdminFeePerDelivery},
                    {"LatePaymentFee",storeAdditionalInfo.LatePaymentFee},
                    {"Drawer",storeAdditionalInfo.Drawer},
                    {"BankUID",storeAdditionalInfo.BankUID},
                    {"BankAccount",storeAdditionalInfo.BankAccount},
                    {"MandatoryPONumber",storeAdditionalInfo.MandatoryPONumber},
                    {"IsStoreCreditCaptureSignatureRequired",storeAdditionalInfo.IsStoreCreditCaptureSignatureRequired},
                    {"StoreCreditAlwaysPrinted",storeAdditionalInfo.StoreCreditAlwaysPrinted},
                    {"IsDummyCustomer",storeAdditionalInfo.IsDummyCustomer},
                    {"DefaultRun",storeAdditionalInfo.DefaultRun},
                    {"IsFOCCustomer",storeAdditionalInfo.IsFOCCustomer},
                    {"RSSShowPrice",storeAdditionalInfo.RSSShowPrice},
                    {"RSSShowPayment",storeAdditionalInfo.RSSShowPayment},
                    {"RSSShowCredit",storeAdditionalInfo.RSSShowCredit},
                    {"RSSShowInvoice",storeAdditionalInfo.RSSShowInvoice},
                    {"RSSIsActive",storeAdditionalInfo.RSSIsActive},
                    {"RSSDeliveryInstructionStatus",storeAdditionalInfo.RSSDeliveryInstructionStatus},
                    {"RSSTimeSpentOnRSSPortal",storeAdditionalInfo.RSSTimeSpentOnRSSPortal},
                    {"RSSOrderPlacedInRSS",storeAdditionalInfo.RSSOrderPlacedInRSS},
                    {"RSSAvgOrdersPerWeek",storeAdditionalInfo.RSSAvgOrdersPerWeek},
                    {"RSSTotalOrderValue",storeAdditionalInfo.RSSTotalOrderValue},
                    {"AllowForceCheckIn",storeAdditionalInfo.AllowForceCheckIn},
                    {"IsManualEditAllowed",storeAdditionalInfo.IsManualEditAllowed},
                    {"CanUpdateLatLong",storeAdditionalInfo.CanUpdateLatLong},
                    {"AllowGoodReturn",storeAdditionalInfo.AllowGoodReturn},
                    {"AllowBadReturn",storeAdditionalInfo.AllowBadReturn},
                   
                    {"AllowReplacement",storeAdditionalInfo.AllowReplacement},
                    {"IsInvoiceCancellationAllowed",storeAdditionalInfo.IsInvoiceCancellationAllowed},
                    {"IsDeliveryNoteRequired",storeAdditionalInfo.IsDeliveryNoteRequired},
                    {"EInvoicingEnabled",storeAdditionalInfo.EInvoicingEnabled},
                    {"ImageRecognizationEnabled",storeAdditionalInfo.ImageRecognizationEnabled},
                    {"MaxOutstandingInvoices",storeAdditionalInfo.MaxOutstandingInvoices},
                    {"NegativeInvoiceAllowed",storeAdditionalInfo.NegativeInvoiceAllowed},
                    {"DeliveryMode",storeAdditionalInfo.DeliveryMode},
                    {"VisitFrequency",storeAdditionalInfo.VisitFrequency},
                    {"ShippingContactSameAsStore",storeAdditionalInfo.ShippingContactSameAsStore},
                    {"BillingAddressSameAsShipping",storeAdditionalInfo.BillingAddressSameAsShipping},
                    {"PaymentMode",storeAdditionalInfo.PaymentMode},
                    {"PriceType",storeAdditionalInfo.PriceType},
                    {"AverageMonthlyIncome",storeAdditionalInfo.AverageMonthlyIncome},
                    {"DefaultBankUID",storeAdditionalInfo.DefaultBankUID},
                    {"AccountNumber",storeAdditionalInfo.AccountNumber},
                    {"NoOfCashCounters",storeAdditionalInfo.NoOfCashCounters},
                    {"CustomField1",storeAdditionalInfo.CustomField1},
                    {"CustomField2",storeAdditionalInfo.CustomField2},
                    {"CustomField3",storeAdditionalInfo.CustomField3},
                    {"CustomField4",storeAdditionalInfo.CustomField4},
                    {"CustomField5",storeAdditionalInfo.CustomField5},
                    {"CustomField6",storeAdditionalInfo.CustomField6},
                    {"CustomField7",storeAdditionalInfo.CustomField7},
                    {"CustomField8",storeAdditionalInfo.CustomField8},
                    {"CustomField9",storeAdditionalInfo.CustomField9},
                    {"CustomField10",storeAdditionalInfo.CustomField10},
                    {"IsAssetEnabled",storeAdditionalInfo.IsAssetEnabled},
                    {"IsSurveyEnabled",storeAdditionalInfo.IsSurveyEnabled},
                  
                    {"AllowReturnAgainstInvoice",storeAdditionalInfo.AllowReturnAgainstInvoice},
                    {"AllowReturnWithSalesOrder",storeAdditionalInfo.AllowReturnWithSalesOrder},
                    {"WeekOffSun",storeAdditionalInfo.WeekOffSun},
                    {"WeekOffMon",storeAdditionalInfo.WeekOffMon},
                    {"WeekOffTue",storeAdditionalInfo.WeekOffTue},
                    {"WeekOffWed",storeAdditionalInfo.WeekOffWed},
                    {"WeekOffThu",storeAdditionalInfo.WeekOffThu},
                    {"WeekOffFri",storeAdditionalInfo.WeekOffFri},
                    {"WeekOffSat",storeAdditionalInfo.WeekOffSat},
                    {"AgingCycle",storeAdditionalInfo.AgingCycle},
                    {"Depot",storeAdditionalInfo.Depot},
                    {"DefaultRouteUID",storeAdditionalInfo.DefaultRouteUID},
                     {"FirmRegNo",storeAdditionalInfo.FirmRegNo},
                    {"CompanyRegNo",storeAdditionalInfo.CompanyRegNo},
                    {"IsMSME",storeAdditionalInfo.IsMSME},
                    {"FirmType",storeAdditionalInfo.FirmType},
                    {"AccSoftName",storeAdditionalInfo.AccSoftName},
                    {"AccSoftLicenseNo",storeAdditionalInfo.AccSoftLicenseNo},
                    {"AccSoftVersionNo",storeAdditionalInfo.AccSoftVersionNo},
                    {"WebSite",storeAdditionalInfo.WebSite},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreAdditionalInfo(Model.Interfaces.IStoreAdditionalInfo storeAdditionalInfo)
        {
            try
            {
                string sql = @"UPDATE store_additional_info 
                            SET 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                store_uid = @StoreUID, 
                                order_type = @OrderType, 
                                is_promotions_block = @IsPromotionsBlock, 
                                customer_start_date = @CustomerStartDate, 
                                customer_end_date = @CustomerEndDate, 
                                purchase_order_number = @PurchaseOrderNumber, 
                                delivery_docket_is_purchase_order_required = @DeliveryDocketIsPurchaseOrderRequired, 
                                is_with_printed_invoices = @IsWithPrintedInvoices, 
                                is_capture_signature_required = @IsCaptureSignatureRequired, 
                                is_always_printed = @IsAlwaysPrinted, 
                                building_delivery_code = @BuildingDeliveryCode, 
                                delivery_information = @DeliveryInformation, 
                                is_stop_delivery = @IsStopDelivery, 
                                is_forecast_top_up_qty = @IsForeCastTopUpQty, 
                                is_temperature_check = @IsTemperatureCheck, 
                                invoice_start_date = @InvoiceStartDate, 
                                invoice_end_date = @InvoiceEndDate, 
                                invoice_format = @InvoiceFormat, 
                                invoice_delivery_method = @InvoiceDeliveryMethod, 
                                display_delivery_docket = @DisplayDeliveryDocket, 
                                display_price = @DisplayPrice, 
                                show_cust_po = @ShowCustPO, 
                                invoice_text = @InvoiceText, 
                                invoice_frequency = @InvoiceFrequency, 
                                stock_credit_is_purchase_order_required = @StockCreditIsPurchaseOrderRequired, 
                                admin_fee_per_billing_cycle = @AdminFeePerBillingCycle, 
                                admin_fee_per_delivery = @AdminFeePerDelivery, 
                                late_payment_fee = @LatePaymentFee, 
                                drawer = @Drawer, 
                                bank_uid = @BankUID, 
                                bank_account = @BankAccount, 
                                mandatory_po_number = @MandatoryPoNumber, 
                                is_store_credit_capture_signature_required = @IsStoreCreditCaptureSignatureRequired, 
                                store_credit_always_printed = @StoreCreditAlwaysPrinted, 
                                is_dummy_customer = @IsDummyCustomer, 
                                default_run = @DefaultRun, 
                                is_foc_customer = @IsFOCCustomer, 
                                rss_show_price = @RSSShowPrice, 
                                rss_show_payment = @RSSShowPayment, 
                                rss_show_credit = @RSSShowCredit, 
                                rss_show_invoice = @RSSShowInvoice, 
                                rss_is_active = @RSSIsActive, 
                                rss_delivery_instruction_status = @RSSDeliveryInstructionStatus, 
                                rss_time_spent_on_rss_portal = @RSSTimeSpentOnRSSPortal, 
                                rss_order_placed_in_rss = @RSSOrderPlacedInRSS, 
                                rss_avg_orders_per_week = @RSSAvgOrdersPerWeek, 
                                rss_total_order_value = @RSSTotalOrderValue, 
                                allow_force_check_in = @AllowForceCheckIn, 
                                is_manual_edit_allowed = @IsManualEditAllowed, 
                                can_update_lat_long = @CanUpdateLatLong, 
                                allow_good_return = @AllowGoodReturn, 
                                allow_bad_return = @AllowBadReturn, 
                               
                                allow_replacement = @AllowReplacement, 
                                is_invoice_cancellation_allowed = @IsInvoiceCancellationAllowed, 
                                is_delivery_note_required = @IsDeliveryNoteRequired, 
                                e_invoicing_enabled = @EInvoicingEnabled, 
                                image_recognition_enabled = @ImageRecognizationEnabled, 
                                max_outstanding_invoices = @MaxOutstandingInvoices, 
                                negative_invoice_allowed = @NegativeInvoiceAllowed, 
                                delivery_mode = @DeliveryMode, 
                                visit_frequency = @VisitFrequency, 
                                shipping_contact_same_as_store = @ShippingContactSameAsStore, 
                                billing_address_same_as_shipping = @BillingAddressSameAsShipping, 
                                payment_mode = @PaymentMode, 
                                price_type = @PriceType, 
                                average_monthly_income = @AverageMonthlyIncome, 
                                default_bank_uid = @DefaultBankUID, 
                                account_number = @AccountNumber, 
                                no_of_cash_counters = @NoOfCashCounters, 
                                custom_field1 = @CustomField1, 
                                custom_field2 = @CustomField2, 
                                custom_field3 = @CustomField3, 
                                custom_field4 = @CustomField4, 
                                custom_field5 = @CustomField5, 
                                custom_field6 = @CustomField6, 
                                custom_field7 = @CustomField7, 
                                custom_field8 = @CustomField8, 
                                custom_field9 = @CustomField9, 
                                custom_field10 = @CustomField10, 
                                is_asset_enabled = @IsAssetEnabled, 
                                is_survey_enabled = @IsSurveyEnabled, 
                            
                                allow_return_against_invoice = @AllowReturnAgainstInvoice, 
                                allow_return_with_sales_order = @AllowReturnWithSalesOrder, 
                                week_off_sun = @WeekOffSun, 
                                week_off_mon = @WeekOffMon, 
                                week_off_tue = @WeekOffTue, 
                                week_off_wed = @WeekOffWed, 
                                week_off_thu = @WeekOffThu, 
                                week_off_fri = @WeekOffFri, 
                                week_off_sat = @WeekOffSat, 
                                aging_cycle = @AgingCycle, 
                                depot = @Depot ,
                               default_route_uid=@DefaultRouteUID,
                                firm_reg_no=@FirmRegNo,
                                company_reg_no=@CompanyRegNo,
                                is_mcme=@IsMSME,
                                firm_type=@FirmType,
                                acc_soft_name=@AccSoftName,
                                acc_soft_license_no=@AccSoftLicenseNo,
                                acc_soft_version_no=@AccSoftVersionNo,
                                website=@WebSite
                                WHERE  uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAdditionalInfo.UID},
                    {"ModifiedBy",storeAdditionalInfo.ModifiedBy},
                    {"ModifiedTime",storeAdditionalInfo.ModifiedTime},
                    {"ServerModifiedTime",storeAdditionalInfo.ServerModifiedTime},
                    {"StoreUID",storeAdditionalInfo.StoreUID},
                    {"OrderType",storeAdditionalInfo.OrderType},
                    {"IsPromotionsBlock",storeAdditionalInfo.IsPromotionsBlock},
                    {"CustomerStartDate",storeAdditionalInfo.CustomerStartDate},
                    {"CustomerEndDate",storeAdditionalInfo.CustomerEndDate},
                    {"PurchaseOrderNumber",storeAdditionalInfo.PurchaseOrderNumber},
                    {"DeliveryDocketIsPurchaseOrderRequired",storeAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired},
                    {"IsWithPrintedInvoices",storeAdditionalInfo.IsWithPrintedInvoices},
                    {"IsCaptureSignatureRequired",storeAdditionalInfo.IsCaptureSignatureRequired},
                    {"IsAlwaysPrinted",storeAdditionalInfo.IsAlwaysPrinted},
                    {"BuildingDeliveryCode",storeAdditionalInfo.BuildingDeliveryCode},
                    {"DeliveryInformation",storeAdditionalInfo.DeliveryInformation},
                    {"IsStopDelivery",storeAdditionalInfo.IsStopDelivery},
                    {"IsForeCastTopUpQty",storeAdditionalInfo.IsForeCastTopUpQty},
                    {"IsTemperatureCheck",storeAdditionalInfo.IsTemperatureCheck},
                    {"InvoiceStartDate",storeAdditionalInfo.InvoiceStartDate},
                    {"InvoiceEndDate",storeAdditionalInfo.InvoiceEndDate},
                    {"InvoiceFormat",storeAdditionalInfo.InvoiceFormat},
                    {"InvoiceDeliveryMethod",storeAdditionalInfo.InvoiceDeliveryMethod},
                    {"DisplayDeliveryDocket",storeAdditionalInfo.DisplayDeliveryDocket},
                    {"DisplayPrice",storeAdditionalInfo.DisplayPrice},
                    {"ShowCustPO",storeAdditionalInfo.ShowCustPO},
                    {"InvoiceText",storeAdditionalInfo.InvoiceText},
                    {"InvoiceFrequency",storeAdditionalInfo.InvoiceFrequency},
                    {"StockCreditIsPurchaseOrderRequired",storeAdditionalInfo.StockCreditIsPurchaseOrderRequired},
                    {"AdminFeePerBillingCycle",storeAdditionalInfo.AdminFeePerBillingCycle},
                    {"AdminFeePerDelivery",storeAdditionalInfo.AdminFeePerDelivery},
                    {"LatePaymentFee",storeAdditionalInfo.LatePaymentFee},
                    {"Drawer",storeAdditionalInfo.Drawer},
                    {"BankUID",storeAdditionalInfo.BankUID},
                    {"BankAccount",storeAdditionalInfo.BankAccount},
                    {"MandatoryPONumber",storeAdditionalInfo.MandatoryPONumber},
                    {"IsStoreCreditCaptureSignatureRequired",storeAdditionalInfo.IsStoreCreditCaptureSignatureRequired},
                    {"StoreCreditAlwaysPrinted",storeAdditionalInfo.StoreCreditAlwaysPrinted},
                    {"IsDummyCustomer",storeAdditionalInfo.IsDummyCustomer},
                    {"DefaultRun",storeAdditionalInfo.DefaultRun},
                    {"IsFOCCustomer",storeAdditionalInfo.IsFOCCustomer},
                    {"RSSShowPrice",storeAdditionalInfo.RSSShowPrice},
                    {"RSSShowPayment",storeAdditionalInfo.RSSShowPayment},
                    {"RSSShowCredit",storeAdditionalInfo.RSSShowCredit},
                    {"RSSShowInvoice",storeAdditionalInfo.RSSShowInvoice},
                    {"RSSIsActive",storeAdditionalInfo.RSSIsActive},
                    {"RSSDeliveryInstructionStatus",storeAdditionalInfo.RSSDeliveryInstructionStatus},
                    {"RSSTimeSpentOnRSSPortal",storeAdditionalInfo.RSSTimeSpentOnRSSPortal},
                    {"RSSOrderPlacedInRSS",storeAdditionalInfo.RSSOrderPlacedInRSS},
                    {"RSSAvgOrdersPerWeek",storeAdditionalInfo.RSSAvgOrdersPerWeek},
                    {"RSSTotalOrderValue",storeAdditionalInfo.RSSTotalOrderValue},
                    {"AllowForceCheckIn",storeAdditionalInfo.AllowForceCheckIn},
                    {"IsManualEditAllowed",storeAdditionalInfo.IsManualEditAllowed},
                    {"CanUpdateLatLong",storeAdditionalInfo.CanUpdateLatLong},
                    {"AllowGoodReturn",storeAdditionalInfo.AllowGoodReturn},
                    {"AllowBadReturn",storeAdditionalInfo.AllowBadReturn},
               
                    {"AllowReplacement",storeAdditionalInfo.AllowReplacement},
                    {"IsInvoiceCancellationAllowed",storeAdditionalInfo.IsInvoiceCancellationAllowed},
                    {"IsDeliveryNoteRequired",storeAdditionalInfo.IsDeliveryNoteRequired},
                    {"EInvoicingEnabled",storeAdditionalInfo.EInvoicingEnabled},
                    {"ImageRecognizationEnabled",storeAdditionalInfo.ImageRecognizationEnabled},
                    {"MaxOutstandingInvoices",storeAdditionalInfo.MaxOutstandingInvoices},
                    {"NegativeInvoiceAllowed",storeAdditionalInfo.NegativeInvoiceAllowed},
                    {"DeliveryMode",storeAdditionalInfo.DeliveryMode},
                    {"VisitFrequency",storeAdditionalInfo.VisitFrequency},
                    {"ShippingContactSameAsStore",storeAdditionalInfo.ShippingContactSameAsStore},
                    {"BillingAddressSameAsShipping",storeAdditionalInfo.BillingAddressSameAsShipping},
                    {"PaymentMode",storeAdditionalInfo.PaymentMode},
                    {"PriceType",storeAdditionalInfo.PriceType},
                    {"AverageMonthlyIncome",storeAdditionalInfo.AverageMonthlyIncome},
                    {"DefaultBankUID",storeAdditionalInfo.DefaultBankUID},
                    {"AccountNumber",storeAdditionalInfo.AccountNumber},
                    {"NoOfCashCounters",storeAdditionalInfo.NoOfCashCounters},
                    {"CustomField1",storeAdditionalInfo.CustomField1},
                    {"CustomField2",storeAdditionalInfo.CustomField2},
                    {"CustomField3",storeAdditionalInfo.CustomField3},
                    {"CustomField4",storeAdditionalInfo.CustomField4},
                    {"CustomField5",storeAdditionalInfo.CustomField5},
                    {"CustomField6",storeAdditionalInfo.CustomField6},
                    {"CustomField7",storeAdditionalInfo.CustomField7},
                    {"CustomField8",storeAdditionalInfo.CustomField8},
                    {"CustomField9",storeAdditionalInfo.CustomField9},
                    {"CustomField10",storeAdditionalInfo.CustomField10},
                    {"IsAssetEnabled",storeAdditionalInfo.IsAssetEnabled},
                    {"IsSurveyEnabled",storeAdditionalInfo.IsSurveyEnabled},
         
                    {"AllowReturnAgainstInvoice",storeAdditionalInfo.AllowReturnAgainstInvoice},
                    {"AllowReturnWithSalesOrder",storeAdditionalInfo.AllowReturnWithSalesOrder},
                    {"WeekOffSun",storeAdditionalInfo.WeekOffSun},
                    {"WeekOffMon",storeAdditionalInfo.WeekOffMon},
                    {"WeekOffTue",storeAdditionalInfo.WeekOffTue},
                    {"WeekOffWed",storeAdditionalInfo.WeekOffWed},
                    {"WeekOffThu",storeAdditionalInfo.WeekOffThu},
                    {"WeekOffFri",storeAdditionalInfo.WeekOffFri},
                    {"WeekOffSat",storeAdditionalInfo.WeekOffSat},
                    {"AgingCycle",storeAdditionalInfo.AgingCycle},
                    {"Depot",storeAdditionalInfo.Depot},
                    {"IsManualEditAllowed",storeAdditionalInfo.IsManualEditAllowed},
                    {"DefaultRouteUID",storeAdditionalInfo.DefaultRouteUID},
                    {"FirmRegNo",storeAdditionalInfo.FirmRegNo},
                    {"CompanyRegNo",storeAdditionalInfo.CompanyRegNo},
                    {"IsMSME",storeAdditionalInfo.IsMSME},
                    {"FirmType",storeAdditionalInfo.FirmType},
                    {"AccSoftName",storeAdditionalInfo.AccSoftName},
                    {"AccSoftLicenseNo",storeAdditionalInfo.AccSoftLicenseNo},
                    {"AccSoftVersionNo",storeAdditionalInfo.AccSoftVersionNo},
                    {"WebSite",storeAdditionalInfo.WebSite},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreAdditionalInfo(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM StoreAdditionalInfo WHERE UID = @UID";
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








