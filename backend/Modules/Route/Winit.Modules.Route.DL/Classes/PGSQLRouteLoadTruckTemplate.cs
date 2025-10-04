using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
    public class PGSQLRouteLoadTruckTemplate : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IRouteLoadTruckTemplateDL
    {
        public PGSQLRouteLoadTruckTemplate(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate>> SelectRouteLoadTruckTemplateAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id AS Id,uid AS UID,ss AS SS,created_time AS CreatedTime,
                modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime
                ,route_uid AS RouteUID,template_name AS TemplateName,template_description AS TemplateDescription,
                    company_uid AS CompanyUID,org_uid AS OrgUID FROM route_load_truck_template");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM route_load_truck_template");
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteLoadTruckTemplate>().GetType();
                IEnumerable<Model.Interfaces.IRouteLoadTruckTemplate> routes = await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplate>(sql.ToString(), parameters, type);

                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

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
                Type routeLoadTruckTemplateSqlType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteLoadTruckTemplate>().GetType();
                List<Model.Interfaces.IRouteLoadTruckTemplate> routeLoadTruckTemplateList = await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplate>(routeLoadTruckTemplateSql.ToString(), Parameters, routeLoadTruckTemplateSqlType);
                var routeLoadTruckTemplateLineSQL = new StringBuilder(@"SELECT id AS Id,uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,
                modified_by AS ModifiedBy,modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,
                org_uid AS OrgUID,company_uid AS CompanyUID,route_load_truck_template_uid AS RouteLoadTruckTemplateUID,sku_code AS SKUCode,uom AS UOM,
                monday_qty AS MondayQty,monday_suggested_qty AS MondaySuggestedQty,tuesday_qty AS TuesdayQty,tuesday_suggested_qty AS TuesdaySuggestedQty,
                wednesday_qty AS WednesdayQty,wednesday_suggested_qty AS WednesdaySuggestedQty,thursday_qty AS ThursdayQty,
                thursday_suggested_qty AS ThursdaySuggestedQty,friday_qty AS FridayQty,friday_suggested_qty AS FridaySuggestedQty,
                saturday_qty AS SaturdayQty,saturday_suggested_qty AS SaturdaySuggestedQty,sunday_qty AS SundayQty,sunday_suggested_qty AS SundaySuggestedQty,
                line_number AS LineNumber FROM route_load_truck_template_line WHERE route_load_truck_template_uid=@UID 
                ORDER BY line_number DESC");
                Type routeLoadTruckTemplateLineSQLType = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteLoadTruckTemplateLine>().GetType();
                List<Model.Interfaces.IRouteLoadTruckTemplateLine> routeLoadTruckTemplateLineList = await ExecuteQueryAsync<Model.Interfaces.IRouteLoadTruckTemplateLine>(routeLoadTruckTemplateLineSQL.ToString(), Parameters, routeLoadTruckTemplateLineSQLType);
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
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var routeLoadTruckTemplateQuery = @"INSERT INTO route_load_truck_template (uid, created_by, created_time, modified_by,modified_time, server_add_time, 
                                              server_modified_time, route_uid, template_name, template_description, company_uid, org_uid)
                                                VALUES (@UID, @CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID, @TemplateName,
                                                @TemplateDescription, @CompanyUID, @OrgUID);";
                            var routeLoadTruckTemplateParameters = new Dictionary<string, object>
                    {
                        {"UID", routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.UID},
                        {"CreatedBy",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.CreatedBy},
                        {"CreatedTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.CreatedTime},
                        {"ModifiedBy",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedBy},
                        {"ModifiedTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedTime},
                        {"ServerAddTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ServerAddTime},
                        {"ServerModifiedTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ServerModifiedTime},
                        {"RouteUID", routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.RouteUID},
                        {"TemplateName",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateName},
                        {"TemplateDescription",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateDescription},
                        {"CompanyUID",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.CompanyUID},
                        {"OrgUID",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.OrgUID},
                    };

                            count += await ExecuteNonQueryAsync(routeLoadTruckTemplateQuery, connection, transaction, routeLoadTruckTemplateParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("RouteLoadTruckTemplate Insert failed");
                            }
                            foreach (var RouteLoadTruckTemplateLine in routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList)
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
                                var routeLoadTruckTemplateLineParameters = new Dictionary<string, object>
                        {
                        {"UID", RouteLoadTruckTemplateLine.UID},
                        {"CreatedBy", RouteLoadTruckTemplateLine.CreatedBy},
                        {"CreatedTime", RouteLoadTruckTemplateLine.CreatedTime},
                        {"ModifiedBy", RouteLoadTruckTemplateLine.ModifiedBy},
                        {"ModifiedTime", RouteLoadTruckTemplateLine.ModifiedTime},
                        {"ServerAddTime", RouteLoadTruckTemplateLine.ServerAddTime},
                        {"ServerModifiedTime", RouteLoadTruckTemplateLine.ServerModifiedTime},
                        {"OrgUID", RouteLoadTruckTemplateLine.OrgUID},
                        {"CompanyUID", RouteLoadTruckTemplateLine.CompanyUID},
                        {"RouteLoadTruckTemplateUID", RouteLoadTruckTemplateLine.RouteLoadTruckTemplateUID},
                        {"SKUCode", RouteLoadTruckTemplateLine.SKUCode},
                        {"UOM", RouteLoadTruckTemplateLine.UOM},
                        {"MondayQty", RouteLoadTruckTemplateLine.MondayQty},
                        {"MondaySuggestedQty", RouteLoadTruckTemplateLine.MondaySuggestedQty},
                        {"TuesdayQty", RouteLoadTruckTemplateLine.TuesdayQty},
                        {"TuesdaySuggestedQty", RouteLoadTruckTemplateLine.TuesdaySuggestedQty},
                        {"WednesdayQty", RouteLoadTruckTemplateLine.WednesdayQty},
                        {"WednesdaySuggestedQty", RouteLoadTruckTemplateLine.WednesdaySuggestedQty},
                        {"ThursdayQty", RouteLoadTruckTemplateLine.ThursdayQty},
                        {"ThursdaySuggestedQty", RouteLoadTruckTemplateLine.ThursdaySuggestedQty},
                        {"FridayQty", RouteLoadTruckTemplateLine.FridayQty},
                        {"FridaySuggestedQty", RouteLoadTruckTemplateLine.FridaySuggestedQty},
                        {"SaturdayQty", RouteLoadTruckTemplateLine.SaturdayQty},
                        {"SaturdaySuggestedQty", RouteLoadTruckTemplateLine.SaturdaySuggestedQty},
                        {"SundayQty", RouteLoadTruckTemplateLine.SundayQty},
                        {"SundaySuggestedQty", RouteLoadTruckTemplateLine.SundaySuggestedQty},
                        {"LineNumber", RouteLoadTruckTemplateLine.LineNumber},
                        };
                                count += await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLineParameters);
                                if (count < 0)
                                {
                                    transaction.Rollback();
                                    throw new Exception("RouteLoadTruckTemplateLine Table Insert Failed");
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
                            var routeLoadTruckTemplateParameters = new Dictionary<string, object>
                    {
                        {"UID", routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.UID},
                        {"ModifiedBy",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedBy},
                        {"ModifiedTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ModifiedTime},
                        {"ServerModifiedTime",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.ServerModifiedTime},
                        {"TemplateName",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateName},
                        {"TemplateDescription",  routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplate.TemplateDescription},
                    };

                            count += await ExecuteNonQueryAsync(routeLoadTruckTemplateQuery, connection, transaction, routeLoadTruckTemplateParameters);
                            if (count != 1)
                            {
                                transaction.Rollback();
                                throw new Exception("RouteLoadTruckTemplate Update failed");
                            }


                            foreach (var RouteLoadTruckTemplateLine in routeLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList)
                            {
                                switch (RouteLoadTruckTemplateLine.ActionTypes)
                                {
                                    case ActionType.Add:
                                        count += await InsertRouteLoadTruckTemplateLine(connection, transaction, RouteLoadTruckTemplateLine);
                                        break;

                                    case ActionType.Update:
                                        count += await UpdateRouteLoadTruckTemplateLine(connection, transaction, RouteLoadTruckTemplateLine);
                                        break;
                                }
                                if (count < 0)
                                {
                                    transaction.Rollback();
                                    throw new Exception("RouteLoadTruckTemplateLine Table Update Failed");
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

        private async Task<int> InsertRouteLoadTruckTemplateLine(NpgsqlConnection connection, NpgsqlTransaction transaction, RouteLoadTruckTemplateLine routeLoadTruckTemplateLine)
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
            var routeLoadTruckTemplateLineParameters = new Dictionary<string, object>
                        {
                        {"UID", routeLoadTruckTemplateLine.UID},
                        {"CreatedBy", routeLoadTruckTemplateLine.CreatedBy},
                        {"CreatedTime", routeLoadTruckTemplateLine.CreatedTime},
                        {"ModifiedBy", routeLoadTruckTemplateLine.ModifiedBy},
                        {"ModifiedTime", routeLoadTruckTemplateLine.ModifiedTime},
                        {"ServerAddTime", routeLoadTruckTemplateLine.ServerAddTime},
                        {"ServerModifiedTime", routeLoadTruckTemplateLine.ServerModifiedTime},
                        {"OrgUID", routeLoadTruckTemplateLine.OrgUID},
                        {"CompanyUID", routeLoadTruckTemplateLine.CompanyUID},
                        {"RouteLoadTruckTemplateUID", routeLoadTruckTemplateLine.RouteLoadTruckTemplateUID},
                        {"SKUCode", routeLoadTruckTemplateLine.SKUCode},
                        {"UOM", routeLoadTruckTemplateLine.UOM},
                        {"MondayQty", routeLoadTruckTemplateLine.MondayQty},
                        {"MondaySuggestedQty", routeLoadTruckTemplateLine.MondaySuggestedQty},
                        {"TuesdayQty", routeLoadTruckTemplateLine.TuesdayQty},
                        {"TuesdaySuggestedQty", routeLoadTruckTemplateLine.TuesdaySuggestedQty},
                        {"WednesdayQty", routeLoadTruckTemplateLine.WednesdayQty},
                        {"WednesdaySuggestedQty", routeLoadTruckTemplateLine.WednesdaySuggestedQty},
                        {"ThursdayQty", routeLoadTruckTemplateLine.ThursdayQty},
                        {"ThursdaySuggestedQty", routeLoadTruckTemplateLine.ThursdaySuggestedQty},
                        {"FridayQty", routeLoadTruckTemplateLine.FridayQty},
                        {"FridaySuggestedQty", routeLoadTruckTemplateLine.FridaySuggestedQty},
                        {"SaturdayQty", routeLoadTruckTemplateLine.SaturdayQty},
                        {"SaturdaySuggestedQty", routeLoadTruckTemplateLine.SaturdaySuggestedQty},
                        {"SundayQty", routeLoadTruckTemplateLine.SundayQty},
                        {"SundaySuggestedQty", routeLoadTruckTemplateLine.SundaySuggestedQty},
                        {"LineNumber", routeLoadTruckTemplateLine.LineNumber},
                        };
            return await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLineParameters);
        }

        private async Task<int> UpdateRouteLoadTruckTemplateLine(NpgsqlConnection connection, NpgsqlTransaction transaction, RouteLoadTruckTemplateLine routeLoadTruckTemplateLine)
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
            var routeLoadTruckTemplateLineParameters = new Dictionary<string, object>
                        {
                        //{"UID", RouteLoadTruckTemplateLine.UID},
                        {"ModifiedBy", routeLoadTruckTemplateLine.ModifiedBy},
                        {"ModifiedTime", routeLoadTruckTemplateLine.ModifiedTime},
                        {"ServerModifiedTime", routeLoadTruckTemplateLine.ServerModifiedTime},
                        {"UID", routeLoadTruckTemplateLine.UID},
                        {"SKUCode", routeLoadTruckTemplateLine.SKUCode},
                        {"UOM", routeLoadTruckTemplateLine.UOM},
                        {"MondayQty", routeLoadTruckTemplateLine.MondayQty},
                        {"MondaySuggestedQty", routeLoadTruckTemplateLine.MondaySuggestedQty},
                        {"TuesdayQty", routeLoadTruckTemplateLine.TuesdayQty},
                        {"TuesdaySuggestedQty", routeLoadTruckTemplateLine.TuesdaySuggestedQty},
                        {"WednesdayQty", routeLoadTruckTemplateLine.WednesdayQty},
                        {"WednesdaySuggestedQty", routeLoadTruckTemplateLine.WednesdaySuggestedQty},
                        {"ThursdayQty", routeLoadTruckTemplateLine.ThursdayQty},
                        {"ThursdaySuggestedQty", routeLoadTruckTemplateLine.ThursdaySuggestedQty},
                        {"FridayQty", routeLoadTruckTemplateLine.FridayQty},
                        {"FridaySuggestedQty", routeLoadTruckTemplateLine.FridaySuggestedQty},
                        {"SaturdayQty", routeLoadTruckTemplateLine.SaturdayQty},
                        {"SaturdaySuggestedQty", routeLoadTruckTemplateLine.SaturdaySuggestedQty},
                        {"SundayQty", routeLoadTruckTemplateLine.SundayQty},
                        {"SundaySuggestedQty", routeLoadTruckTemplateLine.SundaySuggestedQty},
                        {"LineNumber", routeLoadTruckTemplateLine.LineNumber},
                        };
            return await ExecuteNonQueryAsync(routeLoadTruckTemplateLineQuery, connection, transaction, routeLoadTruckTemplateLineParameters);
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

        public async Task<int> DeleteRouteLoadTruckTemplateLine(List<string> RouteLoadTruckTemplateUIDs)
        {
            string commaSeparatedUIDs = string.Join(",", RouteLoadTruckTemplateUIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "RouteLoadTruckTemplateUIDs", commaSeparatedUIDs }
                };
            var sql = @"DELETE FROM route_load_truck_template_line WHERE uid = ANY(string_to_array(@RouteLoadTruckTemplateUIDs, ','));";
            return await ExecuteNonQueryAsync(sql, parameters);

        }
    }
}
