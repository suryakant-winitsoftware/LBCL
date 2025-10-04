using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.JobPosition.DL.Interfaces;




//using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.JobPosition.DL.Classes
{
    public class MSSQLJobPositionDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager,
        Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        public MSSQLJobPositionDL(IApprovalEngineHelper approvalEngineHelper, IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider,
        config)
        {
            _approvalEngineHelper = approvalEngineHelper;
        }

        public async Task<PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>>
            SelectAllJobPositionDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * From (SELECT 
                                                id AS Id,
                                                uid AS UID,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                company_uid AS CompanyUID,
                                                designation AS Designation,
                                                emp_uid AS EmpUID,
                                                department AS Department,
                                                user_role_uid AS UserRoleUID,
                                                location_mapping_template_uid AS LocationMappingTemplateUID,
                                                org_uid AS OrgUID,
                                                seq_code AS SeqCode,
                                                has_eot AS HasEOT,
                                                collection_limit AS CollectionLimit,
                                                ss AS SS,
                                                reports_to_uid AS ReportsToUID,
                                                sku_mapping_template_uid AS SKUMappingTemplateUID
                                            FROM job_position)As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                id AS Id,
                                                uid AS UID,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                company_uid AS CompanyUID,
                                                designation AS Designation,
                                                emp_uid AS EmpUID,
                                                department AS Department,
                                                user_role_uid AS UserRoleUID,
                                                location_mapping_template_uid AS LocationMappingTemplateUID,
                                                org_uid AS OrgUID,
                                                seq_code AS SeqCode,
                                                has_eot AS HasEOT,
                                                collection_limit AS CollectionLimit,
                                                ss AS SS,
                                                reports_to_uid AS ReportsToUID,
                                                sku_mapping_template_uid AS SKUMappingTemplateUID
                                            FROM job_position)As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(filterCriterias,
                    sbFilterCriteria, parameters);
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
                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append(
                        $" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> JobPositionDetails =
                    await ExecuteQueryAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql.ToString(),
                    parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> pagedResponse =
                    new PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>
                    {
                        PagedData = JobPositionDetails,
                        TotalCount = totalCount
                    };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionByUID(string UID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID}
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
                                                company_uid AS CompanyUID,
                                                designation AS Designation,
                                                emp_uid AS EmpUID,
                                                department AS Department,
                                                user_role_uid AS UserRoleUID,
                                                location_mapping_template_uid AS LocationMappingTemplateUID,
                                                org_uid AS OrgUID,
                                                seq_code AS SeqCode,
                                                has_eot AS HasEOT,
                                                collection_limit AS CollectionLimit,
                                                ss AS SS,
                                                reports_to_uid AS ReportsToUID,
                                                sku_mapping_template_uid AS SKUMappingTemplateUID
                                            FROM job_position WHERE uid = @UID";

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails =
                await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters, null,
                connection, transaction);
            return JobPositionDetails;
        }


        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>
            GetJobPositionLocationTypeAndValueByUID(string UID, IDbConnection? connection = null,
            IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID}
            };
            var sql = @"SELECT 
                                                location_type  AS LocationType,
                                                location_value AS LocationValue
                                            FROM job_position WHERE uid = @UID";

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionLocationTypeAndValueDetails =
                await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters, null,
                connection, transaction);
            return JobPositionLocationTypeAndValueDetails;
        }

        public async Task<int> UpdateJobPosition1(Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE job_position SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, company_uid = @CompanyUID, designation = @Designation, emp_uid = @EmpUID,
                            department = @Department, user_role_uid = @UserRoleUID, location_mapping_template_uid = @LocationMappingTemplateUID, 
                            org_uid = @OrgUID, seq_code = @SeqCode, has_eot = @HasEOT, ss = @SS, reports_to_uid = @ReportsToUID, branch_uid =  @BranchUID,
                            collection_limit = @CollectionLimit,sku_mapping_template_uid=@SKUMappingTemplateUID, sales_office_uid = @SalesOfficeUID WHERE uid = @UID;";
                retVal = await ExecuteNonQueryAsync(sql, connection, transaction, jobPositionApprovalDTO.JobPosition);
                _ = GenerateMyTeam(jobPositionApprovalDTO.JobPosition.UID);
                if (retVal>0 && jobPositionApprovalDTO.ApprovalRequestItem != null)
                {
                    await CreateApprovalRequest(jobPositionApprovalDTO.JobPosition.EmpUID, jobPositionApprovalDTO.ApprovalRequestItem);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> CreateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql =
                    @"INSERT INTO job_position ( uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, company_uid, designation, emp_uid, department, user_role_uid, 
                            location_mapping_template_uid, org_uid, seq_code, has_eot, ss, reports_to_uid,collection_limit,
                            sku_mapping_template_uid) VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                            @ServerAddTime, @ServerModifiedTime, @CompanyUID, @Designation, @EmpUID, @Department, @UserRoleUID, 
                            @LocationMappingTemplateUID, @OrgUID, @SeqCode, @HasEOT, @SS, @ReportsToUID,@CollectionLimit,@SKUMappingTemplateUID);";

                retVal = await ExecuteNonQueryAsync(sql, connection, transaction, jobPosition);
                _ = GenerateMyTeam(jobPosition?.UID);

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        #region Approval Engion Region
        private async Task<bool> CreateApprovalRequest(string linkedItemUID, ApprovalRequestItem approvalRequestItem)
        {
            try
            {
                IAllApprovalRequest approvalRequest = _serviceProvider.GetRequiredService<IAllApprovalRequest>();
                approvalRequest.LinkedItemType = "MaintainUser";
                approvalRequest.LinkedItemUID = linkedItemUID;
                ApprovalApiResponse<ApprovalStatus> approvalRequestCreated = await _approvalEngineHelper.CreateApprovalRequest(approvalRequestItem, approvalRequest);
                return approvalRequestCreated.Success;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion
        public async Task<int> UpdateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE job_position SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, company_uid = @CompanyUID, designation = @Designation, emp_uid = @EmpUID,
                            department = @Department, user_role_uid = @UserRoleUID, location_mapping_template_uid = @LocationMappingTemplateUID, 
                            org_uid = @OrgUID, seq_code = @SeqCode, has_eot = @HasEOT, ss = @SS, reports_to_uid = @ReportsToUID, branch_uid =  @BranchUID,
                            collection_limit = @CollectionLimit,sku_mapping_template_uid=@SKUMappingTemplateUID, sales_office_uid = @SalesOfficeUID WHERE uid = @UID;";
                retVal = await ExecuteNonQueryAsync(sql, connection, transaction, JobPosition);
                _ = GenerateMyTeam(JobPosition.UID);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

        public async Task<int> UpdateJobLocationTypeAndValuePosition(
            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition, IDbConnection? connection = null,
            IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql =
                    @"UPDATE job_position SET location_type = @LocationType ,location_value = @LocationValue WHERE uid = @UID;";

                retVal = await ExecuteNonQueryAsync(sql, connection, transaction, JobPosition);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }


        public async Task<int> DeleteJobPosition(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID}
            };
            var sql = @"DELETE  FROM job_position WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IJobPosition> SelectJobPositionByEmpUID(string EmpUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"EmpUID", EmpUID}
            };
            var sql = @" SELECT 
                                                jp.id AS Id,jp.uid AS UID,
                                                jp.created_by AS CreatedBy,jp.created_time AS CreatedTime,
                                                jp.modified_by AS ModifiedBy,jp.modified_time AS ModifiedTime,
                                                jp.server_add_time AS ServerAddTime,jp.server_modified_time AS ServerModifiedTime,
                                                jp.company_uid AS CompanyUID,jp.designation AS Designation,jp.emp_uid AS EmpUID,jp.department AS Department,
                                                jp.user_role_uid AS UserRoleUID,jp.location_mapping_template_uid AS LocationMappingTemplateUID,lt.template_name as LocationMappingTemplateName,jp.org_uid AS OrgUID,
                                                jp.seq_code AS SeqCode,jp.has_eot AS HasEOT,jp.collection_limit AS CollectionLimit,jp.ss AS SS,
                                                jp.reports_to_uid AS ReportsToUID,jp.sku_mapping_template_uid AS SKUMappingTemplateUID,st.template_name as SKUMappingTemplateName,
                                                jp.location_type AS LocationType,jp.location_value AS LocationValue, jp.branch_uid as branchuid,
                                                jp.sales_office_uid as salesofficeuid 
                                            FROM job_position jp
											Left join sku_template st on st.uid=jp.sku_mapping_template_uid
											Left join location_template lt on lt.uid=jp.location_mapping_template_uid
											WHERE jp.emp_uid= @EmpUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IJobPosition>().GetType();

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails =
                await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters,
                type);
            return JobPositionDetails;
        }

        public Task<IJobPositionAttendance> GetJobPositionAttendanceByEmpUID(string jobPositionUID, string empUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateJobPositionAttendance(IJobPositionAttendance jobPositionDetails)
        {
            throw new NotImplementedException();
        }

        private async Task GenerateMyTeam(string jobPositionUid)
        {
            try
            {
                string sql = """
                             USP_Populate_MyData
                             """;
                var parameters = new DynamicParameters();
                parameters.Add("@ParamJobPositionUID", jobPositionUid ?? string.Empty, DbType.String);
                await Query(async (e) => await e.QueryAsync(sql, parameters));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        Task<IJobPositionAttendance> IJobPositionDL.GetTotalAssignedAndVisitedStores(string JobPositionUID)
        {
            throw new NotImplementedException();
        }
    }
}
