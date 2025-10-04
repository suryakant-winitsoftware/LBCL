using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.User.DL.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Constants;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.User.DL.Classes
{
    public class PGSQLMaintainUserDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IMaintainUserDL
    {
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpDL _EmpDL;
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL _empInfoDL;
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpOrgMappingDL _empOrgMappingDL;
        protected readonly Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL _jobPositionDL;
        protected readonly Winit.Modules.FileSys.DL.Interfaces.IFileSysDL _fileSysDL;
        protected readonly Winit.Modules.Role.DL.Interfaces.IRoleDL _roleDL;

        public PGSQLMaintainUserDL(IServiceProvider serviceProvider, IConfiguration config, Emp.DL.Interfaces.IEmpInfoDL empInfoDL, IEmpDL EmpDL,
            Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL jobPositionDL, Role.DL.Interfaces.IRoleDL roleDL, IEmpOrgMappingDL empOrgMappingDL, FileSys.DL.Interfaces.IFileSysDL fileSysDL) : base(serviceProvider, config)
        {
            _empInfoDL = empInfoDL;
            _EmpDL = EmpDL;
            _empOrgMappingDL = empOrgMappingDL;
            _jobPositionDL = jobPositionDL;
            _roleDL = roleDL;
            _fileSysDL = fileSysDL;
        }
        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser>> SelectAllMaintainUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (
                                            select
                                                e.id as Id,
                                                e.uid as UID,
                                                e.code as Code,
                                                e.modified_time as ModifiedTime,
                                                e.name as Name,
                                                e.login_id as LoginId,
                                                jp.org_uid as OrgUID,
                                                case
                                                    when e.auth_type = 'Local' then 'Inline (App)'
                                                    when e.auth_type = 'SSO' then 'SSO (SAML)'
                                                    else e.auth_type
                                                end as AuthType,
                                                e.status as Status,
                                                coalesce(ei.email, 'N/A') as Email,
                                                coalesce(ei.phone, 'N/A') as Phone,
                                                ei.start_date as StartDate,
                                                ei.can_handle_stock as CanHandleStock
                                            from emp e
                                            left join emp_info ei on e.uid = ei.emp_uid
                                            inner join job_position jp on jp.emp_uid=e.uid
                                        ) as sub_query
                                        ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt from (
                                                        select
                                                            e.id as Id,
                                                            e.uid as UID,
                                                            e.code as Code,
                                                            e.modified_time as ModifiedTime,
                                                            e.name as Name,
                                                            e.login_id as LoginId,
                                                            jp.org_uid as OrgUID,
                                                            case
                                                                when e.auth_type = 'Local' then 'Inline (App)'
                                                                when e.auth_type = 'SSO' then 'SSO (SAML)'
                                                                else e.auth_type
                                                            end as AuthType,
                                                            e.status as Status,
                                                            coalesce(ei.email, 'N/A') as Email,
                                                            coalesce(ei.phone, 'N/A') as Phone,
                                                            ei.start_date as StartDate,
                                                            ei.can_handle_stock as CanHandleStock
                                                        from emp e
                                                        left join emp_info ei on e.uid = ei.emp_uid
                                                        inner join job_position jp on jp.emp_uid=e.uid
                                                    ) as sub_query
                                                    ");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.User.Model.Interfaces.IMaintainUser>(filterCriterias, sbFilterCriteria, parameters); ;

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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($@" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IMaintainUser>().GetType();
                IEnumerable<Model.Interfaces.IMaintainUser> maintainUsers = await ExecuteQueryAsync<Model.Interfaces.IMaintainUser>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser> pagedResponse = new PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser>
                {
                    PagedData = maintainUsers,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<int> CUDEmp(Winit.Modules.Emp.Model.Interfaces.IEmp emp, string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;

            try
            {
                switch (emp.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        count = await _EmpDL.CreateEmp(emp, encryptPassword, connection, transaction);
                        break;
                    case Shared.Models.Enums.ActionType.Update:
                        count = await _EmpDL.UpdateEmp(emp, encryptPassword, connection, transaction);
                        break;
                    case Shared.Models.Enums.ActionType.Delete:
                        count = await _EmpDL.DeleteEmp(emp.UID, connection, transaction);
                        break;
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> CUDEmpInfo(Winit.Modules.Emp.Model.Interfaces.IEmpInfo empInfo, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            try
            {
                bool exists = false;
                var existingRec = await _empInfoDL.GetEmpInfoByUID(empInfo.EmpUID, connection, transaction);
                
                switch (empInfo.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        if (existingRec != null)
                        {
                            exists = (existingRec.UID == empInfo.UID);
                        }
                        count = exists ? await _empInfoDL.UpdateEmpInfoDetails(empInfo, connection, transaction) : await _empInfoDL.CreateEmpInfo(empInfo, connection, transaction);
                        break;
                    case Shared.Models.Enums.ActionType.Update:
                        // Handle explicit update action type
                        if (existingRec != null)
                        {
                            count = await _empInfoDL.UpdateEmpInfoDetails(empInfo, connection, transaction);
                        }
                        else
                        {
                            // If record doesn't exist, create it
                            count = await _empInfoDL.CreateEmpInfo(empInfo, connection, transaction);
                        }
                        break;
                    case Shared.Models.Enums.ActionType.Delete:
                        count = await _empInfoDL.DeleteEmpInfoDetails(empInfo.UID, connection, transaction);
                        break;
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> CUDJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;

            try
            {
                Winit.Modules.JobPosition.Model.Interfaces.IJobPosition objJobPosition = await _jobPositionDL.GetJobPositionByUID(jobPosition.UID, connection, transaction);
                if (objJobPosition != null)
                {
                    count += await _jobPositionDL.UpdateJobPosition(jobPosition, connection, transaction);
                }
                else
                {

                    count += await _jobPositionDL.CreateJobPosition(jobPosition, connection, transaction);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<int> CUDEmployee(Winit.Modules.User.Model.Classes.EmpDTOModel empDTO, string encryptPassword)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            count += await CUDEmp(empDTO.Emp, encryptPassword, connection, transaction);
                            count += await CUDEmpInfo(empDTO.EmpInfo, connection, transaction);
                            count += await CUDJobPosition(empDTO.JobPosition, connection, transaction);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<(Winit.Modules.Emp.Model.Interfaces.IEmp, Winit.Modules.Emp.Model.Interfaces.IEmpInfo, Winit.Modules.JobPosition.Model.Interfaces.IJobPosition,
            IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>, Winit.Modules.FileSys.Model.Interfaces.IFileSys)>
            SelectMaintainUserDetailsByUID(string empUID)
        {
            try
            {
                var emp = await _EmpDL.GetEmpByUID(empUID);
                var empInfo = await _empInfoDL.GetEmpInfoByUID(empUID);
                var jobPosition = await _jobPositionDL.SelectJobPositionByEmpUID(empUID);
                var empOrgMapping = await _empOrgMappingDL.GetEmpOrgMappingDetailsByEmpUID(empUID);
                var fileSysList = await _fileSysDL.SelecyFileSysByLinkedItemUID(empUID);
                return (emp, empInfo, jobPosition, empOrgMapping, fileSysList);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles>> SelectUserRolesDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string empUID)
        {
            try
            {
                var sql = new StringBuilder(@"
                                            select distinct
                                                    jp.id as Id,
                                                    jp.uid as UID,
                                                    jp.org_uid as BusinessUnit,
                                                    jp.user_role_uid as Role,
                                                    ur.name as RoleName,
                                                    jp.designation as Designation,
                                                    jp.department as Department,
                                                    case when ur.is_mobile_user = true then 1 else 0 end as RoleType,
                                                    jr.reports_to_job_position_uid as ReportsTo,
                                                    case when r.uid is null then 'N/A' else '[' || r.uid || '] ' || r.name end as ReportsToName,
                                                    l.uid as Location,
                                                    case when l.code is null then 'N/A' else '[' || l.code || '] ' || l.name end as LocationName,
                                                    jp.has_eot as HASEOT,
                                                    case when jp.has_eot = true then 'Yes' else 'No' end as HASEOTstr,
                                                    jp.modified_time as ModifiedTime
                                                from
                                                    job_position jp
                                                join
                                                    job_reporting jr on jp.uid = jr.job_position_uid
                                                left join
                                                    location_mapping lm on jp.uid = lm.linked_item_uid and lm.linked_item_type = 'Emp' and lm.linked_item_uid = jp.emp_uid
                                                left join
                                                    location l on lm.location_uid = l.uid
                                                left join
                                                    job_position jpr on jr.reports_to_job_position_uid = jpr.uid
                                                left join
                                                    emp r on jpr.emp_uid = r.uid
                                                left join
                                                    user_role ur on jp.user_role_uid = ur.uid
                                                where
                                                    jp.emp_uid = @EmpUID;
                                                ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt
                                            from
                                                job_position jp
                                            join
                                                job_reporting jr on jp.uid = jr.job_position_uid
                                            left join
                                                location_mapping lm on jp.uid = lm.linked_item_uid and lm.linked_item_type = 'Emp' and lm.linked_item_uid = jp.emp_uid
                                            left join
                                                location l on lm.location_uid = l.uid
                                            left join
                                                job_position jpr on jr.reports_to_job_position_uid = jpr.uid
                                            left join
                                                emp r on jpr.emp_uid = r.uid
                                            left join
                                                user_role ur on jp.user_role_uid = ur.uid
                                            where
                                                jp.emp_uid = @empUID;
                                            ");
                }
                var parameters = new Dictionary<string, object>()
                {
                    {"@empUID",empUID }
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria<Winit.Modules.User.Model.Interfaces.IUserRoles>(filterCriterias, sbFilterCriteria, parameters);

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
                        sql.Append($@" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IUserRoles>().GetType();
                IEnumerable<Model.Interfaces.IUserRoles> userrolesList = await ExecuteQueryAsync<Model.Interfaces.IUserRoles>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles> pagedResponse = new PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles>
                {
                    PagedData = userrolesList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping>> SelectUserFranchiseeMappingDetails
            (List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string OrgTypeUID, string ParentUID)
        {
            try
            {
                var sql = new StringBuilder(@"select distinct
                                                o.id as Id,
                                                o.uid as OrgUID,
                                                mo.uid as MyOrgUID,
                                                o.code as FranchiseeCode,
                                                o.name as FranchiseeName,
                                                case when mo.org_uid is not null then 1 else 0 end as IsFranchiseeCheck,
                                                case when mo.uid is not null then 1 else 0 end as IsSelected
                                            from
                                                org o
                                            left join
                                                my_orgs mo on o.uid = mo.org_uid and mo.job_position_uid = @JobPositionUID
                                            where
                                                o.org_type_uid = @OrgTypeUID
                                                and o.parent_uid = @ParentUID;
                                            ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt
                                                from
                                                    org o
                                                left join
                                                    my_orgs mo on o.uid = mo.org_uid and mo.job_position_uid = @JobPositionUID
                                                where
                                                    o.org_type_uid = @OrgTypeUID
                                                    and o.parent_uid = @ParentUID;");

                }
                var parameters = new Dictionary<string, object>()
                {
                    {"@JobPositionUID",JobPositionUID },
                    {"@OrgTypeUID",OrgTypeUID },
                    {"@ParentUID",ParentUID },
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" AND ");
                    AppendFilterCriteria<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping>(filterCriterias, sbFilterCriteria, parameters); ;

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
                        sql.Append($@" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IUserFranchiseeMapping>().GetType();
                IEnumerable<Model.Interfaces.IUserFranchiseeMapping> userFranchiseeMappingList = await ExecuteQueryAsync<Model.Interfaces.IUserFranchiseeMapping>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping> pagedResponse = new PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping>
                {
                    PagedData = userFranchiseeMappingList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IUserMaster> GetAllUserMasterDataByLoginID(string LoginID)
        {
            IUserMaster webUser = new UserMaster();
            webUser.Emp = (Emp.Model.Classes.Emp)await _EmpDL.GetEmpByLoginId(LoginID);
            if (webUser.Emp == null)
            {
                return null;
            }
            webUser.JobPosition = (JobPosition.Model.Classes.JobPosition)await _jobPositionDL.SelectJobPositionByEmpUID(webUser.Emp.UID);
            if (webUser.JobPosition == null)
            {
                return null;
            }
            webUser.Role = (Role.Model.Classes.Role)await _roleDL.SelectRolesByUID(webUser.JobPosition.UserRoleUID);
            if (webUser.Role == null)
            {
                return null;
            }
            return webUser;
        }

        public async Task<IEnumerable<IOrg>> GetAplicableOrgs(string orgUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"OrgUID",  orgUID}
                };
                var sql = new StringBuilder(@"SELECT * FROM Org O WHERE O.org_type_uid = 'FR'	 
                                            AND (parent_uid = @OrgUID OR uid = @OrgUID)");
                Type type = _serviceProvider.GetRequiredService<IOrg>().GetType();
                IEnumerable<IOrg> selectionAplicableOrgs = await ExecuteQueryAsync<IOrg>(sql.ToString(), parameters, type);
                return selectionAplicableOrgs;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId)
        {
            try
            {
                dynamic parameters = new { };
                parameters.HierarchyType = hierarchyType;
                parameters.RuleId = ruleId;
                switch (hierarchyType)
                {
                    case UserHierarchyTypeConst.Emp:
                        parameters.HierarchyUID = hierarchyUID;
                        break;
                    case UserHierarchyTypeConst.Store:
                        string sqlStore = "SELECT  asm_emp_uid FROM Address WHERE type = 'Billing' AND linked_item_uid = @HierarchyUID AND is_default = 1;";
                        parameters.HierarchyUID = await ExecuteSingleAsync<string>(sqlStore, new { HierarchyUID = hierarchyUID });
                        break;
                    case UserHierarchyTypeConst.StoreShipTo:
                        string sqlStoreShipTo = "SELECT  asm_emp_uid FROM Address WHERE type = 'Shipping' AND uid = @HierarchyUID AND is_default = 1;";
                        parameters.HierarchyUID = await ExecuteSingleAsync<string>(sqlStoreShipTo, new { HierarchyUID = hierarchyUID });
                        break;
                }

                string sql = """
                    WITH ApplicableRoles AS
                    (
                    SELECT R.code AS RoleCode, R.uid AS RoleUID, AH.level
                    FROM approvalhierarchy AH
                    INNER JOIN roles R ON R.code = AH.approverid
                    WHERE AH.ruleId = @RuleId
                    ),
                    user_hierarchy AS
                    (
                        SELECT JP.user_role_uid, JP.uid, JP.emp_uid, JP.reports_to_uid, 0 AS level_no
                    FROM job_position JP
                    --INNER JOIN ApplicableRoles AR ON AR.RoleUID = JP.user_role_uid
                    WHERE JP.emp_uid = @HierarchyUID
                    UNION ALL
                    SELECT JP.user_role_uid, JP.uid, JP.emp_uid, JP.reports_to_uid, UH.level_no + 1
                        FROM job_position JP
                        INNER JOIN user_hierarchy UH ON UH.reports_to_uid = JP.emp_uid
                    )
                    SELECT AR.RoleCode, UH.emp_uid EmpUID, UH.level_no AS LevelNo,
                    E.Code AS EmpCode, E.Name AS EmpName, AR.level AS ApprovalLevel
                    FROM user_hierarchy UH
                    INNER JOIN Emp E ON E.uid = UH.emp_uid
                    INNER JOIN ApplicableRoles AR ON AR.RoleUID = UH.user_role_uid
                    """;

                return await ExecuteQueryAsync<IUserHierarchy>(sql, parameters);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
