using Microsoft.Extensions.DependencyInjection;
using Nest;
using System.Text;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.StoreMaster.Model.Classes;
using Winit.Modules.StoreMaster.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes;

public class SQLiteStoreDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IStoreDL
{
    public SQLiteStoreDL(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>> SelectAllStore(
        List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(
                "SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, " +
                "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, " +
                "code AS Code, number AS Number, name AS Name, alias_name AS AliasName, legal_name AS LegalName, type AS Type, bill_to_store_uid AS BillToStoreUID, " +
                "ship_to_store_uid AS ShipToStoreUID, sold_to_store_uid AS SoldToStoreUID, status AS Status, is_active AS IsActive, store_class AS StoreClass, " +
                "store_rating AS StoreRating, is_blocked AS IsBlocked, blocked_reason_code AS Blocked_Reason_Code, blocked_reason_description AS BlockedReasonDescription," +
                "created_by_emp_uid AS CreatedByEmpUID, created_by_job_position_uid AS CreatedByJobPositionUID, country_uid AS CountryUID, region_uid AS RegionUID, " +
                "city_uid AS CityUID, source AS Source, arabic_name AS ArabicName, outlet_name AS OutletName, blocked_by_emp_uid AS BlockedByEmpUID, " +
                "is_tax_applicable AS IsTaxApplicable, tax_doc_number AS TaxDocNumber, school_warehouse AS SchoolWarehouse, day_type AS DayType, special_day AS SpecialDay, " +
                "is_tax_doc_verified AS IsTaxDocVerified, store_size AS StoreSize, prospect_emp_uid AS ProspectEmpUID, tax_key_field AS TaxKeyField, " +
                "store_image AS StoreImage, is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, tax_type AS TaxType, franchisee_org_uid AS FranchiseeOrgUID, " +
                "state_uid AS StateUID, route_type AS RouteType, price_type AS PriceType FROM store;");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM store");
            }
            var parameters = new Dictionary<string, object?>();

            //if (filterCriterias != null && filterCriterias.Count > 0)
            //{
            //    sql.Append(" WHERE ");
            //    AppendFilterCriteria(filterCriterias, sql, parameters);
            //}

            //if (sortCriterias != null && sortCriterias.Count > 0)
            //{
            //    sql.Append(" ORDER BY ");
            //    AppendSortCriteria(sortCriterias, sql);
            //}

            //if (pageNumber > 0 && pageSize > 0)
            //{
            //    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            //}
            IEnumerable<Model.Interfaces.IStore> stores =
                (IEnumerable<IStore>)await ExecuteQueryAsync<Model.Interfaces.IStore>(sql.ToString(), parameters);

            PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponse =
                new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>
                {
                    PagedData = stores,
                    TotalCount = 1000
                };

            return pagedResponse;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<Model.Interfaces.IStore> SelectStoreByUID(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "UID", UID }
        };

