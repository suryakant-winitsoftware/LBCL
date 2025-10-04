using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.Syncing.Model;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Syncing.DL.Classes
{
    public class MSSQLAppRequestDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IAppRequestDL
    {
        public MSSQLAppRequestDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<bool> InsertPostAppRequest(List<IAppRequest> appRequests)
        {
            if (appRequests == null || appRequests.Count == 0)
            {
                return true;
            }
            List<string> uidList = appRequests.Select(ar => ar.UID).ToList();
            try
            {
                List<string> existingUIDs = await CheckAppRequestExistsByUID(uidList);
                List<IAppRequest> appRequestsNew = appRequests.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                if (appRequestsNew == null || appRequestsNew.Count == 0)
                {
                    return true;
                }
                int result = await CreateAppRequest(appRequestsNew);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Task<int> UpdateAppRequest_IsNotificationReceived(List<string> uIDs, bool isNotificationReceived)
        {
            throw new NotImplementedException();
        }
        public async Task<int> UpdateAppRequest_RequestPostedToAPITime(List<string> uIDs, DateTime dateTime)
        {
            try
            {
                var query = $@"UPDATE app_request SET request_posted_to_api_time = '{CommonFunctions.GetDateTimeInFormatForSqlite(dateTime)}' 
                                WHERE UID = @UID;";

                var parameters = new { UIDs = uIDs };
                return await ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<IAppRequest>> SelectAppRequestByUID(List<string> UIDs)
        {
            var parameters = new { UIDs = UIDs };
            var sql = @"SELECT 
                            ap.id AS Id,
                            ap.uid AS UID,
                            ap.org_uid AS OrgUID,
                            ap.linked_item_type AS LinkedItemType,
                            ap.year_month AS YearMonth,
                            ap.linked_item_uid AS LinkedItemUID,
                            ap.emp_uid AS EmpUID,
                            ap.job_position_uid AS JobPositionUID,
                            ap.request_created_time AS RequestCreatedTime,
                            ap.request_posted_to_api_time AS RequestPostedToApiTime,
                            ap.request_received_by_api_time AS RequestReceivedByApiTime,
                            ap.next_uid AS NextUID,
                            ap.request_body AS RequestBody,
                            ap.request_uids AS RequestUIDs
                        FROM 
                            app_request ap
                            WHERE
                            UID in @UIDs;
                    ";
            IEnumerable<IAppRequest> appRequestList = await ExecuteQueryAsync<IAppRequest>(sql, parameters);
            return appRequestList;
        }
        private async Task<List<string>> CheckAppRequestExistsByUID(List<string> UIDs)
        {
            var parameters = new { UIDs = UIDs };
            var sql = @"SELECT 
                            ap.uid AS UID
                        FROM 
                            app_request ap
                            WHERE
                            ap.UID in @UIDs;
                    ";
            return await ExecuteQueryAsync<string>(sql, parameters);
        }

        private async Task<int> CreateAppRequest_Old(IAppRequest appRequest)
        {
            var requestBodyJson = JsonConvert.SerializeObject(appRequest.RequestBody);
            var requestUIDsJson = JsonConvert.SerializeObject(appRequest.RequestUIDs);
            try
            {
                var Query = @"INSERT INTO app_request (
                     uid, org_uid, linked_item_type, year_month, linked_item_uid, emp_uid, 
                    job_position_uid, request_created_time, request_posted_to_api_time, 
                    request_received_by_api_time, next_uid, request_body, request_uids
                        ) VALUES (
                     @UID, @OrgUID, @LinkedItemType, @YearMonth, @LinkedItemUID, @EmpUID, 
                    @JobPositionUID, @RequestCreatedTime, @RequestPostedToAPITime, 
                    @RequestReceivedByAPITime, @NextUID, CAST(@RequestBody AS NVARCHAR(MAX)), 
    CAST(@RequestUIDs AS NVARCHAR(MAX)));";

                
                return await ExecuteNonQueryAsync(Query, appRequest);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateAppRequest(List<IAppRequest> appRequests)
        {
            try
            {
                var Query = @"INSERT INTO app_request (
                     uid, org_uid, linked_item_type, year_month, linked_item_uid, emp_uid, 
                    job_position_uid, request_created_time, request_posted_to_api_time, 
                    request_received_by_api_time, next_uid, request_body, request_uids
                        ) VALUES (
                     @UID, @OrgUID, @LinkedItemType, @YearMonth, @LinkedItemUID, @EmpUID, 
                    @JobPositionUID, @RequestCreatedTime, @RequestPostedToAPITime, 
                    @RequestReceivedByAPITime, @NextUID, CAST(@RequestBody AS NVARCHAR(MAX)), 
    CAST(@RequestUIDs AS NVARCHAR(MAX)));";

                return await ExecuteNonQueryAsync(Query, appRequests);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

