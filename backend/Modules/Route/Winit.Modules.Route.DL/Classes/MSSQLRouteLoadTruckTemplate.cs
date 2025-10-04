using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.DL.Classes
{
    public class MSSQLRouteLoadTruckTemplate : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IRouteLoadTruckTemplateDL
    {
        public MSSQLRouteLoadTruckTemplate(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>> SelectRouteLoadTruckTemplateAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from (SELECT id AS Id,uid AS UID,ss AS SS,created_time AS CreatedTime,
                                              modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                              route_uid AS RouteUID,template_name AS TemplateName,template_description AS TemplateDescription,
                                              company_uid AS CompanyUID,org_uid AS OrgUID FROM route_load_truck_template) as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,uid AS UID,ss AS SS,created_time AS CreatedTime,
                                                   modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                                                   route_uid AS RouteUID,template_name AS TemplateName,template_description AS TemplateDescription,
                                                   company_uid AS CompanyUID,org_uid AS OrgUID FROM route_load_truck_template) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>(filterCriterias, sbFilterCriteria, parameters);

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
                IEnumerable<Model.Interfaces.IRouteLoadTruckTemplate> routes = await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplate>(sql.ToString(), parameters);
                int totalCount = 0;

                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate> pagedResponse = new PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>
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


        public async Task<(List<Model.Interfaces.IRouteLoadTruckTemplate>, List<Model.Interfaces.IRouteLoadTruckTemplateLine>)>
          SelectRouteLoadTruckTemplateAndLineByUID(string RouteLoadTruckTemplateUID)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object>
                {
                    { "UID", RouteLoadTruckTemplateUID },
                };
                var routeLoadTruckTemplateSql = new StringBuilder(@"select id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime
                ,route_uid AS RouteUID,template_name AS TemplateName,template_description AS TemplateDescription,company_uid AS CompanyUID,
                org_uid AS OrgUID from route_load_truck_template WHERE uid=@UID ");
                List<Model.Interfaces.IRouteLoadTruckTemplate> routeLoadTruckTemplateList =
                        await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplate>(routeLoadTruckTemplateSql.ToString(), Parameters);
                var routeLoadTruckTemplateLineSQL = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                org_uid AS OrgUID,company_uid AS CompanyUID,route_load_truck_template_uid AS RouteLoadTruckTemplateUID,sku_code AS SKUCode,uom AS UOM,
                monday_qty AS MondayQty,monday_suggested_qty AS MondaySuggestedQty,tuesday_qty AS TuesdayQty,tuesday_suggested_qty AS TuesdaySuggestedQty,
                wednesday_qty AS WednesdayQty,wednesday_suggested_qty AS WednesdaySuggestedQty,thursday_qty AS ThursdayQty,
                thursday_suggested_qty AS ThursdaySuggestedQty,friday_qty AS FridayQty,friday_suggested_qty AS FridaySuggestedQty,
                saturday_qty AS SaturdayQty,saturday_suggested_qty AS SaturdaySuggestedQty,sunday_qty AS SundayQty,sunday_suggested_qty AS SundaySuggestedQty,
                line_number AS LineNumber FROM route_load_truck_template_line WHERE route_load_truck_template_uid=@UID 
                ORDER BY line_number DESC");
                List<Model.Interfaces.IRouteLoadTruckTemplateLine> routeLoadTruckTemplateLineList = await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplateLine>(routeLoadTruckTemplateLineSQL.ToString(), Parameters);
                return (routeLoadTruckTemplateList, routeLoadTruckTemplateLineList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
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
                            var routeLoadTruckTemplateQuery = @"INSERT INTO route_load_truck_template (uid, created_by, created_time, modified_by,modified_time, server_add_time, 
                                                                server_modified_time, route_uid, template_name, template_description, company_uid, org_uid)
                                                                VALUES 
                                                                (@UID, @CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID, 
                                                                @TemplateName, @TemplateDescription, @CompanyUID, @OrgUID);";
                            count += await ExecuteNonQueryAsync(routeLoadTruckTemplateQuery, connection, transaction, routeLoadTruckTemplateViewDTO);
                            if (routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null && routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Any())
                            {
                                count += await CreateRouteLoadTruckTemplateLineList(routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList, connection, transaction);
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
        private async Task<int> CreateRouteLoadTruckTemplateLineList(List<RouteLoadTruckTemplateLine> routeLoadTruckTemplateLineList, IDbConnection connection, IDbTransaction transaction)
        {
            int retVal = -1;
            try
            {
                var routeLoadTruckTemplateLineQuery = @"INSERT INTO route_load_truck_template_line (uid, created_by, created_time, modified_by, modified_time, 
                                                    server_add_time, server_modified_time, org_uid, company_uid, route_load_truck_template_uid, sku_code, uom, 
                                                    monday_qty, monday_suggested_qty, tuesday_qty, tuesday_suggested_qty, wednesday_qty, wednesday_suggested_qty, 
                                                    thursday_qty, thursday_suggested_qty, friday_qty, friday_suggested_qty, saturday_qty, saturday_suggested_qty, 
                                                    sunday_qty, sunday_suggested_qty,line_number)
                                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @OrgUID, @CompanyUID, 
                                                    @RouteLoadTruckTemplateUID, @SKUCode, @UOM, @MondayQty, @MondaySuggestedQty, @TuesdayQty, @TuesdaySuggestedQty, @WednesdayQty, 
                                                    @WednesdaySuggestedQty, @ThursdayQty, @ThursdaySuggestedQty, @FridayQty, @FridaySuggestedQty, @SaturdayQty, @SaturdaySuggestedQty,
                                                    @SundayQty, @SundaySuggestedQty,@LineNumber);";
                retVal += await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLineList);
            }
            catch
            {
                throw;
            }
            return retVal;
        }

        public async Task<int> UpdateRouteLoadTruckTemplateAndLine(Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateViewDTO routeLoadTruckTemplateViewDTO)
        {
            int count = 0;
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var routeLoadTruckTemplateQuery = @"UPDATE route_load_truck_template
                                                                SET
                                                                   modified_by = @ModifiedBy,
                                                                   modified_time = @ModifiedTime,
                                                                   server_modified_time = @ServerModifiedTime,
                                                                   template_name = @TemplateName,
                                                                   template_description = @TemplateDescription
                                                                WHERE uid = @UID;";
                            count += await ExecuteNonQueryAsync(routeLoadTruckTemplateQuery, connection, transaction, routeLoadTruckTemplateViewDTO);
                            if (routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null && routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Any())
                            {
                                List<RouteLoadTruckTemplateLine>? newrouteLoadTruckTemplateLine = null;
                                List<RouteLoadTruckTemplateLine>? existingrouteLoadTruckTemplateLine = null;
                                List<string> uidsFromList = routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Select(e => e.UID).ToList();
                                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.RouteLoadTruckTemplateLine, uidsFromList);
                                if (existingUIDs != null && existingUIDs.Any())
                                {
                                    existingrouteLoadTruckTemplateLine = routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Where(e => existingUIDs.Contains(e.UID)).ToList();
                                    newrouteLoadTruckTemplateLine = routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                                }
                                else
                                {
                                    newrouteLoadTruckTemplateLine = routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList;
                                }
                                if (newrouteLoadTruckTemplateLine != null && newrouteLoadTruckTemplateLine.Any())
                                {
                                    count += await InsertRouteLoadTruckTemplateLine(connection, transaction, newrouteLoadTruckTemplateLine);
                                }
                                if (existingrouteLoadTruckTemplateLine != null && existingrouteLoadTruckTemplateLine.Any())
                                {
                                    count += await UpdateRouteLoadTruckTemplateLine(connection, transaction, existingrouteLoadTruckTemplateLine);
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

        private async Task<int> InsertRouteLoadTruckTemplateLine(IDbConnection connection, IDbTransaction transaction, List<RouteLoadTruckTemplateLine> routeLoadTruckTemplateLines)
        {
            int retVal = -1;
            try
            {
                var routeLoadTruckTemplateLineQuery = @"INSERT INTO route_load_truck_template_line (uid, created_by, created_time, modified_by, modified_time, 
                                                    server_add_time, server_modified_time, org_uid, company_uid, route_load_truck_template_uid, sku_code, uom, 
                                                    monday_qty, monday_suggested_qty, tuesday_qty, tuesday_suggested_qty, wednesday_qty, wednesday_suggested_qty, 
                                                    thursday_qty, thursday_suggested_qty, friday_qty, friday_suggested_qty, saturday_qty, saturday_suggested_qty, 
                                                    sunday_qty, sunday_suggested_qty,line_number)
                                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @OrgUID, @CompanyUID, 
                                                    @RouteLoadTruckTemplateUID, @SKUCode, @UOM, @MondayQty, @MondaySuggestedQty, @TuesdayQty, @TuesdaySuggestedQty, @WednesdayQty, 
                                                    @WednesdaySuggestedQty, @ThursdayQty, @ThursdaySuggestedQty, @FridayQty, @FridaySuggestedQty, @SaturdayQty, @SaturdaySuggestedQty,
                                                    @SundayQty, @SundaySuggestedQty,@LineNumber);";

                retVal = await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLines);
            }
            catch
            {
                throw;
            }
            return retVal;

        }

        private async Task<int> UpdateRouteLoadTruckTemplateLine(IDbConnection connection, IDbTransaction transaction, List<RouteLoadTruckTemplateLine> routeLoadTruckTemplateLines)
        {
            int count = -1;
            try
            {
                var routeLoadTruckTemplateLineQuery = @"UPDATE route_load_truck_template_line
                                                                    SET
                                                                       modified_by = @ModifiedBy,
                                                                       modified_time = @ModifiedTime,
                                                                       server_modified_time = @ServerModifiedTime,
                                                                       uid = @UID,
                                                                       sku_code = @SKUCode,
                                                                       uom = @UOM,
                                                                       monday_qty = @MondayQty,
                                                                       monday_suggested_qty = @MondaySuggestedQty,
                                                                       tuesday_qty = @TuesdayQty,
                                                                       tuesday_suggested_qty = @TuesdaySuggestedQty,
                                                                       wednesday_qty = @WednesdayQty,
                                                                       wednesday_suggested_qty = @WednesdaySuggestedQty,
                                                                       thursday_qty = @ThursdayQty,
                                                                       thursday_suggested_qty = @ThursdaySuggestedQty,
                                                                       friday_qty = @FridayQty,
                                                                       friday_suggested_qty = @FridaySuggestedQty,
                                                                       saturday_qty = @SaturdayQty,
                                                                       saturday_suggested_qty = @SaturdaySuggestedQty,
                                                                       sunday_qty = @SundayQty,
                                                                       sunday_suggested_qty = @SundaySuggestedQty,
                                                                       line_number = @LineNumber
                                                                    WHERE uid = @UID;";
                count = await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLines);
            }
            catch
            {
                throw;
            }
            return count;

        }

        public async Task<int> DeleteRouteLoadTruckTemplate(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"
                    DELETE FROM route_load_truck_template
                    WHERE uid = @UID;

                    DELETE FROM route_load_truck_template_line
                    WHERE route_load_truck_template_uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> DeleteRouteLoadTruckTemplateLine(List<string> uids)
        {
            try
            {
                var parameters = new { RouteLoadTruckTemplateUIDs = uids };
                var sql = @"DELETE FROM route_load_truck_template_line WHERE uid IN @RouteLoadTruckTemplateUIDs;";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }


        }
    }
}
