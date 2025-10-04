using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Contact.BL.Interfaces;
using Winit.Modules.StoreDocument.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Distributor.DL.Interfaces;
using Microsoft.Extensions.Configuration;
using Winit.Shared.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Address.BL.Interfaces;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Shared.Models.Constants;
using System.Collections;
using System.Transactions;
using System.Data.Common;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Contact.Model.Classes;

namespace Winit.Modules.Distributor.DL.Classes
{
    public class MSSQLDIstributorDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IDistributorDL
    {
        IStoreBL _storeBL;
        IStoreAdditionalInfoBL _additionalInfoBL;
        IOrgBL _orgBL;
        IStoreCreditBL _creditBL;
        IContactBL _contactBL;
        IStoreDocumentBL _storeDocumentBL;
        IAddressBL _addressBL;
        public MSSQLDIstributorDL(IServiceProvider serviceProvider, IConfiguration config, IStoreBL storeBL, IStoreAdditionalInfoBL additionalInfoBL,
            IOrgBL orgBL, IStoreCreditBL creditBL, IContactBL contactBL, IStoreDocumentBL storeDocumentBL, IAddressBL addressBL) : base(serviceProvider, config)
        {
            _storeBL = storeBL;
            _additionalInfoBL = additionalInfoBL;
            _orgBL = orgBL;
            _creditBL = creditBL;
            _contactBL = contactBL;
            _storeDocumentBL = storeDocumentBL;
            _addressBL = addressBL;
        }
        public async Task<PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor>> SelectAllDistributors(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT
                                                        o.uid AS UID,
                                                        o.code AS Code,
                                                        o.name AS Name,
                                                        o.seq_code AS SequenceCode,
                                                        c.name AS ContactPerson,
                                                        c.phone AS ContactNumber,
	                                                    o.status as Status,
			   											sa.customer_start_date as OpenAccountDate
                                                    FROM
                                                        org o
                                                    INNER JOIN
                                                        store s ON o.code = s.code
                                                    LEFT JOIN
                                                        store_additional_info sa ON sa.store_uid = s.uid
                                                    LEFT JOIN
                                                        contact c ON s.uid = c.linked_item_uid AND c.is_default = 1
                                                    WHERE o.org_type_uid = 'FR') as SubQuery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                     sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt from (SELECT
                                                            o.uid AS UID,
                                                        o.code AS Code,
                                                        o.name AS Name,
                                                        o.seq_code AS SequenceCode,
                                                        c.name AS ContactPerson,
                                                        c.phone AS ContactNumber,
	                                                    o.status as Status,
			   											sa.customer_start_date as OpenAccountDate
                                                        FROM
                                                            org o
                                                        INNER JOIN
                                                            store s ON o.code = s.code
                                                        LEFT JOIN
                                                            store_additional_info sa ON sa.store_uid = s.uid
                                                        LEFT JOIN
                                                            contact c ON s.uid = c.linked_item_uid AND c.is_default = 1
                                                        WHERE o.org_type_uid = 'FR') as SubQuery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IDistributor>(filterCriterias, sbFilterCriteria, parameters);;

