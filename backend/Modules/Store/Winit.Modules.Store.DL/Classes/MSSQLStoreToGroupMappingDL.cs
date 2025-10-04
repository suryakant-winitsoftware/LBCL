using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MSSQLStoreToGroupMappingDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreToGroupMappingDL
    {
        public MSSQLStoreToGroupMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> CreateStoreToGroupMapping(Model.Interfaces.IStoreToGroupMapping storeToGroupMapping)
        {
            try
            {

                var sql = @"INSERT INTO store_to_group_mapping (uid, store_uid, created_by, modified_by, store_group_uid, created_time, modified_time, 
                          server_add_time, server_modified_time)
                         VALUES ( @UID,@StoreUID,@CreatedBy,@ModifiedBy,@StoreGroupUID, @CreatedTime, 
                              @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
                return await ExecuteNonQueryAsync(sql, storeToGroupMapping);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping>> SelectAllStoreToGroupMapping(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * From (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                              store_uid, store_group_uid FROM store_to_group_mapping)as Subquery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                                               server_modified_time, 
                                              store_uid, store_group_uid FROM store_to_group_mapping)as Subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping>(filterCriterias, sbFilterCriteria, parameters); ;

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
                IEnumerable<Model.Interfaces.IStoreToGroupMapping> StoreToGroupMappings = await ExecuteQueryAsync<Model.Interfaces.IStoreToGroupMapping>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping>
                {
                    PagedData = StoreToGroupMappings,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping> SelectAllStoreToGroupMappingByStoreUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, store_uid, store_group_uid
                        FROM store_to_group_mapping WHERE uid = @UID";
            return await ExecuteSingleAsync<Model.Interfaces.IStoreToGroupMapping>(sql, parameters);
             
        }
        public async Task<int> UpdateStoreToGroupMapping(Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping StoreToGroupMapping)
        {
            try
            {
                var sql = @"UPDATE store_to_group_mapping
                    SET store_uid = @StoreUID, 
                        modified_by = @ModifiedBy, 
                        store_group_uid = @StoreGroupUID, 
                        modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime 
                    WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, StoreToGroupMapping);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreToGroupMapping(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM store_to_group_mapping WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData>> SelectAllChannelMasterData()
        {
            try
            {
                var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        ss AS Ss,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        l1 AS L1,
                        l2 AS L2,
                        l3 AS L3,
                        l4 AS L4,
                        l5 AS L5,
                        l6 AS L6,
                        l7 AS L7,
                        l8 AS L8,
                        l9 AS L9,
                        l10 AS L10,
                        l11 AS L11,
                        l12 AS L12,
                        l13 AS L13,
                        l14 AS L14,
                        l15 AS L15,
                        l16 AS L16,
                        l17 AS L17,
                        l18 AS L18,
                        l19 AS L19,
                        l20 AS L20,
                        primary_uid AS PrimaryUid,
                        json_data AS JsonData,
                        label AS Label,
                        primary_type AS PrimaryType
                    FROM 
                        store_group_data;";
                IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData> StoreToGroupMappingList = await ExecuteQueryAsync<Model.Interfaces.IStoreGroupData>(sql);
                return StoreToGroupMappingList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}








