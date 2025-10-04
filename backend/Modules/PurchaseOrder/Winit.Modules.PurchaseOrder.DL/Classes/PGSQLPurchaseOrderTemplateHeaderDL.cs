using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLPurchaseOrderTemplateHeaderDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPurchaseOrderTemplateHeaderDL
{
    private readonly IPurchaseOrderTemplateLineDL _purchaseOrderTemplateLineDL;

    public PGSQLPurchaseOrderTemplateHeaderDL(IServiceProvider serviceProvider, IConfiguration config, IPurchaseOrderTemplateLineDL purchaseOrderTemplateLineDL)
        : base(serviceProvider, config)
    {
        _purchaseOrderTemplateLineDL = purchaseOrderTemplateLineDL;
    }

    public async Task<PagedResponse<IPurchaseOrderTemplateHeader>> GetAllPurchaseOrderTemplateHeaders(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(
            """
            SELECT  * FROM
            (SELECT id, uid, created_by as createdby, created_time as createdtime, modified_by, modified_time, server_add_time, 
            server_modified_time as servermodifiedtime , ss, org_uid as orguid, 
            template_name as templatename, is_active as isactive, store_uid as StroreUid, is_created_by_store as IsCreatedByStore
            FROM purchase_order_template_header)
            AS  purchaseordertemplateheader
            """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                """
                    SELECT count(*) FROM
                    (SELECT id, uid, created_by as createdby, created_time as createdtime, modified_by, modified_time, server_add_time, 
                    server_modified_time as servermodifiedtime , ss, org_uid as orguid, 
                    template_name as templatename, is_active as isactive, store_uid as StroreUid, is_created_by_store as IsCreatedByStore
                    FROM purchase_order_template_header)
                    AS  purchaseordertemplateheader
                """);
            }
            Dictionary<string, object?> parameters = [];
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
                
                AppendFilterCriteria<IPurchaseOrderTemplateHeader>(filterCriterias, sbFilterCriteria, parameters);

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
                AppendSortCriteria([new SortCriteria("createdtime", SortDirection.Desc)], sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders = await ExecuteQueryAsync<IPurchaseOrderTemplateHeader>(sql.ToString()
            , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPurchaseOrderTemplateHeader> pagedResponse = new()
            {
                PagedData = purchaseOrderTemplateHeaders, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreatePurchaseOrderTemplateHeaders(List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                         INSERT INTO purchase_order_template_header
                         (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, org_uid, 
                          template_name, is_active, store_uid, is_created_by_store)
                         VALUES( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                         @SS, @OrgUID, @TemplateName, @IsActive, @StoreUID, @IsCreatedByStore);
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderTemplateHeaders);
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<int> UpdatePurchaseOrderTemplateHeader(
        List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = """
                         UPDATE purchase_order_template_header
                         SET 
                         created_by=@CreatedBy,
                         created_time=@CreatedTime,
                         modified_by=@ModifiedBy,
                         modified_time=@ModifiedTime,
                         server_add_time=@ServerAddTime,
                         server_modified_time=@ServerModifiedTime,
                         ss=@SS,
                         org_uid=@OrgUID,
                         template_name=@TemplateName,
                         is_active=@IsActive,
                         Store_uid=@StoreUID,
                         is_created_by_store=@IsCreatedByStore
                         WHERE
                                 uid = @UID;
                         """;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderTemplateHeaders);
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<bool> CUD_PurchaseOrderTemplate(IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster)
    {
        using IDbConnection connection = CreateConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            if (purchaseOrderTemplateMaster.ActionType == ActionType.Update)
            {
                if (await UpdatePurchaseOrderTemplateHeader([purchaseOrderTemplateMaster.PurchaseOrderTemplateHeader], connection, transaction) != 1)
                {
                    transaction.Rollback();
                    return false;
                }
                List<string>? existingUids = await CheckIfUIDExistsInDB("purchase_order_template_line", purchaseOrderTemplateMaster.PurchaseOrderTemplateLines!.Select(e => e.UID).ToList(), connection, transaction);
                if (existingUids == null) existingUids = [];
                List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLinesforUpdate = purchaseOrderTemplateMaster.PurchaseOrderTemplateLines!.FindAll(e => existingUids!.Contains(e.UID));
                List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLinesforAdd = purchaseOrderTemplateMaster.PurchaseOrderTemplateLines!.FindAll(e => !existingUids!.Contains(e.UID));
                if (purchaseOrderTemplateLinesforUpdate != null && purchaseOrderTemplateLinesforUpdate.Any() && await _purchaseOrderTemplateLineDL.UpdatePurchaseOrderTemplateLines
                    (purchaseOrderTemplateLinesforUpdate, connection, transaction) != purchaseOrderTemplateLinesforUpdate.Count)
                {
                    transaction.Rollback();
                    return false;
                }
                if (purchaseOrderTemplateLinesforAdd != null && purchaseOrderTemplateLinesforAdd.Any() && await _purchaseOrderTemplateLineDL.CreatePurchaseOrderTemplateLines
                    (purchaseOrderTemplateLinesforAdd, connection, transaction) != purchaseOrderTemplateLinesforAdd.Count)
                {
                    transaction.Rollback();
                    return false;
                }
                transaction.Commit();
                return true;
            }
            else if (purchaseOrderTemplateMaster.ActionType == ActionType.Add)
            {
                if (await CreatePurchaseOrderTemplateHeaders([purchaseOrderTemplateMaster.PurchaseOrderTemplateHeader], connection, transaction) != 1)
                {
                    transaction.Commit();
                    return false;
                }
                if (await _purchaseOrderTemplateLineDL.CreatePurchaseOrderTemplateLines
                    (purchaseOrderTemplateMaster.PurchaseOrderTemplateLines!, connection, transaction) != purchaseOrderTemplateMaster.PurchaseOrderTemplateLines!.Count)
                {
                    transaction.Commit();
                    return false;
                }
            }
            else
            {

            }
            transaction.Commit();
            return true;
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

    public async Task<IPurchaseOrderTemplateMaster> GetPurchaseOrderTemplateMasterByUID(string uid)
    {
        try
        {
            FilterCriteria purchaseOrderTemplateHeaderFilterCriteria = new("UID", uid, FilterType.Equal);
            IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster = _serviceProvider.GetRequiredService<IPurchaseOrderTemplateMaster>();
            purchaseOrderTemplateMaster.PurchaseOrderTemplateHeader = (await GetAllPurchaseOrderTemplateHeaders(null, 0, 0,
                [purchaseOrderTemplateHeaderFilterCriteria], false))
                .PagedData
                .FirstOrDefault()!;
            FilterCriteria purchaseOrderTemplateLineFilterCriteria = new("purchaseordertemplateheaderuid", uid, FilterType.Equal);

            purchaseOrderTemplateMaster.PurchaseOrderTemplateLines = (await _purchaseOrderTemplateLineDL.GetAllPurchaseOrderTemplateLines
                    (null, 0, 0, [purchaseOrderTemplateLineFilterCriteria], false))
                .PagedData
                .ToList();
            return purchaseOrderTemplateMaster;

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<int> DeletePurchaseOrderHeaderByUID(List<string> purchaseOrderHeaderUids, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        using IDbConnection connection = CreateConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            await _purchaseOrderTemplateLineDL.DeletePurchaseOrderTemplateLinesByPurchaseOrderTemplateHeaderUIDs(purchaseOrderHeaderUids, connection, transaction);
            string sql = """
                         DELETE FROM purchase_order_template_header WHERE uid in @UIDs;
                         """;
            var parameters = new
            {
                UIDs = purchaseOrderHeaderUids
            };
            if (await ExecuteNonQueryAsync(sql, connection, transaction, parameters) == purchaseOrderHeaderUids.Count)
            {
                transaction.Commit();
            }
            return purchaseOrderHeaderUids.Count;
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

    public async Task<List<IPurchaseOrderTemplateHeader>> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(string storeUid, string? createdBy = null)
    {
        try
        {
            StringBuilder sql = new StringBuilder("""
                                                  SELECT * FROM purchase_order_template_header
                                                  where is_active = 1 AND  (store_uid = @channelPartnerUID)
                                                  """);
            var parameters = new
            {
                channelPartnerUID = storeUid, EmpUid = createdBy
            };
            if (!string.IsNullOrEmpty(createdBy))
            {
                sql.Append($"  OR (created_by = @EmpUid and is_created_by_store = 0) ");
            }
            return await ExecuteQueryAsync<IPurchaseOrderTemplateHeader>(sql.ToString(), parameters);
        }
        catch (Exception e)
        {
            throw;
        }
    }

}
