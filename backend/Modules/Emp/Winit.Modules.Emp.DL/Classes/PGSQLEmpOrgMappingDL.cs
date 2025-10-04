using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;


using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Classes
{
    public class PGSQLEmpOrgMappingDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IEmpOrgMappingDL
    {
        public PGSQLEmpOrgMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select id,uid,created_by as CreatedBy,created_time as CreatedTime,
                                            modified_by as ModifiedBy, modified_time as ModifiedTime,server_add_time as ServerAddTime,
                                            server_modified_time as ServerModifiedTime,emp_uid as EmpUID,org_uid as OrgUID from emp_org_mapping");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) AS Cnt from emp_org_mapping");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmpOrgMapping>().GetType();

                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> empOrgMappingDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> pagedResponse = new PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>
                {
                    PagedData = empOrgMappingDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateEmpOrgMapping(List<Winit.Modules.Emp.Model.Classes.EmpOrgMapping> empOrgMappings)
        {
            int retVal = -1;
            try
            {
                foreach(var empOrgMapping in empOrgMappings)
                {
                    var sql = @"insert into emp_org_mapping (uid, created_by, created_time,modified_by, 
                            modified_time,server_add_time, server_modified_time, emp_uid, org_uid)
                        values (@UID, @CreatedBy, @CreatedTime, @CreatedBy, @ModifiedTime, 
                        @ServerAddTime,  @ServerModifiedTime, @EmpUID, @OrgUID)";

                    Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", empOrgMapping.UID},
                   {"CreatedBy", empOrgMapping.CreatedBy},
                   {"CreatedTime", empOrgMapping.CreatedTime},
                   {"ModifiedTime", empOrgMapping.ModifiedTime},
                   {"ServerAddTime", DateTime.Now},
                   {"ServerModifiedTime", DateTime.Now},
                   {"EmpUID", empOrgMapping.EmpUID},
                   {"OrgUID", empOrgMapping.OrgUID},
             };
                    retVal= await ExecuteNonQueryAsync(sql, parameters);
                }
               

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

        public async Task<int> DeleteEmpOrgMapping(string uid)
        {
            try
            {
                var sql = @"delete from emp_org_mapping where uid=@UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", uid},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetailsByEmpUID(string empUID)
        {
            try
            {
                var sql = new StringBuilder(@"select id,uid,created_by as CreatedBy,created_time as CreatedTime,
                                            modified_by as ModifiedBy, modified_time as ModifiedTime,server_add_time as ServerAddTime,
                                            server_modified_time as ServerModifiedTime,emp_uid as EmpUID,org_uid as OrgUID from emp_org_mapping where emp_uid=@Empuid");
                var parameters = new Dictionary<string, object>() {
                    {"Empuid",empUID }
                };

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmpOrgMapping>().GetType();

               IEnumerable< Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> empOrgMappings = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>(sql.ToString(), parameters, type);
                return empOrgMappings;

            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
