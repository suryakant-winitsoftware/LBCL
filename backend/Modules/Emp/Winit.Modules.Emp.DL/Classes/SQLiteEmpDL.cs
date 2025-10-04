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
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;


using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Classes
{
    public  class SQLiteEmpDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IEmpDL
    {
        public SQLiteEmpDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmpDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUid,
                    code AS Code,
                    name AS Name,
                    alias_name AS AliasName,
                    login_id AS LoginId,
                    emp_no AS EmpNo,
                    auth_type AS AuthType,
                    status AS Status,
                    active_auth_key AS ActiveAuthKey,
                    encrypted_password AS EncryptedPassword
                FROM 
                    emp) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUid,
                    code AS Code,
                    name AS Name,
                    alias_name AS AliasName,
                    login_id AS LoginId,
                    emp_no AS EmpNo,
                    auth_type AS AuthType,
                    status AS Status,
                    active_auth_key AS ActiveAuthKey,
                    encrypted_password AS EncryptedPassword
                FROM 
                    emp) As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmp>().GetType();

                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp> pagedResponse = new PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>
                {
                    PagedData = EmpDetails,
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                    E.id AS Id,
                    E.uid AS Uid,
                    E.created_by AS CreatedBy,
                    E.created_time AS CreatedTime,
                    E.modified_by AS ModifiedBy,
                    E.modified_time AS ModifiedTime,
                    E.server_add_time AS ServerAddTime,
                    E.server_modified_time AS ServerModifiedTime,
                    E.company_uid AS CompanyUid,
                    E.code AS Code,
                    E.name AS Name,
                    E.alias_name AS AliasName,
                    E.login_id AS LoginId,
                    E.emp_no AS EmpNo,
                    E.auth_type AS AuthType,
                    E.status AS Status,
                    E.active_auth_key AS ActiveAuthKey,
                    E.encrypted_password AS EncryptedPassword,
                    FS.relative_path || '/' || FS.file_name AS ProfileImage
                FROM 
                    emp E
                    LEFT JOIN file_sys FS ON FS.linked_item_type = 'emp' AND FS.linked_item_uid = E.uid
                    AND fs.file_sys_type = 'profile' AND fs.file_type = 'Image'
                    WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmp>().GetType();

            Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters, type);
            return EmpDetails;
        }
        public async Task<int> CreateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp createEmp, string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"INSERT INTO Emp (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, code, name, alias_name, login_id, emp_no, auth_type, status, active_auth_key, encrypted_password) VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, @Code, @Name, @AliasName, @LoginId, @EmpNo, @AuthType, @Status, 
                            @ActiveAuthKey, @EncryptedPassword)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createEmp.UID},
                   {"CreatedBy", createEmp.CreatedBy},
                   {"ModifiedBy", createEmp.ModifiedBy},
                   {"Code", createEmp.Code},
                   {"Name", createEmp.Name},
                   {"CompanyUID", createEmp.CompanyUID},
                    { "AliasName" ,createEmp.AliasName},
                    { "LoginId" , createEmp.LoginId },
                    { "EmpNo" , createEmp.EmpNo },
                    { "AuthType" , createEmp.AuthType },
                    { "Status" , createEmp.Status },
                    { "ActiveAuthKey" , createEmp.ActiveAuthKey },
                    { "EncryptedPassword", createEmp.EncryptedPassword },
                   {"CreatedTime", createEmp.CreatedTime},
                   {"ModifiedTime", createEmp.ModifiedTime},
                   {"ServerAddTime", createEmp.ServerAddTime},
                   {"ServerModifiedTime", createEmp.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters, connection, transaction);
                
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp updateEmp, string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {

                var sql = @"UPDATE emp SET
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        company_uid = @CompanyUID,
                        org_uid = @OrgUID,
                        code = @Code,
                        name = @Name,
                        alias_name = @AliasName,
                        login_id = @LoginId,
                        emp_no = @EmpNo,
                        auth_type = @AuthType,
                        status = @Status,
                        active_auth_key = @ActiveAuthKey,
                        encrypted_password = @EncryptedPassword
                    WHERE 
                        uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",updateEmp.UID },
                   {"ModifiedBy", updateEmp.ModifiedBy},
                   {"Code", updateEmp.Code},
                   {"Name", updateEmp.Name},
                   {"CompanyUID", updateEmp.CompanyUID},
                   {"AliasName" ,updateEmp.AliasName},
                   {"LoginId" , updateEmp.LoginId },
                   {"EmpNo" , updateEmp.EmpNo },
                   {"AuthType" , updateEmp.AuthType },
                   {"Status" , updateEmp.Status },
                   {"ActiveAuthKey" , updateEmp.ActiveAuthKey },
                   {"EncryptedPassword", updateEmp.EncryptedPassword },
                   {"ModifiedTime", updateEmp.ModifiedTime},
                   {"ServerModifiedTime", updateEmp.ServerModifiedTime},
                 };
                return await ExecuteNonQueryAsync(sql, parameters,connection,transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteEmp(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM Emp WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters, connection, transaction);
        }


        // niranjan
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByLoginId(string LoginId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"LoginId",  LoginId}
            };
            var sql = @"SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUid,
                    code AS Code,
                    name AS Name,
                    alias_name AS AliasName,
                    login_id AS LoginId,
                    emp_no AS EmpNo,
                    auth_type AS AuthType,
                    status AS Status,
                    active_auth_key AS ActiveAuthKey,
                    encrypted_password AS EncryptedPassword
                FROM 
                    emp WHERE LoginId = @LoginId";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmp>().GetType();

            Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await ExecuteSingleAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters, type);
            return EmpDetails;
        }

        public async Task<Model.Interfaces.IEmpPassword> GetPasswordByLoginId(string LoginId)
        {
            throw new NotImplementedException();
        }

        public Task CheckProcedure()
        {
            throw new NotImplementedException();
        }

        public Task<IEmpInfo> GetEmpInfoByLoginId(string LoginId)
        {
            throw new NotImplementedException();
        }
        public Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllEmpDetailsByOrgUID(string OrgUID)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<IEmp>> GetReportsToEmployeesByRoleUID(string roleUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"roleUID",  roleUID}
            };
            var sql = @"SELECT E.uid as UID, E.code as Code, E.name as Name, E.emp_no as EmpNo, E.login_id AS LoginId
                       FROM roles R
                       INNER JOIN job_position JP ON JP.user_role_uid = R.uid
                       INNER JOIN emp E ON E.uid = JP.emp_uid  where r.uid=@roleUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEmp>().GetType();

           IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql, parameters, type);
            return EmpDetails;
        }

        public Task<List<IEmpView>> GetEmpViewByUID(string empUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<IEmp>> GetEmployeesByRoleUID(string orgUID, string roleUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISelectionItem>> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetASMList(string branchUID)
        {
            try
            {
                var sql = @"select e.* from emp e join job_position j
                                on j.emp_uid=e.uid join roles r on r.uid=j.user_role_uid and r.code='ASM' where j.branch_uid=@branchUID";
                return await ExecuteQueryAsync<Winit.Modules.Emp.Model.Interfaces.IEmp>(sql);
            }
            catch
            {
                throw;
            }
        }

        Task<List<IEmp>> IEmpDL.GetAllASM()
        {
            throw new NotImplementedException();
        }

        Task<List<IEmp>> IEmpDL.GetASMList(string branchUID, string Code)
        {
            throw new NotImplementedException();
        }

        Task<IEmp> IEmpDL.GetBMByBranchUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<List<EscalationMatrixDto>> GetEscalationMatrixAsync(string jobPositionUid)
        {
            throw new NotImplementedException();
        }
    }
}