                    sql.Append(sbFilterCriteria);
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
                        sql.Append($" ORDER BY UID OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
               IEnumerable<Winit.Modules.Distributor.Model.Interfaces.IDistributor> distributors = await ExecuteQueryAsync<Winit.Modules.Distributor.Model.Interfaces.IDistributor>(sql.ToString(), parameters);
                
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor> pagedResponse = new PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor>
                {
                    PagedData = distributors,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateDistributor(Winit.Modules.Distributor.Model.Classes.DistributorMasterView distributorMasterView)
        {
            int count = 0;
            //org
            string? isExists = await CheckIfUIDExistsInDB(DbTableName.Org, distributorMasterView.Org.UID);
            if (!string.IsNullOrEmpty(isExists))
            {
                count += await _orgBL.UpdateOrg(distributorMasterView.Org);
            }
            else
            {
                count += await _orgBL.CreateOrg(distributorMasterView.Org);
            }
            //Store
            string? storeUIDExists = await CheckIfUIDExistsInDB(DbTableName.Store, distributorMasterView.Store.UID);
            if (!string.IsNullOrEmpty(storeUIDExists))
            {
                count += await _storeBL.UpdateStore(distributorMasterView.Store);
            }
            else
            {
                count += await _storeBL.CreateStore(distributorMasterView.Store);
            }
            //Store Additional Info
            string? storeAdditionalInfoUIDExists = await CheckIfUIDExistsInDB(DbTableName.StoreAdditionalInfo, distributorMasterView.StoreAdditionalInfo.UID);
            if (!string.IsNullOrEmpty(storeAdditionalInfoUIDExists))
            {
                count += await _additionalInfoBL.CreateStoreAdditionalInfo(distributorMasterView.StoreAdditionalInfo);
            }
            else
            {
                count += await _additionalInfoBL.CreateStoreAdditionalInfo(distributorMasterView.StoreAdditionalInfo);
            }
            //Store Credit
            string? storecreditUIDExists = await CheckIfUIDExistsInDB(DbTableName.StoreCredit, distributorMasterView.StoreCredit.UID);
            if (!string.IsNullOrEmpty(storecreditUIDExists))
            {
                count += await _creditBL.UpdateStoreCredit(distributorMasterView.StoreCredit);
            }
            else
            {
                count += await _creditBL.CreateStoreCredit(distributorMasterView.StoreCredit);
            }
            //Address
            string? addressUIDExists = await CheckIfUIDExistsInDB(DbTableName.Address, distributorMasterView.Address.UID);
            if (!string.IsNullOrEmpty(addressUIDExists))
            {
                count += await _addressBL.UpdateAddressDetails(distributorMasterView.Address);
            }
            else
            {
                count += await _addressBL.CreateAddressDetails(distributorMasterView.Address);
            }
            List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.Contact, distributorMasterView.Contacts.Select(c=>c.UID).ToList());

            List<Contact.Model.Classes.Contact>? newContacts = null;
            List<Contact.Model.Classes.Contact>? existingContacts = null;
            if (existingUIDs != null && existingUIDs.Count > 0)
            {
                newContacts = distributorMasterView.Contacts.Where(c => !existingUIDs.Contains(c.UID)).ToList();
                existingContacts = distributorMasterView.Contacts.Where(c => existingUIDs.Contains(c.UID)).ToList();
            }
            else
            {
                newContacts = distributorMasterView.Contacts;
            }
            if (existingContacts != null && existingContacts.Any())
            {
                count += await UpdateContacts(existingContacts);
            }
            if (newContacts!=null && newContacts.Any())
            {
                await CreateContacts(newContacts);
            }
            List<string>? existingStoreDocumentUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreDocument, distributorMasterView.Documents.Select(sd => sd.UID).ToList());
            List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument>? newStoreDocuments = null;
            List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument>? existingStoreDocuments = null;
            if (existingStoreDocumentUIDs != null && existingStoreDocumentUIDs.Count > 0)
            {
                newStoreDocuments = distributorMasterView.Documents.Where(sd => !existingStoreDocumentUIDs.Contains(sd.UID)).ToList();
                existingStoreDocuments = distributorMasterView.Documents.Where(sd => existingStoreDocumentUIDs.Contains(sd.UID)).ToList();
            }
            else
            {
                newStoreDocuments = distributorMasterView.Documents;
            }
            if (existingStoreDocuments != null && existingStoreDocuments.Any())
            {
                count += await UpdateStoreDocuments(existingStoreDocuments);
            }
            if (newStoreDocuments!=null && newStoreDocuments.Any())
            {
                await CreateStoreDocuments(newStoreDocuments);
            }
            return count;
        }

