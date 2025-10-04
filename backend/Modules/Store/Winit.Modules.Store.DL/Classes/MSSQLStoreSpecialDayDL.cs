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
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLStoreSpecialDayDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreSpecialDayDL
    {
        public MSSQLStoreSpecialDayDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
      
        public async Task<PagedResponse<Model.Interfaces.IStoreSpecialDay>> SelectAllStoreSpecialDay(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * from( select id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, 
                       modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                       server_modified_time AS ServerModifiedTime, store_uid AS StoreUid 
                       FROM store_special_day) as subquery;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ( select id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, 
                       modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                       server_modified_time AS ServerModifiedTime, store_uid AS StoreUid 
                       FROM store_special_day) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IStoreSpecialDay>(filterCriterias, sbFilterCriteria, parameters);

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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreSpecialDay>().GetType();
                IEnumerable<Model.Interfaces.IStoreSpecialDay> StoreSpecialDays = await ExecuteQueryAsync<Model.Interfaces.IStoreSpecialDay>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay>
                {
                    PagedData = StoreSpecialDays,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Model.Interfaces.IStoreSpecialDay> SelectAllStoreSpecialDayByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, store_uid AS StoreUid 
                        FROM store_special_day 
                        WHERE uid = @UID;";
            Model.Interfaces.IStoreSpecialDay StoreSpecialDayList = await ExecuteSingleAsync<Model.Interfaces.IStoreSpecialDay>(sql, parameters);
            return StoreSpecialDayList;
        }
        public async Task<int> CreateStoreSpecialDay(Model.Interfaces.IStoreSpecialDay storeSpecialDay)
        {
            try
            {
                var sql = @"INSERT INTO public.store_special_day (
                            id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, store_uid) 
                            VALUES (
                            @id, @uid, @created_by, @created_time, @modified_by, @modified_time, @server_add_time, @server_modified_time, @store_uid)";
                return await ExecuteNonQueryAsync(sql, storeSpecialDay);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreSpecialDay(Model.Interfaces.IStoreSpecialDay storeSpecialDay)
        {
            try
            {
                var sql = @"UPDATE store_special_day 
                            SET 
                                modified_by = @modified_by, 
                                modified_time = @modified_time, 
                                server_modified_time = @server_modified_time, 
                                store_uid = @store_uid 
                            WHERE 
                                uid = @UID;";
                return await ExecuteNonQueryAsync(sql, storeSpecialDay);
                 
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreSpecialDay(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM store_special_day WHERE uid = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}








