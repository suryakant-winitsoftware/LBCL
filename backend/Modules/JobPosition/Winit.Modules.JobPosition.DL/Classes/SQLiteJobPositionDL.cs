using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JobPosition.DL.Classes
{
    public class SQLiteJobPositionDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL
    {
        public SQLiteJobPositionDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>> SelectAllJobPositionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time 
                  AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                   server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid, designation AS Designation, emp_uid AS EmpUid,
                   department AS Department, user_role_uid AS UserRoleUid, location_mapping_template_uid AS LocationMappingTemplateUID, org_uid AS OrgUid, seq_code AS SeqCode,
                   has_eot AS HasEot, ss AS Ss, reports_to_uid AS ReportsToUid,sku_mapping_template_uid AS SKUMappingTemplateUID
                FROM job_position) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                    company_uid AS CompanyUid, designation AS Designation, emp_uid AS EmpUid, department AS Department, user_role_uid AS UserRoleUid,
                                    location_mapping_template_uid AS LocationMappingTemplateUID, org_uid AS OrgUid, seq_code AS SeqCode, has_eot AS HasEot, 
                                    ss AS Ss, reports_to_uid AS ReportsToUid,sku_mapping_template_uid AS SKUMappingTemplateUID
                                    FROM job_position) As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sql.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sql, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IJobPosition>().GetType();

                IEnumerable<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> JobPositionDetails = await ExecuteQueryAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> pagedResponse = new PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>
                {
                    PagedData = JobPositionDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid, designation AS Designation, 
                emp_uid AS EmpUid, department AS Department, user_role_uid AS UserRoleUid, location_mapping_template_uid AS LocationMappingTemplateUID, 
                org_uid AS OrgUid, seq_code AS SeqCode, has_eot AS HasEot, ss AS Ss, reports_to_uid AS ReportsToUid,sku_mapping_template_uid AS SKUMappingTemplateUID
                FROM job_position WHERE uid = @UID";

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails = await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters, null, connection, transaction);
            return JobPositionDetails;
        }

        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionLocationTypeAndValueByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT location_type AS LocationType,location_value AS LocationValue FROM job_position WHERE uid = @UID";

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionLocationTypeAndValueDetails = await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters, null, connection, transaction);
            return JobPositionLocationTypeAndValueDetails;
        }
        public async Task<int> UpdateJobPosition1(Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"UPDATE job_position SET
                modified_by = @ModifiedBy,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                company_uid = @CompanyUID,
                designation = @Designation,
                emp_uid = @EmpUID,
                department = @Department,
                user_role_uid = @UserRoleUID,
                location_mapping_template_uid = @LocationMappingTemplateUID,
                org_uid = @OrgUID,
                seq_code = @SeqCode,
                has_eot = @HasEOT,
                sku_mapping_template_uid = @SKUMappingTemplateUID
            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, jobPositionApprovalDTO.JobPosition, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"INSERT INTO job_position (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            company_uid, designation, emp_uid, department, user_role_uid, location_mapping_template_uid, org_uid, seq_code, has_eot,sku_mapping_template_uid)
                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                        @CompanyUID, @Designation, @EmpUID, @Department, @UserRoleUID, @LocationMappingTemplateUID, @OrgUID, @SeqCode, @HasEOT,@SKUMappingTemplateUID);";

                return await ExecuteNonQueryAsync(sql, jobPosition, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"UPDATE job_position SET
                modified_by = @ModifiedBy,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                company_uid = @CompanyUID,
                designation = @Designation,
                emp_uid = @EmpUID,
                department = @Department,
                user_role_uid = @UserRoleUID,
                location_mapping_template_uid = @LocationMappingTemplateUID,
                org_uid = @OrgUID,
                seq_code = @SeqCode,
                has_eot = @HasEOT,
                sku_mapping_template_uid = @SKUMappingTemplateUID
            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, JobPosition, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateJobLocationTypeAndValuePosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"UPDATE job_position SET
               location_type = LocationType , location_value = LocationValue
            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, JobPosition, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteJobPosition(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM job_position WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }




        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> SelectJobPositionByEmpUID(string EmpUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"EmpUID",  EmpUID}
            };
            var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid, designation AS Designation, 
                        emp_uid AS EmpUid, department AS Department, user_role_uid AS UserRoleUid, location_mapping_template_uid AS LocationMappingTemplateUID, 
                        org_uid AS OrgUid, seq_code AS SeqCode, has_eot AS HasEot, ss AS Ss, reports_to_uid AS ReportsToUid,sku_mapping_template_uid AS SKUMappingTemplateUID,
                        location_type AS LocationType,location_value = LocationValue
                        FROM job_position WHERE emp_uid = @EmpUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IJobPosition>().GetType();

            Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails = await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>(sql, parameters, type);
            return JobPositionDetails;
        }

        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetJobPositionAttendanceByEmpUID(string jobPositionUID, string empUID)
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            var parameters = new Dictionary<string, object>
            {
                { "job_position_uid", jobPositionUID },
                { "emp_uid", empUID },
                { "year", currentYear },
                { "month", currentMonth }
            };

            var sql = @"
                SELECT 
                    id AS Id, 
                    uid AS Uid, 
                    ss AS Ss, 
                    created_by AS CreatedBy, 
                    created_time AS CreatedTime, 
                    modified_by AS ModifiedBy, 
                    modified_time AS ModifiedTime, 
                    server_add_time AS ServerAddTime, 
                    server_modified_time AS ServerModifiedTime, 
                    org_uid AS OrgUid, 
                    job_position_uid AS JobPositionUid, 
                    emp_uid AS EmpUid, 
                    year AS Year, 
                    month AS Month, 
                    no_of_days AS NoOfDays, 
                    no_of_holidays AS NoOfHolidays, 
                    no_of_working_days AS NoOfWorkingDays, 
                    days_present AS DaysPresent, 
                    attendance_percentage AS AttendancePercentage, 
                    last_update_date AS LastUpdateDate 
                FROM job_position_attendance 
                WHERE job_position_uid = @job_position_uid 
                  AND emp_uid = @emp_uid 
                  AND year = @year 
                  AND month = @month";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IJobPositionAttendance>().GetType();

            Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance JobPositionDetails = await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance>(sql, parameters, type);
            return JobPositionDetails;
        }

        public async Task<int> UpdateJobPositionAttendance(Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance jobPositionDetails)
        {
            try
            {
                var sql = @"UPDATE job_position_attendance
                SET 
                    days_present = @DaysPresent,
                    attendance_percentage = @AttendancePercentage,
                    last_update_date = @LastUpdateDate,
                    ss = @SS
                WHERE uid = @UID";
                string isoFormattedDate = jobPositionDetails.LastUpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",jobPositionDetails.UID},
                    {"DaysPresent",jobPositionDetails.DaysPresent},
                    {"AttendancePercentage",jobPositionDetails.AttendancePercentage},
                    {"LastUpdateDate",isoFormattedDate},
                    {"SS",2}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetTotalAssignedAndVisitedStores(string JobPositionUID)
        {
            var parameters = new Dictionary<string, object>
            {
                { "JobPositionUID", JobPositionUID },
            };

            var sql = @"
                 SELECT 
                    (SELECT COUNT(DISTINCT RC.store_uid)
                     FROM route R
                     INNER JOIN route_customer RC ON RC.route_uid = R.uid 
                     WHERE R.job_position_uid = @JobPositionUID) AS TotalAssignedStores,

                    (SELECT COUNT(DISTINCT SH.store_uid)
                     FROM route R
                     INNER JOIN beat_history BH 
                         ON BH.route_uid = R.uid 
                         AND R.job_position_uid = @JobPositionUID
                         AND strftime('%Y-%m', BH.visit_date) = strftime('%Y-%m', 'now')
                     INNER JOIN store_history SH 
                         ON SH.beat_history_uid = BH.uid) AS TotalVisitedStores;";

            Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance JobPositionDetails = await ExecuteSingleAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance>(sql, parameters);
            return JobPositionDetails;
        }
    }
}
