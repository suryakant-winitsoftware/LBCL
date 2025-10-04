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
    public class SQLiteStoreToGroupMappingDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IStoreToGroupMappingDL
    {
        public SQLiteStoreToGroupMappingDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<int> CreateStoreToGroupMapping(Model.Interfaces.IStoreToGroupMapping storeToGroupMapping)
        {
             try
             {
                var sql = @"INSERT INTO store_to_group_mapping (uid,store_uid,created_by,modified_by,store_group_uid,created_time, modified_time, 
                          server_add_time, server_modified_time) VALUES ( @UID,@StoreUID,@CreatedBy,@ModifiedBy,@StoreGroupUID, @CreatedTime, 
                          @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                     {"UID", storeToGroupMapping.UID},
                     {"StoreUID", storeToGroupMapping.StoreUID},
                     {"CreatedBy", storeToGroupMapping.CreatedBy},
                     {"ModifiedBy", storeToGroupMapping.ModifiedBy},
                     {"StoreGroupUID", storeToGroupMapping.StoreGroupUID},
                     {"CreatedTime", storeToGroupMapping.CreatedTime},
                     {"ModifiedTime", storeToGroupMapping.ModifiedTime},
                     {"ServerAddTime", storeToGroupMapping.ServerAddTime},
                     {"ServerModifiedTime", storeToGroupMapping.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
             }
             catch (Exception)
             {
                throw;
             }
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreToGroupMapping>> SelectAllStoreToGroupMapping(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy," +
                    "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID," +
                    "store_group_uid AS StoreGroupUID FROM store_to_group_mapping;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store_to_group_mapping");
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

                if (pageNumber > 0 && pageSize > 0)
                {                  
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");                   
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreToGroupMapping>().GetType();
                IEnumerable<Model.Interfaces.IStoreToGroupMapping> StoreToGroupMappings = await ExecuteQueryAsync<Model.Interfaces.IStoreToGroupMapping>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
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
        public async Task<Model.Interfaces.IStoreToGroupMapping> SelectAllStoreToGroupMappingByStoreUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID,
                    store_group_uid AS StoreGroupUID FROM store_to_group_mapping WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreToGroupMapping>().GetType();
            Model.Interfaces.IStoreToGroupMapping StoreToGroupMappingList = await ExecuteSingleAsync<Model.Interfaces.IStoreToGroupMapping>(sql, parameters, type);
            return StoreToGroupMappingList;
        }
        public async Task<int> UpdateStoreToGroupMapping(Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping StoreToGroupMapping)
        {
            try
            {
                var sql = "UPDATE store_to_group_mapping SET store_uid=@StoreUID, modified_by=@ModifiedBy, store_group_uid=@StoreGroupUID, modified_time=@ModifiedTime," +
                    "server_modified_time=@ServerModifiedTime WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                     {"UID", StoreToGroupMapping.UID},
                     {"StoreUID", StoreToGroupMapping.StoreUID},
                    // {"CreatedBy", StoreToGroupMapping.CreatedBy},
                     {"ModifiedBy", StoreToGroupMapping.ModifiedBy},
                     {"StoreGroupUID", StoreToGroupMapping.StoreGroupUID},
                     //{"CreatedTime", StoreToGroupMapping.CreatedTime},
                     {"ModifiedTime", StoreToGroupMapping.ModifiedTime},
                    // {"ServerAddTime", StoreToGroupMapping.ServerAddTime},
                     {"ServerModifiedTime", StoreToGroupMapping.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw ;
            }
        }
        public async Task<int> DeleteStoreToGroupMapping(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = "DELETE FROM store_to_group_mapping WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData>> SelectAllChannelMasterData()
        {
            throw new NotImplementedException();
        }
    }
}








