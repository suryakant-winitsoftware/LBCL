using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Classes
{
    public class PGSQLEmpInfoDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IEmpInfoDL
    {
        public PGSQLEmpInfoDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>> GetEmpInfoDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                                    var sql = new StringBuilder(@"SELECT 
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        emp_uid AS EmpUID,
                        email AS Email,
                        phone AS Phone,
                        start_date AS StartDate,
                        end_date AS EndDate,
                        can_handle_stock AS CanHandleStock,
                        ad_group AS ADGroup,
                        ad_username AS ADUsername
                    FROM 
                        emp_info");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM emp_info");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmp>().GetType();

                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> EmpInfoDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> pagedResponse = new PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>
                {
                    PagedData = EmpInfoDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByUID(string EmpUID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"EmpUID",  EmpUID}
            };
            var sql = @"SELECT 
    id AS Id,
    uid AS UID,
    created_by AS CreatedBy,
    created_time AS CreatedTime,
    modified_by AS ModifiedBy,
    modified_time AS ModifiedTime,
    server_add_time AS ServerAddTime,
    server_modified_time AS ServerModifiedTime,
    emp_uid AS EmpUID,
    email AS Email,
    phone AS Phone,
    start_date AS StartDate,
    end_date AS EndDate,
    can_handle_stock AS CanHandleStock,
    ad_group AS ADGroup,
    ad_username AS ADUsername
FROM 
    emp_info where emp_uid= @EmpUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmpInfo>().GetType();

            Winit.Modules.Emp.Model.Interfaces.IEmpInfo EmpInfoDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>(sql, parameters,type,connection, transaction);
            return EmpInfoDetails;
        }
        public async Task<int> CreateEmpInfo(Winit.Modules.Emp.Model.Interfaces.IEmpInfo createEmpInfo, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO emp_info (uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, emp_uid, email, phone, start_date, end_date, can_handle_stock, 
                            ad_group, ad_username) VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @EmpUID, @Email, @Phone, @StartDate, @EndDate, @CanHandleStock, @ADGroup, @ADUsername)";

                
                retVal= await ExecuteNonQueryAsync(sql,connection,transaction, createEmpInfo);

            }
            catch (Exception ex)
            {
                throw;
            }
            return retVal;

        }
        public async Task<int> UpdateEmpInfoDetails(Winit.Modules.Emp.Model.Interfaces.IEmpInfo updateEmpInfo, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
             var sql = @"UPDATE emp_info SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, 
                        server_modified_time = @ServerModifiedTime, emp_uid = @EmpUID, email = @Email,
                        phone = @Phone, start_date = @StartDate, end_date = @EndDate, can_handle_stock = @CanHandleStock,
                        ad_group = @ADGroup, ad_username = @ADUsername WHERE uid = @UID;";
                retVal= await ExecuteNonQueryAsync(sql,connection,transaction, updateEmpInfo);


            }
            catch (Exception ex)
            {
                throw;
            }

            return retVal;
        }
        public async Task<int> DeleteEmpInfoDetails(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM Emp_Info WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);

        }
    }
}
