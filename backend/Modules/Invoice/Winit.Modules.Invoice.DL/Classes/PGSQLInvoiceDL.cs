using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Invoice.DL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Invoice.DL.Classes;

public class PGSQLInvoiceDL : PostgresDBManager, IInvoiceDL
{
    public IWalletDL _walletUpdaterDL;

    public PGSQLInvoiceDL(IServiceProvider serviceProvider, IConfiguration configuration, IWalletDL walletDL) : base(serviceProvider, configuration)
    {
        _walletUpdaterDL = walletDL;
    }
    public async Task<IInvoiceHeaderView> GetInvoiceByUID(string invoiceUID)
    {
        try
        {
            string sql = """
                                            
                SELECT 
                i.uid,
                i.org_uid orguid, 
                o.code orgcode,
                o.name orgname,
                i.invoice_number invoiceno,
                i.invoice_date invoicedate,
                i.proforma_invoice_number as GSTInvoiceNo,
                i.customer_po as OraclePONumber,
                i.total_amount as TotalAmount,
                i.total_tax as TotalTax,
                i.net_amount as NetAmount,
                i.ar_number as ArNumber,
                poh.order_number as WINITPONumber,
                poh.uid as POUID,
                fs.relative_path +'/'+ fs.file_name as InvoiceURL
                FROM invoice i
                inner join org o on i.org_uid = o.uid  
                inner join my_orgs mo on mo.org_uid=o.uid 
                LEFT JOIN file_sys fs ON i.uid = fs.linked_item_uid and fs.linked_item_type = 'Invoice'
                LEFT JOIN purchase_order_header poh 
                ON poh.uid = i.linked_item_uid
                where i.UID=@UID
                """;
            var parameters = new Dictionary<string, object>()
            {
                {"UID",invoiceUID }
            };
            return await ExecuteSingleAsync<IInvoiceHeaderView>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<PagedResponse<IInvoiceHeaderView>> GetAllInvoices(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired, string jobPositionUID)
    {
        try
        {
            string cte = """
                                With Invoices as (
                SELECT 
                i.uid,
                i.org_uid orguid, 
                o.code orgcode,
                o.name orgname,
                i.invoice_number invoiceno,
                i.invoice_date invoicedate,
                i.proforma_invoice_number as GSTInvoiceNo,
                i.customer_po as OraclePONumber,
                i.total_amount as TotalAmount,
                i.total_tax as TotalTax,
                i.net_amount as NetAmount,
                i.ar_number as ArNumber,
                poh.order_number as WINITPONumber,
                poh.uid as POUID,
                fs.relative_path +'/'+ fs.file_name as InvoiceURL
                FROM invoice i
                inner join org o on i.org_uid = o.uid  
                inner join my_orgs mo on mo.org_uid=o.uid and mo.job_position_uid=@JobPositionUID
                LEFT JOIN file_sys fs ON i.uid = fs.linked_item_uid and fs.linked_item_type = 'Invoice'
                LEFT JOIN purchase_order_header poh 
                ON poh.uid = i.linked_item_uid) 

                """;
            StringBuilder sql = new(
            $"""
            {cte}
            select * from Invoices
            """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                $"""
                {cte}
                
                                
                select count(1) as cnt from Invoices
                """);
            }
            Dictionary<string, object?> parameters = new Dictionary<string, object?>()
            {
                {"JobPositionUID",jobPositionUID }
            };
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sql.Append(" WHERE ");
                _ = sqlCount.Append(" WHERE ");
                FilterCriteria? jobPositionFilterCriteria = filterCriterias
                    .Find(e => e.Name == "JobPositionUid");
                if (jobPositionFilterCriteria != null)
                {
                    if (jobPositionFilterCriteria.Value != null)
                    {
                        parameters["JobPositionUid"] = jobPositionFilterCriteria.Value;
                        string jobPositionQuery = """
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
                    if (filterCriterias.Any())
                    {
                        sql.Append($" AND ");
                        sqlCount.Append($" AND ");
                    }
                }
                AppendFilterCriteria<IInvoiceHeaderView>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    //_ = sqlCount.Append(" WHERE ");
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
                AppendSortCriteria([new SortCriteria("invoicedate", SortDirection.Desc)], sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<IInvoiceHeaderView> purchaseOrderHeaders = await ExecuteQueryAsync<IInvoiceHeaderView>(sql.ToString()
            , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IInvoiceHeaderView> pagedResponse = new()
            {
                PagedData = purchaseOrderHeaders,
                TotalCount = totalCount,
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IInvoiceMaster> GetInvoiceMasterByInvoiceUID(string invoiceUID)
    {
        try
        {
            IInvoiceMaster invoiceMaster = new InvoiceMaster();
            invoiceMaster.Invoiceheader = await GetInvoiceByUID(invoiceUID);
            invoiceMaster.InvoiceLines = await GetInvoiceLineViewByInvoiceUID(invoiceUID);
            IDictionary<string, List<string>> serailNumberKV = await GetSerialNumbersByInvoiceUid(invoiceUID);
            if (serailNumberKV != null)
            {
                foreach (var invoiceLine in invoiceMaster.InvoiceLines)
                {
                    if (serailNumberKV.TryGetValue(invoiceLine.UID, out List<string>? SerialNos))
                    {
                        invoiceLine.SerialNo = SerialNos;
                    }

                }
            }
            return invoiceMaster;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<List<IInvoiceLineView>> GetInvoiceLineViewByInvoiceUID(string invoiceUID)
    {
        try
        {
            string sql = """
                         SELECT 
                         il.uid,
                         item_code itemcode, 
                         ordered_qty orderedqty,
                         canceled_qty cancelledqty,
                         il.unit_price as UnitPrice,
                         il.total_amount as TotalAmount,
                         il.total_Tax as TotalTax,
                         qty shippedqty,
                         s.name as itemname
                         FROM invoice_line il LEFT JOIN SKU S ON s.uid = il.sku_uid
                         WHERE invoice_uid  = @InvoiceUID
                         """;
            var parameters = new
            {
                InvoiceUID = invoiceUID
            };
            return await ExecuteQueryAsync<IInvoiceLineView>(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<PagedResponse<IInvoiceApprove>> GetInvoiceApproveSatsusDetails(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT * From (SELECT IP.uid, O.code AS ChannelPartnerCode, O.name AS ChannelPartnerName, 
                                          I.invoice_number, I.invoice_date, IP.credit_note_amount_1,
                                          IP.credit_note_amount_2, IP.credit_note_amount_3,
                                          IP.credit_note_amount_4,IP.linked_item_type,IP.linked_item_uid
                                          FROM invoice_provisioning IP
                                          INNER JOIN invoice I on I.[uid] = IP.invoice_uid
                                          INNER JOIN org  O ON O.[uid] = I.org_uid
                                          WHERE IP.status = 0 )As SubQuery");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT IP.uid, O.code AS ChannelPartnerCode, O.name AS ChannelPartnerName, 
                                          I.invoice_number, I.invoice_date, IP.credit_note_amount_1,
                                          IP.credit_note_amount_2, IP.credit_note_amount_3,
                                          IP.credit_note_amount_4,IP.linked_item_type,IP.linked_item_uid
                                          FROM invoice_provisioning IP
                                          INNER JOIN invoice I on I.[uid] = IP.invoice_uid
                                          INNER JOIN org  O ON O.[uid] = I.org_uid
                                          WHERE IP.status = 0 )As SubQuery");
            }
            Dictionary<string, object> parameters = new();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.Invoice.Model.Interfaces.IInvoiceApprove>(filterCriterias, sbFilterCriteria, parameters);
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
            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    _ = sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }
            IEnumerable<Winit.Modules.Invoice.Model.Interfaces.IInvoiceApprove> invoiceApproves = await ExecuteQueryAsync<Winit.Modules.Invoice.Model.Interfaces.IInvoiceApprove>(sql.ToString(), parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Invoice.Model.Interfaces.IInvoiceApprove> pagedResponse = new()
            {
                PagedData = invoiceApproves,
                TotalCount = totalCount
            };
            return pagedResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IDictionary<string, List<string>>> GetSerialNumbersByInvoiceUid(string invoiceUid)
    {
        try
        {
            string sql = "SELECT invoice_line_uid InvoiceLineUID, serial_no SerialNo FROM invoice_line_serial_no WHERE invoice_uid = @InvoiceUID";
            return await Query(async (e) =>
                (await e.QueryAsync(sql, new
                {
                    InvoiceUID = invoiceUid
                }))
                .GroupBy(e => e.InvoiceLineUID)
                .ToDictionary(k => (string)k.Key, v => v.Select(x => (string)x.SerialNo).ToList())
            );
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<int> UpdateWalletFromInvoiceCNApproval(List<IProvisioningCreditNoteView> purchaseOrderLines
        , IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? iWalletLedgers = ConvertToWalletLedger(purchaseOrderLines);
            if (iWalletLedgers == null || iWalletLedgers.Count == 0)
            {
                return -1;
            }
            return await _walletUpdaterDL.UpdateWalletAsync(iWalletLedgers, connection, transaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public virtual List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? ConvertToWalletLedger(List<IProvisioningCreditNoteView> purchaseOrderLine)
    {
        if (purchaseOrderLine == null || purchaseOrderLine.Count == 0)
        {
            return null;
        }
        //var objpurchaseOrderLine = purchaseOrderLine
        //                    .GroupBy(x => x.PurchaseOrderHeaderUID)
        //                    .Select(g => new
        //                    {
        //                        PurchaseOrderHeaderUID = g.Key,
        //                        SellInCnP1Value = g.Sum(x => x.SellInCnP1Value),
        //                        SellInP2Amount = g.Sum(x => x.SellInP2Amount),
        //                        SellInP3Amount = g.Sum(x => x.SellInP3Amount),
        //                        P3StandingAmount = g.Sum(x => x.P3StandingAmount)
        //                    })
        //                    .ToList().FirstOrDefault();


        List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? WalletLedgerList = null;
        WalletLedgerList = new List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>();
        foreach (IProvisioningCreditNoteView objpurchaseOrderLine in purchaseOrderLine)
        {
            string sourceType;
            string sourceUid;
            string type;
            decimal amount;
            string CreatedBy;
            string CreditType;
            string linkedItemType;
            string linkedItemUid;
            sourceType = "PurchaseInvoice";
            if (objpurchaseOrderLine != null)
            {
                CreatedBy = objpurchaseOrderLine.ApprovedByEmpUID;
                sourceType = objpurchaseOrderLine.LinkedItemType;
                sourceUid = objpurchaseOrderLine.LinkedItemUid;

                //Not Required
                //type = "CNP1";
                //amount = objpurchaseOrderLine.CreditNoteAmount1;
                //CreatedBy = objpurchaseOrderLine.ApprovedByEmpUID;
                //CreditType = "1";
                //linkedItemType = "Dist";
                //linkedItemUid= objpurchaseOrderLine.OrgUID;
                //Winit.Modules.Scheme.Model.Interfaces.IWalletLedger? walletLedger = ConvertToWHStockLedger(objpurchaseOrderLine.OrgUID, sourceType, sourceUid
                //    , type, amount, CreatedBy, CreditType,linkedItemType,linkedItemUid);
                //if (walletLedger != null)
                //{
                //    WalletLedgerList.Add(walletLedger);
                //}

                type = "P2";
                amount = objpurchaseOrderLine.CreditNoteAmount2;
                linkedItemType = "Branch";
                linkedItemUid = objpurchaseOrderLine.BranchUID;
                CreditType = "1";
                Winit.Modules.Scheme.Model.Interfaces.IWalletLedger? walletLedger = ConvertToWHStockLedger(objpurchaseOrderLine.OrgUID, sourceType, sourceUid
                , type, amount, CreatedBy, CreditType, linkedItemType, linkedItemUid);
                if (walletLedger != null)
                {
                    WalletLedgerList.Add(walletLedger);
                }

                type = "P3";
                amount = objpurchaseOrderLine.CreditNoteAmount3;
                linkedItemType = "HO";
                linkedItemUid = objpurchaseOrderLine.HOOrgUID;
                CreditType = "1";
                walletLedger = ConvertToWHStockLedger(objpurchaseOrderLine.OrgUID, sourceType, sourceUid
                , type, amount, CreatedBy, CreditType, linkedItemType, linkedItemUid);
                if (walletLedger != null)
                {
                    WalletLedgerList.Add(walletLedger);
                }

                type = "P3S";
                amount = objpurchaseOrderLine.CreditNoteAmount4;
                linkedItemType = "HO";
                linkedItemUid = objpurchaseOrderLine.HOOrgUID;
                CreditType = "1";
                walletLedger = ConvertToWHStockLedger(objpurchaseOrderLine.OrgUID, sourceType, sourceUid
                , type, amount, CreatedBy, CreditType, linkedItemType, linkedItemUid);
                if (walletLedger != null)
                {
                    WalletLedgerList.Add(walletLedger);
                }
            }
        }

        return WalletLedgerList;
    }
    public virtual Winit.Modules.Scheme.Model.Interfaces.IWalletLedger? ConvertToWHStockLedger(string OrgUid, string sourceType, string sourceUid
        , string type, decimal amount, string CreatedBy, string CreditType, string linkedItemType, string linkedItemUid)
    {
        string warehouseUID = string.Empty;
        IWalletLedger walletLedger = _serviceProvider.GetRequiredService<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>();
        if (OrgUid == "")
        {
            return walletLedger;
        }
        walletLedger.Id = 0;
        walletLedger.UID = Guid.NewGuid().ToString();
        walletLedger.SS = 0;
        walletLedger.CreatedBy = CreatedBy;
        walletLedger.CreatedTime = DateTime.Now;
        walletLedger.ModifiedBy = CreatedBy;
        walletLedger.ModifiedTime = DateTime.Now;
        walletLedger.ServerAddTime = DateTime.Now;
        walletLedger.ServerModifiedTime = DateTime.Now;
        walletLedger.OrgUid = OrgUid;
        walletLedger.TransactionDateTime = DateTime.Now;
        walletLedger.Type = type;
        walletLedger.Amount = amount;
        walletLedger.CreditType = CreditType;
        walletLedger.SourceType = sourceType;
        walletLedger.SourceUid = sourceUid;
        walletLedger.LinkedItemType = linkedItemType;
        walletLedger.LinkedItemUid = linkedItemUid;

        return walletLedger;
    }

    public async Task<PagedResponse<IProvisioningCreditNoteView>> GetInvoiceApproveSatsusDetails(
        List<SortCriteria>? sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria>? filterCriterias,
        bool isCountRequired,
        bool status)
    {
        try
        {
            // SQL query with parameter for status
            var sql = new StringBuilder(
            """
              SELECT * FROM (SELECT IP.uid, O.code AS ChannelPartnerCode, O.name AS ChannelPartnerName ,O.uid As OrgUID, 
            I.invoice_number, I.invoice_date,IP.invoice_uid, IP.credit_note_amount_1,
            IP.credit_note_amount_2, IP.credit_note_amount_3,
            IP.credit_note_amount_4,
            poh.branch_uid as BranchUID,
            poh.ho_org_uid as HOOrgUID,IP.linked_item_type,IP.linked_item_uid
            FROM invoice_provisioning IP
            INNER JOIN invoice I ON I.[uid] = IP.invoice_uid
            INNER JOIN org O ON O.[uid] = I.org_uid
            Left JOIN purchase_order_header poh ON poh.uid = i.linked_item_uid
            WHERE IP.status = @Status) AS SubQuery
            """);

            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("""
                                              SELECT count(*) FROM (SELECT IP.uid, O.code AS ChannelPartnerCode, O.name AS ChannelPartnerName ,O.uid As OrgUID, 
                                             I.invoice_number, I.invoice_date, IP.credit_note_amount_1,
                                             IP.credit_note_amount_2, IP.credit_note_amount_3,
                                             IP.credit_note_amount_4,
                                             poh.branch_uid brnachuid,
                                             poh.ho_org_uid as hoorguid
                                             FROM invoice_provisioning IP
                                             INNER JOIN invoice I ON I.[uid] = IP.invoice_uid
                                             INNER JOIN org O ON O.[uid] = I.org_uid
                                             Left JOIN purchase_order_header poh ON poh.uid = i.linked_item_uid
                                             WHERE IP.status = @Status) AS SubQuery
                                             """);
            }

            var parameters = new Dictionary<string, object>
            {
                {
                    "@Status", status ? 1 : 0
                }
            };

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                var sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView>(filterCriterias, sbFilterCriteria, parameters);
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
                    sql.Append($" ORDER BY IP.uid OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }
            IEnumerable<IProvisioningCreditNoteView> invoiceApproves = await ExecuteQueryAsync<IProvisioningCreditNoteView>(sql.ToString(), parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            return new PagedResponse<IProvisioningCreditNoteView>
            {
                PagedData = invoiceApproves,
                TotalCount = totalCount
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateApprovedStatus(List<IProvisioningCreditNoteView> provisioningItems)
    {
        try
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();

            try
            {
                var walletUpdateResult = await UpdateWalletFromInvoiceCNApproval(provisioningItems, connection, transaction);

                if (walletUpdateResult <= 0 || walletUpdateResult != provisioningItems.Count)
                {
                    transaction.Rollback();
                    return -1;
                }
                string sql = """
                             UPDATE invoice_provisioning
                             SET 
                                 approved_by_emp_uid = @ApprovedByEmpUid,
                                 approved_on = @ApprovedOn,
                                 status = @Status
                             WHERE uid = @Uid;
                             """;

                int updateResult = await ExecuteNonQueryAsync(sql, connection, transaction, provisioningItems);

                if (updateResult != provisioningItems.Count)
                {
                    transaction.Rollback();
                    return -1;
                }

                transaction.Commit();
                return updateResult;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task<PagedResponse<IOutstandingInvoiceReport>> GetOutstandingInvoiceReportData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public async Task<List<IInvoiceView>> GetInvoicesForReturnOrder(InvoiceListRequest invoiceListRequest)
    {
        try
        {
            StringBuilder sql = new StringBuilder
            ("""
             SELECT * FROM
             (SELECT 
                 i.uid, 
                 i.invoice_number AS InvoiceNumber, 
                 i.invoice_date AS InvoiceDate, 
                 COALESCE(SUM(il.available_qty_bu), 0) AS AvailableQty
             FROM 
                 invoice i
             LEFT JOIN 
                 invoice_line il ON il.invoice_uid = i.uid AND il.available_qty_bu > 0
             LEFT JOIN 
                 sku s ON s.uid = il.sku_uid
             WHERE 
                 (@ModelNameOrCode is null OR s.code  = @ModelNameOrCode OR s.name =  @ModelNameOrCode) 
             GROUP BY 
                 i.uid, i.invoice_number, i.invoice_date
             )
             as SubQuery
             ORDER BY 
                InvoiceNumber DESC
             WHERE 
                 1=1 
             """);
            var parameters = new Dictionary<string, object?>();
            if (invoiceListRequest is not null)
            {
                if (invoiceListRequest.StartDate.HasValue && invoiceListRequest.EndDate.HasValue)
                {
                    parameters.Add("@StartDate", invoiceListRequest.StartDate.Value);
                    parameters.Add("@EndDate", invoiceListRequest.EndDate.Value);
                    sql.Append(" AND InvoiceDate BETWEEN @StartDate AND @EndDate ");
                }
                if (!string.IsNullOrEmpty(invoiceListRequest.InvoiceNumber))
                {
                    parameters.Add("@InvoiceNumber", invoiceListRequest.InvoiceNumber);
                    sql.Append(" AND InvoiceNumber = @InvoiceNumber ");
                }
                if (!string.IsNullOrEmpty(invoiceListRequest.ModelNameOrCode))
                {
                    parameters.Add("@ModelNameOrCode", invoiceListRequest.InvoiceNumber);
                    sql.Append(" AND ModelNameOrCode = @ModelNameOrCode ");
                }
            }
            return await ExecuteQueryAsync<IInvoiceView>(sql.ToString(), parameters);
        }
        catch (Exception e)
        {
            throw;
        }
    }
    //public async Task<List<IProvisionComparisonView>> GetProvisionComparisonReport(ProvisionComparisonRequest provisionComparisonRequest)
    //{
    //    try
    //    {
    //        StringBuilder sql = new StringBuilder
    //        ("""
    //         SELECT pd.ar_no as ArNo,pd.gst_invoice_number as GstInvoiceNumber, pd.invoice_date AS InvoiceDate, pd.item_code AS ItemCode, pd.qty AS Qty,'' provision_type AS ProvisionType,pdd.scheme_amount AS DmsProvisionAmount, pd.scheme_amount AS OracleProvisionAmount,
    //         pdd.scheme_amount - pd.scheme_amount AS Diff
    //         FROM provision_data pd
    //         LEFT JOIN provision_data_dms pdd ON pdd.customer_code = pd.customer_code AND pdd.store_uid =  pd.store_uid AND pdd.oracle_order_number = pd.oracle_order_number AND pdd.delivery_id = pd.delivery_id
    //         AND pdd.gst_invoice_number = pd.gst_invoice_number AND pdd.item_code = pd.item_code
    //         WHERE 
    //             1=1 
    //         """);
    //        var parameters = new Dictionary<string, object?>();
    //        if (provisionComparisonRequest is not null)
    //        {
    //            if (provisionComparisonRequest.StartDate.HasValue && provisionComparisonRequest.EndDate.HasValue)
    //            {
    //                parameters.Add("@StartDate", provisionComparisonRequest.StartDate.Value);
    //                parameters.Add("@EndDate", provisionComparisonRequest.EndDate.Value);
    //                sql.Append(" AND pd.invoice_date BETWEEN @StartDate AND @EndDate ");
    //            }
    //            //if (!string.IsNullOrEmpty(invoiceListRequest.InvoiceNumber))
    //            //{
    //            //    parameters.Add("@InvoiceNumber", invoiceListRequest.InvoiceNumber);
    //            //    sql.Append(" AND InvoiceNumber = @InvoiceNumber ");
    //            //}
    //            //if (!string.IsNullOrEmpty(invoiceListRequest.ModelNameOrCode))
    //            //{
    //            //    parameters.Add("@ModelNameOrCode", invoiceListRequest.InvoiceNumber);
    //            //    sql.Append(" AND ModelNameOrCode = @ModelNameOrCode ");
    //            //}
    //        }
    //        return await ExecuteQueryAsync<IProvisionComparisonView>(sql.ToString(), parameters);
    //    }
    //    catch (Exception e)
    //    {
    //        throw;
    //    }
    //}
    public async Task<PagedResponse<IProvisionComparisonReportView>> GetProvisionComparisonReport(
    List<SortCriteria> sortCriterias,
    int pageNumber,
    int pageSize,
    List<FilterCriteria> filterCriterias,
    bool isCountRequired)
    {
        try
        {
            // Base SQL query
            var sql = new StringBuilder(@"
SELECT * FROM (
    SELECT pdd.scheme_amount AS DmsProvisionAmount, 
           pd.ar_no AS ArNo, 
           pd.gst_invoice_number AS GstInvoiceNumber, 
           pd.invoice_date AS InvoiceDate,
           pd.sales_office_code AS SalesOffice,
           pd.item_code AS ItemCode, 
           pd.qty AS Qty, 
           '' AS ProvisionType, 
           pd.scheme_amount AS OracleProvisionAmount,
           ISNULL(pdd.scheme_amount, 0) - ISNULL(pd.scheme_amount, 0) AS Diff,
           vsia.broad_classification AS BroadClassification, 
           vsia.branch_uid AS Branch,
           vsia.store_uid AS ChannelPartner  
    FROM provision_data_dms pdd
    LEFT JOIN provision_data pd 
           ON pdd.customer_code = pd.customer_code 
           AND pdd.store_uid = pd.store_uid 
           AND pdd.oracle_order_number = pd.oracle_order_number 
           AND pdd.delivery_id = pd.delivery_id
           AND pdd.gst_invoice_number = pd.gst_invoice_number 
           AND pdd.item_code = pd.item_code
    INNER JOIN VW_StoreImpAttributes vsia 
           ON pdd.store_uid = vsia.store_uid
) AS subquery");

            // SQL Count Query
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount.Append(@"
    SELECT COUNT(1) AS Cnt FROM (
        SELECT pdd.scheme_amount AS DmsProvisionAmount, 
               pd.ar_no AS ArNo, 
               pd.gst_invoice_number AS GstInvoiceNumber, 
               pd.invoice_date AS InvoiceDate,
               pd.sales_office_code AS SalesOffice,
               pd.item_code AS ItemCode, 
               pd.qty AS Qty, 
               '' AS ProvisionType, 
               pd.scheme_amount AS OracleProvisionAmount,
               ISNULL(pdd.scheme_amount, 0) - ISNULL(pd.scheme_amount, 0) AS Diff,
               vsia.broad_classification AS BroadClassification, 
               vsia.branch_uid AS Branch,
               vsia.store_uid AS ChannelPartner  
        FROM provision_data_dms pdd
        LEFT JOIN provision_data pd 
               ON pdd.customer_code = pd.customer_code 
               AND pdd.store_uid = pd.store_uid 
               AND pdd.oracle_order_number = pd.oracle_order_number 
               AND pdd.delivery_id = pd.delivery_id
               AND pdd.gst_invoice_number = pd.gst_invoice_number 
               AND pdd.item_code = pd.item_code
        INNER JOIN VW_StoreImpAttributes vsia 
               ON pdd.store_uid = vsia.store_uid
    ) AS subquery");
            }

            // Parameters dictionary
            var parameters = new Dictionary<string, object?>();

            // Apply filter criteria
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder(" WHERE ");
                AppendFilterCriteria<IProvisionComparisonReportView>(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }

            // Apply sorting
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            // Apply pagination
            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY InvoiceDate OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            IEnumerable<IProvisionComparisonReportView> provisionComparisonDetails =
                await ExecuteQueryAsync<IProvisionComparisonReportView>(sql.ToString(), parameters);

            // Get total count if required
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            // Prepare the paged response
            return new PagedResponse<IProvisionComparisonReportView>
            {
                PagedData = provisionComparisonDetails.ToList(),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving provision comparison report: " + ex.Message, ex);
        }
    }

    public async Task<List<IProvisionDataDMS>> CreateProvision(string invoiceUID)
    {
        try
        {
            string sql = """
                         SELECT Uid FROM provision_data_dms
                         WHERE invoice_uid  = @InvoiceUID
                         """;
            var parameters = new
            {
                InvoiceUID = invoiceUID
            };
            List<IProvisionDataDMS> provisionDataDMS = await ExecuteQueryAsync<IProvisionDataDMS>(sql, parameters);
            if (provisionDataDMS.Count > 0)
            {
                return provisionDataDMS;
            }
            else
            {
                string query = """
                         INSERT INTO [dbo].[provision_data_dms]
                                    ([uid]
                                    ,[created_by]
                                    ,[created_time]
                                    ,[modified_by]
                                    ,[modified_time]
                                    ,[ss]
                                    ,[provision_id]
                                    ,[customer_code]
                                    ,[store_uid]
                                    ,[branch_code]
                                    ,[sales_office_code]
                                    ,[oracle_order_number]
                                    ,[invoice_uid]
                                    ,[invoice_line_uid]
                                    ,[delivery_id]
                                    ,[gst_invoice_number]
                                    ,[ar_no]
                                    ,[invoice_date]
                                    ,[sku_uid]
                                    ,[item_code]
                                    ,[qty]
                                    ,[scheme_type]
                                    ,[scheme_code]
                                    ,[scheme_amount]
                                    ,[naration]
                                    ,[is_verified]
                                    ,[verified_by_emp_uid]
                                    ,[is_dms_release_requested]
                                    ,[requested_by_emp_uid]
                                    ,[requested_time]
                                    ,[is_credit_note_processed]
                                    ,[acc_credit_note_uid]
                                    ,[cn_number]
                                    ,[cn_date]
                                    ,[cn_amount]
                                    ,[remarks])
                              SELECT NEWID() uid
                         		   ,'admin' [created_by]
                                    ,GETDATE() [created_time]
                                    ,'admin' [modified_by]
                                    ,GETDATE() [modified_time]
                                    ,'' [ss]
                         		   ,0 provision_id, V.store_code AS customer_code, V.store_uid, V.branch_code, NULL sales_office_code,
                         			I.customer_po AS oracle_order_number, I.uid AS Invoice_uid, IL.uid AS Invoice_line_uid,I.invoice_number as delivery_id, I.proforma_invoice_number AS gst_invoice_number,
                         			I.ar_number as ar_no, I.invoice_date, IL.sku_uid,IL.item_code, IL.qty,
                         			ISNULL(POLP.provision_type,'') as scheme_type, POLP.scheme_code, POLP.approved_provision_unit_amount * IL.qty AS scheme_amount
                         		   ,NULL [naration]
                                    ,0 [is_verified]
                                    ,NULL [verified_by_emp_uid]
                                    ,NULL [is_dms_release_requested]
                                    ,NULL [requested_by_emp_uid]
                                    ,NULL [requested_time]
                                    ,NULL [is_credit_note_processed]
                                    ,NULL [acc_credit_note_uid]
                                    ,NULL [cn_number]
                                    ,NULL [cn_date]
                                    ,NULL [cn_amount]
                                    ,NULL [remarks]

                         	FROM Invoice I
                         	INNER JOIN VW_StoreImpAttributes V ON V.store_uid = I.store_uid
                         	INNER JOIN invoice_line IL on IL.invoice_uid = I.uid
                         	LEFT JOIN purchase_order_line_provision POLP ON POLP.purchase_order_line_uid = IL.sales_order_line_uid
                         	AND POLP.is_selected = 1 AND POLP.approved_provision_unit_amount > 0
                         	WHERE I.uid = @InvoiceUID
                         """;
                var param = new
                {
                    InvoiceUID = invoiceUID
                };
                int updateResult = await ExecuteNonQueryAsync(query, param);
                provisionDataDMS = await ExecuteQueryAsync<IProvisionDataDMS>(sql, parameters);
                if (updateResult > 0)
                {
                    return provisionDataDMS;
                }
                else
                {
                    return provisionDataDMS;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

}
