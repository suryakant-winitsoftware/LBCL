using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Classes
{
    public class MSSQLBeatHistoryDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, Interfaces.IBeatHistoryDL
    {
        public MSSQLBeatHistoryDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                    // SQL Server requires ORDER BY when using OFFSET/FETCH
                    sql.Append(" ORDER BY Id");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                // Use concrete class for proper deserialization
                var type = typeof(Winit.Modules.JourneyPlan.Model.Classes.BeatHistory);
                IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> beatHistoryDetails = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> pagedResponse = new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>
                {
                    PagedData = beatHistoryDetails,
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM beat_history WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID)
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
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
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

                // Add pagination for SQL Server
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                // Get count
                var countSql = @"SELECT COUNT(1) FROM beat_history WHERE route_uid = @RouteUID";
                
                // Use direct Dapper query to avoid the interface deserialization issue
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var beatHistories = await Dapper.SqlMapper.QueryAsync<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>(connection, sql.ToString(), parameters);
                var totalCount = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, countSql, parameters);
                
                return new PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>
                {
                    PagedData = beatHistories,
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
            }
            catch
            {
                throw;
            }
            return true;
        }
        private async Task<int> CreateBeatHistoryList(List<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory> beatHistoryList)
        {
            try
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

                return await ExecuteNonQueryAsync(sql, beatHistoryList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateStoreHistoryList(List<StoreHistory> storeHistoryList)
        {
            try
            {
                var sql = @"INSERT INTO store_history (id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            user_journey_uid, year_month, beat_history_uid, org_uid, route_uid, store_uid, is_planned, serial_no, status, visit_duration, travel_time, 
                            planned_login_time, planned_logout_time, login_time, logout_time, no_of_visits, last_visit_date, is_skipped, is_productive, is_green, target_value, 
                            target_volume, target_lines, actual_value, actual_volume, actual_lines, planned_time_spend_in_minutes, latitude, longitude, notes, ss) 
                            VALUES (@Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @UserJourneyUID, @YearMonth, 
                            @BeatHistoryUID, @OrgUID, @RouteUID, @StoreUID, @IsPlanned, @SerialNo, @Status, @VisitDuration, @TravelTime, @PlannedLoginTime, @PlannedLogoutTime,
                            @LoginTime, @LogoutTime, @NoOfVisits, @LastVisitDate, @IsSkipped, @IsProductive, @IsGreen, @TargetValue, @TargetVolume, @TargetLines, @ActualValue,
                            @ActualVolume, @ActualLines, @PlannedTimeSpendInMinutes, @Latitude, @Longitude, @Notes, 0);";

                return await ExecuteNonQueryAsync(sql, storeHistoryList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateJobPositionAttendenceList(List<JobPositionAttendance> jobPositionAttendancesList)
        {
            try
            {
                string sql = @"INSERT INTO job_position_attendance ( uid, ss, created_by, created_time, modified_by, modified_time, 
                              server_add_time, server_modified_time, org_uid, job_position_uid, emp_uid, year, month, no_of_days, 
                              no_of_holidays, no_of_working_days, days_present, attendance_percentage, last_update_date) VALUES
                              ( @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                              @OrgUID, @JobPositionUID, @EmpUID, @Year, @Month, @NoOfDays, @NoOfHolidays, @NoOfWorkingDays, @DaysPresent, 
                              @AttendancePercentage, @LastUpdateDate)";

                return await ExecuteNonQueryAsync(sql, jobPositionAttendancesList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateUserJourneyList(List<UserJourney> userJourneyList)
        {
            try
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

                return await ExecuteNonQueryAsync(sql, userJourneyList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateExceptionLogList(List<ExceptionLog> exceptionLogList)
        {
            try
            {
                string sql = @"INSERT INTO exception_log (uid, store_history_uid, store_history_stats_uid, exception_type, exception_details,
                               created_time, modified_time, ss, created_by, server_add_time, server_modified_time)
                              VALUES (@Id, @UID, @StoreHistoryUID, @StoreHistoryStatsUID, @ExceptionType, @ExceptionDetails, @CreatedTime, 
                                @ModifiedTime, @SS, @CreatedBy, @ServerAddTime, @ServerModifiedTime);";

                return await ExecuteNonQueryAsync(sql, exceptionLogList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateStoreActivityHistoryList(List<StoreActivityHistory> storeActivityHistoryList)
        {
            try
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
                                            @IsCompulsory, 
                                            @IsLocked, 
                                            @Status, 
                                            @SS, 
                                            @CreatedTime, 
                                            @ModifiedTime, 
                                            @ServerAddTime, 
                                            @ServerModifiedTime
                                        );";

                return await ExecuteNonQueryAsync(sql, storeActivityHistoryList);
            }
            catch (Exception)
            {
                throw;
            }
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
