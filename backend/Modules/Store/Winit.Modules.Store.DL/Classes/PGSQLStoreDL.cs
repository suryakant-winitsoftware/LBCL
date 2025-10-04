using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.FileSys.DL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.StoreDocument.Model.Interfaces;
using Winit.Modules.StoreMaster.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLStoreDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreDL
    {
        private readonly IStoreAdditionalInfoDL _storeAdditionalInfoDL;
        private readonly IStoreAdditionalInfoCMIDL _storeAdditionalInfoCMIDL;
        private readonly IFileSysDL _fileSysDL;

        public PGSQLStoreDL(IServiceProvider serviceProvider, IConfiguration config,
            IStoreAdditionalInfoDL storeAdditionalInfoDL, IStoreAdditionalInfoCMIDL storeAdditionalInfoCMIDL,
            IFileSysDL filesysDL) : base(serviceProvider, config)
        {
            _storeAdditionalInfoDL = storeAdditionalInfoDL;
            _storeAdditionalInfoCMIDL = storeAdditionalInfoCMIDL;
            _fileSysDL = filesysDL;
        }

        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>> SelectAllStore(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (select s.id as Id,  s.uid as UID, 
                                s.created_by as CreatedBy,  s.created_time as CreatedTime, 
                                s.modified_by as ModifiedBy, s.modified_time as ModifiedTime,
                                s.server_add_time as ServerAddTime, s.server_modified_time as ServerModifiedTime, 
                                s.company_uid as CompanyUID, code,s.number, s.name as Name, 
                                s.alias_name as AliasName,  s.legal_name as LegalName, 
                                s.type as Type, s.bill_to_store_uid as BillToStoreUID,s.ship_to_store_uid as ShipToStoreUID, s.sold_to_store_uid as SoldToStoreUID, s.status as StoreStatus,  s.is_active as IsActive,
                                s.store_class as StoreClass, s.store_rating as StoreRating, s.is_blocked as IsBlocked, s.blocked_reason_code as BlockedReasonCode,
                                s.blocked_reason_description as BlockedReasonDescription, s.created_by_emp_uid as CreatedByEmpUID, 
                                s.created_by_job_position_uid as CreatedByJobPositionUID,s.country_uid as CountryUID, s.region_uid as RegionUID, city_uid as CityUID, s.source as StoreSource, outlet_name as OutletName,
                                s.blocked_by_emp_uid as BlockedByEmpUID, s.arabic_name as ArabicName, s.is_tax_applicable as IsTaxApplicable, s.tax_doc_number as TaxDocNumber,s.school_warehouse as SchoolWarehouse, 
                                s.day_type as DayType, special_day as SpecialDay, s.is_tax_doc_verified as IsTaxDocVerified,s.store_size as StoreSize, s.prospect_emp_uid as ProspectEmpUID, 
                                s.tax_key_field as TaxKeyField, s.store_image as StoreImage,s.is_vat_qr_capture_mandatory as IsVATQRCaptureMandatory, 
                                s.tax_type as TaxType,ct.mobile as StoreNumber,ct.Email as Email,sc.outstanding_invoices as TotalOutStandings,s.franchisee_org_UID as franchiseeorguid
                                from store s
								left join contact ct on ct.linked_item_uid=s.uid
								Left join store_credit sc ON sc.store_uid = s.uid
                                                    ) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt from (select s.id as Id,  s.uid as UID, 
                                s.created_by as CreatedBy,  s.created_time as CreatedTime, 
                                s.modified_by as ModifiedBy, s.modified_time as ModifiedTime,
                                s.server_add_time as ServerAddTime, s.server_modified_time as ServerModifiedTime, 
                                s.company_uid as CompanyUID, code,s.number, s.name as Name, 
                                s.alias_name as AliasName,  s.legal_name as LegalName, 
                                s.type as Type, s.bill_to_store_uid as BillToStoreUID,s.ship_to_store_uid as ShipToStoreUID, s.sold_to_store_uid as SoldToStoreUID, s.status as StoreStatus,  s.is_active as IsActive,
                                s.store_class as StoreClass, s.store_rating as StoreRating, s.is_blocked as IsBlocked, s.blocked_reason_code as BlockedReasonCode,
                                s.blocked_reason_description as BlockedReasonDescription, s.created_by_emp_uid as CreatedByEmpUID, 
                                s.created_by_job_position_uid as CreatedByJobPositionUID,s.country_uid as CountryUID, s.region_uid as RegionUID, city_uid as CityUID, s.source as StoreSource, outlet_name as OutletName,
                                s.blocked_by_emp_uid as BlockedByEmpUID, s.arabic_name as ArabicName, s.is_tax_applicable as IsTaxApplicable, s.tax_doc_number as TaxDocNumber,s.school_warehouse as SchoolWarehouse, 
                                s.day_type as DayType, special_day as SpecialDay, s.is_tax_doc_verified as IsTaxDocVerified,s.store_size as StoreSize, s.prospect_emp_uid as ProspectEmpUID, 
                                s.tax_key_field as TaxKeyField, s.store_image as StoreImage,s.is_vat_qr_capture_mandatory as IsVATQRCaptureMandatory, 
                                s.tax_type as TaxType,ct.mobile as StoreNumber,ct.Email as Email,sc.outstanding_invoices as TotalOutStandings,s.franchisee_org_UID as franchiseeorguid
                                from store s
								left join contact ct on ct.linked_item_uid=s.uid
								Left join store_credit sc ON sc.store_uid = s.uid) as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Store.Model.Interfaces.IStore>(filterCriterias, sbFilterCriteria,
                        parameters);

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
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStore>().GetType();
                IEnumerable<Model.Interfaces.IStore> stores =
                    await ExecuteQueryAsync<Model.Interfaces.IStore>(sql.ToString(), parameters, type);
                //Count
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponse =
                    new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>
                    {
                        PagedData = stores,
                        TotalCount = totalCount
                    };
                return pagedResponse;
            }
            catch (Exception)
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

            var sql = @" Select id AS Id,
                                            UID AS Uid,
                                            created_by AS CreatedBy,
                                            created_time AS CreatedTime,
                                            modified_by AS ModifiedBy,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime,
                                            company_UID AS CompanyUid,
                                            code AS Code,
                                            number AS Number,
                                            name AS Name,
                                            alias_name AS AliasName,
                                            legal_name AS LegalName,
                                            type AS Type,
                                            bill_to_store_UID AS BillToStoreUid,
                                            ship_to_store_UID AS ShipToStoreUid,
                                            sold_to_store_UID AS SoldToStoreUid,
                                            status AS Status,
                                            is_active AS IsActive,
                                            store_class AS StoreClass,
                                            store_rating AS StoreRating,
                                            is_blocked AS IsBlocked,
                                            blocked_reason_code AS BlockedReasonCode,
                                            blocked_reason_description AS BlockedReasonDescription,
                                            created_by_emp_UID AS CreatedByEmpUid,
                                            created_by_job_position_UID AS CreatedByJobPositionUid,
                                            country_UID AS CountryUid,
                                            region_UID AS RegionUid,
                                            city_UID AS CityUid,
                                            source AS Source,
                                            outlet_name AS OutletName,
                                            blocked_by_emp_UID AS BlockedByEmpUid,
                                            arabic_name AS ArabicName,
                                            is_tax_applicable AS IsTaxApplicable,
                                            tax_doc_number AS TaxDocNumber,
                                            school_warehouse AS SchoolWarehouse,
                                            day_type AS DayType,
                                            special_day AS SpecialDay,
                                            is_tax_doc_verified AS IsTaxDocVerified,
                                            store_size AS StoreSize,
                                            prospect_emp_uid AS ProspectEmpUid,
                                            tax_key_field AS TaxKeyField,
                                            store_image AS StoreImage,
                                            is_vat_qr_capture_mandatory AS IsVatQrCaptureMandatory,
                                            tax_type AS TaxType,price_type as PriceType,
                                            franchisee_org_uid AS FranchiseeOrgUid,location_uid as LocationUID FROM store WHERE uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStore>().GetType();
            Model.Interfaces.IStore StoreList =
                await ExecuteSingleAsync<Model.Interfaces.IStore>(sql, parameters, type);
            return StoreList;
        }

        public async Task<int> CreateStore(Model.Interfaces.IStore store)
        {
            try
            {
                var sql =
                    @"INSERT INTO store (UID, created_by, created_time, modified_by, modified_time, server_add_time, 
                   server_modified_time, company_UID, code, number, name, alias_name, legal_name, type, 
                   bill_to_store_UID, ship_to_store_UID, sold_to_store_UID, status, is_active, store_class, store_rating, 
                   is_blocked, blocked_reason_code, blocked_reason_description, created_by_emp_UID, created_by_job_position_UID, 
                   country_UID, region_UID, city_UID, source, outlet_name, blocked_by_emp_UID, arabic_name, is_tax_applicable, 
                   tax_doc_number, school_warehouse, day_type, special_day, is_tax_doc_verified, store_size, prospect_emp_UID,
                   tax_key_field, store_image, is_vat_qr_capture_mandatory, tax_type, franchisee_org_UID, state_UID, route_type, price_type,location_uid,broad_classification,classfication_type ) 
                           VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,@ServerModifiedTime, @CompanyUID, @Code, 
                           @Number, @Name, @AliasName, @LegalName, @Type, @BillToStoreUID, @ShipToStoreUID, @SoldToStoreUID, @Status, @IsActive, 
                           @StoreClass, @StoreRating, @IsBlocked, @BlockedReasonCode, @BlockedReasonDescription, @CreatedByEmpUID, @CreatedByJobPositionUID,
                           @CountryUID, @RegionUID, @CityUID, @Source, @OutletName, @BlockedByEmpUID, @ArabicName, @IsTaxApplicable, @TaxDocNumber, 
                           @SchoolWarehouse, @DayType, @SpecialDay, @IsTaxDocVerified, @StoreSize, @ProspectEmpUID,@TaxKeyField, @StoreImage, 
                            @IsVATQRCaptureMandatory, @TaxType,@FranchiseeOrgUID,@StateUID,@RouteType,@PriceType,@LocationUID,@BroadClassification,@ClassficationType)";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
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
                    { "@StateUID", store.StateUID },
                    { "@RouteType", store.RouteType },
                    { "@PriceType", store.PriceType },
                    { "@LocationUID", store.LocationUID },
                    { "@BroadClassification", store.BroadClassification },
                    { "@ClassficationType", store.ClassficationType },
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
                            prospect_emp_uid = @ProspectEmpUID,
                            tax_key_field = @TaxKeyField, 
                            store_image = @StoreImage, 
                            is_vat_qr_capture_mandatory = @IsVATQRCaptureMandatory, 
                            tax_type = @TaxType,
                            state_uid = @StateUID,
                            price_type = @PriceType,
                            route_type = @RouteType,
                            location_uid=@LocationUID,
                            broad_classification=@BroadClassification,
                            classfication_type=@ClassficationType

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
                    { "@StateUID", store.StateUID },
                    { "@RouteType", store.RouteType },
                    { "@PriceType", store.PriceType },
                    { "@LocationUID", store.LocationUID },
                    { "@BroadClassification", store.BroadClassification },
                    { "@ClassficationType", store.ClassficationType },
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
            var sql = @"DELETE  FROM Store WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> CreateStoreMaster(
            Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO createStoreMaster)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var storeOrderQuery =
                                @"INSERT INTO store (UID, created_by, created_time, modified_by, modified_time, server_add_time, 
                           server_modified_time, company_UID, code, number, name, alias_name, legal_name, type, 
                           bill_to_store_UID, ship_to_store_UID, sold_to_store_UID, status, is_active, store_class, store_rating, 
                           is_blocked, blocked_reason_code, blocked_reason_description, created_by_emp_UID, created_by_job_position_UID, 
                           country_UID, region_UID, city_UID, source, outlet_name, blocked_by_emp_UID, arabic_name) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, @Code, @Number, @Name, @AliasName, @LegalName, @Type, 
                            @BillToStoreUID, @ShipToStoreUID, @SoldToStoreUID, @Status, @IsActive, @StoreClass, @StoreRating, @IsBlocked, @BlockedReasonCode, 
                            @BlockedReasonDescription, @CreatedByEmpUID, @CreatedByJobPositionUID, @CountryUID, @RegionUID, @CityUID, @Source, @OutletName, 
                            @BlockedByEmpUID, @ArabicName)";
                            var storeParameters = new Dictionary<string, object?>
                            {
                                { "CompanyUID", createStoreMaster.store.CompanyUID },
                                { "Code", createStoreMaster.store.Code },
                                { "Number", createStoreMaster.store.Number },
                                { "Name", createStoreMaster.store.Name },
                                { "AliasName", createStoreMaster.store.AliasName },
                                { "LegalName", createStoreMaster.store.LegalName },
                                { "Type", createStoreMaster.store.Type },
                                { "BillToStoreUID", createStoreMaster.store.BillToStoreUID },
                                { "ShipToStoreUID", createStoreMaster.store.ShipToStoreUID },
                                { "SoldToStoreUID", createStoreMaster.store.SoldToStoreUID },
                                { "Status", createStoreMaster.store.Status },
                                { "IsActive", createStoreMaster.store.IsActive },
                                { "StoreClass", createStoreMaster.store.StoreClass },
                                { "StoreRating", createStoreMaster.store.StoreRating },
                                { "IsBlocked", createStoreMaster.store.IsBlocked },
                                { "BlockedReasonCode", createStoreMaster.store.BlockedReasonCode },
                                { "BlockedReasonDescription", createStoreMaster.store.BlockedReasonDescription },
                                { "CreatedByEmpUID", createStoreMaster.store.CreatedByEmpUID },
                                { "CreatedByJobPositionUID", createStoreMaster.store.CreatedByJobPositionUID },
                                { "CountryUID", createStoreMaster.store.CountryUID },
                                { "RegionUID", createStoreMaster.store.RegionUID },
                                { "CityUID", createStoreMaster.store.CityUID },
                                { "Source", createStoreMaster.store.Source },
                                { "CreatedTime", createStoreMaster.store.CreatedTime },
                                { "ModifiedTime", createStoreMaster.store.ModifiedTime },
                                { "ModifiedBy", createStoreMaster.store.ModifiedBy },
                                { "ServerAddTime", createStoreMaster.store.ServerAddTime },
                                { "ServerModifiedTime", createStoreMaster.store.ServerModifiedTime },
                                { "CreatedBy", createStoreMaster.store.CreatedBy },
                                { "UID", createStoreMaster.store.UID },
                                { "OutletName", createStoreMaster.store.OutletName },
                                { "BlockedByEmpUID", createStoreMaster.store.BlockedByEmpUID },
                                { "ArabicName", createStoreMaster.store.ArabicName }
                            };

                            count = await ExecuteNonQueryAsync(storeOrderQuery, connection, transaction,
                                storeParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("Store Insert failed");
                            }
                            var storeAdditionalInfoQuery =
                                @"INSERT INTO store_additional_info (UID, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, store_UID, order_type, is_promotions_block, customer_start_date, customer_end_date, 
                            purchase_order_number, delivery_docket_is_purchase_order_required, is_with_printed_invoices, 
                            is_capture_signature_required, is_always_printed, building_delivery_code, delivery_information, is_stop_delivery, 
                            is_forecast_top_up_qty, is_temperature_check, invoice_start_date, invoice_end_date, invoice_format, invoice_delivery_method, 
                            display_delivery_docket, display_price, show_cust_po, invoice_text, invoice_frequency, stock_credit_is_purchase_order_required, 
                            admin_fee_per_billing_cycle, admin_fee_per_delivery, late_payment_fee, drawer, bank_UID, bank_account, mandatory_po_number, 
                            is_store_credit_capture_signature_required, store_credit_always_printed, is_dummy_customer, default_run, 
                            is_foc_customer, rss_show_price, rss_show_payment, rss_show_credit, rss_show_invoice, rss_is_active, rss_delivery_instruction_status, 
                            rss_time_spent_on_rss_portal, rss_order_placed_in_rss, rss_avg_orders_per_week, rss_total_order_value, allow_force_check_in, 
                            is_manual_edit_allowed, can_update_lat_long, allow_good_return, 
                            allow_bad_return,  allow_replacement, is_invoice_cancellation_allowed, is_delivery_note_required, 
                            e_invoicing_enabled, image_recognition_enabled, max_outstanding_invoices, negative_invoice_allowed, delivery_mode, 
                            visit_frequency, shipping_contact_same_as_store, billing_address_same_as_shipping, payment_mode, price_type, average_monthly_income, 
                            default_bank_UID, account_number, no_of_cash_counters, custom_field1, custom_field2, custom_field3, custom_field4, custom_field5, 
                            custom_field6, custom_field7, custom_field8, custom_field9, custom_field10, 
                            is_asset_enabled, is_survey_enabled,  allow_return_against_invoice, allow_return_with_sales_order, 
                            week_off_sun, week_off_mon, week_off_tue, week_off_wed, week_off_thu, week_off_fri, week_off_sat) 
                            VALUES ( @UID , @CreatedBy , @CreatedTime , @ModifiedBy , @ModifiedTime , @ServerAddTime , @ServerModifiedTime , @StoreUID , @OrderType , 
                            @IsPromotionsBlock , @CustomerStartDate , @CustomerEndDate , @PurchaseOrderNumber , @DeliveryDocketIsPurchaseOrderRequired , 
                            @IsWithPrintedInvoices , @IsCaptureSignatureRequired , @IsAlwaysPrinted , @BuildingDeliveryCode , @DeliveryInformation , @IsStopDelivery , 
                            @IsForeCastTopUpQty , @IsTemperatureCheck , @InvoiceStartDate , @InvoiceEndDate , @InvoiceFormat , @InvoiceDeliveryMethod , 
                            @DisplayDeliveryDocket , @DisplayPrice , @ShowCustPO , @InvoiceText , @InvoiceFrequency , @StockCreditIsPurchaseOrderRequired , 
                            @AdminFeePerBillingCycle , @AdminFeePerDelivery , @LatePaymentFee , @Drawer , @BankUID , @BankAccount , @MandatoryPONumber , 
                            @IsStoreCreditCaptureSignatureRequired , @StoreCreditAlwaysPrinted , @IsDummyCustomer , @DefaultRun , @IsFOCCustomer , 
                            @RSSShowPrice , @RSSShowPayment , @RSSShowCredit , @RSSShowInvoice , @RSSIsActive , @RSSDeliveryInstructionStatus , @RSSTimeSpentOnRSSPortal , 
                            @RSSOrderPlacedInRSS , @RSSAvgOrdersPerWeek , @RSSTotalOrderValue , @AllowForceCheckIn , @IsManualEditAllowed , @CanUpdateLatLong , 
                            @AllowGoodReturn , @AllowBadReturn , @AllowReplacement , 
                            @IsInvoiceCancellationAllowed , @IsDeliveryNoteRequired , @EInvoicingEnabled , @ImageRecognizationEnabled , @MaxOutstandingInvoices , 
                            @NegativeInvoiceAllowed , @DeliveryMode , @VisitFrequency , @ShippingContactSameAsStore , @BillingAddressSameAsShipping , 
                            @PaymentMode , @PriceType , @AverageMonthlyIncome , @DefaultBankUID , @AccountNumber , @NoOfCashCounters,@CustomField1,@CustomField2,@CustomField3,
                            @CustomField4,@CustomField5,@CustomField6,@CustomField7,@CustomField8,@CustomField9, @CustomField10,
                            @IsAssetEnabled ,@IsSurveyEnabled ,@AllowReturnAgainstInvoice ,@AllowReturnWithSalesOrder,@WeekOffSun,@WeekOffMon,
                            @WeekOffTue,@WeekOffWed,@WeekOffThu,@WeekOffFri,@WeekOffSat) ";
                            var storeAdditionalInfoParameters = new Dictionary<string, object?>
                            {
                                { "UID", createStoreMaster.StoreAdditionalInfo.UID },
                                { "CreatedBy", createStoreMaster.StoreAdditionalInfo.CreatedBy },
                                { "CreatedTime", createStoreMaster.StoreAdditionalInfo.CreatedTime },
                                { "ModifiedBy", createStoreMaster.StoreAdditionalInfo.ModifiedBy },
                                { "ModifiedTime", createStoreMaster.StoreAdditionalInfo.ModifiedTime },
                                { "ServerAddTime", createStoreMaster.StoreAdditionalInfo.ServerAddTime },
                                { "ServerModifiedTime", createStoreMaster.StoreAdditionalInfo.ServerModifiedTime },
                                { "StoreUID", createStoreMaster.StoreAdditionalInfo.StoreUID },
                                { "OrderType", createStoreMaster.StoreAdditionalInfo.OrderType },
                                { "IsPromotionsBlock", createStoreMaster.StoreAdditionalInfo.IsPromotionsBlock },
                                { "CustomerStartDate", createStoreMaster.StoreAdditionalInfo.CustomerStartDate },
                                { "CustomerEndDate", createStoreMaster.StoreAdditionalInfo.CustomerEndDate },
                                // {"SchoolWarehouse",createStoreMaster.StoreAdditionalInfo.SchoolWarehouse},
                                { "PurchaseOrderNumber", createStoreMaster.StoreAdditionalInfo.PurchaseOrderNumber },
                                {
                                    "DeliveryDocketIsPurchaseOrderRequired",
                                    createStoreMaster.StoreAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired
                                },
                                {
                                    "IsWithPrintedInvoices", createStoreMaster.StoreAdditionalInfo.IsWithPrintedInvoices
                                },
                                {
                                    "IsCaptureSignatureRequired",
                                    createStoreMaster.StoreAdditionalInfo.IsCaptureSignatureRequired
                                },
                                { "IsAlwaysPrinted", createStoreMaster.StoreAdditionalInfo.IsAlwaysPrinted },
                                { "BuildingDeliveryCode", createStoreMaster.StoreAdditionalInfo.BuildingDeliveryCode },
                                { "DeliveryInformation", createStoreMaster.StoreAdditionalInfo.DeliveryInformation },
                                { "IsStopDelivery", createStoreMaster.StoreAdditionalInfo.IsStopDelivery },
                                { "IsForeCastTopUpQty", createStoreMaster.StoreAdditionalInfo.IsForeCastTopUpQty },
                                { "IsTemperatureCheck", createStoreMaster.StoreAdditionalInfo.IsTemperatureCheck },
                                { "InvoiceStartDate", createStoreMaster.StoreAdditionalInfo.InvoiceStartDate },
                                { "InvoiceEndDate", createStoreMaster.StoreAdditionalInfo.InvoiceEndDate },
                                { "InvoiceFormat", createStoreMaster.StoreAdditionalInfo.InvoiceFormat },
                                {
                                    "InvoiceDeliveryMethod", createStoreMaster.StoreAdditionalInfo.InvoiceDeliveryMethod
                                },
                                {
                                    "DisplayDeliveryDocket", createStoreMaster.StoreAdditionalInfo.DisplayDeliveryDocket
                                },
                                { "DisplayPrice", createStoreMaster.StoreAdditionalInfo.DisplayPrice },
                                { "ShowCustPO", createStoreMaster.StoreAdditionalInfo.ShowCustPO },
                                { "InvoiceText", createStoreMaster.StoreAdditionalInfo.InvoiceText },
                                { "InvoiceFrequency", createStoreMaster.StoreAdditionalInfo.InvoiceFrequency },
                                {
                                    "StockCreditIsPurchaseOrderRequired",
                                    createStoreMaster.StoreAdditionalInfo.StockCreditIsPurchaseOrderRequired
                                },
                                {
                                    "AdminFeePerBillingCycle",
                                    createStoreMaster.StoreAdditionalInfo.AdminFeePerBillingCycle
                                },
                                { "AdminFeePerDelivery", createStoreMaster.StoreAdditionalInfo.AdminFeePerDelivery },
                                { "LatePaymentFee", createStoreMaster.StoreAdditionalInfo.LatePaymentFee },
                                { "Drawer", createStoreMaster.StoreAdditionalInfo.Drawer },
                                { "BankUID", createStoreMaster.StoreAdditionalInfo.BankUID },
                                { "BankAccount", createStoreMaster.StoreAdditionalInfo.BankAccount },
                                { "MandatoryPONumber", createStoreMaster.StoreAdditionalInfo.MandatoryPONumber },
                                {
                                    "IsStoreCreditCaptureSignatureRequired",
                                    createStoreMaster.StoreAdditionalInfo.IsStoreCreditCaptureSignatureRequired
                                },
                                {
                                    "StoreCreditAlwaysPrinted",
                                    createStoreMaster.StoreAdditionalInfo.StoreCreditAlwaysPrinted
                                },
                                { "IsDummyCustomer", createStoreMaster.StoreAdditionalInfo.IsDummyCustomer },
                                { "DefaultRun", createStoreMaster.StoreAdditionalInfo.DefaultRun },
                                //{"ProspectEmpUID",createStoreMaster.StoreAdditionalInfo.ProspectEmpUID},
                                { "IsFOCCustomer", createStoreMaster.StoreAdditionalInfo.IsFOCCustomer },
                                { "RSSShowPrice", createStoreMaster.StoreAdditionalInfo.RSSShowPrice },
                                { "RSSShowPayment", createStoreMaster.StoreAdditionalInfo.RSSShowPayment },
                                { "RSSShowCredit", createStoreMaster.StoreAdditionalInfo.RSSShowCredit },
                                { "RSSShowInvoice", createStoreMaster.StoreAdditionalInfo.RSSShowInvoice },
                                { "RSSIsActive", createStoreMaster.StoreAdditionalInfo.RSSIsActive },
                                {
                                    "RSSDeliveryInstructionStatus",
                                    createStoreMaster.StoreAdditionalInfo.RSSDeliveryInstructionStatus
                                },
                                {
                                    "RSSTimeSpentOnRSSPortal",
                                    createStoreMaster.StoreAdditionalInfo.RSSTimeSpentOnRSSPortal
                                },
                                { "RSSOrderPlacedInRSS", createStoreMaster.StoreAdditionalInfo.RSSOrderPlacedInRSS },
                                { "RSSAvgOrdersPerWeek", createStoreMaster.StoreAdditionalInfo.RSSAvgOrdersPerWeek },
                                { "RSSTotalOrderValue", createStoreMaster.StoreAdditionalInfo.RSSTotalOrderValue },
                                { "AllowForceCheckIn", createStoreMaster.StoreAdditionalInfo.AllowForceCheckIn },
                                { "IsManualEditAllowed", createStoreMaster.StoreAdditionalInfo.IsManualEditAllowed },
                                { "CanUpdateLatLong", createStoreMaster.StoreAdditionalInfo.CanUpdateLatLong },
                                //{"IsTaxApplicable",createStoreMaster.StoreAdditionalInfo.IsTaxApplicable},
                                //{"TaxDocNumber",createStoreMaster.StoreAdditionalInfo.TaxDocNumber},
                                //{"IsTaxDocVerified",createStoreMaster.StoreAdditionalInfo.IsTaxDocVerified},
                                { "AllowGoodReturn", createStoreMaster.StoreAdditionalInfo.AllowGoodReturn },
                                { "AllowBadReturn", createStoreMaster.StoreAdditionalInfo.AllowBadReturn },

                                { "AllowReplacement", createStoreMaster.StoreAdditionalInfo.AllowReplacement },
                                {
                                    "IsInvoiceCancellationAllowed",
                                    createStoreMaster.StoreAdditionalInfo.IsInvoiceCancellationAllowed
                                },
                                {
                                    "IsDeliveryNoteRequired",
                                    createStoreMaster.StoreAdditionalInfo.IsDeliveryNoteRequired
                                },
                                { "EInvoicingEnabled", createStoreMaster.StoreAdditionalInfo.EInvoicingEnabled },
                                {
                                    "ImageRecognizationEnabled",
                                    createStoreMaster.StoreAdditionalInfo.ImageRecognizationEnabled
                                },
                                {
                                    "MaxOutstandingInvoices",
                                    createStoreMaster.StoreAdditionalInfo.MaxOutstandingInvoices
                                },
                                {
                                    "NegativeInvoiceAllowed",
                                    createStoreMaster.StoreAdditionalInfo.NegativeInvoiceAllowed
                                },
                                { "DeliveryMode", createStoreMaster.StoreAdditionalInfo.DeliveryMode },
                                //{"StoreSize",createStoreMaster.StoreAdditionalInfo.StoreSize},
                                {"VisitFrequency",createStoreMaster.StoreAdditionalInfo.VisitFrequency},
                                {
                                    "ShippingContactSameAsStore",
                                    createStoreMaster.StoreAdditionalInfo.ShippingContactSameAsStore
                                },
                                {
                                    "BillingAddressSameAsShipping",
                                    createStoreMaster.StoreAdditionalInfo.BillingAddressSameAsShipping
                                },
                                { "PaymentMode", createStoreMaster.StoreAdditionalInfo.PaymentMode },
                                { "PriceType", createStoreMaster.StoreAdditionalInfo.PriceType },
                                { "AverageMonthlyIncome", createStoreMaster.StoreAdditionalInfo.AverageMonthlyIncome },
                                { "DefaultBankUID", createStoreMaster.StoreAdditionalInfo.DefaultBankUID },
                                { "AccountNumber", createStoreMaster.StoreAdditionalInfo.AccountNumber },
                                { "NoOfCashCounters", createStoreMaster.StoreAdditionalInfo.NoOfCashCounters },
                                { "CustomField1", createStoreMaster.StoreAdditionalInfo.CustomField1 },
                                { "CustomField2", createStoreMaster.StoreAdditionalInfo.CustomField2 },
                                { "CustomField3", createStoreMaster.StoreAdditionalInfo.CustomField3 },
                                { "CustomField4", createStoreMaster.StoreAdditionalInfo.CustomField4 },
                                { "CustomField5", createStoreMaster.StoreAdditionalInfo.CustomField5 },
                                { "CustomField6", createStoreMaster.StoreAdditionalInfo.CustomField6 },
                                { "CustomField7", createStoreMaster.StoreAdditionalInfo.CustomField7 },
                                { "CustomField8", createStoreMaster.StoreAdditionalInfo.CustomField8 },
                                { "CustomField9", createStoreMaster.StoreAdditionalInfo.CustomField9 },
                                { "CustomField10", createStoreMaster.StoreAdditionalInfo.CustomField10 },
                                //{"TaxType",createStoreMaster.StoreAdditionalInfo.TaxType},
                                //{"TaxKeyField",createStoreMaster.StoreAdditionalInfo.TaxKeyField},
                                //{"StoreImage",createStoreMaster.StoreAdditionalInfo.StoreImage},
                                //{"IsVATQRCaptureMandatory",createStoreMaster.StoreAdditionalInfo.IsVATQRCaptureMandatory},
                                { "IsAssetEnabled", createStoreMaster.StoreAdditionalInfo.IsAssetEnabled },
                                { "IsSurveyEnabled", createStoreMaster.StoreAdditionalInfo.IsSurveyEnabled },

                                {
                                    "AllowReturnAgainstInvoice",
                                    createStoreMaster.StoreAdditionalInfo.AllowReturnAgainstInvoice
                                },
                                {
                                    "AllowReturnWithSalesOrder",
                                    createStoreMaster.StoreAdditionalInfo.AllowReturnWithSalesOrder
                                },
                                { "WeekOffSun", createStoreMaster.StoreAdditionalInfo.WeekOffSun },
                                { "WeekOffMon", createStoreMaster.StoreAdditionalInfo.WeekOffMon },
                                { "WeekOffTue", createStoreMaster.StoreAdditionalInfo.WeekOffTue },
                                { "WeekOffWed", createStoreMaster.StoreAdditionalInfo.WeekOffWed },
                                { "WeekOffThu", createStoreMaster.StoreAdditionalInfo.WeekOffThu },
                                { "WeekOffFri", createStoreMaster.StoreAdditionalInfo.WeekOffFri },
                                { "WeekOffSat", createStoreMaster.StoreAdditionalInfo.WeekOffSat },
                            };

                            count = await ExecuteNonQueryAsync(storeAdditionalInfoQuery, connection, transaction,
                                storeAdditionalInfoParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception($"storeAdditionalInfo Table Insert Failed. Rows affected: {count}");
                            }
                            foreach (var store in createStoreMaster.StoreCredits)
                            {
                                var storeCreditQuery =
                                    @"INSERT INTO store_credit (UID, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                            store_UID, payment_term_UID, credit_type, credit_limit, temporary_credit, org_UID, distribution_channel_UID, preferred_payment_mode,
                            is_active, is_blocked, blocking_reason_code, blocking_reason_description )VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,
                            @ServerAddTime ,@ServerModifiedTime ,@StoreUID ,@PaymentTermUID ,@CreditType ,@CreditLimit ,@TemporaryCredit ,@OrgUID ,@DistributionChannelUID ,
                            @PreferredPaymentMode ,@IsActive ,@IsBlocked ,@BlockingReasonCode ,@BlockingReasonDescription)";

                                var storeCreditParameters = new Dictionary<string, object?>
                                {
                                    { "UID", store.UID },
                                    { "StoreUID", store.StoreUID },
                                    { "PaymentTermUID", store.PaymentTermUID },
                                    { "CreditType", store.CreditType },
                                    { "CreditLimit", store.CreditLimit },
                                    { "TemporaryCredit", store.TemporaryCredit },
                                    { "OrgUID", store.OrgUID },
                                    { "DistributionChannelUID", store.DistributionChannelUID },
                                    { "PreferredPaymentMode", store.PreferredPaymentMode },
                                    { "IsActive", store.IsActive },
                                    { "IsBlocked", store.IsBlocked },
                                    { "BlockingReasonCode", store.BlockingReasonCode },
                                    { "BlockingReasonDescription", store.BlockingReasonDescription },
                                    { "CreatedBy", store.CreatedBy },
                                    { "ModifiedBy", store.ModifiedBy },
                                    { "CreatedTime", store.CreatedTime },
                                    { "ModifiedTime", store.ModifiedTime },
                                    { "ServerAddTime", store.ServerAddTime },
                                    { "ServerModifiedTime", store.ServerModifiedTime },
                                };

                                count = await ExecuteNonQueryAsync(storeCreditQuery, connection, transaction,
                                    storeCreditParameters);
                                if (count != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table StoreCredit at Row");
                                }
                            }

                            foreach (var storedocument in createStoreMaster.StoreDocuments)
                            {
                                var storeDocumentQuery =
                                    @"INSERT INTO store_document (UID, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                            store_UID, document_type, document_no, valid_from, valid_up_to )
                               VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID, @DocumentType, @DocumentNo, 
                               @ValidFrom, @ValidUpTo)";
                                var storeDocumentParameters = new Dictionary<string, object?>
                                {
                                    { "UID", storedocument.UID },
                                    { "CreatedTime", storedocument.CreatedTime },
                                    { "ModifiedTime", storedocument.ModifiedTime },
                                    { "ServerAddTime", storedocument.ServerAddTime },
                                    { "ServerModifiedTime", storedocument.ServerModifiedTime },
                                    { "CreatedBy", storedocument.CreatedBy },
                                    { "ModifiedBy", storedocument.ModifiedBy },
                                    { "StoreUID", storedocument.StoreUID },
                                    { "DocumentType", storedocument.DocumentType },
                                    { "DocumentNo", storedocument.DocumentNo },
                                    { "ValidFrom", storedocument.ValidFrom },
                                    { "ValidUpTo", storedocument.ValidUpTo },
                                };

                                count = await ExecuteNonQueryAsync(storeDocumentQuery, connection, transaction,
                                    storeDocumentParameters);
                                if (count != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table StoreDocument at Row");
                                }
                            }

                            foreach (var address in createStoreMaster.Addresses)
                            {
                                var addressQuery =
                                    @"INSERT INTO address (UID, created_by, created_time, modified_by, modified_time, server_add_time,
                                                 server_modified_time, type, name, line1, line2, line3, landmark, area, sub_area, zip_code,
                                                 city, country_code, region_code, phone, phone_extension, mobile1, mobile2, email, fax, latitude,
                                                 longitude, altitude, linked_item_UID, linked_item_type, status, state_code, territory_code, pan, aadhar,
                                                 ssn, is_editable, is_default )
                                                   VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@Type,@Name,@Line1,@Line2,
                                                   @Line3,@Landmark,@Area,@SubArea,@ZipCode,@City,@CountryCode,@RegionCode,@Phone,@PhoneExtension,@Mobile1,@Mobile2,@Email,@Fax,
                                                   @Latitude,@Longitude,@Altitude,@LinkedItemUID,@LinkedItemType,@Status,@StateCode,@TerritoryCode,@PAN,@AADHAR,@SSN,@IsEditable,
                                                   @IsDefault);";

                                var addressParameters = new Dictionary<string, object?>
                                {
                                    { "UID", address.UID },
                                    { "CreatedBy", address.CreatedBy },
                                    { "CreatedTime", address.CreatedTime },
                                    { "ModifiedBy", address.ModifiedBy },
                                    { "ModifiedTime", address.ModifiedTime },
                                    { "ServerAddTime", address.ServerAddTime },
                                    { "ServerModifiedTime", address.ServerModifiedTime },
                                    { "Type", address.Type },
                                    { "Name", address.Name },
                                    { "Line1", address.Line1 },
                                    { "Line2", address.Line2 },
                                    { "Line3", address.Line3 },
                                    { "Landmark", address.Landmark },
                                    { "Area", address.Area },
                                    { "SubArea", address.SubArea },
                                    { "ZipCode", address.ZipCode },
                                    { "City", address.City },
                                    { "CountryCode", address.CountryCode },
                                    { "RegionCode", address.RegionCode },
                                    { "Phone", address.Phone },
                                    { "PhoneExtension", address.PhoneExtension },
                                    { "Mobile1", address.Mobile1 },
                                    { "Mobile2", address.Mobile2 },
                                    { "Email", address.Email },
                                    { "Fax", address.Fax },
                                    { "Latitude", address.Latitude },
                                    { "Longitude", address.Longitude },
                                    { "Altitude", address.Altitude },
                                    { "LinkedItemUID", address.LinkedItemUID },
                                    { "LinkedItemType", address.LinkedItemType },
                                    { "Status", address.Status },
                                    { "StateCode", address.StateCode },
                                    { "TerritoryCode", address.TerritoryCode },
                                    { "PAN", address.PAN },
                                    { "AADHAR", address.AADHAR },
                                    { "SSN", address.SSN },
                                    { "IsEditable", address.IsEditable },
                                    { "IsDefault", address.IsDefault }
                                };

                                count = await ExecuteNonQueryAsync(addressQuery, connection, transaction,
                                    addressParameters);
                                if (count != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table Address at Row");
                                }
                            }

                            foreach (var contact in createStoreMaster.Contacts)
                            {
                                var contactQuery =
                                    @"INSERT INTO contact (UID, created_by, created_time, modified_by, modified_time, server_add_time,
                                                 server_modified_time, title, name, phone, phone_extension, description, designation, mobile,
                                                 email, email2, email3, invoice_for_email1, invoice_for_email2, invoice_for_email3, fax, linked_item_UID,
                                                 linked_item_type, is_default, is_editable, enabled_for_invoice_email, enabled_for_docket_email, enabled_for_promo_email, is_email_cc )
                                                   VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Title, @Name, @Phone,
                                                   @PhoneExtension, @Description, @Designation, @Mobile, @Email, @Email2, @Email3, @InvoiceForEmail1, @InvoiceForEmail2, 
                                                  @InvoiceForEmail3, @Fax, @LinkedItemUID, @LinkedItemType, @IsDefault, @IsEditable, @EnabledForInvoiceEmail, 
                                                   @EnabledForDocketEmail, @EnabledForPromoEmail, @IsEmailCC);";

                                var contactParameters = new Dictionary<string, object?>
                                {
                                    { "UID", contact.UID },
                                    { "CreatedTime", contact.CreatedTime },
                                    { "ModifiedTime", contact.ModifiedTime },
                                    { "ServerAddTime", contact.ServerAddTime },
                                    { "ServerModifiedTime", contact.ServerModifiedTime },
                                    { "CreatedBy", contact.CreatedBy },
                                    { "ModifiedBy", contact.ModifiedBy },
                                    { "Title", contact.Title },
                                    { "Name", contact.Name },
                                    { "Phone", contact.Phone },
                                    { "PhoneExtension", contact.PhoneExtension },
                                    { "Description", contact.Description },
                                    { "Designation", contact.Designation },
                                    { "Mobile", contact.Mobile },
                                    { "Email", contact.Email },
                                    { "Email2", contact.Email2 },
                                    { "Email3", contact.Email3 },
                                    { "InvoiceForEmail1", contact.InvoiceForEmail1 },
                                    { "InvoiceForEmail2", contact.InvoiceForEmail2 },
                                    { "InvoiceForEmail3", contact.InvoiceForEmail3 },
                                    { "Fax", contact.Fax },
                                    { "LinkedItemUID", contact.LinkedItemUID },
                                    { "LinkedItemType", contact.LinkedItemType },
                                    { "IsDefault", contact.IsDefault },
                                    { "IsEditable", contact.IsEditable },
                                    { "EnabledForInvoiceEmail", contact.EnabledForInvoiceEmail },
                                    { "EnabledForDocketEmail", contact.EnabledForDocketEmail },
                                    { "EnabledForPromoEmail", contact.EnabledForPromoEmail },
                                    { "IsEmailCC", contact.IsEmailCC }
                                };

                                count = await ExecuteNonQueryAsync(contactQuery, connection, transaction,
                                    contactParameters);
                                if (count != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table Contact at Row");
                                }
                            }

                            transaction.Commit();
                            return count;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction?.Rollback();
                            }
                            catch
                            {
                                // Transaction already rolled back, ignore
                            }
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO> SelectStoreMasterByUID(
            string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "UID", UID }
            };
            var sql = @"SELECT 
    s.uid AS ""StoreUID"",
    s.created_by AS ""StoreCreatedBy"",
    s.created_time AS ""StoreCreatedTime"",
    s.modified_by AS ""StoreModifiedBy"",
    s.modified_time AS ""StoreModifiedTime"",
    s.server_add_time AS ""StoreServerAddTime"",
    s.server_modified_time AS ""StoreServerModifiedTime"",
    s.company_uid AS ""StoreCompanyUID"",
    s.code AS ""StoreCode"",
    s.number AS ""StoreNumber"",
    s.name AS ""StoreName"",
    s.alias_name AS ""StoreAliasName"",
    s.legal_name AS ""StoreLegalName"",
    s.type AS ""StoreType"",
    s.bill_to_store_uid AS ""StoreBillToStoreUID"",
    s.ship_to_store_uid AS ""StoreShipToStoreUID"",
    s.sold_to_store_uid AS ""StoreSoldToStoreUID"",
    s.status AS ""StoreStatus"",
    s.is_active AS ""StoreIsActive"",
    s.store_class AS ""StoreClass"",
    s.store_rating AS ""StoreRating"",
    s.is_blocked AS ""StoreIsBlocked"",
    s.blocked_reason_code AS ""StoreBlockedReasonCode"",
    s.blocked_reason_description AS ""StoreBlockedReasonDescription"",
    s.created_by_emp_uid AS ""StoreCreatedByEmpUID"",
    s.created_by_job_position_uid AS ""StoreCreatedByJobPositionUID"",
    s.country_uid AS ""StoreCountryUID"",
    s.region_uid AS ""StoreRegionUID"",
    s.city_uid AS ""StoreCityUID"",
    s.source AS ""StoreSource"",
    s.outlet_name AS ""StoreOutletName"",
    s.blocked_by_emp_uid AS ""StoreBlockedByEmpUID"",
    s.arabic_name AS ""StoreArabicName"",

    
    sai.uid AS ""StoreAdditionalInfoUID"",
    sai.created_by AS ""StoreAdditionalInfoCreatedBy"",
    sai.created_time AS ""StoreAdditionalInfoCreatedTime"",
    sai.modified_by AS ""StoreAdditionalInfoModifiedBy"",
    sai.modified_time AS ""StoreAdditionalInfoModifiedTime"",
    sai.server_add_time AS ""StoreAdditionalInfoServerAddTime"",
    sai.server_modified_time AS ""StoreAdditionalInfoServerModifiedTime"",
    sai.store_uid AS ""StoreAdditionalInfoStoreUID"",
    sai.order_type AS  ""StoreAdditionalInfoOrderType"",
    sai.is_promotions_block AS  ""StoreAdditionalInfoPromotionsBlock"",
    sai.customer_start_date AS  ""StoreAdditionalInfoCustomerStartDate"",
    sai.customer_end_date AS  ""StoreAdditionalInfoCustomerEndDate"",
    s.school_warehouse AS  ""StoreSchoolWarehouse"",
    sai.purchase_order_number AS  ""StoreAdditionalInfoPurchaseOrderNumber"",
    sai.delivery_docket_is_purchase_order_required AS  ""StoreAdditionalInfoDocketRequiresPurchaseOrder"",
    sai.is_with_printed_invoices AS  ""StoreAdditionalInfoWithPrintedInvoices"",
    sai.is_capture_signature_required AS  ""StoreAdditionalInfoCaptureSignatureRequired"",
    sai.is_always_printed AS  ""StoreAdditionalInfoAlwaysPrinted"",
    sai.building_delivery_code AS  ""StoreAdditionalInfoBuildingDeliveryCode"",
    sai.delivery_information AS  ""StoreAdditionalInfoDeliveryInformation"",
    sai.is_stop_delivery AS  ""StoreAdditionalInfoStopDelivery"",
    sai.is_forecast_top_up_qty AS  ""StoreAdditionalInfoForecastTopUpQuantity"",
    sai.is_temperature_check AS  ""StoreAdditionalInfoTemperatureCheck"",
    sai.invoice_start_date AS  ""StoreAdditionalInfoInvoiceStartDate"",
    sai.invoice_end_date AS  ""StoreAdditionalInfoInvoiceEndDate"",
    sai.invoice_format AS  ""StoreAdditionalInfoInvoiceFormat"",
    sai.invoice_delivery_method AS  ""StoreAdditionalInfoInvoiceDeliveryMethod"",
    sai.display_delivery_docket AS  ""StoreAdditionalInfoDisplayDeliveryDocket"",
    sai.display_price AS  ""StoreAdditionalInfoDisplayPrice"",
    sai.show_cust_po AS  ""StoreAdditionalInfoShowCustomerPO"",
    sai.invoice_text AS  ""StoreAdditionalInfoInvoiceText"",
    sai.invoice_frequency AS  ""StoreAdditionalInfoInvoiceFrequency"",
    sai.stock_credit_is_purchase_order_required AS  ""StoreAdditionalInfoStockCreditRequiresPurchaseOrder"",
    sai.admin_fee_per_billing_cycle AS  ""StoreAdditionalInfoAdminFeePerBillingCycle"",
    sai.admin_fee_per_delivery AS  ""StoreAdditionalInfoAdminFeePerDelivery"",
    sai.late_payment_fee AS  ""StoreAdditionalInfoLatePaymentFee"",
    sai.drawer AS  ""StoreAdditionalInfoDrawer"",
    sai.bank_uid AS  ""StoreAdditionalInfoBankUID"",
    sai.bank_account AS  ""StoreAdditionalInfoBankAccount"",
    sai.mandatory_po_number AS  ""StoreAdditionalInfoMandatoryPONumber"",
    sai.is_store_credit_capture_signature_required AS  ""StoreAdditionalInfoStoreCreditCaptureSignatureRequired"",
    sai.store_credit_always_printed AS  ""StoreAdditionalInfoStoreCreditAlwaysPrinted"",
    sai.is_dummy_customer AS  ""StoreAdditionalInfoDummyCustomer"",
    sai.default_run AS  ""StoreAdditionalInfoDefaultRun"",
    s.prospect_emp_uid AS  ""StoreProspectEmpUID"",
    sai.is_foc_customer AS  ""StoreAdditionalInfoFOCCustomer"",
    sai.rss_show_price AS  ""StoreAdditionalInfoRSSShowPrice"",
    sai.rss_show_payment AS  ""StoreAdditionalInfoRSSShowPayment"",
    sai.rss_show_credit AS  ""StoreAdditionalInfoRSSShowCredit"",
    sai.rss_show_invoice AS  ""StoreAdditionalInfoRSSShowInvoice"",
    sai.rss_is_active AS  ""StoreAdditionalInfoRSSIsActive"",
    sai.rss_delivery_instruction_status AS  ""StoreAdditionalInfoRSSDeliveryInstructionStatus"",
    sai.rss_time_spent_on_rss_portal AS  ""StoreAdditionalInfoRSSTimeSpentOnPortal"",
    sai.rss_order_placed_in_rss AS  ""StoreAdditionalInfoRSSOrderPlacedInRSS"",
    sai.rss_avg_orders_per_week AS  ""StoreAdditionalInfoRSSAvgOrdersPerWeek"",
    sai.rss_total_order_value AS  ""StoreAdditionalInfoRSSTotalOrderValue"",
    sai.allow_force_check_in AS  ""StoreAdditionalInfoAllowForceCheckIn"",
    sai.is_manual_edit_allowed AS  ""StoreAdditionalInfoManualEditAllowed"",
    sai.can_update_lat_long AS  ""StoreAdditionalInfoCanUpdateLatLong"",
    s.is_tax_applicable AS  ""StoreIsTaxApplicable"",
    s.tax_doc_number AS  ""StoreTaxDocNumber"",
    s.is_tax_doc_verified AS  ""StoreIsTaxDocVerified"",
    sai.allow_good_return AS  ""StoreAdditionalInfoAllowGoodReturn"",
    sai.allow_bad_return AS  ""StoreAdditionalInfoAllowBadReturn"",
    sai.allow_replacement AS  ""StoreAdditionalInfoAllowReplacement"",
    sai.is_invoice_cancellation_allowed AS  ""StoreAdditionalInfoInvoiceCancellationAllowed"",
    sai.is_delivery_note_required AS  ""StoreAdditionalInfoDeliveryNoteRequired"",
    sai.e_invoicing_enabled AS  ""StoreAdditionalInfoEInvoicingEnabled"",
    sai.image_recognition_enabled AS  ""StoreAdditionalInfoImageRecognitionEnabled"",
    sai.max_outstanding_invoices AS  ""StoreAdditionalInfoMaxOutstandingInvoices"",
    sai.negative_invoice_allowed AS  ""StoreAdditionalInfoNegativeInvoiceAllowed"",
    sai.delivery_mode AS  ""StoreAdditionalInfoDeliveryMode"",
    s.store_size AS  ""StoreStoreSize"",
    sai.visit_frequency AS  ""StoreAdditionalInfoVisitFrequency"",
    sai.shipping_contact_same_as_store AS  ""StoreAdditionalInfoShippingContactSameAsStore"",
    sai.billing_address_same_as_shipping AS  ""StoreAdditionalInfoBillingAddressSameAsShipping"",
    sai.payment_mode AS  ""StoreAdditionalInfoPaymentMode"",
    sai.price_type AS  ""StoreAdditionalInfoPriceType"",
    sai.average_monthly_income AS  ""StoreAdditionalInfoAverageMonthlyIncome"",
    sai.default_bank_uid AS  ""StoreAdditionalInfoDefaultBankUID"",
    sai.account_number AS  ""StoreAdditionalInfoAccountNumber"",
    sai.no_of_cash_counters AS  ""StoreAdditionalInfoNumberOfCashCounters"",
    sai.custom_field1 AS  ""StoreAdditionalInfoCustomField1"",
    sai.custom_field2 AS  ""StoreAdditionalInfoCustomField2"",
    sai.custom_field3 AS  ""StoreAdditionalInfoCustomField3"",
    sai.custom_field4 AS  ""StoreAdditionalInfoCustomField4"",
    sai.custom_field5 AS  ""StoreAdditionalInfoCustomField5"",
    sai.custom_field6 AS  ""StoreAdditionalInfoCustomField6"",
    sai.custom_field7 AS  ""StoreAdditionalInfoCustomField7"",
    sai.custom_field8 AS  ""StoreAdditionalInfoCustomField8"",
    sai.custom_field9 AS  ""StoreAdditionalInfoCustomField9"",
    sai.custom_field10 AS  ""StoreAdditionalInfoCustomField10"",
    s.tax_type AS  ""StoreTaxType"",
    s.tax_key_field AS  ""StoreTaxKeyField"",
    s.store_image AS  ""StoreStoreImage"",
    s.is_vat_qr_capture_mandatory AS  ""StoreIsVATQRCaptureMandatory"",
    sai.is_asset_enabled AS  ""StoreAdditionalInfoAssetEnabled"",
    sai.is_survey_enabled AS  ""StoreAdditionalInfoSurveyEnabled"",
    sai.allow_return_against_invoice AS  ""StoreAdditionalInfoAllowReturnAgainstInvoice"",
    sai.allow_return_with_sales_order AS  ""StoreAdditionalInfoAllowReturnWithSalesOrder"",
    sai.week_off_sun AS  ""StoreAdditionalInfoWeekOffSun"",
    sai.week_off_mon AS  ""StoreAdditionalInfoWeekOffMon"",
    sai.week_off_tue AS  ""StoreAdditionalInfoWeekOffTue"",
    sai.week_off_wed AS  ""StoreAdditionalInfoWeekOffWed"",
    sai.week_off_thu AS  ""StoreAdditionalInfoWeekOffThu"",
    sai.week_off_fri AS  ""StoreAdditionalInfoWeekOffFri"",
    sai.week_off_sat AS  ""StoreAdditionalInfoWeekOffSat"",

    sd.uid AS ""StoreDocumentUID"",
    sd.created_by AS ""StoreDocumentCreatedBy"",
    sd.created_time AS ""StoreDocumentCreatedTime"",
    sd.modified_by AS ""StoreDocumentModifiedBy"",
    sd.modified_time AS ""StoreDocumentModifiedTime"",
    sd.server_add_time AS ""StoreDocumentServerAddTime"",
    sd.server_modified_time AS ""StoreDocumentServerModifiedTime"",
    sd.store_uid AS ""StoreDocumentStoreUID"",
    sd.document_type AS ""StoreDocumentDocumentType"",
    sd.document_no AS ""StoreDocumentDocumentNo"",
    sd.valid_from AS ""StoreDocumentValidFrom"",
    sd.valid_up_to AS ""StoreDocumentValidUpTo"",

    
    sc.uid AS ""StoreCreditUID"",
    sc.created_by AS ""StoreCreditCreatedBy"",
    sc.created_time AS ""StoreCreditCreatedTime"",
    sc.modified_by AS ""StoreCreditModifiedBy"",
    sc.modified_time AS ""StoreCreditModifiedTime"",
    sc.server_add_time AS ""StoreCreditServerAddTime"",
    sc.server_modified_time AS ""StoreCreditServerModifiedTime"",
    sc.store_uid AS ""StoreCreditStoreUID"",
    sc.payment_term_uid AS ""StoreCreditPaymentTermUID"",
    sc.credit_type AS ""StoreCreditCreditType"",
    sc.credit_limit AS ""StoreCreditCreditLimit"",
    sc.temporary_credit AS ""StoreCreditTemporaryCredit"",
    sc.org_uid AS ""StoreCreditOrgUID"",
    sc.distribution_channel_uid AS ""StoreCreditDistributionChannelUID"",
    sc.preferred_payment_mode AS ""StoreCreditPreferredPaymentMode"",
    sc.is_active AS ""StoreCreditIsActive"",
    sc.is_blocked AS ""StoreCreditIsBlocked"",
    sc.blocking_reason_code AS ""StoreCreditBlockingReasonCode"",
    sc.blocking_reason_description AS ""StoreCreditBlockingReasonDescription"",
    sc.price_list AS ""StoreCreditPriceList"",
    sc.disabled AS ""StoreCreditDisabled"",
    sc.outstanding_invoices AS ""StoreCreditOutstandingInvoices"",
    sc.credit_days AS ""StoreCreditCreditDays"",
    sc.temporary_credit_days AS ""StoreCreditTemporaryCreditDays"",
    sc.division_org_uid AS ""StoreCreditDivisionOrgUID"",

    ad.uid AS ""AddressUID"",
    ad.created_by AS ""AddressCreatedBy"",
    ad.created_time AS ""AddressCreatedTime"",
    ad.modified_by AS ""AddressModifiedBy"",
    ad.modified_time AS ""AddressModifiedTime"",
    ad.server_add_time AS ""AddressServerAddTime"",
    ad.server_modified_time AS ""AddressServerModifiedTime"",
    ad.type AS ""AddressType"",
    ad.name AS ""AddressName"",
    ad.line1 AS ""AddressLine1"",
    ad.line2 AS ""AddressLine2"",
    ad.line3 AS ""AddressLine3"",
    ad.line4 AS ""AddressLine4"",
    ad.landmark AS ""AddressLandmark"",
    ad.area AS ""AddressArea"",
    ad.sub_area AS ""AddressSubArea"",
    ad.zip_code AS ""AddressZipCode"",
    ad.city AS ""AddressCity"",
    ad.country_code AS ""AddressCountryCode"",
    ad.region_code AS ""AddressRegionCode"",
    ad.phone AS ""AddressPhone"",
    ad.phone_extension AS ""AddressPhoneExtension"",
    ad.mobile1 AS ""AddressMobile1"",
    ad.mobile2 AS ""AddressMobile2"",
    ad.email AS ""AddressEmail"",
    ad.fax AS ""AddressFax"",
    ad.latitude AS ""AddressLatitude"",
    ad.longitude AS ""AddressLongitude"",
    ad.altitude AS ""AddressAltitude"",
    ad.linked_item_uid AS ""AddressLinkedItemUID"",
    ad.linked_item_type AS ""AddressLinkedItemType"",
    ad.status AS ""AddressStatus"",
    ad.state_code AS ""AddressStateCode"",
    ad.territory_code AS ""AddressTerritoryCode"",
    ad.pan AS ""AddressPAN"",
    ad.aadhar AS ""AddressAadhar"",
    ad.ssn AS ""AddressSSN"",
    ad.is_editable AS ""AddressIsEditable"",
    ad.is_default AS ""AddressIsDefault"",
    ad.info AS ""AddressInfo"",

    
    ct.uid AS ""ContactUID"",
    ct.created_by AS ""ContactCreatedBy"",
    ct.created_time AS ""ContactCreatedTime"",
    ct.modified_by AS ""ContactModifiedBy"",
    ct.modified_time AS ""ContactModifiedTime"",
    ct.server_add_time AS ""ContactServerAddTime"",
    ct.server_modified_time AS ""ContactServerModifiedTime"",
    ct.title AS ""ContactTitle"",
    ct.name AS ""ContactName"",
    ct.phone AS ""ContactPhone"",
    ct.phone_extension AS ""ContactPhoneExtension"",
    ct.description AS ""ContactDescription"",
    ct.designation AS ""ContactDesignation"",
    ct.mobile AS ""ContactMobile"",
    ct.email AS ""ContactEmail"",
    ct.email2 AS ""ContactEmail2"",
    ct.email3 AS ""ContactEmail3"",
    ct.invoice_for_email1 AS ""ContactInvoiceForEmail1"",
    ct.invoice_for_email2 AS ""ContactInvoiceForEmail2"",
    ct.invoice_for_email3 AS ""ContactInvoiceForEmail3"",
    ct.fax AS ""ContactFax"",
    ct.linked_item_uid AS ""ContactLinkedItemUID"",
    ct.linked_item_type AS ""ContactLinkedItemType"",
    ct.is_default AS ""ContactIsDefault"",
    ct.is_editable AS ""ContactIsEditable"",
    ct.enabled_for_invoice_email AS ""ContactEnabledForInvoiceEmail"",
    ct.enabled_for_docket_email AS ""ContactEnabledForDocketEmail"",
    ct.enabled_for_promo_email AS ""ContactEnabledForPromoEmail"",
    ct.is_email_cc AS ""ContactIsEmailCC"",
    ct.mobile2 AS ""ContactMobile2""

    
    FROM 
        store s
    LEFT JOIN 
        store_additional_info sai ON s.uid = sai.store_uid
    LEFT JOIN 
        store_document sd ON sd.store_uid = s.uid
    LEFT JOIN 
        store_credit sc ON sc.store_uid = s.uid
    LEFT JOIN 
        address ad ON ad.linked_item_uid = s.uid
    LEFT JOIN 
        contact ct ON ct.linked_item_uid = s.uid
    WHERE 
        s.uid = @UID";

            DataTable dt = await ExecuteQueryDataTableAsync(sql.ToString(), parameters);
            Dictionary<string, string> storeColumnMappings = new Dictionary<string, string>
            {
                { "StoreUID", "UID" },
                { "StoreCreatedBy", "CreatedBy" },
                { "StoreCreatedTime", "CreatedTime" },
                { "StoreModifiedBy", "ModifiedBy" },
                { "StoreModifiedTime", "ModifiedTime" },
                { "StoreServerAddTime", "ServerAddTime" },
                { "StoreServerModifiedTime", "ServerModifiedTime" },
                { "StoreCompanyUID", "CompanyUID" },
                { "StoreName", "Name" },
                { "StoreType", "Type" },
            };
            Dictionary<string, string> storeAdditionalInfoColumnMappings = new Dictionary<string, string>
            {
                { "StoreAdditionalInfoUID", "UID" },
                { "StoreAdditionalInfoCreatedBy", "CreatedBy" },
                { "StoreAdditionalInfoCreatedTime", "CreatedTime" },
                { "StoreAdditionalInfoModifiedBy", "ModifiedBy" },
                { "StoreAdditionalInfoModifiedTime", "ModifiedTime" },
                { "StoreAdditionalInfoServerAddTime", "ServerAddTime" },
                { "StoreAdditionalInfoServerModifiedTime", "ServerModifiedTime" },
                { "StoreAdditionalInfoStoreUID", "StoreUID" },
            };
            Dictionary<string, string> storeDocumentColumnMappings = new Dictionary<string, string>
            {
                { "StoreDocumentUID", "UID" },
                { "StoreDocumentCreatedBy", "CreatedBy" },
                { "StoreDocumentCreatedTime", "CreatedTime" },
                { "StoreDocumentModifiedBy", "ModifiedBy" },
                { "StoreDocumentModifiedTime", "ModifiedTime" },
                { "StoreDocumentServerAddTime", "ServerAddTime" },
                { "StoreDocumentServerModifiedTime", "ServerModifiedTime" },
                { "StoreDocumentStoreUID", "StoreUID" },
            };
            Dictionary<string, string> storeCreditColumnMappings = new Dictionary<string, string>
            {
                { "StoreCreditUID", "UID" },
                { "StoreCreditCreatedBy", "CreatedBy" },
                { "StoreCreditCreatedTime", "CreatedTime" },
                { "StoreCreditModifiedBy", "ModifiedBy" },
                { "StoreCreditModifiedTime", "ModifiedTime" },
                { "StoreCreditServerAddTime", "ServerAddTime" },
                { "StoreCreditServerModifiedTime", "ServerModifiedTime" },
                { "StoreCreditStoreUID", "StoreUID" },
                { "StoreCreditPaymentTermUID", "PaymentTermUID" },
                { "StoreCreditCreditType", "CreditType" },
                { "StoreCreditCreditLimit", "CreditLimit" },
                { "StoreCreditTemporaryCredit", "TemporaryCredit" },
                { "StoreCreditOrgUID", "OrgUID" },
                { "StoreCreditDistributionChannelUID", "DistributionChannelUID" },
                { "StoreCreditPreferredPaymentMode", "PreferredPaymentMode" },
                { "StoreCreditIsActive", "IsActive" },
                { "StoreCreditIsBlocked", "IsBlocked" },
                { "StoreCreditBlockingReasonCode", "BlockingReasonCode" },
                { "StoreCreditBlockingReasonDescription", "BlockingReasonDescription" },
                { "StoreCreditPriceList", "PriceList" },
                { "StoreCreditDisabled", "Disabled" },
                { "StoreCreditOutstandingInvoices", "OutstandingInvoices" },
                { "StoreCreditCreditDays", "CreditDays" },
                { "StoreCreditTemporaryCreditDays", "TemporaryCreditDays" },
                { "StoreCreditDivisionOrgUID", "DivisionOrgUID" },
            };
            Dictionary<string, string> addressColumnMappings = new Dictionary<string, string>
            {
                { "AddressUID", "UID" },
                { "AddressCreatedBy", "CreatedBy" },
                { "AddressCreatedTime", "CreatedTime" },
                { "AddressModifiedBy", "ModifiedBy" },
                { "AddressModifiedTime", "ModifiedTime" },
                { "AddressServerAddTime", "ServerAddTime" },
                { "AddressServerModifiedTime", "ServerModifiedTime" },
                { "AddressType", "Type" },
                { "AddressName", "Name" },
                { "AddressIsDefault", "IsDefault" },
            };
            Dictionary<string, string> contactColumnMappings = new Dictionary<string, string>
            {
                { "ContactUID", "UID" },
                { "ContactCreatedBy", "CreatedBy" },
                { "ContactCreatedTime", "CreatedTime" },
                { "ContactModifiedBy", "ModifiedBy" },
                { "ContactModifiedTime", "ModifiedTime" },
                { "ContactServerAddTime", "ServerAddTime" },
                { "ContactServerModifiedTime", "ServerModifiedTime" },
                { "ContactName", "Name" },
                { "ContacIsDefault", "IsDefault" },
            };
            Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO storeDco = new StoreViewModelDCO();
            List<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO> storeMasterViewModelList = new List<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO>();
            if (dt != null && dt.Rows.Count > 0)
            {
                StoreViewModelDCO1 storeViewModelDCO = new StoreViewModelDCO1();
                storeViewModelDCO.StoreCredits = new List<IStoreCredit>();
                storeViewModelDCO.StoreDocuments = new List<IStoreDocument>();
                storeViewModelDCO.addresses = new List<IAddress>();
                storeViewModelDCO.Contacts = new List<IContact>();


                foreach (DataRow row in dt.Rows)
                {
                    Type storeType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStore>()
                        .GetType();
                    Type storeAdditionalInfoType = _serviceProvider
                        .GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>().GetType();
                    Type storeCreditType = _serviceProvider
                        .GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreCredit>().GetType();
                    Type storeDocumentType = _serviceProvider
                        .GetRequiredService<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>().GetType();
                    Type addressType = _serviceProvider
                        .GetRequiredService<Winit.Modules.Address.Model.Interfaces.IAddress>().GetType();
                    Type contactType = _serviceProvider
                        .GetRequiredService<Winit.Modules.Contact.Model.Interfaces.IContact>().GetType();
                    var storeList =
                        ConvertDataTableToObject<Winit.Modules.Store.Model.Interfaces.IStore>(row, storeColumnMappings,
                            storeType);
                    var storeAdditionalInfoList =
                        ConvertDataTableToObject<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>(row,
                            storeAdditionalInfoColumnMappings, storeAdditionalInfoType);
                    var storeCreditList =
                        ConvertDataTableToObject<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(row,
                            storeCreditColumnMappings, storeCreditType);
                    var storeDocumentList =
                        ConvertDataTableToObject<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>(row,
                            storeDocumentColumnMappings, storeDocumentType);
                    var addressList =
                        ConvertDataTableToObject<Winit.Modules.Address.Model.Interfaces.IAddress>(row,
                            addressColumnMappings, addressType);
                    var contactList =
                        ConvertDataTableToObject<Winit.Modules.Contact.Model.Interfaces.IContact>(row,
                            contactColumnMappings, contactType);
                    if (storeList != null && storeAdditionalInfoList != null && storeCreditList != null &&
                        storeDocumentList != null && addressList != null && contactList != null)
                    {
                        //var storeViewModelDCO = new StoreViewModelDCO1
                        //{
                        //    store = (Winit.Modules.Store.Model.Interfaces.IStore)storeList,
                        //    StoreAdditionalInfo = (Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo)storeAdditionalInfoList,
                        //    StoreCredits = new List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> { storeCreditList },
                        //    StoreDocuments = new List<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> { storeDocumentList },
                        //    addresses = new List<Winit.Modules.Address.Model.Interfaces.IAddress> { addressList },
                        //    Contacts = new List<Winit.Modules.Contact.Model.Interfaces.IContact> { contactList }
                        //};
                        //storeMasterViewModelList.Add(storeViewModelDCO);

                        storeViewModelDCO.store = (Winit.Modules.Store.Model.Interfaces.IStore)storeList;
                        storeViewModelDCO.StoreAdditionalInfo =
                            (Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo)storeAdditionalInfoList;
                        // Add store credit only if not already added (avoid duplicates from JOIN)
                        var newStoreCredit = (Winit.Modules.Store.Model.Interfaces.IStoreCredit)storeCreditList;
                        if (newStoreCredit != null && !storeViewModelDCO.StoreCredits.Any(sc => sc.UID == newStoreCredit.UID))
                        {
                            storeViewModelDCO.StoreCredits.Add(newStoreCredit);
                        }
                        // Add store document only if not already added (avoid duplicates from JOIN)
                        var newStoreDocument = (Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument)storeDocumentList;
                        if (newStoreDocument != null && !storeViewModelDCO.StoreDocuments.Any(sd => sd.UID == newStoreDocument.UID))
                        {
                            storeViewModelDCO.StoreDocuments.Add(newStoreDocument);
                        }
                        
                        // Add address only if not already added (avoid duplicates from JOIN)
                        var newAddress = (Winit.Modules.Address.Model.Interfaces.IAddress)addressList;
                        if (newAddress != null && !storeViewModelDCO.addresses.Any(ad => ad.UID == newAddress.UID))
                        {
                            storeViewModelDCO.addresses.Add(newAddress);
                        }
                        
                        // Add contact only if not already added (avoid duplicates from JOIN)
                        var newContact = (Winit.Modules.Contact.Model.Interfaces.IContact)contactList;
                        if (newContact != null && !storeViewModelDCO.Contacts.Any(ct => ct.UID == newContact.UID))
                        {
                            storeViewModelDCO.Contacts.Add(newContact);
                        }
                    }
                }
                storeMasterViewModelList.Add(storeViewModelDCO);
            }
            return storeMasterViewModelList.FirstOrDefault();
        }

        public async Task<int> UpdateStoreMaster(
            Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO updateStoreMaster)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var storeOrderQuery = @"UPDATE store 
                            SET 
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
                                modified_by = @ModifiedBy,
                                created_by_job_position_uid = @CreatedByJobPositionUID,
                                country_uid = @CountryUID,
                                city_uid = @CityUID,
                                source = @Source,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                arabic_name = @ArabicName,
                                outlet_name = @OutletName,
                                blocked_by_emp_uid = @BlockedByEmpUID 
                            WHERE 
                                uid = @UID";
                            var storeParameters = new Dictionary<string, object?>
                            {
                                { "Number", updateStoreMaster.store.Number },
                                { "Name", updateStoreMaster.store.Name },
                                { "UID", updateStoreMaster.store.UID },
                                { "AliasName", updateStoreMaster.store.AliasName },
                                { "LegalName", updateStoreMaster.store.LegalName },
                                { "BillToStoreUID", updateStoreMaster.store.BillToStoreUID },
                                { "ShipToStoreUID", updateStoreMaster.store.ShipToStoreUID },
                                { "SoldToStoreUID", updateStoreMaster.store.SoldToStoreUID },
                                { "Status", updateStoreMaster.store.Status },
                                { "IsActive", updateStoreMaster.store.IsActive },
                                { "StoreClass", updateStoreMaster.store.StoreClass },
                                { "StoreRating", updateStoreMaster.store.StoreRating },
                                { "IsBlocked", updateStoreMaster.store.IsBlocked },
                                { "BlockedReasonCode", updateStoreMaster.store.BlockedReasonCode },
                                { "BlockedReasonDescription", updateStoreMaster.store.BlockedReasonDescription },
                                { "CreatedByEmpUID", updateStoreMaster.store.CreatedByEmpUID },
                                { "CreatedByJobPositionUID", updateStoreMaster.store.CreatedByJobPositionUID },
                                { "CountryUID", updateStoreMaster.store.CountryUID },
                                { "CityUID", updateStoreMaster.store.CityUID },
                                { "Source", updateStoreMaster.store.Source },
                                { "ModifiedBy", updateStoreMaster.store.ModifiedBy },
                                { "ModifiedTime", updateStoreMaster.store.ModifiedTime },
                                { "ServerModifiedTime", updateStoreMaster.store.ServerModifiedTime },
                                { "Type", updateStoreMaster.store.Type },
                                { "ArabicName", updateStoreMaster.store.ArabicName },
                                { "OutletName", updateStoreMaster.store.OutletName },
                                { "BlockedByEmpUID", updateStoreMaster.store.BlockedByEmpUID },
                            };


                            count = await ExecuteNonQueryAsync(storeOrderQuery, connection, transaction,
                                storeParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("Store Update failed");
                            }
                            var storeAdditionalInfoQuery = @"UPDATE store_additional_info 
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
                                                        is_fore_cast_top_up_qty = @IsForeCastTopUpQty,
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
                                                        late_payement_fee = @LatePaymentFee,
                                                        drawer = @Drawer,
                                                        bank_uid = @BankUID,
                                                        bank_account = @BankAccount,
                                                        mandatory_po_number = @MandatoryPONumber,
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
                                                        is_manaual_edit_allowed = @IsManualEditAllowed,
                                                        can_update_lat_long = @CanUpdateLatLong,
                                                        allow_good_return = @AllowGoodReturn,
                                                        allow_bad_return = @AllowBadReturn,
                                                        allow_replacement = @AllowReplacement,
                                                        is_invoice_cancellation_allowed = @IsInvoiceCancellationAllowed,
                                                        is_delivery_note_required = @IsDeliveryNoteRequired,
                                                        e_invoicing_enabled = @EInvoicingEnabled,
                                                        image_recognization_enabled = @ImageRecognizationEnabled,
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
                                                        week_off_sat = @WeekOffSat
                                                    WHERE 
                                                        uid = @UID";

                            var storeAdditionalInfoParameters = new Dictionary<string, object?>
                            {
                                { "UID", updateStoreMaster.StoreAdditionalInfo.UID },
                                { "ModifiedBy", updateStoreMaster.StoreAdditionalInfo.ModifiedBy },
                                { "ModifiedTime", updateStoreMaster.StoreAdditionalInfo.ModifiedTime },
                                { "ServerModifiedTime", updateStoreMaster.StoreAdditionalInfo.ServerModifiedTime },
                                { "StoreUID", updateStoreMaster.StoreAdditionalInfo.StoreUID },
                                { "OrderType", updateStoreMaster.StoreAdditionalInfo.OrderType },
                                { "IsPromotionsBlock", updateStoreMaster.StoreAdditionalInfo.IsPromotionsBlock },
                                { "CustomerStartDate", updateStoreMaster.StoreAdditionalInfo.CustomerStartDate },
                                { "CustomerEndDate", updateStoreMaster.StoreAdditionalInfo.CustomerEndDate },
                                //{"SchoolWarehouse",updateStoreMaster.StoreAdditionalInfo.SchoolWarehouse},
                                { "PurchaseOrderNumber", updateStoreMaster.StoreAdditionalInfo.PurchaseOrderNumber },
                                {
                                    "DeliveryDocketIsPurchaseOrderRequired",
                                    updateStoreMaster.StoreAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired
                                },
                                {
                                    "IsWithPrintedInvoices", updateStoreMaster.StoreAdditionalInfo.IsWithPrintedInvoices
                                },
                                {
                                    "IsCaptureSignatureRequired",
                                    updateStoreMaster.StoreAdditionalInfo.IsCaptureSignatureRequired
                                },
                                { "IsAlwaysPrinted", updateStoreMaster.StoreAdditionalInfo.IsAlwaysPrinted },
                                { "BuildingDeliveryCode", updateStoreMaster.StoreAdditionalInfo.BuildingDeliveryCode },
                                { "DeliveryInformation", updateStoreMaster.StoreAdditionalInfo.DeliveryInformation },
                                { "IsStopDelivery", updateStoreMaster.StoreAdditionalInfo.IsStopDelivery },
                                { "IsForeCastTopUpQty", updateStoreMaster.StoreAdditionalInfo.IsForeCastTopUpQty },
                                { "IsTemperatureCheck", updateStoreMaster.StoreAdditionalInfo.IsTemperatureCheck },
                                { "InvoiceStartDate", updateStoreMaster.StoreAdditionalInfo.InvoiceStartDate },
                                { "InvoiceEndDate", updateStoreMaster.StoreAdditionalInfo.InvoiceEndDate },
                                { "InvoiceFormat", updateStoreMaster.StoreAdditionalInfo.InvoiceFormat },
                                {
                                    "InvoiceDeliveryMethod", updateStoreMaster.StoreAdditionalInfo.InvoiceDeliveryMethod
                                },
                                {
                                    "DisplayDeliveryDocket", updateStoreMaster.StoreAdditionalInfo.DisplayDeliveryDocket
                                },
                                { "DisplayPrice", updateStoreMaster.StoreAdditionalInfo.DisplayPrice },
                                { "ShowCustPO", updateStoreMaster.StoreAdditionalInfo.ShowCustPO },
                                { "InvoiceText", updateStoreMaster.StoreAdditionalInfo.InvoiceText },
                                { "InvoiceFrequency", updateStoreMaster.StoreAdditionalInfo.InvoiceFrequency },
                                {
                                    "StockCreditIsPurchaseOrderRequired",
                                    updateStoreMaster.StoreAdditionalInfo.StockCreditIsPurchaseOrderRequired
                                },
                                {
                                    "AdminFeePerBillingCycle",
                                    updateStoreMaster.StoreAdditionalInfo.AdminFeePerBillingCycle
                                },
                                { "AdminFeePerDelivery", updateStoreMaster.StoreAdditionalInfo.AdminFeePerDelivery },
                                { "LatePaymentFee", updateStoreMaster.StoreAdditionalInfo.LatePaymentFee },
                                { "Drawer", updateStoreMaster.StoreAdditionalInfo.Drawer },
                                { "BankUID", updateStoreMaster.StoreAdditionalInfo.BankUID },
                                { "BankAccount", updateStoreMaster.StoreAdditionalInfo.BankAccount },
                                { "MandatoryPONumber", updateStoreMaster.StoreAdditionalInfo.MandatoryPONumber },
                                {
                                    "IsStoreCreditCaptureSignatureRequired",
                                    updateStoreMaster.StoreAdditionalInfo.IsStoreCreditCaptureSignatureRequired
                                },
                                {
                                    "StoreCreditAlwaysPrinted",
                                    updateStoreMaster.StoreAdditionalInfo.StoreCreditAlwaysPrinted
                                },
                                { "IsDummyCustomer", updateStoreMaster.StoreAdditionalInfo.IsDummyCustomer },
                                { "DefaultRun", updateStoreMaster.StoreAdditionalInfo.DefaultRun },
                                //{"ProspectEmpUID",updateStoreMaster.StoreAdditionalInfo.ProspectEmpUID},
                                { "IsFOCCustomer", updateStoreMaster.StoreAdditionalInfo.IsFOCCustomer },
                                { "RSSShowPrice", updateStoreMaster.StoreAdditionalInfo.RSSShowPrice },
                                { "RSSShowPayment", updateStoreMaster.StoreAdditionalInfo.RSSShowPayment },
                                { "RSSShowCredit", updateStoreMaster.StoreAdditionalInfo.RSSShowCredit },
                                { "RSSShowInvoice", updateStoreMaster.StoreAdditionalInfo.RSSShowInvoice },
                                { "RSSIsActive", updateStoreMaster.StoreAdditionalInfo.RSSIsActive },
                                {
                                    "RSSDeliveryInstructionStatus",
                                    updateStoreMaster.StoreAdditionalInfo.RSSDeliveryInstructionStatus
                                },
                                {
                                    "RSSTimeSpentOnRSSPortal",
                                    updateStoreMaster.StoreAdditionalInfo.RSSTimeSpentOnRSSPortal
                                },
                                { "RSSOrderPlacedInRSS", updateStoreMaster.StoreAdditionalInfo.RSSOrderPlacedInRSS },
                                { "RSSAvgOrdersPerWeek", updateStoreMaster.StoreAdditionalInfo.RSSAvgOrdersPerWeek },
                                { "RSSTotalOrderValue", updateStoreMaster.StoreAdditionalInfo.RSSTotalOrderValue },
                                { "AllowForceCheckIn", updateStoreMaster.StoreAdditionalInfo.AllowForceCheckIn },
                                { "IsManualEditAllowed", updateStoreMaster.StoreAdditionalInfo.IsManualEditAllowed },
                                { "CanUpdateLatLong", updateStoreMaster.StoreAdditionalInfo.CanUpdateLatLong },
                                // {"IsTaxApplicable",updateStoreMaster.StoreAdditionalInfo.IsTaxApplicable},
                                //{"TaxDocNumber",updateStoreMaster.StoreAdditionalInfo.TaxDocNumber},
                                //{"IsTaxDocVerified",updateStoreMaster.StoreAdditionalInfo.IsTaxDocVerified},
                                { "AllowGoodReturn", updateStoreMaster.StoreAdditionalInfo.AllowGoodReturn },
                                { "AllowBadReturn", updateStoreMaster.StoreAdditionalInfo.AllowBadReturn },

                                { "AllowReplacement", updateStoreMaster.StoreAdditionalInfo.AllowReplacement },
                                {
                                    "IsInvoiceCancellationAllowed",
                                    updateStoreMaster.StoreAdditionalInfo.IsInvoiceCancellationAllowed
                                },
                                {
                                    "IsDeliveryNoteRequired",
                                    updateStoreMaster.StoreAdditionalInfo.IsDeliveryNoteRequired
                                },
                                { "EInvoicingEnabled", updateStoreMaster.StoreAdditionalInfo.EInvoicingEnabled },
                                {
                                    "ImageRecognizationEnabled",
                                    updateStoreMaster.StoreAdditionalInfo.ImageRecognizationEnabled
                                },
                                {
                                    "MaxOutstandingInvoices",
                                    updateStoreMaster.StoreAdditionalInfo.MaxOutstandingInvoices
                                },
                                {
                                    "NegativeInvoiceAllowed",
                                    updateStoreMaster.StoreAdditionalInfo.NegativeInvoiceAllowed
                                },
                                { "DeliveryMode", updateStoreMaster.StoreAdditionalInfo.DeliveryMode },
                                //{"StoreSize",updateStoreMaster.StoreAdditionalInfo.StoreSize},
                                { "VisitFrequency", updateStoreMaster.StoreAdditionalInfo.VisitFrequency },
                                {
                                    "ShippingContactSameAsStore",
                                    updateStoreMaster.StoreAdditionalInfo.ShippingContactSameAsStore
                                },
                                {
                                    "BillingAddressSameAsShipping",
                                    updateStoreMaster.StoreAdditionalInfo.BillingAddressSameAsShipping
                                },
                                { "PaymentMode", updateStoreMaster.StoreAdditionalInfo.PaymentMode },
                                { "PriceType", updateStoreMaster.StoreAdditionalInfo.PriceType },
                                { "AverageMonthlyIncome", updateStoreMaster.StoreAdditionalInfo.AverageMonthlyIncome },
                                { "DefaultBankUID", updateStoreMaster.StoreAdditionalInfo.DefaultBankUID },
                                { "AccountNumber", updateStoreMaster.StoreAdditionalInfo.AccountNumber },
                                { "NoOfCashCounters", updateStoreMaster.StoreAdditionalInfo.NoOfCashCounters },
                                { "CustomField1", updateStoreMaster.StoreAdditionalInfo.CustomField1 },
                                { "CustomField2", updateStoreMaster.StoreAdditionalInfo.CustomField2 },
                                { "CustomField3", updateStoreMaster.StoreAdditionalInfo.CustomField3 },
                                { "CustomField4", updateStoreMaster.StoreAdditionalInfo.CustomField4 },
                                { "CustomField5", updateStoreMaster.StoreAdditionalInfo.CustomField5 },
                                { "CustomField6", updateStoreMaster.StoreAdditionalInfo.CustomField6 },
                                { "CustomField7", updateStoreMaster.StoreAdditionalInfo.CustomField7 },
                                { "CustomField8", updateStoreMaster.StoreAdditionalInfo.CustomField8 },
                                { "CustomField9", updateStoreMaster.StoreAdditionalInfo.CustomField9 },
                                { "CustomField10", updateStoreMaster.StoreAdditionalInfo.CustomField10 },
                                //{"TaxType",updateStoreMaster.StoreAdditionalInfo.TaxType},
                                //{"TaxKeyField",updateStoreMaster.StoreAdditionalInfo.TaxKeyField},
                                //{"StoreImage",updateStoreMaster.StoreAdditionalInfo.StoreImage},
                                //{"IsVATQRCaptureMandatory",updateStoreMaster.StoreAdditionalInfo.IsVATQRCaptureMandatory},
                                { "IsAssetEnabled", updateStoreMaster.StoreAdditionalInfo.IsAssetEnabled },
                                { "IsSurveyEnabled", updateStoreMaster.StoreAdditionalInfo.IsSurveyEnabled },

                                {
                                    "AllowReturnAgainstInvoice",
                                    updateStoreMaster.StoreAdditionalInfo.AllowReturnAgainstInvoice
                                },
                                {
                                    "AllowReturnWithSalesOrder",
                                    updateStoreMaster.StoreAdditionalInfo.AllowReturnWithSalesOrder
                                },
                                { "WeekOffSun", updateStoreMaster.StoreAdditionalInfo.WeekOffSun },
                                { "WeekOffMon", updateStoreMaster.StoreAdditionalInfo.WeekOffMon },
                                { "WeekOffTue", updateStoreMaster.StoreAdditionalInfo.WeekOffTue },
                                { "WeekOffWed", updateStoreMaster.StoreAdditionalInfo.WeekOffWed },
                                { "WeekOffThu", updateStoreMaster.StoreAdditionalInfo.WeekOffThu },
                                { "WeekOffFri", updateStoreMaster.StoreAdditionalInfo.WeekOffFri },
                                { "WeekOffSat", updateStoreMaster.StoreAdditionalInfo.WeekOffSat },
                            };

                            int count1 = await ExecuteNonQueryAsync(storeAdditionalInfoQuery, connection, transaction,
                                storeAdditionalInfoParameters);
                            if (count1 != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("storeAdditionalInfo Table Update Failed");
                            }
                            foreach (var store in updateStoreMaster.StoreCredits)
                            {
                                var storeCreditQuery = @"UPDATE store_credit 
                                                        SET 
                                                            modified_by = @ModifiedBy,
                                                            modified_time = @ModifiedTime,
                                                            server_modified_time = @ServerModifiedTime,
                                                            store_uid = @StoreUID,
                                                            payment_term_uid = @PaymentTermUID,
                                                            credit_type = @CreditType,
                                                            credit_limit = @CreditLimit,
                                                            temporary_credit = @TemporaryCredit,
                                                            org_uid = @OrgUID,
                                                            distribution_channel_uid = @DistributionChannelUID,
                                                            preferred_payment_mode = @PreferredPaymentMode,
                                                            is_active = @IsActive,
                                                            is_blocked = @IsBlocked,
                                                            blocking_reason_code = @BlockingReasonCode,
                                                            blocking_reason_description = @BlockingReasonDescription
                                                        WHERE 
                                                            uid = @UID";

                                var storeCreditParameters = new Dictionary<string, object?>
                                {
                                    { "UID", store.UID },
                                    { "ModifiedBy", store.ModifiedBy },
                                    { "ModifiedTime", store.ModifiedTime },
                                    { "ServerModifiedTime", store.ServerModifiedTime },
                                    { "StoreUID", store.StoreUID },
                                    { "PaymentTermUID", store.PaymentTermUID },
                                    { "CreditType", store.CreditType },
                                    { "CreditLimit", store.CreditLimit },
                                    { "TemporaryCredit", store.TemporaryCredit },
                                    { "OrgUID", store.OrgUID },
                                    { "DistributionChannelUID", store.DistributionChannelUID },
                                    { "PreferredPaymentMode", store.PreferredPaymentMode },
                                    { "IsActive", store.IsActive },
                                    { "IsBlocked", store.IsBlocked },
                                    { "BlockingReasonCode", store.BlockingReasonCode },
                                    { "BlockingReasonDescription", store.BlockingReasonDescription },
                                };
                                int count2 = await ExecuteNonQueryAsync(storeCreditQuery, connection, transaction,
                                    storeCreditParameters);
                                if (count2 != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception(" Update Failed In StoreCredit at Row");
                                }
                            }
                            foreach (var storedocument in updateStoreMaster.StoreDocuments)
                            {
                                var storeDocumentQuery = @"UPDATE store_document 
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
                                                                uid = @UID";

                                var storeDocumentParameters = new Dictionary<string, object?>
                                {
                                    { "UID", storedocument.UID },
                                    { "CreatedBy", storedocument.CreatedBy },
                                    { "ModifiedBy", storedocument.ModifiedBy },
                                    { "ModifiedTime", storedocument.ModifiedTime },
                                    { "ServerModifiedTime", storedocument.ServerModifiedTime },
                                    { "StoreUID", storedocument.StoreUID },
                                    { "DocumentType", storedocument.DocumentType },
                                    { "DocumentNo", storedocument.DocumentNo },
                                    { "ValidFrom", storedocument.ValidFrom },
                                    { "ValidUpTo", storedocument.ValidUpTo }
                                };
                                int count3 = await ExecuteNonQueryAsync(storeDocumentQuery, connection, transaction,
                                    storeDocumentParameters);
                                if (count3 != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table StoreDocument at Row");
                                }
                            }
                            foreach (var address in updateStoreMaster.Addresses)
                            {
                                var addressQuery = @"UPDATE address
                                                    SET
                                                        created_by = @CreatedBy,
                                                        modified_by = @ModifiedBy,
                                                        modified_time = @ModifiedTime,
                                                        server_modified_time = @ServerModifiedTime,
                                                        type = @Type,
                                                        name = @Name,
                                                        line1 = @Line1,
                                                        line2 = @Line2,
                                                        line3 = @Line3,
                                                        landmark = @Landmark,
                                                        area = @Area,
                                                        sub_area = @SubArea,
                                                        zip_code = @ZipCode,
                                                        city = @City,
                                                        country_code = @CountryCode,
                                                        region_code = @RegionCode,
                                                        phone = @Phone,
                                                        phone_extension = @PhoneExtension,
                                                        mobile1 = @Mobile1,
                                                        mobile2 = @Mobile2,
                                                        email = @Email,
                                                        fax = @Fax,
                                                        latitude = @Latitude,
                                                        longitude = @Longitude,
                                                        altitude = @Altitude,
                                                        linked_item_uid = @LinkedItemUID,
                                                        linked_item_type = @LinkedItemType,
                                                        status = @Status,
                                                        state_code = @StateCode,
                                                        territory_code = @TerritoryCode,
                                                        pan = @PAN,
                                                        aadhar = @AADHAR,
                                                        ssn = @SSN,
                                                        is_editable = @IsEditable,
                                                        is_default = @IsDefault,
                                                        info = @Info,
                                                        line4 = @Line4
                                                    WHERE
                                                        uid = @UID";
                                var addressParameters = new Dictionary<string, object?>
                                {
                                    { "UID", address.UID },
                                    { "CreatedBy", address.CreatedBy },
                                    { "ModifiedBy", address.ModifiedBy },
                                    { "ModifiedTime", address.ModifiedTime },
                                    { "ServerModifiedTime", address.ServerModifiedTime },
                                    { "Type", address.Type },
                                    { "Name", address.Name },
                                    { "Line1", address.Line1 },
                                    { "Line2", address.Line2 },
                                    { "Line3", address.Line3 },
                                    { "Landmark", address.Landmark },
                                    { "Area", address.Area },
                                    { "SubArea", address.SubArea },
                                    { "ZipCode", address.ZipCode },
                                    { "City", address.City },
                                    { "CountryCode", address.CountryCode },
                                    { "RegionCode", address.RegionCode },
                                    { "Phone", address.Phone },
                                    { "PhoneExtension", address.PhoneExtension },
                                    { "Mobile1", address.Mobile1 },
                                    { "Mobile2", address.Mobile2 },
                                    { "Email", address.Email },
                                    { "Fax", address.Fax },
                                    { "Latitude", address.Latitude },
                                    { "Longitude", address.Longitude },
                                    { "Altitude", address.Altitude },
                                    { "LinkedItemUID", address.LinkedItemUID },
                                    { "LinkedItemType", address.LinkedItemType },
                                    { "Status", address.Status },
                                    { "StateCode", address.StateCode },
                                    { "TerritoryCode", address.TerritoryCode },
                                    { "PAN", address.PAN },
                                    { "AADHAR", address.AADHAR },
                                    { "SSN", address.SSN },
                                    { "IsEditable", address.IsEditable },
                                    { "IsDefault", address.IsDefault },
                                    { "Info", address.Info },
                                    { "Line4", address.Line4 }
                                };

                                int count4 = await ExecuteNonQueryAsync(addressQuery, connection, transaction,
                                    addressParameters);
                                if (count4 != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Error Inserting Data In Table Address at Row");
                                }
                            }
                            foreach (var contact in updateStoreMaster.Contacts)
                            {
                                var contactQuery = @"UPDATE contact
                                                    SET
                                                        created_by = @CreatedBy,
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
                                                    WHERE
                                                        uid = @UID";
                                var contactParameters = new Dictionary<string, object?>
                                {
                                    { "UID", contact.UID },
                                    { "CreatedBy", contact.CreatedBy },
                                    { "ModifiedBy", contact.ModifiedBy },
                                    { "ModifiedTime", contact.ModifiedTime },
                                    { "ServerModifiedTime", contact.ServerModifiedTime },
                                    { "Title", contact.Title },
                                    { "Name", contact.Name },
                                    { "Phone", contact.Phone },
                                    { "PhoneExtension", contact.PhoneExtension },
                                    { "Description", contact.Description },
                                    { "Designation", contact.Designation },
                                    { "Mobile", contact.Mobile },
                                    { "Email", contact.Email },
                                    { "Email2", contact.Email2 },
                                    { "Email3", contact.Email3 },
                                    { "InvoiceForEmail1", contact.InvoiceForEmail1 },
                                    { "InvoiceForEmail2", contact.InvoiceForEmail2 },
                                    { "InvoiceForEmail3", contact.InvoiceForEmail3 },
                                    { "Fax", contact.Fax },
                                    { "LinkedItemUID", contact.LinkedItemUID },
                                    { "LinkedItemType", contact.LinkedItemType },
                                    { "IsDefault", contact.IsDefault },
                                    { "IsEditable", contact.IsEditable },
                                    { "EnabledForInvoiceEmail", contact.EnabledForInvoiceEmail },
                                    { "EnabledForDocketEmail", contact.EnabledForDocketEmail },
                                    { "EnabledForPromoEmail", contact.EnabledForPromoEmail },
                                    { "IsEmailCC", contact.IsEmailCC },
                                    { "Mobile2", contact.Mobile2 }
                                };

                                int count5 = await ExecuteNonQueryAsync(contactQuery, connection, transaction,
                                    contactParameters);
                                if (count5 != 1)
                                {
                                    transaction.Rollback();
                                    throw new Exception("Update Failed In Contact");
                                }
                            }
                            transaction.Commit();
                            return count;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction?.Rollback();
                            }
                            catch
                            {
                                // Transaction already rolled back, ignore
                            }
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>,
           List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
           List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>,
           List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>,
           List<Winit.Modules.Address.Model.Interfaces.IAddress>,
           List<Winit.Modules.Contact.Model.Interfaces.IContact>)> PrepareStoreMaster(List<string> storeUIDs)
        {
            try
            {
                var parameters = new
                {
                    UIDs = storeUIDs
                };

                var storeSql = new StringBuilder(@"
                                                 SELECT s.id AS Id, s.uid AS UID, s.created_by AS CreatedBy, s.created_time AS CreatedTime,
                                                 s.modified_by AS ModifiedBy, s.modified_time AS ModifiedTime, s.server_add_time AS ServerAddTime, 
                                                 s.server_modified_time AS ServerModifiedTime, s.company_uid AS CompanyUID, s.code AS Code, s.number AS Number,
                                                 s.name AS Name, s.alias_name AS AliasName, s.legal_name AS LegalName,
                                                 s.type AS Type, s.bill_to_store_uid AS BillToStoreUID, s.ship_to_store_uid AS ShipToStoreUID, 
                                                 s.sold_to_store_uid AS SoldToStoreUID, s.status AS Status, s.is_active AS IsActive, s.store_class AS StoreClass,
                                                 s.store_rating AS StoreRating, s.is_blocked AS IsBlocked, s.blocked_reason_code AS BlockedReasonCode, 
                                                 s.blocked_reason_description AS BlockedReasonDescription, s.created_by_emp_uid AS CreatedByEmpUID, 
                                                 s.created_by_job_position_uid AS CreatedByJobPositionUID, s.country_uid AS CountryUID, 
                                                 s.region_uid AS RegionUID, s.city_uid AS CityUID, s.source AS Source, s.arabic_name AS ArabicName, 
                                                 s.outlet_name AS OutletName, s.blocked_by_emp_uid AS BlockedByEmpUID, s.is_tax_applicable AS IsTaxApplicable, 
                                                 s.tax_doc_number AS TaxDocNumber, s.school_warehouse AS SchoolWarehouse, s.day_type AS DayType, 
                                                 s.special_day AS SpecialDay, s.is_tax_doc_verified AS IsTaxDocVerified, s.store_size AS StoreSize, 
                                                 s.prospect_emp_uid AS ProspectEmpUID, s.tax_key_field AS TaxKeyField, s.store_image AS StoreImage,
                                                 s.is_vat_qr_capture_mandatory AS IsVATQRCaptureMandatory, s.tax_type AS TaxType, 
                                                 s.franchisee_org_uid AS FranchiseeOrgUID, s.state_uid AS StateUID, s.route_type AS RouteType, 
                                                 s.price_type AS PriceType,s.broad_classification AS BroadClassification,
                                                 s.classfication_type  AS ClassficationType,s.is_available_to_use AS IsAvailableToUse, is_asm_mapped_by_customer as IsAsmMappedByCustomer  FROM store s");
                if (storeUIDs != null && storeUIDs.Any())
                {
                    storeSql.Append(@" WHERE s.uid In @UIDs");
                }

                List<Winit.Modules.Store.Model.Interfaces.IStore> storeList =
                    await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStore>(storeSql.ToString(),
                    parameters);

                var storeAdditionalInfoSql = new StringBuilder(
                @"select id as Id, uid as UID, created_by as CreatedBy, created_time as CreatedTime, modified_by as ModifiedBy, modified_time as ModifiedTime,
                      server_add_time as ServerAddTime, server_modified_time as ServerModifiedTime, store_uid as StoreUID, order_type as OrderType,
                      is_promotions_block as IsPromotionsBlock, customer_start_date as CustomerStartDate, customer_end_date as CustomerEndDate,
                      purchase_order_number as PurchaseOrderNumber, delivery_docket_is_purchase_order_required as DeliveryDocketIsPurchaseOrderRequired,
                      is_with_printed_invoices as IsWithPrintedInvoices, is_capture_signature_required as IsCaptureSignatureRequired,
                      is_always_printed as IsAlwaysPrinted, building_delivery_code as BuildingDeliveryCode, delivery_information as DeliveryInformation,
                      is_stop_delivery as IsStopDelivery, is_forecast_top_up_qty as IsForeCastTopUpQty, is_temperature_check as IsTemperatureCheck,
                      invoice_start_date as InvoiceStartDate, invoice_end_date as InvoiceEndDate, invoice_format as InvoiceFormat,
                      invoice_delivery_method as InvoiceDeliveryMethod, display_delivery_docket as DisplayDeliveryDocket, display_price as DisplayPrice,
                      show_cust_po as ShowCustPO, invoice_text as InvoiceText, invoice_frequency as InvoiceFrequency,
                      stock_credit_is_purchase_order_required as StockCreditIsPurchaseOrderRequired, admin_fee_per_billing_cycle as AdminFeePerBillingCycle,
                      admin_fee_per_delivery as AdminFeePerDelivery, late_payment_fee as LatePaymentFee, drawer, bank_uid as BankUID,
                      bank_account as BankAccount, mandatory_po_number as MandatoryPONumber, 
                      is_store_credit_capture_signature_required as IsStoreCreditCaptureSignatureRequired,
                      store_credit_always_printed as StoreCreditAlwaysPrinted, is_dummy_customer as IsDummyCustomer, default_run as DefaultRun,
                      is_foc_customer as IsFOCCustomer, rss_show_price as RSSShowPrice, rss_show_payment as RSSShowPayment, rss_show_credit as RSSShowCredit,
                      rss_show_invoice as RSSShowInvoice, rss_is_active as RSSIsActive, rss_delivery_instruction_status as RSSDeliveryInstructionStatus,
                      rss_time_spent_on_rss_portal as RSSTimeSpentOnRSSPortal, rss_order_placed_in_rss as RSSOrderPlacedInRSS, rss_avg_orders_per_week as RSSAvgOrdersPerWeek,
                      rss_total_order_value as RSSTotalOrderValue, allow_force_check_in as AllowForceCheckIn, is_manual_edit_allowed as IsManualEditAllowed,
                      can_update_lat_long as CanUpdateLatLong, allow_good_return as AllowGoodReturn, allow_bad_return as AllowBadReturn, 
                       allow_replacement as AllowReplacement, is_invoice_cancellation_allowed as IsInvoiceCancellationAllowed,
                      is_delivery_note_required as IsDeliveryNoteRequired, e_invoicing_enabled as EInvoicingEnabled, image_recognition_enabled as ImageRecognitionEnabled,
                      max_outstanding_invoices as MaxOutstandingInvoices, negative_invoice_allowed as NegativeInvoiceAllowed, delivery_mode as DeliveryMode,
                      visit_frequency as VisitFrequency, shipping_contact_same_as_store as ShippingContactSameAsStore, billing_address_same_as_shipping as BillingAddressSameAsShipping,
                      payment_mode as PaymentMode, price_type as PriceType, average_monthly_income as AverageMonthlyIncome, default_bank_uid as DefaultBankUID,
                      account_number as AccountNumber, no_of_cash_counters as NoOfCashCounters, custom_field1 as CustomField1, custom_field2 as CustomField2,
                      custom_field3 as CustomField3, custom_field4 as CustomField4, custom_field5 as CustomField5, custom_field6 as CustomField6,
                      custom_field7 as CustomField7, custom_field8 as CustomField8, custom_field9 as CustomField9, custom_field10 as CustomField10,
                      is_asset_enabled as IsAssetEnabled, is_survey_enabled as IsSurveyEnabled, 
                      allow_return_against_invoice as AllowReturnAgainstInvoice, allow_return_with_sales_order as AllowReturnWithSalesOrder,
                      week_off_sun as WeekOffSun, week_off_mon as WeekOffMon, week_off_tue as WeekOffTue, week_off_wed as WeekOffWed, week_off_thu as WeekOffThu,
                      week_off_fri as WeekOffFri, week_off_sat as WeekOffSat, aging_cycle as AgingCycle, depot
                     from store_additional_info");
                if (storeUIDs != null && storeUIDs.Any())
                {
                    storeAdditionalInfoSql.Append(@" WHERE  store_uid In @UIDs");
                }

                List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> storeAdditionalInfoList =
                    await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>(
                    storeAdditionalInfoSql.ToString(), parameters);

                var storeCreditSql = new StringBuilder(
                @"select id as Id, uid as UID, created_by as CreatedBy, created_time as CreatedTime, modified_by as ModifiedBy, modified_time as ModifiedTime,
                           server_add_time as ServerAddTime, server_modified_time as ServerModifiedTime, store_uid as StoreUID, payment_term_uid as PaymentTermUID,
                           credit_type as CreditType, credit_limit as CreditLimit, temporary_credit as TemporaryCredit, org_uid as OrgUID,
                           distribution_channel_uid as DistributionChannelUID, preferred_payment_mode as PreferredPaymentMode, is_active as IsActive,
                           is_blocked as IsBlocked, blocking_reason_code as BlockingReasonCode, blocking_reason_description as BlockingReasonDescription,
                           price_list as PriceList, authorized_item_grp_key as AuthorizedItemGRPKey, message_key as MessageKey, tax_key_field as TaxKeyField,
                           promotion_key as PromotionKey, disabled, bill_to_address_uid as BillToAddressUID, ship_to_address_uid as ShipToAddressUID,
                           outstanding_invoices as OutstandingInvoices, preferred_payment_method as PreferredPaymentMethod, payment_type as PaymentType,
                           invoice_admin_fee_per_billing_cycle as InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery as InvoiceAdminFeePerDelivery,
                           invoice_late_payment_fee as InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed as IsCancellationOfInvoiceAllowed,
                           is_allow_cash_on_credit_exceed as IsAllowCashOnCreditExceed, is_outstanding_bill_control as IsOutstandingBillControl,
                           is_negative_invoice_allowed as IsNegativeInvoiceAllowed,credit_days AS CreditDays,temporary_credit_days AS TemporaryCreditDays,
                           division_org_uid AS DivisionOrgUID
                           from store_credit");
                if (storeUIDs != null && storeUIDs.Any())
                {
                    storeCreditSql.Append(@" WHERE store_uid In @UIDs");
                }

                List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditList =
                    await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(
                    storeCreditSql.ToString(), parameters);


                var storeAttributesSql = new StringBuilder(@"
                        SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid, org_uid AS OrgUID,
                        distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, name AS Name, code AS Code, value AS Value, 
                        parent_name AS ParentName  FROM store_attributes");

                if (storeUIDs != null && storeUIDs.Any())
                {
                    storeAttributesSql.Append(@" WHERE store_uid In @UIDs");
                }

                List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList =
                    await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>(
                    storeAttributesSql.ToString(), parameters);

                var addressSql = new StringBuilder(
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                                                     modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                     type AS Type, name AS Name, line1 AS Line1, line2 AS Line2,line3 AS Line3, landmark AS Landmark, 
                                                     area AS Area, sub_area AS SubArea, zip_code AS ZipCode, city AS City, country_code AS CountryCode, 
                                                     region_code AS RegionCode, phone AS Phone, phone_extension AS PhoneExtension, mobile1 AS Mobile1, 
                                                     mobile2 AS Mobile2, email AS Email,  fax AS Fax, latitude AS Latitude, longitude AS Longitude,
                                                     altitude AS Altitude, linked_item_uid AS LinkedItemUID, linked_item_type AS LinkedItemType,
                                                     status AS Status, state_code AS StateCode, territory_code AS TerritoryCode, pan AS Pan, aadhar AS Aadhar, 
                                                     ssn AS Ssn, is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info, depot AS Depot
                                                    , branch_uid as branchuid, org_unit_uid as orgunituid, sales_office_uid as salesofficeuid, custom_field1 as CustomField1, 
                                                     custom_field2 as CustomField2, state, custom_field3 as CustomField3 
                                                     FROM address");
                if (storeUIDs != null && storeUIDs.Any())
                {
                    addressSql.Append(@" WHERE linked_item_uid In @UIDs");
                }

                List<Winit.Modules.Address.Model.Interfaces.IAddress> addressList =
                    await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(addressSql.ToString(),
                    parameters);
                var contactSql = new StringBuilder(
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                                                     modified_time AS ModifiedTime,server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                     title AS Title, name AS Name, phone AS Phone, phone_extension AS PhoneExtension, description AS Description,
                                                     designation AS Designation, mobile AS Mobile, email AS Email, email2 AS Email2, email3 AS Email3,
                                                     invoice_for_email1 AS InvoiceForEmail1, invoice_for_email2 AS InvoiceForEmail2, 
                                                     invoice_for_email3 AS InvoiceForEmail3, fax AS Fax, linked_item_uid AS LinkedItemUid, 
                                                     linked_item_type AS LinkedItemType, is_default AS IsDefault, is_editable AS IsEditable, 
                                                     enabled_for_invoice_email AS EnabledForInvoiceEmail, enabled_for_docket_email AS EnabledForDocketEmail, 
                                                     enabled_for_promo_email AS EnabledForPromoEmail,
                                                     is_email_cc AS IsEmailCc, mobile2 AS Mobile2 FROM contact");

                if (storeUIDs != null && storeUIDs.Any())
                {
                    contactSql.Append(@" WHERE linked_item_uid In @UIDs");
                }

                List<Winit.Modules.Contact.Model.Interfaces.IContact> contactList =
                    await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(contactSql.ToString(),
                    parameters);
                return (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList,
                    contactList);
            }
            catch (Exception)
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
                    @"SELECT ""UID"", ""Number"" AS Code, ""Name"" AS Label  FROM ""Store"" WHERE ""FranchiseeOrgUID"" = @OrgUID");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""Store""");
                }


                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<SelectionItem>(filterCriterias, sbFilterCriteria, parameters);

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
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

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

        public async Task<PagedResponse<IStoreCustomer>> GetAllStoreItems(List<SortCriteria> sortCriterias,
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
                    @"SELECT s.uid AS UID, s.number AS Code, s.name AS Label, a.line1 || ' ' || a.line2 AS Address
                FROM store s
                LEFT JOIN address a ON a.linked_item_uid = s.UID AND a.is_default = true
                WHERE s.franchisee_org_uid = @OrgUID ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""Store""");
                }

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria<StoreCustomer>(filterCriterias, sbFilterCriteria, parameters);

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
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<IStoreCustomer>().GetType();
                IEnumerable<IStoreCustomer> storeItemViews =
                    await ExecuteQueryAsync<IStoreCustomer>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<IStoreCustomer> pagedResponse = new PagedResponse<IStoreCustomer>
                {
                    PagedData = storeItemViews,
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
                @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
       server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, code AS Code, number AS Number, 
       name AS Name, alias_name AS AliasName, legal_name AS LegalName, type AS Type, bill_to_store_uid AS BillToStoreUID, ship_to_store_uid AS ShipToStoreUID, 
       sold_to_store_uid AS SoldToStoreUID, status AS Status, is_active AS IsActive, store_class AS StoreClass, store_rating AS StoreRating, 
       is_blocked AS IsBlocked, blocked_reason_code AS BlockedReasonCode, blocked_reason_description AS BlockedReasonDescription, 
       created_by_emp_uid AS CreatedByEmpUID, created_by_job_position_uid AS CreatedByJobPositionUID, country_uid AS CountryUID, 
       region_uid AS RegionUid, city_uid AS CityUid, source AS Source, outlet_name AS OutletName, blocked_by_emp_uid AS BlockedByEmpUid, 
       arabic_name AS ArabicName, is_tax_applicable AS IsTaxApplicable, tax_doc_number AS TaxDocNumber, school_warehouse AS SchoolWarehouse, 
       day_type AS DayType, special_day AS SpecialDay, is_tax_doc_verified AS IsTaxDocVerified, store_size AS StoreSize, prospect_emp_uid AS ProspectEmpUID, 
       tax_key_field AS TaxKeyField, store_image AS StoreImage, is_vat_qr_capture_mandatory AS IsVatQrCaptureMandatory, tax_type AS TaxType, 
       franchisee_org_uid AS FranchiseeOrgUID FROM store  WHERE franchisee_org_uid = @OrgUID";


            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStore>().GetType();
            Model.Interfaces.IStore StoreList =
                await ExecuteSingleAsync<Model.Interfaces.IStore>(sql, parameters, type);
            return StoreList;
        }

        public async Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "routeUID", routeUID }
            };

            var sql =
                @"SELECT s.uid AS UID, s.number AS Number, s.code AS Code, s.name AS Name, s.is_active AS IsActive,
       s.is_blocked AS IsBlocked, COALESCE(s.blocked_reason_description, '') AS BlockedReason,
       a.line1 || ' ' || a.line2 AS Address, a.latitude AS Latitude, a.longitude AS Longitude
        FROM route_customer rc
        INNER JOIN store s ON s.uid = rc.store_UID AND rc.route_uid = @routeUID
        LEFT JOIN address a ON a.linked_item_uid = s.UID AND a.is_default = true
        ORDER BY rc.seq_no ";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreItemView>().GetType();
            List<Model.Interfaces.IStoreItemView> CustomerList =
                await ExecuteQueryAsync<Model.Interfaces.IStoreItemView>(sql, parameters, type);
            return CustomerList;
        }

        public async Task<List<IStoreCustomer>> GetStoreCustomersByRouteUID(string routeUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "routeUID", routeUID }
            };

            var sql =
                @"SELECT s.uid AS UID, s.number AS Number, s.code AS Code, s.name AS Name, s.is_active AS IsActive, 
       s.is_blocked AS IsBlocked, COALESCE(s.blocked_reason_description, '') AS BlockedReason,
       a.line1 || ' ' || a.line2 AS Address, a.latitude AS Latitude, a.longitude AS Longitude
        FROM route_customer rc 
        INNER JOIN store s ON s.uid = rc.store_UID AND rc.route_uid = @routeUID
        LEFT JOIN address a ON a.linked_item_uid = s.UID AND a.is_default = true
        ORDER BY rc.seq_no ";

            Type type = _serviceProvider.GetRequiredService<IStoreCustomer>().GetType();
            List<IStoreCustomer> CustomerList = await ExecuteQueryAsync<IStoreCustomer>(sql, parameters, type);
            return CustomerList;
        }

        public Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID, string BeatHistoryUID, bool notInJP)
        {
            throw new NotImplementedException();
        }

        private async Task<int> CUDStore(IStore store)
        {
            int count = -1;
            try
            {
                string? existingUID = await CheckIfUIDExistsInDB(DbTableName.Store, store.UID);
                if (existingUID != null)
                {
                    count = await UpdateStore(store);
                }
                else
                {
                    count = await CreateStore(store);
                }
                return count;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> CUDOnBoardCustomerInfo(IOnBoardCustomerDTO onBoardCustomerDTO)
        {
            if (onBoardCustomerDTO == null) throw new ArgumentNullException(nameof(onBoardCustomerDTO));
            if (onBoardCustomerDTO.Store == null) throw new ArgumentNullException(nameof(onBoardCustomerDTO.Store));
            if (onBoardCustomerDTO.StoreAdditionalInfo == null)
                throw new ArgumentNullException(nameof(onBoardCustomerDTO.StoreAdditionalInfo));
            if (onBoardCustomerDTO.StoreAdditionalInfoCMI == null)
                throw new ArgumentNullException(nameof(onBoardCustomerDTO.StoreAdditionalInfoCMI));
            if (onBoardCustomerDTO.FileSys == null) throw new ArgumentNullException(nameof(onBoardCustomerDTO.FileSys));

            int count = -1;
            try
            {
                count = await CUDStore(onBoardCustomerDTO.Store);
                count = await _storeAdditionalInfoDL.CUDStoreAdditionalInfo(onBoardCustomerDTO.StoreAdditionalInfo);
                count = await _storeAdditionalInfoCMIDL.CUDStoreAdditionalInfoCMI(onBoardCustomerDTO
                    .StoreAdditionalInfoCMI);
                List<CommonUIDResponse> response = await _fileSysDL.CreateFileSysForBulk(onBoardCustomerDTO.FileSys
                    .Cast<Winit.Modules.FileSys.Model.Classes.FileSys>().ToList());
                if (response != null)
                {
                    count += response.Count;
                }
                return count;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> CreateAllApprovalRequest(
            IAllApprovalRequest allApprovalRequest)
        {
            int retVal = -1;
            try
            {
                string query = @"Insert Into AllApprovalRequest(linkedItemType,linkedItemUID,requestID)
                                 values(@linkedItemType,@linkedItemUID,@requestID)";
                retVal = await ExecuteNonQueryAsync(query, allApprovalRequest);
            }
            catch
            {
                throw;
            }
            return retVal;
        }


        public async Task<PagedResponse<IOnBoardGridview>> SelectAllOnBoardCustomer(List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Role)
        {
            try
            {
                var sql = new StringBuilder(
                    @"select * from (select s.id as Id,  s.uid as UID, s.created_by as CreatedBy,  s.created_time as CreatedTime, 
                                              s.modified_by as ModifiedBy, s.modified_time as ModifiedTime,s.server_add_time as ServerAddTime, 
                                              s.server_modified_time as ServerModifiedTime,s.code as CustomerCode, s.legal_name as CustomerName,
                                              s.broad_classification as BroadClassification,
                                              sai.gst_owner_name as OwnerName
                                              from store s
								              inner join store_additional_info sai on sai.store_uid=s.uid
                                                  WHERE s.type = 'FR') as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(
                        @"SELECT COUNT(1) AS Cnt from (select s.id as Id,  s.uid as UID, s.created_by as CreatedBy,  s.created_time as CreatedTime, 
                                              s.modified_by as ModifiedBy, s.modified_time as ModifiedTime,s.server_add_time as ServerAddTime, 
                                              s.server_modified_time as ServerModifiedTime,s.code as CustomerCode, s.legal_name as CustomerName,
                                              s.broad_classification as BroadClassification,
                                              sai.gst_owner_name as OwnerName
                                              from store s
								              inner join store_additional_info sai on sai.store_uid=s.uid
                                                    WHERE s.type = 'FR') as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Store.Model.Interfaces.IStore>(filterCriterias, sbFilterCriteria,
                        parameters);

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
                        sql.Append(
                            $" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Model.Interfaces.IOnBoardGridview> stores =
                    await ExecuteQueryAsync<Model.Interfaces.IOnBoardGridview>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview> pagedResponse =
                    new PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>
                    {
                        PagedData = stores,
                        TotalCount = totalCount
                    };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateStoreStatus(StoreApprovalDTO storeApprovalDTO)
        {
            try
            {
                var sql = @"UPDATE store SET 
                            modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            status = @Status
                        WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, storeApprovalDTO.Store);
            }
            catch (Exception)
            {
                throw;
            }
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

        Task<int> IStoreDL.CreateChangeRequest(Winit.Modules.Store.Model.Classes.ChangeRequestDTO changeRequestDTO)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<string>> GetDivisionsByAsmEmpUID(string asmEmpUID)
        {
            try
            {
                string sql = "SELECT DISTINCT division_uid FROM asm_division_mapping where asm_emp_uid = @EMPUID";
                var parameters = new
                {
                    EMPUID = asmEmpUID
                };
                return await ExecuteQueryAsync<string>(sql, parameters);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<List<IStore>> GetChannelPartner(string jobPositionUid)
        {
            var sql = @"select s.*, o.parent_uid as orgparentuid from store s
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

        Task<List<IStoreItemView>> IStoreDL.GetStoreByRouteUIDWithoutAddress(string routeUID)
        {
            throw new NotImplementedException();
        }
    }
}
