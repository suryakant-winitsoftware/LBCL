using Microsoft.Extensions.Configuration;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.DL.Classes;

public class MSSQLStoreHistoryDL : SqlServerDBManager, IStoreHistoryDL
{
    public MSSQLStoreHistoryDL(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider, configuration)
    {

    }
    public async Task<IStoreHistory?> GetStoreHistoryByRouteUIDVisitDateAndStoreUID(string routeUID, string visitDate, string storeUID)
    {
        try
        {
            string sql = """
						 SELECT SH.* FROM store_history SH 
						 INNER JOIN beat_history BH ON BH.uid = SH.beat_history_uid WHERE BH.route_uid = @RouteUID 
						 AND BH.visit_date::Date =  @VisitDate
						 AND SH.store_uid = @StoreUID
						 """;
            var parameters = new
            {
                RouteUID = routeUID,
                VisitDate = DateTime.Parse(visitDate).Date,
                StoreUID = storeUID
            };
            IStoreHistory? storeHistory = await ExecuteSingleAsync<IStoreHistory>(sql, parameters);
            if (storeHistory is not null)
            {
                return storeHistory;
            }

            _ = await CreateStoreHistoryByRouteUIDVisitDateAndStoreUID(routeUID, visitDate, storeUID);
            return await ExecuteSingleAsync<IStoreHistory>(sql, parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<int> CreateStoreHistoryByRouteUIDVisitDateAndStoreUID(string routeUID, string visitDate, string storeUID)
    {
        try
        {
            string sql = """
                            INSERT INTO store_history(
                         	uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                         	server_modified_time, user_journey_uid, year_month, beat_history_uid, 
                         	org_uid, route_uid, store_uid, is_planned, 
                         	serial_no, status, visit_duration, travel_time, planned_login_time, planned_logout_time, 
                         	login_time, logout_time, no_of_visits, last_visit_date, 
                         	is_skipped, is_productive, is_green, target_value, 
                         	target_volume, target_lines, actual_value, actual_volume, actual_lines, 
                         	planned_time_spend_in_minutes, latitude, longitude, notes, ss)
                         SELECT BH.uid || RC.store_uid uid, BH.created_by, NOW() created_time, BH.modified_by, 
                         	NOW() modified_time, NOW() server_add_time, 
                         	NOW() server_modified_time, null user_journey_uid, BH.year_month year_month, 
                         	BH.uid beat_history_uid, 
                         	BH.org_uid, BH.route_uid, RC.store_uid, true is_planned, 
                         	RC.seq_no serial_no, 'Pending' status, RC.visit_duration, RC.travel_time, 
                         	RC.visit_time planned_login_time, 
                         	RC.end_time planned_logout_time, null login_time, null logout_time, 
                         	0 no_of_visits, null last_visit_date, false is_skipped, false is_productive, 
                         	false is_green, 0 target_value, 0 target_volume, 0 target_lines, 
                         	0 actual_value, 0 actual_volume, 0 actual_lines, 
                         	0 planned_time_spend_in_minutes, 0 latitude, 0 longitude, 
                         	null notes, 0 ss
                         	FROM beat_history BH 
                         	INNER JOIN route_customer RC ON RC.route_uid = BH.route_uid 
                         	AND BH.route_uid = @RouteUID AND BH.visit_date::Date = @VisitDate
                         	AND RC.store_uid = @StoreUID
                         	LEFT JOIN store_history SH on SH.beat_history_uid = BH.uid 
                         	AND SH.store_uid = RC.store_uid
                         	WHERE SH.id is null;
                         """;
            var parameters = new
            {
                RouteUID = routeUID,
                VisitDate = DateTime.Parse(visitDate).Date,
                StoreUID = storeUID
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
