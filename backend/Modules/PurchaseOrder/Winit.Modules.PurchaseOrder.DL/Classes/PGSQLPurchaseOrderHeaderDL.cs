using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLPurchaseOrderHeaderDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPurchaseOrderHeaderDL
{
    private readonly IPurchaseOrderLineDL _purchaseOrderLineDL;

    public PGSQLPurchaseOrderHeaderDL(IServiceProvider serviceProvider, IConfiguration config,
        IPurchaseOrderLineDL purchaseOrderLineDL)
        : base(serviceProvider, config)
    {
        _purchaseOrderLineDL = purchaseOrderLineDL;
    }

    public async Task<PagedResponse<IPurchaseOrderHeaderItem>> GetPurchaseOrderHeadersAsync(
        List<SortCriteria> sortCriterias,
        int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new("""
                                    SELECT
                                        poh.uid AS UID,
                                        poh.org_uid AS OrgUID,
                                        poh.division_uid AS DivisionUID,
                                        poh.warehouse_uid AS WareHouseUID,
                                        wh.name AS WarehouseName,
                                        poh.order_date AS OrderDate,
                                        poh.order_number AS OrderNumber,
                                        poh.draft_order_number AS DraftOrderNumber,
                                        poh.expected_delivery_date AS RequestedDeliveryDate,
                                        poh.shipping_address_uid AS ShippingAddressUID,
                                        poh.billing_address_uid AS BillingAddressUID,
                                        poh.status AS Status,
                                        poh.erp_status AS ERPStatus,
                                        poh.qty_count AS QtyCount,
                                        poh.line_count AS LineCount,
                                        poh.total_amount AS TotalAmount,
                                        poh.net_amount AS NetAmount,
                                        poh.app1_emp_uid AS App1EmpUID,
                                        e1.name AS App1EmpName,
                                        poh.app2_emp_uid AS App2EmpUID,
                                        e2.name AS App2EmpName,
                                        poh.app3_emp_uid AS App3EmpUID,
                                        e3.name AS App3EmpName,
                                        poh.app4_emp_uid AS App4EmpUID,
                                        e4.name AS App4EmpName,
                                        poh.app5_emp_uid AS App5EmpUID,
                                        e5.name AS App5EmpName,
                                        poh.app6_emp_uid AS App6EmpUID,
                                        e6.name AS App6EmpName
                                    FROM
                                        purchase_order_header poh
                                    LEFT JOIN
                                        store wh ON poh.warehouse_uid = wh.uid
                                    LEFT JOIN
                                        emp e1 ON poh.app1_emp_uid = e1.uid
                                    LEFT JOIN
                                        emp e2 ON poh.app2_emp_uid = e2.uid
                                    LEFT JOIN
                                        emp e3 ON poh.app3_emp_uid = e3.uid
                                    LEFT JOIN
                                        emp e4 ON poh.app4_emp_uid = e4.uid
                                    LEFT JOIN
                                        emp e5 ON poh.app5_emp_uid = e5.uid
                                    LEFT JOIN
                                        emp e6 ON poh.app6_emp_uid = e6.uid

                                    """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM public.purchase_order_header poh");
            }

            Dictionary<string, object> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                // Map filter criteria column names to database columns
                foreach (var filterCriteria in filterCriterias)
                {
                    if (!filterCriteria.Name.Contains('.'))
                    {
                        // Map frontend field names to database column names and set data types
                        var originalName = filterCriteria.Name.ToLower();
                        filterCriteria.Name = originalName switch
                        {
                            "startdate" => "poh.order_date",
                            "enddate" => "poh.expected_delivery_date",
                            _ => $"poh.{originalName}"  // Use lowercase column name for PostgreSQL case sensitivity
                        };

                        // Set data type for date fields to ensure proper casting
                        if (originalName == "startdate" || originalName == "enddate")
                        {
                            filterCriteria.DataType = typeof(DateTime);
                        }
                    }
                }

                StringBuilder sbFilterCriteria = new();
                AppendFilterCriteria<PurchaseOrderHeaderItem>(filterCriterias, sbFilterCriteria, parameters);

                if (sbFilterCriteria.Length > 0)
                {
                    _ = sql.Append(" WHERE ").Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        _ = sqlCount.Append(" WHERE ").Append(sbFilterCriteria);
                    }
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                // Map frontend field names to database column names
                foreach (var sortCriteria in sortCriterias)
                {
                    var sortParamLower = sortCriteria.SortParameter.ToLower();
                    sortCriteria.SortParameter = sortParamLower switch
                    {
                        "startdate" => "poh.order_date",
                        "enddate" => "poh.expected_delivery_date",
                        _ => sortCriteria.SortParameter.Contains('.')
                            ? sortCriteria.SortParameter
                            : $"poh.{sortParamLower}"  // Use lowercase column name for PostgreSQL case sensitivity
                    };
                }

                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            }

            //Data
            Type type = _serviceProvider
                .GetRequiredService<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem>().GetType();
            IEnumerable<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem> returnOrderLines =
                await ExecuteQueryAsync<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem>(
                sql.ToString(), parameters, type);
            //Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem> pagedResponse = new()
            {
                PagedData = returnOrderLines, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> CreatePurchaseOrderHeaders(List<IPurchaseOrderHeader> purchaseOrderHeaders,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            // Clean up the data - convert "string" placeholder values to null
            foreach (var header in purchaseOrderHeaders)
            {
                // Handle mandatory fields that have foreign key constraints
                // Set to null to bypass FK constraint - DB should allow nullable
                if (header.CreatedBy == "string" || string.IsNullOrWhiteSpace(header.CreatedBy))
                    header.CreatedBy = null;
                if (header.ModifiedBy == "string" || string.IsNullOrWhiteSpace(header.ModifiedBy))
                    header.ModifiedBy = null;

                // Handle optional foreign key fields - set to null if placeholder value
                if (header.BillingAddressUID == "string") header.BillingAddressUID = null!;
                if (header.ShippingAddressUID == "string") header.ShippingAddressUID = null;

                // CRITICAL FIX: Handle invalid org_uid values
                if (string.IsNullOrWhiteSpace(header.OrgUID) ||
                    header.OrgUID == "string" ||
                    header.OrgUID == "default-org" ||
                    header.OrgUID == "null")
                {
                    header.OrgUID = null;
                }

                if (header.DivisionUID == "string") header.DivisionUID = null;
                if (header.WareHouseUID == "string") header.WareHouseUID = null;
                if (header.PurchaseOrderTemplateHeaderUID == "string") header.PurchaseOrderTemplateHeaderUID = null;

                // CRITICAL FIX: Handle address UIDs to prevent FK constraint violations
                // NULL is required for foreign key constraints when no address exists
                if (string.IsNullOrWhiteSpace(header.ShippingAddressUID) || header.ShippingAddressUID == "null")
                {
                    header.ShippingAddressUID = null;
                }
                if (string.IsNullOrWhiteSpace(header.BillingAddressUID) || header.BillingAddressUID == "null")
                {
                    header.BillingAddressUID = null;
                }

                if (header.App1EmpUID == "string") header.App1EmpUID = null;
                if (header.App2EmpUID == "string") header.App2EmpUID = null;
                if (header.App3EmpUID == "string") header.App3EmpUID = null;
                if (header.App4EmpUID == "string") header.App4EmpUID = null;
                if (header.App5EmpUID == "string") header.App5EmpUID = null;
                if (header.App6EmpUID == "string") header.App6EmpUID = null;
                if (header.TaxData == "string") header.TaxData = null!;

                // Also handle branch and HO org UIDs
                if (header.BranchUID == "string") header.BranchUID = null!;
                if (header.HOOrgUID == "string") header.HOOrgUID = null!;
                if (header.ReportingEmpUID == "string") header.ReportingEmpUID = null;

                // Ensure timestamps are set
                if (header.CreatedTime == default) header.CreatedTime = DateTime.Now;
                if (header.ModifiedTime == default) header.ModifiedTime = DateTime.Now;
                if (header.ServerAddTime == default) header.ServerAddTime = DateTime.Now;
                if (header.ServerModifiedTime == default) header.ServerModifiedTime = DateTime.Now;
            }

            string sql = """
                         INSERT INTO purchase_order_header (
                             uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                             org_uid, division_uid, has_template, purchase_order_template_header_uid, warehouse_uid, order_date,
                             order_number, draft_order_number,
                             expected_delivery_date,
                             shipping_address_uid, billing_address_uid, status, qty_count, line_count, total_amount, total_discount,
                             line_discount, header_discount, total_tax_amount, line_tax_amount, header_tax_amount, net_amount,
                             available_credit_limit, tax_data, app1_emp_uid, app2_emp_uid, app3_emp_uid, app4_emp_uid, app5_emp_uid,
                             app6_emp_uid, app1_date, app2_date, app3_date, app4_date, app5_date, app6_date
                         ) VALUES (
                             @UID, @SS,
                             CASE WHEN @CreatedBy IS NULL OR @CreatedBy = '' THEN (SELECT uid FROM emp LIMIT 1) ELSE @CreatedBy END,
                             @CreatedTime,
                             CASE WHEN @ModifiedBy IS NULL OR @ModifiedBy = '' THEN (SELECT uid FROM emp LIMIT 1) ELSE @ModifiedBy END,
                             @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                             @OrgUID, @DivisionUID, @HasTemplate, @PurchaseOrderTemplateHeaderUID, @WareHouseUID, @OrderDate,
                             @OrderNumber, @DraftOrderNumber,
                             @ExpectedDeliveryDate,
                             @ShippingAddressUID, @BillingAddressUID, @Status, @QtyCount, @LineCount, @TotalAmount, @TotalDiscount,
                             @LineDiscount, @HeaderDiscount, @TotalTaxAmount, @LineTaxAmount, @HeaderTaxAmount, @NetAmount,
                             @AvailableCreditLimit, COALESCE(NULLIF(@TaxData, '')::json, '{}'::json), @App1EmpUID, @App2EmpUID, @App3EmpUID, @App4EmpUID, @App5EmpUID,
                             @App6EmpUID, @App1Date, @App2Date, @App3Date, @App4Date, @App5Date, @App6Date
                         );
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderHeaders);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CUD_PurchaseOrder(List<IPurchaseOrderMaster> purchaseOrderMasters)
    {
        try
        {
            using IDbConnection connection = PostgreConnection();
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();

            // Handle UPDATE operations
            if (purchaseOrderMasters.First().ActionType == ActionType.Update)
            {
                int purchaseOrderHeaderUpdateCount = await UpdatePurchaseOrderHeader(
                    [purchaseOrderMasters.First().PurchaseOrderHeader!], connection, transaction);
                if (purchaseOrderHeaderUpdateCount != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                // Check which lines already exist in the database
                List<string>? existingUids = await CheckIfUIDExistsInDB("purchase_order_line",
                    purchaseOrderMasters.First().PurchaseOrderLines!.Select(e => e.UID).ToList(), connection, transaction);
                existingUids ??= new List<string>();

                // Separate lines into update and create lists
                List<IPurchaseOrderLine> purchaseOrderLinesforUpdate =
                    purchaseOrderMasters.First().PurchaseOrderLines!.FindAll(e => existingUids!.Contains(e.UID));
                List<IPurchaseOrderLine> purchaseOrderLinesforAdd =
                    purchaseOrderMasters.First().PurchaseOrderLines!.FindAll(e => !existingUids!.Contains(e.UID));

                // Update existing lines
                if (purchaseOrderLinesforUpdate != null && purchaseOrderLinesforUpdate.Any() &&
                    await _purchaseOrderLineDL.UpdatePurchaseOrderLines
                        (purchaseOrderLinesforUpdate, connection, transaction) != purchaseOrderLinesforUpdate.Count)
                {
                    transaction.Rollback();
                    return false;
                }

                // Add new lines
                if (purchaseOrderLinesforAdd != null && purchaseOrderLinesforAdd.Any() &&
                    await _purchaseOrderLineDL.CreatePurchaseOrderLines
                        (purchaseOrderLinesforAdd, connection, transaction) != purchaseOrderLinesforAdd.Count)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }

            // Handle CREATE (Add) operations
            if (purchaseOrderMasters.First().ActionType == ActionType.Add)
            {
                foreach (IPurchaseOrderMaster purchaseOrder in purchaseOrderMasters)
                {
                    if (!string.IsNullOrEmpty(purchaseOrder.PurchaseOrderHeader!.OrderNumber))
                    {
                        purchaseOrder.PurchaseOrderHeader!.DraftOrderNumber = null;
                    }

                    if (await CreatePurchaseOrderHeaders([purchaseOrder.PurchaseOrderHeader!], connection, transaction) != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    if (await _purchaseOrderLineDL.CreatePurchaseOrderLines
                            (purchaseOrder.PurchaseOrderLines!, connection, transaction) !=
                        purchaseOrder.PurchaseOrderLines!.Count)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<PagedResponse<IPurchaseOrderHeader>> GetAllPurchaseOrderHeaders(List<SortCriteria>? sortCriterias,
        int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(
            """
            SELECT  poh.uid AS UID,
            poh.ss AS SS,
            poh.created_by AS CreatedBy,
            poh.created_time AS CreatedTime,
            poh.modified_by AS ModifiedBy,
            poh.modified_time AS ModifiedTime,
            poh.server_add_time AS ServerAddTime,
            poh.server_modified_time AS ServerModifiedTime,
            poh.org_uid AS OrgUID,
            poh.division_uid AS DivisionUID,
            poh.has_template AS HasTemplate,
            poh.purchase_order_template_header_uid AS PurchaseOrderTemplateHeaderUID,
            poh.warehouse_uid AS WareHouseUID,
            poh.order_date AS OrderDate,
            poh.order_number AS OrderNumber,
            poh.draft_order_number AS DraftOrderNumber,
            poh.expected_delivery_date AS ExpectedDeliveryDate,
            poh.shipping_address_uid AS ShippingAddressUID,
            poh.billing_address_uid AS BillingAddressUID,
            poh.status AS Status,
            poh.qty_count AS QtyCount,
            poh.line_count AS LineCount,
            poh.total_amount AS TotalAmount,
            poh.total_discount AS TotalDiscount,
            poh.line_discount AS LineDiscount,
            poh.header_discount AS HeaderDiscount,
            poh.total_tax_amount AS TotalTaxAmount,
            poh.line_tax_amount AS LineTaxAmount,
            poh.header_tax_amount AS HeaderTaxAmount,
            poh.net_amount AS NetAmount,
            poh.available_credit_limit AS AvailableCreditLimit,
            poh.tax_data AS TaxData,
            poh.app1_emp_uid AS App1EmpUID,
            poh.app2_emp_uid AS App2EmpUID,
            poh.app3_emp_uid AS App3EmpUID,
            poh.app4_emp_uid AS App4EmpUID,
            poh.app5_emp_uid AS App5EmpUID,
            poh.app6_emp_uid AS App6EmpUID,
            poh.app1_date AS App1Date,
            poh.app2_date AS App2Date,
            poh.app3_date AS App3Date,
            poh.app4_date AS App4Date,
            poh.app5_date AS App5Date,
            poh.app6_date AS App6Date,
            COALESCE(org.name, franchise_org.name) AS OrgName,
            COALESCE(org.code, franchise_org.code) AS OrgCode,
            wh.name AS WarehouseName
            FROM purchase_order_header poh
            LEFT JOIN store wh ON poh.warehouse_uid = wh.uid
            LEFT JOIN org franchise_org ON wh.franchisee_org_uid = franchise_org.uid
            LEFT JOIN org ON poh.org_uid = org.uid
            """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                """
                    SELECT count(*)
                    FROM purchase_order_header poh
                    LEFT JOIN store wh ON poh.warehouse_uid = wh.uid
                    LEFT JOIN org franchise_org ON wh.franchisee_org_uid = franchise_org.uid
                    LEFT JOIN org ON poh.org_uid = org.uid
                """);
            }
            Dictionary<string, object?> parameters = [];
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                // Prefix filter columns with table alias to avoid ambiguity
                foreach (var filterCriteria in filterCriterias)
                {
                    if (!filterCriteria.Name.Contains('.'))
                    {
                        // Map column names to use poh prefix
                        filterCriteria.Name = filterCriteria.Name.ToUpper() switch
                        {
                            "UID" => "poh.uid",
                            "PURCHASEORDERHEADERUID" => "poh.uid",
                            "ORGUID" => "poh.org_uid",
                            "WAREHOUSEUID" => "poh.warehouse_uid",
                            "STATUS" => "poh.status",
                            "ORDERDATE" => "poh.order_date",
                            _ => $"poh.{filterCriteria.Name.ToLower()}"
                        };
                    }
                }

                StringBuilder sbFilterCriteria = new();
                AppendFilterCriteria<IPurchaseOrderHeader>(filterCriterias, sbFilterCriteria, parameters);

                if (sbFilterCriteria.Length > 0)
                {
                    _ = sql.Append(" WHERE ").Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        _ = sqlCount.Append(" WHERE ").Append(sbFilterCriteria);
                    }
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            }

            IEnumerable<IPurchaseOrderHeader> purchaseOrderHeaders = await ExecuteQueryAsync<IPurchaseOrderHeader>(
            sql.ToString()
            , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPurchaseOrderHeader> pagedResponse = new()
            {
                PagedData = purchaseOrderHeaders, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IPurchaseOrderMaster> GetPurchaseOrderMasterByUID(string uid)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uid))
                return null!;

            FilterCriteria headerFilterCriteria = new FilterCriteria("UID", uid, FilterType.Equal);
            FilterCriteria linesFilterCriteria = new FilterCriteria("PurchaseOrderHeaderUID", uid, FilterType.Equal);

            var headerResult = await GetAllPurchaseOrderHeaders(null, 0, 0, [headerFilterCriteria], false);
            if (headerResult.PagedData == null || !headerResult.PagedData.Any())
                return null!;

            IPurchaseOrderMaster purchaseOrderMaster = _serviceProvider.GetRequiredService<IPurchaseOrderMaster>();
            purchaseOrderMaster.PurchaseOrderHeader = headerResult.PagedData.First();

            purchaseOrderMaster.PurchaseOrderLines = (await _purchaseOrderLineDL
                    .GetAllPurchaseOrderLines(null, 0, 0, [linesFilterCriteria], false))
                .PagedData
                .ToList();
            return purchaseOrderMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<Dictionary<string, int>> GetPurchaseOrderSatatusCounts(List<FilterCriteria>? filterCriterias = null)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdatePurchaseOrderHeader(
        List<IPurchaseOrderHeader> purchaseOrderHeaders, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                         UPDATE purchase_order_header
                         SET
                             ss = @SS,
                             created_by = @CreatedBy,
                             created_time = @CreatedTime,
                             modified_by = @ModifiedBy,
                             modified_time = @ModifiedTime,
                             server_add_time = @ServerAddTime,
                             server_modified_time = @ServerModifiedTime,
                             org_uid = @OrgUID,
                             division_uid = @DivisionUID,
                             has_template = @HasTemplate,
                             purchase_order_template_header_uid = @PurchaseOrderTemplateHeaderUID,
                             warehouse_uid = @WareHouseUID,
                             order_date = @OrderDate,
                             order_number = @OrderNumber,
                             draft_order_number = @DraftOrderNumber,
                             expected_delivery_date = @ExpectedDeliveryDate,
                             shipping_address_uid = @ShippingAddressUID,
                             billing_address_uid = @BillingAddressUID,
                             status = @Status,
                             qty_count = @QtyCount,
                             line_count = @LineCount,
                             total_amount = @TotalAmount,
                             total_discount = @TotalDiscount,
                             line_discount = @LineDiscount,
                             header_discount = @HeaderDiscount,
                             total_tax_amount = @TotalTaxAmount,
                             line_tax_amount = @LineTaxAmount,
                             header_tax_amount = @HeaderTaxAmount,
                             net_amount = @NetAmount,
                             available_credit_limit = @AvailableCreditLimit,
                             tax_data = COALESCE(NULLIF(@TaxData, '')::json, '{}'::json),
                             app1_emp_uid = @App1EmpUID,
                             app2_emp_uid = @App2EmpUID,
                             app3_emp_uid = @App3EmpUID,
                             app4_emp_uid = @App4EmpUID,
                             app5_emp_uid = @App5EmpUID,
                             app6_emp_uid = @App6EmpUID,
                             app1_date = @App1Date,
                             app2_date = @App2Date,
                             app3_date = @App3Date,
                             app4_date = @App4Date,
                             app5_date = @App5Date,
                             app6_date = @App6Date,
                             branch_uid = @BranchUID,
                             ho_org_uid = @HOOrgUID,
                             org_unit_uid = @OrgUnitUID,
                             reporting_emp_uid = @ReportingEmpUID,
                             source_warehouse_uid = @SourceWareHouseUID,
                             erp_status = @OracleOrderStatus,
                             is_approval_created = @IsApprovalCreated
                         WHERE
                                 uid = @UID;
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderHeaders);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdatePurchaseOrderHeaderStatusAfterApproval(IPurchaseOrderHeader purchaseOrderHeader)
    {
        try
        {
            string sql = """
                         UPDATE purchase_order_header
                         SET
                             modified_by = @ModifiedBy,
                             modified_time = @ModifiedTime,
                             server_modified_time = @ServerModifiedTime,
                              status = @Status
                         WHERE
                                 uid = @UID;
                         """;

            return await ExecuteNonQueryAsync(sql, parameters: purchaseOrderHeader);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<List<IPurchaseOrderLineQPS>> GetPurchaseOrderLineQPSs(string orgUid, string schemeUid,
        List<string> skuUids = null)
    {
        throw new NotImplementedException();
    }
    public Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem approvalRequestItem)
    {
        throw new NotImplementedException();
    }
}
