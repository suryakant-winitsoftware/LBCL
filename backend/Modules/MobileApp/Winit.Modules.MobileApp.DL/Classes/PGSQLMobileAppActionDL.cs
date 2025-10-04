using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.Mobile.DL.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Syncing.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.DL.Classes
{
    public class PGSQLMobileAppActionDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IMobileAppActionDL
    {
        public PGSQLMobileAppActionDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<Shared.Models.Common.PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> GetClearDataDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                                *
                                                            FROM
                                                                (
                                                                    SELECT DISTINCT
                                                                        ma.id AS Id,
                                                                        ma.uid AS UID,
                                                                        e.uid AS EmpUID,
                                                                        ma.action AS Action,
                                                                        ma.status AS Status,
                                                                        ma.action_date AS ActionDate,
                                                                        ma.result AS Result,
                                                                        ma.ss AS SS,
                                                                        ma.created_time AS CreatedTime,
                                                                        ma.modified_time AS ModifiedTime,
                                                                        ma.server_add_time AS ServerAddTime,
                                                                        ma.server_modified_time AS ServerModifiedTime,
                                                                        e.login_id AS LoginId
                                                                    FROM
                                                                        mobile_app_action ma
                                                                    JOIN
                                                                        org o ON ma.org_uid = o.uid
                                                                    JOIN
                                                                        emp e ON ma.emp_uid = e.uid
                                                                ) AS Subquery
");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM
                                               (SELECT DISTINCT
                                                                        ma.id AS Id,
                                                                        ma.uid AS UID,
                                                                        e.uid AS EmpUID,
                                                                        ma.action AS Action,
                                                                        ma.status AS Status,
                                                                        ma.action_date AS ActionDate,
                                                                        ma.result AS Result,
                                                                        ma.ss AS SS,
                                                                        ma.created_time AS CreatedTime,
                                                                        ma.modified_time AS ModifiedTime,
                                                                        ma.server_add_time AS ServerAddTime,
                                                                        ma.server_modified_time AS ServerModifiedTime,
                                                                        e.login_id AS LoginId
                                                                    FROM
                                                                        mobile_app_action ma
                                                                    JOIN
                                                                        org o ON ma.org_uid = o.uid
                                                                    JOIN
                                                                        emp e ON ma.emp_uid = e.uid)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IMobileAppAction>().GetType();

                IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> mobileAppActions = await ExecuteQueryAsync<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> pagedResponse = new PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>
                {
                    PagedData = mobileAppActions,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IUser>> GetUserDDL(string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT DISTINCT 
                                    '[' || e.login_id || '] ' || e.name AS Name,
                                    j.emp_uid AS UID 
                                FROM 
                                    emp e 
                                JOIN 
                                    job_position j ON e.uid = j.emp_uid 
                                WHERE 
                                    j.org_uid = @OrgUID;
");
                var sqlCount = new StringBuilder();
                var parameters = new Dictionary<string, object>()
                {
                    {"@OrgUID",OrgUID }
                };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IUser>().GetType();
                IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IUser> userList = await ExecuteQueryAsync<Winit.Modules.Mobile.Model.Interfaces.IUser>(sql.ToString(), parameters, type);
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>> SelectMobileAppActionByEmpUID(List<string> EmpUIDs)
        {
            try
            {
                string commaSeperatedUIDs = string.Join(",", EmpUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"EmpUIDs",  commaSeperatedUIDs}
            };
                var sql = new StringBuilder(@"SELECT 
                            id AS Id,
                            uid AS UID,
                            emp_uid AS EmpUID,
                            action AS Action,
                            status AS Status,
                            action_date AS ActionDate,
                            result AS Result,
                            ss AS SS,
                            created_time AS CreatedTime,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            org_uid AS OrgUID
                        FROM 
                            mobile_app_action
                          WHERE emp_uid= ANY(string_to_array(@EmpUIDs, ','));");
                var sqlCount = new StringBuilder();

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IMobileAppAction>().GetType();
                IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> list = await ExecuteQueryAsync<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>(sql.ToString(), parameters, type);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> PerformCUD(List<Winit.Modules.Mobile.Model.Classes.MobileAppAction> mobileAppActions)
        {
            int count = -1;
            try
            {
                if (mobileAppActions == null || mobileAppActions.Count == 0)
                {
                    return count;
                }
                
                List<string> empUIDs = mobileAppActions.Select(po => po.EmpUID).ToList();
                List<string> uids = mobileAppActions.Select(po => po.UID).ToList();
                
                List<MobileAppAction>? newMobileAppActions = null;
                List<MobileAppAction>? existingMobileAppActions = null;
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.MobileAppAction, uids);
                
                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newMobileAppActions = mobileAppActions.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                    existingMobileAppActions = mobileAppActions.Where(e => existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newMobileAppActions = mobileAppActions;
                }

                if (existingMobileAppActions != null && existingMobileAppActions.Any())
                {
                    count += await UpdateMobileAppActionList(existingMobileAppActions);
                }
                if (newMobileAppActions != null && newMobileAppActions.Any())
                {
                    count += await CreateMobileAppActionList(newMobileAppActions);
                }
            }
            catch
            {
                throw;
            }
            
            return count;
        }
        private async Task<int> UpdateMobileAppActionList(List<Winit.Modules.Mobile.Model.Classes.MobileAppAction> mobileAppActions)
        {
            int retVal = -1;
            try
            {
                var Query = @"UPDATE mobile_app_action 
                                    SET 
                                        action = @Action, 
                                        status = @Status, 
                                        action_date = @ActionDate, 
                                        result = @Result, 
                                        ss = @SS, 
                                        modified_time = @ModifiedTime, 
                                        server_modified_time = @ServerModifiedTime
                                    WHERE 
                                        uid = @UID;";

                retVal = await ExecuteNonQueryAsync(Query, mobileAppActions);
            }
            catch
            {
                throw;
            }
            return retVal;

        }
        private async Task<int> CreateMobileAppActionList(List<Winit.Modules.Mobile.Model.Classes.MobileAppAction> mobileAppActions)
        {
            int retVal = -1;
            try
            {
                var Query = @"INSERT INTO mobile_app_action (uid, emp_uid, action, status, action_date, result, ss, created_time,
                              modified_time, server_add_time, server_modified_time, org_uid)
                                VALUES 
                            (@UID, @EmpUID, @Action, @Status, @ActionDate, @Result, @SS, @CreatedTime,
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @OrgUID);";

                retVal = await ExecuteNonQueryAsync(Query, mobileAppActions);

            }
            catch
            {
                throw;
            }
            return retVal;

        }
        private async Task<int> UpdateMobileAppAction(string empUID)
        {

            int retVal = -1;
            try
            {
                var parameters = new Dictionary<string, object>
        {
            { "EmpUID", empUID },
            { "Action", true },
            { "ActionDate", DateTime.Now },
            { "Result","Success" },
            { "SS", 0 },
            { "ModifiedTime", DateTime.Now },
            { "ServerModifiedTime", DateTime.Now },
            { "Status",1 },
        };
                var updateQuery = @"
                UPDATE mobile_app_action 
                SET 
                    action = @Action, 
                    status = @Status, 
                    action_date = @ActionDate, 
                    result = @Result, 
                    ss = @SS, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime
                WHERE 
                    emp_uid = @EmpUID";
                retVal = await ExecuteNonQueryAsync(updateQuery, parameters);
            }
            catch
            {
                throw;
            }

            return retVal;
        }
        private async Task<int> UpdateMobileAppActionForSQLiteCreated(string empUID)
        {

            int retVal = -1;
            try
            {
                var parameters = new Dictionary<string, object>
        {
            { "EmpUID", empUID },
            { "Result","Success" },
            { "ModifiedTime", DateTime.Now },
            { "ServerModifiedTime", DateTime.Now },
            { "Status",1 },
        };
                var updateQuery = @"
                UPDATE mobile_app_action 
                SET 
                    status = @Status, 
                    result = @Result, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime
                WHERE 
                    emp_uid = @EmpUID";
                retVal = await ExecuteNonQueryAsync(updateQuery, parameters);
            }
            catch
            {
                throw;
            }

            return retVal;
        }

        private async Task<int> CreateMobileAppAction(Winit.Modules.Mobile.Model.Classes.MobileAppAction mobileAppAction)
        {

            var Query = @"INSERT INTO mobile_app_action (uid, emp_uid, action, status, action_date, result, ss, created_time, modified_time, server_add_time, server_modified_time, org_uid)
                                VALUES (@UID, @EmpUID, @Action, @Status, @ActionDate, @Result, @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @OrgUID);";
            var Parameters = new Dictionary<string, object>
                        {
                          { "@UID", mobileAppAction.UID },
                          { "@EmpUID", mobileAppAction.EmpUID },
                          { "@Action", mobileAppAction.Action },
                          { "@Status", mobileAppAction.Status },
                          { "@ActionDate", mobileAppAction.ActionDate },
                          { "@Result", mobileAppAction.Result },
                          { "@SS", mobileAppAction.SS },
                          { "@CreatedTime", mobileAppAction.CreatedTime },
                          { "@ModifiedTime", mobileAppAction.ModifiedTime },
                          { "@ServerAddTime", DateTime.Now},
                          { "@ServerModifiedTime", DateTime.Now},
                          { "@OrgUID", mobileAppAction.OrgUID },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }

        public async Task<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> GetMobileAppAction(string empUID)
        {
            try
            {
                var sql = new StringBuilder(@"
                     SELECT 

                ma.id AS Id,
                ma.uid AS UID,
                ma.emp_uid AS EmpUID,
                ma.action AS Action,
                ma.status AS Status,
                ma.action_date AS ActionDate,
                ma.result AS Result,
                ma.ss AS SS,
                ma.created_time AS CreatedTime,
                ma.modified_time AS ModifiedTime,
                ma.server_add_time AS ServerAddTime,
                ma.server_modified_time AS ServerModifiedTime,
                ma.org_uid AS OrgUID

                FROM mobile_app_action ma
                WHERE ma.emp_uid = @empUID
                AND ma.status = 0
                AND DATE_PART('day', age(ma.action_date, CURRENT_DATE)) = 0;");
                var parameters = new Dictionary<string, object>()
                {
                    { "empUID",empUID}
                };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IMobileAppAction>().GetType();

                Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction mobileAppAction = await ExecuteSingleAsync<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction>(sql.ToString(), parameters, type);

                return mobileAppAction;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> InitiateDBCreation(string employeeUID, string jobPositionUID, string roleUID, string orgUID, string vehicleUID, string empCode)
        {
            int count = -1;

            try
            {
                bool exists = false;
                ISqlitePreparation existingRec = await GetDBCreationStatus(employeeUID, jobPositionUID, roleUID);
                if (existingRec != null)
                {
                    exists = existingRec.EmpUID == employeeUID;
                }
                if (exists)
                {
                    count = await UpdateSqlitePreparation(employeeUID, jobPositionUID, roleUID, Winit.Modules.Auth.Model.Constants.SqlitePreparationStatus.IN_PROCESS);
                }
                else
                {
                    count = await CreateSqlitePreparation(employeeUID, jobPositionUID, roleUID);
                }
                if (count > 0)
                {
                    _ = CreateDB(employeeUID, jobPositionUID, roleUID, orgUID, vehicleUID, empCode);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<int> CreateDB(string employeeUID, string jobPositionUID, string roleUID, string orgUID, string vehicleUID, string empCode)
        {
            int retVal = -1;

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mobileDataSyncBL = scope.ServiceProvider.GetRequiredService<IMobileDataSyncBL>();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var commonFunctions = scope.ServiceProvider.GetRequiredService<CommonFunctions>();
                    string sqliteUrl = config["ApiSettings:SqliteDownloadUrl"];
                    if (await UpdateMobileAppActionForSQLiteCreated(employeeUID) == 1)
                    {
                        var relativePath = await mobileDataSyncBL.DownloadServerDataInSqlite(
                        null, null, employeeUID, jobPositionUID, roleUID, vehicleUID, orgUID, empCode);

                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            try
                            {
                                // Use SqliteFileHandler to create ZIP with proper file handling
                                var fileHandler = new Winit.Modules.Syncing.BL.Classes.SqliteFileHandler();
                                string zipFilePath = await fileHandler.CreateZipFromSqlite(relativePath);

                                // Convert physical path to URL path
                                var fileName = Path.GetFileName(zipFilePath);
                                var today = DateTime.Now; // Or use extracted timestamp if needed
                                var fullUrl = $"{sqliteUrl.TrimEnd('/')}/Data/Sqlite/User/{today.Year}/{today.Month}/{today.Day}/{employeeUID}/{fileName}";
                                retVal = await UpdateSqlitePreparation(employeeUID, jobPositionUID, roleUID,
                                    Winit.Modules.Auth.Model.Constants.SqlitePreparationStatus.READY, fullUrl);
                            }
                            catch (Exception zipEx)
                            {
                                await UpdateSqlitePreparation(employeeUID, jobPositionUID, roleUID,
                                    Winit.Modules.Auth.Model.Constants.SqlitePreparationStatus.FAILURE, null, $"Failed to create ZIP file: {zipEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        await UpdateSqlitePreparation(employeeUID, jobPositionUID, roleUID,
                            Winit.Modules.Auth.Model.Constants.SqlitePreparationStatus.FAILURE, null, "Unable to update MobileAppAction");
                    }
                }
            }
            catch (Exception ex)
            {

                await UpdateSqlitePreparation(employeeUID, jobPositionUID, roleUID,
                           Winit.Modules.Auth.Model.Constants.SqlitePreparationStatus.FAILURE, null, ex.Message);
            }

            return retVal;
        }
        public async Task<Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation> GetDBCreationStatus(string employeeUID, string jobPositionUID, string roleUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"EmployeeUID",  employeeUID},
                {"JobPositionUID",  jobPositionUID},
                {"RoleUID",  roleUID},
            };
                var sql = new StringBuilder(@"SELECT id as Id,uid as UID,emp_uid as EmpUID,
                                            job_position_uid as JobPositionUID,role_uid as RoleUID,status,
                                            sqlite_path as SqlitePath,created_time as CreatedTime,modified_time as ModifiedTime,
                                            error_message as ErrorMessage,server_add_time as ServerAddTime,
                                            server_modified_time as ServerModifiedTime,vehicle_uid as VehicleUID,
                                            start_time as StarTime,end_time as EndTime FROM sqlite_preparation  where emp_uid=@EmployeeUID 
                                            AND job_position_uid=@JobPositionUID AND role_uid=@RoleUID");

                Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation? list = await ExecuteSingleAsync<Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation>(sql.ToString(), parameters);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> UpdateSqlitePreparation(string employeeUID, string jobPositionUID, string roleUID, string status, string path = null, string errorMessage = null)
        {
            int retVal = -1;

            try
            {
                string sqlitePathClause = (status == "Ready" && path != null)
                    ? ", sqlite_path = @SQLitePath"
                    : (status == "In_Process")
                        ? ", sqlite_path = NULL"
                        : "";

                string errorMsgClause = status == "Failure" && !string.IsNullOrEmpty(errorMessage)
                    ? ", error_message = @ErrorMsg"
                    : ", error_message = NULL";

                var query = $@"
    UPDATE sqlite_preparation 
    SET 
        status = @Status,
        modified_time = @ModifiedTime,
        server_modified_time = @ServerModifiedTime
        {sqlitePathClause}
        {errorMsgClause}
    WHERE 
        emp_uid = @EmployeeUID 
        AND job_position_uid = @JobPositionUID 
        AND role_uid = @RoleUID";

                // Prepare the parameters for the query
                var parameters = new Dictionary<string, object>
        {
            { "@Status", status },
            { "@EmployeeUID", employeeUID },
            { "@JobPositionUID", jobPositionUID },
            { "@RoleUID", roleUID },
            { "@ModifiedTime", DateTime.Now },
            { "@ServerModifiedTime", DateTime.Now }
        };

                if (status == "Ready" && path != null)
                {
                    parameters.Add("@SQLitePath", path);
                }

                if (status == "Failure" && !string.IsNullOrEmpty(errorMessage))
                {
                    parameters.Add("@ErrorMsg", errorMessage);
                }

                retVal = await ExecuteNonQueryAsync(query, parameters);
            }
            catch
            {
                throw;
            }

            return retVal;
        }

        private async Task<int> CreateSqlitePreparation(string employeeUID, string jobPositionUID, string roleUID)
        {

            var Query = @"INSERT INTO sqlite_preparation (uid, emp_uid, job_position_uid, role_uid, status, 
                           created_time, modified_time, server_add_time, server_modified_time)
                          VALUES (@UID, @EmployeeUID, @JobPositionUID, @RoleUID, @Status,@CreatedTime, 
                          @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
            var Parameters = new Dictionary<string, object>
                        {
                          { "@UID",  Guid.NewGuid().ToString() },
                          { "@EmployeeUID", employeeUID },
                          { "@JobPositionUID", jobPositionUID },
                          { "@RoleUID", roleUID},
                          { "@Status", "NOT_READY" },
                          { "@CreatedTime", DateTime.Now },
                          { "@ServerAddTime", DateTime.Now },
                          { "@ModifiedTime", DateTime.Now },
                          { "@ServerModifiedTime", DateTime.Now },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
    }
}
