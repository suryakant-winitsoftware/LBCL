using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.DL.Classes
{
    public class SQLiteRouteDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IRouteDL
    {
        public SQLiteRouteDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute>> SelectRouteAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, code, name, role_uid, org_uid, 
                        wh_org_uid, vehicle_uid, job_position_uid, location_uid, is_active, status, valid_from, valid_upto, print_standing, print_topup, print_order_summary, auto_freeze_jp,
                        add_to_run, auto_freeze_run_time, ss, total_customers, print_forward, visit_time, end_time, visit_duration, travel_time, is_customer_with_time
	                        FROM route WHERE org_uid=NULL OR org_uid=@OrgUID");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM route  WHERE org_uid=NULL OR org_uid=@OrgUID");
                }
                var parameters = new Dictionary<string, object>()
                {
                    {"OrgUID",OrgUID }
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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

                //if (pageNumber > 0 && pageSize > 0)
                //{
                //   sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}

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

                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute> pagedResponse = new PagedResponse<Winit.Modules.Route.Model.Interfaces.IRoute>
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, code, name, role_uid, org_uid, wh_org_uid, 
                vehicle_uid, job_position_uid, location_uid, is_active, status, valid_from, valid_upto, print_standing, print_topup, print_order_summary, auto_freeze_jp, add_to_run,
                auto_freeze_run_time, ss, total_customers, print_forward, visit_time, end_time, visit_duration, travel_time, is_customer_with_time
	                FROM public.route WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRoute>().GetType();
            Model.Interfaces.IRoute RouteList = await ExecuteSingleAsync<Model.Interfaces.IRoute>(sql, parameters, type);
            return RouteList;
        }

        public async Task<int> CreateRouteDetails(Winit.Modules.Route.Model.Interfaces.IRoute routeDetails)
        {
            var sql = @"INSERT INTO route (uid, company, code, name, role_uid, org_uid, wh_org_uid, vehicle_uid, job_position_uid, location_uid, is_active, status, valid_from, 
                                valid_upto, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, print_standing, print_forward, print_topup, 
                                print_order_summary, auto_freeze, add_to_run, auto_freeze_run_time, total_customers) 
                                       VALUES (@UID, @CompanyUID, @Code, @Name, @RoleUID, @OrgUID, @WHOrgUID, 
                                       @VehicleUID, @JobPositionUID, @LocationUID,@IsActive,@Status,@ValidFrom,@ValidUpto,@CreatedBy,@CreatedTime,
                                       @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@PrintStanding,@PrintForward,@PrintTopup,@PrintOrderSummary,
                                       @AutoFreezeJP,@AddToRun,@AutoFreezeRunTime,@TotalCustomers)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
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
                var sql = @"UPDATE route SET code AS Code = @Code,name AS  Name = @Name,
                          role_uid AS RoleUID = @RoleUID,is_active AS IsActive = @IsActive,status AS Status = @Status,valid_from AS ValidFrom = @ValidFrom , 
                          ValidUpto = @ValidUpto, ModifiedBy = @ModifiedBy, ModifiedTime = @ModifiedTime,
                          server_modified_time AS ServerModifiedTime = @ServerModifiedTime,print_standing AS PrintStanding=@PrintStanding,
                          print_forward AS PrintForward=@PrintForward,print_topup AS PrintTopup=@PrintTopup,print_order_summary AS PrintOrderSummary=@PrintOrderSummary,
                          auto_freeze_jp AS AutoFreezeJP=@AutoFreezeJP, add_to_run AS AddToRun=@AddToRun,auto_freeze_run_time AS AutoFreezeRunTime=@AutoFreezeRunTime,
                          total_customers AS TotalCustomers=@TotalCustomers  WHERE uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
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
                return  await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteRouteDetail(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, code, name, role_uid, org_uid, wh_org_uid, 
                vehicle_uid, job_position_uid, location_uid, is_active, status, valid_from, valid_upto, print_standing, print_topup, print_order_summary, auto_freeze_jp, add_to_run,
                auto_freeze_run_time, ss, total_customers, print_forward, visit_time, end_time, visit_duration, travel_time, is_customer_with_time
	                FROM public.route WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreateRouteMaster(RouteMaster routeMasterDetails)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateRouteMaster(RouteMaster routeMasterDetails)
        {
            throw new NotImplementedException();
        }

        public Task<(List<IRouteChangeLog>, List<IRouteSchedule>, List<Model.Interfaces.IRouteScheduleConfig>, 
            List<Model.Interfaces.IRouteScheduleCustomerMapping>, List<IRouteCustomer>, List<IRouteUser>)> SelectRouteMasterViewByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISelectionItem>> GetDDL(string companyUID, string orgUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISelectionItem>> GetVehicleDDL(string orgUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISelectionItem>> GetWareHouseDDL(string OrgTypeUID, string ParentUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISelectionItem>> GetUserDDL(string OrgUID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<IRouteChangeLog>> SelectRouteChangeLogAllDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }
        
        public Task<List<Model.Interfaces.IRoute>> GetRoutesByStoreUID(string orgUID, string storeUID )
        {
            throw new NotImplementedException();
        }

        public Task<List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>> GetAllRouteScheduleConfigs()
        {
            throw new NotImplementedException();
        }
    }
}
