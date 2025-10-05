using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.DL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.DL.Classes
{
    public class PGSQLOrgDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IOrgDL
    {


        public PGSQLOrgDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM   
                                        (SELECT id AS Id,
                                        uid AS UID,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        code AS Code,
                                        '[' || code::text || ']' || name AS Name,
                                        is_active AS IsActive,
                                        org_type_uid AS OrgTypeUID,
                                        parent_uid AS ParentUID,
                                        country_uid AS CountryUID,
                                        company_uid AS CompanyUID,
                                        tax_group_uid AS TaxGroupUID,
                                        status AS Status,
                                        seq_code AS SeqCode,
                                        has_early_access AS HasEarlyAccess,
                                        show_in_ui AS ShowInUI,
                                        show_in_template AS ShowInTemplate
                                    FROM
                                        org) AS subquery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,
                                        uid AS UID,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        code AS Code,
                                        '[' || code::text || ']' || name AS Name,
                                        is_active AS IsActive,
                                        org_type_uid AS OrgTypeUID,
                                        parent_uid AS ParentUID,
                                        country_uid AS CountryUID,
                                        company_uid AS CompanyUID,
                                        tax_group_uid AS TaxGroupUID,
                                        status AS Status,
                                        seq_code AS SeqCode,
                                        has_early_access AS HasEarlyAccess,
                                        show_in_ui AS ShowInUI,
                                        show_in_template AS ShowInTemplate
                                    FROM
                                        org) AS subquery ");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" where ");
                    AppendFilterCriteria<Winit.Modules.Org.Model.Interfaces.IOrg>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
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
            var sql = @"SELECT  id AS Id,
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
                                        has_early_access AS HasEarlyAccess,
                                        show_in_ui AS ShowInUI,
                                        show_in_template AS ShowInTemplate
                                    FROM
                                        org WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IOrg>().GetType();

            Winit.Modules.Org.Model.Interfaces.IOrg orgDetails = await ExecuteSingleAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql, parameters, type);
            return orgDetails;
        }
        public async Task<int> CreateOrg(Winit.Modules.Org.Model.Interfaces.IOrg createOrg)
        {
            try
            {
                var sql = @"INSERT INTO org (id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, code, name, is_active, org_type_uid, parent_uid, country_uid, company_uid, tax_group_uid, status, seq_code, has_early_access, show_in_ui, show_in_template)
                            VALUES (@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Code, @Name, @IsActive, @OrgTypeUID, @ParentUID, @CountryUID, @CompanyUID, @TaxGroupUID, @Status, @SeqCode, @HasEarlyAccess, @ShowInUI, @ShowInTemplate);";
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
                   {"ShowInUI", createOrg.ShowInUI},
                   {"ShowInTemplate", createOrg.ShowInTemplate},


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

                var sql = @"UPDATE org 
                                    SET 
                                        modified_by = @ModifiedBy, 
                                        name = @Name, 
                                        is_active = @IsActive, 
                                        modified_time = @ModifiedTime, 
                                        server_modified_time = @ServerModifiedTime, 
                                        code = @Code,
                                        status = @Status,
                                        show_in_ui = @ShowInUI,
                                        show_in_template = @ShowInTemplate 
                                    WHERE 
                                        uid = @UID;";

                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                   {"ModifiedBy", updateOrg.ModifiedBy},
                   {"ModifiedTime", updateOrg.ModifiedTime},
                   {"ServerModifiedTime", updateOrg.ServerModifiedTime},
                   {"Code", updateOrg.Code},
                   {"Name", updateOrg.Name},
                   {"IsActive", updateOrg.IsActive},
                   {"Status", updateOrg.Status},
                   {"ShowInUI", updateOrg.ShowInUI},
                   {"ShowInTemplate", updateOrg.ShowInTemplate},
                   {"UID", updateOrg.UID},
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
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "UID", UID }
        };

                var sql = @"DELETE FROM Org WHERE UID = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState == "23503")
                {
                    return -1;
                }
                throw new Exception("An error occurred while deleting the organization.", ex);
            }
        }

        public async Task<int> InsertOrgHierarchy()
        {
            int retValue = 0;
            try
            {
                string hierarchySql = @"
                TRUNCATE TABLE org_hierarchy;

                WITH RECURSIVE org_hierarchy AS (
                    SELECT o.uid, o.parent_uid, 1 AS level, o.created_by, o.modified_by
                    FROM 
                        org o
                    LEFT JOIN 
                        org c ON o.uid = c.parent_uid
                    WHERE 
                        c.uid IS NULL
                
                    UNION ALL
                
                    SELECT cp.uid, po.parent_uid, cp.level + 1 AS level, po.created_by, po.modified_by
                    FROM 
                        org_hierarchy cp
                    INNER JOIN 
                        org po ON po.uid = cp.parent_uid
                )
                
                INSERT INTO org_hierarchy (
                    uid, org_uid, parent_uid, parent_level, created_by, modified_by, created_time, modified_time, server_add_time, server_modified_time
                )
                SELECT 
                    gen_random_uuid(),
                    d.uid, 
                    d.parent_uid, 
                    d.level,  
                    d.created_by,
                    d.modified_by,
                    NOW(),
                    NOW(), 
                    NOW(), 
                    NOW()
                FROM 
                    (SELECT DISTINCT uid, parent_uid, level, created_by, modified_by FROM org_hierarchy WHERE parent_uid IS NOT NULL) AS d
                ORDER BY 
                    uid, level;";

                retValue = await ExecuteNonQueryAsync(hierarchySql, null);
            }
            catch (Exception ex)
            {
                throw;
            }
            return retValue;
        }

        public async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> PrepareOrgMaster()
        {
            try
            {
                var skuSql = new StringBuilder(@"SELECT      id AS Id,
                                                uid AS UID,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                code AS Code,
                                                '[' || code::text || ']' || name AS Name,
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
                                                org");
                var skuParameters = new Dictionary<string, object?>();
                Type skuType = _serviceProvider.GetRequiredService<Winit.Modules.Org.Model.Interfaces.IOrg>().GetType();
                List<Winit.Modules.Org.Model.Interfaces.IOrg> orgList = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(skuSql.ToString(), skuParameters, skuType);

                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> ViewFranchiseeWarehouse(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (
                                            SELECT 
                                                o.id AS Id,
                                                o.code AS warehousecode,
                                                o.uid AS warehouseuid,
                                                o.name AS warehousename,
                                                p.code AS franchisecode,
                                                p.name AS franchisename,
                                                '[' || ot.uid::text || ']' || ot.name AS orgtypeuid,
                                                ot.uid AS orgtype,
                                                o.modified_time AS orgmodifiedtime
                                            FROM 
                                                org o
                                            INNER JOIN 
                                                org p ON p.uid = o.parent_uid
                                            JOIN 
                                                org_type ot ON o.org_type_uid = ot.uid
                                            WHERE 
                                                o.org_type_uid IN ('FRWH', 'WOWH') AND o.parent_uid = @FranchiseeOrgUID) 
                                                 AS SubQuery ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@" SELECT COUNT(*) AS TotalCount
                                                            FROM 
                                                                (
                                                                    SELECT 
                                                o.id AS Id,
                                                o.code AS warehousecode,
                                                o.uid AS warehouseuid,
                                                o.name AS warehousename,
                                                p.code AS franchisecode,
                                                p.name AS franchisename,
                                                '[' || ot.uid::text || ']' || ot.name AS orgtypeuid,
                                                ot.uid AS orgtype,
                                                o.modified_time AS orgmodifiedtime
                                            FROM 
                                                org o
                                            INNER JOIN 
                                                org p ON p.uid = o.parent_uid
                                            JOIN 
                                                org_type ot ON o.org_type_uid = ot.uid
                                            WHERE 
                                                o.org_type_uid IN ('FRWH', 'WOWH') AND o.parent_uid = @FranchiseeOrgUID) 
                                                 AS SubQuery ");
                }
                var parameters = new Dictionary<string, object?>() {
                    {"FranchiseeOrgUID", FranchiseeOrgUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" where ");
                    AppendFilterCriteria<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append("  ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IWarehouseItemView>().GetType();

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
        public async Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> ViewFranchiseeWarehouseByUID(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID},

            }; var sql = @"SELECT 
                                o.uid AS UID,
                                o.code AS WarehouseCode,
                                o.name AS WarehouseName,
                                o1.uid AS FranchiseCode,
                                o1.name AS FranchiseName,
                                o.org_type_uid AS OrgTypeUID,
                                ad.uid AS AddressUID,
                                ad.name AS AddressName,
                                ad.line1 AS AddressLine1,
                                ad.line2 AS AddressLine2,
                                ad.line3 AS AddressLine3,
                                ad.landmark AS Landmark,
                                ad.area AS Area,
                                ad.zip_code AS ZipCode,
                                ad.city AS City,
                                ad.region_code AS RegionCode,
                                '[' || ot.uid::text || ']' || ot.name AS WarehouseType
                            FROM
                                org o
                            JOIN
                                org o1 ON o.parent_uid = o1.uid
                            JOIN
                                address ad ON o.uid = ad.linked_item_uid
                            JOIN
                                org_type ot ON ot.uid = o.org_type_uid
                            WHERE
                                o.uid = @uid  AND o.org_type_uid IN ('FRWH', 'WOWH');
";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEditWareHouseItemView>().GetType();

            Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView editWareHouseItemDetails = await ExecuteSingleAsync<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView>(sql, parameters, type);
            return editWareHouseItemDetails;
        }

        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouseStockDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID, string WarehouseUID, string StockType)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT
                                s.uid AS SubqueryUid,
                                s.code AS SkuCode,
                                s.name AS SkuName,
                                wa.qty AS Qty,
                                wa.uom AS Uom,
                                wa.stock_type AS StockType,
                                su.multiplier AS OuterMultiplier,
                                CAST(wa.qty / su.multiplier AS INT) AS OuterQty,
                                wa.qty % su.multiplier AS EaQty,
                                SUM(wa.qty) AS TotalEaQty
                            FROM
                                WH_Stock_Available wa
                            INNER JOIN
                                SKU s ON s.uid = wa.sku_uid
                            INNER JOIN
                                sku_uom su ON su.sku_uid = s.uid AND su.is_outer_uom = TRUE
                            WHERE
                                wa.warehouse_uid = @WarehouseUID
                                AND wa.stock_type = @StockType 
                            GROUP BY
                                s.uid, s.code, s.name, wa.qty, wa.uom, wa.stock_type, su.multiplier)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt
                                                FROM (
                                                    SELECT
                                                        S.uid AS SubqueryUid,
                                                        S.code AS SkuCode,
                                                        S.name AS SkuName,
                                                        WA.qty,
                                                        WA.uom,
                                                        WA.stock_type AS StockType,
                                                        SU.multiplier AS OuterMultiplier,
                                                        CAST(WA.qty / SU.multiplier AS INT) AS OuterQty,
                                                        WA.qty % SU.multiplier AS EaQty,
                                                        SUM(WA.qty) AS TotalEaQty
                                                    FROM
                                                        WH_Stock_Available WA
                                                    INNER JOIN
                                                        SKU S ON S.uid = WA.sku_uid
                                                    INNER JOIN
                                                        SKU_UOM SU ON SU.sku_uid = S.uid AND SU.is_outer_uom = TRUE
                                                    WHERE
                                                        WA.warehouse_uid =@WarehouseUID
                                                        AND WA.stock_type =@StockType
                                                    GROUP BY
                                                        S.uid, S.code, S.name, WA.qty, WA.uom, WA.stock_type, SU.multiplier
                                                ) AS subquery");
                }
                var parameters = new Dictionary<string, object?>()
                {
                    {"FranchiseeOrgUID",FranchiseeOrgUID },
                    {"WarehouseUID",WarehouseUID},
                    {"StockType",StockType }
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IWarehouseStockItemView>().GetType();

                IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> WarehouseStockItemViewDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> pagedResponse = new PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>
                {
                    PagedData = WarehouseStockItemViewDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateViewFranchiseeWarehouse(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView createWareHouseItemView)
        {
            int retVal = -1;
            try
            {
                var sql = @"insert into org (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,code,
                            name,org_type_uid,is_active,parent_uid) values(@uid,@created_by,@created_time,@modified_by,
                             @modified_time,@server_add_time, @server_modified_time,@code,@name,@org_type_uid,@is_active,@parent_uid);";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                        {
                            {"uid", createWareHouseItemView.UID},
                            {"created_by", createWareHouseItemView.CreatedBy},
                            {"modified_by", createWareHouseItemView.ModifiedBy},
                            {"created_time", DateTime.Now},
                            {"modified_time", DateTime.Now},
                            {"server_add_time", DateTime.Now},
                            {"server_modified_time", DateTime.Now},
                            {"code", createWareHouseItemView.WarehouseCode},
                            {"parent_uid", createWareHouseItemView.ParentUID},
                            {"name", createWareHouseItemView.WarehouseName},
                            {"org_type_uid", createWareHouseItemView.OrgTypeUID},
                            {"is_active", true}
                        };

                var Addresssql = @"INSERT INTO address(uid,created_by,created_time,modified_by,modified_time,server_add_time,
                     server_modified_time,name,line1,line2,line3,
                     landmark,area,zip_code,city,
                     region_code,linked_item_uid) VALUES(@uid,@created_by,@created_time,@modified_by,@modified_time,@server_add_time,@server_modified_time,@name,@line1,@line2,@line3,
                     @landmark,@area,@zip_code,@city,@region_code,@linked_item_uid);";

                Dictionary<string, object?> Addressparameters = new Dictionary<string, object?>
                        {
                            {"uid", createWareHouseItemView.AddressUID},
                            {"created_by", createWareHouseItemView.CreatedBy},
                            {"modified_by", createWareHouseItemView.ModifiedBy},
                            {"created_time", DateTime.Now},
                            {"modified_time", DateTime.Now},
                            {"server_add_time", DateTime.Now},
                            {"server_modified_time", DateTime.Now},
                            {"name", createWareHouseItemView.AddressName},
                            {"line1", createWareHouseItemView.AddressLine1},
                            {"line2", createWareHouseItemView.AddressLine2},
                            {"line3", createWareHouseItemView.AddressLine3},
                            {"landmark", createWareHouseItemView.Landmark},
                            {"area", createWareHouseItemView.Area},
                            {"zip_code", createWareHouseItemView.ZipCode},
                            {"city", createWareHouseItemView.City},
                            {"region_code", createWareHouseItemView.RegionCode},
                            {"linked_item_uid", createWareHouseItemView.LinkedItemUID},
                        };

                retVal = await ExecuteNonQueryAsync(sql, parameters);
                retVal = await ExecuteNonQueryAsync(Addresssql, Addressparameters);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateViewFranchiseeWarehouse(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView updateWareHouseItemView)
        {
            int retVal = -1;
            try
            {
                var warehouseSQL = @"UPDATE org SET modified_by = @modified_by, name = @name, is_active = @is_active, 
                     modified_time = @modified_time, server_modified_time = @server_modified_time ,code = @code WHERE uid = @uid";

                Dictionary<string, object?> warehouseParameters = new Dictionary<string, object?>
                {
                    {"uid", updateWareHouseItemView.UID},
                    {"modified_by", updateWareHouseItemView.ModifiedBy},
                    {"modified_time", DateTime.Now},
                    {"server_modified_time", DateTime.Now},
                    {"code", updateWareHouseItemView.WarehouseCode},
                    {"name", updateWareHouseItemView.WarehouseName},
                    {"is_active", true},
                };

                var addressSQL = @"UPDATE address SET   
                   modified_by = @modified_by,
                   modified_time = @modified_time,
                   server_modified_time = @server_modified_time,
                   name = @name,
                   line1 = @line1,
                   line2 = @line2,
                   line3 = @line3,
                   landmark = @landmark,
                   area = @area,
                   zip_code = @zip_code,
                   city = @city,
                   region_code = @region_code
                   WHERE
                   uid = @uid";

                Dictionary<string, object?> addressParameters = new Dictionary<string, object?>
                {
                    {"uid", updateWareHouseItemView.AddressUID},
                    {"modified_by", updateWareHouseItemView.ModifiedBy},
                    {"modified_time", DateTime.Now},
                    {"server_modified_time", DateTime.Now},
                    {"name", updateWareHouseItemView.AddressName},
                    {"line1", updateWareHouseItemView.AddressLine1},
                    {"line2", updateWareHouseItemView.AddressLine2},
                    {"line3", updateWareHouseItemView.AddressLine3},
                    {"landmark", updateWareHouseItemView.Landmark},
                    {"area", updateWareHouseItemView.Area},
                    {"zip_code", updateWareHouseItemView.ZipCode},
                    {"city", updateWareHouseItemView.City},
                    {"region_code", updateWareHouseItemView.RegionCode},
                };

                retVal = await ExecuteNonQueryAsync(warehouseSQL, warehouseParameters);
                retVal = await ExecuteNonQueryAsync(addressSQL, addressParameters);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteViewFranchiseeWarehouse(string UID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "UID", UID }
        };

                var sql = @"DELETE FROM org WHERE uid = @UID;
                            DELETE FROM address WHERE linked_Item_uid = @UID;";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState == "23503")
                {
                    return -1;
                }
                throw new Exception("An error occurred while deleting the FranchiseeWarehouse.", ex);
            }
        }

        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetOrgTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                    id as Id,
                    uid as UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    name as Name,
                    parent_uid AS ParentUID,
                    is_company_org AS IsCompanyOrg,
                    is_franchisee_org AS IsFranchiseeOrg,
                    is_wh AS IsWH,
                    show_in_ui AS ShowInUI,
                    show_in_template AS ShowInTemplate,
                    '[' || uid::text || ']' || name AS WarehouseType 
                FROM 
                    Org_Type
                  ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM Org_Type");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Org.Model.Interfaces.IOrgType>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IOrgType>().GetType();

                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrgType> OrgTypeDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrgType>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType> pagedResponse = new PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType>
                {
                    PagedData = OrgTypeDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID,
            string? parentUID = null, string? branchUID = null)
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
                                        org WHERE Org_Type_UID= @OrgTypeUID");
            if (!string.IsNullOrEmpty(parentUID))
            {
                sql.Append(" AND parent_uid = @ParentUID;");
            }
            if (!string.IsNullOrEmpty(branchUID))
            {
                if (OrgTypeUID == OrgTypeConst.WH)
                {
                    sql.Append(@" AND uid IN (SELECT DISTINCT SO.warehouse_uid FROM sales_office SO 
                                WHERE SO.branch_uid = @BranchUID)");
                }
                else if (OrgTypeUID == OrgTypeConst.SWH)
                {
                    sql.Append(@" AND uid IN (SELECT DISTINCT SOSW.sub_warehouse_uid 
                                FROM sales_office SO
                                INNER JOIN sales_office_sub_warehouse SOSW ON SOSW.sales_office_uid = SO.uid 
                                AND branch_uid = @BranchUID)");
                }
            }
            IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> orgDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql.ToString(), parameters);
            return orgDetails;
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IFOCStockItem>> GetFOCStockItemDetails(string StockType)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"StockType",  StockType}
            };
            var sql = @"SELECT 
                    reserved_ou_qty AS ReservedOUQty,
                    reserved_bu_qty AS ReservedBUQty
                FROM 
                    WH_Stock_Available
                WHERE 
                    stock_type = 'FOCReserved';
