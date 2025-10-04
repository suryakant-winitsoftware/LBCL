using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                                        poh.shipping_address_uid AS ShippingAddressUID,
                                        poh.billing_address_uid AS BillingAddressUID,
                                        poh.status AS Status,
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
                                        warehouse wh ON poh.warehouse_uid = wh.uid
                                    LEFT JOIN 
                                        emp e1 ON poh.app1_emp_uid = e1.uid
                                    LEFT JOIN 
                                        emp e2 ON poh.app2_emp_uid = e2.uid
                                    LEFT JOIN 
                                        .emp e3 ON poh.app3_emp_uid = e3.uid
                                    LEFT JOIN 
                                        emp e4 ON poh.app4_emp_uid = e4.uid
                                    LEFT JOIN 
                                        emp e5 ON poh.app5_emp_uid = e5.uid
                                    LEFT JOIN 
                                        emp e6 ON poh.app6_emp_uid = e6.uid WHERE 
                                        poh.status = @Status
                                        
                                    """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM public.purchase_order_header poh");
            }

            Dictionary<string, object> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<PurchaseOrderHeaderItem>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql, true);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
            string sql = """
                         INSERT INTO purchase_order_header (
                             uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                             org_uid, division_uid, has_template, purchase_order_template_header_uid, warehouse_uid, order_date,
                             expected_delivery_date,
                             shipping_address_uid, billing_address_uid, status, qty_count, line_count, total_amount, total_discount, 
                             line_discount, header_discount, total_tax_amount, line_tax_amount, header_tax_amount, net_amount, 
                             available_credit_limit, tax_data, app1_emp_uid, app2_emp_uid, app3_emp_uid, app4_emp_uid, app5_emp_uid, 
                             app6_emp_uid, app1_date, app2_date, app3_date, app4_date, app5_date, app6_date
                         ) VALUES (
                             @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                             @OrgUID, @DivisionUID, @HasTemplate, @PurchaseOrderTemplateHeaderUID, @WareHouseUID, @OrderDate,
                             @ExpectedDeliveryDate,
                             @ShippingAddressUID, @BillingAddressUID, @Status, @QtyCount, @LineCount, @TotalAmount, @TotalDiscount, 
                             @LineDiscount, @HeaderDiscount, @TotalTaxAmount, @LineTaxAmount, @HeaderTaxAmount, @NetAmount, 
                             @AvailableCreditLimit, @TaxData, @App1EmpUID, @App2EmpUID, @App3EmpUID, @App4EmpUID, @App5EmpUID, 
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
            using IDbTransaction transaction = connection.BeginTransaction();
            foreach (IPurchaseOrderMaster purchaseOrder in purchaseOrderMasters)
            {
                if (await CreatePurchaseOrderHeaders([purchaseOrder.PurchaseOrderHeader], connection, transaction) != 1)
                {
                    transaction.Rollback();
                    return false;
                }
                if (await _purchaseOrderLineDL.CreatePurchaseOrderLines
                        (purchaseOrder.PurchaseOrderLines, connection, transaction) !=
                    purchaseOrder.PurchaseOrderLines.Count)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            return true;
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
            SELECT  *
            (SELECT uid , ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
            org_uid, division_uid, has_template, purchase_order_template_header_uid, warehouse_uid, order_date,
            expected_delivery_date,shipping_address_uid, billing_address_uid, status, qty_count, line_count, 
            total_amount, total_discount, 
            line_discount, header_discount, total_tax_amount, line_tax_amount, header_tax_amount, net_amount, 
            available_credit_limit, tax_data, app1_emp_uid, app2_emp_uid, app3_emp_uid, app4_emp_uid, app5_emp_uid, 
            app6_emp_uid, app1_date, app2_date, app3_date, app4_date, app5_date, app6_date FROM purchase_order_header)
            AS  purchaseorderheader
            """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                """
                    SELECT count(*)
                    (SELECT uid , ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                    org_uid, division_uid, has_template, purchase_order_template_header_uid, warehouse_uid, order_date,
                    expected_delivery_date,shipping_address_uid, billing_address_uid, status, qty_count, line_count, 
                    total_amount, total_discount, 
                    line_discount, header_discount, total_tax_amount, line_tax_amount, header_tax_amount, net_amount, 
                    available_credit_limit, tax_data, app1_emp_uid, app2_emp_uid, app3_emp_uid, app4_emp_uid, app5_emp_uid, 
                    app6_emp_uid, app1_date, app2_date, app3_date, app4_date, app5_date, app6_date FROM purchase_order_header)
                    AS  purchaseorderheader
                                        
                """);
            }
            Dictionary<string, object?> parameters = [];
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sql.Append(" WHERE ");
                AppendFilterCriteria<IPurchaseOrderHeader>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(" WHERE ");
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
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
            FilterCriteria headerFilterCriteria = new FilterCriteria("UID", uid, FilterType.Equal);
            FilterCriteria linesFilterCriteria = new FilterCriteria("PurchaseOrderHeaderUID", uid, FilterType.Equal);
            IPurchaseOrderMaster purchaseOrderMaster = _serviceProvider.GetRequiredService<IPurchaseOrderMaster>();
            purchaseOrderMaster.PurchaseOrderHeader =
                (await GetAllPurchaseOrderHeaders(null, 0, 0, [headerFilterCriteria], false))
                .PagedData
                .First();

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
