using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.DL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.DL.Classes
{
    public class SQLiteOrgDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IOrgDL
    {
        public SQLiteOrgDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * from (SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        code AS Code,
                        name AS Name,
                        is_active AS IsActive,
                        org_type_uid AS OrgTypeUid,
                        parent_uid AS ParentUid,
                        country_uid AS CountryUid,
                        company_uid AS CompanyUid,
                        tax_group_uid AS TaxGroupUid,
                        status AS Status,
                        seq_code AS SeqCode,
                        has_early_access AS HasEarlyAccess
                    FROM 
                        org) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        code AS Code,
                        name AS Name,
                        is_active AS IsActive,
                        org_type_uid AS OrgTypeUid,
                        parent_uid AS ParentUid,
                        country_uid AS CountryUid,
                        company_uid AS CompanyUid,
                        tax_group_uid AS TaxGroupUid,
                        status AS Status,
                        seq_code AS SeqCode,
                        has_early_access AS HasEarlyAccess
                    FROM 
                        org) As SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IOrg>().GetType();

                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> OrgDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg> pagedResponse = new PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg>
                {
                    PagedData = OrgDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Org.Model.Interfaces.IOrg> GetOrgByUID(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        code AS Code,
                        name AS Name,
                        is_active AS IsActive,
                        org_type_uid AS OrgTypeUid,
                        parent_uid AS ParentUid,
                        country_uid AS CountryUid,
                        company_uid AS CompanyUid,
                        tax_group_uid AS TaxGroupUid,
                        status AS Status,
                        seq_code AS SeqCode,
                        has_early_access AS HasEarlyAccess
                    FROM 
                        org WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IOrg>().GetType();

            Winit.Modules.Org.Model.Interfaces.IOrg orgDetails = await ExecuteSingleAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql, parameters, type);
            return orgDetails;
        }
        public async Task<int> CreateOrg(Winit.Modules.Org.Model.Interfaces.IOrg createOrg)
        {
            try
            {
                var sql = @"Insert Into  org (id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, code, name, is_active, org_type_uid,
                parent_uid, country_uid, company_uid, tax_group_uid, status, seq_code, has_early_access ) Values(@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                    @ServerAddTime, @ServerModifiedTime, @Code, @Name, @IsActive, @OrgTypeUID
                          , @ParentUID, @CountryUID, @CompanyUID, @TaxGroupUID, @Status, @SeqCode, @HasEarlyAccess)";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                   {"Id", createOrg.Id},
                   {"UID", createOrg.UID},
                   {"CreatedBy", createOrg.CreatedBy},
                   {"ModifiedBy", createOrg.ModifiedBy},
                   {"CreatedTime", createOrg.CreatedTime},
                   {"ModifiedTime", createOrg.ModifiedTime},
                   {"ServerAddTime", createOrg.ServerAddTime},
                   {"ServerModifiedTime", createOrg.ServerModifiedTime},
                   {"Code", createOrg.Code},
                   {"Name", createOrg.Name},
                    {"IsActive", createOrg.IsActive},
                   {"OrgTypeUID", createOrg.OrgTypeUID},
                   {"ParentUID", createOrg.ParentUID},
                   {"CountryUID", createOrg.CountryUID},
                   {"CompanyUID", createOrg.CompanyUID},
                   {"TaxGroupUID", createOrg.TaxGroupUID},
                   {"Status", createOrg.Status},
                   {"SeqCode", createOrg.SeqCode},
                   {"HasEarlyAccess", createOrg.HasEarlyAccess},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateOrg(Winit.Modules.Org.Model.Interfaces.IOrg updateOrg)
        {
            try
            {
                var sql = @"UPDATE org SET 
                        modified_by = @ModifiedBy,
                        name = @Name,
                        is_active = @IsActive,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        code = @Code,
                        status = @Status
                    WHERE 
                        uid = @UID";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                   {"ModifiedBy", updateOrg.ModifiedBy},
                   {"ModifiedTime", updateOrg.ModifiedTime},
                   {"ServerModifiedTime", updateOrg.ServerModifiedTime},
                   {"Code", updateOrg.Code},
                   {"Name", updateOrg.Name},
                   {"IsActive", updateOrg.IsActive},
                   {"Status", updateOrg.Status},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteOrg(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID" , UID}
            };
            var sql = "DELETE  FROM org WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> InsertOrgHierarchy()
        {
            throw new NotImplementedException();
        }

        public Task<List<IOrg>> PrepareOrgMaster()
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> ViewFranchiseeWarehouse(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT DISTINCT
                               o.Id AS Id,
                               o.Code AS WarehouseCode,
                               o.UID AS WarehouseUID,
                               o.Name AS WarehouseName,
                               o.Code AS FranchiseCode,
                               o.Name AS FranchiseName,
                               o.modified_time AS ModifiedTime,
                               '[' + ot.UID + ']' + ot.Name AS OrgTypeUID,
                               CASE
                                   WHEN o.is_preferred_wh IS NULL OR o.org_type_uid = 'WOWH' THEN 0
                                   ELSE o.is_preferred_wh
                               END AS IsPreferredWH,
                               ot.uid AS OrgType
                           FROM
                               Org AS o
                           JOIN
                               Org_type AS ot ON o.org_type_uid = ot.UID
                           WHERE
                           O.org_type_uid IN ('FRWH', 'WOWH') AND O.parent_uid = @FranchiseeOrgUID ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(*) AS TotalCount FROM (SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        code AS Code,
                        name AS Name,
                        is_active AS IsActive,
                        org_type_uid AS OrgTypeUid,
                        parent_uid AS ParentUid,
                        country_uid AS CountryUid,
                        company_uid AS CompanyUid,
                        tax_group_uid AS TaxGroupUid,
                        status AS Status,
                        seq_code AS SeqCode,
                        has_early_access AS HasEarlyAccess
                    FROM 
                        org) As SubQuery AS o WHERE o.""org_type_uid IN ('FRWH', 'WOWH') AND o.parent_uid = @FranchiseeOrgUID;");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IOrg>().GetType();

                IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView> warehouseDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {

                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView> pagedResponse = new PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>
                {
                    PagedData = warehouseDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> ViewFranchiseeWarehouseByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<IWarehouseStockItemView>> GetWarehouseStockDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID, string WarehouseUID, string StockType)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateViewFranchiseeWarehouse(IEditWareHouseItemView createWareHouseItemView)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateViewFranchiseeWarehouse(IEditWareHouseItemView updateWareHouseItemView)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteViewFranchiseeWarehouse(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<IOrgType>> GetOrgTypeDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null, string? branchUID = null)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null)
        {

            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"OrgTypeUID",  OrgTypeUID},
                {"ParentUID",  parentUID}
            };
            var sql = new StringBuilder(@"SELECT  id AS Id,
                                uid AS UID,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                code AS Code,
                                name AS Name,
                                is_active AS IsActive,
                                org_type_uid AS OrgTypeUID,
                                parent_uid AS ParentUID,
                                country_uid AS CountryUID,
                                company_uid AS CompanyUID,
                                tax_group_uid AS TaxGroupUID,
                                status AS Status,
                                seq_code AS SeqCode,
                                has_early_access AS HasEarlyAccess
                            FROM
                                org WHERE org_type_uid = @OrgTypeUID");

            if (!string.IsNullOrEmpty(parentUID))
            {
                sql.Append(" AND parent_uid = @ParentUID;");
            }

            // Execute the query with SQLite-compatible parameters
            IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> orgDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql.ToString(), parameters);
            return orgDetails;
        }
        public Task<IEnumerable<IFOCStockItem>> GetFOCStockItemDetails(string StockType)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetVanStockItems(string warehouseUID, string orgUID,
            StockType? stockType)
        {
            string stockTypeQuery = string.Empty;
            if (stockType != null)
            {
                stockTypeQuery = $"AND wa.stock_type = '{Enum.GetName(stockType.Value)}'";
            }
            string query = $@"SELECT
                s.uid AS SKUUID,
                s.code AS SKUCode,
                s.name AS SKUName,
                wa.qty AS Qty,
                wa.uom AS UOM,
	            FS.relative_path || '/' || FS.file_name AS SKUImage,
                wa.stock_type AS StockType,
                su.multiplier AS OuterMultiplier,
                CAST(wa.qty / su.multiplier AS INT) AS OuterQty,
                wa.qty % su.multiplier AS EAQty,
                COALESCE(wa.qty, 0) AS TotalEAQty
            FROM
                wh_stock_available wa
                INNER JOIN sku s ON s.uid = wa.sku_uid {stockTypeQuery}
                INNER JOIN sku_uom su ON su.sku_uid = s.uid AND su.is_outer_uom = TRUE
	            LEFT JOIN file_sys FS ON FS.linked_item_type = '{Winit.Shared.Models.Constants.LinkedItemType.SKU}' AND FS.linked_item_uid = S.UID
             AND FS.file_sys_type = '{Winit.Shared.Models.Constants.FileSysType.Image}' AND fs.file_type = '{Winit.Modules.Common.Model.Constants.FileTypeConstants.Image}' AND FS.is_default = 1
            WHERE
                wa.warehouse_uid = @warehouseUID";


            Dictionary<string, object?> parameters = new()
            {
            { "warehouseUID", warehouseUID }
            };

            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>().GetType();
            IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> vanStockItems = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>(query, parameters, type);

            return vanStockItems;
        }
        public async Task<List<string>> GetOrgHierarchyParentUIDsByOrgUID(List<string> orgs)
        {
            try
            {
                string sql = "select parent_uid from org_hierarchy where org_uid in @OrgUIDs";
                var parameters = new
                {
                    OrgUIDs = orgs
                };
                return await ExecuteQueryAsync<string>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<ISelectionItem>> GetProductOrgSelectionItems()
        {
            try
            {
                string sql = "select distinct o.uid, o.code, o.name as label  from org o inner join sku s on s.org_uid = o.uid";
                return await ExecuteQueryAsync<ISelectionItem>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ISelectionItem>> GetProductDivisionSelectionItems()
        {
            try
            {
                string sql = "select distinct o.uid, o.code, o.name as label  from org o inner join sku s on s.supplier_org_uid = o.uid";
                return await ExecuteQueryAsync<ISelectionItem>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        Task<int> IOrgDL.CreateOrgBulk(List<IOrg> createOrg)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDivisions(string storeUID)
        {
            string sql;
            try
            {
                var parameters = new Dictionary<string, object>()
        {
            {"StoreUID", storeUID }
        };

                if (!string.IsNullOrEmpty(storeUID))
                {
                    sql = @"SELECT
                        O.Id,
                        O.uid AS UID,
                        O.created_by AS CreatedBy,
                        O.created_time AS CreatedTime,
                        O.modified_by AS ModifiedBy,
                        O.modified_time AS ModifiedTime,
                        O.server_add_time AS ServerAddTime,
                        O.server_modified_time AS ServerModifiedTime,
                        O.code AS Code,
                        '[' + O.code + ']' + O.name AS Name,  
                        O.is_active AS IsActive,
                        O.org_type_uid AS OrgTypeUID,
                        O.parent_uid AS ParentUID,
                        O.country_uid AS CountryUID,
                        O.company_uid AS CompanyUID,
                        O.tax_group_uid AS TaxGroupUID,
                        O.status AS Status,
                        O.seq_code AS SeqCode,
                        O.has_early_access AS HasEarlyAccess
                    FROM store_credit SC
                    INNER JOIN org O ON O.uid = SC.division_org_uid 
                    WHERE SC.store_uid = @StoreUID";
                }
                else
                {
                    sql = @"SELECT
                        O.id AS Id,
                        O.uid AS UID,
                        O.created_by AS CreatedBy,
                        O.created_time AS CreatedTime,
                        O.modified_by AS ModifiedBy,
                        O.modified_time AS ModifiedTime,
                        O.server_add_time AS ServerAddTime,
                        O.server_modified_time AS ServerModifiedTime,
                        O.code AS Code,
                        '[' || O.code || ']' || O.name AS Name,  -- Use '||' for string concatenation in PostgreSQL
                        O.is_active AS IsActive,
                        O.org_type_uid AS OrgTypeUID,
                        O.parent_uid AS ParentUID,
                        O.country_uid AS CountryUID,
                        O.company_uid AS CompanyUID,
                        O.tax_group_uid AS TaxGroupUID,
                        O.status AS Status,
                        O.seq_code AS SeqCode,
                        O.has_early_access AS HasEarlyAccess
                    FROM org O
                    WHERE O.org_type_uid = 'Supplier'";
                }

                return await ExecuteQueryAsync<IOrg>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<PagedResponse<IWareHouseStock>> GetAllWareHouseStock(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISKUGroup>> GetSkuGroupBySkuGroupTypeUID(string skuGroupTypeUid)
        {
            throw new NotImplementedException();
        }
    }
}
