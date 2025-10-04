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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class SQLiteStoreGroupDL : Base.DL.DBManager.SqliteDBManager , Interfaces.IStoreGroupDL
    {
        public SQLiteStoreGroupDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreGroup>> SelectAllStoreGroup(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy," +
                    "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_group_type_uid AS StoreGroupTypeUID," +
                    "code AS Code, name AS Name, parent_uid AS ParentUID, item_level AS ItemLevel FROM store_group;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store_group");
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreGroup>().GetType();
                IEnumerable<Model.Interfaces.IStoreGroup> StoreGroups = await ExecuteQueryAsync<Model.Interfaces.IStoreGroup>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroup> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroup>
                {
                    PagedData = StoreGroups,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Model.Interfaces.IStoreGroup> SelectStoreGroupByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_group_type_uid AS StoreGroupTypeUID,
                    code AS Code, name AS Name, parent_uid AS ParentUID, item_level AS ItemLevel FROM store_group WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreGroup>().GetType();
            Model.Interfaces.IStoreGroup StoreGroupStoreList = await ExecuteSingleAsync<Model.Interfaces.IStoreGroup>(sql, parameters, type);
            return StoreGroupStoreList;
        }
        public async Task<int> CreateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
        {
            try
            {
                var sql = @"INSERT INTO store_group (uid, store_group_type_uid, code, parent_uid, created_by, modified_by, created_time, modified_time,
                    server_add_time, server_modified_time)VALUES (@UID,@StoreGroupTypeUID,@Code, @ParentUID, @CreatedBy, @ModifiedBy, 
                    @CreatedTime,@ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", storeGroup.UID},
                    {"StoreGroupTypeUID", storeGroup.StoreGroupTypeUID},
                    {"Code", storeGroup.Code},
                    {"Name", storeGroup.Name},
                    {"ParentUID", storeGroup.ParentUID},
                    {"CreatedBy", storeGroup.CreatedBy},
                    {"ModifiedBy", storeGroup.ModifiedBy},
                    {"CreatedTime", storeGroup.CreatedTime},
                    {"ModifiedTime", storeGroup.ModifiedTime},
                    {"ServerAddTime", storeGroup.ServerAddTime},
                    {"ServerModifiedTime", storeGroup.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertStoreGroupHierarchy(string type, string uid)
        {
            throw new NotImplementedException();
        }
        public async Task<int> UpdateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
        {
            try
            {
                var sql = @"UPDATE StoreGroup SET code = @Code,name=@Name,modified_by=@ModifiedBy, modified_time = @ModifiedTime,
                          server_modified_time = @ServerModifiedTime WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", storeGroup.UID},
                    {"Code", storeGroup.Code},
                    {"Name", storeGroup.Name},
                    {"ModifiedBy", storeGroup.ModifiedBy},
                    {"ModifiedTime", storeGroup.ModifiedTime},
                    {"ServerModifiedTime", storeGroup.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreGroup(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = "DELETE FROM store_group WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}








