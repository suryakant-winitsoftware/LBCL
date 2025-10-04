using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Classes
{
    public class PGSQLBeatHistoryDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, Interfaces.IBeatHistoryDL
    {
        public PGSQLBeatHistoryDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> SelectAllBeatHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM(SELECT 
                                                        bh.id AS Id,
                                                        bh.uid AS UID,
                                                        bh.created_by AS CreatedBy,
                                                        bh.created_time AS CreatedTime,
                                                        bh.modified_by AS ModifiedBy,
                                                        bh.modified_time AS ModifiedTime,
                                                        bh.server_add_time AS ServerAddTime,
                                                        bh.server_modified_time AS ServerModifiedTime,
                                                        bh.user_journey_uid AS UserJourneyUID,
                                                        bh.route_uid AS RouteUID,
                                                        bh.start_time AS StartTime,
                                                        bh.end_time AS EndTime,
                                                        bh.job_position_uid AS JobPositionUID,
                                                        bh.login_id AS LoginId,
                                                        bh.visit_date AS VisitDate,
                                                        bh.location_uid AS LocationUID,
                                                        bh.planned_start_time AS PlannedStartTime,
                                                        bh.planned_end_time AS PlannedEndTime,
                                                        bh.planned_store_visits AS PlannedStoreVisits,
                                                        bh.unplanned_store_visits AS UnplannedStoreVisits,
                                                        bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                                                        bh.msl_store_visits AS MSLStoreVisits,
                                                        bh.skipped_store_visits AS SkippedStoreVisits,
                                                        bh.actual_store_visits AS ActualStoreVisits,
                                                        bh.coverage AS Coverage,
                                                        bh.a_coverage AS ACoverage,
                                                        bh.t_coverage AS TCoverage,
                                                        bh.invoice_status AS InvoiceStatus,
                                                        bh.notes AS Notes,
                                                        bh.invoice_finalization_date AS InvoiceFinalizationDate,
                                                        bh.route_wh_org_uid AS RouteWHOrgUID,
                                                        bh.cfd_time AS CFDTime,
                                                        bh.has_audit_completed AS HasAuditCompleted,
                                                        bh.wh_stock_audit_uid AS WHStockAuditUID,
                                                        bh.ss AS SS,
                                                        bh.default_job_position_uid AS DefaultJobPositionUID,
                                                        bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                                                        bh.year_month AS YearMonth
                                                    FROM 
                                                        beat_history bh)AS SUBQUERY");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                        bh.id AS Id,
                                                        bh.uid AS UID,
                                                        bh.created_by AS CreatedBy,
                                                        bh.created_time AS CreatedTime,
                                                        bh.modified_by AS ModifiedBy,
                                                        bh.modified_time AS ModifiedTime,
                                                        bh.server_add_time AS ServerAddTime,
                                                        bh.server_modified_time AS ServerModifiedTime,
                                                        bh.user_journey_uid AS UserJourneyUID,
                                                        bh.route_uid AS RouteUID,
                                                        bh.start_time AS StartTime,
                                                        bh.end_time AS EndTime,
                                                        bh.job_position_uid AS JobPositionUID,
                                                        bh.login_id AS LoginId,
                                                        bh.visit_date AS VisitDate,
                                                        bh.location_uid AS LocationUID,
                                                        bh.planned_start_time AS PlannedStartTime,
                                                        bh.planned_end_time AS PlannedEndTime,
                                                        bh.planned_store_visits AS PlannedStoreVisits,
                                                        bh.unplanned_store_visits AS UnplannedStoreVisits,
                                                        bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                                                        bh.msl_store_visits AS MSLStoreVisits,
                                                        bh.skipped_store_visits AS SkippedStoreVisits,
                                                        bh.actual_store_visits AS ActualStoreVisits,
                                                        bh.coverage AS Coverage,
                                                        bh.a_coverage AS ACoverage,
                                                        bh.t_coverage AS TCoverage,
                                                        bh.invoice_status AS InvoiceStatus,
                                                        bh.notes AS Notes,
                                                        bh.invoice_finalization_date AS InvoiceFinalizationDate,
                                                        bh.route_wh_org_uid AS RouteWHOrgUID,
                                                        bh.cfd_time AS CFDTime,
                                                        bh.has_audit_completed AS HasAuditCompleted,
                                                        bh.wh_stock_audit_uid AS WHStockAuditUID,
                                                        bh.ss AS SS,
                                                        bh.default_job_position_uid AS DefaultJobPositionUID,
                                                        bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                                                        bh.year_month AS YearMonth
                                                    FROM 
                                                        beat_history bh)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(filterCriterias, sbFilterCriteria, parameters);
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
                else if (pageNumber > 0 && pageSize > 0)
                {
                    // PostgreSQL requires ORDER BY when using OFFSET/LIMIT
                    sql.Append(" ORDER BY Id");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} LIMIT {pageSize}");
                }
                
                // Log the SQL query for debugging
                Console.WriteLine($"[DEBUG] BeatHistory SQL Query: {sql.ToString()}");
                Console.WriteLine($"[DEBUG] PageNumber: {pageNumber}, PageSize: {pageSize}");
                
                // Use concrete class for proper deserialization
                var type = typeof(Winit.Modules.JourneyPlan.Model.Classes.BeatHistory);
                IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> beatHistoryDetails = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql.ToString(), parameters, type);
                
                // Log the results for debugging
                var beatHistoryList = beatHistoryDetails?.ToList() ?? new List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>();
                Console.WriteLine($"[DEBUG] Records returned: {beatHistoryList.Count}");
                
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                    Console.WriteLine($"[DEBUG] Total count: {totalCount}");
                }
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>
                {
                    PagedData = beatHistoryList,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatHistoryByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = @"SELECT 
                                    bh.id AS Id,
                                    bh.uid AS UID,
                                    bh.created_by AS CreatedBy,
                                    bh.created_time AS CreatedTime,
                                    bh.modified_by AS ModifiedBy,
                                    bh.modified_time AS ModifiedTime,
                                    bh.server_add_time AS ServerAddTime,
                                    bh.server_modified_time AS ServerModifiedTime,
                                    bh.user_journey_uid AS UserJourneyUID,
                                    bh.route_uid AS RouteUID,
                                    bh.start_time AS StartTime,
                                    bh.end_time AS EndTime,
                                    bh.job_position_uid AS JobPositionUID,
                                    bh.login_id AS LoginId,
                                    bh.visit_date AS VisitDate,
                                    bh.location_uid AS LocationUID,
                                    bh.planned_start_time AS PlannedStartTime,
                                    bh.planned_end_time AS PlannedEndTime,
                                    bh.planned_store_visits AS PlannedStoreVisits,
                                    bh.unplanned_store_visits AS UnplannedStoreVisits,
                                    bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                                    bh.msl_store_visits AS MSLStoreVisits,
                                    bh.skipped_store_visits AS SkippedStoreVisits,
                                    bh.actual_store_visits AS ActualStoreVisits,
                                    bh.coverage AS Coverage,
                                    bh.a_coverage AS ACoverage,
                                    bh.t_coverage AS TCoverage,
                                    bh.invoice_status AS InvoiceStatus,
                                    bh.notes AS Notes,
                                    bh.invoice_finalization_date AS InvoiceFinalizationDate,
                                    bh.route_wh_org_uid AS RouteWHOrgUID,
                                    bh.cfd_time AS CFDTime,
                                    bh.has_audit_completed AS HasAuditCompleted,
                                    bh.wh_stock_audit_uid AS WHStockAuditUID,
                                    bh.ss AS SS,
                                    bh.default_job_position_uid AS DefaultJobPositionUID,
                                    bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                                    bh.year_month AS YearMonth
                                FROM 
                                    beat_history bh WHERE bh.uid = @UID";
                var type = typeof(Winit.Modules.JourneyPlan.Model.Classes.BeatHistory);
                Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory? beatHistoryDetails = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);
                return beatHistoryDetails;

            }
            catch
            {
                throw;
            }

        }
        public async Task<int> CreateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
        {
            try
            {
                var sql = @"INSERT INTO beat_history (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                           server_modified_time, user_journey_uid, route_uid, start_time, end_time, job_position_uid, login_id, 
                           visit_date, location_uid, planned_start_time, planned_end_time, planned_store_visits, unplanned_store_visits, 
                           zero_sales_store_visits, msl_store_visits, skipped_store_visits, actual_store_visits, coverage, a_coverage, 
                           t_coverage, invoice_status, notes, invoice_finalization_date, route_wh_org_uid, cfd_time, has_audit_completed, 
                            wh_stock_audit_uid, ss, default_job_position_uid, user_journey_vehicle_uid,year_month)
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @UserJourneyUID,
                            @RouteUID, @StartTime, @EndTime, @JobPositionUID, @LoginId, @VisitDate, @LocationUID, @PlannedStartTime, 
                            @PlannedEndTime, @PlannedStoreVisits, @UnPlannedStoreVisits, @ZeroSalesStoreVisits, @MSLStoreVisits, 
                            @SkippedStoreVisits, @ActualStoreVisits, @Coverage, @ACoverage, @TCoverage, @InvoiceStatus, @Notes, 
                            @InvoiceFinalizationDate, @RouteWHOrgUID, @CFDTime, @HasAuditCompleted, @WHStockAuditUID, @SS, 
                            @DefaultJobPositionUID, @UserJourneyVehicleUID,@YearMonth);";

                return await ExecuteNonQueryAsync(sql, beatHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
        {
            try
            {
                var sql = @"UPDATE beat_history 
                                SET 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                start_time = @StartTime, 
                                end_time = @EndTime, 
                                visit_date = @VisitDate, 
                                planned_start_time = @PlannedStartTime, 
                                planned_end_time = @PlannedEndTime, 
                                planned_store_visits = @PlannedStoreVisits, 
                                unplanned_store_visits = @UnPlannedStoreVisits, 
                                zero_sales_store_visits = @ZeroSalesStoreVisits, 
                                msl_store_visits = @MSLStoreVisits, 
                                skipped_store_visits = @SkippedStoreVisits, 
                                actual_store_visits = @ActualStoreVisits, 
                                coverage = @Coverage, 
                                a_coverage = @ACoverage, 
                                t_coverage = @TCoverage, 
                                invoice_status = @InvoiceStatus, 
                                notes = @Notes, 
                                invoice_finalization_date = @InvoiceFinalizationDate, 
                                cfd_time = @CFDTime, 
                                has_audit_completed = @HasAuditCompleted, 
                                ss = @SS,
                                year_month = @YearMonth
                            WHERE 
                                UID = @UID";
                return await ExecuteNonQueryAsync(sql, beatHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBeatHistory(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
                var sql = @"DELETE  FROM beat_history WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }

        }

        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"RouteUID", RouteUID}
                            };

                var sql = @"SELECT 
                            bh.id AS Id,
                            bh.uid AS UID,
                            bh.created_by AS CreatedBy,
                            bh.created_time AS CreatedTime,
                            bh.modified_by AS ModifiedBy,
                            bh.modified_time AS ModifiedTime,
                            bh.server_add_time AS ServerAddTime,
                            bh.server_modified_time AS ServerModifiedTime,
                            bh.user_journey_uid AS UserJourneyUID,
                            bh.route_uid AS RouteUID,
                            bh.start_time AS StartTime,
                            bh.end_time AS EndTime,
                            bh.job_position_uid AS JobPositionUID,
                            bh.login_id AS LoginId,
                            bh.visit_date AS VisitDate,
                            bh.location_uid AS LocationUID,
                            bh.planned_start_time AS PlannedStartTime,
                            bh.planned_end_time AS PlannedEndTime,
                            bh.planned_store_visits AS PlannedStoreVisits,
                            bh.unplanned_store_visits AS UnplannedStoreVisits,
                            bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                            bh.msl_store_visits AS MSLStoreVisits,
                            bh.skipped_store_visits AS SkippedStoreVisits,
                            bh.actual_store_visits AS ActualStoreVisits,
                            bh.coverage AS Coverage,
                            bh.a_coverage AS ACoverage,
                            bh.t_coverage AS TCoverage,
                            bh.invoice_status AS InvoiceStatus,
                            bh.notes AS Notes,
                            bh.invoice_finalization_date AS InvoiceFinalizationDate,
                            bh.route_wh_org_uid AS RouteWHOrgUID,
                            bh.cfd_time AS CFDTime,
                            bh.has_audit_completed AS HasAuditCompleted,
                            bh.wh_stock_audit_uid AS WHStockAuditUID,
                            bh.ss AS SS,
                            bh.default_job_position_uid AS DefaultJobPositionUID,
                            bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                            bh.year_month AS YearMonth
                        FROM 
                            beat_history bh WHERE bh.route_uid  = @RouteUID AND date(visit_date) > current_date";


                var type = typeof(Winit.Modules.JourneyPlan.Model.Classes.BeatHistory);
                Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory? selectedBeatHistory = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);

                return selectedBeatHistory;
            }
            catch
            {
                throw;
            }

        }

        public async Task<IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"RouteUID", RouteUID}
                };
                var sql = @"SELECT 
                            bh.id AS Id,
                            bh.uid AS UID,
                            bh.created_by AS CreatedBy,
                            bh.created_time AS CreatedTime,
                            bh.modified_by AS ModifiedBy,
                            bh.modified_time AS ModifiedTime,
                            bh.server_add_time AS ServerAddTime,
                            bh.server_modified_time AS ServerModifiedTime,
                            bh.user_journey_uid AS UserJourneyUID,
                            bh.route_uid AS RouteUID,
                            bh.start_time AS StartTime,
                            bh.end_time AS EndTime,
                            bh.job_position_uid AS JobPositionUID,
                            bh.login_id AS LoginId,
                            bh.visit_date AS VisitDate,
                            bh.location_uid AS LocationUID,
                            bh.planned_start_time AS PlannedStartTime,
                            bh.planned_end_time AS PlannedEndTime,
                            bh.planned_store_visits AS PlannedStoreVisits,
                            bh.unplanned_store_visits AS UnplannedStoreVisits,
                            bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                            bh.msl_store_visits AS MSLStoreVisits,
                            bh.skipped_store_visits AS SkippedStoreVisits,
                            bh.actual_store_visits AS ActualStoreVisits,
                            bh.coverage AS Coverage,
                            bh.a_coverage AS ACoverage,
                            bh.t_coverage AS TCoverage,
                            bh.invoice_status AS InvoiceStatus,
                            bh.notes AS Notes,
                            bh.invoice_finalization_date AS InvoiceFinalizationDate,
                            bh.route_wh_org_uid AS RouteWHOrgUID,
                            bh.cfd_time AS CFDTime,
                            bh.has_audit_completed AS HasAuditCompleted,
                            bh.wh_stock_audit_uid AS WHStockAuditUID,
                            bh.ss AS SS,
                            bh.default_job_position_uid AS DefaultJobPositionUID,
                            bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                            bh.year_month AS YearMonth
                        FROM 
                            beat_history bh 
                        WHERE bh.route_uid = @RouteUID 
                        ORDER BY bh.visit_date DESC";

                // Use direct Dapper query to avoid the interface deserialization issue
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var beatHistories = await Dapper.SqlMapper.QueryAsync<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>(connection, sql, parameters);
                return beatHistories;
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID, int pageNumber, int pageSize)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetAllBeatHistoriesByRouteUID (Paginated) - RouteUID: {RouteUID}, PageNumber: {pageNumber}, PageSize: {pageSize}");
                
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"RouteUID", RouteUID}
                };
                
                var sql = new StringBuilder(@"SELECT 
                            bh.id AS Id,
                            bh.uid AS UID,
                            bh.created_by AS CreatedBy,
                            bh.created_time AS CreatedTime,
                            bh.modified_by AS ModifiedBy,
                            bh.modified_time AS ModifiedTime,
                            bh.server_add_time AS ServerAddTime,
                            bh.server_modified_time AS ServerModifiedTime,
                            bh.user_journey_uid AS UserJourneyUID,
                            bh.route_uid AS RouteUID,
                            bh.start_time AS StartTime,
                            bh.end_time AS EndTime,
                            bh.job_position_uid AS JobPositionUID,
                            bh.login_id AS LoginId,
                            bh.visit_date AS VisitDate,
                            bh.location_uid AS LocationUID,
                            bh.planned_start_time AS PlannedStartTime,
                            bh.planned_end_time AS PlannedEndTime,
                            bh.planned_store_visits AS PlannedStoreVisits,
                            bh.unplanned_store_visits AS UnplannedStoreVisits,
                            bh.zero_sales_store_visits AS ZeroSalesStoreVisits,
                            bh.msl_store_visits AS MSLStoreVisits,
                            bh.skipped_store_visits AS SkippedStoreVisits,
                            bh.actual_store_visits AS ActualStoreVisits,
                            bh.coverage AS Coverage,
                            bh.a_coverage AS ACoverage,
                            bh.t_coverage AS TCoverage,
                            bh.invoice_status AS InvoiceStatus,
                            bh.notes AS Notes,
                            bh.invoice_finalization_date AS InvoiceFinalizationDate,
                            bh.route_wh_org_uid AS RouteWHOrgUID,
                            bh.cfd_time AS CFDTime,
                            bh.has_audit_completed AS HasAuditCompleted,
                            bh.wh_stock_audit_uid AS WHStockAuditUID,
                            bh.ss AS SS,
                            bh.default_job_position_uid AS DefaultJobPositionUID,
                            bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,
                            bh.year_month AS YearMonth
                        FROM 
                            beat_history bh 
                        WHERE bh.route_uid = @RouteUID 
                        ORDER BY bh.visit_date DESC");

                // Add pagination
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} LIMIT {pageSize}");
                }

                Console.WriteLine($"[DEBUG] Final SQL Query: {sql.ToString()}");

                // Get count
                var countSql = @"SELECT COUNT(1) FROM beat_history WHERE route_uid = @RouteUID";
                
                // Use direct Dapper query to avoid the interface deserialization issue
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var beatHistories = await Dapper.SqlMapper.QueryAsync<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>(connection, sql.ToString(), parameters);
                var beatHistoryList = beatHistories?.ToList() ?? new List<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>();
                var totalCount = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, countSql, parameters);
                
                Console.WriteLine($"[DEBUG] Records returned: {beatHistoryList.Count}, Total count: {totalCount}");
                
                return new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>
                {
                    PagedData = beatHistoryList,
                    TotalCount = totalCount
                };
            }
            catch
            {
                throw;
            }
        }

        public Task<IEnumerable<IStoreItemView>> GetCustomersByBeatHistoryUID(string BeatHistoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IOrgHeirarchy>> LoadOrgHeirarchy(IStoreItemView selectedcustomer)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateStoreHistoryStatus(string StoreHistoryUID, string Status)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreHistory>> GetStoreHistoriesByUserJourneyUID(string userJourneyUID)
        {
            try
            {
                var sql = @"SELECT 
                    id, uid, created_by as CreatedBy, created_time as CreatedTime, 
                    modified_by as ModifiedBy, modified_time as ModifiedTime,
                    server_add_time as ServerAddTime, server_modified_time as ServerModifiedTime,
                    user_journey_uid as UserJourneyUID, year_month as YearMonth,
                    beat_history_uid as BeatHistoryUID, org_uid as OrgUID,
                    route_uid as RouteUID, store_uid as StoreUID,
                    is_planned as IsPlanned, serial_no as SerialNo,
                    status as Status, visit_duration as VisitDuration,
                    travel_time as TravelTime, planned_login_time as PlannedLoginTime,
                    planned_logout_time as PlannedLogoutTime, login_time as LoginTime,
                    logout_time as LogoutTime, no_of_visits as NoOfVisits,
                    last_visit_date as LastVisitDate, is_skipped as IsSkipped,
                    is_productive as IsProductive, is_green as IsGreen,
                    target_value as TargetValue, target_volume as TargetVolume,
                    target_lines as TargetLines, actual_value as ActualValue,
                    actual_volume as ActualVolume, actual_lines as ActualLines,
                    planned_time_spend_in_minutes as PlannedTimeSpendInMinutes,
                    latitude as Latitude, longitude as Longitude,
                    notes as Notes, ss as SS
                FROM store_history 
                WHERE user_journey_uid = @UserJourneyUID
                ORDER BY created_time";

                var parameters = new Dictionary<string, object?>
                {
                    ["UserJourneyUID"] = userJourneyUID
                };

                Type type = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreHistory>().GetType();
                IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreHistory> storeHistories = 
                    await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreHistory>(sql, parameters, type);

                return storeHistories;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> CreateStoreHistoryStats(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView,
                                                                                                                    int totalTimeInMin,
                                                                                                                    bool isForceCheckIn, string UID, string empUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateExceptionLog(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnCheckout(IStoreItemView storeItemView)
        {
            throw new NotImplementedException();
        }

        public Task<IBeatHistory> GetBeatSummaryFromStoreHistory(string BeatHistoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateBeathistory_Checkout(IBeatHistory beatHistoryTarget, string BeatHistoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> CheckCustomerExistsInJP(string StoreUID, string BeatHistoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddCustomerInJP(string UID, string StoreUID, string BeatHistoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IStandardListSource>> GetPendingPushData()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertUserJourney(IUserJourney userJourney)
        {
            throw new NotImplementedException();
        }

        public Task<List<IJPBeatHistory>> GetActiveOrTodayBeatHistory()
        {
            throw new NotImplementedException();
        }

        public Task<int> StartBeatHistory(IJPBeatHistory beatHistory)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateUserJourneyUIDForBeatHistory(IJPBeatHistory beatHistory)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAlreadyCollectedLoadRequestCountForRoute()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAlreadyVisitedCustomerCountForRoute()
        {
            throw new NotImplementedException();
        }

        public Task<int> OpenBeatHistory(IJPBeatHistory beatHistory)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateStockAuditAndStopBeatHistory(string stockAuditUID, IBeatHistory beatHistory, DateTime stopTime)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateBeatHistoryUIDInUserJourney(string beatHistoryUID, string userJourneyUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteUserJourney()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> InsertMasterRabbitMQueue(MasterDTO masterDTO)
        {
            try
            {
                if (masterDTO == null)
                {
                    return false;
                }
                if (masterDTO.BeatHistoryList != null && masterDTO.BeatHistoryList.Any())
                {
                    await CreateBeatHistoryList(masterDTO.BeatHistoryList);
                }
                if (masterDTO.StoreHistoryList != null && masterDTO.StoreHistoryList.Any())
                {
                    await CreateStoreHistoryList(masterDTO.StoreHistoryList);
                }
                if (masterDTO.StoreHistoryStatsList != null && masterDTO.StoreHistoryStatsList.Any())
                {
                    await CreateStoreHistoryStatsList(masterDTO.StoreHistoryStatsList);
                }
                if (masterDTO.JobPositionAttendenceList != null && masterDTO.JobPositionAttendenceList.Any())
                {
                    await CreateJobPositionAttendenceList(masterDTO.JobPositionAttendenceList);
                }
                if (masterDTO.UserJourneyList != null && masterDTO.UserJourneyList.Any())
                {
                    await CreateUserJourneyList(masterDTO.UserJourneyList);
                }
                if (masterDTO.ExceptionLogList != null && masterDTO.ExceptionLogList.Any())
                {
                    await CreateExceptionLogList(masterDTO.ExceptionLogList);
                }
                if (masterDTO.StoreActivityHistoryList != null && masterDTO.StoreActivityHistoryList.Any())
                {
                    await CreateStoreActivityHistoryList(masterDTO.StoreActivityHistoryList);
                }
                if (masterDTO.AddressList != null && masterDTO.AddressList.Any())
                {
                    await CreateStoreAddressList(masterDTO.AddressList);
                }

            }
            catch
            {
                throw;
            }
            return true;
        }
        private async Task<int> CreateBeatHistoryList(List<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory> beatHistoryList)
        {
            int retVal = -1;
            try
            {
                List<string> uidList = beatHistoryList.Select(bh => bh.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.BeatHistory, uidList);
                List<BeatHistory>? newBeatHistoryLines = null;
                List<BeatHistory>? existingBeatHistoryLines = null;
                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newBeatHistoryLines = beatHistoryList.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                    existingBeatHistoryLines = beatHistoryList.Where(e => existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newBeatHistoryLines = beatHistoryList;
                }
                if (existingBeatHistoryLines != null && existingBeatHistoryLines.Any())
                {
                    var sql = @"UPDATE beat_history 
                                SET 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                start_time = @StartTime, 
                                end_time = @EndTime, 
                                visit_date = @VisitDate, 
                                planned_start_time = @PlannedStartTime, 
                                planned_end_time = @PlannedEndTime, 
                                planned_store_visits = @PlannedStoreVisits, 
                                unplanned_store_visits = @UnPlannedStoreVisits, 
                                zero_sales_store_visits = @ZeroSalesStoreVisits, 
                                msl_store_visits = @MSLStoreVisits, 
                                skipped_store_visits = @SkippedStoreVisits, 
                                actual_store_visits = @ActualStoreVisits, 
                                coverage = @Coverage, 
                                a_coverage = @ACoverage, 
                                t_coverage = @TCoverage, 
                                invoice_status = @InvoiceStatus, 
                                notes = @Notes, 
                                invoice_finalization_date = @InvoiceFinalizationDate, 
                                cfd_time = @CFDTime, 
                                has_audit_completed = @HasAuditCompleted, 
                                ss = @SS,
                                year_month = @YearMonth
                            WHERE 
                                UID = @UID";
                    retVal = await ExecuteNonQueryAsync(sql, existingBeatHistoryLines);
                }
                if (newBeatHistoryLines.Any())
                {
                    string sql = @"INSERT INTO beat_history (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                        user_journey_uid, route_uid, start_time, end_time, job_position_uid, login_id, visit_date, location_uid, planned_start_time, planned_end_time, 
                        planned_store_visits, unplanned_store_visits, zero_sales_store_visits, msl_store_visits, skipped_store_visits, actual_store_visits, coverage, 
                        a_coverage, t_coverage, invoice_status, notes, invoice_finalization_date, route_wh_org_uid, cfd_time, has_audit_completed, wh_stock_audit_uid, 
                        ss, default_job_position_uid, user_journey_vehicle_uid,year_month) 
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                        @UserJourneyUID, @RouteUID, @StartTime, @EndTime, @JobPositionUID, @LoginId, @VisitDate, @LocationUID, 
                        @PlannedStartTime, @PlannedEndTime, @PlannedStoreVisits, @UnPlannedStoreVisits, @ZeroSalesStoreVisits, 
                        @MSLStoreVisits, @SkippedStoreVisits, @ActualStoreVisits, @Coverage, @ACoverage, @TCoverage, 
                        @InvoiceStatus, @Notes, @InvoiceFinalizationDate, @RouteWHOrgUID, @CFDTime, @HasAuditCompleted,
                        @WHStockAuditUID, @ss, @DefaultJobPositionUID, @UserJourneyVehicleUID,@YearMonth);";

                    retVal = await ExecuteNonQueryAsync(sql, newBeatHistoryLines);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateStoreHistoryList(List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory> storeHistoryList)
        {
            int retVal = -1;
            try
            {
                var uidList = storeHistoryList.Select(s => s.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreHistory, uidList);
                List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? existingStoreHistories = null;
                List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? newStoreHistories = null;

                if (existingUIDs != null && existingUIDs.Any())
                {
                    newStoreHistories = storeHistoryList.Where(sh => !existingUIDs.Contains(sh.UID)).ToList();
                    existingStoreHistories = storeHistoryList.Where(sh => existingUIDs.Contains(sh.UID)).ToList();
                }
                else
                {
                    newStoreHistories = storeHistoryList;
                }
                if (newStoreHistories != null && newStoreHistories.Any())
                {
                    var sql = @"INSERT INTO store_history ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            user_journey_uid, year_month, beat_history_uid, org_uid, route_uid, store_uid, is_planned, serial_no, status, visit_duration, travel_time, 
                            planned_login_time, planned_logout_time, login_time, logout_time, no_of_visits, last_visit_date, is_skipped, is_productive, is_green, target_value, 
                            target_volume, target_lines, actual_value, actual_volume, actual_lines, planned_time_spend_in_minutes, latitude, longitude, notes, ss) 
                            VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, now(), @UserJourneyUID, @YearMonth, 
                            @BeatHistoryUID, @OrgUID, @RouteUID, @StoreUID, @IsPlanned, @SerialNo, @Status, @VisitDuration, @TravelTime, @PlannedLoginTime, @PlannedLogoutTime,
                            @LoginTime, @LogoutTime, @NoOfVisits, @LastVisitDate, @IsSkipped, @IsProductive, @IsGreen, @TargetValue, @TargetVolume, @TargetLines, @ActualValue,
                            @ActualVolume, @ActualLines, @PlannedTimeSpendInMinutes, @Latitude, @Longitude, @Notes, 0);";

                    retVal = await ExecuteNonQueryAsync(sql, newStoreHistories);

                }
                if (existingStoreHistories != null && existingStoreHistories.Any())
                {
                    var sql = @"UPDATE store_history SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = now(), 
                               is_planned = @IsPlanned, serial_no = @SerialNo, status = @Status,   visit_duration = @VisitDuration, travel_time = @TravelTime, 
                               planned_login_time = @PlannedLoginTime, planned_logout_time = @PlannedLogoutTime,  login_time = @LoginTime, 
                               logout_time = @LogoutTime, no_of_visits = @NoOfVisits, last_visit_date = @LastVisitDate, is_skipped = @IsSkipped, 
                               is_productive = @IsProductive, is_green = @IsGreen, target_value = @TargetValue, target_volume = @TargetVolume, 
                               target_lines = @TargetLines,    actual_value = @ActualValue, actual_volume = @ActualVolume, actual_lines = @ActualLines, 
                               planned_time_spend_in_minutes = @PlannedTimeSpendInMinutes,
                               latitude = @Latitude, longitude = @Longitude, notes = @Notes, ss = 0 WHERE uid = @UID";

                    retVal = await ExecuteNonQueryAsync(sql, existingStoreHistories);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        
        private async Task<int> CreateStoreHistoryStatsList(List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats> storeHistoryList)
        {
            int retVal = -1;
            try
            {
                var uidList = storeHistoryList.Select(s => s.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreHistoryStats, uidList);
                List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats>? existingStoreHistories = null;
                List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats>? newStoreHistories = null;

                if (existingUIDs != null && existingUIDs.Any())
                {
                    newStoreHistories = storeHistoryList.Where(sh => !existingUIDs.Contains(sh.UID)).ToList();
                    existingStoreHistories = storeHistoryList.Where(sh => existingUIDs.Contains(sh.UID)).ToList();
                }
                else
                {
                    newStoreHistories = storeHistoryList;
                }
                if (newStoreHistories != null && newStoreHistories.Any())
                {
                    var sql = @"INSERT INTO store_history_stats (
                                id, uid, store_history_uid, check_in_time, check_out_time, 
                                total_time_in_min, is_force_check_in, latitude, longitude, 
                                ss, created_by, created_time, modified_by, modified_time, 
                                server_add_time, server_modified_time
                            )
                            VALUES (
                                @Id, @UID, @StoreHistoryUID, @CheckInTime, @CheckOutTime, 
                                @TotalTimeInMin, @IsForceCheckIn, @Latitude, @Longitude, 
                                0, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                                @ServerAddTime, @ServerModifiedTime
                            )";

                    retVal = await ExecuteNonQueryAsync(sql, newStoreHistories);

                }
                if (existingStoreHistories != null && existingStoreHistories.Any())
                {
                    var sql = @"UPDATE store_history_stats
                                SET 
                                    store_history_uid = @StoreHistoryUID,
                                    check_in_time = @CheckInTime,
                                    check_out_time = @CheckOutTime,
                                    total_time_in_min = @TotalTimeInMin,
                                    is_force_check_in = @IsForceCheckIn,
                                    latitude = @Latitude,
                                    longitude = @Longitude,
                                    modified_by = @ModifiedBy,
                                    modified_time = @ModifiedTime,
                                    server_modified_time = now(),
                                    ss = 0
                                    WHERE uid = @UID";

                    retVal = await ExecuteNonQueryAsync(sql, existingStoreHistories);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateJobPositionAttendenceList(List<JobPositionAttendance> jobPositionAttendancesList)
        {
            int retVal = -1;
            try
            {
                var uidList = jobPositionAttendancesList.Select(jpa => jpa.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.JobPositionAttendance, uidList);
                List<JobPositionAttendance>? newJobPositionAttendence = null;
                List<JobPositionAttendance>? existingJobPositionAttendence = null;
                if (existingUIDs != null && !existingUIDs.Any())
                {
                    newJobPositionAttendence = jobPositionAttendancesList.Where(jpa => !existingUIDs.Contains(jpa.UID)).ToList();
                    existingJobPositionAttendence = jobPositionAttendancesList.Where(jpa => existingUIDs.Contains(jpa.UID)).ToList();
                }
                else
                {
                    newJobPositionAttendence = jobPositionAttendancesList;
                }
                if (newJobPositionAttendence != null && !newJobPositionAttendence.Any())
                {
                    string sql = @"INSERT INTO job_position_attendance ( uid, ss, created_by, created_time, modified_by, modified_time, 
                              server_add_time, server_modified_time, org_uid, job_position_uid, emp_uid, year, month, no_of_days, 
                              no_of_holidays, no_of_working_days, days_present, attendance_percentage, last_update_date) VALUES
                              ( @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                              @OrgUID, @JobPositionUID, @EmpUID, @Year, @Month, @NoOfDays, @NoOfHolidays, @NoOfWorkingDays, @DaysPresent, 
                              @AttendancePercentage, @LastUpdateDate)";

                    retVal = await ExecuteNonQueryAsync(sql, newJobPositionAttendence);

                }
                if (existingJobPositionAttendence != null && !existingJobPositionAttendence.Any())
                {

                    var sql = @"UPDATE job_position SET
                modified_by = @ModifiedBy,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                company_uid = @CompanyUID,
                designation = @Designation,
                emp_uid = @EmpUID,
                department = @Department,
                user_role_uid = @UserRoleUID,
                location_mapping_template_uid = @LocationMappingTemplateUID,
                org_uid = @OrgUID,
                seq_code = @SeqCode,
                has_eot = @HasEOT,
                sku_mapping_template_uid = @SKUMappingTemplateUID
            WHERE uid = @UID;";
                    retVal = await ExecuteNonQueryAsync(sql, existingJobPositionAttendence);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateUserJourneyList(List<UserJourney> userJourneyList)
        {
            int retVal = -1;
            try
            {
                var uidList = userJourneyList.Select(uj => uj.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.UserJourney, uidList);
                List<UserJourney>? newUserJourneyList = null;
                List<UserJourney>? existingUserJourneyList = null;

                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newUserJourneyList = userJourneyList.Where(uj => !existingUIDs.Contains(uj.UID)).ToList();
                    existingUserJourneyList = userJourneyList.Where(uj => existingUIDs.Contains(uj.UID)).ToList();

                }
                else
                {
                    newUserJourneyList = userJourneyList;
                }
                if (newUserJourneyList != null && newUserJourneyList.Any())
                {
                    string sql = @"INSERT INTO user_journey ( uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, job_position_uid, emp_uid, journey_start_time, journey_end_time, start_odometer_reading, 
                            end_odometer_reading, journey_time, vehicle_uid, eot_status, reopened_by, has_audit_completed, ss, beat_history_uid, 
                            wh_stock_request_uid, is_synchronizing, has_internet, internet_type, download_speed, upload_speed, has_mobile_network, 
                            is_location_enabled, battery_percentage_target, battery_percentage_available, attendance_status, attendance_latitude, 
                            attendance_longitude, attendance_address)
                            VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @JobPositionUID, @EmpUID, @JourneyStartTime, @JourneyEndTime, @StartOdometerReading, @EndOdometerReading, @JourneyTime,
                            @VehicleUID, @EOTStatus, @ReOpenedBy, @HasAuditCompleted, @SS, @BeatHistoryUID, @WHStockRequestUID, @IsSynchronizing,
                            @HasInternet, @InternetType, @DownloadSpeed, @UploadSpeed, @HasMobileNetwork, @IsLocationEnabled, @BatteryPercentageTarget, 
                            @BatteryPercentageAvailable, @AttendanceStatus, @AttendanceLatitude, @AttendanceLongitude, @AttendanceAddress);";

                    retVal = await ExecuteNonQueryAsync(sql, newUserJourneyList);

                }

                if (existingUserJourneyList != null && existingUserJourneyList.Any())
                {
                    string sql = @"UPDATE user_journey
                                    SET
                                        modified_by = @ModifiedBy,
                                        modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        journey_start_time = @JourneyStartTime,
                                        journey_end_time = @JourneyEndTime,
                                        start_odometer_reading = @StartOdometerReading,
                                        end_odometer_reading = @EndOdometerReading,
                                        journey_time = @JourneyTime,
                                        vehicle_uid = @VehicleUID,
                                        eot_status = @EOTStatus,
                                        reopened_by = @ReOpenedBy,
                                        has_audit_completed = @HasAuditCompleted,
                                        ss = @SS,
                                        is_synchronizing = @IsSynchronizing,
                                        has_internet = @HasInternet,
                                        internet_type = @InternetType,
                                        download_speed = @DownloadSpeed,
                                        upload_speed = @UploadSpeed,
                                        has_mobile_network = @HasMobileNetwork,
                                        is_location_enabled = @IsLocationEnabled,
                                        battery_percentage_target = @BatteryPercentageTarget,
                                        battery_percentage_available = @BatteryPercentageAvailable,
                                        attendance_status = @AttendanceStatus,
                                        attendance_latitude = @AttendanceLatitude,
                                        attendance_longitude = @AttendanceLongitude,
                                        attendance_address = @AttendanceAddress
                                    WHERE
                                        uid = @UID;";
                    retVal = await ExecuteNonQueryAsync(sql, existingUserJourneyList);


                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateExceptionLogList(List<ExceptionLog> exceptionLogList)
        {
            int retVal = -1;
            try
            {
                var uidList = exceptionLogList.Select(j => j.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.ExceptionLog, uidList);
                List<ExceptionLog>? newexceptionLogs = null;
                List<ExceptionLog>? existingexceptionLogs = null;

                if (existingUIDs != null && existingUIDs.Any())
                {
                    newexceptionLogs = exceptionLogList.Where(el => !existingUIDs.Contains(el.UID)).ToList();
                    existingexceptionLogs = exceptionLogList.Where(el => existingUIDs.Contains(el.UID)).ToList();

                }
                else
                {
                    newexceptionLogs = exceptionLogList;
                }
                if (newexceptionLogs != null && newexceptionLogs.Any())
                {
                    string sql = @"INSERT INTO exception_log (uid, store_history_uid, store_history_stats_uid, exception_type, exception_details,
                               created_time, modified_time, ss, created_by, server_add_time, server_modified_time)
                              VALUES (@UID, @StoreHistoryUID, @StoreHistoryStatsUID, @ExceptionType, @ExceptionDetails, @CreatedTime, 
                                @ModifiedTime, @SS, @CreatedBy, @ServerAddTime, @ServerModifiedTime);";

                    retVal = await ExecuteNonQueryAsync(sql, newexceptionLogs);

                }
                if (existingexceptionLogs != null && existingexceptionLogs.Any())
                {
                    string sql = @"UPDATE exception_log
                                    SET
                                        exception_type = @ExceptionType,
                                        exception_details = @ExceptionDetails,
                                        modified_time = @ModifiedTime,
                                        ss = @SS,
                                        server_modified_time = @ServerModifiedTime
                                    WHERE
                                        uid = @UID;";

                    retVal = await ExecuteNonQueryAsync(sql, existingexceptionLogs);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateStoreActivityHistoryList(List<StoreActivityHistory> storeActivityHistoryList)
        {
            int retVal = -1;
            try
            {
                var uidList = storeActivityHistoryList.Select(el => el.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreActivityHistory, uidList);
                List<StoreActivityHistory>? newstoreActivityHistoryList = null;
                List<StoreActivityHistory>? existingstoreActivityHistoryList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    newstoreActivityHistoryList = storeActivityHistoryList.Where(sah => !existingUIDs.Contains(sah.UID)).ToList();
                    existingstoreActivityHistoryList = storeActivityHistoryList.Where(sah => existingUIDs.Contains(sah.UID)).ToList();
                }
                else
                {
                    newstoreActivityHistoryList = storeActivityHistoryList;
                }
                if (newstoreActivityHistoryList != null && newstoreActivityHistoryList.Any())
                {
                    string sql = @"INSERT INTO store_activity_history(
                                        uid, 
                                        store_history_uid, 
                                        store_activity_uid, 
                                        serial_no, 
                                        is_compulsory, 
                                        is_locked, 
                                        status, 
                                        ss, 
                                        created_time, 
                                        modified_time, 
                                        server_add_time, 
                                        server_modified_time)
                                VALUES
                                        (
                                            @UID, 
                                            @StoreHistoryUID, 
                                            @StoreActivityUID, 
                                            @SerialNo, 
                                            @IsCompulsory::bool, 
                                            @IsLocked::bool, 
                                            @Status, 
                                            @SS, 
                                            @CreatedTime, 
                                            @ModifiedTime, 
                                            @ServerAddTime, 
                                            @ServerModifiedTime
                                        );";

                    retVal = await ExecuteNonQueryAsync(sql, newstoreActivityHistoryList);

                }
                if (existingstoreActivityHistoryList != null && existingstoreActivityHistoryList.Any())
                {

                    string sql = @"UPDATE store_activity_history
                                SET
                                    serial_no = @SerialNo,
                                    is_compulsory = @IsCompulsory::bool,
                                    is_locked = @IsLocked::bool,
                                    status = @Status,
                                    ss = @SS,
                                    modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime
                                WHERE
                                    uid = @UID;";
                    retVal = await ExecuteNonQueryAsync(sql, existingstoreActivityHistoryList);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

        private async Task<int> CreateStoreAddressList(List<Winit.Modules.Address.Model.Classes.Address> addressList)
        {
            int retVal = -1;
            try
            {
                var uidList = addressList.Select(a => a.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.Address, uidList);
                List<Winit.Modules.Address.Model.Classes.Address>? newAddressList = null;
                List<Winit.Modules.Address.Model.Classes.Address>? existingAddressList = null;

                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newAddressList = addressList.Where(a => !existingUIDs.Contains(a.UID)).ToList();
                    existingAddressList = addressList.Where(a => existingUIDs.Contains(a.UID)).ToList();
                }
                else
                {
                    newAddressList = addressList;
                }

                if (newAddressList != null && newAddressList.Any())
                {
                    string sql = @"
                INSERT INTO address (
                    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                    type, name, line1, line2, line3, landmark, area, sub_area, zip_code, city, country_code, region_code,
                    phone, phone_extension, mobile1, mobile2, email, fax, latitude, longitude, altitude,
                    linked_item_uid, linked_item_type, status, state_code, territory_code, pan, aadhar, ssn,
                    is_editable, is_default, line4, info, depot, location_uid, custom_field1, custom_field2,
                    custom_field3, custom_field4, custom_field5, custom_field6,
                    branch_uid, sales_office_uid, state, locality, org_unit_uid, ss
                )
                VALUES (
                    @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                    @Type, @Name, @Line1, @Line2, @Line3, @Landmark, @Area, @SubArea, @ZipCode, @City, @CountryCode, @RegionCode,
                    @Phone, @PhoneExtension, @Mobile1, @Mobile2, @Email, @Fax, @Latitude, @Longitude, @Altitude,
                    @LinkedItemUID, @LinkedItemType, @Status, @StateCode, @TerritoryCode, @PAN, @Aadhar, @SSN,
                    @IsEditable, @IsDefault, @Line4, @Info, @Depot, @LocationUID, @CustomField1, @CustomField2,
                    @CustomField3, @CustomField4, @CustomField5, @CustomField6,
                    @BranchUID, @SalesOfficeUID, @State, @Locality, @OrgUnitUID, @SS
                );";

                    retVal = await ExecuteNonQueryAsync(sql, newAddressList);
                }

                if (existingAddressList != null && existingAddressList.Any())
                {
                    string sql = @"
                UPDATE address SET
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    type = @Type,
                    name = @Name,
                    line1 = @Line1,
                    line2 = @Line2,
                    line3 = @Line3,
                    landmark = @Landmark,
                    area = @Area,
                    sub_area = @SubArea,
                    zip_code = @ZipCode,
                    city = @City,
                    country_code = @CountryCode,
                    region_code = @RegionCode,
                    phone = @Phone,
                    phone_extension = @PhoneExtension,
                    mobile1 = @Mobile1,
                    mobile2 = @Mobile2,
                    email = @Email,
                    fax = @Fax,
                    latitude = @Latitude,
                    longitude = @Longitude,
                    altitude = @Altitude,
                    linked_item_uid = @LinkedItemUID,
                    linked_item_type = @LinkedItemType,
                    status = @Status,
                    state_code = @StateCode,
                    territory_code = @TerritoryCode,
                    pan = @PAN,
                    aadhar = @Aadhar,
                    ssn = @SSN,
                    is_editable = @IsEditable,
                    is_default = @IsDefault,
                    line4 = @Line4,
                    info = @Info,
                    depot = @Depot,
                    location_uid = @LocationUID,
                    custom_field1 = @CustomField1,
                    custom_field2 = @CustomField2,
                    custom_field3 = @CustomField3,
                    custom_field4 = @CustomField4,
                    custom_field5 = @CustomField5,
                    custom_field6 = @CustomField6,
                    branch_uid = @BranchUID,
                    sales_office_uid = @SalesOfficeUID,
                    state = @State,
                    locality = @Locality,
                    org_unit_uid = @OrgUnitUID,
                    ss = @SS
                WHERE uid = @UID;";

                    retVal = await ExecuteNonQueryAsync(sql, existingAddressList);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }



        public Task<int> UpdateUserJourney(IUserJourney userJourney)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateBeatHistoryjourneyEndTime(IBeatHistory beatHistory)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
        {
            throw new NotImplementedException();
        }
    }
}
