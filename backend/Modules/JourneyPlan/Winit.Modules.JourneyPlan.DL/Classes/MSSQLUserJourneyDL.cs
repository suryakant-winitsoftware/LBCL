using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.JourneyPlan.Model.Classes;
using Nest;

namespace Winit.Modules.JourneyPlan.DL.Classes
{
    public class MSSQLUserJourneyDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, Winit.Modules.JourneyPlan.DL.Interfaces.IUserJourneyDL
    {
        public MSSQLUserJourneyDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>> SelectAlUserJourneyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from (SELECT 
                                u.id AS Id,
                                u.uid AS UID,
                                u.job_position_uid AS JobPositionUID,
                                u.emp_uid AS EmpUID,
                                e.login_id AS LoginId,
                                u.journey_start_time AS JourneyStartTime, 
                                u.journey_end_time AS JourneyEndTime,
                                u.start_odometer_reading AS StartOdometerReading,
                                u.end_odometer_reading AS EndOdometerReading,
                                u.journey_time AS JourneyTime,
                               -- u.vehicle_no AS VehicleNo,
                                u.eot_status AS EOTStatus,
                                --u.re_opened_by AS ReOpenedBy,
                                u.has_audit_completed AS HasAuditCompleted,
                                u.ss AS SS,
                                u.created_time AS CreatedTime,
                                u.modified_time AS ModifiedTime,
                                u.server_add_time AS ServerAddTime,
                                u.server_modified_time AS ServerModifiedTime,
                                u.beat_history_uid AS BeatHistoryUID,
                                u.wh_stock_request_uid AS WHStockRequestUID
                            FROM 
                                user_journey u
                            JOIN 
                                job_position j ON u.job_position_uid = j.uid
                            JOIN 
                                emp e ON u.emp_uid = e.uid
                            JOIN 
                                org o ON j.org_uid = o.uid)As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT 
                                                            COUNT(1) AS Cnt
                                                        FROM 
                                                            (SELECT 
                                u.id AS Id,
                                u.uid AS UID,
                                u.job_position_uid AS JobPositionUID,
                                u.emp_uid AS EmpUID,
                                e.login_id AS LoginId,
                                u.journey_start_time AS JourneyStartTime, 
                                u.journey_end_time AS JourneyEndTime,
                                u.start_odometer_reading AS StartOdometerReading,
                                u.end_odometer_reading AS EndOdometerReading,
                                u.journey_time AS JourneyTime,
                               -- u.vehicle_no AS VehicleNo,
                                u.eot_status AS EOTStatus,
                                --u.re_opened_by AS ReOpenedBy,
                                u.has_audit_completed AS HasAuditCompleted,
                                u.ss AS SS,
                                u.created_time AS CreatedTime,
                                u.modified_time AS ModifiedTime,
                                u.server_add_time AS ServerAddTime,
                                u.server_modified_time AS ServerModifiedTime,
                                u.beat_history_uid AS BeatHistoryUID,
                                u.wh_stock_request_uid AS WHStockRequestUID
                            FROM 
                                user_journey u
                            JOIN 
                                job_position j ON u.job_position_uid = j.uid
                            JOIN 
                                emp e ON u.emp_uid = e.uid
                            JOIN 
                                org o ON j.org_uid = o.uid)As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>(filterCriterias, sbFilterCriteria, parameters);
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

                IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney> userJourneys = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>
                {
                    PagedData = userJourneys,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>> SelectTodayJourneyPlanDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Type, DateTime VisitDate, string JobPositionUID, string OrgUID)
        {
            try
            {
                if (Type == "Assigned")
                {
                    var sql = new StringBuilder(@"SELECT * FROM (
                                                    SELECT
                                                        bh.uid AS BeatHistoryUID, 
                                                        bh.visit_date AS VisitDate,
                                                        e.name AS SalesmanName,
                                                        e.login_id AS SalesmanLoginId,
                                                        r.uid AS RouteUID,
                                                        e.uid AS EmpUID,
                                                        SUM(CASE WHEN sh.login_time IS NOT NULL AND sh.is_planned = 1 THEN 1 ELSE 0 END) AS ActualStoreVisits,
                                                        SUM(CASE WHEN sh.login_time IS NULL AND sh.is_skipped = 1 THEN 1 ELSE 0 END) AS SkippedStore,
                                                        SUM(CASE WHEN sh.is_planned = 1 THEN 1 ELSE 0 END) AS ScheduleCall,
                                                        '[' + r.code + '] ' + r.name AS RouteName,
                                                        '[' + v.vehicle_no + ']' + v.registration_no AS VehicleName,
                                                        0 AS PendingVisits 
                                                    FROM
                                                        beat_history bh
                                                        INNER JOIN route r ON r.uid = bh.route_uid
                                                        INNER JOIN store_history sh ON sh.beat_history_uid = bh.uid
                                                        INNER JOIN store s ON s.uid = sh.store_uid
                                                        INNER JOIN job_position jp ON jp.uid = bh.job_position_uid
                                                        INNER JOIN emp e ON e.uid = jp.emp_uid
                                                        LEFT JOIN vehicle v ON v.uid = r.vehicle_uid
                                                    WHERE
                                                        (sh.is_planned = 1 OR sh.status = 'Visited')
                                                        AND CAST(bh.visit_date AS DATE) = @VisitDate
                                                        --AND (@JobPositionUID = '' OR bh.job_position_uid = @JobPositionUID)
                                                        --AND (@OrgUID = '' OR jp.org_uid = @OrgUID)
                                                    GROUP BY
                                                        bh.uid,
                                                        e.name,
                                                        e.login_id,
                                                        e.uid,
                                                        r.uid,
                                                        bh.visit_date,
                                                        '[' + r.code + '] ' + r.name,
                                                        '[' + v.vehicle_no + ']' + v.registration_no) AS subquery");
                    var sqlCount = new StringBuilder();
                    if (isCountRequired)
                    {
                        sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM( SELECT
                                                        bh.uid AS BeatHistoryUID, 
                                                        bh.visit_date AS VisitDate,
                                                        e.name AS SalesmanName,
                                                        e.login_id AS SalesmanLoginId,
                                                        r.uid AS RouteUID,
                                                        e.uid AS EmpUID,
                                                        SUM(CASE WHEN sh.login_time IS NOT NULL AND sh.is_planned = 1 THEN 1 ELSE 0 END) AS ActualStoreVisits,
                                                        SUM(CASE WHEN sh.login_time IS NULL AND sh.is_skipped = 1 THEN 1 ELSE 0 END) AS SkippedStore,
                                                        SUM(CASE WHEN sh.is_planned = 1 THEN 1 ELSE 0 END) AS ScheduleCall,
                                                        '[' + r.code + '] ' + r.name AS RouteName,
                                                        '[' + v.vehicle_no + ']' + v.registration_no AS VehicleName,
                                                        0 AS PendingVisits 
                                                    FROM
                                                        beat_history bh
                                                        INNER JOIN route r ON r.uid = bh.route_uid
                                                        INNER JOIN store_history sh ON sh.beat_history_uid = bh.uid
                                                        INNER JOIN store s ON s.uid = sh.store_uid
                                                        INNER JOIN job_position jp ON jp.uid = bh.job_position_uid
                                                        INNER JOIN emp e ON e.uid = jp.emp_uid
                                                        LEFT JOIN vehicle v ON v.uid = r.vehicle_uid
                                                    WHERE
                                                        (sh.is_planned = 1 OR sh.status = 'Visited')
                                                        AND CAST(bh.visit_date AS DATE) = @VisitDate
                                                        --AND (@JobPositionUID = '' OR bh.job_position_uid = @JobPositionUID)
                                                        --AND (@OrgUID = '' OR jp.org_uid = @OrgUID)
                                                    GROUP BY
                                                        bh.uid,
                                                        e.name,
                                                        e.login_id,
                                                        e.uid,
                                                        r.uid,
                                                        bh.visit_date,
                                                        '[' + r.code + '] ' + r.name,
                                                        '[' + v.vehicle_no + ']' + v.registration_no) AS subquery");
                    }
                    var parameters = new Dictionary<string, object>()
                    {
                        {"VisitDate",VisitDate }
                    };
                    if (filterCriterias != null && filterCriterias.Count > 0)
                    {
                        StringBuilder sbFilterCriteria = new StringBuilder();
                        sbFilterCriteria.Append(" WHERE ");
                        AppendFilterCriteria<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>(filterCriterias, sbFilterCriteria, parameters); ;

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
                    IEnumerable<Model.Interfaces.IAssignedJourneyPlan> assignedJourneys = await ExecuteQueryAsync<Model.Interfaces.IAssignedJourneyPlan>(sql.ToString(), parameters);
                    int totalCount = 0;
                    if (isCountRequired)
                    {
                        totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                    }
                    PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>
                    {
                        PagedData = assignedJourneys,
                        TotalCount = totalCount
                    };
                    return pagedResponse;
                }


                else if (Type == "UnAssigned")
                {
                    var sql = new StringBuilder(@"select * from (
                                                select
                                                    e.name as SalesmanName,
                                                    e.login_id as SalesmanLoginId,
                                                    r.uid as RouteUID,
                                                    e.uid AS EmpUID,

                                                    '[' + r.code + '] ' + r.name as RouteName,
                                                    '[' + v.vehicle_no + ']' + v.registration_no as VehicleName
                                                from
                                                    route r
                                                    inner join job_position jp on jp.uid = r.job_position_uid
                                                    inner join emp e on e.uid = jp.emp_uid
                                                    left join vehicle v on v.uid = r.vehicle_uid
                                                where
                                                    r.uid not in (
                                                        select route_uid
                                                        from beat_history
                                                        where CAST(visit_date AS DATE) = @VisitDate
                                                    )
                                            ) as subquery");

                    var sqlCount = new StringBuilder();
                    if (isCountRequired)
                    {
                        sqlCount = new StringBuilder(@"select count(1) as Cnt from (
                                      select  
                                            e.name as SalesmanName,
                                            e.login_id as SalesmanLoginId,
                                            r.uid as RouteUID,
                                            e.uid AS EmpUID,

                                            '[' + r.code + '] ' + r.name as RouteName,
                                            '[' + v.vehicle_no + ']' + v.registration_no as VehicleName
                                        from
                                            route r
                                            inner join job_position jp on jp.uid = r.job_position_uid
                                            inner join emp e on e.uid = jp.emp_uid
                                            left join vehicle v on v.uid = r.vehicle_uid
                                        where
                                            r.uid not in (
                                                select route_uid
                                                from beat_history
                                                where CAST(visit_date AS DATE) = @VisitDate
                                            )
                                    ) as subquery");

                    }
                    var parameters = new Dictionary<string, object>()
                    {
                         {"VisitDate",VisitDate }
                    };
                    if (filterCriterias != null && filterCriterias.Count > 0)
                    {
                        StringBuilder sbFilterCriteria = new StringBuilder();
                        sbFilterCriteria.Append(" WHERE ");
                        AppendFilterCriteria<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>(filterCriterias, sbFilterCriteria, parameters);

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
                    IEnumerable<Model.Interfaces.IAssignedJourneyPlan> assignedJourneys = await ExecuteQueryAsync<Model.Interfaces.IAssignedJourneyPlan>(sql.ToString(), parameters);
                    int totalCount = 0;
                    if (isCountRequired)
                    {
                        totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                    }
                    PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>
                    {
                        PagedData = assignedJourneys,
                        TotalCount = totalCount
                    };
                    return pagedResponse;
                }

                else
                {
                    throw new ArgumentException("Invalid Type value");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid>> SelecteatHistoryInnerGridDetails(string BeatHistoryUID)
        {
            try
            {
                var sql = new StringBuilder(@"
                                    select distinct
                                                    c.number as StoreNumber,
                                                    sh.serial_no as SeqNo,
                                                    c.code as StoreCode,
                                                    c.uid as StoreUID,
                                                    c.name as StoreName,
                                                    sh.is_planned as IsPlanned,
                                                    case
                                                        when sh.login_time is null and sh.is_skipped = 1 then 'Skipped'
                                                        when sh.login_time is null then 'Pending'
                                                        else sh.status
                                                    end as VisitStatus,
                                                    sh.login_time as VisitTime
                                                from
                                                    store_history sh 
                                                    inner join store c on sh.store_uid = c.uid
                                                where 
                                                    sh.beat_history_uid = @BeatHistoryUID 
                                                    and (sh.is_planned = 1 or sh.status = 'Visited')
                                                order by 
                                                    sh.serial_no");
                var parameters = new Dictionary<string, object>()
                    {
                        {"BeatHistoryUID",BeatHistoryUID }
                    };
                IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid> beatHistoryList = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid>(sql.ToString(), parameters);
                return beatHistoryList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (
                                                        select 
                                                            '[' + e.code + '] ' + e.name as [User],
                                                            uj.journey_start_time as JourneyDate,
                                                            uj.uid,
                                                            uj.eot_status as EOTStatus,
                                                            uj.journey_start_time as StartTime,
                                                            uj.journey_end_time as EndTime
                                                        from 
                                                            user_journey uj
                                                            join emp e on uj.emp_uid = e.uid
                                                    ) as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select count(1) as Cnt from (
                                                                    select 
                                                                        e.code + e.name as [User],
                                                                        uj.journey_start_time as JourneyDate,
                                                                        uj.uid,
                                                                        uj.eot_status as EOTStatus,
                                                                        uj.journey_start_time as StartTime,
                                                                        uj.journey_end_time as EndTime
                                                                    from 
                                                                        user_journey uj 
                                                                        join emp e on uj.emp_uid = e.uid
                                                                ) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>(filterCriterias, sbFilterCriteria, parameters); ;

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
                IEnumerable<Model.Interfaces.IUserJourneyGrid> assignedJourneys = await ExecuteQueryAsync<Model.Interfaces.IUserJourneyGrid>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>
                {
                    PagedData = assignedJourneys,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyDetailsByUID(string UID)
        {
            try
            {
                var sql = new StringBuilder(@"select 
                                                    e.code + e.name as User,
                                                    uj.journey_start_time as JourneyDate,
                                                    uj.eot_status as EOTStatus,
                                                    uj.journey_start_time as StartTime,
                                                    uj.journey_end_time as EndTime,
                                                    uj.journey_time as JourneyTime,
                                                    uj.start_odometer_reading as StartOdometerReading,
                                                    uj.end_odometer_reading as EndOdometerReading,
                                                    uj.end_odometer_reading - uj.start_odometer_reading as TotalReading,
                                                    uj.is_synchronizing as IsSynchronizing,
                                                    uj.has_internet as HasInternet,
                                                    uj.internet_type as InternetType,
                                                    case when uj.battery_percentage_available >= uj.battery_percentage_target then 'OK' else 'Not Ok' end as BatteryStatus,
                                                    uj.is_location_enabled as IsLocationEnabled,
                                                    uj.battery_percentage_target as BatteryPercentageTarget,
                                                    uj.has_mobile_network as HasMobileNetwork,
                                                    uj.download_speed as DownloadSpeed,
                                                    uj.upload_speed as UploadSpeed,
                                                    uj.battery_percentage_available as BatteryPercentageAvailable,
                                                    uj.attendance_status as AttendanceStatus,
                                                    uj.attendance_latitude as AttendanceLatitude,
                                                    uj.attendance_longitude as AttendanceLongitude,
                                                    fs.relative_path + '/' + fs.file_name as ImagePath
                                                from 
                                                    user_journey uj
                                                    join emp e on uj.emp_uid = e.uid
                                                    left join file_sys fs on fs.linked_item_uid = uj.uid and fs.linked_item_type = 'Attendance'
                                                where 
                                                    uj.uid = @UID;");
                var parameters = new Dictionary<string, object>() { { "uid", UID } };

                Model.Interfaces.IUserJourneyView? userJourneyView = await ExecuteSingleAsync<Model.Interfaces.IUserJourneyView>(sql.ToString(), parameters);
                return userJourneyView;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
