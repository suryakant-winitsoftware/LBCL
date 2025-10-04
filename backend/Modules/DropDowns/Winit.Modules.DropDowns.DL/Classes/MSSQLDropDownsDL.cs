using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Winit.Modules.DropDowns.DL.Interfaces;
using Microsoft.Extensions.Configuration;
using Winit.Shared.Models.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Winit.Modules.DropDowns.DL.Classes
{
    public class MSSQLDropDownsDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IDropDownsDL
    {
        public MSSQLDropDownsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<ISelectionItem>> GetEmpDropDown(string orgUID, bool getDataByLoginId = false)
        {
            IEnumerable<ISelectionItem> empDetails;
            var parameters = new Dictionary<string, object>()
            {
                { "OrgUID", orgUID }
            };
            try
            {
                if (getDataByLoginId)
                {
                    var query = new StringBuilder(@"SELECT  e.uid AS UID,     e.code AS Code,     e.login_id AS Label 
                                                            FROM  emp e 
                                                            JOIN job_position jp ON     e.uid = jp.emp_uid 
                                                            WHERE jp.org_uid =@OrgUID");
                    empDetails = await ExecuteQueryAsync<ISelectionItem>(query.ToString(), parameters);
                    return empDetails;
                }
                else
                {
                    var sql = new StringBuilder(@"SELECT e.uid AS UID,'[' + e.code + '] ' + e.name AS Code,'[' + e.code + '] ' + e.name AS Label 
                                                        FROM emp e 
                                                        JOIN  job_position jp ON     e.uid = jp.emp_uid 
                                                        WHERE jp.org_uid = @OrgUID;");
                     empDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                    return empDetails;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetRouteDropDown(string orgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT
                                uid AS UID,
                                '[' + code + '] ' + name AS Code,
                                '[' + code + '] ' + name AS Label
                            FROM route WHERE    org_uid = @OrgUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@OrgUID",orgUID }
                };
                IEnumerable<ISelectionItem> routeDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return routeDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetVehicleDropDown(string parentUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT
                                v.uid AS UID,
                                '[' + v.vehicle_no + ']' + v.registration_no AS Code,
                                '[' + v.vehicle_no + ']' + v.registration_no AS Label
                                FROM vehicle v JOIN    org o ON o.uid = v.org_uid WHERE o.parent_uid = @ParentUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@ParentUID",parentUID }
                };
                IEnumerable<ISelectionItem> vehicleDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return vehicleDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetRequestFromDropDown(string parentUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT uid AS Uid, code AS Code,
                                              name AS Label FROM org
                                              WHERE org_type_uid IN ('WH', 'VWH')
                                              AND parent_uid = @ParentUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@ParentUID",parentUID }
                };
                IEnumerable<ISelectionItem> requestFromDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return requestFromDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetDistributorDropDown()
        {
            try
            {
                var sql = new StringBuilder(@"SELECT uid AS UID, code AS Code,
                                            name AS Label FROM org WHERE org_type_uid = 'FR';");
                var parameters = new Dictionary<string, object>();
                IEnumerable<ISelectionItem> distributorDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return distributorDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetCustomerDropDown(string franchiseeOrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"
	                   SELECT uid as UID, code as Code,name AS Label FROM store
                       WHERE franchisee_org_uid = @franchiseeOrgUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@franchiseeOrgUID",franchiseeOrgUID }
                };
                IEnumerable<ISelectionItem> distributorDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return distributorDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetDistributorChannelDropDown(string parentUID)
        {
            try
            {
                var sql = new StringBuilder(@"
	                    SELECT uid as UID, code as Code, name AS Label FROM org
                        WHERE org_type_uid = 'DC'  AND parent_uid = @ParentUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@ParentUID",parentUID }
                };
                IEnumerable<ISelectionItem> distributorChannelDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return distributorChannelDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetWareHouseTypeDropDown(string parentUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT uid as UID,code as Code, name AS Label
                                            FROM org WHERE org_type_uid = 'WH'  AND parent_uid = @ParentUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@ParentUID",parentUID }
                };
                IEnumerable<ISelectionItem> requestFromDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return requestFromDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ISelectionItem>> GetCustomersByRouteUIDDropDown(string routeUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT s.uid, s.code, s.name AS label
                                               FROM route_customer rc 
                                               INNER JOIN store s ON s.uid = rc.store_uid AND rc.route_uid = @RouteUID;");
                var parameters = new Dictionary<string, object>()
                {
                    {"@RouteUID",routeUID }
                };
                IEnumerable<ISelectionItem> requestFromDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString(), parameters);
                return requestFromDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
