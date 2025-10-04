using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Route.DL.Classes;

public class PGSQLRoute : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IRouteDL
{
    public PGSQLRoute(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }
    
    // Map PascalCase filter names to database column names
    private string MapFilterNameToDbColumn(string filterName)
    {
        return filterName switch
        {
            "RoleUID" => "role_uid",
            "JobPositionUID" => "job_position_uid", 
            "IsActive" => "is_active",
            "Code" => "code",
            "Name" => "name",
            "OrgUID" => "org_uid",
            "VehicleUID" => "vehicle_uid",
            "Status" => "status",
            "ValidFrom" => "valid_from",
            "ValidUpto" => "valid_upto",
            _ => filterName.ToLower() // Default fallback
        };
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
                                                        FROM route WHERE org_uid = @OrgUID) as SubQUEry");
            }
            Dictionary<string, object?> parameters = new()
            {
                {"OrgUID",OrgUID },
            };

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" AND ");
                
                // Map PascalCase filter names to database column names and apply filters within the subquery
                for (int i = 0; i < filterCriterias.Count; i++)
                {
                    var filter = filterCriterias[i];
                    string dbColumnName = MapFilterNameToDbColumn(filter.Name);

                    // Add AND or OR based on FilterMode (0 = AND, 1 = OR)
                    if (i > 0)
                    {
                        if (filter.FilterMode == FilterMode.Or || filter.FilterMode == (FilterMode)1)
                        {
                            sbFilterCriteria.Append(" OR ");
                        }
                        else
                        {
                            sbFilterCriteria.Append(" AND ");
                        }
                    }

                    if (filter.Type == FilterType.Equal || filter.Type == (FilterType)1)
                    {
                        string paramName = $"filterParam{i}";
                        sbFilterCriteria.Append($"{dbColumnName} = @{paramName}");
                        parameters.Add(paramName, filter.Value);
                    }
                    else if (filter.Type == FilterType.Contains || filter.Type == (FilterType)15 || filter.Type == (FilterType)2)
                    {
                        string paramName = $"filterParam{i}";
                        sbFilterCriteria.Append($"{dbColumnName} ILIKE @{paramName}");
                        parameters.Add(paramName, $"%{filter.Value}%");
                    }
                }

                // Insert the filter criteria within the subquery WHERE clause
                string sqlStr = sql.ToString();
                int whereIndex = sqlStr.IndexOf("WHERE org_uid = @OrgUID");
                if (whereIndex > 0)
                {
                    string beforeWhere = sqlStr.Substring(0, whereIndex + "WHERE org_uid = @OrgUID".Length);
                    string afterWhere = sqlStr.Substring(whereIndex + "WHERE org_uid = @OrgUID".Length);
                    sql.Clear();
                    sql.Append(beforeWhere);
                    sql.Append(sbFilterCriteria);
                    sql.Append(afterWhere);
                }
                
                // If count required then add filters to count
                if (isCountRequired)
                {
                    string sqlCountStr = sqlCount.ToString();
                    int countWhereIndex = sqlCountStr.IndexOf("WHERE org_uid = @OrgUID");
                    if (countWhereIndex > 0)
                    {
                        string beforeCountWhere = sqlCountStr.Substring(0, countWhereIndex + "WHERE org_uid = @OrgUID".Length);
                        string afterCountWhere = sqlCountStr.Substring(countWhereIndex + "WHERE org_uid = @OrgUID".Length);
                        sqlCount.Clear();
                        sqlCount.Append(beforeCountWhere);
                        sqlCount.Append(sbFilterCriteria);
                        sqlCount.Append(afterCountWhere);
                    }
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            }

            //Data
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRoute>().GetType();
            IEnumerable<Model.Interfaces.IRoute> routes = await ExecuteQueryAsync<Model.Interfaces.IRoute>(sql.ToString(), parameters, type);
            //Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
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
            StringBuilder sql = new(@"SELECT * FROM(SELECT e.login_id AS User,
                                                        r.id AS Id,
                                                        r.uid AS UID,
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
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM(SELECT e.login_id AS user,
                                                        r.id AS Id,
                                                        r.uid AS UID,
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
                _ = sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            }

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteChangeLog>().GetType();
            IEnumerable<Model.Interfaces.IRouteChangeLog> routes = await ExecuteQueryAsync<Model.Interfaces.IRouteChangeLog>(sql.ToString(), parameters, type);
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
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRoute>().GetType();
        Model.Interfaces.IRoute RouteList = await ExecuteSingleAsync<Model.Interfaces.IRoute>(sql, parameters, type);
        return RouteList;
    }

    public async Task<int> CreateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
    {
        string sql = @"INSERT INTO route (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                      job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                      modified_time, server_add_time, server_modified_time,
                                       print_standing,print_forward,print_topup,print_order_summary,auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers) 
                                       VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                       @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                       @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                       @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers);";

        Dictionary<string, object?> parameters = new()
        {
                    {"UID", routeDetails.UID},
                    {"CompanyUID",  routeDetails.CompanyUID},
                    {"Code",  routeDetails.Code},
                    {"Name",  routeDetails.Name},
                    {"RoleUID",  routeDetails.RoleUID},
                    {"OrgUID",  routeDetails.OrgUID},
                    {"WHOrgUID",  routeDetails.WHOrgUID},
                    {"VehicleUID",  routeDetails.VehicleUID},
                    {"JobPositionUID",  routeDetails.JobPositionUID},
                    {"LocationUID",  routeDetails.LocationUID},
                    {"IsActive",  routeDetails.IsActive},
                    {"Status",  routeDetails.Status},
                    {"ValidFrom",  routeDetails.ValidFrom},
                    {"ValidUpto",  routeDetails.ValidUpto},
                    {"CreatedTime", DateTime.Now},
                    {"CreatedBy",  routeDetails.CreatedBy},
                    {"ModifiedBy",  routeDetails.ModifiedBy},
                    {"ModifiedTime",  routeDetails.ModifiedTime},
                    {"ServerAddTime",   DateTime.Now},
                    {"ServerModifiedTime",  DateTime.Now},
                    {"PrintStanding",  routeDetails.PrintStanding},
                    {"PrintForward",  routeDetails.PrintForward },
                    {"PrintTopup",  routeDetails.PrintTopup},
                    {"PrintOrderSummary",  routeDetails.PrintOrderSummary},
                    {"AutoFreezeJP",  routeDetails.AutoFreezeJP},
                    {"AddToRun",  routeDetails.AddToRun},
                    {"AutoFreezeRunTime",  routeDetails.AutoFreezeRunTime},
                    {"TotalCustomers",  routeDetails.TotalCustomers},
         };
        return await ExecuteNonQueryAsync(sql, parameters);
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

            Dictionary<string, object?> parameters = new()
            {
                    {"UID", routeDetails.UID},
                    {"Code", routeDetails.Code},
                    {"Name", routeDetails.Name},
                    {"RoleUID", routeDetails.RoleUID},
                    {"IsActive", routeDetails.IsActive},
                    {"Status", routeDetails.Status},
                    {"ValidFrom", routeDetails.ValidFrom},
                    {"ValidUpto", routeDetails.ValidUpto},
                    {"ModifiedBy", routeDetails.ModifiedBy},
                    {"ModifiedTime", routeDetails.ModifiedTime},
                    {"ServerModifiedTime", DateTime.Now},
                     {"PrintStanding",  routeDetails.PrintStanding},
                     {"PrintForward",  routeDetails.PrintForward},
                    {"PrintTopup",  routeDetails.PrintTopup},
                    {"PrintOrderSummary",  routeDetails.PrintOrderSummary},
                    {"AutoFreezeJP",  routeDetails.AutoFreezeJP},
                    {"AddToRun",  routeDetails.AddToRun},
                    {"AutoFreezeRunTime",  routeDetails.AutoFreezeRunTime},
                    {"TotalCustomers" , routeDetails.TotalCustomers },
             };
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteRouteDetail(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"DELETE  FROM route WHERE uid = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);

    }

    public async Task<int> CreateRouteMaster(Winit.Modules.Route.Model.Classes.RouteMaster routeMasterDetails)
    {
        Console.WriteLine("[DEBUG] ========== CreateRouteMaster Started ==========");
        Console.WriteLine($"[DEBUG] RouteCustomersList count: {routeMasterDetails.RouteCustomersList?.Count ?? 0}");
        Console.WriteLine($"[DEBUG] RouteScheduleCustomerMappings count: {routeMasterDetails.RouteScheduleCustomerMappings?.Count ?? 0}");
        Console.WriteLine($"[DEBUG] RouteScheduleConfigs count: {routeMasterDetails.RouteScheduleConfigs?.Count ?? 0}");
        
        if (routeMasterDetails.RouteScheduleCustomerMappings != null && routeMasterDetails.RouteScheduleCustomerMappings.Any())
        {
            foreach (var mapping in routeMasterDetails.RouteScheduleCustomerMappings)
            {
                Console.WriteLine($"[DEBUG] Incoming mapping - CustomerUID: {mapping.CustomerUID}, ConfigUID: {mapping.RouteScheduleConfigUID}, ScheduleUID: {mapping.RouteScheduleUID}");
            }
        }
        else
        {
            Console.WriteLine("[WARNING] No RouteScheduleCustomerMappings received!");
        }
        
        int count = 0;
        try
        {
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string routeQuery = @"INSERT INTO route (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                      job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                      modified_time, server_add_time, server_modified_time,
                                       print_standing,print_forward,print_topup,print_order_summary,auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers
                                    ,visit_time,end_time,visit_duration,travel_time,is_customer_with_time) 
                                       VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                       @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                       @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                       @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers,@VisitTime,@EndTime,@VisitDuration,@TravelTime,@IsCustomerWithTime)";
                Dictionary<string, object?> routeParameters = new()
                {
                    {"UID", routeMasterDetails.Route.UID},
                    {"CompanyUID",  routeMasterDetails.Route.CompanyUID},
                    {"Code",  routeMasterDetails.Route.Code},
                    {"Name",  routeMasterDetails.Route.Name},
                    {"RoleUID",  routeMasterDetails.Route.RoleUID},
                    {"OrgUID",  routeMasterDetails.Route.OrgUID},
                    {"WHOrgUID",  routeMasterDetails.Route.WHOrgUID},
                    {"VehicleUID",  routeMasterDetails.Route.VehicleUID},
                    {"JobPositionUID",  routeMasterDetails.Route.JobPositionUID},
                    {"LocationUID",  routeMasterDetails.Route.LocationUID},
                    {"IsActive",  routeMasterDetails.Route.IsActive},
                    {"Status",  routeMasterDetails.Route.Status},
                    {"ValidFrom",  routeMasterDetails.Route.ValidFrom},
                    {"ValidUpto",  routeMasterDetails.Route.ValidUpto},
                    {"CreatedTime", DateTime.Now},
                    {"CreatedBy",  routeMasterDetails.Route.CreatedBy},
                    {"ModifiedBy",  routeMasterDetails.Route.ModifiedBy},
                    {"ModifiedTime",  routeMasterDetails.Route.ModifiedTime},
                    {"ServerAddTime",   DateTime.Now},
                    {"ServerModifiedTime",  DateTime.Now},
                    {"PrintStanding",  routeMasterDetails.Route.PrintStanding},
                    {"PrintForward",  routeMasterDetails.Route.PrintForward },
                    {"PrintTopup",  routeMasterDetails.Route.PrintTopup},
                    {"PrintOrderSummary",  routeMasterDetails.Route.PrintOrderSummary},
                    {"AutoFreezeJP",  routeMasterDetails.Route.AutoFreezeJP},
                    {"AddToRun",  routeMasterDetails.Route.AddToRun},
                    {"AutoFreezeRunTime",  routeMasterDetails.Route.AutoFreezeRunTime},
                    {"TotalCustomers",  routeMasterDetails.Route.TotalCustomers},
                    {"VisitTime" , routeMasterDetails.Route.VisitTime },
                    {"EndTime" , routeMasterDetails.Route.EndTime },
                    {"VisitDuration" , routeMasterDetails.Route.VisitDuration },
                    {"TravelTime" , routeMasterDetails.Route.TravelTime },
                    {"IsCustomerWithTime" , routeMasterDetails.Route.IsCustomerWithTime },

                };

                int routeInsertResult = await ExecuteNonQueryAsync(routeQuery, connection, transaction, routeParameters);
                if (routeInsertResult <= 0)
                {
                    transaction.Rollback();
                    throw new Exception("Route Insert failed");
                }
                count += routeInsertResult;

                string routeChangeLogQuery = @"INSERT INTO route_change_log (uid, company_uid, code, name, role_uid, org_uid,wh_org_uid,vehicle_uid,
                                      job_position_uid,location_uid,is_active,status,valid_from,valid_upto,created_by, created_time,modified_by, 
                                      modified_time, server_add_time, server_modified_time,
                                       print_standing,print_forward,print_topup,print_order_summary,auto_freeze_jp,add_to_run,auto_freeze_run_time,total_customers
                                    ,visit_time,end_time,visit_duration,travel_time,is_customer_with_time) 
                                       VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                       @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                       @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                       @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers,@VisitTime,@EndTime,@VisitDuration,@TravelTime,@IsCustomerWithTime)";
                Dictionary<string, object?> routeChangeLogParameters = new()
                {
                    {"UID", routeMasterDetails.Route.UID},
                    {"CompanyUID",  routeMasterDetails.Route.CompanyUID},
                    {"Code",  routeMasterDetails.Route.Code},
                    {"Name",  routeMasterDetails.Route.Name},
                    {"RoleUID",  routeMasterDetails.Route.RoleUID},
                    {"OrgUID",  routeMasterDetails.Route.OrgUID},
                    {"WHOrgUID",  routeMasterDetails.Route.WHOrgUID},
                    {"VehicleUID",  routeMasterDetails.Route.VehicleUID},
                    {"JobPositionUID",  routeMasterDetails.Route.JobPositionUID},
                    {"LocationUID",  routeMasterDetails.Route.LocationUID},
                    {"IsActive",  routeMasterDetails.Route.IsActive},
                    {"Status",  routeMasterDetails.Route.Status},
                    {"ValidFrom",  routeMasterDetails.Route.ValidFrom},
                    {"ValidUpto",  routeMasterDetails.Route.ValidUpto},
                    {"CreatedTime", routeMasterDetails.Route.CreatedTime},
                    {"CreatedBy",  routeMasterDetails.Route.CreatedBy},
                    {"ModifiedBy",  routeMasterDetails.Route.ModifiedBy},
                    {"ModifiedTime",  routeMasterDetails.Route.ModifiedTime},
                    {"ServerAddTime",  DateTime.Now},
                    {"ServerModifiedTime",  DateTime.Now},
                    {"PrintStanding",  routeMasterDetails.Route.PrintStanding},
                    {"PrintForward",  routeMasterDetails.Route.PrintForward },
                    {"PrintTopup",  routeMasterDetails.Route.PrintTopup},
                    {"PrintOrderSummary",  routeMasterDetails.Route.PrintOrderSummary},
                    {"AutoFreezeJP",  routeMasterDetails.Route.AutoFreezeJP},
                    {"AddToRun",  routeMasterDetails.Route.AddToRun},
                    {"AutoFreezeRunTime",  routeMasterDetails.Route.AutoFreezeRunTime},
                    {"IsChangeApplied",  true},
                    {"TotalCustomers" , routeMasterDetails.Route.TotalCustomers },
                    {"VisitTime" , routeMasterDetails.Route.VisitTime },
                    {"EndTime" , routeMasterDetails.Route.EndTime },
                    {"VisitDuration" , routeMasterDetails.Route.VisitDuration },
                    {"TravelTime" , routeMasterDetails.Route.TravelTime },
                    {"IsCustomerWithTime" , routeMasterDetails.Route.IsCustomerWithTime },
                };

                int changeLogInsertResult = await ExecuteNonQueryAsync(routeChangeLogQuery, connection, transaction, routeChangeLogParameters);
                if (changeLogInsertResult <= 0)
                {
                    transaction.Rollback();
                    throw new Exception("RouteChangeLog Insert failed");
                }
                count += changeLogInsertResult;


                string routeScheduleQuery = @"Insert Into route_schedule(uid,created_by,created_time,modified_by,modified_time,server_add_time
                          ,server_modified_time,company_uid,route_uid,name,type,start_date,status,from_date,to_date,start_time,end_time
                          ,visit_duration_in_minutes,travel_time_in_minutes,last_beat_date,next_beat_date,allow_multiple_beats_per_day,planned_days,ss)
                          values(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@CompanyUID,@RouteUID,@Name,@Type
                         ,@StartDate,@Status,@FromDate,@ToDate,@StartTime,@EndTime,@VisitDurationInMinutes,@TravelTimeInMinutes,@LastBeatDate
                         ,@NextBeatDate,@AllowMultipleBeatsPerDay,@PlannedDays,@SS )";
                Dictionary<string, object?> routeScheduleParameters = new()
                {
                    {"UID", routeMasterDetails.RouteSchedule.UID}, // Use RouteSchedule UID, not Route UID
                    {"NextBeatDate", routeMasterDetails.RouteSchedule.NextBeatDate},
                    {"LastBeatDate", routeMasterDetails.RouteSchedule.LastBeatDate},
                    {"AllowMultipleBeatsPerDay", routeMasterDetails.RouteSchedule.AllowMultipleBeatsPerDay},
                    {"PlannedDays", routeMasterDetails.RouteSchedule.PlannedDays},
                    {"CreatedBy", routeMasterDetails.RouteSchedule.CreatedBy},
                    {"CreatedTime", routeMasterDetails.RouteSchedule.CreatedTime},
                    {"ModifiedBy", routeMasterDetails.RouteSchedule.ModifiedBy},
                    {"ModifiedTime", routeMasterDetails.RouteSchedule.ModifiedTime},
                    {"ServerAddTime", DateTime.Now},
                    {"ServerModifiedTime", DateTime.Now},
                    {"SS", routeMasterDetails.RouteSchedule.SS},
                    {"CompanyUID", routeMasterDetails.RouteSchedule.CompanyUID},
                    {"RouteUID", routeMasterDetails.Route.UID},
                    {"Name", routeMasterDetails.RouteSchedule.Name},
                    {"Type", routeMasterDetails.RouteSchedule.Type},
                    {"StartDate", routeMasterDetails.RouteSchedule.StartDate},
                    {"Status", routeMasterDetails.RouteSchedule.Status},
                    {"FromDate", routeMasterDetails.RouteSchedule.FromDate},
                    {"ToDate", routeMasterDetails.RouteSchedule.ToDate},
                    {"StartTime", routeMasterDetails.RouteSchedule.StartTime},
                    {"EndTime", routeMasterDetails.RouteSchedule.EndTime},
                    {"VisitDurationInMinutes", routeMasterDetails.RouteSchedule.VisitDurationInMinutes},
                    {"TravelTimeInMinutes", routeMasterDetails.RouteSchedule.TravelTimeInMinutes},
                    };

                Console.WriteLine($"[DEBUG] Inserting RouteSchedule with UID: {routeMasterDetails.RouteSchedule.UID}");
                int scheduleInsertResult = await ExecuteNonQueryAsync(routeScheduleQuery, connection, transaction, routeScheduleParameters);
                if (scheduleInsertResult <= 0)
                {
                    transaction.Rollback();
                    throw new Exception("RouteSchedule Insert failed");
                }
                count += scheduleInsertResult;
                Console.WriteLine($"[DEBUG] Successfully inserted RouteSchedule with UID: {routeMasterDetails.RouteSchedule.UID}");
                // Insert route schedule configs
                if (routeMasterDetails.RouteScheduleConfigs != null && routeMasterDetails.RouteScheduleConfigs.Any())
                {
                    foreach (var scheduleConfig in routeMasterDetails.RouteScheduleConfigs)
                    {
                        string routeScheduleConfigQuery = @"INSERT INTO route_schedule_config(uid, created_by, created_time, modified_by, modified_time,
                                                            server_add_time, server_modified_time, schedule_type, week_number, day_number, is_deleted) 
                                                            VALUES(@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                                            @ServerAddTime, @ServerModifiedTime, @ScheduleType, @WeekNumber, @DayNumber, @IsDeleted)";
                        Dictionary<string, object?> routeScheduleConfigParameters = new()
                        {
                            {"UID", scheduleConfig.UID},
                            {"CreatedBy", scheduleConfig.CreatedBy},
                            {"CreatedTime", scheduleConfig.CreatedTime},
                            {"ModifiedBy", scheduleConfig.ModifiedBy},
                            {"ModifiedTime", scheduleConfig.ModifiedTime},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"ScheduleType", scheduleConfig.ScheduleType},
                            {"WeekNumber", scheduleConfig.WeekNumber},
                            {"DayNumber", scheduleConfig.DayNumber},
                            {"IsDeleted", scheduleConfig.IsDeleted}
                        };

                        int configInsertResult = await ExecuteNonQueryAsync(routeScheduleConfigQuery, connection, transaction, routeScheduleConfigParameters);
                        if (configInsertResult <= 0)
                        {
                            transaction.Rollback();
                            throw new Exception("RouteScheduleConfig Insert failed");
                        }
                        count += configInsertResult;
                    }
                }


                // IMPORTANT: Insert route customers FIRST (before mappings) since mappings reference route_customer.uid
                Console.WriteLine($"[DEBUG] Processing RouteCustomersList FIRST: {routeMasterDetails.RouteCustomersList?.Count ?? 0} customers");
                Dictionary<string, string> storeToRouteCustomerMap = new Dictionary<string, string>();
                
                foreach (RouteCustomer RouteCustomer in routeMasterDetails.RouteCustomersList)
                {
                    Console.WriteLine($"[DEBUG] Inserting RouteCustomer: StoreUID={RouteCustomer.StoreUID}, Frequency={RouteCustomer.Frequency ?? "NULL"}");
                    
                    string routeCustomerQuery = @"INSERT INTO route_customer (uid, route_uid, store_uid, seq_no, visit_time, visit_duration,
                            end_time,is_deleted,created_by, created_time,modified_by,   modified_time, server_add_time, server_modified_time,travel_time,frequency)  
                            VALUES (@UID, @RouteUID, @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime,@TravelTime,@Frequency);";
                    Dictionary<string, object?> routeCustomerParameters = new()
                    {
                                {"UID", RouteCustomer.UID},
                                {"RouteUID", routeMasterDetails.Route.UID},
                                {"StoreUID", RouteCustomer.StoreUID},
                                {"SeqNo", RouteCustomer.SeqNo},
                                {"VisitTime", RouteCustomer.VisitTime},
                                {"VisitDuration", RouteCustomer.VisitDuration},
                                {"EndTime", RouteCustomer.EndTime},
                                {"IsDeleted", RouteCustomer.IsDeleted},
                                {"CreatedTime", RouteCustomer.CreatedTime},
                                {"CreatedBy", RouteCustomer.CreatedBy},
                                {"ModifiedBy", RouteCustomer.ModifiedBy},
                                {"ModifiedTime", RouteCustomer.ModifiedTime},
                                {"ServerAddTime", DateTime.Now},
                                {"ServerModifiedTime", DateTime.Now},
                                {"TravelTime", RouteCustomer.TravelTime},
                                {"Frequency", RouteCustomer.Frequency},
                            };
                    int customerInsertResult = await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, routeCustomerParameters);
                    if (customerInsertResult <= 0)
                    {
                        Console.WriteLine($"[ERROR] RouteCustomer Insert failed for StoreUID={RouteCustomer.StoreUID}, result={customerInsertResult}");
                        transaction.Rollback();
                        throw new Exception($"RouteCustomer Table Insert Failed for StoreUID={RouteCustomer.StoreUID}");
                    }
                    count += customerInsertResult;
                    
                    // Map store UID to route_customer UID for use in mappings
                    storeToRouteCustomerMap[RouteCustomer.StoreUID] = RouteCustomer.UID;
                    Console.WriteLine($"[DEBUG] Successfully inserted RouteCustomer: StoreUID={RouteCustomer.StoreUID}, RouteCustomerUID={RouteCustomer.UID}, total count={count}");
                }

                // NOW insert route schedule customer mappings (using route_customer UIDs)
                Console.WriteLine($"[DEBUG] RouteScheduleCustomerMappings: {routeMasterDetails.RouteScheduleCustomerMappings?.Count ?? 0} mappings to process");
                if (routeMasterDetails.RouteScheduleCustomerMappings != null && routeMasterDetails.RouteScheduleCustomerMappings.Any())
                {
                    Console.WriteLine($"[DEBUG] Processing {routeMasterDetails.RouteScheduleCustomerMappings.Count} customer mappings with BATCH INSERT");

                    // Step 1: Prepare all data for batch insert
                    var validMappings = new List<Dictionary<string, object?>>();

                    foreach (var customerMapping in routeMasterDetails.RouteScheduleCustomerMappings)
                    {
                        // Get the route_customer UID from our map
                        if (!storeToRouteCustomerMap.TryGetValue(customerMapping.CustomerUID, out string routeCustomerUID))
                        {
                            Console.WriteLine($"[ERROR] No RouteCustomer found for StoreUID={customerMapping.CustomerUID}");
                            transaction.Rollback();
                            throw new Exception($"No RouteCustomer found for StoreUID={customerMapping.CustomerUID}");
                        }

                        Console.WriteLine($"[DEBUG] Preparing batch data: StoreUID={customerMapping.CustomerUID} -> RouteCustomerUID={routeCustomerUID}");

                        // Parse time values
                        TimeSpan? startTimeSpan = null;
                        TimeSpan? endTimeSpan = null;

                        if (!string.IsNullOrEmpty(customerMapping.StartTime))
                        {
                            if (TimeSpan.TryParse(customerMapping.StartTime, out TimeSpan parsedStart))
                            {
                                startTimeSpan = parsedStart;
                            }
                            else
                            {
                                Console.WriteLine($"[WARNING] Could not parse StartTime: {customerMapping.StartTime}, using default 09:00:00");
                                startTimeSpan = new TimeSpan(9, 0, 0);
                            }
                        }

                        if (!string.IsNullOrEmpty(customerMapping.EndTime))
                        {
                            if (TimeSpan.TryParse(customerMapping.EndTime, out TimeSpan parsedEnd))
                            {
                                endTimeSpan = parsedEnd;
                            }
                            else
                            {
                                Console.WriteLine($"[WARNING] Could not parse EndTime: {customerMapping.EndTime}, using default 10:00:00");
                                endTimeSpan = new TimeSpan(10, 0, 0);
                            }
                        }

                        // Add to batch data
                        validMappings.Add(new Dictionary<string, object?>
                        {
                            {"UID", customerMapping.UID},
                            {"CreatedBy", customerMapping.CreatedBy},
                            {"CreatedTime", customerMapping.CreatedTime},
                            {"ModifiedBy", customerMapping.ModifiedBy},
                            {"ModifiedTime", customerMapping.ModifiedTime},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"RouteScheduleUID", customerMapping.RouteScheduleUID},
                            {"RouteScheduleConfigUID", customerMapping.RouteScheduleConfigUID},
                            {"CustomerUID", routeCustomerUID}, // Use route_customer UID, not store UID
                            {"SeqNo", customerMapping.SeqNo},
                            {"StartTime", startTimeSpan},
                            {"EndTime", endTimeSpan},
                            {"IsDeleted", customerMapping.IsDeleted}
                        });
                    }

                    // Step 2: Execute batch insert if we have valid mappings
                    if (validMappings.Count > 0)
                    {
                        Console.WriteLine($"[DEBUG] Executing BATCH INSERT for {validMappings.Count} customer mappings");

                        var startTime = DateTime.Now;

                        // Create batch insert query
                        var valuesClauses = new List<string>();
                        var batchParameters = new Dictionary<string, object?>();

                        for (int i = 0; i < validMappings.Count; i++)
                        {
                            var mapping = validMappings[i];
                            var paramPrefix = $"p{i}";

                            valuesClauses.Add($"(@{paramPrefix}_UID, @{paramPrefix}_CreatedBy, @{paramPrefix}_CreatedTime, @{paramPrefix}_ModifiedBy, " +
                                             $"@{paramPrefix}_ModifiedTime, @{paramPrefix}_ServerAddTime, @{paramPrefix}_ServerModifiedTime, " +
                                             $"@{paramPrefix}_RouteScheduleUID, @{paramPrefix}_RouteScheduleConfigUID, @{paramPrefix}_CustomerUID, " +
                                             $"@{paramPrefix}_SeqNo, @{paramPrefix}_StartTime, @{paramPrefix}_EndTime, @{paramPrefix}_IsDeleted)");

                            // Add parameters for this mapping
                            foreach (var kvp in mapping)
                            {
                                batchParameters.Add($"{paramPrefix}_{kvp.Key}", kvp.Value);
                            }
                        }

                        string batchInsertQuery = $@"
                            INSERT INTO route_schedule_customer_mapping(
                                uid, created_by, created_time, modified_by, modified_time,
                                server_add_time, server_modified_time, route_schedule_uid,
                                route_schedule_config_uid, customer_uid, seq_no, start_time, end_time, is_deleted
                            ) VALUES {string.Join(", ", valuesClauses)}";

                        // Execute the batch insert
                        int batchInsertResult = await ExecuteNonQueryAsync(batchInsertQuery, connection, transaction, batchParameters);

                        var executionTime = DateTime.Now - startTime;
                        Console.WriteLine($"[DEBUG] BATCH INSERT completed in {executionTime.TotalMilliseconds}ms, affected rows: {batchInsertResult}");

                        if (batchInsertResult != validMappings.Count)
                        {
                            Console.WriteLine($"[ERROR] Batch insert failed. Expected: {validMappings.Count}, Actual: {batchInsertResult}");
                            transaction.Rollback();
                            throw new Exception($"Batch RouteScheduleCustomerMapping Insert failed. Expected {validMappings.Count} rows, inserted {batchInsertResult}");
                        }

                        count += batchInsertResult;
                        Console.WriteLine($"[DEBUG] Total records processed so far: {count}");
                    }
                }

                // RouteCustomers have already been inserted above (before mappings)
                foreach (RouteUser routeUser in routeMasterDetails.RouteUserList)
                {

                    string routeUSerQuery = @"INSERT INTO route_user (uid, created_by, created_time, modified_by, modified_time, 
                                                          server_add_time, server_modified_time, route_uid, job_position_uid, from_date, to_date,is_active) 
                                                          VALUES 
                                                          (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID, 
                                                          @JobPositionUID, @FromDate, @ToDate, @IsActive);";
                    Dictionary<string, object?> routeUserParameters = new()
                    {
                                { "@UID", routeUser.UID },
                                { "@CreatedBy", routeUser.CreatedBy },
                                { "@CreatedTime", routeUser.CreatedTime },
                                { "@ModifiedBy", routeUser.ModifiedBy },
                                { "@ModifiedTime", routeUser.ModifiedTime },
                                { "@ServerAddTime", DateTime.Now },
                                { "@ServerModifiedTime", DateTime.Now },
                                { "@RouteUID", routeMasterDetails.Route.UID },
                                { "@JobPositionUID", routeUser.JobPositionUID },
                                { "@FromDate", routeUser.FromDate },
                                { "@ToDate", routeUser.ToDate },
                                { "@IsActive", routeUser.IsActive },
                            };
                    int userInsertResult = await ExecuteNonQueryAsync(routeUSerQuery, connection, transaction, routeUserParameters);
                    if (userInsertResult <= 0)
                    {
                        transaction.Rollback();
                        throw new Exception("RouteUser Table Insert Failed");
                    }
                    count += userInsertResult;
                }

                transaction.Commit();
                int total = count;
                return total;
            }
            catch (Exception ex)
            {
                // Only rollback if transaction is still active
                if (transaction != null && transaction.Connection != null)
                {
                    try 
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                        // Transaction already rolled back, ignore
                    }
                }
                throw new Exception("Error during transaction", ex);
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
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
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

                Dictionary<string, object?> routeChangeParameters = new()
                {
                    {"UID", routeMasterDetails.Route.UID},
                    {"CompanyUID", routeMasterDetails.Route.CompanyUID},
                    {"Code", routeMasterDetails.Route.Code},
                    {"Name", routeMasterDetails.Route.Name},
                    {"RoleUID", routeMasterDetails.Route.RoleUID},
                    {"IsActive", routeMasterDetails.Route.IsActive},
                    {"Status", routeMasterDetails.Route.Status},
                    {"ValidFrom", routeMasterDetails.Route.ValidFrom},
                    {"ValidUpto", routeMasterDetails.Route.ValidUpto},
                    {"ModifiedBy", routeMasterDetails.Route.ModifiedBy},
                    {"ModifiedTime", routeMasterDetails.Route.ModifiedTime},
                    {"ServerModifiedTime", DateTime.Now},
                     {"PrintStanding",  routeMasterDetails.Route.PrintStanding},
                     {"PrintForward",  routeMasterDetails.Route.PrintForward},
                    {"PrintTopup",  routeMasterDetails.Route.PrintTopup},
                    {"PrintOrderSummary",  routeMasterDetails.Route.PrintOrderSummary},
                    {"AutoFreezeJP",  routeMasterDetails.Route.AutoFreezeJP},
                    {"AddToRun",  routeMasterDetails.Route.AddToRun},
                    {"AutoFreezeRunTime",  routeMasterDetails.Route.AutoFreezeRunTime},
                    {"TotalCustomers" , routeMasterDetails.Route.TotalCustomers },
                    {"JobPositionUID" , routeMasterDetails.Route.JobPositionUID },
                    {"VisitTime" , routeMasterDetails.Route.VisitTime },
                    {"EndTime" , routeMasterDetails.Route.EndTime },
                    {"VisitDuration" , routeMasterDetails.Route.VisitDuration },
                    {"TravelTime" , routeMasterDetails.Route.TravelTime },
                    {"IsCustomerWithTime" , routeMasterDetails.Route.IsCustomerWithTime },
                    };

                count = await ExecuteNonQueryAsync(routeChangeLogQuery, connection, transaction, routeChangeParameters);

                if (count < 0)
                {
                    transaction.Rollback();
                    throw new Exception("RouteChangeLog Update failed");
                }
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

                Dictionary<string, object?> routeParameters = new()
                {
                            {"UID", routeMasterDetails.Route.UID},
                            {"ModifiedBy", routeMasterDetails.Route.ModifiedBy},
                            {"ModifiedTime", routeMasterDetails.Route.ModifiedTime},
                            {"ServerModifiedTime", DateTime.Now},
                            {"VisitTime" , routeMasterDetails.Route.VisitTime },
                            {"EndTime" , routeMasterDetails.Route.EndTime },
                            {"VisitDuration" , routeMasterDetails.Route.VisitDuration },
                            {"TravelTime" , routeMasterDetails.Route.TravelTime },
                            {"IsCustomerWithTime" , routeMasterDetails.Route.IsCustomerWithTime },
                         };

                count = await ExecuteNonQueryAsync(routeQuery, connection, transaction, routeParameters);

                if (count < 0)
                {
                    transaction.Rollback();
                    throw new Exception("Route Update failed");
                }

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

                Dictionary<string, object?> routeScheduleParameters = new()
                {
              {"UID", routeMasterDetails.RouteSchedule.UID},
              {"NextBeatDate", routeMasterDetails.RouteSchedule.NextBeatDate},
              {"LastBeatDate", routeMasterDetails.RouteSchedule.LastBeatDate},
              {"AllowMultipleBeatsPerDay", routeMasterDetails.RouteSchedule.AllowMultipleBeatsPerDay},
              {"PlannedDays", routeMasterDetails.RouteSchedule.PlannedDays},
              {"ModifiedBy", routeMasterDetails.RouteSchedule.ModifiedBy},
              {"ModifiedTime", routeMasterDetails.RouteSchedule.ModifiedTime},
              {"ServerModifiedTime", DateTime.Now},
              {"SS", routeMasterDetails.RouteSchedule.SS},
              {"RouteUID", routeMasterDetails.RouteSchedule.RouteUID},
              {"Name", routeMasterDetails.RouteSchedule.Name},
              {"Type", routeMasterDetails.RouteSchedule.Type},
              {"StartDate", routeMasterDetails.RouteSchedule.StartDate},
              {"Status", routeMasterDetails.RouteSchedule.Status},
              {"FromDate", routeMasterDetails.RouteSchedule.FromDate},
              {"ToDate", routeMasterDetails.RouteSchedule.ToDate},
              {"StartTime", routeMasterDetails.RouteSchedule.StartTime},
              {"EndTime", routeMasterDetails.RouteSchedule.EndTime},
              {"VisitDurationInMinutes", routeMasterDetails.RouteSchedule.VisitDurationInMinutes},
              {"TravelTimeInMinutes", routeMasterDetails.RouteSchedule.TravelTimeInMinutes},
              };
                count += await ExecuteNonQueryAsync(routeScheduleQuery, connection, transaction, routeScheduleParameters);

                if (count < 0)
                {
                    transaction.Rollback();
                    throw new Exception("RouteSchedule Update failed");
                }
                // Update route schedule configs
                if (routeMasterDetails.RouteScheduleConfigs != null && routeMasterDetails.RouteScheduleConfigs.Any())
                {
                    foreach (var scheduleConfig in routeMasterDetails.RouteScheduleConfigs)
                    {
                        string updateConfigQuery = @"UPDATE route_schedule_config SET
                                                    modified_by = @ModifiedBy,
                                                    modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime,
                                                    schedule_type = @ScheduleType,
                                                    week_number = @WeekNumber,
                                                    day_number = @DayNumber,
                                                    is_deleted = @IsDeleted
                                                    WHERE uid = @UID";
                        
                        Dictionary<string, object?> configParameters = new()
                        {
                            {"UID", scheduleConfig.UID},
                            {"ModifiedBy", scheduleConfig.ModifiedBy},
                            {"ModifiedTime", scheduleConfig.ModifiedTime},
                            {"ServerModifiedTime", DateTime.Now},
                            {"ScheduleType", scheduleConfig.ScheduleType},
                            {"WeekNumber", scheduleConfig.WeekNumber},
                            {"DayNumber", scheduleConfig.DayNumber},
                            {"IsDeleted", scheduleConfig.IsDeleted}
                        };

                        count += await ExecuteNonQueryAsync(updateConfigQuery, connection, transaction, configParameters);
                        if (count < 0)
                        {
                            transaction.Rollback();
                            throw new Exception("RouteScheduleConfig Update failed");
                        }
                    }
                }

                // Update route schedule customer mappings
                if (routeMasterDetails.RouteScheduleCustomerMappings != null && routeMasterDetails.RouteScheduleCustomerMappings.Any())
                {
                    foreach (var customerMapping in routeMasterDetails.RouteScheduleCustomerMappings)
                    {
                        string updateMappingQuery = @"UPDATE route_schedule_customer_mapping SET
                                                     modified_by = @ModifiedBy,
                                                     modified_time = @ModifiedTime,
                                                     server_modified_time = @ServerModifiedTime,
                                                     route_schedule_uid = @RouteScheduleUID,
                                                     route_schedule_config_uid = @RouteScheduleConfigUID,
                                                     customer_uid = @CustomerUID,
                                                     seq_no = @SeqNo,
                                                     start_time = @StartTime,
                                                     end_time = @EndTime,
                                                     is_deleted = @IsDeleted
                                                     WHERE uid = @UID";
                        
                        // Convert string time to TimeSpan for PostgreSQL time type
                        TimeSpan? startTimeSpan = null;
                        TimeSpan? endTimeSpan = null;
                        
                        if (!string.IsNullOrEmpty(customerMapping.StartTime))
                        {
                            if (TimeSpan.TryParse(customerMapping.StartTime, out TimeSpan parsedStart))
                            {
                                startTimeSpan = parsedStart;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(customerMapping.EndTime))
                        {
                            if (TimeSpan.TryParse(customerMapping.EndTime, out TimeSpan parsedEnd))
                            {
                                endTimeSpan = parsedEnd;
                            }
                        }
                        
                        Dictionary<string, object?> mappingParameters = new()
                        {
                            {"UID", customerMapping.UID},
                            {"ModifiedBy", customerMapping.ModifiedBy},
                            {"ModifiedTime", customerMapping.ModifiedTime},
                            {"ServerModifiedTime", DateTime.Now},
                            {"RouteScheduleUID", customerMapping.RouteScheduleUID},
                            {"RouteScheduleConfigUID", customerMapping.RouteScheduleConfigUID},
                            {"CustomerUID", customerMapping.CustomerUID},
                            {"SeqNo", customerMapping.SeqNo},
                            {"StartTime", startTimeSpan},
                            {"EndTime", endTimeSpan},
                            {"IsDeleted", customerMapping.IsDeleted}
                        };

                        count += await ExecuteNonQueryAsync(updateMappingQuery, connection, transaction, mappingParameters);
                        if (count < 0)
                        {
                            transaction.Rollback();
                            throw new Exception("RouteScheduleCustomerMapping Update failed");
                        }
                    }
                }
                if (routeMasterDetails.RouteCustomersList != null && routeMasterDetails.RouteCustomersList.Count > 0)
                {
                    foreach (RouteCustomer routeCustomer in routeMasterDetails.RouteCustomersList)
                    {
                        switch (routeCustomer.ActionType)
                        {
                            case Winit.Shared.Models.Enums.ActionType.Add:
                                count += await InsertRouteCustomer(connection, transaction, routeCustomer);
                                break;

                            case Winit.Shared.Models.Enums.ActionType.Update:
                                count += await UpdateRouteCustomer(connection, transaction, routeCustomer);
                                break;
                        }
                    }
                }

                if (routeMasterDetails.RouteUserList != null && routeMasterDetails.RouteUserList.Count > 0)
                {
                    List<string> uidList = routeMasterDetails.RouteUserList.Select(po => po.UID).ToList();
                    List<string> deletedUidList = routeMasterDetails.RouteUserList.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

                    IEnumerable<IRouteUser> existingRec = await SelectRouteUserByUID(uidList);

                    foreach (RouteUser routeUser in routeMasterDetails.RouteUserList)
                    {
                        switch (routeUser.ActionType)
                        {
                            case Winit.Shared.Models.Enums.ActionType.Add:
                                bool exists = existingRec.Any(po => po.UID == routeUser.UID);
                                count += exists ?
                                 await UpdateRouteUser(routeUser) :
                                 await CreateRouteUser(routeUser);
                                break;

                            case Winit.Shared.Models.Enums.ActionType.Delete:
                                count += await DeleteRouteUser(deletedUidList);
                                break;
                        }
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
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<int> InsertRouteCustomer(NpgsqlConnection connection, NpgsqlTransaction transaction, RouteCustomer routeCustomer)
    {
        string routeCustomerQuery = @"INSERT INTO route_customer (uid, route_uid, store_uid, seq_no, visit_time, visit_duration,
                            end_time,is_deleted,created_by, created_time,modified_by,   modified_time, server_add_time, server_modified_time)  
                            VALUES (@UID, @RouteUID, @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime);";
        Dictionary<string, object?> returnOrderLineParameters = new()
        {
                        {"UID", routeCustomer.UID},
                        {"RouteUID", routeCustomer.RouteUID},
                        {"StoreUID", routeCustomer.StoreUID},
                        {"SeqNo", routeCustomer.SeqNo},
                        {"VisitTime", routeCustomer.VisitTime},
                        {"VisitDuration", routeCustomer.VisitDuration},
                        {"EndTime", routeCustomer.EndTime},
                        {"IsDeleted", routeCustomer.IsDeleted},
                        {"CreatedTime", routeCustomer.CreatedTime},
                        {"CreatedBy", routeCustomer.CreatedBy},
                        {"ModifiedBy", routeCustomer.ModifiedBy},
                        {"ModifiedTime", routeCustomer.ModifiedTime},
                        {"ServerAddTime", DateTime.Now},
                        {"ServerModifiedTime", DateTime.Now},
                        {"TravelTime", routeCustomer.TravelTime},
                    };

        return await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, returnOrderLineParameters);
    }

    private async Task<int> UpdateRouteCustomer(NpgsqlConnection connection, NpgsqlTransaction transaction, RouteCustomer routeCustomer)
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

        Dictionary<string, object?> returnOrderLineParameters = new()
        {
            {"UID", routeCustomer.UID},
            {"SeqNo", routeCustomer.SeqNo},
            {"VisitTime", routeCustomer.VisitTime},
            {"VisitDuration", routeCustomer.VisitDuration},
            {"EndTime", routeCustomer.EndTime},
            {"IsDeleted", routeCustomer.IsDeleted},
            {"ModifiedBy", routeCustomer.ModifiedBy},
            {"ModifiedTime", routeCustomer.ModifiedTime},
            {"ServerModifiedTime", DateTime.Now},
            {"TravelTime", routeCustomer.TravelTime},
        };

        return await ExecuteNonQueryAsync(routeCustomerQuery, connection, transaction, returnOrderLineParameters);
    }

    private int GetDayNumber(string dayName)
    {
        switch (dayName?.ToLower())
        {
            case "monday": return 1;
            case "tuesday": return 2;
            case "wednesday": return 3;
            case "thursday": return 4;
            case "friday": return 5;
            case "saturday": return 6;
            case "sunday": return 7;
            default: return 0;
        }
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
            
            // Get the route_schedule UID from the route UID
            string routeScheduleUID = routeSheduleList?.FirstOrDefault()?.UID ?? "";
            Dictionary<string, object?> ScheduleParameters = new()
            {
                { "RouteScheduleUID", routeScheduleUID },
            };
            
            // Get configs that are referenced in the route_schedule_customer_mapping
            StringBuilder routeScheduleConfigSQL = new(@"SELECT DISTINCT
                                                            rsc.id AS Id, 
                                                            rsc.uid AS UID, 
                                                            NULL AS CreatedBy, 
                                                            NULL AS CreatedTime,
                                                            NULL AS ModifiedBy, 
                                                            NULL AS ModifiedTime,
                                                            NULL AS ServerAddTime, 
                                                            NULL AS ServerModifiedTime,
                                                            rsc.schedule_type AS ScheduleType, 
                                                            COALESCE(rsc.week_number, '') AS WeekNumber, 
                                                            COALESCE(rsc.day_number, 0) AS DayNumber,
                                                            COALESCE(rsc.is_deleted, false) AS IsDeleted
                                                            FROM route_schedule_config rsc
                                                            WHERE rsc.uid IN (
                                                                SELECT DISTINCT route_schedule_config_uid 
                                                                FROM route_schedule_customer_mapping 
                                                                WHERE route_schedule_uid = @RouteScheduleUID
                                                            );");
            Type routeScheduleConfigType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleConfig>().GetType();
            List<Model.Interfaces.IRouteScheduleConfig> routeScheduleConfigList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleConfig>(routeScheduleConfigSQL.ToString(), ScheduleParameters, routeScheduleConfigType);

            StringBuilder routeScheduleCustomerMappingSQL = new(@"SELECT
                                                            rscm.id AS Id, rscm.uid AS UID, rscm.created_by AS CreatedBy, rscm.created_time AS CreatedTime,
                                                            rscm.modified_by AS ModifiedBy, rscm.modified_time AS ModifiedTime,
                                                            rscm.server_add_time AS ServerAddTime, rscm.server_modified_time AS ServerModifiedTime,
                                                            rscm.route_schedule_uid AS RouteScheduleUID, rscm.route_schedule_config_uid AS RouteScheduleConfigUID,
                                                            rscm.customer_uid AS CustomerUID, rscm.seq_no AS SeqNo, 
                                                            COALESCE(rscm.start_time::text, '00:00:00') AS StartTime,
                                                            COALESCE(rscm.end_time::text, '00:00:00') AS EndTime, 
                                                            COALESCE(rscm.is_deleted, false) AS IsDeleted,
                                                            s.name AS CustomerName, s.code AS CustomerCode, s.uid AS StoreUID
                                                            FROM route_schedule_customer_mapping rscm
                                                            LEFT JOIN route_customer rc ON rscm.customer_uid = rc.uid
                                                            LEFT JOIN store s ON rc.store_uid = s.uid
                                                            WHERE rscm.route_schedule_uid = @RouteScheduleUID;");
            Type routeScheduleCustomerMappingType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleCustomerMapping>().GetType();
            List<Model.Interfaces.IRouteScheduleCustomerMapping> routeScheduleCustomerMappingList = await ExecuteQueryAsync<Model.Interfaces.IRouteScheduleCustomerMapping>(routeScheduleCustomerMappingSQL.ToString(), ScheduleParameters, routeScheduleCustomerMappingType);

            // If no configs found but we have mappings, create synthetic configs from the mapping UIDs
            if ((routeScheduleConfigList == null || routeScheduleConfigList.Count == 0) && 
                routeScheduleCustomerMappingList != null && routeScheduleCustomerMappingList.Count > 0)
            {
                // Get distinct route_schedule_config_uids and create configs from them
                StringBuilder syntheticConfigSQL = new(@"SELECT DISTINCT 
                                                        route_schedule_config_uid AS UID,
                                                        route_schedule_config_uid AS RouteScheduleConfigUID
                                                        FROM route_schedule_customer_mapping
                                                        WHERE route_schedule_uid = @RouteScheduleUID
                                                        AND route_schedule_config_uid IS NOT NULL;");
                
                var distinctConfigs = await ExecuteQueryAsync<dynamic>(syntheticConfigSQL.ToString(), ScheduleParameters);
                
                if (distinctConfigs != null && distinctConfigs.Count > 0)
                {
                    routeScheduleConfigList = new List<Model.Interfaces.IRouteScheduleConfig>();
                    foreach (var configUID in distinctConfigs)
                    {
                        var config = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteScheduleConfig>();
                        config.UID = configUID.UID;
                        
                        // Parse the UID to determine schedule type
                        string uid = configUID.UID?.ToString() ?? "";
                        var parts = uid.Split('_');
                        
                        if (parts.Length > 0)
                        {
                            config.ScheduleType = parts[0]; // daily, weekly, monthly, fortnight
                            
                            if (parts[0] == "weekly" && parts.Length > 2)
                            {
                                // weekly_W1_Monday
                                if (parts[1].StartsWith("W"))
                                {
                                    config.WeekNumber = parts[1].Substring(1); // WeekNumber is string
                                }
                                config.DayNumber = GetDayNumber(parts[2]);
                            }
                            else if (parts[0] == "monthly" && parts.Length > 1)
                            {
                                // monthly_15 - store date in WeekNumber field as workaround
                                config.WeekNumber = parts[1];
                                config.DayNumber = 0;
                            }
                            else if (parts[0] == "fortnight" && parts.Length > 2)
                            {
                                // fortnight_F1_Monday - store fortnight type in WeekNumber
                                config.WeekNumber = parts[1];
                                config.DayNumber = GetDayNumber(parts[2]);
                            }
                        }
                        
                        config.IsDeleted = false; // Use IsDeleted instead of IsActive
                        routeScheduleConfigList.Add(config);
                    }
                }
            }

            StringBuilder routeCustomerSQL = new(@"SELECT
                                                        rc.id AS Id, rc.uid AS UID, rc.created_by AS CreatedBy, rc.created_time AS CreatedTime,
                                                        rc.modified_by AS ModifiedBy, rc.modified_time AS ModifiedTime,
                                                        rc.server_add_time AS ServerAddTime, rc.server_modified_time AS ServerModifiedTime,
                                                        rc.route_uid AS RouteUID, rc.store_uid AS StoreUID, rc.seq_no AS SeqNo,
                                                        COALESCE(rc.visit_time::text, '00:00:00') AS VisitTime, 
                                                        rc.visit_duration AS VisitDuration, 
                                                        COALESCE(rc.end_time::text, '00:00:00') AS EndTime,
                                                        rc.is_deleted AS IsDeleted, rc.travel_time AS TravelTime, rc.frequency AS Frequency,
                                                        s.name AS StoreName, s.code AS StoreCode, s.alias_name AS StoreAliasName,
                                                        s.city_uid AS StoreCity, s.region_uid AS StoreRegion
                                                        FROM route_customer rc
                                                        LEFT JOIN store s ON rc.store_uid = s.uid
                                                        WHERE rc.route_uid = @UID;");
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
            return (routeList, routeSheduleList, routeScheduleConfigList, routeScheduleCustomerMappingList, routeCustomerList, routeUserList);
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
            string sql = @"SELECT v.uid AS UID, '[' || v.vehicle_no || '] ' || v.registration_no AS Label
                       FROM vehicle AS v WHERE v.org_uid = @orgUID AND COALESCE(v.is_active, false) = true;";
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
        string sql = @"SELECT O.uid AS UID, '[' || O.code || '] ' || O.name AS Label FROM org AS O
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
            string sql = @"SELECT DISTINCT jp.uid as UID,  '[' || e.code || ']' ||
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
    private async Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectRouteUserByUID(List<string> UIDs)
    {
        string commaSeparatedUIDs = string.Join(",", UIDs);
        Dictionary<string, object?> parameters = new()
        {
            {"UIDs" , commaSeparatedUIDs}
        };
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
                                FROM route_user WHERE uid= ANY(string_to_array(@UIDs, ','))";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteUser>().GetType();
        IEnumerable<Model.Interfaces.IRouteUser> RouteUserList = await ExecuteQueryAsync<Model.Interfaces.IRouteUser>(sql, parameters, type);
        return RouteUserList;
    }
    private async Task<int> CreateRouteUser(Winit.Modules.Route.Model.Classes.RouteUser routeUser)
    {
        int retVal;
        try
        {
            string sql = @"INSERT INTO route_user (uid,created_by,created_time,modified_by,modified_time,
                        server_add_time,server_modified_time,route_uid, job_position_uid,from_date,to_date,is_active)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                         @RouteUID, @JobPositionUID, @FromDate, @ToDate, @IsActive);";
            Dictionary<string, object?> parameters = new()
            {
            {"UID", routeUser.UID},
            {"CreatedBy", routeUser.CreatedBy},
            {"CreatedTime", routeUser.CreatedTime},
            {"ModifiedBy", routeUser.ModifiedBy},
            {"ModifiedTime", routeUser.ModifiedTime},
            {"ServerAddTime", routeUser.ServerAddTime},
            {"ServerModifiedTime", routeUser.ServerModifiedTime},
            {"RouteUID", routeUser.RouteUID},
            {"JobPositionUID", routeUser.JobPositionUID},
            {"FromDate", routeUser.FromDate},
            {"ToDate", routeUser.ToDate},
            {"IsActive", routeUser.IsActive},
        };
            retVal = await ExecuteNonQueryAsync(sql, parameters);

        }
        catch (Exception)
        {
            throw;
        }

        return retVal;
    }

    private async Task<int> UpdateRouteUser(Winit.Modules.Route.Model.Classes.RouteUser routeUser)
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
            Dictionary<string, object?> parameters = new()
            {
               {"UID",routeUser.UID},
               {"ModifiedBy",routeUser.ModifiedBy},
               {"ModifiedTime",routeUser.ModifiedTime},
               {"ServerModifiedTime",routeUser.ServerModifiedTime},
               {"RouteUID",routeUser.RouteUID},
               {"JobPositionUID",routeUser.JobPositionUID},
               {"FromDate",routeUser.FromDate},
               {"ToDate",routeUser.ToDate},
               {"IsActive",routeUser.IsActive},
            };
            retVal = await ExecuteNonQueryAsync(sql, parameters);

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
        string sql = @"DELETE  FROM route_user WHERE uid = ANY(string_to_array(@UIDs, ','))";
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
                        NULL AS CreatedBy, 
                        NULL AS CreatedTime,
                        NULL AS ModifiedBy, 
                        NULL AS ModifiedTime, 
                        NULL AS ServerAddTime, 
                        NULL AS ServerModifiedTime, 
                        schedule_type AS ScheduleType, 
                        week_number AS WeekNumber, 
                        day_number AS DayNumber, 
                        COALESCE(is_deleted, false) AS IsDeleted
                       FROM route_schedule_config 
                       ORDER BY schedule_type, week_number, day_number";
        
        Type type = _serviceProvider.GetRequiredService<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>().GetType();
        List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig> configs = await ExecuteQueryAsync<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>(sql, null, type);

        return configs;
    }

}
