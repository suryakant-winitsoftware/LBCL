using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class PGSQLDataPreparationDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IDataPreparationDL
    {
        public PGSQLDataPreparationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }



        public async Task<(List<Model.Interfaces.ISKU>, List<Model.Interfaces.ISKUConfig>, List<Model.Interfaces.ISKUUOM>, List<Model.Interfaces.ISKUAttributes>, List<Model.Interfaces.ITaxSkuMap>)>
            PrepareSKUMaster(List<String> orgUIDs, List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes)
        {
            try
            {
                string commaSeparatedorgUIDs = null;  
                string commaSeparatedskuUIDs = null;
                string commaSeparatedDistributionChannelUIDs = null;
                string commaSeparatedattributeTypes = null;
                if (orgUIDs != null)
                {
                    commaSeparatedorgUIDs = string.Join(",", orgUIDs);
                }
                if (skuUIDs != null)
                {
                    commaSeparatedskuUIDs = string.Join(",", skuUIDs);
                }
                if (DistributionChannelUIDs != null)
                {
                    commaSeparatedDistributionChannelUIDs = string.Join(",", DistributionChannelUIDs);
                }
                if (attributeTypes != null)
                {
                    commaSeparatedattributeTypes = string.Join(",", attributeTypes);
                }

                Dictionary<string, object> skuParameters = new Dictionary<string, object>
                {
                    { "ORGUIDs", commaSeparatedorgUIDs },
                };

                var skuSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                                company_uid AS CompanyUID,org_uid AS OrgUID,code AS Code,name AS Name,arabic_name AS ArabicName,
                                                alias_name AS AliasName,long_name AS LongName,base_uom AS BaseUOM,outer_uom AS OuterUOM,
                                                from_date AS FromDate,to_date AS ToDate,is_stockable AS IsStockable,parent_uid AS ParentUID,
                                                is_active AS IsActive,is_third_party AS IsThirdParty,supplier_org_uid AS SupplierOrgUID FROM sku WHERE is_active = true ");
                if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
                {
                    skuSql.Append(@" AND (  org_uid = ANY(string_to_array(@ORGUIDs, ','))); ");
                }
                Type skuType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKU>().GetType();
                List<Model.Interfaces.ISKU> skuList = await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), skuParameters, skuType);

                var skuConfigParameters = new Dictionary<string, object>
                {
                    { "ORGUIDs", commaSeparatedorgUIDs },
                    { "SKUUIDS", commaSeparatedskuUIDs },
                    { "DistributionChannelOrgUIDs", commaSeparatedDistributionChannelUIDs },
                };
                var skuConfig = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                                org_uid AS OrgUID,distribution_channel_org_uid AS DistributionChannelOrgUID,sku_uid AS SKUUID,
                                                can_buy AS CanBuy,can_sell AS CanSell,buying_uom AS BuyingUOM,selling_uom AS SellingUOM,
                                                is_active AS IsActive FROM sku_config WHERE is_active = true");

                if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
                {
                    skuConfig.Append(@" AND  (org_uid = ANY(string_to_array(@ORGUIDs, ','))) ");
                }
                if (!string.IsNullOrEmpty(commaSeparatedDistributionChannelUIDs))
                {
                    skuConfig.Append(@" AND  (distribution_channel_org_uid = ANY(string_to_array(@DistributionChannelOrgUIDs, ','))) ");
                }
                if (!string.IsNullOrEmpty(commaSeparatedskuUIDs))
                {
                    skuConfig.Append(@" AND  (sku_uid = ANY(string_to_array(@SKUUIDS, ','))); ");
                }
                Type skuConfigType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUConfig>().GetType();
                List<Model.Interfaces.ISKUConfig> skuConfigList = await ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfig.ToString(), skuConfigParameters, skuConfigType);

                var skuUOMParameters = new Dictionary<string, object>
                {
                    { "SKUUIDS", commaSeparatedskuUIDs },
                };
                var skuUomSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                                                    server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,sku_uid AS SKUUID,
                                                    code AS Code,name AS Name,label AS Label,barcodes AS Barcodes,is_base_uom AS IsBaseUOM,is_outer_uom AS IsOuterUOM,
                                                    multiplier AS Multiplier,length AS Length,depth AS Depth,width AS Width,height AS Height,
                                                    volume AS Volume,weight AS Weight,gross_weight AS GrossWeight, dimension_unit AS DimensionUnit,
                                                    volume_unit AS VolumeUnit,weight_unit AS WeightUnit,gross_weight_unit AS GrossWeightUnit,
                                                    liter AS Liter,kgm AS KGM FROM sku_uom");
                if (!string.IsNullOrEmpty(commaSeparatedskuUIDs))
                {
                    skuUomSql.Append(@" WHERE ( sku_uid = ANY(string_to_array(@SKUUIDS, ','))); ");
                }
               
                Type skuUomtype = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUUOM>().GetType();
                List<Model.Interfaces.ISKUUOM> skuUomList = await ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), skuUOMParameters, skuUomtype);


                var skuAttributesparameters = new Dictionary<string, object>
                {
                     { "SKUUIDS", commaSeparatedskuUIDs },
                     { "Types", commaSeparatedattributeTypes },
                };
                var skuAtrributesSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                                sku_uid AS SKUUID,type AS Type,code AS Code,value AS Value,parent_type AS ParentType FROM sku_attributes");

               
                Type skuAttributestype = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUAttributes>().GetType();

                if (!string.IsNullOrEmpty(commaSeparatedskuUIDs))
                {
                    skuAtrributesSql.Append(@" WHERE ( sku_uid = ANY(string_to_array(@SKUUIDS, ','))) ");
                }

                if (!string.IsNullOrEmpty(commaSeparatedattributeTypes))
                {
                    skuAtrributesSql.Append(@" AND ( type = ANY(string_to_array(@Types, ','))) ");
                }
                List<Model.Interfaces.ISKUAttributes> sKUAttributesList = await ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAtrributesSql.ToString(), skuAttributesparameters, skuAttributestype);


                var taxSkuMapparameters = new Dictionary<string, object>
                {
                     { "SKUUIDS", commaSeparatedskuUIDs },
                };
                var taxSkuMapSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                                    modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                                    company_uid AS CompanyUID,sku_uid AS SKUUID,tax_uid AS TaxUID FROM tax_sku_map");
                if (!string.IsNullOrEmpty(commaSeparatedskuUIDs))
                {
                    taxSkuMapSql.Append(@" WHERE  (sku_uid = ANY(string_to_array(@SKUUIDS, ','))); ");
                }

                Type taxSkuMaptype = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxSkuMap>().GetType();
                List<Model.Interfaces.ITaxSkuMap> taxSkuMapList = await ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(taxSkuMapSql.ToString(), taxSkuMapparameters, taxSkuMaptype);

                return (skuList, skuConfigList,skuUomList, sKUAttributesList, taxSkuMapList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>, List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>, List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>, List<Winit.Modules.Address.Model.Interfaces.IAddress>,
            List<Winit.Modules.Contact.Model.Interfaces.IContact>)> PrepareStoreMaster(List<string> storeUIDs)
        {
            try
            {
                string commaSeparatedstoreUIDs = null; // Initialize to null
                if (storeUIDs != null)
                {
                    commaSeparatedstoreUIDs = string.Join(",", storeUIDs);
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UIDs", commaSeparatedstoreUIDs }
                };
                var storeSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime, 
                                                company_uid AS CompanyUID, code AS Code, number AS Number, name AS Name, alias_name AS AliasName,
                                                legal_name AS LegalName, type AS Type, bill_to_store_uid AS BillToStoreUID, ship_to_store_uid AS ShipToStoreUID,
                                                sold_to_store_uid AS SoldToStoreUID, status AS Status, is_active AS IsActive, store_class AS StoreClass,
                                                store_rating AS StoreRating, is_blocked AS IsBlocked, blocked_reason_code AS BlockedReasonCode, 
                                                blocked_reason_description AS BlockedReasonDescription, created_by_emp_uid AS CreatedByEmpUID,
                                                created_by_job_position_uid AS CreatedByJobPositionUID, country_uid AS CountryUID, 
                                                region_uid AS RegionUID, city_uid AS CityUID, source AS Source, outlet_name AS OutletName, 
                                                blocked_by_emp_uid AS BlockedByEmpUID, arabic_name AS ArabicName, is_tax_applicable AS IsTaxApplicable,
                                                tax_doc_number AS TaxDocNumber, school_warehouse AS SchoolWarehouse, day_type AS DayType,
                                                special_day AS SpecialDay, is_tax_doc_verified AS IsTaxDocVerified,store_size AS StoreSize, 
                                                prospect_emp_uid AS ProspectEmpUID, tax_key_field AS TaxKeyField, store_image AS StoreImage, 
                                                is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, tax_type AS TaxType FROM store");
                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    storeSql.Append(@" WHERE uid = ANY(string_to_array(@UIDs, ','))");
                }

                var storeParameters = new Dictionary<string, object>();
                Type storeType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStore>().GetType();
                List<Winit.Modules.Store.Model.Interfaces.IStore> storeList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStore>(storeSql.ToString(), parameters, storeType);

                var storeAdditionalInfoSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                                    server_modified_time AS ServerModifiedTime,store_uid AS StoreUID, order_type AS OrderType, 
                                                    is_promotions_block AS IsPromotionsBlock, customer_start_date AS CustomerStartDate, customer_end_date AS CustomerEndDate, 
                                                    purchase_order_number AS PurchaseOrderNumber, delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired, 
                                                    is_with_printed_invoices AS IsWithPrintedInvoices, is_capture_signature_required AS IsCaptureSignatureRequired, 
                                                    is_always_printed AS IsAlwaysPrinted, building_delivery_code AS BuildingDeliveryCode, delivery_information AS DeliveryInformation,
                                                    is_stop_delivery AS IsStopDelivery, is_forecast_top_up_qty AS IsForeCastTopUpQty, is_temperature_check AS IsTemperatureCheck, 
                                                    invoice_start_date AS InvoiceStartDate, invoice_end_date AS InvoiceEndDate, invoice_format AS InvoiceFormat, 
                                                    invoice_delivery_method AS InvoiceDeliveryMethod, display_delivery_docket AS DisplayDeliveryDocket, display_price AS DisplayPrice, 
                                                    show_cust_po AS ShowCustPO, invoice_text AS InvoiceText, invoice_frequency AS InvoiceFrequency, stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired,
                                                    admin_fee_per_billing_cycle AS AdminFeePerBillingCycle, admin_fee_per_delivery AS AdminFeePerDelivery,
                                                    late_payment_fee AS LatePayementFee, drawer AS Drawer, bank_uid AS BankUID, bank_account AS BankAccount,
                                                    mandatory_po_number AS MandatoryPONumber, is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired,
                                                    store_credit_always_printed AS StoreCreditAlwaysPrinted, is_dummy_customer AS IsDummyCustomer, default_run AS DefaultRun,
                                                    is_foc_customer AS IsFOCCustomer, rss_show_price AS RSSShowPrice, rss_show_payment AS RSSShowPayment, rss_show_credit AS RSSShowCredit,
                                                    rss_show_invoice AS RSSShowInvoice, rss_is_active AS RSSIsActive, rss_delivery_instruction_status AS RSSDeliveryInstructionStatus, 
                                                    rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal, rss_order_placed_in_rss AS RSSOrderPlacedInRSS, rss_avg_orders_per_week AS RSSAvgOrdersPerWeek,
                                                    rss_total_order_value AS RSSTotalOrderValue, allow_force_check_in AS AllowForceCheckIn, is_manual_edit_allowed AS IsManaualEditAllowed,
                                                    can_update_lat_long AS CanUpdateLatLong, allow_good_return AS AllowGoodReturn, allow_bad_return AS AllowBadReturn, 
                                                    allow_replacement AS AllowReplacement, is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed,
                                                    is_delivery_note_required AS IsDeliveryNoteRequired, e_invoicing_enabled AS EInvoicingEnabled, image_recognition_enabled AS ImageRecognitionEnabled,
                                                    max_outstanding_invoices AS MaxOutstandingInvoices, negative_invoice_allowed AS NegativeInvoiceAllowed, delivery_mode AS DeliveryMode, 
                                                    visit_frequency AS VisitFrequency, shipping_contact_same_as_store AS ShippingContactSameAsStore, billing_address_same_as_shipping AS BillingAddressSameAsShipping, 
                                                    payment_mode AS PaymentMode, price_type AS PriceType, average_monthly_income AS AverageMonthlyIncome, default_bank_uid AS DefaultBankUID,
                                                    account_number AS AccountNumber, no_of_cash_counters AS NoOfCashCounters, custom_field1 AS CustomField1, custom_field2 AS CustomField2,
                                                    custom_field3 AS CustomField3, custom_field4 AS CustomField4, custom_field5 AS CustomField5, custom_field6 AS CustomField6, custom_field7 AS CustomField7,
                                                    custom_field8 AS CustomField8, custom_field9 AS CustomField9, custom_field10 AS CustomField10, is_asset_enabled AS IsAssetEnabled, is_survey_enabled AS IsSurveyEnabled,
                                                    allow_return_against_invoice AS AllowReturnAgainstInvoice,
                                                    allow_return_with_sales_order AS AllowReturnWithSalesOrder, week_off_sun AS WeekOffSun, week_off_mon AS WeekOffMon, week_off_tue AS WeekOffTue,
                                                    week_off_wed AS WeekOffWed, week_off_thu AS WeekOffThu, week_off_fri AS WeekOffFri, week_off_sat AS WeekOffSat FROM store_additional_info");
                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    storeAdditionalInfoSql.Append(@" WHERE store_uid = ANY(string_to_array(@UIDs, ','))");
                }
                var storeAdditionalInfoParameters = new Dictionary<string, object>();
                Type storeAdditionalInfoType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>().GetType();
                List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> storeAdditionalInfoList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>(storeAdditionalInfoSql.ToString(), parameters, storeAdditionalInfoType);

                var storeCreditSql = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,store_uid AS StoreUID,payment_term_uid AS PaymentTermUID,
                                                        credit_type AS CreditType,credit_limit AS CreditLimit,temporary_credit AS TemporaryCredit,org_uid AS OrgUID,
                                                        distribution_channel_uid AS DistributionChannelUID,preferred_payment_mode AS PreferredPaymentMode,is_active AS IsActive,
                                                        is_blocked AS IsBlocked,blocking_reason_code AS BlockingReasonCode,blocking_reason_description FROM store_credit");
                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    storeCreditSql.Append(@" WHERE store_uid = ANY(string_to_array(@UIDs, ','))");
                }
                var storeCreditParameters = new Dictionary<string, object>();
                Type storeCreditType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreCredit>().GetType();
                List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(storeCreditSql.ToString(), parameters, storeCreditType);


                var storeAttributesSql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUid,
                    org_uid AS OrgUid,
                    distribution_channel_uid AS DistributionChannelUid,
                    store_uid AS StoreUid,
                    name AS Name,
                    code AS Code,
                    value AS Value,
                    parent_name AS ParentName
                FROM 
                    store_attributes
                ");

                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    storeAttributesSql.Append(@" WHERE store_uid = ANY(string_to_array(@UIDs, ','))");
                }
                var storeAttributesParameters = new Dictionary<string, object>();
                Type storeAttributesType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>().GetType();
                List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>(storeAttributesSql.ToString(), parameters, storeAttributesType);

                var addressSql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    type AS Type,
                    name AS Name,
                    line1 AS Line1,
                    line2 AS Line2,
                    line3 AS Line3,
                    landmark AS Landmark,
                    area AS Area,
                    sub_area AS SubArea,
                    zip_code AS ZipCode,
                    city AS City,
                    country_code AS CountryCode,
                    region_code AS RegionCode,
                    phone AS Phone,
                    phone_extension AS PhoneExtension,
                    mobile1 AS Mobile1,
                    mobile2 AS Mobile2,
                    email AS Email,
                    fax AS Fax,
                    latitude AS Latitude,
                    longitude AS Longitude,
                    altitude AS Altitude,
                    linked_item_uid AS LinkedItemUID,
                    linked_item_type AS LinkedItemType,
                    status AS Status,
                    state_code AS StateCode,
                    territory_code AS TerritoryCode,
                    pan AS PAN,
                    aadhar AS AADHAR,
                    ssn AS SSN,
                    is_editable AS IsEditable,
                    is_default AS IsDefault,
                    line4 AS Line4,
                    info AS Info,
                    depot AS Depot
                FROM 
                    address");
                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    addressSql.Append(@" WHERE linked_item_uid"" = ANY(string_to_array(@UIDs, ','))");
                }

                var addressParameters = new Dictionary<string, object>();
                Type addressType = _serviceProvider.GetRequiredService<Winit.Modules.Address.Model.Interfaces.IAddress>().GetType();
                List<Winit.Modules.Address.Model.Interfaces.IAddress> addressList = await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(addressSql.ToString(), parameters, addressType);

                var contactSql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    title AS Title,
                    name AS Name,
                    phone AS Phone,
                    phone_extension AS PhoneExtension,
                    description AS Description,
                    designation AS Designation,
                    mobile AS Mobile,
                    email AS Email,
                    email2 AS Email2,
                    email3 AS Email3,
                    invoice_for_email1 AS InvoiceForEmail1,
                    invoice_for_email2 AS InvoiceForEmail2,
                    invoice_for_email3 AS InvoiceForEmail3,
                    fax AS Fax,
                    linked_item_uid AS LinkedItemUID,
                    linked_item_type AS LinkedItemType,
                    is_default AS IsDefault,
                    is_editable AS IsEditable,
                    enabled_for_invoice_email AS EnabledForInvoiceEmail,
                    enabled_for_docket_email AS EnabledForDocketEmail,
                    enabled_for_promo_email AS EnabledForPromoEmail,
                    is_email_cc AS IsEmailCC,
                    mobile2 AS Mobile2
                FROM 
                    contact");

                if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
                {
                    contactSql.Append(@" WHERE linked_item_uid"" = ANY(string_to_array(@UIDs, ','))");
                }
                var contactParameters = new Dictionary<string, object>();
                Type contactType = _serviceProvider.GetRequiredService<Winit.Modules.Contact.Model.Interfaces.IContact>().GetType();
                List<Winit.Modules.Contact.Model.Interfaces.IContact> contactList = await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(contactSql.ToString(), parameters, contactType);

                return (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList, contactList);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<IOrgCurrency>> PrepareOrgCurrencyMaster()
        {
            try
            {
                var orgCurrencySql = new StringBuilder(@"SELECT 
                    id AS Id,
                    org_uid AS OrgUID,
                    currency_uid AS CurrencyUID,
                    is_primary AS IsPrimary,
                    ss AS SS,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    org_currency");
                var orgCurrencyParameters = new Dictionary<string, object>();
                Type orgCurrencyType = _serviceProvider.GetRequiredService<IOrgCurrency>().GetType();
                List<IOrgCurrency> orgCurrencyList = await ExecuteQueryAsync<IOrgCurrency>(orgCurrencySql.ToString(), orgCurrencyParameters, orgCurrencyType);
                return (orgCurrencyList);
            }
            catch (Exception)
            {
                throw;
            }
        }




    }
}