        var sql =
            @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID,
                    code AS Code, number AS Number, name AS Name, alias_name AS AliasName, legal_name AS LegalName, type AS Type, bill_to_store_uid AS BillToStoreUID,
                    ship_to_store_uid AS ShipToStoreUID, sold_to_store_uid AS SoldToStoreUID, status AS Status, is_active AS IsActive, store_class AS StoreClass,
                    store_rating AS StoreRating, is_blocked AS IsBlocked, blocked_reason_code AS Blocked_Reason_Code, blocked_reason_description AS BlockedReasonDescription,
                    created_by_emp_uid AS CreatedByEmpUID, created_by_job_position_uid AS CreatedByJobPositionUID, country_uid AS CountryUID, region_uid AS RegionUID,
                    city_uid AS CityUID, source AS Source, arabic_name AS ArabicName, outlet_name AS OutletName, blocked_by_emp_uid AS BlockedByEmpUID,
                    is_tax_applicable AS IsTaxApplicable, tax_doc_number AS TaxDocNumber, school_warehouse AS SchoolWarehouse, day_type AS DayType, special_day AS SpecialDay, 
                    is_tax_doc_verified AS IsTaxDocVerified, store_size AS StoreSize, prospect_emp_uid AS ProspectEmpUID, tax_key_field AS TaxKeyField, 
                    store_image AS StoreImage, is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, tax_type AS TaxType, franchisee_org_uid AS FranchiseeOrgUID,
                    state_uid AS StateUID, route_type AS RouteType, price_type AS PriceType FROM Store WHERE uid = @UID";
        Model.Interfaces.IStore StoreList = await ExecuteSingleAsync<Model.Interfaces.IStore>(sql, parameters);
        return StoreList;
    }

    public async Task<int> CreateStore(Model.Interfaces.IStore store)
    {
        try
        {
            var sql = @"INSERT INTO store (
                        id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, 
                        code, number, name, alias_name, legal_name, type, bill_to_store_uid, ship_to_store_uid, sold_to_store_uid, status, 
                        is_active, store_class, store_rating, is_blocked, blocked_reason_code, blocked_reason_description, created_by_emp_uid, 
                        created_by_job_position_uid, country_uid, region_uid, city_uid, source, outlet_name, blocked_by_emp_uid, arabic_name, 
                        is_tax_applicable, tax_doc_number, school_warehouse, day_type, special_day, is_tax_doc_verified, store_size, 
                        prospect_emp_uid, tax_key_field, store_image, is_vat_qr_capture_mandatory, tax_type, franchisee_org_uid)
                        values(@Id  ,@UID  ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@CompanyUID ,
                        @Code ,@Number ,@Name ,@AliasName ,@LegalName ,@Type ,@BillToStoreUID ,@ShipToStoreUID ,@SoldToStoreUID ,@Status ,
                        @IsActive ,@StoreClass ,@StoreRating ,@IsBlocked ,@BlockedReasonCode ,@BlockedReasonDescription ,@CreatedByEmpUID ,
                        @CreatedByJobPositionUID ,@CountryUID ,@RegionUID ,@CityUID ,@Source ,@OutletName ,@BlockedByEmpUID ,@ArabicName ,
                        @IsTaxApplicable ,@TaxDocNumber,@SchoolWarehouse ,@DayType ,@SpecialDay, @IsTaxDocVerified ,
                        @StoreSize ,@ProspectEmpUID ,@TaxKeyField ,@StoreImage ,@IsVATQRCaptureMandatory,@TaxType,@FranchiseeOrgUID)";
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "@Id", store.Id },
                { "@UID", store.UID },
                { "@CreatedBy", store.CreatedBy },
                { "@CreatedTime", store.CreatedTime },
                { "@ModifiedBy", store.ModifiedBy },
                { "@ModifiedTime", store.ModifiedTime },
                { "@ServerAddTime", store.ServerAddTime },
                { "@ServerModifiedTime", store.ServerModifiedTime },
                { "@CompanyUID", store.CompanyUID },
                { "@Code", store.Code },
                { "@Number", store.Number },
                { "@Name", store.Name },
                { "@AliasName", store.AliasName },
                { "@LegalName", store.LegalName },
                { "@Type", store.Type },
                { "@BillToStoreUID", store.BillToStoreUID },
                { "@ShipToStoreUID", store.ShipToStoreUID },
                { "@SoldToStoreUID", store.SoldToStoreUID },
                { "@Status", store.Status },
                { "@IsActive", store.IsActive },
                { "@StoreClass", store.StoreClass },
                { "@StoreRating", store.StoreRating },
                { "@IsBlocked", store.IsBlocked },
                { "@BlockedReasonCode", store.BlockedReasonCode },
                { "@BlockedReasonDescription", store.BlockedReasonDescription },
                { "@CreatedByEmpUID", store.CreatedByEmpUID },
                { "@CreatedByJobPositionUID", store.CreatedByJobPositionUID },
                { "@CountryUID", store.CountryUID },
                { "@RegionUID", store.RegionUID },
                { "@CityUID", store.CityUID },
                { "@Source", store.Source },
                { "@OutletName", store.OutletName },
                { "@BlockedByEmpUID", store.BlockedByEmpUID },
                { "@ArabicName", store.ArabicName },
                { "@IsTaxApplicable", store.IsTaxApplicable },
                { "@TaxDocNumber", store.TaxDocNumber },
                { "@SchoolWarehouse", store.SchoolWarehouse },
                { "@DayType", store.DayType },
                { "@SpecialDay", store.SpecialDay },
                { "@IsTaxDocVerified", store.IsTaxDocVerified },
                { "@StoreSize", store.StoreSize },
                { "@ProspectEmpUID", store.ProspectEmpUID },
                { "@TaxKeyField", store.TaxKeyField },
                { "@StoreImage", store.StoreImage },
                { "@IsVATQRCaptureMandatory", store.IsVATQRCaptureMandatory },
                { "@TaxType", store.TaxType },
                { "@FranchiseeOrgUID", store.FranchiseeOrgUID },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateStore(Model.Interfaces.IStore store)
    {
        try
        {
            var sql = @"UPDATE store SET 
                           modified_by = @ModifiedBy, 
                           modified_time = @ModifiedTime, 
                           server_modified_time = @ServerModifiedTime, 
                           company_uid = @CompanyUID, 
                           code = @Code, 
                           number = @Number, 
                           name = @Name, 
                           alias_name = @AliasName, 
                           legal_name = @LegalName, 
                           type = @Type, 
                           bill_to_store_uid = @BillToStoreUID, 
                           ship_to_store_uid = @ShipToStoreUID, 
                           sold_to_store_uid = @SoldToStoreUID, 
                           status = @Status, 
                           is_active = @IsActive, 
                           store_class = @StoreClass, 
                           store_rating = @StoreRating, 
                           is_blocked = @IsBlocked, 
                           blocked_reason_code = @BlockedReasonCode, 
                           blocked_reason_description = @BlockedReasonDescription, 
                           created_by_emp_uid = @CreatedByEmpUID, 
                           created_by_job_position_uid = @CreatedByJobPositionUID, 
                           country_uid = @CountryUID, 
                           region_uid = @RegionUID, 
                           city_uid = @CityUID, 
                           source = @Source, 
                           outlet_name = @OutletName, 
                           blocked_by_emp_uid = @BlockedByEmpUID, 
                           arabic_name = @ArabicName, 
                           is_tax_applicable = @IsTaxApplicable, 
                           tax_doc_number = @TaxDocNumber, 
                           school_warehouse = @SchoolWarehouse, 
                           day_type = @DayType, 
                           special_day = @SpecialDay, 
                           is_tax_doc_verified = @IsTaxDocVerified, 
                           store_size = @StoreSize, 
                           prospect_emp_uid = @ProspectEmpUID ,
                           tax_key_field = @TaxKeyField, 
                           store_image = @StoreImage, 
                           is_vat_qr_capture_mandatory = @IsVATQRCaptureMandatory, 
                           tax_type = @TaxType 
                           WHERE uid = @UID";

            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "@UID", store.UID },
                { "@ModifiedBy", store.ModifiedBy },
                { "@ModifiedTime", store.ModifiedTime },
                { "@ServerModifiedTime", store.ServerModifiedTime },
                { "@CompanyUID", store.CompanyUID },
                { "@Code", store.Code },
                { "@Number", store.Number },
                { "@Name", store.Name },
                { "@AliasName", store.AliasName },
                { "@LegalName", store.LegalName },
                { "@Type", store.Type },
                { "@BillToStoreUID", store.BillToStoreUID },
                { "@ShipToStoreUID", store.ShipToStoreUID },
                { "@SoldToStoreUID", store.SoldToStoreUID },
                { "@Status", store.Status },
                { "@IsActive", store.IsActive },
                { "@StoreClass", store.StoreClass },
                { "@StoreRating", store.StoreRating },
                { "@IsBlocked", store.IsBlocked },
                { "@BlockedReasonCode", store.BlockedReasonCode },
                { "@BlockedReasonDescription", store.BlockedReasonDescription },
                { "@CreatedByEmpUID", store.CreatedByEmpUID },
                { "@CreatedByJobPositionUID", store.CreatedByJobPositionUID },
                { "@CountryUID", store.CountryUID },
                { "@RegionUID", store.RegionUID },
                { "@CityUID", store.CityUID },
                { "@Source", store.Source },
                { "@OutletName", store.OutletName },
                { "@BlockedByEmpUID", store.BlockedByEmpUID },
                { "@ArabicName", store.ArabicName },
                { "@IsTaxApplicable", store.IsTaxApplicable },
                { "@TaxDocNumber", store.TaxDocNumber },
                { "@SchoolWarehouse", store.SchoolWarehouse },
                { "@DayType", store.DayType },
                { "@SpecialDay", store.SpecialDay },
                { "@IsTaxDocVerified", store.IsTaxDocVerified },
                { "@StoreSize", store.StoreSize },
                { "@ProspectEmpUID", store.ProspectEmpUID },
                { "@TaxKeyField", store.TaxKeyField },
                { "@StoreImage", store.StoreImage },
                { "@IsVATQRCaptureMandatory", store.IsVATQRCaptureMandatory },
                { "@TaxType", store.TaxType },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> DeleteStore(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "UID", UID }
        };
        var sql = "DELETE  FROM store WHERE company_uid = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);
    }

    public Task<int> CreateStoreMaster(StoreViewModelDCO createStoreMaster)
    {
        throw new NotImplementedException();
    }

    public Task<IStoreViewModelDCO> SelectStoreMasterByUID(string UID)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateStoreMaster(StoreViewModelDCO updateStoreMaster)
    {
        throw new NotImplementedException();
    }

    public async Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>,
            List<Winit.Modules.Address.Model.Interfaces.IAddress>, List<Winit.Modules.Contact.Model.Interfaces.IContact>
            )>
        PrepareStoreMaster(List<string> storeUIDs)
    {
        try
        {
            string? commaSeparatedstoreUIDs = null;
            if (storeUIDs != null)
            {
                commaSeparatedstoreUIDs = string.Join("','", storeUIDs);
            }
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "UIDs", commaSeparatedstoreUIDs }
            };
            var storeSql = new StringBuilder(@"SELECT 
                    id AS Id, 
                    uid AS UID, 
                    created_by AS CreatedBy, 
                    created_time AS CreatedTime, 
                    modified_by AS ModifiedBy, 
                    modified_time AS ModifiedTime, 
                    server_add_time AS ServerAddTime, 
                    server_modified_time AS ServerModifiedTime, 
                    company_uid AS CompanyUID, 
                    code AS Code, 
                    number AS Number, 
                    name AS Name, 
                    alias_name AS AliasName, 
                    legal_name AS LegalName, 
                    type AS Type, 
                    bill_to_store_uid AS BillToStoreUID, 
                    ship_to_store_uid AS ShipToStoreUID, 
                    sold_to_store_uid AS SoldToStoreUID, 
                    status AS Status, 
                    is_active AS IsActive, 
                    store_class AS StoreClass, 
                    store_rating AS StoreRating, 
                    is_blocked AS IsBlocked, 
                    blocked_reason_code AS BlockedReasonCode, 
                    blocked_reason_description AS BlockedReasonDescription, 
                    created_by_emp_uid AS CreatedByEmpUID, 
                    created_by_job_position_uid AS CreatedByJobPositionUID, 
                    country_uid AS CountryUID, 
                    region_uid AS RegionUID, 
                    city_uid AS CityUID, 
                    source AS Source, 
                    outlet_name AS OutletName, 
                    blocked_by_emp_uid AS BlockedByEmpUID, 
                    arabic_name AS ArabicName, 
                    is_tax_applicable AS IsTaxApplicable, 
                    tax_doc_number AS TaxDocNumber, 
                    school_warehouse AS SchoolWarehouse, 
                    day_type AS DayType, 
                    special_day AS SpecialDay, 
                    is_tax_doc_verified AS IsTaxDocVerified, 
                    store_size AS StoreSize, 
                    prospect_emp_uid AS ProspectEmpUID, 
                    tax_key_field AS TaxKeyField, 
                    store_image AS StoreImage, 
                    is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, 
                    tax_type AS TaxType
                FROM store");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                storeSql.Append($" WHERE uid IN ('{commaSeparatedstoreUIDs}');");
            }

            var storeParameters = new Dictionary<string, object?>();
            Type storeType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStore>()
                .GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStore> storeList =
                await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStore>(storeSql.ToString(), parameters,
                    storeType);

            var storeAdditionalInfoSql = new StringBuilder(@"SELECT 
                    id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                    store_uid AS StoreUID, order_type AS OrderType, is_promotions_block AS IsPromotionsBlock, 
                    customer_start_date AS CustomerStartDate, customer_end_date AS CustomerEndDate, purchase_order_number AS PurchaseOrderNumber, 
                    delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, is_with_printed_invoices AS IsWithPrintedInvoices, 
                    is_capture_signature_required AS IsCaptureSignatureRequired, is_always_printed AS IsAlwaysPrinted, building_delivery_code AS BuildingDeliveryCode, 
                    delivery_information AS DeliveryInformation, is_stop_delivery AS IsStopDelivery, is_forecast_top_up_qty AS IsForeCastTopUpQty, 
                    is_temperature_check AS IsTemperatureCheck, invoice_start_date AS InvoiceStartDate, invoice_end_date AS InvoiceEndDate, 
                    invoice_format AS InvoiceFormat, invoice_delivery_method AS InvoiceDeliveryMethod, display_delivery_docket AS DisplayDeliveryDocket, 
                    display_price AS DisplayPrice, show_cust_po AS ShowCustPO, invoice_text AS InvoiceText, invoice_frequency AS InvoiceFrequency, 
                    stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired, admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, 
                    admin_fee_per_delivery AS AdminFeePerDelivery, late_payment_fee AS LatePaymentFee, drawer AS Drawer, bank_uid AS BankUID, 
                    bank_account AS BankAccount, mandatory_po_number AS MandatoryPONumber, is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                    store_credit_always_printed AS StoreCreditAlwaysPrinted, is_dummy_customer AS IsDummyCustomer, default_run AS DefaultRun, 
                    is_foc_customer AS IsFOCCustomer, rss_show_price AS RSSShowPrice, rss_show_payment AS RSSShowPayment, rss_show_credit AS RSSShowCredit, 
                    rss_show_invoice AS RSSShowInvoice, rss_is_active AS RSSIsActive, rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                    rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, rss_order_placed_in_rss AS RSSOrderPlacedInRSS, 
                    rss_avg_orders_per_week AS RSSAvgOrdersPerWeek, rss_total_order_value AS RSSTotalOrderValue, allow_force_check_in AS AllowForceCheckIn, 
                    is_manual_edit_allowed AS IsManualEditAllowed, can_update_lat_long AS CanUpdateLatLong,  allow_replacement AS AllowReplacement, 
                    is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed, is_delivery_note_required AS IsDeliveryNoteRequired, 
                    e_invoicing_enabled AS EInvoicingEnabled, image_recognition_enabled AS ImageRecognizationEnabled, max_outstanding_invoices AS MaxOutstandingInvoices, 
                    negative_invoice_allowed AS NegativeInvoiceAllowed, delivery_mode AS DeliveryMode, visit_frequency AS VisitFrequency, 
                    shipping_contact_same_as_store AS ShippingContactSameAsStore, billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                    payment_mode AS PaymentMode, price_type AS PriceType, average_monthly_income AS AverageMonthlyIncome, default_bank_uid AS DefaultBankUID, 
                    account_number AS AccountNumber, no_of_cash_counters AS NoOfCashCounters, custom_field1 AS CustomField1, custom_field2 AS CustomField2, 
                    custom_field3 AS CustomField3, custom_field4 AS CustomField4, custom_field5 AS CustomField5, custom_field6 AS CustomField6, 
                    custom_field7 AS CustomField7, custom_field8 AS CustomField8, custom_field9 AS CustomField9, custom_field10 AS CustomField10, 
                    is_asset_enabled AS IsAssetEnabled, is_survey_enabled AS IsSurveyEnabled, allow_return_against_invoice AS AllowReturnAgainstInvoice, allow_return_with_sales_order AS AllowReturnWithSalesOrder, 
                    week_off_sun AS WeekOffSun, week_off_mon AS WeekOffMon, week_off_tue AS WeekOffTue, week_off_wed AS WeekOffWed, 
                    week_off_thu AS WeekOffThu, week_off_fri AS WeekOffFri, week_off_sat AS WeekOffSat
                FROM store_additional_info");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                storeAdditionalInfoSql.Append($" WHERE store_uid IN ('{commaSeparatedstoreUIDs}')");
            }
            var storeAdditionalInfoParameters = new Dictionary<string, object?>();
            Type storeAdditionalInfoType = _serviceProvider
                .GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> storeAdditionalInfoList =
                await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>(
                    storeAdditionalInfoSql.ToString(), parameters, storeAdditionalInfoType);

            var storeCreditSql = new StringBuilder(
                @"SELECT SC.id AS Id, SC.uid AS UID, SC.created_by AS CreatedBy, SC.created_time AS CreatedTime, 
                        SC.modified_by AS ModifiedBy, SC.modified_time AS ModifiedTime, SC.server_add_time AS ServerAddTime, SC.server_modified_time AS ServerModifiedTime, 
                        SC.store_uid AS StoreUID, SC.payment_term_uid AS PaymentTermUID, SC.credit_type AS CreditType, SC.credit_limit AS CreditLimit, 
                        SC.temporary_credit AS TemporaryCredit, SC.org_uid AS OrgUID, SC.distribution_channel_uid AS DistributionChannelUID, 
                        SC.preferred_payment_mode AS PreferredPaymentMode, SC.is_active AS IsActive, SC.is_blocked AS IsBlocked, SC.blocking_reason_code AS BlockingReasonCode, 
                        SC.blocking_reason_description AS BlockingReasonDescription, O.code || '-' || DC.code AS DCLabel
                        FROM store_credit SC
                        INNER JOIN org O ON O.[uid] = SC.org_uid 
                        INNER JOIN org DC ON DC.[uid] = SC.distribution_channel_uid
                        ");
            var storeCreditParameters = new Dictionary<string, object?>();
            Type storeCreditType = _serviceProvider
                .GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreCredit>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditList =
                await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(storeCreditSql.ToString(),
                    parameters, storeCreditType);


            var storeAttributesSql = new StringBuilder(
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                                                            modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                            company_uid AS CompanyUID, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                                                            name AS Name, code AS Code, value AS Value, parent_name AS ParentName FROM store_attributes");

            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                storeAttributesSql.Append($" WHERE store_uid IN ('{commaSeparatedstoreUIDs}')");
                //   storeAttributesSql.Append(@" WHERE StoreUID IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));");
            }
            var storeAttributesParameters = new Dictionary<string, object?>();
            Type storeAttributesType = _serviceProvider
                .GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList =
                await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>(
                    storeAttributesSql.ToString(), parameters, storeAttributesType);

            var addressSql = new StringBuilder(
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, type AS Type, 
                                name AS Name, line1 AS Line1, line2 AS Line2, line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea, 
                                zip_code AS ZipCode, city AS City, country_code AS CountryCode, region_code AS RegionCode, phone AS Phone, 
                                phone_extension AS PhoneExtension, mobile1 AS Mobile1, mobile2 AS Mobile2, email AS Email, fax AS Fax, latitude AS Latitude, 
                                longitude AS Longitude, altitude AS Altitude, linked_item_uid AS LinkedItemUID, linked_item_type AS LinkedItemType, 
                                status AS Status, state_code AS StateCode, territory_code AS TerritoryCode, pan AS PAN, aadhar AS AADHAR, ssn AS SSN, 
                                is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info, depot AS Depot FROM address");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                addressSql.Append($" WHERE linked_item_uid IN ('{commaSeparatedstoreUIDs}')");
            }

            var addressParameters = new Dictionary<string, object?>();
            Type addressType = _serviceProvider.GetRequiredService<Winit.Modules.Address.Model.Interfaces.IAddress>()
                .GetType();
            List<Winit.Modules.Address.Model.Interfaces.IAddress> addressList =
                await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(addressSql.ToString(),
                    parameters, addressType);

            var contactSql = new StringBuilder(
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                                        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, title AS Title, 
                                        name AS Name, phone AS Phone, phone_extension AS PhoneExtension, description AS Description, designation AS Designation, 
                                        mobile AS Mobile, email AS Email, email2 AS Email2, email3 AS Email3, invoice_for_email1 AS InvoiceForEmail1, 
                                        invoice_for_email2 AS InvoiceForEmail2, invoice_for_email3 AS InvoiceForEmail3, fax AS Fax, linked_item_uid AS LinkedItemUID, 
                                        linked_item_type AS LinkedItemType, is_default AS IsDefault, is_editable AS IsEditable, enabled_for_invoice_email AS EnabledForInvoiceEmail, 
                                        enabled_for_docket_email AS EnabledForDocketEmail, enabled_for_promo_email AS EnabledForPromoEmail, is_email_cc AS IsEmailCC, 
                                        mobile2 AS Mobile2 FROM contact");

            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                contactSql.Append($" WHERE linked_item_uid IN ('{commaSeparatedstoreUIDs}')");
            }
            var contactParameters = new Dictionary<string, object?>();
            Type contactType = _serviceProvider.GetRequiredService<Winit.Modules.Contact.Model.Interfaces.IContact>()
                .GetType();
            List<Winit.Modules.Contact.Model.Interfaces.IContact> contactList =
                await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(contactSql.ToString(),
                    parameters, contactType);

            return (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList, contactList);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PagedResponse<SelectionItem>> GetAllStoreAsSelectionItems(List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "OrgUID", OrgUID }
            };
            var sql = new StringBuilder(
                @"SELECT uid AS UID, number AS Code, name AS Label  FROM store Where franchisee_org_uid=@OrgUID");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store");
            }

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

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

            //if (pageNumber > 0 && pageSize > 0)
            //{
            //    if (sortCriterias != null && sortCriterias.Count > 0)
            //    {
            //        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            //    }
            //    else
            //    {
            //        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            //    }
            //}

            //Data
            Type type = typeof(SelectionItem);
            IEnumerable<SelectionItem> selectionItemsStores =
                await ExecuteQueryAsync<SelectionItem>(sql.ToString(), parameters, type);
            //Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<SelectionItem> pagedResponse = new PagedResponse<SelectionItem>
            {
                PagedData = selectionItemsStores,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Model.Interfaces.IStore> SelectStoreByOrgUID(string FranchiseeOrgUID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "OrgUID", FranchiseeOrgUID }
        };

        var sql =
            @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                    company_uid AS CompanyUID, code AS Code, number AS Number, name AS Name, alias_name AS AliasName, legal_name AS LegalName, 
                    type AS Type, bill_to_store_uid AS BillToStoreUID, ship_to_store_uid AS ShipToStoreUID, sold_to_store_uid AS SoldToStoreUID, 
                    status AS Status, is_active AS IsActive, store_class AS StoreClass, store_rating AS StoreRating, is_blocked AS IsBlocked, 
                    blocked_reason_code AS BlockedReasonCode, blocked_reason_description AS BlockedReasonDescription, 
                    created_by_emp_uid AS CreatedByEmpUID, created_by_job_position_uid AS CreatedByJobPositionUID, country_uid AS CountryUID, 
                    region_uid AS RegionUID, city_uid AS CityUID, source AS Source, outlet_name AS OutletName, blocked_by_emp_uid AS BlockedByEmpUID, 
                    arabic_name AS ArabicName, is_tax_applicable AS IsTaxApplicable, tax_doc_number AS TaxDocNumber, school_warehouse AS SchoolWarehouse, 
                    day_type AS DayType, special_day AS SpecialDay, is_tax_doc_verified AS IsTaxDocVerified, store_size AS StoreSize, 
                    prospect_emp_uid AS ProspectEmpUID, tax_key_field AS TaxKeyField, store_image AS StoreImage, is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, 
                    tax_type AS TaxType
                    FROM store WHERE franchisee_org_uid = @OrgUID";

        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStore>().GetType();
        Model.Interfaces.IStore StoreList = await ExecuteSingleAsync<Model.Interfaces.IStore>(sql, parameters, type);
        return StoreList;
    }

    public async Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "routeUID", routeUID }
        };

        var sql = @"SELECT
                            S.uid AS UID,
                            S.number AS Number,
                            S.code AS Code,
                            S.name AS Name,
                            S.is_active AS IsActive,
                            S.is_blocked,
                            COALESCE(S.blocked_reason_description, '') AS BlockedReason,
                            A.line1 || ' ' || A.line2 AS Address,
                            A.[uid] AS AddressUID,
                            A.latitude,
                            A.longitude
                        FROM
                            route_customer RC
                            INNER JOIN store S ON S.uid = RC.store_uid AND RC.route_uid = @routeUID
                            LEFT JOIN address A ON A.linked_item_uid = S.uid AND A.is_default = 1
                        ORDER BY
                            RC.seq_no;";

        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreItemView>().GetType();
        List<Model.Interfaces.IStoreItemView> CustomerList =
            await ExecuteQueryAsync<Model.Interfaces.IStoreItemView>(sql, parameters, type);
        return CustomerList;
    }
    
    public async Task<List<IStoreItemView>> GetStoreByRouteUIDWithoutAddress(string routeUID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "routeUID", routeUID }
        };

        var sql = @"SELECT
                            S.uid AS UID,
                            S.number AS Number,
                            S.code AS Code,
                            S.name AS Name,
                            S.is_active AS IsActive,
                            S.is_blocked,
                            COALESCE(S.blocked_reason_description, '') AS BlockedReason
                        FROM
                            route_customer RC
                            INNER JOIN store S ON S.uid = RC.store_uid AND RC.route_uid = @routeUID
                        ORDER BY
                            RC.seq_no;";

        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreItemView>().GetType();
        List<Model.Interfaces.IStoreItemView> CustomerList =
            await ExecuteQueryAsync<Model.Interfaces.IStoreItemView>(sql, parameters, type);
        return CustomerList;
    }

    public async Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID, string BeatHistoryUID,
        bool notInJP = false)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "RouteUID", routeUID },
            { "BeatHistoryUID", BeatHistoryUID },
        };


        try
        {
            string storeHistoryQuery = string.Empty;
            if (notInJP)
            {
                storeHistoryQuery = @"AND SH.Id IS NULL ";
            }
            var sql = $@"SELECT DISTINCT 
                    CASE WHEN IFNULL(SH.uid,'') != '' THEN 1 ELSE 0 END AS IsAddedInJP, C.name AS ContactPerson, C.phone AS ContactNumber,
					SH.planned_login_time AS DeliveryTime, S.store_rating AS StoreRating, S.store_class AS StoreClass, S.type AS Type, 
                    S.number AS Number, S.[uid] AS StoreUID, S.code AS Code, S.name AS Name, A.latitude AS Latitude, S.price_type AS PriceType, 
                    A.longitude AS Longitude,  IFNULL(A.line1, '') || CASE WHEN LENGTH(IFNULL(A.line1, '')) > 0 AND LENGTH(IFNULL(A.line2, '')) > 0 THEN ', ' ELSE '' END || 
                    IFNULL(A.line2, '') AS Address,A.uid AS AddressUID, 0 AS StoreDistanceInKM, SH.serial_no, SH.[uid] AS StoreHistoryUID, SH.is_planned AS IsPlanned, 
                    SH.no_of_visits AS NoOfVisits, SH.is_productive AS IsProductive, SH.is_green AS IsGreen, SH.target_value AS TargetValue, 
                    SH.target_volume AS TargetVolume, SH.target_lines AS TargetLines, SH.actual_value AS ActualValue, SH.actual_volume AS ActualVolume, 
                    SH.actual_lines AS ActualLines, SH.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes, IFNULL(S.is_blocked,0) AS IsBlocked,  
                    IFNULL(S.blocked_reason_description,'') AS BlockedReasonDescription, 
                    '' as TIN, CASE WHEN S.[type] = 'DCom' THEN S.type ELSE SAI.order_type END AS OrderType, 
                    CASE WHEN AP.storeuid IS NOT NULL THEN 1 ELSE 0 END AS IsAwayPeriod, IFNULL(SAI.is_temperature_check, 0) AS IsTemperatureCheck, 
                    IFNULL(SAI.is_always_printed, 0) AS PrintEmailOption, IFNULL(SAI.delivery_docket_is_purchase_order_required, 0) AS DeliveryDocketIsPurchaseOrderRequired, 
                    SH.notes AS Notes, SAI.drawer AS Drawer, SAI.is_stop_delivery AS IsStopDelivery, SAI.is_with_printed_invoices AS IsWithPrintedInvoices,
					B.uid AS UID, B.bank_name AS BankName, SAI.purchase_order_number AS PurchaseOrderNumber, SAI.delivery_information AS  AdditionalNotes, 
                    SAI.is_promotions_block AS IsPromotionsBlock, SAI.building_delivery_code AS BuildingDeliveryCode, SAI.is_dummy_customer AS IsDummyCustomer, 
                    SAI.mandatory_po_number AS MandatoryPONumber, SAI.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired, 
                    SAI.store_credit_always_printed AS StoreCreditAlwaysPrinted, SAI.is_capture_signature_required AS IsCaptureSignatureRequired, 
                    S.sold_to_store_uid AS SoldToStoreUID, S.bill_to_store_uid AS BillToStoreUID, IFNULL(SAI.stock_credit_is_purchase_order_required, 0) AS StockCreditIsPurchaseOrderRequired 
                    FROM route_customer RS 
                    INNER JOIN route R ON R.[uid] = RS.route_uid 
                    AND R.[uid] = @RouteUID             
                    INNER JOIN store S ON S.[uid]=RS.store_uid AND S.is_active = 1 
                    INNER JOIN store_additional_info SAI ON SAI.store_uid = S.[uid] 
                    AND IFNULL(SAI.is_foc_customer, 0) = 0 
					LEFT JOIN bank B ON B.uid=SAI.bank_uid
                    LEFT JOIN store_history SH ON SH.store_uid = S.uid 
                    AND SH.beat_history_uid = @BeatHistoryUID 
                    LEFT JOIN [address] A ON A.linked_item_type = 'Store'  AND A.type = 'Billing'
                    AND A.linked_item_uid = S.[uid] AND IFNULL(A.is_default, 0) = 1 
                    LEFT JOIN contact C on C.linked_item_type = 'Store' 
                    AND C.linked_item_uid = S.[uid] AND IFNULL(C.is_default, 0) = 1 
                    LEFT JOIN (
                        SELECT DISTINCT linked_item_uid AS StoreUID FROM away_period AP 
                        WHERE AP.linked_item_type = 'Store' AND IFNULL(AP.is_active, 0) = 1 
                        AND AP.from_date IS NOT NULL AND AP.to_date IS NOT NULL 
                        AND date('now') BETWEEN Date(from_date) AND Date(to_date)
                    ) AP ON AP.StoreUId = S.[uid]  
                    WHERE 1=1 {storeHistoryQuery}
                    ORDER BY IFNULL(RS.seq_no, 9999), S.name";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreItemView>().GetType();
            List<Model.Interfaces.IStoreItemView> CustomerList =
                await ExecuteQueryAsync<Model.Interfaces.IStoreItemView>(sql, parameters, type);
            return CustomerList;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<IStoreCustomer>> GetStoreCustomersByRouteUID(string routeUID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "routeUID", routeUID }
        };

        var sql = @"SELECT
                        S.uid AS UID,
                        S.number AS Number,
                        S.code AS Code,
                        S.name AS Name,
                        S.is_active AS IsActive,
                        S.is_blocked AS IsBlocked,
                        COALESCE(S.blocked_reason_description, '') AS BlockedReason,
                        A.line1 || ' ' || A.line2 AS Address,
                        A.latitude AS Latitude,
                        A.longitude AS Longitude
                    FROM
                        route_customer RC
                        INNER JOIN store S ON S.uid = RC.store_uid AND RC.route_uid = @routeUID
                        LEFT JOIN address A ON A.linked_item_uid = S.uid AND A.is_default = 1
                    ORDER BY
                        RC.seq_no;";

        Type type = _serviceProvider.GetRequiredService<IStoreCustomer>().GetType();
        List<IStoreCustomer> CustomerList = await ExecuteQueryAsync<IStoreCustomer>(sql, parameters, type);
        return CustomerList;
    }

    public Task<PagedResponse<IStoreCustomer>> GetAllStoreItems(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string orgUID)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CUDOnBoardCustomerInfo(IOnBoardCustomerDTO onBoardCustomerDTO)
    {
        throw (new NotImplementedException());
    }

    public async Task<int> CreateAllApprovalRequest(
        IAllApprovalRequest allApprovalRequest)
    {
        throw (new NotImplementedException());
    }

    public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>> SelectAllOnBoardCustomer(
        List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Role)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateStoreStatus(StoreApprovalDTO storeApprovalDTO)
    {
        throw (new NotImplementedException());
    }

    Task<List<IAllApprovalRequest>> IStoreDL.GetApprovalDetailsByStoreUID(string LinkItemUID)
    {
        throw new NotImplementedException();
    }


    Task<OnBoardEditCustomerDTO> IStoreDL.GetAllOnBoardingDetailsByStoreUID(string UID)
    {
        throw new NotImplementedException();
    }

    Task<int> IStoreDL.DeleteOnBoardingDetails(string UID)
    {
        throw new NotImplementedException();
    }

    public async Task<List<IStore>> GetChannelPartner(string jobPositionUid)
    {
        var sql = @"select s.* from store s
                 inner join Org o on s.uid=o.uid where s.type='FR'";
        IEnumerable<IStore> standingProvisionBranches = await ExecuteQueryAsync<IStore>(sql.ToString());
        return standingProvisionBranches.ToList();
    }

    Task<List<IAllApprovalRequest>> IStoreDL.GetApprovalStatusByStoreUID(string LinkItemUID)
    {
        throw new NotImplementedException();
    }


    Task<List<IAsmDivisionMapping>> IStoreDL.GetAsmDivisionMappingByUID(string LinkedItemType,
        string LinkedItemUID, string? asmEmpUID = null)
    {
        throw new NotImplementedException();
    }

    Task<int> IStoreDL.CreateChangeRequest(Winit.Modules.Store.Model.Classes.ChangeRequestDTO changeRequestDTO)
    {
        throw new NotImplementedException();
    }
    public Task<IEnumerable<string>> GetDivisionsByAsmEmpUID(string asmEmpUID) => throw new NotImplementedException();

    Task<int> IStoreDL.UpdateAsmDivisionMapping(List<IAsmDivisionMapping> Store)
    {
        throw new NotImplementedException();
    }

    Task<int> IStoreDL.DeleteAsmDivisionMapping(string UID)
    {
        throw new NotImplementedException();
    }

    Task<int> IStoreDL.CreateAsmDivisionMapping(IAsmDivisionMapping asmDivisionMapping)
    {
        throw new NotImplementedException();
    }

    Task<IAsmDivisionMapping> IStoreDL.CheckAsmDivisionMappingRecordExistsOrNot(string UID)
    {
        throw new NotImplementedException();
    }

    Task<PagedResponse<IOnBoardGridview>> IStoreDL.SelectAllOnBoardCustomer(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string Role)
    {
        throw new NotImplementedException();
    }

    Task IStoreDL.GenerateMyTeam(string jobPositionUid)
    {
        throw new NotImplementedException();
    }

    Task<bool> IStoreDL.IsGstUnique(string GstNumber)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<string, int>> IStoreDL.GetTabsCount(List<FilterCriteria> filterCriterias, string JobPositionUid, string Role)
    {
        throw new NotImplementedException();
    }
}
