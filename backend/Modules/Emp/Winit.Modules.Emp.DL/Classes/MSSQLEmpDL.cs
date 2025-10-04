using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;


using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Classes
{
    public class MSSQLEmpDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IEmpDL
    {
        public MSSQLEmpDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmpDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                                              modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,
                                              server_modified_time AS ServerModifiedTime,company_uid AS CompanyUID,code AS Code,name AS Name,
                                              alias_name AS AliasName,login_id AS LoginId,emp_no AS EmpNo, auth_type AS AuthType,status AS Status,
                                              active_auth_key AS ActiveAuthKey,encrypted_password AS EncryptedPassword FROM emp)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                                              modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,
                                              server_modified_time AS ServerModifiedTime,company_uid AS CompanyUID,code AS Code,name AS Name,
                                              alias_name AS AliasName,login_id AS LoginId,emp_no AS EmpNo, auth_type AS AuthType,status AS Status,
                                              active_auth_key AS ActiveAuthKey,encrypted_password AS EncryptedPassword FROM emp)as subquery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Emp.Model.Interfaces.IEmp>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
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
                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> empDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp> pagedResponse = new PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>
                {
                    PagedData = empDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByUID(string UID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };
                var sql = $@" SELECT E.id AS Id,E.uid AS UID,E.created_by AS CreatedBy,E.created_time AS CreatedTime,
                    E.modified_by AS ModifiedBy,E.modified_time AS ModifiedTime,E.server_add_time AS ServerAddTime,
                    E.server_modified_time AS ServerModifiedTime,
                    E.company_uid AS CompanyUID,E.code AS Code,E.name AS Name,E.alias_name AS AliasName,E.login_id AS LoginId,E.emp_no AS EmpNo,
                    E.auth_type AS AuthType,E.status AS Status,E.active_auth_key AS ActiveAuthKey,E.encrypted_password AS EncryptedPassword,
                    FS.relative_path + '/' + FS.file_name AS ProfileImage
                    FROM emp E
                    LEFT JOIN file_sys FS ON FS.linked_item_type = '{Winit.Shared.Models.Constants.LinkedItemType.Emp}' AND FS.linked_item_uid = E.uid
                    AND fs.file_sys_type = '{Winit.Shared.Models.Constants.FileSysType.Profile}' AND fs.file_type = '{Winit.Modules.Common.Model.Constants.FileTypeConstants.Image}'
                    where E.uid = @UID";

                Winit.Modules.Emp.Model.Interfaces.IEmp? EmpDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
                return EmpDetails;
            }
            catch
            {
                throw;
            }


        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetBMByBranchUID(string UID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };
                var sql = $@"Select e.code as Code,e.login_id as LoginId,j.branch_uid as CompanyUID from emp e join job_position j
				             on e.uid=j.emp_uid join roles r on r.uid=j.user_role_uid and r.code='BM'
				             where j.branch_uid=@UID";

                Winit.Modules.Emp.Model.Interfaces.IEmp? EmpDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
                return EmpDetails;
            }
            catch
            {
                throw;
            }


        }
        public async Task<int> CreateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp createEmp, string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                createEmp.EncryptedPassword=encryptPassword;
                var sql = @"INSERT INTO emp (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, company_uid, code, name, alias_name, login_id, emp_no, auth_type, status,
                            active_auth_key, encrypted_password) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime, @CompanyUID, @Code, @Name, @AliasName, @LoginId, @EmpNo,
                            @AuthType, @Status, @ActiveAuthKey, @EncryptedPassword);";
                retVal= await ExecuteNonQueryAsync(sql, connection, transaction, createEmp);

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;

        }
        public async Task<int> UpdateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp updateEmp, string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var sql = @"UPDATE emp SET modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime, company_uid = @CompanyUID,
                            code = @Code, name = @Name, alias_name = @AliasName, login_id = @LoginId,
                            emp_no = @EmpNo, auth_type = @AuthType, status = @Status, active_auth_key = @ActiveAuthKey,
                            encrypted_password = @EncryptedPassword WHERE uid = @UID;";
                retVal= await ExecuteNonQueryAsync(sql, connection, transaction, updateEmp);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        public async Task<int> DeleteEmp(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM emp WHERE uid = @UID";

            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        public async Task<IEmp> GetEmpByLoginId(string LoginId)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"LoginId",  LoginId}
            };
            var sql = @"SELECT  id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                    modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUID,code AS Code,name AS Name,alias_name AS AliasName,login_id AS LoginId,emp_no AS EmpNo,
                    auth_type AS AuthType,status AS Status,active_auth_key AS ActiveAuthKey,encrypted_password AS EncryptedPassword FROM emp
                    WHERE LOWER(login_id) = LOWER(@LoginId)";
            Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
            return EmpDetails;
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmpPassword> GetPasswordByLoginId(string LoginId)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"LoginId",  LoginId}
            };
            var sql = @"SELECT uid AS EmpUID, encrypted_password AS EncryptedPassword
                        FROM emp  
                        WHERE LOWER(login_id) = LOWER(@LoginId)";
            return await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmpPassword>(sql, parameters);
        }
        public async Task CheckProcedure()
        {
            try
            {
                var input_data = new List<(string, string, DateTime, string)>
                {
                    ("sales_order_uid_1", "route_uid_1", new DateTime(2024, 2, 6), "action_type_1"),
                    ("sales_order_uid_2", "route_uid_2", new DateTime(2024, 2, 7), "action_type_2"),
                    ("sales_order_uid_3", "route_uid_3", new DateTime(2024, 2, 8), "action_type_3")
                };
                // SQL statement to call the function
                string sql = "DO $$" +
                             "DECLARE " +
                             "    input_data udt_order_assignment[]; " +
                             "    out_value INTEGER := 0; " +
                             "BEGIN " +
                             "    input_data := @input_data::udt_order_assignment[]; " +
                             "    CALL public.perform_order_assignment(input_data, out_value); " +
                             "    RAISE NOTICE 'out_param1 value: %', out_value; " +
                             "END $$";

                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                       {"input_data", input_data}
                 };
                //await ExecuteStoredProcedureAsync("public.perform_order_assignment", parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEmpInfo> GetEmpInfoByLoginId(string LoginId)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"LoginId",  LoginId}
                };
                var sql = @"SELECT 
                        EI.id AS Id,
                        EI.uid AS UID,
                        EI.created_by AS CreatedBy,
                        EI.created_time AS CreatedTime,
                        EI.modified_by AS ModifiedBy,
                        EI.modified_time AS ModifiedTime,
                        EI.server_add_time AS ServerAddTime,
                        EI.server_modified_time AS ServerModifiedTime,
                        EI.emp_uid AS EmpUID,
                        EI.email AS Email,
                        EI.phone AS Phone,
                        EI.start_date AS StartDate,
                        EI.end_date AS EndDate,
                        EI.can_handle_stock AS CanHandleStock,
                        EI.ad_group AS ADGroup,
                        EI.ad_username AS ADUsername
                    FROM 
                        emp E
                    INNER JOIN 
                        emp_info EI ON E.uid = EI.emp_uid

                                            WHERE LOWER(E.Login_Id) = LOWER(@LoginId)";
                return await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllEmpDetailsByOrgUID(string OrgUID)
        {
            try
            {
                var sql = $@"SELECT 
                        E.id AS Id,
                        E.uid AS UID,
                        E.created_by AS CreatedBy,
                        E.created_time AS CreatedTime,
                        E.modified_by AS ModifiedBy,
                        E.modified_time AS ModifiedTime,
                        E.server_add_time AS ServerAddTime,
                        E.server_modified_time AS ServerModifiedTime,
                        E.company_uid AS CompanyUID,
                        E.code AS Code,
                        E.name AS Name,
                        E.alias_name AS AliasName,
                        E.login_id AS LoginId,
                        E.emp_no AS EmpNo,
                        E.auth_type AS AuthType,
                        E.status AS Status,
                        E.active_auth_key AS ActiveAuthKey,
                        E.encrypted_password AS EncryptedPassword
                    FROM emp E
                    INNER JOIN job_position J 
                        ON J.emp_uid = E.uid 
                        AND J.org_uid = '{OrgUID}'";

                List<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql);
                return EmpDetails;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<IEmp>> GetReportsToEmployeesByRoleUID(string roleUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"roleUID",  roleUID}
            };

            var sql =
                """
                    WITH RoleHierarchy AS (
                    -- Anchor member: Start with the initial role specified by @roleUID
                    SELECT 
                        R.uid AS RoleUID,
                        R.parent_role_uid AS ParentRoleUID
                    FROM roles R
                    WHERE R.uid = @roleUID
                
                    UNION ALL
                
                    -- Recursive member: Join roles to get the parent hierarchy
                    SELECT 
                        R.uid AS RoleUID,
                        R.parent_role_uid AS ParentRoleUID
                    FROM roles R
                    INNER JOIN RoleHierarchy RH ON R.uid = RH.ParentRoleUID
                )
                SELECT 
                    E.uid AS UID, 
                    E.code AS Code, 
                    E.name AS Name, 
                    E.emp_no AS EmpNo, 
                    E.login_id AS LoginId,
                    JP.uid AS JobPositionUid
                FROM RoleHierarchy RH
                INNER JOIN job_position JP ON JP.user_role_uid = RH.RoleUID AND RH.RoleUID != @roleUID
                INNER JOIN emp E ON E.uid = JP.emp_uid
                WHERE EXISTS (
                    -- Ensures we only get users within roles that have 'is_for_reports_to' set to 1 in the roles table
                    SELECT 1
                    FROM roles R
                    WHERE R.uid = RH.RoleUID AND R.is_for_reports_to = 1
                )
                ORDER BY E.name;
                """;
            IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
            return EmpDetails;
        }
        public async Task<List<IEmp>> GetEmployeesByRoleUID(string orgUID, string roleUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"RoleUID",  roleUID},
                {"orgUID",  orgUID},
            };
            var sql = @"SELECT 
                    E.uid AS UID, 
                    E.code AS Code, 
                    E.name AS Name, 
                    E.emp_no AS EmpNo, 
                    E.login_id AS LoginId
                    FROM job_position JP 
                    INNER JOIN  emp E ON E.uid = JP.emp_uid 
                    where JP.user_role_uid=@RoleUID AND JP.org_uid = @orgUID";
            List<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails =
                await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
            return EmpDetails;
        }
        public async Task<List<ISelectionItem>> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"RoleUID",  roleUID},
                {"orgUID",  orgUID},
            };
            var sql = @"SELECT 
                    JP.uid AS UID, 
                    E.code AS Code, 
                    E.name AS Label  
                    FROM job_position JP 
                    INNER JOIN  emp E ON E.uid = JP.emp_uid 
                    where JP.user_role_uid=@RoleUID AND JP.org_uid = @orgUID";
            List<ISelectionItem> EmpDetails =
                await ExecuteQueryAsync<ISelectionItem>(sql, parameters);
            return EmpDetails;
        }
        public async Task<List<IEmpView>> GetEmpViewByUID(string empUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"empUID",  empUID}
            };
            var sql = @"SELECT 
                        EI.phone AS Phone,
                        E.id AS Id,
                        E.uid AS UID,
                        E.created_by AS CreatedBy,
                        E.created_time AS CreatedTime,
                        E.modified_by AS ModifiedBy,
                        E.modified_time AS ModifiedTime,
                        E.server_add_time AS ServerAddTime,
                        E.server_modified_time AS ServerModifiedTime,
                        E.company_uid AS CompanyUID,
                        E.code AS Code,
                        E.name AS Name,
                        E.alias_name AS AliasName,
                        E.login_id AS LoginId,
                        E.emp_no AS EmpNo,
                        E.auth_type AS AuthType,
                        E.status AS Status,
                        E.activauth_key AS ActiveAuthKey,
                        E.encrypted_password AS EncryptedPassword
                    FROM 
                        emp_info EI
                    INNER JOIN 
                        emp E ON EI.emp_uid = E.uid;
                     WHERE E.uid = @empUID";
            List<IEmpView> EmpDetails = await ExecuteQueryAsync<IEmpView>(sql, parameters);
            return EmpDetails;
        }

        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetASMList(string branchUID, string Code)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"branchUID",  branchUID},
                {"Code",  Code}
            };
                var sql = @"select e.* from emp e join job_position j
                                on j.emp_uid=e.uid join roles r on r.uid=j.user_role_uid and r.code=@Code where j.branch_uid=@branchUID";
                return await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM()
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                };
                var sql = @"select e.* from emp e join job_position j
                                on j.emp_uid=e.uid join roles r on r.uid=j.user_role_uid and r.code='ASM'";
                return await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        public Task<List<EscalationMatrixDto>> GetEscalationMatrixAsync(string jobPositionUid)
        {
            throw new NotImplementedException();
        }
    }
}
