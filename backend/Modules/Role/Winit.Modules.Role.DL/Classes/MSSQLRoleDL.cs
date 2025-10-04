using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.DL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Role.DL.Classes
{
    public class MSSQLRoleDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IRoleDL
    {
        public MSSQLRoleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<int> UpdateRoles(IRole role)
        {
            try
            {
                var sql = @"UPDATE roles
                    SET org_uid = @org_uid,
                        role_name_en = @role_name_en,
                        code=@code,
                        role_name_other = @role_name_other,
                        parent_role_uid = @parent_role_uid,
                        is_principal_role = @is_principal_role,
                        is_distributor_role = @is_distributor_role,
                        is_admin = @is_admin,
                        bu_to_dist_access = @bu_to_dist_access,
                        is_app_user = @is_app_user,
                        is_web_user = @is_web_user,
                        is_active = @is_active,
                        ss = @ss,
                        created_by = @created_by,
                        modified_by = @modified_by,
                        created_time = @created_time,
                        modified_time = @modified_time,
                        server_add_time = @server_add_time,
                        server_modified_time = @server_modified_time,
                        have_warehouse=@have_warehouse,
                        have_vehicle=@have_vehicle,
                        is_for_reports_to=@is_for_reports_to,
                        has_p1_access=@has_p1_access,
                        has_p2_access=@has_p2_access,
                        has_p3_access=@has_p3_access,
                        has_msp_access=@has_msp_access,
                        has_margin_access=@has_margin_access
                       WHERE uid = @uid;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"uid" , role.UID},
                 {"org_uid" , role.OrgUid},
                 {"role_name_en" , role.RoleNameEn},
                 {"role_name_other" , role.RoleNameOther},
                 {"parent_role_uid" , role.ParentRoleUid},
                 {"is_principal_role" , role.IsPrincipalRole},
                 {"is_distributor_role" , role.IsDistributorRole},
                 {"is_admin" , role.IsAdmin},
                 {"bu_to_dist_access" , role.BuToDistAccess},
                 {"is_app_user" , role.IsAppUser},
                 {"is_web_user" , role.IsWebUser},
                 {"is_active" , role.IsActive},
                 {"code" , role.Code},
                 {"ss" , role.SS},
                 {"created_by" , role.CreatedBy},
                 {"modified_by" , role.ModifiedBy},
                 {"created_time" , role.CreatedTime},
                 {"modified_time" , role.ModifiedTime},
                 {"server_add_time" , role.ServerAddTime},
                 {"server_modified_time" , role.ServerModifiedTime},
                 {"have_vehicle" , role.HaveVehicle},
                 {"have_warehouse" , role.HaveWarehouse},
                 {"is_for_reports_to" , role.IsForReportsTo},
                 {"has_p1_access" , role.HasP1Access},
                 {"has_p2_access" , role.HasP2Access},
                 {"has_p3_access" , role.HasP3Access},
                 {"has_msp_access" , role.HasMSPAccess},
                 {"has_margin_access" , role.HasMarginAccess},
            };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CreateRoles(IRole role)
        {
            try
            {
                var sql = @"insert into roles(uid,org_uid,code,role_name_en,role_name_other,parent_role_uid,is_principal_role,is_distributor_role,is_admin,bu_to_dist_access,
				 is_app_user,is_web_user,is_active,ss,created_by,modified_by,created_time,modified_time,server_add_time,server_modified_time,have_warehouse,
            have_vehicle,is_for_reports_to,has_p1_access,has_p2_access,has_p3_access,has_margin_access,has_msp_access)
			values(@uid,@org_uid,@code,@role_name_en,@role_name_other,@parent_role_uid,@is_principal_role,@is_distributor_role,@is_admin,@bu_to_dist_access,
				 @is_app_user,@is_web_user,@is_active,@ss,@created_by,@modified_by,@created_time,@modified_time,@server_add_time,@server_modified_time,
                @have_warehouse,@have_vehicle,@is_for_reports_to,@has_p1_access,@has_p2_access,@has_p3_access,@has_margin_access,@has_msp_access)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"uid" , role.UID},
                 {"org_uid" , role.OrgUid},
                 {"code" , role.Code},
                 {"role_name_en" , role.RoleNameEn},
                 {"role_name_other" , role.RoleNameOther},
                 {"parent_role_uid" , role.ParentRoleUid},
                 {"is_principal_role" , role.IsPrincipalRole},
                 {"is_admin" , role.IsAdmin},
                 {"bu_to_dist_access" , role.BuToDistAccess},
                 {"is_app_user" , role.IsAppUser},
                 {"is_web_user" , role.IsWebUser},
                 {"is_active" , role.IsActive},
                 {"ss" , role.SS},
                 {"created_by" , role.CreatedBy},
                 {"modified_by" , role.ModifiedBy},
                 {"created_time" , role.CreatedTime},
                 {"modified_time" , role.ModifiedTime},
                 {"server_add_time" , role.ServerAddTime},
                 {"server_modified_time" , role.ServerModifiedTime},
                 {"is_distributor_role" , role.IsDistributorRole},
                 {"have_warehouse" , role.HaveWarehouse},
                 {"have_vehicle" , role.HaveVehicle},
                 {"is_for_reports_to" , role.IsForReportsTo},
                  {"has_p1_access" , role.HasP1Access},
                 {"has_p2_access" , role.HasP2Access},
                 {"has_p3_access" , role.HasP3Access},
                  {"has_msp_access" , role.HasMSPAccess},
                 {"has_margin_access" , role.HasMarginAccess},
            };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResponse<IRole>> SelectAllRole(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new StringBuilder(@"select * from(SELECT 
                                                            r.id AS Id,
                                                            r.code AS Code,
                                                            r.uid AS Uid,
                                                            r.org_uid AS OrgUid,
                                                            r.role_name_en AS RoleNameEn,
                                                            r.role_name_other AS RoleNameOther,
                                                            r.parent_role_uid AS ParentRoleUid,
	                                                        rs.role_name_en as ParentRoleName,
                                                            r.is_principal_role AS IsPrincipalRole,
                                                            r.is_distributor_role AS IsDistributorRole,
                                                            r.is_admin AS IsAdmin,
                                                            r.bu_to_dist_access AS BuToDistAccess,
                                                            r.is_web_user AS IsWebUser,
                                                            r.is_app_user AS IsAppUser,
                                                            r.is_active AS IsActive,
                                                            r.ss AS Ss,
                                                            r.created_by AS CreatedBy,
                                                            r.modified_by AS ModifiedBy,
                                                            r.created_time AS CreatedTime,
                                                            r.modified_time AS ModifiedTime,
                                                            r.server_add_time AS ServerAddTime,
                                                            r.server_modified_time AS ServerModifiedTime,
                                                            r.have_warehouse as HaveWareHouse,
                                                            r.have_vehicle as HaveVehicle,
                                                            r.is_for_reports_to as IsForReportsTo, 
                                                            r.has_p1_access as HasP1Access, 
                                                            r.has_p2_access as HasP2Access, 
                                                            r.has_p3_access as HasP3Access,
                                                            r.has_msp_access,
                                                            r.has_margin_access
                                                        FROM 
                                                            roles r
                                                        Left JOIN 
                                                            roles rs ON rs.uid = r.parent_role_uid)as SUBQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                            r.id AS Id,
                                                            r.code AS Code,
                                                            r.uid AS Uid,
                                                            r.org_uid AS OrgUid,
                                                            r.role_name_en AS RoleNameEn,
                                                            r.role_name_other AS RoleNameOther,
                                                            r.parent_role_uid AS ParentRoleUid,
	                                                        rs.role_name_en as ParentRoleName,
                                                            r.is_principal_role AS IsPrincipalRole,
                                                            r.is_distributor_role AS IsDistributorRole,
                                                            r.is_admin AS IsAdmin,
                                                            r.bu_to_dist_access AS BuToDistAccess,
                                                            r.is_web_user AS IsWebUser,
                                                            r.is_app_user AS IsAppUser,
                                                            r.is_active AS IsActive,
                                                            r.ss AS Ss,
                                                            r.created_by AS CreatedBy, 
                                                            r.modified_by AS ModifiedBy,
                                                            r.created_time AS CreatedTime,
                                                            r.modified_time AS ModifiedTime,
                                                            r.server_add_time AS ServerAddTime,
                                                            r.server_modified_time AS ServerModifiedTime,
                                                            r.is_for_reports_to as IsForReportsTo,
                                                            r.has_p1_access as HasP1Access, 
                                                            r.has_p2_access as HasP2Access, 
                                                            r.has_p3_access as HasP3Access ,
                                                            r.has_msp_access,
                                                            r.has_margin_access
                                                        FROM 
                                                            roles r
                                                        Left JOIN 
                                                            roles rs ON rs.uid = r.parent_role_uid)as SUBQuery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IRole>(filterCriterias, sbFilterCriteria, parameters);

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
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                List<IRole> roles = await ExecuteQueryAsync<IRole>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IRole> pagedResponse = new PagedResponse<IRole>
                {
                    PagedData = roles,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch
            {
                throw;
            }
        }
        public async Task<ModulesMasterView> GetAllModulesMaster(string platform)
        {

            try
            {
                ModulesMasterView modulesMasterView = new();
                modulesMasterView.Modules = new();
                modulesMasterView.SubModules = new();
                modulesMasterView.SubSubModules = new();
                var parameteres = new Dictionary<string, object>()
                {
                    {"platform",platform }
                };

                var sql = @$"select * from modules where show_in_menu=1 and platform=@platform order by serial_no;
;

                            select smd.* from modules md 
                            inner join sub_modules smd on md.uid=smd.module_uid and smd.show_in_menu=1
                            where platform=@platform  order by smd.serial_no;

                            select ssmd.* from modules md 
                            inner join sub_modules smd on md.uid=smd.module_uid 
                            inner join sub_sub_modules ssmd on smd.uid =ssmd.sub_module_uid	 and ssmd.show_in_menu=1
                            where platform=@platform  order by ssmd.serial_no;";

                DataSet ds = await ExecuteQueryDataSetAsync(sql, parameteres);

                if (ds != null)
                {
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables.Count >= 1)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    modulesMasterView.Modules.Add(ConvertDataTableToObject<Module>(row));
                                }
                            }
                        }
                        if (ds.Tables.Count >= 2)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {
                                    modulesMasterView.SubModules.Add(ConvertDataTableToObject<SubModule>(row));
                                }
                            }
                        }
                        if (ds.Tables.Count >= 3)
                        {
                            if (ds.Tables[2].Rows.Count > 0)
                            {
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {
                                    modulesMasterView.SubSubModules.Add(ConvertDataTableToObject<SubSubModules>(row));
                                }
                            }
                        }
                    }
                }
                return modulesMasterView;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.Role.Model.Interfaces.IModule>> SelectAllmodules(string platform)
        {
            try
            {
                var parameteres = new Dictionary<string, object>()
                {
                    {"platform",platform }
                };

                var sql = @$"SELECT id, uid, module_name_en, module_name_other, serial_no, platform, show_in_menu, is_for_distributor, is_for_principal, ss, created_by, modified_by, created_time, modified_time, server_add_time, server_modified_time
	                            FROM modules where platform=@platform and show_in_menu=1 order by serial_no;";
                List<Winit.Modules.Role.Model.Interfaces.IModule> modules = await ExecuteQueryAsync<Winit.Modules.Role.Model.Interfaces.IModule>(sql, parameteres);
                return modules;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<ISubModule>> SelectAllSubModules(string platform)
        {
            try
            {

                var parameteres = new Dictionary<string, object>()
                {
                    {"platform",platform }
                };
                var sql = $@"select smd.* from modules md 
                            inner join sub_modules smd on md.uid=smd.module_uid where platform=@platform and smd.show_in_menu=1 order by smd.serial_no";
                List<ISubModule> subModules = await ExecuteQueryAsync<ISubModule>(sql.ToString(), parameteres);
                return subModules;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<ISubSubModules>> SelectAllSubSubModules(string platform)
        {
            try
            {
                var parameteres = new Dictionary<string, object>()
                {
                    {"platform",platform }
                };
                var sql = @$"select ssmd.* from modules md 
                            inner join sub_modules smd on md.uid=smd.module_uid 
                            inner join sub_sub_modules ssmd on smd.uid =ssmd.sub_module_uid	
                            where platform=@platform and ssmd.show_in_menu=1 order by ssmd.serial_no;";
                List<ISubSubModules> subsubModules = await ExecuteQueryAsync<ISubSubModules>(sql.ToString(), parameteres);
                return subsubModules;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<IPermission>> SelectAllPrincipalPermissions()
        {
            try
            {
                var sql = @"select * from principal_permissions";
                List<IPermission> permissions = await ExecuteQueryAsync<IPermission>(sql.ToString());
                return permissions;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<IPermission>> SelectAllnormalPermissions()
        {
            try
            {
                var sql = @"select * from normal_permissions";
                List<IPermission> permissions = await ExecuteQueryAsync<IPermission>(sql.ToString());
                return permissions;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<IPermission>> SelectAllPermissions(string roleUID, string platform, bool isPrincipalTypePermission)
        {
            try
            {
                var parametres = new Dictionary<string, object>()
                {
                    {"roleUID",roleUID },
                    {"platform",platform },
                };
                var sql1 = @$"select * from principal_permissions where role_uid=@roleUID and platform=@platform";
                var sql2 = @$"select * from normal_permissions where role_uid=@roleUID and platform=@platform";
                List<IPermission> permissions = await ExecuteQueryAsync<IPermission>(isPrincipalTypePermission ? sql1.ToString() : sql2.ToString(), parametres);
                return permissions;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<IPermission>> GetExistingPermissions(string roleUID, string platform, bool isPrincipalTypePermission, List<string>? extUIDs)
        {
            try
            {
                // MSSQL Performance Fix: Don't use IN clause with large arrays
                // Instead, just get all permissions for the role and filter in memory
                // This is much faster for MSSQL compared to IN clause with 300+ UIDs
                var param = new Dictionary<string, object>
                {
                    {"RoleUID", roleUID },
                    {"platform", platform },
                };

                var sql1 = @"select uid from principal_permissions where role_uid=@RoleUID and platform=@platform";
                var sql2 = @"select uid from normal_permissions where role_uid=@RoleUID and platform=@platform";
                List<IPermission> allPermissions = await ExecuteQueryAsync<IPermission>(isPrincipalTypePermission ? sql1 : sql2, param);

                // Filter in memory - much faster than SQL IN clause on MSSQL
                if (extUIDs != null && extUIDs.Count > 0)
                {
                    var uidHashSet = new HashSet<string>(extUIDs);
                    return allPermissions.Where(p => uidHashSet.Contains(p.UID)).ToList();
                }

                return allPermissions;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CUDPermissionMaster(PermissionMaster permissionMaster)
        {
            List<IPermission> permissions = await GetExistingPermissions(permissionMaster.RoleUID, permissionMaster.Platform, permissionMaster.IsPrincipalPermission, permissionMaster?.Permissions?.Select(p => p.UID)?.ToList());
            int insCount = 0;
            foreach (IPermission permission in permissionMaster.Permissions)
            {
                bool isexist = permissions.Any(p => p.UID == permission.UID);
                insCount += isexist ? await UpdatePermissition(permission, permissionMaster.IsPrincipalPermission) : await CreatePermissition(permission, permissionMaster.IsPrincipalPermission);
            }

            return insCount;
        }

        public async Task<int> UpdateMenuDataByRole(IRole role)
        {
            try
            {
                var sql = @"UPDATE roles
                    SET 
                        web_menu_data=@web_menu_data,
                        mobile_menu_data=@mobile_menu_data,
                        modified_time = @modified_time,
                        server_modified_time = @server_modified_time
                       WHERE uid = @uid;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"uid" , role.UID},
                 {"web_menu_data" , role.WebMenuData},
                 {"mobile_menu_data" , role.MobileMenuData},
                 {"modified_time" , role.ModifiedTime},
                 {"server_modified_time" , role.ServerModifiedTime},
            };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CreatePermissition(IPermission permission, bool isPrincipalPermission)
        {
            try
            {

                var sql1 = @"INSERT INTO principal_permissions(
	                    uid, role_uid, sub_sub_module_uid, full_access, add_access, edit_access, view_access, delete_access, download_access, approval_access, ss, 
                        created_by,modified_by,created_time,modified_time,server_add_time,server_modified_time,platform)
	                    VALUES ( @uid,@role_uid,@sub_sub_module_uid,@full_access,@add_access,@edit_access,@view_access,@delete_access,@download_access,@approval_access,@ss,
                                @created_by,@modified_by,@created_time,@modified_time,@server_add_time,@server_modified_time,@platform);";

                var sql2 = @"INSERT INTO normal_permissions(
	                    uid, role_uid, sub_sub_module_uid, full_access, add_access, edit_access, view_access, delete_access, download_access, approval_access, ss, 
                        created_by,modified_by,created_time,modified_time,server_add_time,server_modified_time,platform)
	                    VALUES ( @uid,@role_uid,@sub_sub_module_uid,@full_access,@add_access,@edit_access,@view_access,@delete_access,@download_access,@approval_access,@ss,
                          @created_by,@modified_by,@created_time,@modified_time,@server_add_time,@server_modified_time,@platform);";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"uid" , permission.UID},
                 {"role_uid" , permission.RoleUid},
                 {"sub_sub_module_uid" , permission.SubSubModuleUid},
                 {"full_access" , permission.FullAccess},
                 {"add_access" , permission.AddAccess},
                 {"edit_access" , permission.EditAccess},
                 {"view_access" , permission.ViewAccess},
                 {"delete_access" , permission.DeleteAccess},
                 {"download_access" , permission.DownloadAccess},
                 {"approval_access" , permission.ApprovalAccess},
                 {"ss" , permission.SS},
                 {"created_by" , permission.CreatedBy},
                 {"modified_by" , permission.ModifiedBy},
                 {"created_time" , permission.CreatedTime},
                 {"modified_time" , permission.ModifiedTime},
                 {"server_add_time" , permission.ServerAddTime},
                 {"platform" , permission.Platform},
                 {"server_modified_time" , DateTime.Now},
            };

                return await ExecuteNonQueryAsync(isPrincipalPermission ? sql1.ToString() : sql2.ToString(), parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> UpdatePermissition(IPermission permission, bool isPrincipalPermission)
        {
            try
            {

                var sql1 = @"UPDATE principal_permissions
	                    SET   role_uid=@role_uid, sub_sub_module_uid=@sub_sub_module_uid, full_access=@full_access, add_access=@add_access, edit_access=@edit_access, view_access=@view_access, 
                        delete_access=@delete_access, download_access=@download_access, approval_access=@approval_access, ss=@ss, modified_by=@modified_by,  
                        modified_time=@modified_time, server_modified_time=@server_modified_time,platform=@platform
	                    WHERE uid=@uid;";

                var sql2 = @"UPDATE normal_permissions
	                    SET   role_uid=@role_uid, sub_sub_module_uid=@sub_sub_module_uid, full_access=@full_access, add_access=@add_access, edit_access=@edit_access, view_access=@view_access, 
                        delete_access=@delete_access, download_access=@download_access, approval_access=@approval_access, ss=@ss,  modified_by=@modified_by, 
                        modified_time=@modified_time,server_modified_time=@server_modified_time,platform=@platform
	                    WHERE uid=@uid;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"uid" , permission.UID},
                 {"role_uid" , permission.RoleUid},
                 {"sub_sub_module_uid" , permission.SubSubModuleUid},
                 {"platform" , permission.Platform},
                 {"full_access" , permission.FullAccess},
                 {"add_access" , permission.AddAccess},
                 {"edit_access" , permission.EditAccess},
                 {"view_access" , permission.ViewAccess},
                 {"delete_access" , permission.DeleteAccess},
                 {"download_access" , permission.DownloadAccess},
                 {"approval_access" , permission.ApprovalAccess},
                 {"ss" , permission.SS},
                 {"modified_by" , permission.ModifiedBy},
                 {"modified_time" , permission.ModifiedTime},
                 {"server_modified_time" , DateTime.Now},
            };

                return await ExecuteNonQueryAsync(isPrincipalPermission ? sql1 : sql2, parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<IRole>> SelectRolesByOrgUID(string orguid, bool IsAppUser = false)
        {
            try
            {
                var sql = @"SELECT 
                                                            r.id AS Id,
                                                            r.code AS Code,
                                                            r.uid AS Uid,
                                                            r.org_uid AS OrgUid,
                                                            r.role_name_en AS RoleNameEn,
                                                            r.role_name_other AS RoleNameOther,
                                                            r.parent_role_uid AS ParentRoleUid,
	                                                        rs.role_name_en as ParentRoleName,
                                                            r.is_principal_role AS IsPrincipalRole,
                                                            r.is_distributor_role AS IsDistributorRole,
                                                            r.is_admin AS IsAdmin,
                                                            r.bu_to_dist_access AS BuToDistAccess,
                                                            r.is_web_user AS IsWebUser,
                                                            r.is_app_user AS IsAppUser,
                                                            r.is_active AS IsActive,
                                                            r.ss AS Ss,
                                                            r.created_by AS CreatedBy,
                                                            r.modified_by AS ModifiedBy,
                                                            r.created_time AS CreatedTime,
                                                            r.modified_time AS ModifiedTime,
                                                            r.server_add_time AS ServerAddTime,
                                                            r.server_modified_time AS ServerModifiedTime,
                                                            r.code AS Code,
                                                            r.is_branch_applicable AS IsBranchApplicable,
			                         						r.web_menu_data as WebMenuData,r.mobile_menu_data as MobileMenuData,r.have_warehouse as HaveWareHouse,r.have_vehicle as HaveVehicle,r.has_p1_access as HasP1Access, 
                                                            r.has_p2_access as HasP2Access, 
                                                            r.has_p3_access as HasP3Access 
                                                        FROM 
                                                            roles r
                                                        Left JOIN 
                                                            roles rs ON rs.uid = r.parent_role_uid where r.org_uid=@orguid  ";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"orguid", orguid},
                };
                if (IsAppUser)
                {
                    sql += " AND r.is_app_user = @IsAppUser";
                    parameters.Add("IsAppUser", IsAppUser);
                }

                IEnumerable<IRole> roles = await ExecuteQueryAsync<IRole>(sql.ToString(), parameters);
                return roles;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IRole> SelectRolesByUID(string uid)
        {
            try
            {
                var sql = @"SELECT 
                                                            r.id AS Id,
                                                            r.code AS Code,
                                                            r.uid AS Uid,
                                                            r.org_uid AS OrgUid,
                                                            r.role_name_en AS RoleNameEn,
                                                            r.role_name_other AS RoleNameOther,
                                                            r.parent_role_uid AS ParentRoleUid,
	                                                        rs.role_name_en as ParentRoleName,
                                                            r.is_principal_role AS IsPrincipalRole,
                                                            r.is_distributor_role AS IsDistributorRole,
                                                            r.is_admin AS IsAdmin,
                                                            r.bu_to_dist_access AS BuToDistAccess,
                                                            r.is_web_user AS IsWebUser,
                                                            r.is_app_user AS IsAppUser,
                                                            r.is_active AS IsActive,
                                                            r.ss AS Ss,
                                                            r.created_by AS CreatedBy,
                                                            r.modified_by AS ModifiedBy,
                                                            r.created_time AS CreatedTime,
                                                            r.modified_time AS ModifiedTime,
                                                            r.server_add_time AS ServerAddTime,
                                                            r.server_modified_time AS ServerModifiedTime,
                                                            r.code AS Code,
			                         						r.web_menu_data as WebMenuData,r.mobile_menu_data as MobileMenuData,r.have_warehouse as HaveWareHouse,r.have_vehicle as HaveVehicle,
                                                            r.has_p1_access as HasP1Access, 
                                                            r.has_p2_access as HasP2Access, 
                                                            r.has_p3_access as HasP3Access ,r.has_margin_access,r.has_msp_access
                                                        FROM 
                                                           roles r
                                                        Left JOIN 
                                                            roles rs ON rs.uid = r.parent_role_uid where r.uid=@uid";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"uid", uid},
                 };
                IRole role = await ExecuteSingleAsync<IRole>(sql.ToString(), parameters);
                return role;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IPermission> GetPermissionByRoleAndPage(string roleUID, string relativePath, bool isPrincipleRole)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"RoleUID", roleUID},
                   {"RelativePath", relativePath},
                 };
            string principal_permissions = """
                                select p.* from principal_permissions p
                inner join roles r on r.uid=p.role_uid and r.uid=@RoleUID
                inner join sub_sub_modules ssm on ssm.uid=p.sub_sub_module_uid and ssm.relative_path=@RelativePath
                """;
            string normal_permissions = """
                                select p.* from normal_permissions p
                inner join roles r on r.uid=p.role_uid and r.uid=@RoleUID
                inner join sub_sub_modules ssm on ssm.uid=p.sub_sub_module_uid and ssm.relative_path=@RelativePath
                """;

            IPermission permission = await ExecuteSingleAsync<IPermission>(isPrincipleRole ? principal_permissions : normal_permissions, parameters);
            return permission;
        }

    }


}
