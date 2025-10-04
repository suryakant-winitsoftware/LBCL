using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSKUPriceListDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUPriceListDL
    {
        public MSSQLSKUPriceListDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>> SelectAllSKUPriceListDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUID,
                    code AS Code,
                    name AS Name,
                    type AS Type,
                    org_uid AS OrgUID,
                    distribution_channel_uid AS DistributionChannelUID,
                    priority AS Priority,
                    selection_group AS SelectionGroup,
                    selection_type AS SelectionType,
                    selection_uid AS SelectionUID,
                    is_active AS IsActive,
                    status AS Status,
                    valid_from AS ValidFrom,
                    valid_upto AS ValidUpto
                FROM sku_price_list) as subQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUID,
                    code AS Code,
                    name AS Name,
                    type AS Type,
                    org_uid AS OrgUID,
                    distribution_channel_uid AS DistributionChannelUID,
                    priority AS Priority,
                    selection_group AS SelectionGroup,
                    selection_type AS SelectionType,
                    selection_uid AS SelectionUID,
                    is_active AS IsActive,
                    status AS Status,
                    valid_from AS ValidFrom,
                    valid_upto AS ValidUpto
                FROM sku_price_list) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Model.Interfaces.ISKUPriceList> sKUPriceLists = await ExecuteQueryAsync<Model.Interfaces.ISKUPriceList>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>
                {
                    PagedData = sKUPriceLists,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList> SelectSKUPriceListByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            company_uid AS CompanyUID,
                            code AS Code,
                            name AS Name,
                            type AS Type,
                            org_uid AS OrgUID,
                            distribution_channel_uid AS DistributionChannelUID,
                            priority AS Priority,
                            selection_group AS SelectionGroup,
                            selection_type AS SelectionType,
                            selection_uid AS SelectionUID,
                            is_active AS IsActive,
                            status AS Status,
                            valid_from AS ValidFrom,
                            valid_upto AS ValidUpto 
                    FROM sku_price_list WHERE uid= @UID";
            return await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>(sql, parameters);
             
        }
        public async Task<int> CreateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {

                var sql = @"INSERT INTO sku_price_list (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            company_uid, code, name, type, org_uid, distribution_channel_uid, priority, selection_group, selection_type, selection_uid, 
                            is_active, status, valid_from, valid_upto) VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime,
                            @ServerModifiedTime ,@CompanyUID ,@Code ,@Name ,@Type ,@OrgUID ,@DistributionChannelUID ,@Priority, @SelectionGroup ,@SelectionType,
                            @SelectionUID ,@IsActive ,@Status ,@ValidFrom,@ValidUpto);";
                retVal= await ExecuteNonQueryAsync(sql, connection,transaction, sKUPriceList);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> UpdateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE sku_price_list
                SET modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    company_uid = @CompanyUID,
                    code = @Code,
                    name = @Name,
                    type = @Type,
                    org_uid = @OrgUID,
                    distribution_channel_uid = @DistributionChannelUID,
                    priority = @Priority,
                    selection_group = @SelectionGroup,
                    selection_type = @SelectionType,
                    selection_uid = @SelectionUID,
                    is_active = @IsActive,
                    status = @Status,
                    valid_from = @ValidFrom,
                    valid_upto = @ValidUpto
                WHERE uid = @UID;";
                retVal= await ExecuteNonQueryAsync(sql, sKUPriceList);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> DeleteSKUPriceList(string UID)
        {
            int count = 0;
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"SKUPriceListUID" , UID}
            };
            var sqlSKUPrice = @"DELETE FROM sku_price
                WHERE sku_price_list_uid = @SKUPriceListUID;";
            count = await ExecuteNonQueryAsync(sqlSKUPrice, parameters);
            var sqlSKUPriceList = @"DELETE FROM sku_price_list
                WHERE uid = @SKUPriceListUID;";
            if (count > 0)
            {
                return await ExecuteNonQueryAsync(sqlSKUPriceList, parameters);
            }
            return 0;

        }

        public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.IBuyPrice>> PopulateBuyPrice(string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT spl.selection_uid AS FranchiseeOrgUID,
                                             sp.sku_code AS SKUCode,sp.price AS Price
                                             FROM sku_price_list spl INNER JOIN sku_price sp ON sp.sku_price_list_uid = spl.uid
                                             AND spl.type = 'FR' AND spl.selection_uid = @OrgUID
                                            INNER JOIN sku s ON s.uid = sp.sku_uid
                                            AND s.base_uom = sp.uom WHERE sp.is_active = 1
                                            AND GETDATE() >= sp.valid_from
                                            AND GETDATE() <= CAST(sp.valid_upto AS DATE);");
                var parameters = new Dictionary<string, object>()
                {
                    {"OrgUID",OrgUID }
                };
                IEnumerable<Model.Interfaces.IBuyPrice> buyPriceList = await ExecuteQueryAsync<Model.Interfaces.IBuyPrice>(sql.ToString(), parameters);
                return buyPriceList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateStandardPriceList(string orgUID)
        {
            try
            {

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"OrgUID" , orgUID}
            };
                StringBuilder sql = new StringBuilder(@"INSERT INTO sku_price_list (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, company_uid, code, name, type, org_uid, distribution_channel_uid, priority,
                            selection_group, selection_type, selection_uid, is_active, status, valid_from, valid_upto)
                                    SELECT 
                                    O.uid AS uid, 
                                    O.created_by, 
                                    O.created_time, 
                                    O.modified_by, 
                                    O.modified_time, 
                                    O.server_add_time,
                                    O.server_modified_time, 
                                    O.company_uid, 
                                    O.code, 
                                    O.code + ' Price List' AS name, 
                                    O.org_type_uid AS type, 
                                    O.parent_uid AS org_uid, 
                                    NULL AS distribution_channel_uid, 
                                    99999 AS priority,
                                    'Org' AS selection_group, 
                                    'Org' AS selection_type, 
                                    O.uid AS selection_uid, 
                                    1 AS is_active,
                                    'Published' AS status, 
                                    CAST(O.created_time AS DATE) AS valid_from,
                                    '2099-12-31' AS valid_upto
                                FROM 
                                    Org O
                                LEFT JOIN 
                                    sku_price_list SPL ON SPL.type = 'FR' 
                                    AND SPL.org_uid = O.parent_uid 
                                    AND SPL.selection_group = 'Org' 
                                    AND SPL.selection_type = 'Org' 
                                    AND SPL.selection_uid = O.uid 
                                WHERE 
                                    O.org_type_uid = 'FR' 
                                    AND O.uid = @OrgUID
                                    AND SPL.id IS NULL;");
                return await ExecuteNonQueryAsync(sql.ToString(), parameters);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
