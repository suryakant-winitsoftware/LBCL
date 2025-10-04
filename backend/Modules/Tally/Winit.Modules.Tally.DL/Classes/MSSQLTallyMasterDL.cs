using Microsoft.Extensions.Configuration;
using Nest;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tally.DL.Interfaces;
using Winit.Modules.Tally.Model.Classes;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.DL.Classes
{
    public class MSSQLTallyMasterDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ITallyMasterDL
    {
        public MSSQLTallyMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<PagedResponse<ITallyDealerMaster>> GetTallyDealerMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            SELECT * from(SELECT 
                                                r.id AS Id,
                                                ledger_name AS LedgerName,
                                                parent_name AS ParentName,
                                                opening_balance AS OpeningBalance,
                                                primary_group AS PrimaryGroup,
                                                remote_alt_guid AS RemoteAltGuid,
                                                address AS Address,
                                                address1 AS Address1,
                                                address2 AS Address2,
                                                country_name AS CountryName,
                                                email AS Email,
                                                phone AS Phone,
                                                pincode AS Pincode,
                                                state_name AS StateName,
                                                income_tax_number AS IncomeTaxNumber,
                                                country_of_residence AS CountryOfResidence,
                                                o.name AS DistributorCode,
                                                r.status AS Status,
                                                gstin AS Gstin,
                                                r.created_time AS CreatedTime,
                                                r.modified_time AS ModifiedTime
                                            FROM retailers_from_tally r
                                            Inner join org O on O.uid=r.distributor_org_uid
                                            WHERE r.distributor_org_uid = @INPUTUID ) as subquery
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    r.id AS Id,
                                                    ledger_name AS LedgerName,
                                                    parent_name AS ParentName,
                                                    opening_balance AS OpeningBalance,
                                                    primary_group AS PrimaryGroup,
                                                    remote_alt_guid AS RemoteAltGuid,
                                                    address AS Address,
                                                    address1 AS Address1,
                                                    address2 AS Address2,
                                                    country_name AS CountryName,
                                                    email AS Email,
                                                    phone AS Phone,
                                                    pincode AS Pincode,
                                                    state_name AS StateName,
                                                    income_tax_number AS IncomeTaxNumber,
                                                    country_of_residence AS CountryOfResidence,
                                                    o.name AS DistributorCode,
                                                    r.status AS Status,
                                                    gstin AS Gstin,
                                                    r.created_time AS CreatedTime,
                                                    r.modified_time AS ModifiedTime
                                                FROM retailers_from_tally r
                                                Inner join org O on O.uid=r.distributor_org_uid
                                                WHERE r.distributor_org_uid = @INPUTUID) as sub_query
    
                                                """);
                }
                //     var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster> DealerItems = await ExecuteQueryAsync<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster>
                {
                    PagedData = DealerItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ITallyDealerMaster> GetTallyDealerMasterItem(string uID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  uID}
            };
                var sql = """
                            SELECT 
                                id AS Id,
                                ledger_name AS LedgerName,
                                parent_name AS ParentName,
                                opening_balance AS OpeningBalance,
                                primary_group AS PrimaryGroup,
                                remote_alt_guid AS RemoteAltGuid,
                                address AS Address,
                                address1 AS Address1,
                                address2 AS Address2,
                                country_name AS CountryName,
                                email AS Email,
                                phone AS Phone,
                                pincode AS Pincode,
                                state_name AS StateName,
                                income_tax_number AS IncomeTaxNumber,
                                country_of_residence AS CountryOfResidence,
                                distributor_code AS DistributorCode,
                                status AS Status,
                                gstin AS Gstin,
                                created_time AS CreatedTime,
                                modified_time AS ModifiedTime
                            FROM retailers_from_tally
                            WHERE remote_alt_guid = @UID
                            """;
                return await ExecuteSingleAsync<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }



        public async Task<PagedResponse<ITallyInventoryMaster>> GetTallyInventoryMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            SELECT * from (SELECT 
                                                r.id AS Id,
                                                r.name AS Name,
                                                r.code AS Code,
                                                r.units AS Units,
                                                r.last_record_date AS LastRecordDate,
                                                r.opening_balance AS OpeningBalance,
                                                r.opening_rate AS OpeningRate,
                                                r.stock_group AS StockGroup,
                                                r.parent AS Parent,
                                                r.gst_details AS GstDetails,
                                                r.remote_alt_guid AS RemoteAltGuid,
                                                o.name AS DistributorCode,
                                                r.created_time AS CreatedTime,
                                                r.modified_time AS ModifiedTime
                                            FROM 
                                                inventory_from_tally r
                                            Inner join org O on O.uid=r.distributor_org_uid
                                            WHERE 
                                                distributor_org_uid = @INPUTUID) as subquery
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    r.id AS Id,
                                                    r.name AS Name,
                                                    r.code AS Code,
                                                    r.units AS Units,
                                                    r.last_record_date AS LastRecordDate,
                                                    r.opening_balance AS OpeningBalance,
                                                    r.opening_rate AS OpeningRate,
                                                    r.stock_group AS StockGroup,
                                                    r.parent AS Parent,
                                                    r.gst_details AS GstDetails,
                                                    r.remote_alt_guid AS RemoteAltGuid,
                                                    o.name AS DistributorCode,
                                                    r.created_time AS CreatedTime,
                                                    r.modified_time AS ModifiedTime
                                                FROM 
                                                    inventory_from_tally r
                                                Inner join org O on O.uid=r.distributor_org_uid
                                                WHERE 
                                                    TRIM(distributor_org_uid) = @INPUTUID) as sub_query
                                                """);
                }
                //     var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster> InventoryItems = await ExecuteQueryAsync<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster>
                {
                    PagedData = InventoryItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ITallyInventoryMaster> GetInventoryMasterItem(string uID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  uID}
            };
                var sql = """
                            SELECT 
                                id AS Id,
                                name AS Name,
                                code AS Code,
                                units AS Units,
                                last_record_date AS LastRecordDate,
                                opening_balance AS OpeningBalance,
                                opening_rate AS OpeningRate,
                                stock_group AS StockGroup,
                                parent AS Parent,
                                gst_details AS GstDetails,
                                remote_alt_guid AS RemoteAltGuid,
                                distributor_code AS DistributorCode,
                                created_time AS CreatedTime,
                                modified_time AS ModifiedTime
                            FROM 
                                inventory_from_tally
                            WHERE remote_alt_guid = @UID
                            """;
                return await ExecuteSingleAsync<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        
        
        
        public async Task<PagedResponse<ITallySalesInvoiceMaster>> GetTallySalesInvoiceMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            SELECT * from (select 
                                                r.id AS Id,
                                                r.dmsuid AS Dmsuid,
                                                r.voucher_number AS VoucherNumber,
                                                r.date AS Date,
                                                r.guid AS Guid,
                                                r.state_name AS StateName,
                                                r.country_of_residence AS CountryOfResidence,
                                                r.party_name AS PartyName,
                                                r.voucher_type_name AS VoucherTypeName,
                                                r.party_ledger_name AS PartyLedgerName,
                                                r.basic_base_party_name AS BasicBasePartyName,
                                                r.place_of_supply AS PlaceOfSupply,
                                                r.basic_buyer_name AS BasicBuyerName,
                                                r.basic_datetime_of_invoice AS BasicDatetimeOfInvoice,
                                                r.consignee_pin_number AS ConsigneePinNumber,
                                                r.consignee_state_name AS ConsigneeStateName,
                                                r.voucher_key AS VoucherKey,
                                                r.amount AS Amount,   
                                                r.persisted_view AS PersistedView,
                                                o.name AS DistributorCode,
                                                r.cgst AS Cgst,
                                                r.sgst AS Sgst,
                                                r.gst AS Gst,
                                                r.status AS Status,
                                                r.created_time AS CreatedTime,
                                                r.modified_time AS ModifiedTime
                                            FROM 
                                                sales_order_tally r
                                            Inner join org O on O.uid=r.distributor_org_uid
                                            WHERE 
                                                distributor_org_uid = @INPUTUID ) as subquery
                                            
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    r.id AS Id,
                                                    r.dmsuid AS Dmsuid,
                                                    r.voucher_number AS VoucherNumber,
                                                    r.date AS Date,
                                                    r.guid AS Guid,
                                                    r.state_name AS StateName,
                                                    r.country_of_residence AS CountryOfResidence,
                                                    r.party_name AS PartyName,
                                                    r.voucher_type_name AS VoucherTypeName,
                                                    r.party_ledger_name AS PartyLedgerName,
                                                    r.basic_base_party_name AS BasicBasePartyName,
                                                    r.place_of_supply AS PlaceOfSupply,
                                                    r.basic_buyer_name AS BasicBuyerName,
                                                    r.basic_datetime_of_invoice AS BasicDatetimeOfInvoice,
                                                    r.consignee_pin_number AS ConsigneePinNumber,
                                                    r.consignee_state_name AS ConsigneeStateName,
                                                    r.voucher_key AS VoucherKey,
                                                    r.amount AS Amount,   
                                                    r.persisted_view AS PersistedView,
                                                    o.name AS DistributorCode,
                                                    r.cgst AS Cgst,
                                                    r.sgst AS Sgst,
                                                    r.gst AS Gst,
                                                    r.status AS Status,
                                                    r.created_time AS CreatedTime,
                                                    r.modified_time AS ModifiedTime
                                                FROM 
                                                    sales_order_tally r
                                                Inner join org O on O.uid=r.distributor_org_uid
                                                WHERE 
                                                    distributor_org_uid = @INPUTUID) as sub_query
                                                """);
                }
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster> InventoryItems = await ExecuteQueryAsync<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>
                {
                    PagedData = InventoryItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItem(string uID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        {"UID",  uID}
              };
                var sql = """
                                    SELECT 
                                        id AS Id,
                                        dmsuid AS Dmsuid,
                                        voucher_number AS VoucherNumber,
                                        date AS Date,
                                        guid AS Guid,
                                        state_name AS StateName,
                                        country_of_residence AS CountryOfResidence,
                                        party_name AS PartyName,
                                        voucher_type_name AS VoucherTypeName,
                                        party_ledger_name AS PartyLedgerName,
                                        basic_base_party_name AS BasicBasePartyName,
                                        place_of_supply AS PlaceOfSupply,
                                        basic_buyer_name AS BasicBuyerName,
                                        basic_datetime_of_invoice AS BasicDatetimeOfInvoice,
                                        consignee_pin_number AS ConsigneePinNumber,
                                        consignee_state_name AS ConsigneeStateName,
                                        voucher_key AS VoucherKey,
                                        amount AS Amount,   
                                        persisted_view AS PersistedView,
                                        distributor_code AS DistributorCode,
                                        cgst AS Cgst,
                                        sgst AS Sgst,
                                        gst AS Gst,
                                        status AS Status,
                                        created_time AS CreatedTime,
                                        modified_time AS ModifiedTime
                                    FROM 
                                        sales_order_tally
                                    WHERE guid = @UID
                                    """;
                ;
                return await ExecuteSingleAsync<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResponse<ITallySalesInvoiceLineMaster>> GetTallySalesInvoiceLineMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            SELECT 
                                                id AS Id,
                                                dmsuid AS Dmsuid,
                                                guid AS Guid,
                                                voucher_number AS VoucherNumber,
                                                stock_item_name AS StockItemName,
                                                rate AS Rate,
                                                amount AS Amount,
                                                actual_qty AS ActualQty,
                                                billed_qty AS BilledQty,
                                                gst AS Gst,
                                                discount_percentage AS DiscountPercentage,
                                                qty AS Qty,
                                                unit_price AS UnitPrice,
                                                total_amount AS TotalAmount,
                                                total_discount AS TotalDiscount,
                                                total_tax AS TotalTax,
                                                net_amount AS NetAmount,
                                                created_time AS CreatedTime
                                            FROM sales_order_line_tally
                                            WHERE 
                                                guid = @INPUTUID
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    id AS Id,
                                                    dmsuid AS Dmsuid,
                                                    guid AS Guid,
                                                    voucher_number AS VoucherNumber,
                                                    stock_item_name AS StockItemName,
                                                    rate AS Rate,
                                                    amount AS Amount,
                                                    actual_qty AS ActualQty,
                                                    billed_qty AS BilledQty,
                                                    gst AS Gst,
                                                    discount_percentage AS DiscountPercentage,
                                                    qty AS Qty,
                                                    unit_price AS UnitPrice,
                                                    total_amount AS TotalAmount,
                                                    total_discount AS TotalDiscount,
                                                    total_tax AS TotalTax,
                                                    net_amount AS NetAmount,
                                                    created_time AS CreatedTime
                                                FROM sales_order_line_tally
                                                WHERE 
                                                    guid = @INPUTUID) as sub_query
                                                """);
                }
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster> InventoryItems = await ExecuteQueryAsync<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster>
                {
                    PagedData = InventoryItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<PagedResponse<ITallySalesInvoiceResult>> GetTallySalesInvoiceData(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"INPUTUID",  UID}
                };
                var sql = new StringBuilder("""
                                            SELECT 
                                                SOT.id AS Id,
                                                SOT.dmsuid AS Dmsuid,
                                                SOT.voucher_number AS VoucherNumber,
                                                SOT.date AS Date,
                                                SOT.guid AS Guid,
                                                SOT.state_name AS StateName,
                                                SOT.country_of_residence AS CountryOfResidence,
                                                SOT.party_name AS PartyName,
                                                SOT.voucher_type_name AS VoucherTypeName,
                                                SOT.party_ledger_name AS PartyLedgerName,
                                                SOT.basic_base_party_name AS BasicBasePartyName,
                                                SOT.place_of_supply AS PlaceOfSupply,
                                                SOT.basic_buyer_name AS BasicBuyerName,
                                                SOT.basic_datetime_of_invoice AS DatetimeOfInvoice,
                                                SOT.consignee_pin_number AS ConsigneePinNumber,
                                                SOT.consignee_state_name AS ConsigneeStateName,
                                                SOT.voucher_key AS VoucherKey,
                                                SOT.amount AS Amount,
                                                SOT.persisted_view AS PersistedView,
                                                SOT.distributor_code AS DistributorCode,
                                                SOT.cgst AS Cgst,
                                                SOT.sgst AS Sgst,
                                                SOT.gst AS Gst,
                                                SOT.status AS Status,
                                                SOT.created_time AS CreatedTime,
                                                SOT.modified_time AS ModifiedTime,

                                                SOLT.id AS LineId,
                                                SOLT.dmsuid AS LineDmsuid,
                                                SOLT.guid AS LineGuid,
                                                SOLT.voucher_number AS LineVoucherNumber,
                                                SOLT.stock_item_name AS StockItemName,
                                                SOLT.rate AS Rate,
                                                SOLT.amount AS LineAmount,
                                                SOLT.actual_qty AS ActualQty,
                                                SOLT.billed_qty AS BilledQty,
                                                SOLT.gst AS LineGst,
                                                SOLT.discount_percentage AS DiscountPercentage,
                                                SOLT.qty AS Qty,
                                                SOLT.unit_price AS UnitPrice,
                                                SOLT.total_amount AS TotalAmount,
                                                SOLT.total_discount AS TotalDiscount,
                                                SOLT.total_tax AS TotalTax,
                                                SOLT.net_amount AS NetAmount,
                                                SOLT.created_time AS LineCreatedTime
                                            FROM 
                                                sales_order_tally SOT
                                            LEFT JOIN 
                                                sales_order_line_tally SOLT
                                                ON SOT.guid = SOLT.guid
                                            WHERE 
                                                SOT.distributor_code = @INPUTUID
                                            ORDER BY 
                                                SOT.guid, SOLT.voucher_number
                                            
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    SOT.id AS Id,
                                                    SOT.dmsuid AS Dmsuid,
                                                    SOT.voucher_number AS VoucherNumber,
                                                    SOT.date AS Date,
                                                    SOT.guid AS Guid,
                                                    SOT.state_name AS StateName,
                                                    SOT.country_of_residence AS CountryOfResidence,
                                                    SOT.party_name AS PartyName,
                                                    SOT.voucher_type_name AS VoucherTypeName,
                                                    SOT.party_ledger_name AS PartyLedgerName,
                                                    SOT.basic_base_party_name AS BasicBasePartyName,
                                                    SOT.place_of_supply AS PlaceOfSupply,
                                                    SOT.basic_buyer_name AS BasicBuyerName,
                                                    SOT.basic_datetime_of_invoice AS DatetimeOfInvoice,
                                                    SOT.consignee_pin_number AS ConsigneePinNumber,
                                                    SOT.consignee_state_name AS ConsigneeStateName,
                                                    SOT.voucher_key AS VoucherKey,
                                                    SOT.amount AS Amount,
                                                    SOT.persisted_view AS PersistedView,
                                                    SOT.distributor_code AS DistributorCode,
                                                    SOT.cgst AS Cgst,
                                                    SOT.sgst AS Sgst,
                                                    SOT.gst AS Gst,
                                                    SOT.status AS Status,
                                                    SOT.created_time AS CreatedTime,
                                                    SOT.modified_time AS ModifiedTime,
                                                
                                                    SOLT.id AS Id,
                                                    SOLT.dmsuid AS Dmsuid,
                                                    SOLT.guid AS Guid,
                                                    SOLT.voucher_number AS VoucherNumber,
                                                    SOLT.stock_item_name AS StockItemName,
                                                    SOLT.rate AS Rate,
                                                    SOLT.amount AS Amount,
                                                    SOLT.actual_qty AS ActualQty,
                                                    SOLT.billed_qty AS BilledQty,
                                                    SOLT.gst AS Gst,
                                                    SOLT.discount_percentage AS DiscountPercentage,
                                                    SOLT.qty AS Qty,
                                                    SOLT.unit_price AS UnitPrice,
                                                    SOLT.total_amount AS TotalAmount,
                                                    SOLT.total_discount AS TotalDiscount,
                                                    SOLT.total_tax AS TotalTax,
                                                    SOLT.net_amount AS NetAmount,
                                                    SOLT.created_time AS CreatedTime
                                                FROM 
                                                    sales_order_tally SOT
                                                LEFT JOIN 
                                                    sales_order_line_tally SOLT
                                                    ON SOT.guid = SOLT.guid
                                                WHERE 
                                                    SOT.distributor_code = @INPUTUID
                                                ORDER BY 
                                                    SOT.guid, SOLT.voucher_number 
                                                    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
                                                ) as sub_query
                                                """);
                }
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult> InvoiceItems = await ExecuteQueryAsync<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult>
                {
                    PagedData = InvoiceItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> UpdateTallyMasterData(ITallyDealerMaster data)
        {
            try
            {
                int Retval = 0;
                var sql = @"""UPDATE retailers_from_tally
                            SET 
                                ledger_name = @LedgerName,
                                parent_name = @ParentName,
                                opening_balance = @OpeningBalance,
                                primary_group = @PrimaryGroup,
                                remote_alt_guid = @RemoteAltGuid,
                                address = @Address,
                                address1 = @Address1,
                                address2 = @Address2,
                                country_name = @CountryName,
                                email = @Email,
                                phone = @Phone,
                                pincode = @Pincode,
                                state_name = @StateName,
                                income_tax_number = @IncomeTaxNumber,
                                country_of_residence = @CountryOfResidence,
                                status = @Status,
                                gstin = @Gstin,
                                modified_time = @ModifiedTime
                            WHERE 
                                distributor_org_uid = @DistributorOrgUid  """;
                Retval = await ExecuteNonQueryAsync(sql, data);
                if (Retval == 0)
                {
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }


    }
}
