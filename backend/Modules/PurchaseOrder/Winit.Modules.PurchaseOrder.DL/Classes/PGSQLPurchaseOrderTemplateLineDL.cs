using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLPurchaseOrderTemplateLineDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPurchaseOrderTemplateLineDL
{
    public PGSQLPurchaseOrderTemplateLineDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }


    public async Task<PagedResponse<IPurchaseOrderTemplateLine>> GetAllPurchaseOrderTemplateLines(List<SortCriteria>? sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(
                    """
                    SELECT  * FROM
                    (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                    server_modified_time, ss, purchase_order_template_header_uid AS purchaseordertemplateheaderuid, 
                    line_number AS linenumber, sku_uid AS skuuid, sku_code as skucode, uom, qty
                    FROM purchase_order_template_line)
                    AS  purchaseordertemplateline
                    """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                    """
                    SELECT count(*) FROM
                        (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time, ss, purchase_order_template_header_uid AS purchaseordertemplateheaderuid, 
                        line_number AS linenumber, sku_uid AS skuuid, sku_code as skucode, uom, qty
                        FROM purchase_order_template_line)
                        AS  purchaseordertemplateline
                    """);
            }
            Dictionary<string, object?> parameters = [];
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sql.Append(" WHERE ");
                AppendFilterCriteria<IPurchaseOrderTemplateLine>(filterCriterias, sbFilterCriteria, parameters);

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

            IEnumerable<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines = await ExecuteQueryAsync<IPurchaseOrderTemplateLine>(sql.ToString()
                , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPurchaseOrderTemplateLine> pagedResponse = new()
            {
                PagedData = purchaseOrderTemplateLines,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreatePurchaseOrderTemplateLines(List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines,
       IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                         INSERT INTO purchase_order_template_line
                         (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                         ss, purchase_order_template_header_uid, line_number, sku_uid, sku_code, uom, qty)
                         VALUES( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                         @SS, @PurchaseOrderTemplateHeaderUID, @LineNumber, @SKUUID, @SKUCode, @UOM, @Qty);
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderTemplateLines);
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<int> UpdatePurchaseOrderTemplateLines(
        List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                         UPDATE purchase_order_template_line
                         SET 
                         created_by=@CreatedBy,
                         created_time=@CreatedTime,
                         modified_by=@ModifiedBy,
                         modified_time=@ModifiedTime,
                         server_add_time=@ServerAddTime,
                         server_modified_time=@ServerModifiedTime,
                         ss=@SS,
                         purchase_order_template_header_uid= @PurchaseOrderTemplateHeaderUID,
                         line_number=@LineNumber,
                         sku_uid=@SKUUID,
                         sku_code=@SKUCode, 
                         uom=@UOM, 
                         qty=@Qty
                         WHERE
                         uid = @UID;
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderTemplateLines);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                DELETE FROM purchase_order_template_line WHERE uid IN @UIDs
                """;
            var parameters = new
            {
                UIDs = purchaseOrderTemplateLineUids,
            };
            return await ExecuteNonQueryAsync(sql,dbConnection, dbTransaction, parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }
    
    public async Task<int> DeletePurchaseOrderTemplateLinesByPurchaseOrderTemplateHeaderUIDs(List<string> purchaseOrderTemplateHeaderUids,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                DELETE FROM purchase_order_template_line WHERE purchase_order_template_header_uid IN @UIDs
                """;
            var parameters = new
            {
                UIDs = purchaseOrderTemplateHeaderUids,
            };
            return await ExecuteNonQueryAsync(sql,dbConnection, dbTransaction, parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }

}
