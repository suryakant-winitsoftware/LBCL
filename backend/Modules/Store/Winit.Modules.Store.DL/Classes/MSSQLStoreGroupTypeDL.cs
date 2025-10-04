using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLStoreGroupTypeDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreGroupTypeDL
    {
        public MSSQLStoreGroupTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>> SelectAllStoreGroupType
          (List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"SELECT * FROM (SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                          modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                          server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, org_uid AS OrgUID, 
                                          distribution_channel_uid AS DistributionChannelUID, name AS Name, parent_uid AS ParentUID, 
                                          level_no AS LevelNo, code AS Code
	                                      FROM store_group_type) AS SUBQUERY ");
                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(*) FROM (SELECT id AS Id, uid AS UID, created_by AS CreatedBy, 
                                                  created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                                                  server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid, 
                                                  org_uid AS OrgUid,  distribution_channel_uid AS DistributionChannelUID, name AS Name, parent_uid AS ParentUID, 
                                                  level_no AS LevelNo, code AS Code FROM store_group_type) AS SUBQUERY ");
                }
                Dictionary<string, object> parameters = new();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    _ = sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>(filterCriterias, sbFilterCriteria, parameters); ;

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
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Model.Interfaces.IStoreGroupType> storeGroupTypes = await ExecuteQueryAsync<Model.Interfaces.IStoreGroupType>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType> pagedResponse = new()
                {
                    PagedData = storeGroupTypes,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupType?> SelectStoreGroupTypeByUID(string UID)
        {

            Dictionary<string, object?> parameters = new()
        {
            {"StoreGroupTypeUID",  UID}
        };

            string sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                           server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, org_uid AS OrgUID, 
                           distribution_channel_uid AS DistributionChannelUID, name AS Name, parent_uid AS ParentUID, level_no AS LevelNo, code AS Code
	                       FROM store_group_type WHERE uid = @StoreGroupTypeUID";
            Model.Interfaces.IStoreGroupType? StoreGroupTypeList = await ExecuteSingleAsync<Model.Interfaces.IStoreGroupType>(sql, parameters);
            return StoreGroupTypeList;
        }
        public async Task<int> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            try
            {
                string sql = @"INSERT INTO store_group_type (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                               server_modified_time, company_uid, org_uid, distribution_channel_uid, name, parent_uid, level_no, code)
                               VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@CompanyUID,@OrgUID,
                               @DistributionChannelUID,@Name,@ParentUID,@LevelNo,@Code)";
                return await ExecuteNonQueryAsync(sql, storeGroupType);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            try
            {
                string sql = @"UPDATE store_group_type SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime,
                        company_uid = @CompanyUID, org_uid = @OrgUID, name = @Name, parent_uid = @ParentUID, level_no = @LevelNo ,
                          code = @Code  WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, storeGroupType);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreGroupType(string UID)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
            {
            {"UID",UID}
            };
                string sql = @"DELETE  FROM store_group_type WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
           

        }
    }
}








