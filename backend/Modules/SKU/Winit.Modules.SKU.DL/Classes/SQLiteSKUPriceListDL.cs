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
    public class SQLiteSKUPriceListDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager,ISKUPriceListDL
    {
        public SQLiteSKUPriceListDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>> SelectAllSKUPriceListDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy," +
                    "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID," +
                    "code AS Code, name AS Name, type AS Type, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, priority AS Priority," +
                    "selection_group AS SelectionGroup, selection_type AS SelectionType, selection_uid AS SelectionUID, is_active AS IsActive, status AS Status," +
                    "valid_from AS ValidFrom, valid_upto AS ValidUpto FROM sku_price_list");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM sku_price_list");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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
                //    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPriceList>().GetType();
                IEnumerable<Model.Interfaces.ISKUPriceList> sKUPriceLists = await ExecuteQueryAsync<Model.Interfaces.ISKUPriceList>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
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
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID,
                        code AS Code, name AS Name, type AS Type, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, priority AS Priority,
                        selection_group AS SelectionGroup, selection_type AS SelectionType, selection_uid AS SelectionUID, is_active AS IsActive, status AS Status,
                        valid_from AS ValidFrom, valid_upto AS ValidUpto FROM sku_price_list WHERE uid= @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPriceList>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>(sql, parameters, type);
            return sKUPriceDetails;
        }
        public async Task<int> CreateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"INSERT INTO sku_price_list (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,company_uid,code,name,
                            type,org_uid,distribution_channel_uid,priority,selection_group,selection_type,selection_uid,is_active,status,valid_from,valid_upto)VALUES    
                            (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@CompanyUID ,@Code ,@Name ,@Type ,@OrgUID ,@DistributionChannelUID ,@Priority ,
                             @SelectionGroup ,@SelectionType ,@SelectionUID ,@IsActive ,@Status ,@ValidFrom);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPriceList.UID},
                   {"CreatedBy",sKUPriceList.CreatedBy},
                   {"CreatedTime",sKUPriceList.CreatedTime},
                   {"ModifiedBy",sKUPriceList.ModifiedBy},
                   {"ModifiedTime",sKUPriceList.ModifiedTime},
                   {"ServerAddTime",sKUPriceList.ServerAddTime},
                   {"ServerModifiedTime",sKUPriceList.ServerModifiedTime},
                   {"CompanyUID",sKUPriceList.CompanyUID},
                   {"Code",sKUPriceList.Code},
                   {"Name",sKUPriceList.Name},
                   {"Type",sKUPriceList.Type},
                   {"OrgUID",sKUPriceList.OrgUID},
                   {"DistributionChannelUID",sKUPriceList.DistributionChannelUID},
                   {"Priority",sKUPriceList.Priority},
                   {"SelectionGroup",sKUPriceList.SelectionGroup},
                   {"SelectionType",sKUPriceList.SelectionType},
                   {"SelectionUID",sKUPriceList.SelectionUID},
                   {"IsActive",sKUPriceList.IsActive},
                   {"Status",sKUPriceList.Status},
                   {"ValidFrom",sKUPriceList.ValidFrom},
                   {"ValidUpto",sKUPriceList.ValidUpto}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList)
        {
            try
            {
                var sql = @"UPDATE sku_price_list SET 
                           ,modified_by=@ModifiedBy
                           ,modified_time=@ModifiedTime
                           ,server_modified_time=@ServerModifiedTime
                           ,company_uid=@CompanyUID
                           ,code=@Code
                           ,name=@Name
                           ,type=@Type
                           ,org_uid	=@OrgUID
                           ,distribution_channel_uid =@DistributionChannelUID
                           ,priority=@Priority
                           ,selection_group=@SelectionGroup
                           ,selection_type=@SelectionType
                           ,selection_uid=@SelectionUID
                           ,is_active=@IsActive
                           ,status=@Status
                           ,valid_from=@ValidFrom
                           ,valid_upto=@ValidUpto
                             WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPriceList.UID},
                    {"ModifiedBy",sKUPriceList.ModifiedBy},
                    {"ModifiedTime",sKUPriceList.ModifiedTime},
                    {"ServerModifiedTime",sKUPriceList.ServerModifiedTime},
                    {"CompanyUID",sKUPriceList.CompanyUID},
                    {"Code",sKUPriceList.Code},
                    {"Name",sKUPriceList.Name},
                    {"Type",sKUPriceList.Type},
                    {"OrgUID",sKUPriceList.OrgUID},
                    {"DistributionChannelUID",sKUPriceList.DistributionChannelUID},
                    {"Priority",sKUPriceList.Priority},
                    {"SelectionGroup",sKUPriceList.SelectionGroup},
                    {"SelectionType",sKUPriceList.SelectionType},
                    {"SelectionUID",sKUPriceList.SelectionUID},
                    {"IsActive",sKUPriceList.IsActive},
                    {"Status",sKUPriceList.Status},
                    {"ValidFrom",sKUPriceList.ValidFrom},
                    {"ValidUpto",sKUPriceList.ValidUpto},

                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUPriceList(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM sku_price_list WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<IBuyPrice>> PopulateBuyPrice(string orgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT spl.selection_uid AS FranchiseeOrgUID,
                    sp.sku_code AS SKUCode,
                    sp.price AS Price
                    FROM sku_price_list spl
                    INNER JOIN sku_price sp ON sp.sku_price_list_uid = spl.uid
                    AND spl.type = 'FR'
                    AND spl.selection_uid = @OrgUID
                    INNER JOIN sku s ON s.uid = sp.sku_uuid 
                    AND s.base_uom = sp.uom
                    WHERE sp.is_active = true
                    AND Date('now', 'localtime') >= Date(sp.valid_from, 'localtime')
                    AND Date('now', 'localtime') <= Date(sp.valid_upto, 'localtime');");
                var parameters = new Dictionary<string, object>()
                {
                    {"OrgUID",orgUID }
                };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBuyPrice>().GetType();
                IEnumerable<Model.Interfaces.IBuyPrice> buyPriceList = await ExecuteQueryAsync<Model.Interfaces.IBuyPrice>(sql.ToString(), parameters, type);
                return buyPriceList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
