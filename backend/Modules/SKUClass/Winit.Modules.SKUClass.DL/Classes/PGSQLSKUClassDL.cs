using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.DL.Classes
{
    public class PGSQLSKUClassDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUClassDL
    {
        public PGSQLSKUClassDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>> SelectAllSKUClassDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, class_name AS ClassName, description AS Description,class_label AS ClassLabel FROM sku_class");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM sku_class");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>(filterCriterias, sbFilterCriteria, parameters);

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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUClass>().GetType();
                IEnumerable<Model.Interfaces.ISKUClass> sKUClasss = await ExecuteQueryAsync<Model.Interfaces.ISKUClass>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass> pagedResponse = new PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>
                {
                    PagedData = sKUClasss,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass> GetSKUClassByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime, modified_by AS ModifiedBy, 
                        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                        company_uid AS CompanyUID, class_name AS ClassName, description AS Description,class_label AS ClassLabel FROM sku_class WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUClass>().GetType();
            Winit.Modules.SKUClass.Model.Interfaces.ISKUClass SKUClassDetails = await ExecuteSingleAsync<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>(sql, parameters, type);
            return SKUClassDetails;
        }
        public async Task<int> CreateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass createSKUClass)
        {
            try
            {
                var sql = @"INSERT INTO sku_class (
                    uid, company_uid, class_name, description, class_label, created_time, modified_time, server_add_time, server_modified_time,
                    created_by, modified_by) VALUES (@UID, @CompanyUID, @ClassName, @Description,@ClassLabel, @CreatedTime, @ModifiedTime, 
                    @ServerAddTime, @ServerModifiedTime,@CreatedBy,@ModifiedBy)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createSKUClass.UID},
                   {"CompanyUID", createSKUClass.CompanyUID},
                   {"ClassName", createSKUClass.ClassName},
                   {"Description", createSKUClass.Description},
                   {"ClassLabel",createSKUClass.ClassLabel},
                   {"CreatedBy",createSKUClass.CreatedBy},
                   {"ModifiedBy",createSKUClass.ModifiedBy},
                   {"CreatedTime", createSKUClass.CreatedTime},
                   {"ModifiedTime", createSKUClass.ModifiedTime},
                   {"ServerAddTime", createSKUClass.ServerAddTime},
                   {"ServerModifiedTime", createSKUClass.ServerModifiedTime},

             };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass updateSKUClass)
        {
            try
            {
                var sql = @"UPDATE sku_class 
                SET 
                    company_uid = @CompanyUID,
                    class_name = @ClassName, 
                    description = @Description,
                    class_label = @ClassLabel, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime 
                WHERE 
                    uid = @UID;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                  {"UID", updateSKUClass.UID},
                   {"CompanyUID", updateSKUClass.CompanyUID},
                   {"ClassName", updateSKUClass.ClassName},
                   {"Description", updateSKUClass.Description},
                   {"ClassLabel", updateSKUClass.ClassLabel},
                   {"ModifiedTime", updateSKUClass.ModifiedTime},
                   {"ServerModifiedTime", updateSKUClass.ServerModifiedTime},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUClass(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_class 
                WHERE uid = @UID;";

            return await ExecuteNonQueryAsync(sql, parameters);

        }
    }
}
