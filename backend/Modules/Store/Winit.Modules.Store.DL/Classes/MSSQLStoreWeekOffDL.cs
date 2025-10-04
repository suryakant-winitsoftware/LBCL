using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class MSSQLStoreWeekOffDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreWeekOffDL
    {
        public MSSQLStoreWeekOffDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> CreateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff)
        {
            try
            {
                var sql = @"INSERT INTO store_week_off (uid, created_by, created_time, modified_by, modified_time,
                            server_add_time, server_modified_time, store_uid, sun, mon, tue, wed, thu, fri, sat)
                           VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@StoreUID ,@Sun ,@Mon
                            ,@Tue ,@Wed ,@Thu ,@Fri ,@Sat)";
                
                return await ExecuteNonQueryAsync(sql, storeWeekOff);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteStoreWeekOff(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = @"DELETE  FROM store_week_off WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<PagedResponse<Model.Interfaces.IStoreWeekOff>> SelectAllStoreWeekOff(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM ""StoreWeekOff""");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""StoreWeekOff""");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IStoreWeekOff>(filterCriterias, sbFilterCriteria, parameters);;

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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreWeekOff>().GetType();
                IEnumerable<Model.Interfaces.IStoreWeekOff> StoreWeekOffs = await ExecuteQueryAsync<Model.Interfaces.IStoreWeekOff>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreWeekOff> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreWeekOff>
                {
                    PagedData = StoreWeekOffs,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Model.Interfaces.IStoreWeekOff> SelectStoreWeekOffByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                       store_uid, sun, mon, tue, wed, thu, fri, sat
                        FROM store_week_off
                        WHERE uid = @UID ";
            Model.Interfaces.IStoreWeekOff StoreWeekOffStoreList = await ExecuteSingleAsync<Model.Interfaces.IStoreWeekOff>(sql, parameters);
            return StoreWeekOffStoreList;
        }

        public async Task<int> UpdateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff)
        {
            try
            {
                var sql = @"UPDATE store_week_off
                        SET modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            store_uid = @StoreUID,
                            sun = @Sun,
                            mon = @Mon,
                            tue = @Tue,
                            wed = @Wed,
                            thu = @Thu,
                            fri = @Fri,
                            sat = @Sat
                        WHERE uid = @UID ";

                
                return await ExecuteNonQueryAsync(sql, storeWeekOff);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}