        private async Task<int> CreateContacts(List<Winit.Modules.Contact.Model.Classes.Contact> Contacts)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO contact (id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                server_modified_time, title, name, phone, phone_extension, description, designation,
                mobile, email, email2, email3, invoice_for_email1, invoice_for_email2, invoice_for_email3,
                fax, linked_item_uid, linked_item_type, is_default, is_editable, enabled_for_invoice_email,
                enabled_for_docket_email, enabled_for_promo_email, is_email_cc, mobile2)
                VALUES (@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                @Title, @Name, @Phone, @PhoneExtension, @Description, @Designation, @Mobile, @Email, @Email2,
                @Email3, @InvoiceForEmail1, @InvoiceForEmail2, @InvoiceForEmail3, @Fax, @LinkedItemUID,
                @LinkedItemType, @IsDefault, @IsEditable, @EnabledForInvoiceEmail, @EnabledForDocketEmail,
                @EnabledForPromoEmail, @IsEmailCC, @Mobile2);";
                retVal= await ExecuteNonQueryAsync(sql, Contacts);
            }
            catch
            {
                throw;
            }
            return retVal;
        }

        public async Task<int> UpdateContacts(List<Winit.Modules.Contact.Model.Classes.Contact> contacts)
        {
            int retVal = -1;
            try
            {
                                var sql = @"UPDATE contact SET
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                title = @Title,
                                name = @Name,
                                phone = @Phone,
                                phone_extension = @PhoneExtension,
                                description = @Description,
                                designation = @Designation,
                                mobile = @Mobile,
                                email = @Email,
                                email2 = @Email2,
                                email3 = @Email3,
                                invoice_for_email1 = @InvoiceForEmail1,
                                invoice_for_email2 = @InvoiceForEmail2,
                                invoice_for_email3 = @InvoiceForEmail3,
                                fax = @Fax,
                                linked_item_uid = @LinkedItemUID,
                                linked_item_type = @LinkedItemType,
                                is_default = @IsDefault,
                                is_editable = @IsEditable,
                                enabled_for_invoice_email = @EnabledForInvoiceEmail,
                                enabled_for_docket_email = @EnabledForDocketEmail,
                                enabled_for_promo_email = @EnabledForPromoEmail,
                                is_email_cc = @IsEmailCC,
                                mobile2 = @Mobile2
                            WHERE uid = @UID;";
                retVal= await ExecuteNonQueryAsync(sql, contacts);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> CreateStoreDocuments(List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument> storeDocuments)
        {
            int retVal = -1;
            try
            {
                
                var sql = @"insert into store_document (id, uid, created_by, created_time, modified_by, modified_time, server_add_time,
                        server_modified_time, store_uid, document_type, document_no, valid_from, valid_up_to)
                        values
                        (@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID,
                        @DocumentType, @DocumentNo, @ValidFrom, @ValidUpTo);";
                retVal= await ExecuteNonQueryAsync(sql, storeDocuments);
            }
            catch
            {
                throw;
            }

            return retVal;
        }
        public async Task<int> UpdateStoreDocuments(List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument> storeDocuments)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE store_document
                            SET
                                created_by = @CreatedBy,
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                store_uid = @StoreUID,
                                document_type = @DocumentType,
                                document_no = @DocumentNo,
                                valid_from = @ValidFrom,
                                valid_up_to = @ValidUpTo
                            WHERE
                                uid = @UID;";
                
                retVal= await ExecuteNonQueryAsync(sql, storeDocuments);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

        public async Task<DistributorMasterView> GetDistributorDetailsByUID(string UID)
        {

            try
            {
                var parameters = new Dictionary<string, object>()
                {
                    {"UID",UID }
                };

                var query = @"SELECT O.uid AS UID,O.code AS Code,O.name AS Name,O.seq_code AS SeqCode,O.org_type_uid AS OrgTypeUid,O.parent_uid AS ParentUid,
                                O.country_uid AS CountryUid,O.company_uid AS CompanyUid,O.tax_group_uid AS TaxGroupUid,O.status AS Status,
                                O.has_early_access AS HasEarlyAccess,O.created_by AS CreatedBy,O.created_time AS CreatedTime,O.modified_by AS ModifiedBy,
                                O.modified_time AS ModifiedTime,O.server_add_time AS ServerAddTime,O.server_modified_time AS ServerModifiedTime,O.is_active AS IsActive 
                                FROM org AS O Where o.uid=@UID;
                          SELECT S.uid AS UID,
                                S.code AS Code,S.name AS Name,S.alias_name AS AliasName,S.legal_name AS LegalName,S.type AS Type,S.bill_to_store_uid AS BillToStoreUid,
                                S.ship_to_store_uid AS ShipToStoreUid,S.sold_to_store_uid AS SoldToStoreUid,S.status AS Status,
                                S.is_active AS IsActive,S.store_class AS StoreClass,S.store_rating AS StoreRating,S.is_blocked AS IsBlocked,S.blocked_reason_code AS BlockedReasonCode,S.blocked_reason_description AS BlockedReasonDescription,S.created_by AS CreatedBy,
                                S.created_time AS CreatedTime,S.modified_by AS ModifiedBy,S.modified_time AS ModifiedTime,S.server_add_time AS ServerAddTime,S.server_modified_time AS ServerModifiedTime,
                                S.created_by_emp_uid AS CreatedByEmpUid,S.created_by_job_position_uid AS CreatedByJobPositionUid,S.country_uid AS CountryUid,S.region_uid AS RegionUid,S.city_uid AS CityUid,S.source AS Source,
                                S.arabic_name AS ArabicName,S.outlet_name AS OutletName,S.blocked_by_emp_uid AS BlockedByEmpUid,S.is_tax_applicable AS IsTaxApplicable,S.tax_doc_number AS TaxDocNumber,
                                S.school_warehouse AS SchoolWarehouse,S.day_type AS DayType,S.special_day AS SpecialDay,S.is_tax_doc_verified AS IsTaxDocVerified,S.store_size AS StoreSize,
                                S.prospect_emp_uid AS ProspectEmpUid,S.tax_key_field AS TaxKeyField,S.store_image AS StoreImage,S.is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory,
                                S.tax_type AS TaxType,S.franchisee_org_uid AS FranchiseeOrgUid,S.state_uid AS StateUid,S.route_type AS RouteType,S.price_type AS PriceType
                                FROM  store AS S where S.uid=@UID;
                          SELECT S.uid AS UID,S.order_type AS OrderType,
                                    S.is_promotions_block AS IsPromotionsBlock,S.customer_start_date AS CustomerStartDate,
                                    S.customer_end_date AS CustomerEndDate,S.purchase_order_number AS PurchaseOrderNumber,
                                    S.delivery_docket_is_purchase_order_required AS DeliveryDocketIsPurchaseOrderRequired,S.is_with_printed_invoices AS IsWithPrintedInvoices,
                                    S.is_capture_signature_required AS IsCaptureSignatureRequired,S.is_always_printed AS IsAlwaysPrinted,
                                    S.building_delivery_code AS BuildingDeliveryCode,S.delivery_information AS DeliveryInformation,
                                    S.is_stop_delivery AS IsStopDelivery,S.is_forecast_top_up_qty AS IsForeCastTopUpQty,S.is_temperature_check AS IsTemperatureCheck,S.invoice_start_date AS InvoiceStartDate,
                                    S.invoice_end_date AS InvoiceEndDate,S.invoice_format AS InvoiceFormat,S.invoice_delivery_method AS InvoiceDeliveryMethod,S.display_delivery_docket AS DisplayDeliveryDocket,S.display_price AS DisplayPrice,S.show_cust_po AS ShowCustPO,S.invoice_text AS InvoiceText,S.invoice_frequency AS InvoiceFrequency,
                                    S.stock_credit_is_purchase_order_required AS StockCreditIsPurchaseOrderRequired,S.admin_fee_per_billing_cycle AS AdminFeePerBillingCycle,S.admin_fee_per_delivery AS AdminFeePerDelivery,S.late_payment_fee  AS LatePayementFee,S.drawer AS Drawer,S.bank_uid AS BankUid,S.bank_account AS BankAccount,S.mandatory_po_number AS MandatoryPONumber,
                                    S.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired,
                                    S.store_credit_always_printed AS StoreCreditAlwaysPrinted,S.is_dummy_customer AS IsDummyCustomer,S.default_run AS DefaultRun,
                                    S.is_foc_customer AS IsFOCCustomer,S.rss_show_price AS RSSShowPrice,S.rss_show_payment AS RSSShowPayment,S.rss_show_credit AS RSSShowCredit,S.rss_show_invoice AS RSSShowInvoice,
                                    S.rss_is_active AS RSSIsActive,S.rss_delivery_instruction_status AS RSSDeliveryInstructionStatus,S.rss_time_spent_on_rss_portal AS RSSTimeSpentOnRSSPortal,S.rss_order_placed_in_rss AS RSSOrderPlacedInRSS,
                                    S.rss_avg_orders_per_week AS RSSAvgOrdersPerWeek,S.rss_total_order_value AS RSSTotalOrderValue,S.allow_force_check_in AS AllowForceCheckIn,S.is_manual_edit_allowed AS IsManaualEditAllowed,
                                    S.can_update_lat_long AS CanUpdateLatLong,
                                    S.allow_replacement AS AllowReplacement,S.is_invoice_cancellation_allowed AS IsInvoiceCancellationAllowed,S.is_delivery_note_required AS IsDeliveryNoteRequired,S.e_invoicing_enabled AS EInvoicingEnabled,
                                    S.image_recognition_enabled  AS ImageRecognizationEnabled,S.max_outstanding_invoices AS MaxOutstandingInvoices,S.negative_invoice_allowed AS NegativeInvoiceAllowed,S.delivery_mode AS DeliveryMode,
                                    S.visit_frequency AS VisitFrequency,S.shipping_contact_same_as_store AS ShippingContactSameAsStore,S.billing_address_same_as_shipping AS BillingAddressSameAsShipping,S.payment_mode AS PaymentMode,
                                    S.price_type AS PriceType,S.average_monthly_income AS AverageMonthlyIncome,S.default_bank_uid AS DefaultBankUid,S.account_number AS AccountNumber,S.no_of_cash_counters AS NoOfCashCounters,
                                    S.custom_field1 AS CustomField1,S.custom_field2 AS CustomField2,S.custom_field3 AS CustomField3,S.custom_field4 AS CustomField4,S.custom_field5 AS CustomField5,S.custom_field6 AS CustomField6,
                                    S.custom_field7 AS CustomField7,S.custom_field8 AS CustomField8,S.custom_field9 AS CustomField9,S.custom_field10 AS CustomField10,S.store_additional_info AS StoreAdditionalInfo,
                                    S.is_asset_enabled AS IsAssetEnabled,S.is_survey_enabled AS IsSurveyEnabled,S.allow_good_returns AS AllowGoodReturns,S.allow_bad_returns AS AllowBadReturns,S.allow_return_against_invoice AS AllowReturnAgainstInvoice,
                                    S.allow_return_with_sales_order AS AllowReturnWithSalesOrder,S.week_off_sun AS WeekOffSun,S.week_off_mon AS WeekOffMon,S.week_off_tue AS WeekOffTue,S.week_off_wed AS WeekOffWed,
                                    S.week_off_thu AS WeekOffThu,S.week_off_fri AS WeekOffFri,S.week_off_sat AS WeekOffSat,S.aging_cycle AS AgingCycle,S.depot AS Depot
                                    FROM    store_additional_info AS S where Store_UID=@UID;
                          SELECT sc.id AS Id,sc.uid AS Uid,sc.created_by AS CreatedBy,sc.created_time AS CreatedTime,
                                    sc.modified_by AS ModifiedBy,sc.modified_time AS ModifiedTime,sc.server_add_time AS ServerAddTime,
                                    sc.server_modified_time AS ServerModifiedTime,sc.store_uid AS StoreUid,sc.payment_term_uid AS PaymentTermUid,
                                    sc.credit_type AS CreditType,sc.credit_limit AS CreditLimit,sc.temporary_credit AS TemporaryCredit,sc.org_uid AS OrgUid,sc.distribution_channel_uid AS DistributionChannelUid,
                                    sc.preferred_payment_mode AS PreferredPaymentMode,sc.is_active AS IsActive,sc.is_blocked AS IsBlocked,sc.blocking_reason_code AS BlockingReasonCode,sc.blocking_reason_description AS BlockingReasonDescription,sc.price_list AS PriceList,sc.authorized_item_grp_key AS AuthorizedItemGrpKey,sc.message_key AS MessageKey,
                                    sc.tax_key_field AS TaxKeyField,sc.promotion_key AS PromotionKey,sc.disabled AS Disabled,sc.bill_to_address_uid AS BillToAddressUid,
                                    sc.ship_to_address_uid AS ShipToAddressUid,sc.outstanding_invoices AS OutstandingInvoices,sc.preferred_payment_method AS PreferredPaymentMethod,sc.payment_type AS PaymentType,
                                    sc.invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle,sc.invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery,sc.invoice_late_payment_fee AS InvoiceLatePaymentFee,
                                    sc.is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed,sc.is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed,sc.is_outstanding_bill_control AS IsOutstandingBillControl,sc.is_negative_invoice_allowed AS IsNegativeInvoiceAllowed
                                    FROM store_credit AS sc where store_uid=@UID;
                          SELECT c.id AS Id,c.uid AS UID,c.created_by AS CreatedBy,
                                    c.created_time AS CreatedTime,c.modified_by AS ModifiedBy,c.modified_time AS ModifiedTime,c.server_add_time AS ServerAddTime,
                                    c.server_modified_time AS ServerModifiedTime,c.title AS Title,c.name AS Name,c.phone AS Phone,
                                    c.phone_extension AS PhoneExtension,c.description AS Description,c.designation AS Designation,
                                    c.mobile AS Mobile,c.email AS Email,c.email2 AS Email2,c.email3 AS Email3,c.invoice_for_email1 AS InvoiceForEmail1,
                                    c.invoice_for_email2 AS InvoiceForEmail2,c.invoice_for_email3 AS InvoiceForEmail3,c.fax AS Fax,c.linked_item_uid AS LinkedItemUID,
                                    c.linked_item_type AS LinkedItemType,c.is_default AS IsDefault,c.is_editable AS IsEditable,
                                    c.enabled_for_invoice_email AS EnabledForInvoiceEmail,c.enabled_for_docket_email AS EnabledForDocketEmail,c.enabled_for_promo_email AS EnabledForPromoEmail,c.is_email_cc AS IsEmailCC,c.mobile2 AS Mobile2
                                    FROM public.contact c where linked_item_uid=@UID;
                          SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                                    modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,type AS Type,name AS Name,
                                    line1 AS Line1,line2 AS Line2,line3 AS Line3,landmark AS Landmark,area AS Area,sub_area AS SubArea,zip_code AS ZipCode,
                                    city AS City,country_code AS CountryCode,region_code AS RegionCode,phone AS Phone,phone_extension AS PhoneExtension,mobile1 AS Mobile1,
                                    mobile2 AS Mobile2,email AS Email,fax AS Fax,latitude AS Latitude,longitude AS Longitude,altitude AS Altitude,linked_item_uid AS LinkedItemUID,
                                    linked_item_type AS LinkedItemType,status AS Status,state_code AS StateCode,territory_code AS TerritoryCode,pan AS PAN,aadhar AS AADHAR,
                                    ssn AS SSN,is_editable AS IsEditable,is_default AS IsDefault,line4 AS Line4,info AS Info,depot AS Depot
                                    FROM  public. address Where linked_item_uid=@UID;
                         SELECT sd.id AS Id,
                                    sd.uid AS UID,sd.created_by AS CreatedBy,sd.created_time AS CreatedTime,sd.modified_by AS ModifiedBy,sd.modified_time AS ModifiedTime,sd.server_add_time AS ServerAddTime,
                                    sd.server_modified_time AS ServerModifiedTime,sd.store_uid AS StoreUID,sd.document_type AS DocumentType,sd.document_no AS DocumentNo,sd.valid_from AS ValidFrom,sd.valid_up_to AS ValidUpTo
                                    FROM public.store_document sd where store_uid=@UID;";

                DistributorMasterView distributorMasterView = new DistributorMasterView();
                DataSet ds = await ExecuteQueryDataSetAsync(query,parameters);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            distributorMasterView.Org = ConvertDataTableToObject<Winit.Modules.Org.Model.Classes.Org>(ds.Tables[0].Rows[0]);
                        }
                    }
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            distributorMasterView.Store = ConvertDataTableToObject<Winit.Modules.Store.Model.Classes.Store>(ds.Tables[1].Rows[0]);
                        }
                    }
                    if (ds.Tables.Count > 2)
                    {
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            distributorMasterView.StoreAdditionalInfo = ConvertDataTableToObject<Winit.Modules.Store.Model.Classes.StoreAdditionalInfo>(ds.Tables[2].Rows[0]);
                        }
                    }
                    if (ds.Tables.Count > 3)
                    {
                        if (ds.Tables[3].Rows.Count > 0)
                        {
                            distributorMasterView.StoreCredit = ConvertDataTableToObject<Winit.Modules.Store.Model.Classes.StoreCredit>(ds.Tables[3].Rows[0]);
                        }
                    }
                    if (ds.Tables.Count > 4)
                    {
                        if (ds.Tables[4].Rows.Count > 0)
                        {
                            distributorMasterView.Contacts = new();
                            foreach (DataRow row in ds.Tables[4].Rows)
                            {
                                distributorMasterView.Contacts.Add(ConvertDataTableToObject<Winit.Modules.Contact.Model.Classes.Contact>(row));
                            }
                        }
                    }
                    if (ds.Tables.Count > 5)
                    {
                        if (ds.Tables[5].Rows.Count > 0)
                        {
                            distributorMasterView.Address = ConvertDataTableToObject<Winit.Modules.Address.Model.Classes.Address>(ds.Tables[5].Rows[0]);
                        }
                    }
                    if (ds.Tables.Count > 6)
                    {
                        if (ds.Tables[6].Rows.Count > 0)
                        {
                            distributorMasterView.Documents = new();
                            foreach (DataRow row in ds.Tables[6].Rows)
                            {
                                distributorMasterView.Documents.Add(ConvertDataTableToObject<Winit.Modules.StoreDocument.Model.Classes.StoreDocument>(row));
                            }
                        }
                    }
                }
                return distributorMasterView;
            }
            catch
            {
                throw;
            }
        }
    }
}
