using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class PGSQLSalesOfficeDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISalesOfficeDL
    {
        public PGSQLSalesOfficeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ISalesOffice>> SelectAllSalesOfficeDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"select  * from 
                                                (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, branch_uid, 
                                                 code, name FROM sales_office) 
                                                 as SubQuery");
                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                                (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, branch_uid, 
                                                 code, name FROM sales_office) 
                                              as SubQuery");
                }
                Dictionary<string, object> parameters = new();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    _ = sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.ISalesOffice>(filterCriterias, sbFilterCriteria, parameters);
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
                        _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        _ = sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.Location.Model.Interfaces.ISalesOffice> salesOfficeDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ISalesOffice>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(),  parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ISalesOffice> pagedResponse = new()
                {
                    PagedData = salesOfficeDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.ISalesOffice?>> GetSalesOfficeByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new()
                {
                {"UID",  UID}
            };
                string sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, branch_uid, 
                            code, name, warehouse_uid FROM sales_office WHERE branch_uid = @UID";

                return await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ISalesOffice>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<int> CreateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            int retVal;
            try
            {
                string sql = @"INSERT INTO sales_office (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                           ss,branch_uid,code,name,warehouse_uid ) 
                          VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                          @SS,@BranchUID,@Code,@Name,@WareHouseUID)";
                retVal = await ExecuteNonQueryAsync(sql, salesOffice);
            }
            catch (Exception ex)
            {
                throw;
            }
            return retVal;

        }
        public async Task<int> UpdateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            try
            {
                string sql = @"UPDATE sales_office
                            SET 
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                ss = @SS,
                                branch_uid = @BranchUID,
                                code = @Code,
                                name = @Name,
                                warehouse_uid = @WareHouseUID
                            WHERE 
                                uid = @UID;";
                return await ExecuteNonQueryAsync(sql, salesOffice);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSalesOffice(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new()
                {
                {"UID" , UID}
            };
                string sql = @"DELETE  FROM sales_office WHERE UID = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }


        }
        public async Task<string?> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID)
        {
            try
            {
                string sql = "SELECT warehouse_uid FROM sales_office WHERE uid = @UID";

                return await ExecuteScalarAsync<string>(sql, new { UID = salesOfficeUID });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
