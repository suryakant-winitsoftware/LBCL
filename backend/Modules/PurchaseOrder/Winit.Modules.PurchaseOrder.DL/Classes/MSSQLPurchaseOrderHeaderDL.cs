using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIModels.Common;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class MSSQLPurchaseOrderHeaderDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IPurchaseOrderHeaderDL
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPurchaseOrderLineDL _purchaseOrderLineDL;
    private readonly IApprovalEngineHelper _approvalEngineHelper;
    private readonly IPurchaseOrderLineProvisionDL _purchaseOrderLineProvisionDL;
    private readonly ILogger<MSSQLPurchaseOrderHeaderDL> _logger;

    public MSSQLPurchaseOrderHeaderDL(IServiceProvider serviceProvider,
        IConfiguration config,
        IPurchaseOrderLineDL purchaseOrderLineDL,
        IApprovalEngineHelper approvalEngineHelper,
        IPurchaseOrderLineProvisionDL purchaseOrderLineProvisionDL,
        ILogger<MSSQLPurchaseOrderHeaderDL> logger)
        : base(serviceProvider, config)
    {
        _serviceProvider = serviceProvider;
        _purchaseOrderLineDL = purchaseOrderLineDL;
        _approvalEngineHelper = approvalEngineHelper;
        _purchaseOrderLineProvisionDL = purchaseOrderLineProvisionDL;
        _logger = logger;
    }

    public async Task<PagedResponse<IPurchaseOrderHeader>> GetAllPurchaseOrderHeaders(List<SortCriteria>? sortCriterias,
        int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new StringBuilder($"""
                                                   SELECT  * FROM
                                                   (SELECT 
                                                       poh.uid, poh.ss, poh.created_by, poh.created_time, poh.modified_by, poh.modified_time, 
                                                       poh.server_add_time, poh.server_modified_time, poh.org_uid AS orguid, poh.division_uid, 
                                                       poh.has_template, poh.purchase_order_template_header_uid, poh.warehouse_uid, poh.order_date, 
                                                       poh.order_number, poh.draft_order_number AS DraftOrderNumber, poh.expected_delivery_date, 
                                                       poh.shipping_address_uid, poh.billing_address_uid, poh.status, poh.qty_count, poh.line_count, 
                                                       poh.total_amount, poh.total_discount, poh.line_discount, poh.header_discount, 
                                                       poh.total_tax_amount, poh.line_tax_amount, poh.header_tax_amount, poh.net_amount, 
                                                       poh.available_credit_limit, poh.tax_data, poh.app1_emp_uid, poh.app2_emp_uid, poh.app3_emp_uid, 
                                                       poh.app4_emp_uid, poh.app5_emp_uid, poh.app6_emp_uid, poh.app1_date, poh.app2_date, 
                                                       poh.app3_date, poh.app4_date, poh.app5_date, poh.app6_date, poh.branch_uid AS branchuid, 
                                                       poh.ho_org_uid AS hoorguid, poh.org_unit_uid AS orgunituid, poh.reporting_emp_uid AS reportingempuid, 
                                                       poh.source_warehouse_uid AS sourcewarehouseuid, poh.erp_order_number AS OracleNo,
                                                       poh.erp_status AS OracleOrderStatus,
                                                       poh.total_billed_qty AS TotalBilledQty,
                                                       poh.total_cancelled_qty AS TotalCancelledQty,
                                                       e.name AS ReportingEmpName,
                                                       e.code as ReportingEmpCode,
                                                       e1.code as CreatedByEmpCode,
                                                       e1.name as CreatedByEmpName,
                                                       poh.is_approval_created AS IsApprovalCreated
                                                   FROM 
                                                       purchase_order_header poh 
                                                   LEFT JOIN 
                                                       emp e ON e.uid = poh.reporting_emp_uid
                                                   INNER JOIN 
                                                       emp e1 ON e1.uid = poh.created_by)
                                                   AS  purchaseorderheader
                                                   """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                $"""
                     SELECT count(*) FROM 
                         (SELECT 
                         poh.uid, poh.ss, poh.created_by, poh.created_time, poh.modified_by, poh.modified_time, 
                         poh.server_add_time, poh.server_modified_time, poh.org_uid AS orguid, poh.division_uid, 
                         poh.has_template, poh.purchase_order_template_header_uid, poh.warehouse_uid, poh.order_date, 
                         poh.order_number, poh.draft_order_number AS DraftOrderNumber, poh.expected_delivery_date, 
                         poh.shipping_address_uid, poh.billing_address_uid, poh.status, poh.qty_count, poh.line_count, 
                         poh.total_amount, poh.total_discount, poh.line_discount, poh.header_discount, 
                         poh.total_tax_amount, poh.line_tax_amount, poh.header_tax_amount, poh.net_amount, 
                         poh.available_credit_limit, poh.tax_data, poh.app1_emp_uid, poh.app2_emp_uid, poh.app3_emp_uid, 
                         poh.app4_emp_uid, poh.app5_emp_uid, poh.app6_emp_uid, poh.app1_date, poh.app2_date, 
                         poh.app3_date, poh.app4_date, poh.app5_date, poh.app6_date, poh.branch_uid AS branchuid, 
                         poh.ho_org_uid AS hoorguid, poh.org_unit_uid AS orgunituid, poh.reporting_emp_uid AS reportingempuid, 
                         poh.source_warehouse_uid AS sourcewarehouseuid, poh.erp_order_number AS OracleNo, 
                         poh.erp_status AS OracleOrderStatus,
                         poh.total_billed_qty AS TotalBilledQty,
                         poh.total_cancelled_qty AS TotalCancelledQty,
                         e.name AS ReportingEmpName,
                         e.code as ReportingEmpCode,
                         e1.code as CreatedByEmpCode,
                         e1.name as CreatedByEmpName,
                         poh.is_approval_created AS IsApprovalCreated
                          FROM 
                         purchase_order_header poh 
                     LEFT JOIN 
                         emp e ON e.uid = poh.reporting_emp_uid
                     INNER JOIN 
                         emp e1 ON e1.uid = poh.created_by)
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
                PagedData = purchaseOrderHeaders,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<PagedResponse<IPurchaseOrderHeaderItem>> GetPurchaseOrderHeadersAsync(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired
        )
    {
        try
        {
            StringBuilder sql = new($"""
                                     SELECT * from 
                                         (select poh.uid AS UID,
                                         poh.org_uid AS OrgUID,
                                         poh.division_uid AS DivisionUID,
                                         poh.warehouse_uid AS WareHouseUID,
                                         o.name AS WarehouseName,
                                         poh.order_date AS OrderDate,
                                         CASE 
                                             WHEN poh.status = '{PurchaseOrderStatusConst.Draft}' THEN poh.draft_order_number
                                             ELSE poh.order_number
                                         END AS OrderNumber,
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
                                         poh.app6_date as cpeconfirmdatetime,
                                         e6.name AS App6EmpName,
                                         s.name channelpartnername, 
                                         s.code channelpartnercode,
                                         branch_uid as branchuid,
                                         ho_org_uid as hoorguid,
                                         poh.org_unit_uid as orgunituid,
                                         poh.erp_order_number as OracleNo,
                                         poh.erp_status as OracleOrderStatus,
                                         poh.erp_order_date as OracleOrderDate,
                                         e.name as ReportingEmpName,
                                         e.code as ReportingEmpCode,
                                         ce.code as CreatedByEmpCode,
                                         ce.name as CreatedByEmpName,
                                         poh.created_by as CreatedBy,
                                         poh.reporting_emp_uid as ReportingEmpUID
                                         FROM 
                                         purchase_order_header poh
                                     LEFT JOIN 
                                         org o ON poh.warehouse_uid = o.uid and o.org_type_uid = 'FRWH'
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
                                     LEFT JOIN
                                         store s ON poh.org_uid = s.uid
                                     LEFT JOIN  
                                             emp e ON e.uid = poh.reporting_emp_uid
                                     Left join 
                                             emp ce ON ce.uid = poh.created_by
                                         ) as purchaseorderheader
                                         WHERE 
                                            1=1 

                                     """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder($"""
                                              SELECT count(*) from 
                                                  (select poh.uid AS UID,
                                                  poh.org_uid AS OrgUID,
                                                  poh.division_uid AS DivisionUID,
                                                  poh.warehouse_uid AS WareHouseUID,
                                                  o.name AS WarehouseName,
                                                  poh.order_date AS OrderDate,
                                                  CASE 
                                                      WHEN poh.status = '{PurchaseOrderStatusConst.Draft}' THEN poh.draft_order_number
                                                      ELSE poh.order_number
                                                  END AS OrderNumber,
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
                                                  poh.app6_date as cpeconfirmdatetime,
                                                  e6.name AS App6EmpName,
                                                  s.name channelpartnername, 
                                                  s.code channelpartnercode,
                                                  branch_uid as branchuid,
                                                  ho_org_uid as hoorguid,
                                                  poh.org_unit_uid as orgunituid,
                                                  poh.erp_order_number as OracleNo,
                                                  poh.erp_status AS OracleOrderStatus,
                                                  poh.erp_order_date as OracleOrderDate,
                                                  e.name as ReportingEmpName,
                                                  e.code as ReportingEmpCode,
                                                  ce.code as CreatedByEmpCode,
                                                  ce.name as CreatedByEmpName,
                                                  poh.created_by as CreatedBy,
                                                  poh.reporting_emp_uid AS ReportingEmpUID
                                                  FROM 
                                                  purchase_order_header poh
                                              LEFT JOIN 
                                                  org o ON poh.warehouse_uid = o.uid and o.org_type_uid = 'FRWH'
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
                                              LEFT JOIN
                                                  store s ON poh.org_uid = s.uid
                                              LEFT JOIN  
                                                      emp e ON e.uid = poh.reporting_emp_uid
                                              LEFT JOIN 
                                                      emp ce ON ce.uid = poh.created_by) 
                                                      as purchaseorderheader
                                                              WHERE
                                                                  1=1 
                                              """);
            }
            Dictionary<string, object?> parameters = [];
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                FilterCriteria? jobPositionFilterCriteria = filterCriterias
                    .Find(e => e.Name == "JobPositionUid");
                if (jobPositionFilterCriteria != null)
                {
                    if (jobPositionFilterCriteria.Value != null)
                    {
                        parameters["JobPositionUid"] = jobPositionFilterCriteria.Value;
                        string jobPositionQuery = """
                                                  AND 
                                                   OrgUID IN (
                                                  SELECT DISTINCT org_uid FROM my_orgs 
                                                    WHERE 
                                                  job_position_uid = @JobPositionUid 
                                                  )
                                                  """;
                        sql.Append(jobPositionQuery);
                        sqlCount.Append(jobPositionQuery);
                    }
                    filterCriterias.Remove(jobPositionFilterCriteria);
                }
                FilterCriteria? reportsToEmpFilterCriteria = filterCriterias
                    .Find(e => e.Name == "ReportingEmpUID");
                FilterCriteria? statusFilterCriteria = filterCriterias
                    .Find(e => e.Name == "status");

                if (reportsToEmpFilterCriteria != null)
                {
                    parameters["ReportsToEmpUid"] = reportsToEmpFilterCriteria.Value;
                    sql.Append($" AND ( Status = '{PurchaseOrderStatusConst.Draft}' OR ReportingEmpUID = @ReportsToEmpUID) ");
                    sqlCount.Append($" AND ( Status = '{PurchaseOrderStatusConst.Draft}' OR ReportingEmpUID = @ReportsToEmpUID) ");
                    filterCriterias.Remove(reportsToEmpFilterCriteria);
                }
                if (statusFilterCriteria == null)
                {
                    filterCriterias.RemoveAll(e => e.Name == "DivisionUID");
                }
                if (filterCriterias.Any())
                {
                    sql.Append($" AND ");
                    sqlCount.Append($" AND ");
                }
                StringBuilder sbFilterCriteria = new();
                AppendFilterCriteria<IPurchaseOrderHeaderItem>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }
            else
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria([new SortCriteria("orderdate", SortDirection.Desc)], sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<IPurchaseOrderHeaderItem> purchaseOrderLines =
                await ExecuteQueryAsync<IPurchaseOrderHeaderItem>(sql.ToString()
                , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPurchaseOrderHeaderItem> pagedResponse = new()
            {
                PagedData = purchaseOrderLines,
                TotalCount = totalCount
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
            string sql = $"""
                          INSERT INTO purchase_order_header (
                              uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                              org_uid, division_uid, has_template, purchase_order_template_header_uid, warehouse_uid, order_date,
                              order_number,draft_order_number,
                              expected_delivery_date,shipping_address_uid, billing_address_uid, status, qty_count, line_count, 
                              total_amount, total_discount, 
                              line_discount, header_discount, total_tax_amount, line_tax_amount, header_tax_amount, net_amount, 
                              available_credit_limit, tax_data, app1_emp_uid, app2_emp_uid, app3_emp_uid, app4_emp_uid, app5_emp_uid, 
                              app6_emp_uid, app1_date, app2_date, app3_date, app4_date, app5_date, app6_date, branch_uid, ho_org_uid,
                              org_unit_uid, reporting_emp_uid, source_warehouse_uid, erp_status  
                          ) VALUES (
                              @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                              @OrgUID, @DivisionUID, @HasTemplate, @PurchaseOrderTemplateHeaderUID, @WareHouseUID, @OrderDate,
                              @OrderNumber, @DraftOrderNumber,@ExpectedDeliveryDate, 
                              @ShippingAddressUID, @BillingAddressUID, @Status, @QtyCount, @LineCount, @TotalAmount, @TotalDiscount, 
                              @LineDiscount, @HeaderDiscount, @TotalTaxAmount, @LineTaxAmount, @HeaderTaxAmount, @NetAmount, 
                              @AvailableCreditLimit, @TaxData, @App1EmpUID, @App2EmpUID, @App3EmpUID, @App4EmpUID, @App5EmpUID, 
                              @App6EmpUID, @App1Date, @App2Date, @App3Date, @App4Date, @App5Date, @App6Date, @BranchUID, @HOOrgUID,
                              @OrgUnitUID, @ReportingEmpUID, @SourceWareHouseUID, @OracleOrderStatus
                          );
                          """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderHeaders);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdatePurchaseOrderHeader(
        List<IPurchaseOrderHeader> purchaseOrderHeaders, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = $"""
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
                              tax_data = @TaxData,
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
                              Org_unit_uid = @OrgUnitUID,
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

    public async Task<int> DeletePurchaseOrderHeadersByUIDs(List<string> uIDs, IDbConnection? connection = null,
        IDbTransaction? transaction = null)
    {
        try
        {
            string sql = $" DELETE FROM purchase_order_header where uid IN @UIDs";
            var parameters = new
            {
                UIDs = uIDs
            };
            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    //public async Task<int> DeletePurchaseOrderMasterByHeaderUID(string uid, IDbConnection? connection = null,
    //    IDbTransaction? transaction = null)
    //{
    //    try
    //    {
    //        string sql = """
    //                            delete polp from purchase_order_header h
    //            inner join purchase_order_line l on h.uid=l.purchase_order_header_uid
    //            inner join purchase_order_line_provision polp on polp.purchase_order_line_uid=l.uid where h.UID=@PurchaseOrderUID;

    //             delete l from purchase_order_header h
    //            inner join purchase_order_line l on h.uid=l.purchase_order_header_uid where h.UID=@PurchaseOrderUID;

    //             delete h from purchase_order_header h where h.UID=@PurchaseOrderUID;
    //            """;
    //        var parameters = new
    //        {
    //            PurchaseOrderUID = uid
    //        };
    //        return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}

    public async Task<bool> CUD_PurchaseOrder(List<IPurchaseOrderMaster> purchaseOrderMasters)
    {
        using IDbConnection connection = CreateConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            if (purchaseOrderMasters.First().PurchaseOrderHeader!.Status ==
                PurchaseOrderStatusConst.PendingForApproval &&
                !string.IsNullOrEmpty(purchaseOrderMasters.First().PurchaseOrderHeader!.DraftOrderNumber))
            {
                PagedResponse<IPurchaseOrderHeader> pagedResponse = await GetAllPurchaseOrderHeaders(
                null,
                default,
                default,
                [
                    new FilterCriteria
                    ("DraftOrderNumber", purchaseOrderMasters.First().PurchaseOrderHeader!.DraftOrderNumber,
                    FilterType.Equal)
                ],
                false
                );
                if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Any())
                {
                    _ = await _purchaseOrderLineProvisionDL.DeletePurchaseOrderLineProvisionsByPurchaseOrderLineUids(
                    purchaseOrderMasters.SelectMany(e => e.PurchaseOrderLines.Select(item => item.UID))
                        .ToList(), connection, transaction);
                    _ = await _purchaseOrderLineDL.DeletePurchaseOrderLinesByPurchaseOrderHeaderUID(
                    pagedResponse.PagedData.First().UID, connection, transaction);
                    _ = await DeletePurchaseOrderHeadersByUIDs([pagedResponse.PagedData.First().UID], connection,
                    transaction);

                    //_ = await DeletePurchaseOrderMasterByHeaderUID(pagedResponse.PagedData.First().UID, connection,
                    //transaction);
                }
            }
            if (purchaseOrderMasters.First().ActionType == ActionType.Update)
            {
                int purchaseOrderHeaderUpdateCount = await UpdatePurchaseOrderHeader([purchaseOrderMasters.First().PurchaseOrderHeader], connection,
                transaction);
                if (purchaseOrderHeaderUpdateCount != 1)
                {
                    transaction.Rollback();
                    return false;
                }
                List<string>? existingUids = await CheckIfUIDExistsInDB("purchase_order_line",
                purchaseOrderMasters.First().PurchaseOrderLines!.Select(e => e.UID).ToList(), connection,
                transaction);
                existingUids ??= [];
                List<IPurchaseOrderLine> purchaseOrderLinesforUpdate =
                    purchaseOrderMasters.First().PurchaseOrderLines!.FindAll(e => existingUids!.Contains(e.UID));
                List<IPurchaseOrderLine> purchaseOrderLinesforAdd =
                    purchaseOrderMasters.First().PurchaseOrderLines!.FindAll(e => !existingUids!.Contains(e.UID));
                if (purchaseOrderLinesforUpdate != null && purchaseOrderLinesforUpdate.Any() &&
                    await _purchaseOrderLineDL.UpdatePurchaseOrderLines
                        (purchaseOrderLinesforUpdate, connection, transaction) != purchaseOrderLinesforUpdate.Count)
                {
                    transaction.Rollback();
                    return false;
                }

                if (purchaseOrderLinesforAdd != null && purchaseOrderLinesforAdd.Any() &&
                    await _purchaseOrderLineDL.CreatePurchaseOrderLines
                        (purchaseOrderLinesforAdd, connection, transaction) != purchaseOrderLinesforAdd.Count)
                {
                    transaction.Rollback();
                    return false;
                }
                if (purchaseOrderMasters.First().PurchaseOrderLineProvisions != null && purchaseOrderMasters.First().PurchaseOrderLineProvisions.Any())
                {
                    //List<string>? existingPurchaseOrderLineProvisionsUids = await CheckIfUIDExistsInDB("purchase_order_line_provision",
                    //purchaseOrderMasters.First().PurchaseOrderLineProvisions!.Select(e => e.UID).ToList(), connection,
                    //transaction);
                    List<string>? existingPurchaseOrderLineProvisionsUids = await _purchaseOrderLineProvisionDL.GetPurchaseOrderLineProvisionUidsByPurchaseOrderUID(
                    purchaseOrderMasters.First().PurchaseOrderHeader!.UID, connection,
                    transaction);
                    var purchaseOrderLineProvisionsForupdate = purchaseOrderMasters.First().PurchaseOrderLineProvisions.FindAll(e => existingPurchaseOrderLineProvisionsUids.Contains(e.UID));

                    var purchaseOrderLinesProvisionForDelete = existingPurchaseOrderLineProvisionsUids?.
                        Where(p => !purchaseOrderMasters.First().PurchaseOrderLineProvisions.Select(q => q.UID).ToList().Contains(p)).ToList();
                    if (purchaseOrderLinesProvisionForDelete != null && purchaseOrderLinesProvisionForDelete.Count() > 0)
                    {
                        _ = await _purchaseOrderLineProvisionDL.DeletePurchaseOrderLineProvisionsByUids(purchaseOrderLinesProvisionForDelete, connection, transaction);
                    }
                    if (purchaseOrderLineProvisionsForupdate != null && purchaseOrderLineProvisionsForupdate.Any())
                    {
                        _ = await _purchaseOrderLineProvisionDL.UpdatePurchaseOrderLineProvisions(purchaseOrderLineProvisionsForupdate, connection, transaction);
                    }
                    var purchaseOrderLineProvisionsForAdd = purchaseOrderMasters.First().PurchaseOrderLineProvisions.FindAll(e => !existingPurchaseOrderLineProvisionsUids.Contains(e.UID));
                    if (purchaseOrderLineProvisionsForAdd != null && purchaseOrderLineProvisionsForAdd.Any())
                    {
                        _ = await _purchaseOrderLineProvisionDL.CreatePurchaseOrderLineProvisions(purchaseOrderLineProvisionsForAdd, connection, transaction);
                    }
                }
                transaction.Commit();
                if (purchaseOrderMasters.First().PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Draft) return true;
                if (await _approvalEngineHelper.UpdateApprovalStatus(purchaseOrderMasters.First().ApprovalStatusUpdate))
                {
                    if (purchaseOrderMasters.First().ApprovalStatusUpdate.IsFinalApproval)
                    {
                        purchaseOrderMasters.First().PurchaseOrderHeader.Status = PurchaseOrderStatusConst.InProcessERP;
                        purchaseOrderMasters.First().PurchaseOrderHeader.OracleOrderStatus = PurchaseOrderErpStatusConst.InProcess;
                        purchaseOrderMasters.First().PurchaseOrderHeader.App6Date = DateTime.Now;
                        purchaseOrderMasters.First().PurchaseOrderHeader.App6EmpUID = purchaseOrderMasters.First().PurchaseOrderHeader.ModifiedBy;
                        await UpdatePurchaseOrderHeader([purchaseOrderMasters.First().PurchaseOrderHeader]);
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            if (purchaseOrderMasters.First().ActionType == ActionType.Add)
            {
                foreach (IPurchaseOrderMaster purchaseOrder in purchaseOrderMasters)
                {
                    if (!string.IsNullOrEmpty(purchaseOrder.PurchaseOrderHeader.OrderNumber))
                    {
                        purchaseOrder.PurchaseOrderHeader.DraftOrderNumber = null;
                    }
                    if (await CreatePurchaseOrderHeaders([purchaseOrder.PurchaseOrderHeader], connection, transaction) != 1)
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
                    if (await _purchaseOrderLineProvisionDL.CreatePurchaseOrderLineProvisions(purchaseOrder.PurchaseOrderLineProvisions, connection, transaction) < 1)
                    {
                        transaction.Rollback();
                    }

                }
                transaction.Commit();

                foreach (IPurchaseOrderMaster purchaseOrder in purchaseOrderMasters)
                {
                    if (purchaseOrder.PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Draft) continue;
                    purchaseOrder.ApprovalRequestItem.HierarchyUid = purchaseOrder.PurchaseOrderHeader.ReportingEmpUID;
                    await CreateApprovalRequest(purchaseOrder.PurchaseOrderHeader.UID, purchaseOrder.ApprovalRequestItem);
                    purchaseOrder.PurchaseOrderHeader.IsApprovalCreated = true;
                    await UpdatePurchaseOrderHeader([purchaseOrder.PurchaseOrderHeader]);
                }
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            if (transaction.Connection != null)
            {
                transaction.Rollback();
            }
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetPurchaseOrderSatatusCounts(List<FilterCriteria>? filterCriterias = null)
    {
        try
        {
            StringBuilder sql = new(
            """
            SELECT status, COUNT(*) AS count
            FROM purchase_order_header
            WHERE
            1=1 
            """);
            var parameters = new Dictionary<string, object?>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                FilterCriteria? jobPositionFilterCriteria = filterCriterias
                    .Find(e => e.Name == "JobPositionUid");
                if (jobPositionFilterCriteria != null)
                {
                    if (jobPositionFilterCriteria.Value != null)
                    {
                        parameters["JobPositionUid"] = jobPositionFilterCriteria.Value;
                        string jobPositionQuery = """
                                                  AND 
                                                   Org_UID IN (
                                                  SELECT DISTINCT org_uid FROM my_orgs 
                                                    WHERE 
                                                  job_position_uid = @JobPositionUid 
                                                  )
                                                  """;
                        sql.Append(jobPositionQuery);
                    }
                    filterCriterias.Remove(jobPositionFilterCriteria);
                }
                FilterCriteria? reportsToEmpFilterCriteria = filterCriterias
                    .Find(e => e.Name == "ReportingEmpUID");
                FilterCriteria? statusFilterCriteria = filterCriterias
                    .Find(e => e.Name == "status");
                FilterCriteria? oracleOrderStatusFilterCriteria = filterCriterias
                    .Find(e => e.Name == "OracleOrderStatus");
                FilterCriteria? erpOrderNumberFilterCriteria = filterCriterias
                    .Find(e => e.Name == "OracleNo");

                filterCriterias.ForEach(e =>
                {
                    switch (e.Name)
                    {
                        case "OracleOrderStatus":
                            e.Name = "erp_status";
                            break;
                        case "OracleNo":
                            e.Name = "erp_order_number";
                            break;
                        case "OrderNumber":
                            e.Name = e.Value is string str && str.Contains("DIV")
                                    ? "OrderNumber"
                                    : "DraftOrderNumber";
                            break;
                    }
                });

                if (reportsToEmpFilterCriteria != null)
                {
                    parameters["ReportsToEmpUid"] = reportsToEmpFilterCriteria.Value;
                    sql.Append($" AND ( Status = '{PurchaseOrderStatusConst.Draft}' OR reporting_emp_uid = @ReportsToEmpUID) ");
                    filterCriterias.Remove(reportsToEmpFilterCriteria);
                }

                if (statusFilterCriteria == null)
                {
                    filterCriterias.RemoveAll(e => e.Name == "DivisionUID");
                }
                if (filterCriterias.Any())
                {
                    sql.Append($" AND ");
                    filterCriterias.ForEach(e => e.Name = CommonFunctions.ConvertToSnakeCase(e.Name));
                    StringBuilder sbFilterCriteria = new();
                    AppendFilterCriteria<IPurchaseOrderHeaderItem>(filterCriterias, sbFilterCriteria, parameters);
                    _ = sql.Append(sbFilterCriteria);
                }
            }
            _ = sql.Append(" GROUP BY status ");

            return await Query(async (e) => (await e.QueryAsync(sql.ToString(), parameters)).ToDictionary(
            row => (string)row.status,
            row => (int)row.count
            ));
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
            FilterCriteria headerFilterCriteria = new("UID", uid, FilterType.Equal);
            FilterCriteria linesFilterCriteria = new("PurchaseOrderHeaderUID", uid, FilterType.Equal);
            IPurchaseOrderMaster purchaseOrderMaster = _serviceProvider.GetRequiredService<IPurchaseOrderMaster>();
            purchaseOrderMaster.PurchaseOrderHeader =
                (await GetAllPurchaseOrderHeaders(null, 0, 0, [headerFilterCriteria], false))
                .PagedData
                .First();

            purchaseOrderMaster.PurchaseOrderLines = (await _purchaseOrderLineDL
                    .GetAllPurchaseOrderLines(null, 0, 0, [linesFilterCriteria], false))
                .PagedData
                .ToList();
            List<string> purchaseOrderLineUIDs = purchaseOrderMaster.PurchaseOrderLines.Select(e => e.UID).ToList();
            var purchaseOrderLineProvisions = (await _purchaseOrderLineProvisionDL
                    .GetAllPurchaseOrderLineProvisions(
                    [new FilterCriteria("PurchaseOrderLineUID", purchaseOrderLineUIDs, FilterType.In)]
                    ))
                .PagedData.ToList();
            if (purchaseOrderLineProvisions != null && purchaseOrderLineProvisions.Any())
            {
                purchaseOrderMaster.PurchaseOrderLineProvisions = purchaseOrderLineProvisions;
            }
            return purchaseOrderMaster;
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
            List<string>? exstUiDs =
                await CheckIfUIDExistsInDB(DbTableName.PurchaseOrderHeader, [purchaseOrderHeader.UID]);
            if (exstUiDs == null || exstUiDs.Count == 0)
            {
                throw new Exception($"Purchace Order Not Found with {purchaseOrderHeader.UID}");
            }
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
        try
        {
            StringBuilder sql = new("""
                                    SELECT
                                        sku_uid as SKUUID, 
                                        SUM(pol.net_amount) AS TotalAmount,
                                        SUM(pol.final_qty) AS TotalQty
                                    FROM 
                                    	purchase_order_line pol 
                                    		INNER JOIN  
                                    	purchase_order_header poh
                                    	ON poh.uid = pol.purchase_order_header_uid 
                                    GROUP BY sku_uid, org_uid 
                                    HAVING 
                                        org_uid = @OrgUID 
                                        AND poh.promotion_uid = @SchemeUID
                                    """);
            IDictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {
                    "@OrgUID", orgUid
                },
                {
                    "@SchemeUID", schemeUid
                },
            };
            if (skuUids != null && skuUids.Any())
            {
                sql.Append($" AND sku_uid IN @SKUUIDS ");
                parameters.Add("@SKUUIDS", skuUids);
            }
            _logger.LogInformation($"GetPurchaseOrderLineQPSs: query = {sql}, parameters = {parameters}");
            return ExecuteQueryAsync<IPurchaseOrderLineQPS>(sql.ToString(), parameters);
        }
        catch (Exception e)
        {
            _logger.LogError($"GetPurchaseOrderLineQPSs: {e.Message}");
            throw;
        }
    }

    private async Task<bool> CreateApprovalRequest(string purchaseOrderUid, ApprovalRequestItem approvalRequestItem)
    {
        try
        {
            IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
            approvalRequest.LinkedItemType = "PurchaseOrder";
            approvalRequest.LinkedItemUID = purchaseOrderUid;
            ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(approvalRequestItem, approvalRequest);
            return approvalRequestCreated.Success;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem approvalRequestItem)
    {
        try
        {
            await CreateApprovalRequest(purchaseOrderUid, approvalRequestItem);
            string sql = $"UPDATE purchase_order_header SET is_approval_created = 1  WHERE UID = @PurchaseOrderUid";
            var parameters = new
            {
                PurchaseOrderUid = purchaseOrderUid
            };
            await ExecuteNonQueryAsync(sql, parameters);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
