using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.UOM.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.UOM.DL.Classes
{
    public class MSSQLUOMTypeDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IUOMTypeDL
    {
        public MSSQLUOMTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType>> SelectAllUOMTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
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
                                            name AS Name, 
                                            label AS Label
                                        FROM 
                                            uom_type)as sunquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt from (SELECT 
                                            id AS Id, 
                                            uid AS UID, 
                                            created_by AS CreatedBy, 
                                            created_time AS CreatedTime, 
                                            modified_by AS ModifiedBy, 
                                            modified_time AS ModifiedTime, 
                                            server_add_time AS ServerAddTime, 
                                            server_modified_time AS ServerModifiedTime, 
                                            name AS Name, 
                                            label AS Label
                                        FROM 
                                            uom_type)as sunquery;");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.UOM.Model.Interfaces.IUOMType>(filterCriterias, sbFilterCriteria, parameters);

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
                    sql.Append(" ORDER BY Id");
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

                IEnumerable<Model.Interfaces.IUOMType> uOMTypes = await ExecuteQueryAsync<Model.Interfaces.IUOMType>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType> pagedResponse = new PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType>
                {
                    PagedData = uOMTypes,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
      
    }
}
