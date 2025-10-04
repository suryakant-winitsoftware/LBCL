using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Constants;
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
    public class MSSQLMaintainUserDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IMaintainUserDL
    {
        private readonly IApprovalEngineHelper _approvalEngineHelper;
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpDL _EmpDL;
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL _empInfoDL;
        protected readonly Winit.Modules.Emp.DL.Interfaces.IEmpOrgMappingDL _empOrgMappingDL;
        protected readonly Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL _jobPositionDL;
        protected readonly Winit.Modules.FileSys.DL.Interfaces.IFileSysDL _fileSysDL;
        protected readonly Winit.Modules.Role.DL.Interfaces.IRoleDL _roleDL;
        protected readonly Winit.Modules.ApprovalEngine.DL.Interfaces.IApprovalEngineDL _approvalEngineDL;

        public MSSQLMaintainUserDL(IServiceProvider serviceProvider, IConfiguration config, Emp.DL.Interfaces.IEmpInfoDL empInfoDL, IEmpDL EmpDL,
            Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL jobPositionDL, Role.DL.Interfaces.IRoleDL roleDL, IEmpOrgMappingDL empOrgMappingDL, FileSys.DL.Interfaces.IFileSysDL fileSysDL, ApprovalEngine.DL.Interfaces.IApprovalEngineDL approvalEngineDL, IApprovalEngineHelper approvalEngineHelper) : base(serviceProvider, config)
        {
            _approvalEngineHelper = approvalEngineHelper;
            _empInfoDL = empInfoDL;
            _EmpDL = EmpDL;
            _empOrgMappingDL = empOrgMappingDL;
            _jobPositionDL = jobPositionDL;
            _roleDL = roleDL;
            _fileSysDL = fileSysDL;
            _approvalEngineDL=approvalEngineDL;
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
                IEnumerable<Model.Interfaces.IMaintainUser> maintainUsers = await ExecuteQueryAsync<Model.Interfaces.IMaintainUser>(sql.ToString(), parameters);
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
                        if (count>0)
                        {
                            if ((emp.ApprovalStatus == ApprovalConst.Approved) || (emp.ApprovalStatus == ApprovalConst.Rejected))
                            {
                                _approvalEngineHelper.UpdateApprovalStatus(emp.ApprovalStatusUpdate);
                            }
                        }
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

        private async Task<int> CUDJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null,
                                               IDbTransaction? transaction = null)
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
                using (var connection = CreateConnection())
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
                var empTask = _EmpDL.GetEmpByUID(empUID);
                var empInfoTask = _empInfoDL.GetEmpInfoByUID(empUID);
                var jobPositionTask = _jobPositionDL.SelectJobPositionByEmpUID(empUID);
                var empOrgMappingTask = _empOrgMappingDL.GetEmpOrgMappingDetailsByEmpUID(empUID);
                var fileSysListTask = _fileSysDL.SelecyFileSysByLinkedItemUID(empUID);
                await Task.WhenAll(empTask, empInfoTask, jobPositionTask, empOrgMappingTask, fileSysListTask);
                var emp = await empTask;
                var empInfo = await empInfoTask;
                var jobPosition = await jobPositionTask;
                var empOrgMapping = await empOrgMappingTask;
                var fileSysList = await fileSysListTask;
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
                var sql = new StringBuilder(@"SELECT * FROM
                                            (SELECT DISTINCT
                                            jp.id AS Id,
                                            jp.uid AS UID,
                                            jp.org_uid AS BusinessUnit,
                                            jp.user_role_uid AS Role,
                                            ur.name AS RoleName,
                                            jp.designation AS Designation,
                                            jp.department AS Department,
                                            CASE WHEN ur.is_mobile_user = 1 THEN 1 ELSE 0 END AS RoleType,
                                            jr.reports_to_job_position_uid AS ReportsTo,
                                            CASE WHEN r.uid IS NULL THEN 'N/A' ELSE '[' + r.uid + '] ' + r.name END AS ReportsToName,
                                            l.uid AS Location,
                                            CASE WHEN l.code IS NULL THEN 'N/A' ELSE '[' + l.code + '] ' + l.name END AS LocationName,
                                            jp.has_eot AS HASEOT,
                                            CASE WHEN jp.has_eot = 1 THEN 'Yes' ELSE 'No' END AS HASEOTstr
                                            jp.modified_time AS ModifiedTime
                                        FROM
                                            job_position jp
                                        JOIN
                                            job_reporting jr ON jp.uid = jr.job_position_uid
                                        LEFT JOIN
                                            location_mapping lm ON jp.uid = lm.linked_item_uid 
                                                                AND lm.linked_item_type = 'Emp' 
                                                                AND lm.linked_item_uid = jp.emp_uid
                                        LEFT JOIN
                                            location l ON lm.location_uid = l.uid
                                        LEFT JOIN
                                            job_position jpr ON jr.reports_to_job_position_uid = jpr.uid
                                        LEFT JOIN
                                            emp r ON jpr.emp_uid = r.uid
                                        LEFT JOIN
                                            user_role ur ON jp.user_role_uid = ur.uid
                                        WHERE
                                            jp.emp_uid = @EmpUID)As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt
                                            FROM
                                            (SELECT DISTINCT
                                            jp.id AS Id,
                                            jp.uid AS UID,
                                            jp.org_uid AS BusinessUnit,
                                            jp.user_role_uid AS Role,
                                            ur.name AS RoleName,
                                            jp.designation AS Designation,
                                            jp.department AS Department,
                                            CASE WHEN ur.is_mobile_user = 1 THEN 1 ELSE 0 END AS RoleType,
                                            jr.reports_to_job_position_uid AS ReportsTo,
                                            CASE WHEN r.uid IS NULL THEN 'N/A' ELSE '[' + r.uid + '] ' + r.name END AS ReportsToName,
                                            l.uid AS Location,
                                            CASE WHEN l.code IS NULL THEN 'N/A' ELSE '[' + l.code + '] ' + l.name END AS LocationName,
                                            jp.has_eot AS HASEOT,
                                            CASE WHEN jp.has_eot = 1 THEN 'Yes' ELSE 'No' END AS HASEOTstr
                                            jp.modified_time AS ModifiedTime
                                        FROM
                                            job_position jp
                                        JOIN
                                            job_reporting jr ON jp.uid = jr.job_position_uid
                                        LEFT JOIN
                                            location_mapping lm ON jp.uid = lm.linked_item_uid 
                                                                AND lm.linked_item_type = 'Emp' 
                                                                AND lm.linked_item_uid = jp.emp_uid
                                        LEFT JOIN
                                            location l ON lm.location_uid = l.uid
                                        LEFT JOIN
                                            job_position jpr ON jr.reports_to_job_position_uid = jpr.uid
                                        LEFT JOIN
                                            emp r ON jpr.emp_uid = r.uid
                                        LEFT JOIN
                                            user_role ur ON jp.user_role_uid = ur.uid
                                        WHERE
                                            jp.emp_uid = @EmpUID)As SubQuery ");
                }
                var parameters = new Dictionary<string, object>()
                {
                    {"@empUID",empUID }
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" Where ");
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
                IEnumerable<Model.Interfaces.IUserRoles> userrolesList = await ExecuteQueryAsync<Model.Interfaces.IUserRoles>(sql.ToString(), parameters);
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
                var sql = new StringBuilder(@"Select * From (select distinct
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
                                                and o.parent_uid = @ParentUID)As SubQuery
                                            ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt
                                                (select distinct
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
                                                and o.parent_uid = @ParentUID)As SubQuery ;");

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
                    sbFilterCriteria.Append(" Where ");
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
                IEnumerable<Model.Interfaces.IUserFranchiseeMapping> userFranchiseeMappingList = await ExecuteQueryAsync<Model.Interfaces.IUserFranchiseeMapping>(sql.ToString(), parameters);
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
            webUser.Emp = await _EmpDL.GetEmpByLoginId(LoginID);
            if (webUser.Emp == null)
            {
                return null;
            }
            webUser.JobPosition = await _jobPositionDL.SelectJobPositionByEmpUID(webUser.Emp.UID);
            if (webUser.JobPosition == null)
            {
                return null;
            }
            webUser.Role = await _roleDL.SelectRolesByUID(webUser.JobPosition.UserRoleUID);
            if (webUser.Role == null)
            {
                return null;
            }
            webUser.ApprovalRuleMaster = await _approvalEngineDL.GetApprovalRuleMasterData();
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
                                            AND (O.parent_uid = @OrgUID OR O.uid = @OrgUID)");
                IEnumerable<IOrg> selectionAplicableOrgs = await ExecuteQueryAsync<IOrg>(sql.ToString(), parameters);
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
                IDictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"HierarchyType", hierarchyType},
                    {"RuleId", ruleId }
                };
                switch (hierarchyType)
                {
                    case UserHierarchyTypeConst.Emp:
                        parameters["HierarchyUID"] = hierarchyUID;
                        break;
                    case UserHierarchyTypeConst.StoreBM:
                        string sqlStoreBM = """
                            SELECT JP.emp_uid FROM job_position JP
                            INNER JOIN store S ON S.uid = @HierarchyUID 
                            INNER JOIN address A ON A.linked_item_type = 'store' AND A.type = 'Billing' 
                            AND A.is_default = 1 AND A.linked_item_uid = @HierarchyUID 
                            AND JP.branch_uid = A.branch_uid
                            AND JP.user_role_uid = CASE WHEN S.broad_classification = 'Service' THEN 'BSEM' ELSE 'BM' END 
                            """;
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStoreBM, new { HierarchyUID = hierarchyUID });
                        break;
                    case UserHierarchyTypeConst.Store:
                        string sqlStore = "SELECT  reporting_emp_uid FROM store WHERE uid = @HierarchyUID;";
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStore, new { HierarchyUID = hierarchyUID });
                        break;
                    case UserHierarchyTypeConst.StoreShipTo:
                        string sqlStoreShipTo = "SELECT  asm_emp_uid FROM Address WHERE type = 'Shipping' AND uid = @HierarchyUID AND is_default = 1;";
                        parameters["HierarchyUID"] = await ExecuteSingleAsync<string>(sqlStoreShipTo, new { HierarchyUID = hierarchyUID });
                        break;
                }

                string sql = @"
                    WITH 
                    ApplicableRoles AS
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
                        INNER JOIN user_hierarchy UH ON UH.reports_to_uid = /*JP.emp_uid*/ JP.uid
                    )
                    SELECT T.RoleCode, T.EmpUID, AR.level LevelNo, T.EmpCode, T.EmpName 
                    FROM ApplicableRoles AR
                    INNER JOIN (
                    SELECT R.uid AS RoleUID,R.code AS RoleCode, JP.emp_uid AS EmpUID, 0 AS LevelNo,
                    E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
                    FROM job_position JP
                    INNER JOIN roles R ON R.uid = JP.user_role_uid and R.is_branch_applicable = 1
                    AND R.uid IN ('BC','BSEM') 
                    AND JP.branch_uid IN (SELECT branch_uid FROM job_position where emp_uid = @HierarchyUID)
                    INNER JOIN Emp E ON E.uid = JP.emp_uid
                    UNION
                    SELECT R.uid AS RoleUID,R.code AS RoleCode, JP.emp_uid AS EmpUID, 0 AS LevelNo,
                    E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
                    FROM job_position JP
                    INNER JOIN roles R ON R.uid = JP.user_role_uid
                    INNER JOIN Emp E ON E.uid = JP.emp_uid 
                    --AND R.is_admin = 1
                    AND jp.user_role_uid IN ('HOTAX','APF','FC','CH','CFO') 
                    UNION
                    SELECT R.uid AS RoleUID,R.Code RoleCode, UH.emp_uid EmpUID, UH.level_no AS LevelNo,
                    E.Code AS EmpCode, E.Name AS EmpName--, AR.level AS ApprovalLevel
                    FROM user_hierarchy UH
                    INNER JOIN roles R ON R.uid = UH.user_role_uid
                    INNER JOIN Emp E ON E.uid = UH.emp_uid AND UH.user_role_uid NOT IN ('BC','BSEM','HOTAX','APF','FC','CH','CFO') 
                    ) T ON T.RoleUID = AR.RoleUID
                    ";

                return await ExecuteQueryAsync<IUserHierarchy>(sql, parameters);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
