using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Tally.Model.Classes;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.DL.Classes
{
    public class PGSQLTallyMappingDL : Base.DL.DBManager.PostgresDBManager, Interfaces.ITallyMappingDL
    {
        public PGSQLTallyMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }


        public async Task<ITallyConfigurationResponse> GetTallyConfigurationData(string DistCode)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "DistCode", DistCode }
                };

                var tallyConfigSql = new StringBuilder(@"SELECT 
                                                Id AS Id,
                                                org_uid AS OrgUID,
                                                error_log_type AS ErrorLogType,
                                                base_dir AS BaseDir,
                                                log_folder AS LogFolder,
                                                log_path AS LogPath,
                                                request_log_folder AS RequestLogFolder,
                                                response_log_folder AS ResponseLogFolder,
                                                is_request_log_enabled AS IsRequestLogEnabled,
                                                is_response_log_enabled AS IsResponseLogEnabled,
                                                pull_ledger_from_tally AS PULL_LEDGER_FROM_TALLY,
                                                pull_orders_from_tally AS PULL_ORDERS_FROM_TALLY,
                                                pull_inventory_from_tally AS PULL_INVENTORY_FROM_TALLY,
                                                pull_payment_receipts_from_tally AS PULL_PAYMENT_RECEIPTS_FROM_TALLY,
                                                push_sales_tally AS PUSH_SALES_TO_TALLY,
                                                push_retailer_to_tally AS PUSH_RETAILER_TO_TALLY,
                                                pull_sales_from_tally AS PULL_SALES_FROM_TALLY,
                                                web_serivce_api_endpoint_sales AS WebSerivceApiEndpointSales,
                                                web_serivce_api_endpoint_retailer AS WebSerivceApiEndpointRetailer,
                                                tally_url AS TallyURL,
                                                dms_url AS DMSURL,
                                                country_name AS COUNTRYNAME,
                                                ledger_state_name AS LEDSTATENAME,
                                                gst_registration_type AS GSTREGISTRATIONTYPE,
                                                voucher_type_sales AS VOUCHERTYPE_Sales,
                                                ledger_name_accounting AS LEDGERNAME_ACCOUNTING,
                                                ledger_parent AS LEDGER_PARENT,
                                                stock_godown_name AS STOCK_GODOWN_NAME,
                                                stock_batch_name AS STOCK_BATCH_NAME,
                                                order_discount_name AS ORDER_DISCOUNT_NAME,
                                                round_type AS ROUND_TYPE,
                                                round_type_name AS ROUND_TYPE_NAME,
                                                round_method_name AS ROUND_METHOD_TYPE,
                                                dist_code AS DistCode,
                                                dist_name AS DistName,
                                                ss AS SS,
                                                created_date AS CreatedTime,
                                                modified_date AS ModifiedTime,
                                                push_purchase_to_tally AS PUSH_PURCHASE_TO_TALLY,
                                                voucher_type_purchase AS VOUCHERTYPE_PURCHASE,
                                                ledger_purchase_partyname AS PURCHASE_PARTYNAME,
                                                web_serivce_api_endpoint_purchase AS WebSerivceApiEndpointPurchase
                                                        FROM 
                                                            tally_configuration
                                                        WHERE 
                                                            dist_code = @DistCode
                                                 ");

                Type type = _serviceProvider.GetRequiredService<ITallyConfiguration>().GetType();


                var tallyConfigData = await ExecuteQueryAsync<ITallyConfiguration>(tallyConfigSql.ToString(), parameters, type);

                // Second query for TaxConfiguration
                var taxConfigSql = new StringBuilder(@"
                                               SELECT 
                                                ttm.tax_uid AS TaxUID,
                                                ttm.tax_name AS TaxName,
                                                ttm.ledgername_cgst AS LEDGERNAME_CGST,
                                                ttm.ledgername_sgst AS LEDGERNAME_SGST,
                                                ttm.ledgername_accounting AS LEDGERNAME_ACCOUNTING,
                                                ttm.created_date AS TaxCreatedTime,
                                                ttm.modified_date AS TaxModifiedTime
                                            FROM 
                                                tally_tax_mapping ttm
                                            INNER JOIN 
                                                tally_configuration tc ON ttm.org_uid = tc.org_uid
                                            WHERE 
                                                tc.dist_code=@DistCode
                                    ");

                List<ITaxConfiguration> taxConfigurations = await ExecuteQueryAsync<ITaxConfiguration>(taxConfigSql.ToString(), parameters);

                ITallyConfigurationResponse response = new TallyConfigurationResponse
                {
                    TallyConfigurations = tallyConfigData.FirstOrDefault(),
                    TaxConfigurations = taxConfigurations
                };

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> InsertRetailersFromTally(List<IRetailersFromTally> retailersFromTally)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in retailersFromTally)
                        {
                            var sql = @"
                                    MERGE INTO retailers_from_tally AS target
                                    USING (SELECT @DistributorOrgUID AS distributor_org_uid, @LedgerName AS ledger_name) AS source
                                    ON target.distributor_org_uid = source.distributor_org_uid AND target.ledger_name = source.ledger_name
                                    WHEN MATCHED THEN 
                                        UPDATE SET 
                                            ledger_name = @LedgerName, 
                                            parent_name = @ParentName, 
                                            opening_balance = @OpeningBalance, 
                                            primary_group = @PrimaryGroup, 
                                            remote_alt_guid = @RemoteAltGUID, 
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
                                            gstin = @GSTIN,
                                            modified_time = GETDATE()
                                    WHEN NOT MATCHED THEN
                                        INSERT (ledger_name, parent_name, opening_balance, primary_group, remote_alt_guid, address, address1, 
                                        address2, country_name, email, phone, pincode, state_name, income_tax_number, country_of_residence, 
                                        distributor_code, status, gstin, created_time, modified_time, distributor_org_uid)
                                        VALUES (@LedgerName, @ParentName, @OpeningBalance, @PrimaryGroup, @RemoteAltGUID, @Address, @Address1, 
                                        @Address2, @CountryName, @Email, @Phone, @Pincode, @StateName, @IncomeTaxNumber, @CountryOfResidence, 
                                        @DistributorCode, @Status, @GSTIN, GETDATE(), GETDATE(), @DistributorOrgUID);";


                            Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"LedgerName", data.LedgerName ?? "" },
                                {"ParentName", data.ParentName ?? "" },
                                {"OpeningBalance", data.OpeningBalance ?? "" },
                                {"PrimaryGroup", data.PrimaryGroup ?? "" },
                                {"RemoteAltGUID", data.RemoteAltGUID ?? "" },
                                {"Address", data.Address ?? "" },
                                {"Address1", data.Address1 ?? "" },
                                {"Address2", data.Address2 ?? "" },
                                {"CountryName", "India" },
                                {"Email", data.Email ?? "" },
                                {"Phone", data.Phone ?? "" },
                                {"Pincode", data.Pincode ?? "" },
                                {"StateName", data.StateName ?? "" },
                                {"IncomeTaxNumber", data.IncomeTaxNumber ?? "" },
                                {"CountryOfResidence", data.CountryOfResidence ?? "" },
                                {"DistributorCode", data.DistributorCode ?? "" },
                                {"Status", data.Status ?? "" },
                                {"GSTIN", data.GSTIN ?? "" },
                                {"DistributorOrgUID", data.DistributorOrgUID ?? "" }
                            };

                            Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> InsertInventoryFromTally(List<IInventoryFromTally> inventoryFromTally)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in inventoryFromTally)
                        {
                            var sql = @"
                            MERGE INTO inventory_from_tally AS target
                            USING (SELECT @Name AS name, @DistributorOrgUID AS distributor_org_uid) AS source
                            ON target.name = source.name AND target.distributor_org_uid = source.distributor_org_uid
                            WHEN MATCHED THEN 
                                UPDATE SET 
                                    code = @Code,
                                    units = @Units,
                                    last_record_date = @LastRecordDate,
                                    opening_balance = TRIM(@OpeningBalance),
                                    opening_rate = TRIM(@OpeningRate),
                                    stock_group = @StockGroup,
                                    parent = TRIM(REPLACE(@Parent, CHAR(4), '')),
                                    gst_details = @GSTDetails,
                                    remote_alt_guid = @RemoteAltGUID,
                                    modified_time = GETDATE()
                            WHEN NOT MATCHED THEN
                                INSERT (name, code, units, last_record_date, opening_balance, opening_rate, stock_group, parent, 
                                gst_details, remote_alt_guid, distributor_code, created_time, modified_time, distributor_org_uid)
                                VALUES (@Name, @Code, @Units, @LastRecordDate, TRIM(@OpeningBalance), TRIM(@OpeningRate), @StockGroup, TRIM(REPLACE(@Parent, CHAR(4), '')), 
                                @GSTDetails, @RemoteAltGUID, @DistributorCode, GETDATE(), GETDATE(), @DistributorOrgUID);";

                            Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"Name", data.Name ?? "" },
                                {"Code", data.Code ?? "" },
                                {"Units", data.Units ?? "" },
                                {"LastRecordDate", data.LastRecordDate ?? "" },
                                {"OpeningBalance", data.OpeningBalance ?? "" },
                                {"OpeningRate", data.OpeningRate ?? "" },
                                {"StockGroup", data.StockGroup ?? "" },
                                {"Parent", data.Parent ?? "" },
                                {"GSTDetails", data.GSTDetails ?? "" },
                                {"RemoteAltGUID", data.RemoteAltGUID ?? "" },
                                {"DistributorCode", data.DistributorCode ?? "" },
                                {"DistributorOrgUID", data.DistributorOrgUID ?? "" }
                            };

                            Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> InsertOrdersFromTally(List<ISalesOrderHeaderFromTally> ordersFromTally)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in ordersFromTally)
                        {
                            // Insert into SalesOrderHeader table
                            var salesOrderHeaderSql = @"
                                                    IF NOT EXISTS (SELECT 1 FROM sales_order_tally WHERE distributor_org_uid = @DistributorOrgUID AND guid = @GUID AND voucher_type_name = @VoucherTypeName)
                                                    BEGIN
                                                        INSERT INTO sales_order_tally (distributor_code, dmsuid, voucher_number, date, guid, state_name, country_of_residence, 
                                                            party_name, party_ledger_name, basic_buyer_name, voucher_type_name, basic_base_party_name, persisted_view, 
                                                            place_of_supply, basic_datetime_of_invoice, consignee_pin_number, consignee_state_name, voucher_key, 
                                                            amount, cgst, sgst, gst, created_time, modified_time, distributor_org_uid)
                                                        VALUES (@DistributorCode, @DMSUID, @VoucherNumber, @Date, @GUID, @StateName, @CountryOfResidence, 
                                                            @PartyName, @PartyLedgerName, @BasicBuyerName, @VoucherTypeName, @BasicBasePartyName, @PersistedView, 
                                                            @PlaceOfSupply, @BasicDateTimeOfInvoice, @ConsigneePinNumber, @ConsigneeStateName, @VoucherKey, 
                                                            @Amount, @CGST, @SGST, @GST, GETDATE(), GETDATE(), @DistributorOrgUID);
                                                    END";

                            Dictionary<string, object> salesOrderHeaderParameters = new Dictionary<string, object>
                                    {
                                        {"DistributorCode", data.DistributorCode ?? "" },
                                        {"DMSUID", data.DMSUID ?? "" },
                                        {"VoucherNumber", data.VOUCHERNUMBER ?? "" },
                                        {"Date", data.DATE ?? "" },
                                        {"GUID", data.VOUCHERNUMBER + "_" +  data.GUID ?? "" },
                                        {"StateName", data.STATENAME ?? "" },
                                        {"CountryOfResidence", data.COUNTRYOFRESIDENCE ?? "" },
                                        {"PartyName", data.PARTYNAME ?? "" },
                                        {"PartyLedgerName", data.PARTYLEDGERNAME ?? "" },
                                        {"BasicBuyerName", data.BASICBUYERNAME ?? "" },
                                        {"VoucherTypeName", data.VOUCHERTYPENAME ?? "" },
                                        {"BasicBasePartyName", data.BASICBASEPARTYNAME ?? "" },
                                        {"PersistedView", data.PERSISTEDVIEW ?? "" },
                                        {"PlaceOfSupply", data.PLACEOFSUPPLY ?? "" },
                                        {"BasicDateTimeOfInvoice", data.BASICDATETIMEOFINVOICE ?? "" },
                                        {"ConsigneePinNumber", data.CONSIGNEEPINNUMBER ?? "" },
                                        {"ConsigneeStateName", data.CONSIGNEESTATENAME ?? "" },
                                        {"VoucherKey", data.VOUCHERKEY ?? "" },
                                        {"Amount", data.AMOUNT ?? "" },
                                        {"CGST", data.CGST ?? "" },
                                        {"SGST", data.SGST ?? "" },
                                        {"GST", data.GST ?? "" },
                                        {"DistributorOrgUID", data.DistributorOrgUID ?? "" }
                                    };

                            Retval = await ExecuteNonQueryAsync(salesOrderHeaderSql, conn, transaction, salesOrderHeaderParameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            // Insert into SalesOrderLine table for each line item
                            foreach (var line in data.SalesOrderLines)
                            {
                                var salesOrderLineSql = @"
                                                         IF NOT EXISTS(select 1 from sales_order_line_tally where guid=@Guid AND stock_item_name=@StockItemName) 
                                                         BEGIN
                                                        INSERT INTO sales_order_line_tally (dmsuid, guid, voucher_number, stock_item_name, rate, discount_percentage, amount, actual_qty, 
                                                            billed_qty, gst, qty, unit_price, total_amount, total_discount, total_tax,net_amount, created_time
                                                        )
                                                        VALUES (@Dmsuid, @Guid, @VoucherNumber, @StockItemName, @Rate, @Discount, @Amount, @ActualQty, @BilledQty, CAST(@Gst AS DECIMAL(18, 2)), 

                                                        -- qty
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)),

                                                        -- unit_price
                                                        CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)),

                                                        -- total_amount
                                                        CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)),

                                                        -- total_discount
                                                        CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)) 
                                                        * (@Discount / 100.0),

                                                        -- total_tax
                                                        CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)) * 
                                                        (CAST(@Gst AS DECIMAL(18, 2)) / 100.0),

                                                        -- net_amount
                                                        CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)) - 
                                                        (CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)) * 
                                                        (@Discount / 100.0)) + (CAST(SUBSTRING(@Rate, 1, CHARINDEX('/', @Rate) - 1) AS DECIMAL(18, 2)) * 
                                                        CAST(SUBSTRING(LTRIM(@BilledQty), 1, CHARINDEX(' ', LTRIM(@BilledQty)) - 1) AS DECIMAL(18, 2)) * 
                                                        (CAST(@Gst AS DECIMAL(18, 2)) / 100.0)),

                                                        GETDATE());
                                                        END";

                                Dictionary<string, object> salesOrderLineParameters = new Dictionary<string, object>
                                {
                                    {"Dmsuid", data.DMSUID ?? "" },
                                    {"Guid", data.VOUCHERNUMBER + "_" +  data.GUID ?? "" },
                                    {"VoucherNumber", data.VOUCHERNUMBER ?? "" },
                                    {"StockItemName", line.STOCKITEMNAME ?? "" },
                                    {"Rate", line.RATE ?? "" },
                                    {"Discount", line.DISCOUNT ?? "" },
                                    {"Amount", line.AMOUNT ?? "" },
                                    {"ActualQty", line.ACTUALQTY ?? "" },
                                    {"BilledQty", line.BILLEDQTY ?? "" },
                                    {"Gst", line.GST ?? "" }
                                };

                                Retval = await ExecuteNonQueryAsync(salesOrderLineSql, conn, transaction, salesOrderLineParameters);
                                if (Retval == 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ITallySKU>> GetDistMappedSKUList(string DistCode)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "DistCode", DistCode }
                };

                var sql = new StringBuilder(@"SELECT 
                                            DISTINCT
                                            REPLACE(REPLACE(REPLACE(distributor_sku_name, '&', '&amp;'), '>', '&gt;'), '<', '&lt;') AS SkuName
                                            FROM tally_sku_mapping
                                            where distributor_code=@DistCode
                                            ORDER BY SkuName
                                            ");
                var GetDistMappedSKUList = await ExecuteQueryAsync<Model.Interfaces.ITallySKU>(sql.ToString(), parameters);
                return GetDistMappedSKUList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<IRetailersFromDB>> GetRetailersFromDB(string orgUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "OrgUid", orgUID }
                };

                var sql = new StringBuilder(@"SELECT DISTINCT
                                                S.uid AS LedgerUID,
                                                S.code AS ledgerCode,
                                                REPLACE(REPLACE(REPLACE(S.name, '&', '&amp;'), '>', '&gt;'), '<', '&lt;') AS ledgerName,
                                                S.bill_to_store_uid AS parentName,
                                                O.uid AS ledgerOrgUID,
                                                O.name AS ledgerOrgName,
                                                REPLACE(REPLACE(REPLACE(A.line1, '&', '&amp;'), '>', '&gt;'), '<', '&lt;') AS ADDRESS,
                                                REPLACE(REPLACE(REPLACE(A.line2, '&', '&amp;'), '>', '&gt;'), '<', '&lt;') AS ADDRESS1,
                                                REPLACE(REPLACE(REPLACE(A.line3, '&', '&amp;'), '>', '&gt;'), '<', '&lt;') AS ADDRESS2,
                                                'India' AS COUNTRYNAME,
                                                C.email AS EMAIL,
                                                C.phone AS PHONE,
                                                A.zip_code AS PINCODE,
                                                A.state_code AS STATENAME,
                                                S.tax_doc_number AS INCOMETAXNUMBER,
                                                A.pan AS PANNUMBER,
                                                'India' AS COUNTRYOFRESIDENCE
                                            FROM Store S
									        INNER JOIN org O on O.uid = S.franchisee_org_uid
                                            INNER JOIN Contact C ON C.linked_item_uid = S.UID AND C.linked_item_type = 'Store' AND C.is_default = 1
                                            INNER JOIN Address A ON A.linked_item_uid = S.UID AND A.linked_item_type = 'Store' AND A.[name] = 'Bill-to'
                                            LEFT JOIN store_status SS ON S.UID = SS.store_uid
                                            WHERE 
									        (SS.UID IS NULL OR SS.push_status = 1) AND 
									        S.franchisee_org_uid = @OrgUid");

                var retailersFromDB = await ExecuteQueryAsync<Model.Interfaces.IRetailersFromDB>(sql.ToString(), parameters);

                // Insert into `store_status` table for each retrieved retailer
                if (retailersFromDB != null && retailersFromDB.Count > 0)
                {
                    foreach (var retailer in retailersFromDB)
                    {
                        var insertSql = @"INSERT INTO store_status
                                    (uid, store_uid, is_pushed, pushed_on, created_time, modified_time, server_add_time, server_modified_time)
                                  VALUES
                                    (@Uid, @StoreUid, @IsPushed, GETDATE(), GETDATE(), GETDATE(), GETDATE(), GETDATE())";

                        var insertParameters = new Dictionary<string, object>
                {
                    { "Uid", retailer.ledgerUID },
                    { "StoreUid", retailer.ledgerUID },
                    { "IsPushed", 1 }
                };

                        await ExecuteNonQueryAsync(insertSql, insertParameters);
                    }
                }

                return retailersFromDB;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RetailerStatusFromTally(List<IRetailerTallyStatus> retailerStatusFromTally)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in retailerStatusFromTally)
                        {
                            var sql = @"
                            UPDATE store_status SET push_status=@PushStatus, push_message=@PushMessage WHERE store_uid=@StoreUID;";

                            Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"StoreUID", data.LedgerNumber ?? "" },
                                {"PushStatus", data.Status },
                                {"PushMessage", data.StatusText ?? "" }
                            };

                            Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<List<ISalesOrderHeaderFromDB>> GetSalesOrderFromDB(string orgUID)
        {
            List<ISalesOrderHeaderFromDB> salesOrderList = new List<ISalesOrderHeaderFromDB>();

            try
            {
                var parameters = new { OrgUid = orgUID };

                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = "sp_get_all_pending_sales_order_tobe_pushed_to_tally_v1";

                    connection.Open();

                    using (var multi = await connection.QueryMultipleAsync(query, parameters, commandType: CommandType.StoredProcedure))
                    {
                        var salesOrders = (await multi.ReadAsync<SalesOrderHeaderFromDB>()).ToList();
                        var salesOrderLines = (await multi.ReadAsync<SalesOrderLineFromDB>()).ToList();

                        List<ISalesOrderTaxDetailsTally> lstInvoicesTaxDetails = new List<ISalesOrderTaxDetailsTally>();

                        foreach (var salesOrder in salesOrders)
                        {
                            decimal TotalCGST = 0;
                            decimal TotalSGST = 0;
                            decimal? TotalAmountWithDiscount = 0;
                            salesOrder.Date = Convert.ToDateTime(salesOrder.OrderDate).ToString("yyyyMMdd");
                            salesOrder.OrderDate = Convert.ToDateTime(salesOrder.OrderDate).ToString("d-MMM-yyyy") + " at " + Convert.ToDateTime(salesOrder.OrderDate).ToString("HH:mm");

                            var matchingLines = salesOrderLines
                                .Where(line => line.SalesOrderUID == salesOrder.UID)
                                .ToList();

                            foreach (var salesOrderLine in matchingLines)
                            {
                                salesOrderLine.VOUCHERNUMBER = salesOrder.Name;
                                salesOrderLine.Quantity = salesOrderLine.Quantity + " " + salesOrderLine.UOM;
                                salesOrderLine.Rate = salesOrderLine.Rate.ToString() + "/" + salesOrderLine.UOM;

                                if (decimal.TryParse(salesOrderLine.ItemAmount, out decimal itemAmount))
                                {
                                    salesOrderLine.ItemAmount = Math.Round(itemAmount, 2).ToString();
                                }
                                else
                                {
                                    salesOrderLine.ItemAmount = "0.00";
                                }

                                if (decimal.TryParse(salesOrderLine.DiscountAmount, out decimal discountAmount))
                                {
                                    salesOrderLine.DiscountAmount = Math.Round(discountAmount, 2).ToString();
                                }
                                else
                                {
                                    salesOrderLine.DiscountAmount = "0.00";
                                }

                                if (decimal.TryParse(salesOrderLine.TotalTax, out decimal totalTaxLine))
                                {
                                    double roundedTaxLine = Math.Round((double)(totalTaxLine / 2), 3);

                                    TotalCGST += (decimal)roundedTaxLine;
                                    TotalSGST += (decimal)roundedTaxLine;

                                    string TaxUID = salesOrderLine.TaxUID;
                                    UpdateOrAddTaxDetail(lstInvoicesTaxDetails, TaxUID, (decimal)roundedTaxLine);
                                }
                                else
                                {
                                    TotalCGST += 0;
                                    TotalSGST += 0;
                                }

                                if (decimal.TryParse(salesOrderLine.TotalAmountWithDiscount, out decimal DiscountWithAmount))
                                {
                                    double roundedDiscountWithAmount = Math.Round((double)DiscountWithAmount, 2);
                                    salesOrderLine.TotalAmountWithDiscount = roundedDiscountWithAmount.ToString();
                                    TotalAmountWithDiscount += DiscountWithAmount;
                                }
                                else
                                {
                                    salesOrderLine.TotalAmountWithDiscount = "0";
                                }
                            }
                            salesOrder.lstSalesOrderTaxDetails = lstInvoicesTaxDetails;
                            salesOrder.CGST = TotalCGST.ToString();
                            salesOrder.SGST = TotalSGST.ToString();
                            salesOrder.Amount = (-1 * (TotalAmountWithDiscount + TotalCGST + TotalSGST)).ToString();
                            salesOrder.lstSalesOrderLineFromDB = matchingLines.Cast<ISalesOrderLineFromDB>().ToList();
                            salesOrderList.Add(salesOrder);
                        }

                        return salesOrderList;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void UpdateOrAddTaxDetail(List<ISalesOrderTaxDetailsTally> taxDetailsList, string taxUID, decimal taxAmount)
        {
            var existingDetail = taxDetailsList.FirstOrDefault(d => d.TaxUID == taxUID);

            if (existingDetail != null)
            {
                existingDetail.TaxAmount += taxAmount;
                existingDetail.TaxAmountStr = existingDetail.TaxAmount.ToString("0.00");
            }
            else
            {
                var newDetail = new SalesOrderTaxDetailsTally
                {
                    TaxUID = taxUID,
                    TaxAmount = taxAmount,
                    TaxAmountStr = taxAmount.ToString("0.00")
                };
                taxDetailsList.Add(newDetail);
            }
        }
        public async Task<bool> SalesStatusFromTally(List<ISalesTallyStatus> salesStatusFromTally)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in salesStatusFromTally)
                        {
                            var sql = @"
                            UPDATE tally_sales_order_staging SET erp_status=@Status,erp_message=@StatusText,attribute_5=@VoucherId   
                            WHERE sales_order_number=@SalesOrderNumber;";

                            Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"SalesOrderNumber", data.SalesOrderNumber ?? "" },
                                {"Status", data.Status },
                                {"StatusText", data.StatusText ?? "" },
                                {"VoucherId", data.VoucherId ?? "" }
                            };

                            Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region Mahir
        public async Task<PagedResponse<ITallySKU>> GetAllTallySKU(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from(SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                              modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                                              server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                                              ss AS Ss, distributor_code AS DistributorCode, 
                                              sku_code AS SkuCode, sku_name AS SkuName
                                              FROM dbo.tally_sku) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, 
                                              modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                                              server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                                              ss AS Ss, distributor_code AS DistributorCode, 
                                              sku_code AS SkuCode, sku_name AS SkuName
                                              FROM dbo.tally_sku) as SubQuery");
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" Where ");
                    AppendFilterCriteria<Model.Interfaces.ITallySKU>(filterCriterias, sbFilterCriteria, parameters);
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
                else
                {
                    sql.Append(" ORDER BY Id ");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                IEnumerable<Model.Interfaces.ITallySKU> TallyList = await ExecuteQueryAsync<Model.Interfaces.ITallySKU>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKU> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKU>
                {
                    PagedData = TallyList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<PagedResponse<ITallySKUMapping>> GetAllTallySKUMappingByDistCode(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Code, string Tab)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (select ts.principal_sku_code as PrincipalSKUCode,s.name as PrincipalSKUName,ts.distributor_sku_code as DistributorSKUCode,
                                            ts.distributor_sku_name as DistributorSKUName,ts.principal_sku_code as TSCode, ts.id as Id,ts.distributor_code as DistributorCode from 
                                            tally_sku_mapping ts left join sku s on ts.principal_sku_code = s.code ) as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select ts.principal_sku_code as PrincipalSKUCode,s.name as PrincipalSKUName,ts.distributor_sku_code as DistributorSKUCode,
                                            ts.distributor_sku_name as DistributorSKUName,ts.principal_sku_code as TSCode, ts.id as Id,ts.distributor_code as DistributorCode from 
                                            tally_sku_mapping ts left join sku s on ts.principal_sku_code = s.code
                                                ) as SubQuery");
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"Code",  Code}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    if (Tab == TallyConstants.UnMappedWithCMI)
                        sbFilterCriteria.Append(" where DistributorCode =@Code and PrincipalSKUCode is  null and ");
                    if (Tab == TallyConstants.MappedWithCMI)
                        sbFilterCriteria.Append(" where DistributorCode=@Code and PrincipalSKUCode is not null and ");
                    AppendFilterCriteria<Model.Interfaces.ITallySKU>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    if (Tab == TallyConstants.UnMappedWithCMI)
                        sbFilterCriteria.Append(" where  DistributorCode =@Code and PrincipalSKUCode is  null ");
                    if (Tab == TallyConstants.MappedWithCMI)
                        sbFilterCriteria.Append(" where DistributorCode=@Code and PrincipalSKUCode is not null ");
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
                else
                {
                    sql.Append(" ORDER BY Id ");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                IEnumerable<Model.Interfaces.ITallySKUMapping> TallyList = await ExecuteQueryAsync<Model.Interfaces.ITallySKUMapping>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> pagedResponse = new PagedResponse<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>
                {
                    PagedData = TallyList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectSKUByOrgUID(string OrgUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , OrgUID}
            };
            var sql = @"SELECT 
                id AS Id,
                uid AS UID,
                ss AS SS,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                org_uid AS OrgUID,
                code AS Code,
                name AS Name,
                arabic_name AS ArabicName,
                alias_name AS AliasName,
                long_name AS LongName,
                base_uom AS BaseUOM,
                outer_uom AS OuterUOM,
                from_date AS FromDate,
                to_date AS ToDate,
                is_stockable AS IsStockable,
                parent_uid AS ParentUID,
                is_active AS IsActive,
                is_third_party AS IsThirdParty,
                supplier_org_uid AS SupplierOrgUID
                --sku_image AS SKUImage,
                --catalogue_url AS CatalogueURL
            FROM 
                sku
            WHERE 
                org_uid = @UID";
            List<Winit.Modules.SKU.Model.Interfaces.ISKU> skuDetails = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKU>(sql, parameters);
            return skuDetails;
        }
        public async Task<bool> InsertTallySKUMapping(ITallySKUMapping tallySKUMapping)
        {
            try
            {
                int Retval = 0;
                var sql = @"INSERT INTO tally_sku_mapping
                               (uid, created_by, created_time, modified_by, modified_time, 
                                server_add_time, server_modified_time, ss, distributor_code, 
                                principal_sku_code, principal_sku_name, distributor_sku_name)
                                VALUES
                               (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                                @ServerAddTime, @ServerModifiedTime, @Ss, @DistributorCode, 
                                @PrincipalSkuCode, @PrincipalSkuName, @DistributorSkuName)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                            {"UID", tallySKUMapping.UID },
                            {"CreatedBy", tallySKUMapping.CreatedBy ?? "" },
                            {"CreatedTime", DateTime.Now },
                            {"ModifiedBy", tallySKUMapping.ModifiedBy ?? "" },
                            {"ModifiedTime", DateTime.Now },
                            {"ServerAddTime", DateTime.Now },
                            {"ServerModifiedTime", DateTime.Now },
                            {"Ss", 0 },
                            {"DistributorCode", tallySKUMapping.DistributorCode ?? ""},
                            {"PrincipalSkuCode", tallySKUMapping.PrincipalSKUCode ?? "" },
                            {"PrincipalSkuName", tallySKUMapping.PrincipalSKUName ?? "" },
                            {"DistributorSkuName", tallySKUMapping.DistributorSKUName ?? "" },
                        };
                Retval = await ExecuteNonQueryAsync(sql, parameters);
                if (Retval == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> UpdateTallySKUMapping(ITallySKUMapping tallySKUMapping)
        {
            try
            {
                int Retval = 0;
                var sql = @"UPDATE dbo.tally_sku_mapping 
                   SET 
                       modified_by = @ModifiedBy, 
                       modified_time = @ModifiedTime, 
                       server_modified_time = @ServerModifiedTime, 
                       principal_sku_code = @PrincipalSKUCode, 
                       principal_sku_name = @PrincipalSKUName 
                WHERE distributor_sku_name = @DistributorSKUName and distributor_code = @DistributorCode;";
                Retval = await ExecuteNonQueryAsync(sql, tallySKUMapping);
                if (Retval == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> SelectTallySKUMappingBySKUCode(string OrgUID, string DistCode)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"DistributorCode" , DistCode},
                {"PrincipalSKUCode" , OrgUID}
            };
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, 
                server_add_time, server_modified_time, ss, distributor_code, 
                principal_sku_code, principal_sku_name, distributor_sku_name,distributor_uom, principle_uom
                FROM tally_sku_mapping WHERE principal_sku_code = @PrincipalSKUCode and distributor_code = @DistributorCode";
            Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping skuDetails = await ExecuteSingleAsync<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>(sql, parameters);
            return skuDetails;
        }

        public async Task<List<IEmp>> GetAllDistributors()
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };
                var sql = @"Select jp.*,'[' + o.code + ']' + o.name as code, e.code as name  from job_position jp 
                            inner join emp e on jp.emp_uid = e.uid
                            inner join org o on o.uid = jp.org_uid
                            where jp.user_role_uid = 'Distributor'";
                List<IEmp> distributors = await ExecuteQueryAsync<IEmp>(sql, parameters);
                return distributors;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