";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IFOCStockItem>().GetType();

            IEnumerable<Winit.Modules.Org.Model.Interfaces.IFOCStockItem> orgDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IFOCStockItem>(sql, parameters, type);
            return orgDetails;
        }

        public Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetVanStockItems(string warehouseUID, string orgUID, StockType? stockType)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetOrgHierarchyParentUIDsByOrgUID(List<string> orgs)
        {
            return null;
            try
            {
                string sql = "SELECT parent_uid FROM org_hierarchy WHERE org_uid = ANY(@OrgUIDs)";
                var parameters = new
                {
                    OrgUIDs = orgs.ToArray() // PostgreSQL expects array, not List
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
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
        {

            Dictionary<string, object?> parameters = new Dictionary<string, object?>
     {
         {"orgUID",  orgUID},
         {"ParentUID",  storeUID}
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
                         org ");




            // Execute the query with SQLite-compatible parameters
            IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> orgDetails = await ExecuteQueryAsync<Winit.Modules.Org.Model.Interfaces.IOrg>(sql.ToString(), parameters);
            return orgDetails;
        }

        //Task<PagedResponse<IOrg>> IOrgDL.GetOrgDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IOrg> IOrgDL.GetOrgByUID(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.CreateOrg(IOrg createOrg)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.UpdateOrg(IOrg updateOrg)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.DeleteOrg(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.InsertOrgHierarchy()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<IOrg>> IOrgDL.PrepareOrgMaster()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<PagedResponse<IWarehouseItemView>> IOrgDL.ViewFranchiseeWarehouse(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEditWareHouseItemView> IOrgDL.ViewFranchiseeWarehouseByUID(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<PagedResponse<IWarehouseStockItemView>> IOrgDL.GetWarehouseStockDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID, string WarehouseUID, string StockType)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.CreateViewFranchiseeWarehouse(IEditWareHouseItemView createWareHouseItemView)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.UpdateViewFranchiseeWarehouse(IEditWareHouseItemView updateWareHouseItemView)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> IOrgDL.DeleteViewFranchiseeWarehouse(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<PagedResponse<IOrgType>> IOrgDL.GetOrgTypeDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<IOrg>> IOrgDL.GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID, string? branchUID)
        //{
        //    throw new NotImplementedException();
        //}

        Task<IEnumerable<IOrg>> IOrgDL.GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID)
        {
            throw new NotImplementedException();
        }

        //Task<IEnumerable<IFOCStockItem>> IOrgDL.GetFOCStockItemDetails(string StockType)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<IWarehouseStockItemView>> IOrgDL.GetVanStockItems(string warehouseUID, string orgUID, StockType? stockType)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<string>> IOrgDL.GetOrgHierarchyParentUIDsByOrgUID(List<string> orgs)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<ISelectionItem>> IOrgDL.GetProductOrgSelectionItems()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<ISelectionItem>> IOrgDL.GetProductDivisionSelectionItems()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<IOrg>> IOrgDL.GetDivisions(string storeUID)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<PagedResponse<IWareHouseStock>> IOrgDL.GetAllWareHouseStock(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<ISKUGroup>> IOrgDL.GetSkuGroupBySkuGroupTypeUID(string skuGroupTypeUid)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<IOrg>> IOrgDL.GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
