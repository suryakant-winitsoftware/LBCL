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
    public class SQLiteRouteCustomerDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager,IRouteCustomerDL
    {
        public SQLiteRouteCustomerDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>> SelectRouteCustomerAllDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, route_uid, store_uid, seq_no, visit_time, visit_duration, end_time, is_deleted, travel_time\r\n\tFROM public.route_customer");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM route_customer");
                }
                var parameters = new Dictionary<string, object>();

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
                //    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}

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
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, route_uid, store_uid, seq_no, visit_time, visit_duration, end_time, is_deleted, travel_time
	FROM public.route_customer WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteCustomer>().GetType();
             Model.Interfaces.IRouteCustomer RouteCustomerList = await ExecuteSingleAsync<Model.Interfaces.IRouteCustomer>(sql, parameters, type);
            return RouteCustomerList;
        }

        public async Task<int> CreateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            var sql = @"INSERT INTO route_customer (uid AS UID,route_uid AS RouteUID,store_uid AS StoreUID,seq_no AS SeqNo,visit_time AS VisitTime,visit_duration AS VisitDuration,
                        end_time AS EndTime,is_deleted AS IsDeleted,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                        modified_time AS  ModifiedTime,server_add_time AS  ServerAddTime,server_modified_time AS ServerModifiedTime)  VALUES (@UID, @RouteUID, 
                       @StoreUID, @SeqNo, @VisitTime, @VisitDuration, @EndTime,@IsDeleted,@CreatedTime,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,
                       @ServerModifiedTime);";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", routecustomerDetails.UID},
                   {"RouteUID", routecustomerDetails.RouteUID},
                   {"StoreUID", routecustomerDetails.StoreUID},
                   {"SeqNo", routecustomerDetails.SeqNo},
                   {"VisitTime", routecustomerDetails.VisitTime},
                   {"VisitDuration", routecustomerDetails.VisitDuration},
                   {"EndTime", routecustomerDetails.EndTime},
                   {"IsDeleted", routecustomerDetails.IsDeleted},
                   {"CreatedTime", routecustomerDetails.CreatedTime},
                   {"CreatedBy", routecustomerDetails.CreatedBy},
                   {"ModifiedBy", routecustomerDetails.ModifiedBy},
                   {"ModifiedTime", routecustomerDetails.ModifiedTime},
                   {"ServerAddTime", routecustomerDetails.ServerAddTime},
                   {"ServerModifiedTime", routecustomerDetails.ServerModifiedTime},
             };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<int> UpdateRouteCustomerDetails(Winit.Modules.Route.Model.Interfaces.IRouteCustomer routecustomerDetails)
        {
            try
            {
                var sql = @"UPDATE route_customer SET seq_no AS SeqNo = @SeqNo,visit_time AS VisitTime = @VisitTime,visit_duration AS VisitDuration = @VisitDuration,
                         end_time AS EndTime = @EndTime,is_deleted AS IsDeleted = @IsDeleted,valid_from AS ValidFrom = @ValidFrom , 
                         valid_upto AS ValidUpto = @ValidUpto,modified_by AS ModifiedBy = @ModifiedBy,modified_time AS ModifiedTime = @ModifiedTime,
                         server_modified_time AS ServerModifiedTime = @ServerModifiedTime, WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", routecustomerDetails.UID},
                   {"SeqNo", routecustomerDetails.SeqNo},
                   {"VisitTime", routecustomerDetails.VisitTime},
                   {"VisitDuration", routecustomerDetails.VisitDuration},
                   {"EndTime", routecustomerDetails.EndTime},
                   {"IsDeleted", routecustomerDetails.IsDeleted},
                   {"ModifiedBy", routecustomerDetails.ModifiedBy},
                   {"ModifiedTime", routecustomerDetails.ModifiedTime},
                   {"ServerModifiedTime", routecustomerDetails.ServerModifiedTime},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
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
            var sql = @"DELETE  FROM route_customer WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<IEnumerable<SelectionItem>> GetRouteByStoreUID(string UID)
        {
            throw new NotImplementedException();
        }
    }
}
