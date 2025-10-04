using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Route.DL.Classes;

public class MSSQLRoute : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IRouteDL
{
    public MSSQLRoute(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }
    public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute>> SelectRouteAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
    {
        try
        {
            StringBuilder sql = new(@"select * from(SELECT Distinct id AS Id,
                                                        uid AS UID,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime, 
                                                        company_uid AS CompanyUID,
                                                        code AS Code,
                                                        name AS Name,
                                                        role_uid AS RoleUID,
                                                        org_uid AS OrgUID,
                                                        wh_org_uid AS WHOrgUID,
                                                        vehicle_uid AS VehicleUID,
                                                        job_position_uid AS JobPositionUID,
                                                        location_uid AS LocationUID,
                                                        is_active AS IsActive,
                                                        status AS Status,
                                                        valid_from AS ValidFrom,
                                                        valid_upto AS ValidUpto,
                                                        print_standing AS PrintStanding,
                                                        print_topup AS PrintTopup,
                                                        print_order_summary AS PrintOrderSummary,
                                                        auto_freeze_jp AS AutoFreezeJP,
                                                        add_to_run AS AddToRun,
                                                        auto_freeze_run_time AS AutoFreezeRunTime,
                                                        ss AS SS,
                                                        total_customers AS TotalCustomers,
                                                        print_forward AS PrintForward,
                                                        visit_time AS VisitTime,
                                                        end_time AS EndTime,
                                                        visit_duration AS VisitDuration,
                                                        travel_time AS TravelTime,
                                                        is_customer_with_time AS IsCustomerWithTime
                                                        FROM route WHERE org_uid = @OrgUID)as SubQuery");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT Distinct COUNT(1) AS Cnt FROM (SELECT Distinct id AS Id,
                                                        uid AS UID,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime, 
                                                        company_uid AS CompanyUID,
                                                        code AS Code,
                                                        name AS Name,
                                                        role_uid AS RoleUID,
                                                        org_uid AS OrgUID,
                                                        wh_org_uid AS WHOrgUID,
                                                        vehicle_uid AS VehicleUID,
                                                        job_position_uid AS JobPositionUID,
                                                        location_uid AS LocationUID,
                                                        is_active AS IsActive,
                                                        status AS Status,
                                                        valid_from AS ValidFrom,
                                                        valid_upto AS ValidUpto,
                                                        print_standing AS PrintStanding,
                                                        print_topup AS PrintTopup,
                                                        print_order_summary AS PrintOrderSummary,
                                                        auto_freeze_jp AS AutoFreezeJP,
                                                        add_to_run AS AddToRun,
                                                        auto_freeze_run_time AS AutoFreezeRunTime,
                                                        ss AS SS,
                                                        total_customers AS TotalCustomers,
                                                        print_forward AS PrintForward,
                                                        visit_time AS VisitTime,
                                                        end_time AS EndTime,
                                                        visit_duration AS VisitDuration,
                                                        travel_time AS TravelTime,
                                                        is_customer_with_time AS IsCustomerWithTime
                                                        FROM route WHERE org_uid = @OrgUID) as SubQuery");
            }
            Dictionary<string, object?> parameters = new()
            {
                {"OrgUID",OrgUID },
            };

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" where  ");
                AppendFilterCriteria<Winit.Modules.Route.Model.Interfaces.IRoute>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
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

            IEnumerable<Model.Interfaces.IRoute> routes = await ExecuteQueryAsync<Model.Interfaces.IRoute>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute> pagedResponse = new()
            {
                PagedData = routes,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>> SelectRouteChangeLogAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT * FROM (SELECT e.login_id AS [User],
                                                        r.id AS Id,
                                                        r.uid AS [UID],
                                                        r.created_by AS CreatedBy,
                                                        r.created_time AS CreatedTime,
                                                        r.modified_by AS ModifiedBy,
                                                        r.modified_time AS ModifiedTime,
                                                        r.server_add_time AS ServerAddTime,
                                                        r.server_modified_time AS ServerModifiedTime,
                                                        r.company_uid AS CompanyUID,
                                                        r.code AS Code,
                                                        r.name AS Name,
                                                        r.role_uid AS RoleUID,
                                                        r.org_uid AS OrgUID,
                                                        r.wh_org_uid AS WHOrgUID,
                                                        r.vehicle_uid AS VehicleUID,
                                                        r.job_position_uid AS JobPositionUID,
                                                        r.location_uid AS LocationUID,
                                                        r.is_active AS IsActive,
                                                        r.status AS Status,
                                                        r.valid_from AS ValidFrom,
                                                        r.valid_upto AS ValidUpto,
                                                        r.print_standing AS PrintStanding,
                                                        r.auto_freeze_jp AS AutoFreezeJP,
                                                        r.add_to_run AS AddToRun,
                                                        r.auto_freeze_run_time AS AutoFreezeRunTime,
                                                        r.ss AS SS,
                                                        r.is_change_applied AS IsChangeApplied,
                                                        r.total_customers AS TotalCustomers,
                                                        r.print_forward AS PrintForward, 
                                                        r.visit_time AS VisitTime,
                                                        r.end_time AS EndTime,
                                                        r.visit_duration AS VisitDuration,
                                                        r.travel_time AS TravelTime,
                                                        r.is_customer_with_time AS IsCustomerWithTime
                                                    FROM route_change_log r
                                             INNER JOIN job_position jp ON jp.uid = r.job_position_uid
                                                INNER JOIN emp e ON e.uid = jp.emp_uid)AS SUBQUERY");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM(SELECT e.login_id AS [user],
                                                        r.id AS Id,
                                                        r.uid AS [UID],
                                                        r.created_by AS CreatedBy,
                                                        r.created_time AS CreatedTime,
                                                        r.modified_by AS ModifiedBy,
                                                        r.modified_time AS ModifiedTime,
                                                        r.server_add_time AS ServerAddTime,
                                                        r.server_modified_time AS ServerModifiedTime,
                                                        r.company_uid AS CompanyUID,
                                                        r.code AS Code,
                                                        r.name AS Name,
                                                        r.role_uid AS RoleUID,
                                                        r.org_uid AS OrgUID,
                                                        r.wh_org_uid AS WHOrgUID,
                                                        r.vehicle_uid AS VehicleUID,
                                                        r.job_position_uid AS JobPositionUID,
                                                        r.location_uid AS LocationUID,
                                                        r.is_active AS IsActive,
                                                        r.status AS Status,
                                                        r.valid_from AS ValidFrom,
                                                        r.valid_upto AS ValidUpto,
                                                        r.print_standing AS PrintStanding,
                                                        r.auto_freeze_jp AS AutoFreezeJP,
                                                        r.add_to_run AS AddToRun,
                                                        r.auto_freeze_run_time AS AutoFreezeRunTime,
                                                        r.ss AS SS,
                                                        r.is_change_applied AS IsChangeApplied,
                                                        r.total_customers AS TotalCustomers,
                                                        r.print_forward AS PrintForward, 
                                                        r.visit_time AS VisitTime,
                                                        r.end_time AS EndTime,
                                                        r.visit_duration AS VisitDuration,
                                                        r.travel_time AS TravelTime,
                                                        r.is_customer_with_time AS IsCustomerWithTime
                                                    FROM route_change_log r
                                             INNER JOIN job_position jp ON jp.uid = r.job_position_uid
                                                INNER JOIN emp e ON e.uid = jp.emp_uid)AS SUBQUERY");
            }
            Dictionary<string, object?> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
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
            IEnumerable<Model.Interfaces.IRouteChangeLog> routes = await ExecuteQueryAsync<Model.Interfaces.IRouteChangeLog>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog> pagedResponse = new()
            {
                PagedData = routes,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Winit.Modules.Route.Model.Interfaces.IRoute> SelectRouteDetailByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
            string sql = @"SELECT id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime, 
                            company_uid AS CompanyUID,
                            code AS Code,
                            name AS Name,
                            role_UID AS RoleUID,
                            org_uid AS OrgUID,
                            wh_org_uid AS WHOrgUID,
                            vehicle_uid AS VehicleUID,
                            job_position_uid AS JobPositionUID,
                            location_uid AS LocationUID,
                            is_active AS IsActive,
                            status AS Status,
                            valid_from AS ValidFrom,
                            valid_upto AS ValidUpto,
                            print_standing AS PrintStanding,
                            print_topup AS PrintTopup,
                            print_order_summary AS PrintOrderSummary,
                            auto_freeze_jp AS AutoFreezeJP,
                            add_to_run AS AddToRun,
                            auto_freeze_run_time AS AutoFreezeRunTime,
                            ss AS SS,
                            total_customers AS TotalCustomers,
                            print_forward AS PrintForward,
                            visit_time AS VisitTime,
                            end_time AS EndTime,
                            visit_duration AS VisitDuration,
                            travel_time AS TravelTime,
                            is_customer_with_time AS IsCustomerWithTime FROM route WHERE uid = @UID";
            Model.Interfaces.IRoute RouteList = await ExecuteSingleAsync<Model.Interfaces.IRoute>(sql, parameters);
            return RouteList;
        }
        catch
        {
            throw;
        }
      
    }

    public async Task<int> CreateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
    {
        try 
        {
            string sql = @"INSERT INTO route (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                      job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                      modified_time, server_add_time, server_modified_time,
                                       print_standing,print_forward,print_topup,print_order_summary,auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers) 
                                       VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                       @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                       @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                       @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers);";
            return await ExecuteNonQueryAsync(sql, routeDetails);
        }
        catch
        {
            throw;
        }
        
    }

    public async Task<int> UpdateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
    {
        try
        {
            string sql = @"UPDATE route SET  code = @Code, name = @Name,
                         role_uid = @RoleUID, is_active = @IsActive, status = @Status, valid_from = @ValidFrom , 
                         valid_upto = @ValidUpto, modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                         server_modified_time = @ServerModifiedTime,print_standing=@PrintStanding,
                        print_forward=@PrintForward,print_topup=@PrintTopup,print_order_summary=@PrintOrderSummary,auto_freeze_jp=@AutoFreezeJP,
                          add_to_run=@AddToRun,auto_freeze_run_time=@AutoFreezeRunTime,total_customers=@TotalCustomers  WHERE uid = @UID;";

            
            return await ExecuteNonQueryAsync(sql, routeDetails);

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteRouteDetail(string UID)
    { int count = -1;
        try
        {
            Dictionary<string, object?> parameters = new()
            {
            {"UID",  UID}
            };
            string sql = @"DELETE  FROM route WHERE uid = @UID";
            count= await ExecuteNonQueryAsync(sql, parameters);
        }
        catch
        {

        }return count; 
        

    }

    public async Task<int> CreateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
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
                        string routeQuery = @"
                                        INSERT INTO route (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                        job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                        modified_time, server_add_time, server_modified_time,
                                        print_standing,print_forward,print_topup,print_order_summary,auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers,
                                        visit_time,end_time,visit_duration,travel_time,is_customer_with_time) 
                                        VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                        @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                        @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                        @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers,@VisitTime,@EndTime,@VisitDuration,@TravelTime,@IsCustomerWithTime)
                                        ";

                        count += await ExecuteNonQueryAsync(routeQuery, connection, transaction, routeMasterDetails.Route);

                        string routeChangeLogQuery = @"INSERT INTO route_change_log (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                                      job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                                      modified_time, server_add_time, server_modified_time,print_standing,print_forward,print_topup,print_order_summary,
                                                      auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers,visit_time,end_time,visit_duration,travel_time,is_customer_with_time) 
                                                      VALUES 
                                                     (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,
                                                      @Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                                                      @PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,@AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers,
                                                      @VisitTime,@EndTime,@VisitDuration,@TravelTime,@IsCustomerWithTime)";
                        count += await ExecuteNonQueryAsync(routeChangeLogQuery, connection, transaction, routeMasterDetails.Route);

                        string routeScheduleQuery = @"Insert Into route_schedule(uid,created_by,created_time,modified_by,modified_time,server_add_time,
                                                      server_modified_time,company_uid,route_uid,name,type,start_date,status,from_date,to_date,start_time,end_time,
                                                      visit_duration_in_minutes,travel_time_in_minutes,last_beat_date,next_beat_date,allow_multiple_beats_per_day,planned_days,ss)
                                                      values(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@CompanyUID,@RouteUID,@Name,@Type,
                                                      @StartDate,@Status,@FromDate,@ToDate,@StartTime,@EndTime,@VisitDurationInMinutes,@TravelTimeInMinutes,@LastBeatDate,
                                                      @NextBeatDate,@AllowMultipleBeatsPerDay,@PlannedDays,@SS)";
                        count += await ExecuteNonQueryAsync(routeScheduleQuery, connection, transaction, routeMasterDetails.RouteSchedule);
                        // Insert route schedule configs
                        if (routeMasterDetails.RouteScheduleConfigs != null && routeMasterDetails.RouteScheduleConfigs.Any())
                        {
                            string routeScheduleConfigQuery = @"INSERT INTO route_schedule_config(uid, created_by, created_time, modified_by, modified_time,
                                                                server_add_time, server_modified_time, schedule_type, week_number, day_number, is_deleted) 
                                                                VALUES(@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                                                @ServerAddTime, @ServerModifiedTime, @ScheduleType, @WeekNumber, @DayNumber, @IsDeleted)";
                            count += await ExecuteNonQueryAsync(routeScheduleConfigQuery, connection, transaction, routeMasterDetails.RouteScheduleConfigs);
                        }

                        // Insert route schedule customer mappings
                        if (routeMasterDetails.RouteScheduleCustomerMappings != null && routeMasterDetails.RouteScheduleCustomerMappings.Any())
                        {
                            string routeScheduleCustomerMappingQuery = @"INSERT INTO route_schedule_customer_mapping(uid, created_by, created_time, modified_by, modified_time,
                                                                        server_add_time, server_modified_time, route_schedule_uid, route_schedule_config_uid, customer_uid, 
                                                                        seq_no, start_time, end_time, is_deleted) 
                                                                        VALUES(@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                                                        @ServerAddTime, @ServerModifiedTime, @RouteScheduleUID, @RouteScheduleConfigUID, @CustomerUID, 
                                                                        @SeqNo, @StartTime, @EndTime, @IsDeleted)";
                            count += await ExecuteNonQueryAsync(routeScheduleCustomerMappingQuery, connection, transaction, routeMasterDetails.RouteScheduleCustomerMappings);
                        }

                        if (routeMasterDetails.RouteCustomersList != null && routeMasterDetails.RouteCustomersList.Any())
                        {
                            string routeCustomerQuery = @"INSERT INTO route_customer (uid, route_uid, store_uid, seq_no, visit_time, visit_duration,
                            end_time,is_deleted,created_by, created_time,modified_by,   modified_time, server_add_time, server_modified_time,travel_time)  
                            VALUES (@UID, @RouteUID, @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime,@TravelTime);";
                            count += await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, routeMasterDetails.RouteCustomersList);
                        }

                        if (routeMasterDetails.RouteUserList != null && routeMasterDetails.RouteUserList.Any()) { }
                        {
                            string routeUSerQuery = @"INSERT INTO route_user (uid, created_by, created_time, modified_by, modified_time, 
                                                          server_add_time, server_modified_time, route_uid, job_position_uid, from_date, to_date,is_active) 
                                                          VALUES 
                                                          (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID, 
                                                          @JobPositionUID, @FromDate, @ToDate, @IsActive);";
                            count += await ExecuteNonQueryAsync(routeUSerQuery, connection, transaction, routeMasterDetails.RouteUserList);
                        }
                        transaction.Commit();
                        int total = count;
                        return total;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error during transaction", ex);
                    }
                }
            }
            }
        catch (Exception ex)
        {
            throw new Exception("Error outside transaction", ex);
        }
    }


    public async Task<int> UpdateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
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
                        string routeChangeLogQuery = @"UPDATE route_change_log SET
                        company_uid = @CompanyUID,
                        code = @Code,
                        name = @Name,
                        role_uid = @RoleUID,
                        is_active = @IsActive,
                        status = @Status,
                        valid_from = @ValidFrom,
                        valid_upto = @ValidUpto,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        print_standing = @PrintStanding,
                        print_forward = @PrintForward ,
                        print_topup = @PrintTopup,
                        print_order_summary = @PrintOrderSummary,
                        auto_freeze_jp = @AutoFreezeJP,
                        add_to_run = @AddToRun,
                        auto_freeze_run_time = @AutoFreezeRunTime,
                        total_customers =@TotalCustomers,
                        job_position_uid = @JobPositionUID,
                        visit_time = @VisitTime,
                        end_time = @EndTime,
                        visit_duration = @VisitDuration,
                        travel_time = @TravelTime,
                        is_customer_with_time = @IsCustomerWithTime
                        WHERE uid = @UID";
                        count = await ExecuteNonQueryAsync(routeChangeLogQuery, connection, transaction, routeMasterDetails.Route);

                        if (routeMasterDetails.Route != null)
                        {
                            string routeQuery = @"UPDATE route SET
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        visit_time = @VisitTime,
                        end_time = @EndTime,
                        visit_duration = @VisitDuration,
                        travel_time = @TravelTime,
                        is_customer_with_time = @IsCustomerWithTime
                        WHERE uid = @UID";

                            count = await ExecuteNonQueryAsync(routeQuery, connection, transaction, routeMasterDetails.Route);
                        }
                        if (routeMasterDetails.RouteSchedule != null)
                        {
                            string routeScheduleQuery = @"UPDATE route_schedule SET
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        name = @Name,
                        type = @Type,
                        start_date = @StartDate,
                        status = @Status,
                        from_date = @FromDate,
                        to_date = @ToDate,
                        start_time = @StartTime,
                        end_time = @EndTime,
                        visit_duration_in_minutes = @VisitDurationInMinutes,
                        travel_time_in_minutes = @TravelTimeInMinutes,
                        last_beat_date = @LastBeatDate,
                        next_beat_date = @NextBeatDate,
                        allow_multiple_beats_per_day = @AllowMultipleBeatsPerDay,
                        planned_days = @PlannedDays,
                        ss = @SS
                        WHERE uid = @UID";

                            count += await ExecuteNonQueryAsync(routeScheduleQuery, connection, transaction, routeMasterDetails.RouteSchedule);

                        }

                        // Note: Old RouteScheduleDaywise and RouteScheduleFortnight tables are no longer used.
                        // New scheduling system uses RouteScheduleConfigs and RouteScheduleCustomerMappings.
                        // Update logic for new tables can be added here if needed.
                        if (routeMasterDetails.RouteCustomersList != null && routeMasterDetails.RouteCustomersList.Count > 0)
                        {
                            foreach (RouteCustomer routeCustomer in routeMasterDetails.RouteCustomersList)
                            {
                                List<string> uidsFromList= routeMasterDetails.RouteCustomersList.Select(e=>e.UID).ToList();
                                List<string>? existinUIDs = await CheckIfUIDExistsInDB(DbTableName.RouteCustomer, uidsFromList);
                                List<RouteCustomer> newRouteCustomerList = null;
                                List<RouteCustomer> existingRouteCustomerList = null;
                                if(existinUIDs!=null && existinUIDs.Any())
                                {
                                    newRouteCustomerList= routeMasterDetails.RouteCustomersList.Where(rc=>!existinUIDs.Contains(rc.UID)).ToList();
                                    existingRouteCustomerList = routeMasterDetails.RouteCustomersList.Where(rc=>existinUIDs.Contains(rc.UID)).ToList();
                                }
                                else
                                {
                                    newRouteCustomerList = routeMasterDetails.RouteCustomersList;
                                }
                                if(existingRouteCustomerList!=null && existingRouteCustomerList.Count > 0)
                                {
                                    count += await UpdateRouteCustomer(connection, transaction, existingRouteCustomerList);
                                }
                                if (newRouteCustomerList!=null && newRouteCustomerList.Any())
                                {
                                    count += await InsertRouteCustomer(connection, transaction, newRouteCustomerList);
                                }
                            }
                        }

                        if (routeMasterDetails.RouteUserList != null && routeMasterDetails.RouteUserList.Count > 0)
                        {
                            List<string> uidList = routeMasterDetails.RouteUserList.Select(po => po.UID).ToList();
                            List<string> deletedUidList = routeMasterDetails.RouteUserList.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();
                            List<string>? existinUIDs = await CheckIfUIDExistsInDB(DbTableName.RouteUser, uidList);
                            List<RouteUser>? newRouteUsers = null;
                            List<RouteUser>? existingRouteUsers = null;
                            if (routeMasterDetails.RouteUserList.Any(e => e.ActionType == ActionType.Add))
                            {
                                if (existinUIDs != null && existinUIDs.Any())
                                {
                                    newRouteUsers = routeMasterDetails.RouteUserList.Where(e => !existinUIDs.Contains(e.UID)).ToList();
                                    existingRouteUsers = routeMasterDetails.RouteUserList.Where(e => existinUIDs.Contains(e.UID)).ToList();
                                }
                                else
                                {
                                    newRouteUsers = routeMasterDetails.RouteUserList;
                                }
                                if (existingRouteUsers != null && existingRouteUsers.Any())
                                {
                                    count += await UpdateRouteUser(connection, transaction, existingRouteUsers);
                                }
                                if (newRouteUsers != null && newRouteUsers.Any())
                                {
                                    count += await CreateRouteUser(connection, transaction, newRouteUsers);
                                }
                            }
                            if(routeMasterDetails.RouteUserList.Any(e => e.ActionType == ActionType.Delete))
                            {
                                count += await DeleteRouteUser(deletedUidList);
                            }
                        }
                        transaction.Commit();
                        int total = count;
                        return total;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            }
               

           
           
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<int> InsertRouteCustomer(IDbConnection connection, IDbTransaction transaction, List<RouteCustomer> routeCustomerList)
    {
        int count = -1;
        try
        {
            string routeCustomerQuery = @"INSERT INTO route_customer (uid, route_uid, store_uid, seq_no, visit_time, visit_duration,
                            end_time,is_deleted,created_by, created_time,modified_by,   modified_time, server_add_time, server_modified_time)  
                            VALUES (@UID, @RouteUID, @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime);";
            count= await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, routeCustomerList);
        }
        catch
        {
            throw;
        }
        return count;
      
       
    }

    private async Task<int> UpdateRouteCustomer(IDbConnection connection, IDbTransaction transaction,List<RouteCustomer>routeCustomers)
    {
        int count=-1;
        try
        {
            string routeCustomerQuery = @"UPDATE route_customer SET
                      seq_no = @SeqNo,
                      visit_time = @VisitTime,
                      visit_duration = @VisitDuration,
                      end_time = @EndTime,
                      is_deleted = @IsDeleted,
                      modified_by = @ModifiedBy,
                      modified_time = @ModifiedTime,
                      server_modified_time = @ServerModifiedTime,
                      travel_time = @TravelTime
                      WHERE uid = @UID";

            count += await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, routeCustomers);
        }
        catch
        {
            throw;
        }
        return count;
    }
    public async Task<(List<Model.Interfaces.IRouteChangeLog>, List<Model.Interfaces.IRouteSchedule>,
        List<Model.Interfaces.IRouteScheduleConfig>, List<Model.Interfaces.IRouteScheduleCustomerMapping>,
        List<Model.Interfaces.IRouteCustomer>, List<Model.Interfaces.IRouteUser>)> SelectRouteMasterViewByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> Parameters = new()
            {
                { "UID", UID },
            };
            StringBuilder routeSql = new(@"SELECT
                                            id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, code AS Code,
                                            name AS Name, role_uid AS RoleUID, org_uid AS OrgUID, wh_org_uid AS WhOrgUID, vehicle_uid AS VehicleUID,
                                            job_position_uid AS JobPositionUID, location_uid AS LocationUID, is_active AS IsActive, status AS Status,
                                            valid_from AS ValidFrom, valid_upto AS ValidUpto, print_standing AS PrintStanding, 
                                            print_forward AS PrintForward, print_topup AS PrintTopup, print_order_summary AS PrintOrderSummary,
                                            auto_freeze_jp AS AutoFreezeJp, add_to_run AS AddToRun, auto_freeze_run_time AS AutoFreezeRunTime,
                                            ss AS SS, total_customers AS TotalCustomers, visit_time AS VisitTime, end_time AS EndTime,
                                            visit_duration AS VisitDuration, travel_time AS TravelTime, is_customer_with_time AS IsCustomerWithTime
                                            FROM route_change_log
                                            WHERE uid = @UID;");
            Type routeSqlType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteChangeLog>().GetType();
            List<Model.Interfaces.IRouteChangeLog> routeList = await ExecuteQueryAsync<Model.Interfaces.IRouteChangeLog>(routeSql.ToString(), Parameters, routeSqlType);
            StringBuilder routeSheduleSQL = new(@"SELECT
                                                    id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                                    server_modified_time AS ServerModifiedTime, company_uid AS CompanyUid,
                                                    route_uid AS RouteUID, name AS Name, Type, start_date AS StartDate,
                                                    status AS Status, from_date AS FromDate, to_date AS ToDate, start_time AS StartTime,
                                                    end_time AS EndTime, visit_duration_in_minutes AS VisitDurationInMinutes,
                                                    travel_time_in_minutes AS TravelTimeInMinutes, last_beat_date AS LastBeatDate,
                                                    next_beat_date AS NextBeatDate, allow_multiple_beats_per_day AS AllowMultipleBeatsPerDay,
                                                    planned_days AS PlannedDays, ss AS SS
                                                    FROM route_schedule
                                                    WHERE route_uid = @UID;");
            Type routeSheduleType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteSchedule>().GetType();
            List<Model.Interfaces.IRouteSchedule> routeSheduleList = await ExecuteQueryAsync<Model.Interfaces.IRouteSchedule>(routeSheduleSQL.ToString(), Parameters, routeSheduleType);
            StringBuilder routeScheduleDayWiseSQL = new(@"SELECT
                                                            id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                            modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                                                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                            ss AS SS, route_schedule_uid AS RouteScheduleUID, monday AS Monday,
                                                            tuesday AS Tuesday, wednesday AS Wednesday, thursday AS Thursday,
                                                            friday AS Friday, saturday AS Saturday, sunday AS Sunday
                                                            FROM route_schedule_daywise
                                                            WHERE route_schedule_uid = @UID;");
            Type routeScheduleDayWiseType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleDaywise>().GetType();
            List<Model.Interfaces.IRouteScheduleDaywise> routeScheduleDayWiseList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleDaywise>(routeScheduleDayWiseSQL.ToString(), Parameters, routeScheduleDayWiseType);

            StringBuilder routeScheduleFortnightSQL = new(@"SELECT
                                                            id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                            modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                                                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                            ss AS SS, company_uid AS CompanyUID, route_schedule_uid AS RouteScheduleUID,
                                                            monday AS Monday, tuesday AS Tuesday, wednesday AS Wednesday,
                                                            thursday AS Thursday, friday AS Friday, saturday AS Saturday,
                                                            sunday AS Sunday, monday_fn AS MondayFn, tuesday_fn AS TuesdayFn,
                                                            wednesday_fn AS WednesdayFn, thursday_fn AS ThursdayFn, friday_fn AS FridayFn,
                                                            saturday_fn AS SaturdayFn, sunday_fn AS SundayFn 
                                                            FROM route_schedule_fortnight
                                                            WHERE route_schedule_uid = @UID;");
            Type routeScheduleFortnightType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleFortnight>().GetType();
            List<Model.Interfaces.IRouteScheduleFortnight> routeScheduleFortnightList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleFortnight>(routeScheduleFortnightSQL.ToString(), Parameters, routeScheduleFortnightType);

            StringBuilder routeCustomerSQL = new(@"SELECT
                                                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                                                        route_uid AS RouteUID, store_uid AS StoreUID, seq_no AS SeqNo,
                                                        visit_time AS VisitTime, visit_duration AS VisitDuration, end_time AS EndTime,
                                                        is_deleted AS IsDeleted, travel_time AS TravelTime
                                                        FROM route_customer
                                                        WHERE route_uid = @UID;");
            Type routeCustomerType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteCustomer>().GetType();
            List<Model.Interfaces.IRouteCustomer> routeCustomerList = await ExecuteQueryAsync<Model.Interfaces.IRouteCustomer>(routeCustomerSQL.ToString(), Parameters, routeCustomerType);
            StringBuilder routeUserSQL = new(@"SELECT
                                                    id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                                    server_modified_time AS ServerModifiedTime, route_uid AS RouteUID,
                                                    job_position_uid AS JobPositionUID, from_date AS FromDate, to_date AS ToDate,
                                                    is_active AS IsActive
                                                    FROM route_user
                                                    WHERE route_uid = @UID;");
            Type routeUSerType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteUser>().GetType();
            List<Model.Interfaces.IRouteUser> routeUserList = await ExecuteQueryAsync<Model.Interfaces.IRouteUser>(routeUserSQL.ToString(), Parameters, routeUSerType);
            // For now, return empty lists for new table types (this implementation needs to be updated to use new tables)
            var emptyScheduleConfigs = new List<Model.Interfaces.IRouteScheduleConfig>();
            var emptyCustomerMappings = new List<Model.Interfaces.IRouteScheduleCustomerMapping>();
            return (routeList, routeSheduleList, emptyScheduleConfigs, emptyCustomerMappings, routeCustomerList, routeUserList);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<ISelectionItem>> GetVehicleDDL(string orgUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"orgUID",  orgUID},
            };
            string sql = @"SELECT 
                            v.uid AS UID, 
                            '[' + v.vehicle_no + '] ' + v.registration_no AS Label
                        FROM 
                            vehicle AS v 
                        WHERE 
                            v.org_uid = @orgUID 
                            AND COALESCE(v.is_active, 0) = 1;";
            return await ExecuteQueryAsync<ISelectionItem>(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<ISelectionItem>> GetWareHouseDDL(string OrgTypeUID, string ParentUID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"OrgTypeUID",  OrgTypeUID},
            {"ParentUID",  ParentUID},
        };
        string sql = @"SELECT O.uid AS UID,'[' + O.code  + '] ' + O.name AS Label FROM org AS O
                       WHERE O.org_type_uid = @OrgTypeUID AND O.parent_uid = @ParentUID ORDER BY O.code;";
        return await ExecuteQueryAsync<ISelectionItem>(sql, parameters);
    }
    public async Task<List<ISelectionItem>> GetUserDDL(string OrgUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"OrgUID",  OrgUID},
            };
            string sql = @"SELECT DISTINCT jp.uid as UID,  '[' + e.code + ']' +
                                e.name AS Label
                                FROM emp AS e INNER JOIN job_position AS jp ON e.uid = jp.emp_uid
                                AND COALESCE(e.status, '') = 'Active' INNER JOIN roles AS r ON r.uid = jp.user_role_uid
                                WHERE jp.org_uid =@OrgUID;";
            return await ExecuteQueryAsync<ISelectionItem>(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    //RouteUSer
    private async Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectRouteUserByUID(List<string> uids)
    {
        var parameters = new {UIDs = uids };
        
        string sql = @"SELECT id AS Id,
                                uid AS UID,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                route_uid AS RouteUID,
                                job_position_uid AS JobPositionUID,
                                from_date AS FromDate,
                                to_date AS ToDate,
                                is_active AS IsActive
                                FROM route_user WHERE uid IN @UIDs;";
        IEnumerable<Model.Interfaces.IRouteUser> RouteUserList = await ExecuteQueryAsync<Model.Interfaces.IRouteUser>(sql, parameters);
        return RouteUserList;
    }
    private async Task<int> CreateRouteUser(IDbConnection connection, IDbTransaction transaction, List<Winit.Modules.Route.Model.Classes.RouteUser> routeUsers)
    {
        int retVal;
        try
        {
            string sql = @"INSERT INTO route_user (uid,created_by,created_time,modified_by,modified_time,
                        server_add_time,server_modified_time,route_uid, job_position_uid,from_date,to_date,is_active)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                         @RouteUID, @JobPositionUID, @FromDate, @ToDate, @IsActive);";
            
            retVal = await ExecuteNonQueryAsync(sql, connection, transaction, routeUsers);

        }
        catch (Exception)
        {
            throw;
        }

        return retVal;
    }

    private async Task<int> UpdateRouteUser(IDbConnection connection, IDbTransaction transaction,List<Winit.Modules.Route.Model.Classes.RouteUser> routeUsers)
    {
        int retVal;
        try
        {
            string sql = @"UPDATE route_user SET 
                            modified_by	=@ModifiedBy
                            ,modified_time	=@ModifiedTime
                            ,server_modified_time	=@ServerModifiedTime
                            ,route_uid	=@RouteUID
                            ,job_position_uid	=@JobPositionUID
                            ,from_date	=@FromDate
                            ,to_date	=@ToDate
                            ,is_active	=@IsActive
                             WHERE uid = @UID;";
            
            retVal = await ExecuteNonQueryAsync(sql, connection, transaction, routeUsers);

        }
        catch (Exception)
        {
            throw;

        }
        return retVal;

    }
    private async Task<int> DeleteRouteUser(List<string> UIDs)
    {
        string commaSeparatedUIDs = string.Join(",", UIDs);
        Dictionary<string, object?> parameters = new()
        {
            {"UIDs" , commaSeparatedUIDs}
        };
        string sql = @"DELETE  FROM route_user WHERE uid IN (SELECT value FROM STRING_SPLIT(@UIDs, ','))";
        return await ExecuteNonQueryAsync(sql, parameters);

    }

    public async Task<List<Model.Interfaces.IRoute>> GetRoutesByStoreUID(string orgUID, string storeUID)
    {
        StringBuilder sql = new StringBuilder(@"SELECT R.* FROM route R ");
        if (!string.IsNullOrEmpty(storeUID))
        {
            sql.Append($"INNER JOIN route_customer RC ON RC.route_uid = R.uid and RC.store_uid = '{storeUID}' ");
        }
        sql.Append($"WHERE R.org_uid = '{orgUID}'");
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRoute>().GetType();
        List<Model.Interfaces.IRoute> routes = await ExecuteQueryAsync<Model.Interfaces.IRoute>(sql.ToString(), null, type);

        return routes;
    }

    public async Task<List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>> GetAllRouteScheduleConfigs()
    {
        string sql = @"SELECT 
                        id AS Id, 
                        uid AS UID, 
                        created_by AS CreatedBy, 
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy, 
                        modified_time AS ModifiedTime, 
                        server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, 
                        schedule_type AS ScheduleType, 
                        week_number AS WeekNumber, 
                        day_number AS DayNumber, 
                        ISNULL(is_deleted, 0) AS IsDeleted
                       FROM route_schedule_config 
                       ORDER BY schedule_type, week_number, day_number";
        
        Type type = _serviceProvider.GetRequiredService<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>().GetType();
        List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig> configs = await ExecuteQueryAsync<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>(sql, null, type);

        return configs;
    }

}
