using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLPurchaseOrderLineProvisionDL : SqlServerDBManager, IPurchaseOrderLineProvisionDL
{

    public PGSQLPurchaseOrderLineProvisionDL(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider, configuration)
    {

    }
    public async Task<PagedResponse<IPurchaseOrderLineProvision>> GetAllPurchaseOrderLineProvisions(List<FilterCriteria>? filters = null, List<SortCriteria>? sorts = null,
        int? pageSize = null, int? pageNumber = null, bool isCountRequired = false)
    {
        try
        {
            var sql = new StringBuilder(
            """
                select * from (
                    SELECT id , uid, created_by as createdby, created_time as createdtime, modified_by as modifiedby, modified_time as modifiedtime,
                           server_add_time as serveraddtime, server_modified_time as servermodifiedtime, ss, purchase_order_line_uid as purchaseorderlineuid,
                           provision_type as provisiontype, is_selected as isselected, scheme_code as schemecode, actual_provision_unit_amount as actualprovisionunitamount,
                           approved_provision_unit_amount as approvedprovisionunitamount, remarks as remarks 
                    FROM purchase_order_line_provision 
                                ) as SubQuery
            """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                """
                select count(*) from (
                SELECT id , uid, created_by as createdby, created_time as createdtime, modified_by as modifiedby, modified_time as modifiedtime,
                       server_add_time as serveraddtime, server_modified_time as servermodifiedtime, ss, purchase_order_line_uid as purchaseorderlineuid,
                       provision_type as provisiontype, is_selected as isselected, scheme_code as schemecode, actual_provision_unit_amount as actualprovisionunitamount,
                       approved_provision_unit_amount as approvedprovisionunitamount, remarks as remarks 
                FROM purchase_order_line_provision 
                            ) as SubQuery
                """);
            }

            var parameters = new Dictionary<string, object?>();

            if (filters != null && filters.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineProvision>(filters, sbFilterCriteria,
                parameters);

                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sorts != null && sorts.Count > 0)
            {
                sql.Append(" ORDER BY ");
                AppendSortCriteria(sorts, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                if (sorts != null && sorts.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append(
                    $" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            IEnumerable<Model.Interfaces.IPurchaseOrderLineProvision> purchaseOrderLineProvisions =
                await ExecuteQueryAsync<Model.Interfaces.IPurchaseOrderLineProvision>(sql.ToString(), parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineProvision> pagedResponse =
                new PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineProvision>
                {
                    PagedData = purchaseOrderLineProvisions, TotalCount = totalCount
                };
            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreatePurchaseOrderLineProvisions(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions,
        IDbConnection dbConnection = null, IDbTransaction dbTransaction = null)
    {
        try
        {
            String sql = """
                         INSERT INTO
                             purchase_order_line_provision 
                             (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, purchase_order_line_uid,
                              provision_type, is_selected, scheme_code, actual_provision_unit_amount, approved_provision_unit_amount, remarks) 
                         VALUES
                             (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS, @PurchaseOrderLineUID, 
                              @ProvisionType, @IsSelected, @SchemeCode, @ActualProvisionUnitAmount, @ApprovedProvisionUnitAmount, @Remarks);
                         """;
            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderLineProvisions);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public Task<int> UpdatePurchaseOrderLineProvisions(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null)
    {
        string sql = """
                     UPDATE 
                         purchase_order_line_provision 
                     SET 
                          created_by=@CreatedBy, 
                          created_time=@CreatedTime, 
                          modified_by=@ModifiedBy,
                          modified_time=@ModifiedTime, 
                          server_add_time=@ServerAddTime, 
                          server_modified_time=@ServerModifiedTime, 
                          ss=@SS, 
                          purchase_order_line_uid=@PurchaseOrderLineUID, 
                          provision_type=@ProvisionType, 
                          is_selected=@IsSelected, 
                          scheme_code=@SchemeCode,
                          actual_provision_unit_amount=@ActualProvisionUnitAmount,
                          approved_provision_unit_amount=@ApprovedProvisionUnitAmount,
                          remarks=@Remarks 
                     WHERE 
                         uid=@UID;
                     """;
        return ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderLineProvisions);
    }
    public Task<int> DeletePurchaseOrderLineProvisionsByUids(List<string> purchaseOrderLineProvisionUiDs, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null)
    {
        string sql = """
                     DELETE FROM purchase_order_line_provision where UID IN @PurchaseOrderLineUIDs;
                     """;
        var parameters = new
        {
            PurchaseOrderLineUIDs = purchaseOrderLineProvisionUiDs
        };
        return ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, parameters);
    }

    public Task<int> DeletePurchaseOrderLineProvisionsByPurchaseOrderLineUids(List<string> purchaseOrderLineProvisionUID, IDbConnection dbConnection = null, IDbTransaction dbTransaction = null)
    {
        try
        {
            string sql = """
                         DELETE FROM    purchase_order_line_provision where Purchase_Order_Line_UID IN @PurchaseOrderLineUIDs;
                         """;
            var parameters = new
            {
                PurchaseOrderLineUIDs = purchaseOrderLineProvisionUID
            };
            return ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<string>?> GetPurchaseOrderLineProvisionUidsByPurchaseOrderUID(string purchaseOrderUID, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var parameters = new { purchaseOrderUID = purchaseOrderUID };
            string sql = $@"select polp.uid from purchase_order_header h (nolock)
                        inner join purchase_order_line l(nolock) on l.purchase_order_header_uid=h.uid
                        inner join purchase_order_line_provision polp(nolock) on polp.purchase_order_line_uid=l.uid 
                        where h.uid=@purchaseOrderUID";
            return await ExecuteQueryAsync<string>(sql, parameters, null, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
