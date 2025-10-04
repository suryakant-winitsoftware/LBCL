using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.DL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Classes
{
    public class MSSQLProvisioningHeaderViewDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IProvisioningHeaderViewDL
    {
        public MSSQLProvisioningHeaderViewDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView?> GetProvisioningHeaderViewByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };
                var sql = $"""
                 SELECT 
                    SUM(CASE WHEN amount > 0 THEN amount ELSE 0 END) AS TotalAmount,
                    SUM(CASE WHEN amount < 0 THEN ABS(amount) ELSE 0 END) AS UsedAmount,
                    SUM(amount) AS BalanceAmount,
                    (SELECT actual_amount 
                     FROM wallet 
                     WHERE linked_item_type='Branch' 
                       AND type='P2' 
                       /* AND linked_item_uid='BranchUID' */
                       AND org_uid = @UID) AS P2Amount,
                    (SELECT actual_amount 
                     FROM wallet 
                     WHERE linked_item_type='HO' 
                       AND type='P3' 
                       AND org_uid = @UID) AS P3Amount,
                    (SELECT actual_amount 
                     FROM wallet 
                     WHERE linked_item_type='HO' 
                       AND type='P3S'
                       AND org_uid = @UID) AS HoAmount,
                   (SELECT TOP (1) a.city 
                     FROM address a 
                     WHERE a.linked_item_uid = @UID 
                       AND a.linked_item_type = 'store') AS City
                 FROM wallet_ledger 
                 WHERE org_uid = @UID and
                 (
                     (linked_item_type = 'Branch' AND type = 'P2' )
                     OR (linked_item_type = 'HO' AND type = 'P3' )
                     OR (linked_item_type = 'HO' AND type = 'P3S' )        
                 )
                 """;
                return await ExecuteSingleAsync<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        // Provision Approval
        public async Task<PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>> GetProvisionApprovalSummaryDetails(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {

            try
            {
                var sql = new StringBuilder(
                @"SELECT 
                S.name AS CustomerName,
                S.Code AS CustomerCode,
                PD.store_uid AS StoreUID,
                PD.scheme_type AS ProvisionType,
                SUM(PD.scheme_amount) AS Amount,
                STRING_AGG(CAST(PD.Id AS VARCHAR), ',') AS ProvisionIDs
                FROM 
                    provision_data PD
                INNER JOIN 
                    store S 
                    ON PD.store_uid = S.uid");
                

                var parameters = new Dictionary<string, object?>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>(filterCriterias, sbFilterCriteria,
                    parameters);

                    sql.Append(sbFilterCriteria);
                    
                }
                sql.Append(" GROUP BY S.name, S.Code, PD.store_uid, PD.scheme_type ");
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
                        $" ORDER BY CustomerName OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Model.Interfaces.IProvisionApproval> stores =
                    await ExecuteQueryAsync<Model.Interfaces.IProvisionApproval>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    var sqlCount = new StringBuilder(
                        @$"SELECT COUNT(1) AS Cnt from ({sql}) as Q");
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval> pagedResponse =
                    new PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>
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
        public async Task<PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>> GetProvisionApprovalDetailViewDetails(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(
                @"Select * from (Select PD.uid as UID, PD.id as Id , S.name as CustomerName, PD.store_uid as ChannelPartner,(select uid from list_item where code = PD.scheme_type) as SchemeType,PD.scheme_amount as Amount, 
                PD.invoice_date as InvoiceDate,PD.invoice_date as InvoiceToDate,PD.ar_no as ArNo, PD.gst_invoice_number as InvoiceNumber, PD.item_code as ItemCode, PD.qty as Quantity, PD.naration as Naration,
                PD.status as Status, PD.branch_code as Branch, PD.sales_office_code as SalesOffice, (select uid from broad_classification_header where name = S.broad_classification) as ClassificationFilter,
				PD.scheme_type as ProvisionType, S.broad_classification as BroadClassification, PD.remarks as Remarks from provision_data PD
                inner join store S on PD.store_uid = S.uid) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(
                    @"SELECT COUNT(1) AS Cnt from (Select PD.uid as UID, PD.id as Id , S.name as CustomerName, PD.store_uid as ChannelPartner,(select uid from list_item where code = PD.scheme_type) as SchemeType,PD.scheme_amount as Amount, 
                PD.invoice_date as InvoiceDate,PD.invoice_date as InvoiceToDate,PD.ar_no as ArNo, PD.gst_invoice_number as InvoiceNumber, PD.item_code as ItemCode, PD.qty as Quantity, PD.naration as Naration,
                PD.status as Status, PD.branch_code as Branch, PD.sales_office_code as SalesOffice, (select uid from broad_classification_header where name = S.broad_classification) as ClassificationFilter,
				PD.scheme_type as ProvisionType, S.broad_classification as BroadClassification, PD.remarks as Remarks from provision_data PD
                inner join store S on PD.store_uid = S.uid) as SubQuery");
                }

                var parameters = new Dictionary<string, object?>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>(filterCriterias, sbFilterCriteria,
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

                IEnumerable<Model.Interfaces.IProvisionApproval> stores =
                    await ExecuteQueryAsync<Model.Interfaces.IProvisionApproval>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval> pagedResponse =
                    new PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>
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
        public async Task<PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>> GetProvisionRequestHistoryDetails(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(
                @"Select Distinct PRH.id as Id, (select name from store where uid = PRH.store_uid) as CustomerName, PRH.provision_type as ProvisionType, PRH.amount as Amount,
                PRH.provision_ids as ProvisionIDs, PRH.requested_time as RequestedDate, PRH.requested_by_emp_uid as RequestedByEmpUID from provision_request_history PRH 
                Left join provision_data PD on PRH.store_uid = PD.store_uid
                Left join store S on PD.store_uid = S.uid");
                

                var parameters = new Dictionary<string, object?>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>(filterCriterias, sbFilterCriteria,
                    parameters);

                    sql.Append(sbFilterCriteria);
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
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(
                    $"SELECT COUNT(1) AS Cnt from ({sql}) as SubQuery");
                }
                IEnumerable<Model.Interfaces.IProvisionApproval> stores =
                    await ExecuteQueryAsync<Model.Interfaces.IProvisionApproval>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval> pagedResponse =
                    new PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval>
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
        public async Task<int> InsertProvisionRequestHistory(List<IProvisionApproval> provisionApproval)
        {
            try
            {
                var sql =
                    @"INSERT INTO dbo.provision_request_history (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, store_uid,
                      provision_type, amount, provision_ids, requested_by_emp_uid, requested_time) 
                      VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID, @ProvisionType, @Amount, @ProvisionIds,
                      @RequestedByEmpUID, @RequestedDate);";
                return await ExecuteNonQueryAsync(sql, provisionApproval);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateProvisionData(List<string> uIDs)
        {
            try
            {
                var idList = uIDs
            .SelectMany(id => id.Split(',')) // Split each entry by commas
            .Select(id => Convert.ToInt32(id.Trim())) // Convert to integers after trimming spaces
            .ToList();
                var sql = @"Update provision_data 
                            Set status = 'Requested'
                            Where id IN @UIDs";
                var parameters = new
                {
                    UIDs = idList
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IProvisionApproval> SelectProvisionByUID(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID", UID}
            };

            var sql = @" SELECT id AS Id, UID AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, (Select name from store where uid = store_uid) as ChannelPartner,
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, ss AS Ss, provision_id AS ProvisionId, customer_code AS CustomerCode, store_uid AS StoreUID, 
                        branch_code AS Branch, sales_office_code AS SalesOffice, oracle_order_number AS OracleOrderNumber, delivery_id AS DeliveryId, gst_invoice_number AS GstInvoiceNumber,
                        ar_no AS ArNo, invoice_date AS InvoiceDate, item_code AS ItemCode, qty AS Quantity, scheme_type AS ProvisionType, scheme_amount AS Amount, naration AS Naration, 
                        common_attribute_1 AS CommonAttribute1, common_attribute_2 AS CommonAttribute2, common_field_1 AS CommonField1, common_field_2 AS CommonField2, common_field_3 AS CommonField3,
                        common_field_4 AS CommonField4, common_field_5 AS CommonField5, status AS Status, is_dms_release_requested AS IsDmsReleaseRequested, is_oracle_processed AS IsOracleProcessed,
                        cn_number AS CnNumber, cn_date AS CnDate, cn_amount AS CnAmount
                        FROM dbo.provision_data WHERE id = @UID";

            return await ExecuteSingleAsync<IProvisionApproval>(sql, parameters);
        }
        
        public async Task<List<IProvisionApproval>> SelectProvisionRequestHistoryByProvisionIds(List<string> ProvisionIds)
        {

            var sql = @" SELECT id AS Id, UID AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, (Select name from store where uid = store_uid) as CustomerName,
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, ss AS Ss, provision_id AS ProvisionId, customer_code AS CustomerCode, store_uid AS StoreUID, 
                        branch_code AS Branch, sales_office_code AS SalesOffice, oracle_order_number AS OracleOrderNumber, delivery_id AS DeliveryId, gst_invoice_number AS InvoiceNumber,
                        ar_no AS ArNo, invoice_date AS InvoiceDate, item_code AS ItemCode, qty AS Quantity, scheme_type AS ProvisionType, scheme_amount AS Amount, naration AS Naration, 
                        common_attribute_1 AS CommonAttribute1, common_attribute_2 AS CommonAttribute2, common_field_1 AS CommonField1, common_field_2 AS CommonField2, common_field_3 AS CommonField3,
                        common_field_4 AS CommonField4, common_field_5 AS CommonField5, status AS Status, is_dms_release_requested AS IsDmsReleaseRequested, is_oracle_processed AS IsOracleProcessed,
                        cn_number AS CnNumber, cn_date AS CnDate, cn_amount AS CnAmount
                        FROM dbo.provision_data WHERE id in @ProvisionIDs";
            var parameters = new
            {
                ProvisionIDs = ProvisionIds
            };
            return await ExecuteQueryAsync<IProvisionApproval>(sql, parameters);
        }
    }
}
