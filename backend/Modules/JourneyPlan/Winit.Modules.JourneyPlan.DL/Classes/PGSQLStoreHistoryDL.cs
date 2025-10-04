using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Classes
{
    public class PGSQLStoreHistoryDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreHistoryDL
    {
        public PGSQLStoreHistoryDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> CreateStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"INSERT INTO store_history (id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            user_journey_uid, year_month, beat_history_uid, org_uid, route_uid, store_uid, is_planned, serial_no, status, visit_duration, travel_time, 
                            planned_login_time, planned_logout_time, login_time, logout_time, no_of_visits, last_visit_date, is_skipped, is_productive, is_green, target_value, 
                            target_volume, target_lines, actual_value, actual_volume, actual_lines, planned_time_spend_in_minutes, latitude, longitude, notes, ss) 
                            VALUES (@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @UserJourneyUID, @YearMonth, 
                            @BeatHistoryUID, @OrgUID, @RouteUID, @StoreUID, @IsPlanned, @SerialNo, @Status, @VisitDuration, @TravelTime, @PlannedLoginTime, @PlannedLogoutTime,
                            @LoginTime, @LogoutTime, @NoOfVisits, @LastVisitDate, @IsSkipped, @IsProductive, @IsGreen, @TargetValue, @TargetVolume, @TargetLines, @ActualValue,
                            @ActualVolume, @ActualLines, @PlannedTimeSpendInMinutes, @Latitude, @Longitude, @Notes, 0);";

                return await ExecuteNonQueryAsync(sql, storeHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var sql = @"UPDATE store_history SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime,
                            user_journey_uid = @UserJourneyUID, year_month = @YearMonth, beat_history_uid = @BeatHistoryUID, org_uid = @OrgUID,
                            route_uid = @RouteUID, store_uid = @StoreUID, is_planned = @IsPlanned, serial_no = @SerialNo, status = @Status, 
                            visit_duration = @VisitDuration, travel_time = @TravelTime, planned_login_time = @PlannedLoginTime, planned_logout_time = @PlannedLogoutTime,
                            login_time = @LoginTime, logout_time = @LogoutTime, no_of_visits = @NoOfVisits, last_visit_date = @LastVisitDate, is_skipped = @IsSkipped, 
                            is_productive = @IsProductive, is_green = @IsGreen, target_value = @TargetValue, target_volume = @TargetVolume, target_lines = @TargetLines, 
                            actual_value = @ActualValue, actual_volume = @ActualVolume, actual_lines = @ActualLines, planned_time_spend_in_minutes = @PlannedTimeSpendInMinutes,
                            latitude = @Latitude, longitude = @Longitude, notes = @Notes, ss = 0 WHERE uid = @UID;";

                return await ExecuteNonQueryAsync(sql, storeHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CUDStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            try
            {
                //IStoreHistory? existingRec = await SelectStoreHistory_ByUID(storeHistory.UID, connection, transaction);
                string? existingRec = await CheckIfUIDExistsInDB(DbTableName.StoreHistory, storeHistory.UID, connection, transaction);
                if (!string.IsNullOrEmpty(existingRec))
                {
                    count += await UpdateStoreHistory(storeHistory, connection, transaction);
                }
                else
                {
                    count += await CreateStoreHistory(storeHistory, connection, transaction);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }

        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory?> SelectStoreHistory_ByUID(string UID,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
            string sql = @"SELECT * FROM store_history WHERE uid = @UID";
            return await ExecuteSingleAsync<IStoreHistory>(sql, parameters, null, connection, transaction);
        }

    }
}
