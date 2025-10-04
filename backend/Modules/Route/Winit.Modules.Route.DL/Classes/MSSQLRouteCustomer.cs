using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.DL.Classes
{
    public class MSSQLRouteCustomer : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IRouteCustomerDL
    {
        public MSSQLRouteCustomer(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>> SelectRouteCustomerAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT 
                        r.route_uid AS RouteUID, 
                        r.store_uid AS StoreUID, 
                        r.seq_no AS SeqNo, 
                        r.visit_time AS VisitTime, 
                        r.visit_duration AS VisitDuration, 
                        r.end_time AS EndTime, 
                        r.is_deleted AS IsDeleted, 
                        r.travel_time AS TravelTime, 
                        r.action_type AS ActionType
                    FROM route_customer r");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM route_customer");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>(filterCriterias, sbFilterCriteria, parameters);

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

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteCustomer>().GetType();
                IEnumerable<Model.Interfaces.IRouteCustomer> routeCustomers = await ExecuteQueryAsync<Model.Interfaces.IRouteCustomer>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> pagedResponse = new PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>
                {
                    PagedData = routeCustomers,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> SelectRouteCustomerDetailByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"
                SELECT 
                    r.route_uid AS RouteUID, 
                    r.store_uid AS StoreUID, 
                    r.seq_no AS SeqNo, 
                    r.visit_time AS VisitTime, 
                    r.visit_duration AS VisitDuration, 
                    r.end_time AS EndTime, 
                    r.is_deleted AS IsDeleted, 
                    r.travel_time AS TravelTime, 
                    r.action_type AS ActionType
                FROM route_customer r 
                WHERE uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteCustomer>().GetType();
            Model.Interfaces.IRouteCustomer RouteCustomerList = await ExecuteSingleAsync<Model.Interfaces.IRouteCustomer>(sql, parameters, type);
            return RouteCustomerList;
        }

        public async Task<int> CreateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            var sql = @"INSERT INTO route_customer (uid, route_uid, store_uid, seq_no, visit_time, visit_duration,end_time,is_deleted,
                      created_by, created_time,modified_by, modified_time, server_add_time, server_modified_time)  VALUES (@UID, @RouteUID, 
                       @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,
                       @ServerModifiedTime);";

            return await ExecuteNonQueryAsync(sql, routecustomerDetails);
        }
        public async Task<int> UpdateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            try
            {
                var sql = @"UPDATE route_customer SET seq_no = @SeqNo, visit_time = @VisitTime, visit_duration = @VisitDuration,
                         end_time = @EndTime, is_deleted = @IsDeleted , 
                          modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                         server_modified_time = @ServerModifiedTime WHERE uid = @UID;";

                return await ExecuteNonQueryAsync(sql, routecustomerDetails);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteRouteCustomerDetails(List<string> UIDs)
        {
            string commaSeparatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UIDs", commaSeparatedUIDs }
                };
            var sql = @"UPDATE route_customer SET  is_deleted=TRUE WHERE uid IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));";
            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<IEnumerable<SelectionItem>> GetRouteByStoreUID(string storeUID)
        {
            string sql = @"Select distinct r.uid AS UID, r.code AS Code, r.name AS Label  FROM route_customer RC inner join route R 
                            ON RC.route_uid = r.uid WHERE RC.store_uid = @UID";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  storeUID}
            };
            Type type = typeof(SelectionItem);
            IEnumerable<SelectionItem> routeCustomers = await ExecuteQueryAsync<SelectionItem>(sql.ToString(), parameters, type);
            return routeCustomers;
        }
    }
}
