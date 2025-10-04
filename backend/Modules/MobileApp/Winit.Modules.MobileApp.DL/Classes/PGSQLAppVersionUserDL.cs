using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mobile.DL.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.DL.Classes
{
    public class PGSQLAppVersionUserDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IAppVersionUserDL
    {
        public PGSQLAppVersionUserDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<Shared.Models.Common.PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetAppVersionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                        * 
                                                    FROM 
                                                        (
                                                            SELECT DISTINCT
                                                                av.id AS Id,
                                                                av.uid AS UID,
                                                                av.emp_uid AS EmpUID,
                                                                av.device_type AS DeviceType,
                                                                av.device_id AS DeviceId,
                                                                av.app_version AS AppVersion,
                                                                av.app_version_number AS AppVersionNumber,
                                                                av.api_version AS ApiVersion,
                                                                av.deployment_date_time AS DeploymentDateTime,
                                                                av.next_app_version AS NextAppVersion,
                                                                av.next_app_version_number AS NextAppVersionNumber,
                                                                av.publish_date AS PublishDate,
                                                                av.is_test AS IsTest,
                                                                av.imei_no AS IMEINo,
                                                                av.org_uid AS OrgUID,
                                                                av.gcm_key AS GcmKey,
                                                                e.name AS EmployeeName
                                                            FROM
                                                                app_version_user av
                                                            JOIN
                                                                emp e ON av.emp_uid = e.uid
                                                            WHERE
                                                                av.org_uid = @OrgUID
                                                        ) AS Subquery
");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT 
                                                        COUNT(1) AS Cnt  
                                                    FROM
                                                        (
                                                            SELECT DISTINCT
                                                                AV.id AS Id,
                                                                AV.uid AS UID,
                                                                AV.emp_uid AS EmpUID,
                                                                AV.device_type AS DeviceType,
                                                                AV.device_id AS DeviceId,
                                                                AV.app_version AS AppVersion,
                                                                AV.app_version_number AS AppVersionNumber,
                                                                AV.api_version AS ApiVersion,
                                                                AV.deployment_date_time AS DeploymentDateTime,
                                                                AV.next_app_version AS NextAppVersion,
                                                                AV.next_app_version_number AS NextAppVersionNumber,
                                                                AV.publish_date AS PublishDate,
                                                                AV.is_test AS IsTest,
                                                                AV.imei_no AS IMEINo,
                                                                AV.org_uid AS OrgUID,
                                                                AV.gcm_key AS GcmKey,
                                                                E.name AS EmployeeName
                                                            FROM
                                                                app_version_user AV
                                                            JOIN
                                                                emp E ON AV.emp_uid = E.uid
                                                            WHERE
                                                                AV.org_uid = @OrgUID
                                                        ) AS Subquery");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "OrgUID",OrgUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql,true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAppVersionUser>().GetType();

                IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> appVersionUserList = await ExecuteQueryAsync<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> pagedResponse = new PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>
                {
                    PagedData = appVersionUserList,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByUID(string UID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                    id AS Id,
                                                    uid AS UID,
                                                    emp_uid AS EmpUID,
                                                    device_type AS DeviceType,
                                                    device_id AS DeviceId,
                                                    app_version AS AppVersion,
                                                    app_version_number AS AppVersionNumber,
                                                    api_version AS ApiVersion,
                                                    deployment_date_time AS DeploymentDateTime,
                                                    next_app_version AS NextAppVersion,
                                                    next_app_version_number AS NextAppVersionNumber,
                                                    publish_date AS PublishDate,
                                                    is_test AS IsTest,
                                                    imei_no AS IMEINo,
                                                    ss AS SS,
                                                    org_uid AS OrgUID,
                                                    gcm_key AS GcmKey 
                                                FROM 
                                                    app_version_user 
                                                WHERE 
                                                    uid = @UID;");
                var parameters = new Dictionary<string, object>()
                {
                    { "UID",UID}
                };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAppVersionUser>().GetType();

                Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser appVersionUser = await ExecuteSingleAsync<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>(sql.ToString(), parameters, type);

                return appVersionUser;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetAppVersionDetailsByEmpUID(string empUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                    id AS Id,
                                                    uid AS UID,
                                                    emp_uid AS EmpUID,
                                                    device_type AS DeviceType,
                                                    device_id AS DeviceId,
                                                    app_version AS AppVersion,
                                                    app_version_number AS AppVersionNumber,
                                                    api_version AS ApiVersion,
                                                    deployment_date_time AS DeploymentDateTime,
                                                    next_app_version AS NextAppVersion,
                                                    next_app_version_number AS NextAppVersionNumber,
                                                    publish_date AS PublishDate,
                                                    is_test AS IsTest,
                                                    imei_no AS IMEINo,
                                                    ss AS SS,
                                                    org_uid AS OrgUID,
                                                    gcm_key AS GcmKey 
                                                FROM 
                                                    app_version_user 
                                                WHERE 
                                                    emp_uid = @UID;");
                var parameters = new Dictionary<string, object>()
                {
                    { "UID",empUID}
                };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAppVersionUser>().GetType();

                Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser appVersionUser = await ExecuteSingleAsync<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>(sql.ToString(), parameters, type);

                return appVersionUser;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateAppVersionDetails(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {

            var Query = @"UPDATE app_version_user 
                                        SET 
                                            device_id = @DeviceId, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime
                                        WHERE 
                                            uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                            { "@UID", appVersionUser.UID },
                            { "@DeviceId", appVersionUser.DeviceId },
                            { "@ModifiedBy", appVersionUser.ModifiedBy},
                            { "@ModifiedTime", appVersionUser.ModifiedTime },
                            { "@ServerModifiedTime", DateTime.Now }
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }

        /// <summary>
        /// Inserts a new app version user record
        /// </summary>
        /// <param name="appVersionUser">App version user data to insert</param>
        /// <returns>Number of rows affected (1 if successful, 0 if failed)</returns>
        public async Task<int> InsertAppVersionUser(Winit.Modules.Mobile.Model.Classes.AppVersionUser appVersionUser)
        {
            try
            {
                var insertQuery = @"INSERT INTO app_version_user 
                                        (uid, emp_uid, device_type, device_id, app_version, app_version_number, 
                                         api_version, deployment_date_time, next_app_version, next_app_version_number, 
                                         publish_date, is_test, imei_no, org_uid, gcm_key, imei_no2, 
                                         ss, created_by, created_time, server_add_time, server_modified_time) 
                                    VALUES 
                                        (@UID, @EmpUID, @DeviceType, @DeviceId, @AppVersion, @AppVersionNumber,
                                         @ApiVersion, @DeploymentDateTime, @NextAppVersion, @NextAppVersionNumber,
                                         @PublishDate, @IsTest, @IMEINo, @OrgUID, @GcmKey, @IMEINo2,
                                         @SS, @CreatedBy, @CreatedTime, @ServerAddTime, @ServerModifiedTime);";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", appVersionUser.UID },
                    { "@EmpUID", appVersionUser.EmpUID },
                    { "@DeviceType", appVersionUser.DeviceType ?? "Android" },
                    { "@DeviceId", appVersionUser.DeviceId },
                    { "@AppVersion", appVersionUser.AppVersion },
                    { "@AppVersionNumber", appVersionUser.AppVersionNumber },
                    { "@ApiVersion", appVersionUser.ApiVersion },
                    { "@DeploymentDateTime", appVersionUser.DeploymentDateTime },
                    { "@NextAppVersion", appVersionUser.NextAppVersion },
                    { "@NextAppVersionNumber", appVersionUser.NextAppVersionNumber },
                    { "@PublishDate", appVersionUser.PublishDate },
                    { "@IsTest", appVersionUser.IsTest},
                    { "@IMEINo", appVersionUser.IMEINo },
                    { "@OrgUID", appVersionUser.OrgUID},
                    { "@GcmKey", appVersionUser.GcmKey},
                    { "@IMEINo2", appVersionUser.IMEINo2 }, // IMEINo2 field from specification
                    { "@SS", 1 }, // Default sync status
                    { "@CreatedBy", appVersionUser.CreatedBy },
                    { "@CreatedTime", appVersionUser.CreatedTime },
                    { "@ServerAddTime", DateTime.Now },
                    { "@ServerModifiedTime", DateTime.Now }
                };

                return await ExecuteNonQueryAsync(insertQuery, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
