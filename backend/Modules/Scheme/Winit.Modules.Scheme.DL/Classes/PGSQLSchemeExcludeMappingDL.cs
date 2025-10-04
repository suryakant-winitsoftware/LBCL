using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLSchemeExcludeMappingDL : PostgresDBManager, ISchemeExcludeMappingDL
    {
        public PGSQLSchemeExcludeMappingDL(IServiceProvider serviceProvider, IConfiguration config)
            : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (select '[' + s.code + '] ' + s.name as StoreUID, sem.start_date as StartDate, sem.end_date as EndDate, sem.created_time CreatedTime, sem.is_active IsActive
                                              from scheme_exclude_mapping sem
                                              INNER JOIN store s on sem.store_uid = s.uid) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select '[' + s.code + '] ' + s.name as StoreUID, sem.start_date as StartDate, sem.end_date as EndDate, sem.created_time CreatedTime, sem.is_active IsActive
                                              from scheme_exclude_mapping sem
                                              INNER JOIN store s on sem.store_uid = s.uid) AS SubQuery");
                }

                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>(filterCriterias, sbFilterCriteria, parameters);
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
                else
                {
                    sql.Append(" ORDER BY CreatedTime");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> SchemeExcludeDetails = await ExecuteQueryAsync<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>(sql.ToString(), parameters);
                int totalCount = -1;

                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> pagedResponse = new PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>
                {
                    PagedData = SchemeExcludeDetails,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMappingHistory(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (select '[' + s.code + '] ' + s.name as StoreUID, semh.start_date as StartDate, semh.end_date as EndDate, semh.created_time CreatedTime, semh.is_active IsActive, semh.expired_on ExpiredOn
                                              from scheme_exclude_mapping_history semh
                                              INNER JOIN store s on semh.store_uid = s.uid) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select '[' + s.code + '] ' + s.name as StoreUID, semh.start_date as StartDate, semh.end_date as EndDate, semh.created_time CreatedTime, semh.is_active IsActive
                                              from scheme_exclude_mapping_history semh
                                              INNER JOIN store s on semh.store_uid = s.uid) AS SubQuery");
                }

                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>(filterCriterias, sbFilterCriteria, parameters);
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
                else
                {
                    sql.Append(" ORDER BY CreatedTime");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> SchemeExcludeDetails = await ExecuteQueryAsync<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>(sql.ToString(), parameters);
                int totalCount = -1;

                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> pagedResponse = new PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>
                {
                    PagedData = SchemeExcludeDetails,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<ISchemeExcludeMapping>> GetSchemeExcludeMappings(List<SchemeKey> schemeKeys)
        {
            string sql = @"SELECT s.* FROM scheme_exclude_mapping s
                   INNER JOIN @SchemeKeys sk
                   ON s.scheme_type = sk.SchemeType
                   AND s.scheme_uid = sk.SchemeUID
                   AND s.store_uid = sk.StoreUID
                   WHERE s.is_active = 1 
                   AND DATEDIFF(dd, GETDATE(), s.end_date) >= 0";

            var table = new DataTable();
            table.Columns.Add("SchemeType", typeof(string));
            table.Columns.Add("SchemeUID", typeof(string));
            table.Columns.Add("StoreUID", typeof(string));

            foreach (var key in schemeKeys)
            {
                table.Rows.Add(key.SchemeType, key.SchemeUID, key.StoreUID);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@SchemeKeys", table.AsTableValuedParameter("SchemeKeyType"));

            return await ExecuteQueryAsync<ISchemeExcludeMapping>(sql, parameters);
        }
        public async Task<int> InsertSchemeExcludeMapping(List<ISchemeExcludeMapping> schemeExcludeMapping)
        {
            try
            {
                var sql = @"
                INSERT INTO scheme_exclude_mapping 
                (scheme_type, scheme_uid, store_uid, start_date, end_date, is_active, created_by, created_time) 
                VALUES (@SchemeType, @SchemeUID, @StoreUID, @StartDate, @EndDate, 1, @CreatedBy, @CreatedTime)";

                return await ExecuteNonQueryAsync(sql, schemeExcludeMapping);
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        public async Task<int> UpdateSchemeExcludeMapping(List<ISchemeExcludeMapping> schemeExcludeMapping)
        {
            var sql = @"
            UPDATE scheme_exclude_mapping 
            SET end_date = @EndDate, modified_by = @ModifiedBy, modified_time = @ModifiedTime, is_active = @IsActive, expired_on = @ExpiredOn
            WHERE id = @Id;
            
            Insert into scheme_exclude_mapping_history(scheme_type, scheme_uid, store_uid, start_date, end_date, is_active, created_by, created_time, modified_by,modified_time, expired_on)
            Select scheme_type, scheme_uid, store_uid, start_date, end_date, is_active, created_by, created_time, modified_by,modified_time, expired_on from scheme_exclude_mapping where id = @Id;

            Delete from scheme_exclude_mapping where id = @Id;";

            return await ExecuteNonQueryAsync(sql, schemeExcludeMapping);
        }

        public async Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate)
        {
            var param = new Dictionary<string, object>()
            {
                {"StoreUID",storeUID },
                {"CurrentDate" ,currentDate}
            }
            ;
            string sql = """
                               select count(1) from scheme_exclude_mapping 
                where store_uid=@StoreUID and DATEDIFF(mi,start_date,@CurrentDate)>=0 and DATEDIFF(mi,@CurrentDate,end_date)>=0
                """;
            return await ExecuteScalarAsync<int>(sql, param);
        }

        public async Task<string?> CheckUIDExistsInDB(string TableName, string UID)
        {
            try
            {
               string uid = await CheckIfUIDExistsInDB(TableName, UID);
                return uid;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
