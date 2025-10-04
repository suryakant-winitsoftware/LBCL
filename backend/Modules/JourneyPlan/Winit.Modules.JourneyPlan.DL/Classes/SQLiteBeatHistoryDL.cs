using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Constants.Stock;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Classes;

public class SQLiteBeatHistoryDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, Winit.Modules.JourneyPlan.DL.Interfaces.IBeatHistoryDL
{
    public readonly IAppUser _appUser;
    public SQLiteBeatHistoryDL(IServiceProvider serviceProvider, IAppUser appUser) : base(serviceProvider)
    {
        _appUser = appUser;
    }
    public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> SelectAllBeatHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"select * from (SELECT
                id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, user_journey_uid AS UserJourneyUID,
                route_uid AS RouteUid, start_time AS StartTime, end_time AS EndTime, job_position_uid AS JobPositionUID,
                login_id AS LoginId, visit_date AS VisitDate, location_uid AS LocationUID, planned_start_time AS PlannedStartTime, planned_end_time AS PlannedEndTime,
                planned_store_visits AS PlannedStoreVisits, unplanned_store_visits AS UnplannedStoreVisits, zero_sales_store_visits AS ZeroSalesStoreVisits, msl_store_visits AS MSLStoreVisits,
                skipped_store_visits AS SkippedStoreVisits, actual_store_visits AS ActualStoreVisits, coverage AS Coverage, acoverage AS ACoverage, tcoverage AS TCoverage,
                invoice_status AS InvoiceStatus, notes AS Notes, invoice_finalization_date AS InvoiceFinalizationDate, route_wh_org_uid AS RouteWHOrgUID,
                cfd_time AS CFDTime, has_audit_completed AS HasAuditCompleted, wh_stock_audit_uid AS WHStockAuditUID, ss AS Ss,
                default_job_position_uid AS DefaultJobPositionUID, user_journey_vehicle_uid AS UserJourneyVehicleUID,year_month as YearMonth
                FROM beat_history) As SubQuery");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM  (SELECT
                id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, user_journey_uid AS UserJourneyUID,
                route_uid AS RouteUID, start_time AS StartTime, end_time AS EndTime, job_position_uid AS JobPositionUID,
                login_id AS LoginId, visit_date AS VisitDate, location_uid AS LocationUID, planned_start_time AS PlannedStartTime, planned_end_time AS PlannedEndTime,
                planned_store_visits AS PlannedStoreVisits, unplanned_store_visits AS UnplannedStoreVisits, zero_sales_store_visits AS ZeroSalesStoreVisits, msl_store_visits AS MSLStoreVisits,
                skipped_store_visits AS SkippedStoreVisits, actual_store_visits AS ActualStoreVisits, coverage AS Coverage, acoverage AS ACoverage, tcoverage AS TCoverage,
                invoice_status AS InvoiceStatus, notes AS Notes, invoice_finalization_date AS InvoiceFinalizationDate, route_wh_org_uid AS RouteWHOrgUID,
                cfd_time AS CFDTime, has_audit_completed AS HasAuditCompleted, wh_stock_audit_uid AS WHStockAuditUID, ss AS SS,
                default_job_position_uid AS DefaultJobPositionUID, user_journey_vehicle_uid AS UserJourneyVehicleUID,year_month as YearMonth
                FROM beat_history) As SubQuery");
            }
            Dictionary<string, object?> parameters = new();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();

            IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> beatHistoryDetails = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql.ToString(), parameters, type);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> pagedResponse = new()
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
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"SELECT
                id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, user_journey_uid AS UserJourneyUID,
                route_uid AS RouteUID, start_time AS StartTime, end_time AS EndTime, job_position_uid AS JobPositionUID,
                login_id AS LoginId, visit_date AS VisitDate, location_uid AS LocationUID, planned_start_time AS PlannedStartTime, planned_end_time AS PlannedEndTime,
                planned_store_visits AS PlannedStoreVisits, unplanned_store_visits AS UnplannedStoreVisits, zero_sales_store_visits AS ZeroSalesStoreVisits, msl_store_visits AS MSLStoreVisits,
                skipped_store_visits AS SkippedStoreVisits, actual_store_visits AS ActualStoreVisits, coverage AS Coverage, acoverage AS ACoverage, tcoverage AS TCoverage,
                invoice_status AS InvoiceStatus, notes AS Notes, invoice_finalization_date AS InvoiceFinalizationDate, route_wh_org_uid AS RouteWHOrgUID,
                cfd_time AS CFDTime, has_audit_completed AS HasAuditCompleted, wh_stock_audit_uid AS WHStockAuditUID, ss AS Ss,
                default_job_position_uid AS DefaultJobPositionUID, user_journey_vehicle_uid AS UserJourneyVehicleUID,year_month as YearMonth
            FROM beat_history WHERE uid = @UID";
        Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();

        Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistoryDetails = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);
        return beatHistoryDetails;
    }
    public async Task<int> CreateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
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
            Dictionary<string, object?> parameters = new()
            {
                    { "@UID", beatHistory.UID },
                    { "@CreatedBy", beatHistory.CreatedBy },
                    { "@CreatedTime", beatHistory.CreatedTime },
                    { "@ModifiedBy", beatHistory.ModifiedBy },
                    { "@ModifiedTime", beatHistory.ModifiedTime },
                    { "@ServerAddTime", beatHistory.ServerAddTime },
                    { "@ServerModifiedTime", beatHistory.ServerModifiedTime },
                    { "@UserJourneyUID", beatHistory.UserJourneyUID },
                    { "@RouteUID", beatHistory.RouteUID },
                    { "@StartTime", beatHistory.StartTime },
                    { "@EndTime", beatHistory.EndTime },
                    { "@JobPositionUID", beatHistory.JobPositionUID },
                    { "@LoginId", beatHistory.LoginId },
                    { "@VisitDate", beatHistory.VisitDate },
                    { "@LocationUID", beatHistory.LocationUID },
                    { "@PlannedStartTime", beatHistory.PlannedStartTime },
                    { "@PlannedEndTime", beatHistory.PlannedEndTime },
                    { "@PlannedStoreVisits", beatHistory.PlannedStoreVisits },
                    { "@UnPlannedStoreVisits", beatHistory.UnPlannedStoreVisits },
                    { "@ZeroSalesStoreVisits", beatHistory.ZeroSalesStoreVisits },
                    { "@MSLStoreVisits", beatHistory.MSLStoreVisits },
                    { "@SkippedStoreVisits", beatHistory.SkippedStoreVisits },
                    { "@ActualStoreVisits", beatHistory.ActualStoreVisits },
                    { "@Coverage", beatHistory.Coverage },
                    { "@ACoverage", beatHistory.ACoverage },
                    { "@TCoverage", beatHistory.TCoverage },
                    { "@InvoiceStatus", beatHistory.InvoiceStatus },
                    { "@Notes", beatHistory.Notes },
                    { "@InvoiceFinalizationDate", beatHistory.InvoiceFinalizationDate },
                    { "@RouteWHOrgUID", beatHistory.RouteWHOrgUID },
                    { "@CFDTime", beatHistory.CFDTime },
                    { "@HasAuditCompleted", beatHistory.HasAuditCompleted },
                    { "@WHStockAuditUID", beatHistory.WHStockAuditUID },
                    { "@ss", 1 },
                    { "@DefaultJobPositionUID", beatHistory.DefaultJobPositionUID },
                    { "@UserJourneyVehicleUID", beatHistory.UserJourneyVehicleUID },
                    { "@YearMonth", beatHistory.YearMonth },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
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
            string sql = @"UPDATE beat_history SET
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
                       tcoverage = @TCoverage,
                       invoice_status = @InvoiceStatus,
                       notes = @Notes,
                       invoice_finalization_date = @InvoiceFinalizationDate,
                       cfd_time = @CFDTime,
                       has_audit_completed = @HasAuditCompleted,
                       ss = @ss,
                       year_month = @YearMonth
                    WHERE uid = @UID;";
            Dictionary<string, object?> parameters = new()
            {
                    { "@UID", beatHistory.UID },
                    { "@ModifiedBy", beatHistory.ModifiedBy },
                    { "@ModifiedTime", beatHistory.ModifiedTime },
                    { "@ServerModifiedTime", beatHistory.ServerModifiedTime },
                    { "@StartTime", beatHistory.StartTime },
                    { "@EndTime", beatHistory.EndTime },
                    { "@VisitDate", beatHistory.VisitDate },
                    { "@PlannedStartTime", beatHistory.PlannedStartTime },
                    { "@PlannedEndTime", beatHistory.PlannedEndTime },
                    { "@PlannedStoreVisits", beatHistory.PlannedStoreVisits },
                    { "@UnPlannedStoreVisits", beatHistory.UnPlannedStoreVisits },
                    { "@ZeroSalesStoreVisits", beatHistory.ZeroSalesStoreVisits },
                    { "@MSLStoreVisits", beatHistory.MSLStoreVisits },
                    { "@SkippedStoreVisits", beatHistory.SkippedStoreVisits },
                    { "@ActualStoreVisits", beatHistory.ActualStoreVisits },
                    { "@Coverage", beatHistory.Coverage },
                    { "@TCoverage", beatHistory.TCoverage },
                    { "@InvoiceStatus", beatHistory.InvoiceStatus },
                    { "@Notes", beatHistory.Notes },
                    { "@InvoiceFinalizationDate", beatHistory.InvoiceFinalizationDate },
                    { "@RouteWHOrgUID", beatHistory.RouteWHOrgUID },
                    { "@CFDTime", beatHistory.CFDTime },
                    { "@HasAuditCompleted", beatHistory.HasAuditCompleted },
                    { "@ss", 2 },
                    { "@YearMonth", beatHistory.YearMonth },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteBeatHistory(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"DELETE  FROM beat_history WHERE UID = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID)
    {
        Dictionary<string, object?> parameters = new()
        {
                {"RouteUID", RouteUID}
            };

        string sql = @"SELECT
                id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, user_journey_uid AS UserJourneyUid,
                route_uid AS RouteUid, start_time AS StartTime, end_time AS EndTime, job_position_uid AS JobPositionUID,
                login_id AS LoginId, visit_date AS VisitDate, location_uid AS LocationUID, planned_start_time AS PlannedStartTime, planned_end_time AS PlannedEndTime,
                planned_store_visits AS PlannedStoreVisits, unplanned_store_visits AS UnplannedStoreVisits, zero_sales_store_visits AS ZeroSalesStoreVisits, msl_store_visits AS MSLStoreVisits,
                skipped_store_visits AS SkippedStoreVisits, actual_store_visits AS ActualStoreVisits, coverage AS Coverage, acoverage AS ACoverage, tcoverage AS TCoverage,
                invoice_status AS InvoiceStatus, notes AS Notes, invoice_finalization_date AS InvoiceFinalizationDate, route_wh_org_uid AS RouteWHOrgUID,
                cfd_time AS CFDTime, has_audit_completed AS HasAuditCompleted, wh_stock_audit_uid AS WHStockAuditUID, ss AS Ss,
                default_job_position_uid AS DefaultJobPositionUid, user_journey_vehicle_uid AS UserJourneyVehicleUID,year_month as YearMonth
            FROM beat_history 
              WHERE route_uid = @RouteUID AND date(visit_date,'localtime') = date('now','localtime')";

        Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();

        Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory selectedBeatHistory = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);

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
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        user_journey_uid AS UserJourneyUID,
                        route_uid AS RouteUID,
                        start_time AS StartTime,
                        end_time AS EndTime,
                        job_position_uid AS JobPositionUID,
                        login_id AS LoginId,
                        visit_date AS VisitDate,
                        location_uid AS LocationUID,
                        planned_start_time AS PlannedStartTime,
                        planned_end_time AS PlannedEndTime,
                        planned_store_visits AS PlannedStoreVisits,
                        unplanned_store_visits AS UnplannedStoreVisits,
                        zero_sales_store_visits AS ZeroSalesStoreVisits,
                        msl_store_visits AS MSLStoreVisits,
                        skipped_store_visits AS SkippedStoreVisits,
                        actual_store_visits AS ActualStoreVisits,
                        coverage AS Coverage,
                        acoverage AS ACoverage,
                        tcoverage AS TCoverage,
                        invoice_status AS InvoiceStatus,
                        notes AS Notes,
                        invoice_finalization_date AS InvoiceFinalizationDate,
                        route_wh_org_uid AS RouteWHOrgUID,
                        cfd_time AS CFDTime,
                        has_audit_completed AS HasAuditCompleted,
                        wh_stock_audit_uid AS WHStockAuditUID,
                        ss AS SS,
                        default_job_position_uid AS DefaultJobPositionUID,
                        user_journey_vehicle_uid AS UserJourneyVehicleUID,
                        year_month AS YearMonth
                    FROM 
                        beat_history 
                    WHERE route_uid = @RouteUID 
                    ORDER BY visit_date DESC";

            Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();
            var beatHistories = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);
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
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        user_journey_uid AS UserJourneyUID,
                        route_uid AS RouteUID,
                        start_time AS StartTime,
                        end_time AS EndTime,
                        job_position_uid AS JobPositionUID,
                        login_id AS LoginId,
                        visit_date AS VisitDate,
                        location_uid AS LocationUID,
                        planned_start_time AS PlannedStartTime,
                        planned_end_time AS PlannedEndTime,
                        planned_store_visits AS PlannedStoreVisits,
                        unplanned_store_visits AS UnplannedStoreVisits,
                        zero_sales_store_visits AS ZeroSalesStoreVisits,
                        msl_store_visits AS MSLStoreVisits,
                        skipped_store_visits AS SkippedStoreVisits,
                        actual_store_visits AS ActualStoreVisits,
                        coverage AS Coverage,
                        acoverage AS ACoverage,
                        tcoverage AS TCoverage,
                        invoice_status AS InvoiceStatus,
                        notes AS Notes,
                        invoice_finalization_date AS InvoiceFinalizationDate,
                        route_wh_org_uid AS RouteWHOrgUID,
                        cfd_time AS CFDTime,
                        has_audit_completed AS HasAuditCompleted,
                        wh_stock_audit_uid AS WHStockAuditUID,
                        ss AS SS,
                        default_job_position_uid AS DefaultJobPositionUID,
                        user_journey_vehicle_uid AS UserJourneyVehicleUID,
                        year_month AS YearMonth
                    FROM 
                        beat_history 
                    WHERE route_uid = @RouteUID 
                    ORDER BY visit_date DESC");

            // Add pagination for SQLite
            if (pageNumber > 0 && pageSize > 0)
            {
                sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            }

            // Get count
            var countSql = @"SELECT COUNT(1) FROM beat_history WHERE route_uid = @RouteUID";
            
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();
            var beatHistories = await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql.ToString(), parameters, type);
            var totalCount = await ExecuteScalarAsync<int>(countSql, parameters);
            
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

    public async Task<IEnumerable<IStoreItemView>> GetCustomersByBeatHistoryUID(string BeatHistoryUID)
    {
        Dictionary<string, object?> parameters = new()
        {
           {"BeatHistoryUID", BeatHistoryUID}
        };
        _ = string.Format(@"select distinct sh.[store_uid]
                 from store_history sh
					inner join sales_order so on so.store_history_uid = sh.[uid] and so.order_type in ('forward') 
                 and sh.[beat_history_uid] = '{0}'
					and so.[status] = 'allocated'
                 ",
                         BeatHistoryUID);
        //after creating bank table change the bankuid to bank
        string query = string.Format($@"SELECT IFNULL(sh.status, '') AS CurrentVisitStatus,
                    c.name AS ContactPerson,
                    c.Phone AS ContactNumber,
                    sh.planned_login_time AS DeliveryTime,
                    s.store_rating AS StoreRating,
                    s.store_class AS StoreClass,
                    s.number AS StoreNumber,
                    s.type AS Type,
                    s.uid AS StoreUID,
                    s.code AS Code,
                    s.name AS Name,
                    s.is_tax_applicable AS IsTaxApplicable,
                   
                    a.uid AS AddressUID,
                    a.latitude AS Latitude,
                    a.longitude AS Longitude,
                    IFNULL(a.line1, '') || CASE WHEN LENGTH(IFNULL(a.line1, '')) > 0 AND LENGTH(IFNULL(a.line2, '')) > 0 
                    THEN ', ' ELSE '' END || IFNULL(a.line2, '') AS Address, A.uid as AddressUID,
                    0 AS StoreDistanceInKM,
                    sh.serial_no AS SerialNo,
                    sh.uid AS StoreHistoryUID,
                    sh.is_planned AS IsPlanned,
                    sh.no_of_visits AS NoOfVisits,
                    sh.is_productive AS IsProductive,
                    sh.is_green AS IsGreen,
                    sh.target_value AS TargetValue,
                    sh.target_volume AS TargetVolume,
                    sh.target_lines AS TargetLines,
                    sh.actual_value AS ActualValue,
                    sh.actual_volume AS ActualVolume,
                    sh.actual_lines AS ActualLines,
                    sh.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes,
                    IFNULL(s.is_blocked, 0) AS IsBlocked,
                    IFNULL(s.blocked_reason_description, '') AS BlockedReasonDescription,
                    0 AS LMAvgSaleValue,
                    0 AS LMNoofInvoices,
                    0 AS LMInvoiceValue,
                    0 AS Last3MonthsAvgSalesValue,
                    '' AS TIN,
                    CASE WHEN s.type = 'dcom' THEN s.type ELSE sai.Order_Type END AS OrderProfile,
                    CASE WHEN ap.storeuid IS NOT NULL THEN 1 ELSE 0 END AS IsAwayPeriod,
                    IFNULL(sai.is_temperature_check, 0) AS IsTemperatureCheck,
                    IFNULL(sai.is_always_printed, 0) AS PrintEmailOption,
                    IFNULL(sai.delivery_docket_is_purchase_order_required, 0) AS DeliveryDocketIsPurchaseOrderRequired,
                    sai.purchase_order_number AS PurchaseOrderNumber,
                    sh.notes AS Notes,
                    sai.drawer AS Drawer,
                    sai.is_with_printed_invoices AS IsWithPrintedInvoices,
                    sai.is_stop_delivery AS IsStopDelivery,
                    sai.bank_uid AS Bank,
                    sai.delivery_information AS AdditionalNotes,
                    sai.is_promotions_block AS IsPromotionsBlock,
                    sai.building_delivery_code AS BuildingDeliveryCode,
                    sai.price_type AS Price,
                    sai.is_dummy_customer AS IsDummyCustomer,
                    sai.mandatory_po_number AS MandatoryPONumber,
                    sai.is_store_credit_capture_signature_required AS IsStoreCreditCaptureSignatureRequired,
                    sai.store_credit_always_printed AS StoreCreditAlwaysPrinted,
                    sai.is_capture_signature_required AS IsCaptureSignatureRequired,
                    s.sold_to_store_uid AS SoldToStoreUID,
                    s.bill_to_store_uid AS BillToStoreUID,
                    IFNULL(sai.stock_credit_is_purchase_order_required, 0) AS StockCreditIsPurchaseOrderRequired,
                    CASE WHEN fo.store_uid IS NOT NULL THEN 1 ELSE 0 END AS HasForwardOrder

                FROM beat_history bh
                INNER JOIN store_history sh ON sh.beat_history_uid = bh.uid AND bh.uid = '{BeatHistoryUID}'
                INNER JOIN store s ON s.uid = sh.store_uid AND s.is_active = 1
                LEFT JOIN store_additional_info sai ON sai.store_uid = s.uid
                LEFT JOIN address a ON a.linked_item_type = 'store' AND a.linked_item_uid = s.uid AND IFNULL(a.is_default, 0) = 1
                LEFT JOIN contact c ON c.linked_item_type = 'store' AND c.linked_item_uid = s.uid AND IFNULL(c.is_default, 0) = 1
                LEFT JOIN (
                    SELECT DISTINCT linked_item_uid AS storeUID
                    FROM away_period ap
                    WHERE ap.linked_item_type = 'store'
                        AND IFNULL(ap.is_active, 0) = 1
                        AND ap.from_date IS NOT NULL
                        AND ap.to_date IS NOT NULL
                        AND date('now') BETWEEN date(from_date) AND date(to_date)
                ) ap ON ap.storeUID = s.uid
                LEFT JOIN (
                    SELECT DISTINCT sh.store_uid
                    FROM store_history sh 
                        INNER JOIN sales_order SO ON SO.store_history_uid = sh.uid AND so.order_type IN ('forward')
                        AND sh.beat_history_uid = '{BeatHistoryUID}'
                        AND so.status = 'allocated'
                
                ) fo ON fo.store_uid = sh.store_uid
                ORDER BY IFNULL(sh.serial_no, 9999), s.name");


        IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView> customersByBeatHistory = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreItemView>(query, parameters);

        return customersByBeatHistory;
    }

    public async Task<IEnumerable<IOrgHeirarchy>?> LoadOrgHeirarchy(IStoreItemView selectedcustomer)
    {
        if (selectedcustomer == null)
        {
            return null;
        }
        Dictionary<string, object?> parameters = new()
        {
                {"OrgUID", selectedcustomer.SelectedStoreCredit.OrgUID}
            };

        string sql = @"
                    SELECT 
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        org_uid AS OrgUID,
                        parent_uid AS ParentUID,
                        parent_level AS ParentLevel
                    FROM 
                        org_heirarchy 
                    WHERE org_uid = @OrgUID 
                    ORDER BY parent_level";

        Type type = _serviceProvider.GetRequiredService<IOrgHeirarchy>().GetType();

        IEnumerable<IOrgHeirarchy> orgHeirarchyList = await ExecuteQueryAsync<IOrgHeirarchy>(sql, parameters, type);

        return orgHeirarchyList;
    }

    public Task<int> UpdateStoreHistoryStatus(string StoreHistoryUID, string Status)
    {
        try
        {

            string sql = @"
                        UPDATE store_history 
                        SET status = @Status , ss = 2,login_time = @LoginTime ,server_modified_time = @ServerModifiedTime
                        WHERE uid  = @UID;";

            Dictionary<string, object?> parameters = new()
            {
                    { "@UID", StoreHistoryUID },
                    { "@Status", Status },
                {"@ServerModifiedTime" ,DateTime.Now},
                {"@LoginTime" ,DateTime.Now}
             };

            return ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
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

    private async Task<int> UpdateStoreHistoryCheckoutTime(DateTime checkoutTime, string storeHistoryUID)
    {
        try
        {
            string query = @"UPDATE store_history SET logout_time = @LogoutTime, modified_time = @ModifiedTime,
                                ss = 2 ,server_modified_time = @ModifiedTime  WHERE uid = @UID";

            Dictionary<string, object?> parameters = new()
            {
                //{ "@LogoutTime", checkoutTime.TimeOfDay },
                { "@LogoutTime", checkoutTime },
                { "@ServerModifiedTime", checkoutTime },
                { "@ModifiedTime", checkoutTime },
                { "@UID", storeHistoryUID }
            };

            return await ExecuteNonQueryAsync(query, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating store history checkout time.", ex);
        }
    }

    public async Task<int> OnCheckout(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
    {
        try
        {
            DateTime checkoutTime = DateTime.Now;
            int rowsAffected = await UpdateStoreHistoryCheckoutTime(checkoutTime, storeItemView.StoreHistoryUID);

            string query = @"UPDATE store_history_stats SET check_out_time = @CheckOutTime, 
                     total_time_in_min = Cast ((JulianDay(@CheckOutTime) - JulianDay(@CheckInTime)) * 24 * 60 As Integer),
                     ss = 2, modified_time = @ModifiedTime
                  WHERE uid = @UID";

            Dictionary<string, object?> parameters = new()
            {   { "@CheckInTime", storeItemView.CheckInTime },
                { "@CheckOutTime", checkoutTime },
                { "@ModifiedTime", DateTime.Now },
                { "@UID", storeItemView.StoreHistoryStatsUID },
                { "@SS", 2 }
            };

            int retValue = await ExecuteNonQueryAsync(query, parameters);
            if (retValue > 0)
            {
                if (!string.IsNullOrEmpty(storeItemView.ExceptionType) && storeItemView.ExceptionType.Equals(Winit.Shared.Models.Constants.StoryHistoryStatus.ZERO_SALES))
                {
                    _ = await CreateExceptionLog(storeItemView);
                }
            }
            return retValue;
        }
        catch (Exception ex)
        {
            throw new Exception("Error on checkout.", ex);
        }
    }

    public async Task<int> CreateStoreHistoryStats(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView,
                                          int totalTimeInMin, bool isForceCheckIn, string UID, string empUID)
    {
        int id = 0;
        try
        {
            string sql = @"INSERT INTO store_history_stats (id,uid, store_history_uid, check_in_time, check_out_time, total_time_in_min, 
            is_force_check_in, latitude, longitude, ss,created_by, created_time, modified_by,modified_time, server_add_time, server_modified_time) 
            VALUES (@Id, @UID, @StoreHistoryUID, @CheckInTime, @CheckOutTime, @TotalTimeInMin, @IsForceCheckIn, 
            @Latitude, @Longitude, @SS,@CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

            Dictionary<string, object?> parameters = new()
            {
                { "@Id", id },
                { "@UID", UID},
                { "@StoreHistoryUID", storeItemView.StoreHistoryUID },
                { "@CheckInTime", storeItemView.CheckInTime },
                { "@CheckOutTime", storeItemView.CheckInTime  },
                { "@TotalTimeInMin", totalTimeInMin },
                { "@IsForceCheckIn", isForceCheckIn },
                { "@Latitude", storeItemView.Latitude },
                { "@Longitude", storeItemView.Longitude },
                { "@SS", 1},
                { "@CreatedBy",empUID },
                { "@CreatedTime", storeItemView.CreatedTime },
                { "@ModifiedBy", empUID },
                { "@ModifiedTime", storeItemView.ModifiedTime },
                { "@ServerAddTime", storeItemView.ServerAddTime },
                { "@ServerModifiedTime", storeItemView.ServerModifiedTime }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreateExceptionLog(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
    {
        string GUID = Guid.NewGuid().ToString();
        int id = 0;
        try
        {
            string sql = @"INSERT INTO Exception_log (id,uid, store_history_uid, store_history_stats_uid, exception_type, 
                    exception_details, created_time, modified_time, ss, created_by, server_add_time, server_modified_time) 
                    VALUES (@Id,@UID, @StoreHistoryUID, @StoreHistoryStatsUID, @ExceptionType, @ExceptionDetails, 
                    @CreatedTime, @ModifiedTime, @SS, @CreatedBy, @ServerAddTime, @ServerModifiedTime);";

            Dictionary<string, object?> parameters = new()
            {
                { "@Id", id },
                { "@UID", GUID},
                { "@StoreHistoryUID", storeItemView.StoreHistoryUID },
                { "@StoreHistoryStatsUID", storeItemView.StoreHistoryStatsUID },
                { "@ExceptionType", storeItemView.ExceptionType },
                { "@ExceptionDetails", storeItemView.ExceptionReason},
                { "@CreatedTime", storeItemView.CreatedTime },
                { "@ModifiedTime", storeItemView.ModifiedTime },
                { "@SS", 1},
                { "@CreatedBy", storeItemView.CreatedBy },
                { "@ServerAddTime",storeItemView.ServerAddTime },
                { "@ServerModifiedTime", storeItemView.ServerModifiedTime }
             };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatSummaryFromStoreHistory(string BeatHistoryUID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  BeatHistoryUID}
        };
        string sql = @"SELECT 
                SUM(CASE WHEN  is_planned = 1 THEN 1 ELSE 0 END) AS PlannedStoreVisitsTarget,
                SUM(CASE WHEN  is_planned = 0 THEN 1 ELSE 0 END) AS UnPlannedStoreVisitsTarget,
                SUM(CASE WHEN  [status] = 'Visited' AND is_planned = 1 THEN 1 ELSE 0 END) AS PlannedStoreVisitsActual,
                SUM(CASE WHEN  [status] = 'Visited' AND is_planned = 0 THEN 1 ELSE 0 END) AS UnPlannedStoreVisitsActual,
                SUM(CASE WHEN [status] = 'Visited' AND IFNULL(actual_value,0) = 0 THEN 1 ELSE 0 END) AS ZeroSalesStoreVisitsActual,
                0 AS MSLStoreVisitsActual,
                SUM(CASE WHEN is_skipped = 1 THEN 1 ELSE 0 END) AS SkippedStoreVisitsActual
                FROM store_history
                WHERE beat_history_uid = @UID";

        Type type = _serviceProvider.GetRequiredService<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>().GetType();
        Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistoryDetails = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(sql, parameters, type);
        return beatHistoryDetails;
    }

    public async Task<int> UpdateBeathistory_Checkout(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistoryTarget, string BeatHistoryUID)
    {
        //int beatHistoryTarget = await GetBeatSummary();
        decimal PlannedStoreVisitsTarget = beatHistoryTarget.PlannedStoreVisits;
        decimal UnPlannedStoreVisitsTarget = beatHistoryTarget.UnPlannedStoreVisits;
        int actualStoreVisits = beatHistoryTarget.PlannedStoreVisits + beatHistoryTarget.UnPlannedStoreVisits;
        decimal coverage = PlannedStoreVisitsTarget == 0 ? 0 : beatHistoryTarget.PlannedStoreVisits / PlannedStoreVisitsTarget * 100;
        decimal aCoverage = (PlannedStoreVisitsTarget + UnPlannedStoreVisitsTarget) == 0 ? 0 : actualStoreVisits / (PlannedStoreVisitsTarget + UnPlannedStoreVisitsTarget) * 100;

        string query = string.Format(@"UPDATE beat_history SET 
                planned_store_visits = @PlannedStoreVisits, 
                unplanned_store_visits = @UnPlannedStoreVisits,
                zero_sales_store_visits = @ZeroSalesStoreVisits,
                msl_store_visits = @MSLStoreVisits, 
                skipped_store_visits = @SkippedStoreVisits, 
                actual_store_visits = @ActualStoreVisits,
                coverage = @Coverage, 
                a_coverage = @ACoverage, 
                t_coverage = @TCoverage,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime,
                ss = @SS 
            WHERE 
                uid = @UID");

        Dictionary<string, object?> parameters = new()
        {
        { "@PlannedStoreVisits", beatHistoryTarget.PlannedStoreVisits },
        { "@UnPlannedStoreVisits", beatHistoryTarget.UnPlannedStoreVisits },
        { "@ZeroSalesStoreVisits", beatHistoryTarget.ZeroSalesStoreVisits },
        { "@MSLStoreVisits", beatHistoryTarget.MSLStoreVisits },
        { "@SkippedStoreVisits", beatHistoryTarget.SkippedStoreVisits },
        { "@ActualStoreVisits", actualStoreVisits },
        { "@Coverage", coverage },
        { "@ACoverage", aCoverage },
        { "@TCoverage", coverage },
        { "@ModifiedTime", DateTime.Now },
        { "@SS", 2 },
        { "@ServerModifiedTime", DateTime.Now  },
        { "@UID", BeatHistoryUID }
        };

        return await ExecuteNonQueryAsync(query, parameters);
    }

    // this for myCustomer page future it will change to another module
    public async Task<int> CheckCustomerExistsInJP(string StoreUID, string BeatHistoryUID)
    {
        string query = @"SELECT COUNT(*) FROM store_history WHERE beat_history_uid = @BeatHistoryUID AND store_uid = @StoreUID";
        Dictionary<string, object?> parameters = new()
        {
        { "@BeatHistoryUID", BeatHistoryUID },
        { "@StoreUID", StoreUID }
    };
        try
        {
            int totalCount = await ExecuteScalarAsync<int>(query, parameters);
            return totalCount;
        }
        catch (Exception ex)
        {
            throw new Exception("Error checking customer existence in journey plan.", ex);
        }
    }

    public async Task<int> AddCustomerInJP(string UID, string StoreUID, string BeatHistoryUID)
    {
        DateTime CreatedTime = DateTime.Now;
        DateTime ModifiedTime = DateTime.Now;
        string CreatedBy = _appUser?.Emp?.UID ?? "";
        string ModifiedBy = _appUser?.Emp?.UID ?? "";
        DateTime now = DateTime.Now;
        int yearLastTwoDigits = now.Year % 100;
        int month = now.Month;
        int yearMonth = yearLastTwoDigits * 100 + month;
        string RouteUID = _appUser?.SelectedRoute?.UID ?? "";
        string OrgUID = _appUser?.SelectedJobPosition?.OrgUID ?? "";
        string UserjourneyUID = _appUser?.UserJourney?.UID ?? string.Empty;
        string query = @"INSERT INTO store_history (id, uid, beat_history_uid, store_uid, is_planned, serial_no,status ,ss,year_month, created_time,
                modified_time,created_by,modified_by,org_uid ,route_uid,user_journey_uid)
                     VALUES (0, @UID, @BeatHistoryUID, @StoreUID, @IsPlanned, '999','UnPlanned',1 , @YearMonth,@CreatedTime, @ModifiedTime,@CreatedBy,@ModifiedBy,@OrgUID,@RouteUID,@UserjourneyUID)";
        Dictionary<string, object?> parameters = new()
        {
        { "@UID", UID },
        { "@BeatHistoryUID", BeatHistoryUID },
        { "@StoreUID", StoreUID },
        { "@IsPlanned", false },
        { "@SerialNo", 9999 },
        { "@SS", 1 },
        { "@YearMonth", yearMonth },
        { "@status","UnPlanned" },
        { "@CreatedTime", CreatedTime },
        { "@ModifiedTime", ModifiedTime},
        { "@CreatedBy", CreatedBy},
        { "@ModifiedBy", ModifiedBy},
        { "@RouteUID", RouteUID},
        { "@OrgUID", OrgUID},
        { "@UserjourneyUID", UserjourneyUID}

        };

        try
        {
            return await ExecuteNonQueryAsync(query, parameters);

            // _viewmodel.SelectedCustomer.StoreHistoryUID = GUID;
            // _viewmodel.SelectedCustomer.IsAddedInJP = true;

        }
        catch (Exception ex)
        {
            throw new Exception("Error adding customer to journey plan.", ex);
        }
    }

    public async Task<IEnumerable<IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime)
    {
        string query = @"select distinct ifnull(sh.status,'') as CurrentVisitStatus, 
                     c.name as ContactPerson, c.Phone as ContactNumber, sh.planned_login_time as DeliveryTime, s.store_rating As StoreRating, 
                     s.store_class As StoreClass, s.number as StoreNumber, s.type as Type, s.[uid] as StoreUID, s.code as storecode, s.name as Name, 
                     s.is_tax_applicable as IsTaxApplicable,
                     a.latitude as Latitude, a.longitude as Longitude,  
                     ifnull(a.line1, '') || case when length(ifnull(a.line1, '')) > 0 and length(ifnull(a.line2, '')) > 0 then ', ' else '' end || ifnull(a.line2, '') as Address, 
                     0 as StoreDistanceInKM, sh.serial_no as SerialNo, sh.[uid] as StoreHistoryUID, sh.is_planned As IsPlanned, sh.no_of_visits As NoOfVisits, sh.is_productive As IsProductive, 
                     sh.is_green As IsGreen, sh.target_value As TargetValue, sh.target_volume As TargetVolume, sh.target_lines As TargetLines, sh.actual_value As ActualValue, 
                     sh.actual_volume As ActualVolume, sh.actual_lines As ActualLines, 
                     sh.planned_time_spend_in_minutes As PlannedTimeSpendInMinutes, ifnull(s.is_blocked, 0) as IsBlocked, ifnull(s.blocked_reason_description,'') as blockedreasondescription,
                     0 LMAvgSaleValue, 0 LMNoOfInvoices, 0 LMInvoiceValue, 0 Last3MonthsAvgSalesValue, 
                     '' as Tin, case when s.[type] = 'dcom' then s.type else sai.order_type end as OrderProfile, 
                     case when ap.storeuid is not null then 1 else 0 end as IsAwayPeriod,
                     ifnull(sai.is_temperature_check, 0) as IsTemperatureCheck, ifnull(sai.is_always_printed, 0) as PrintEmailOption,
                     sai.purchase_order_number As PurchaseOrderNumber, sh.notes AS Notes,
                     sai.drawer As Drawer, sai.is_with_printed_invoices As IsWithPrintedInvoices, sai.is_stop_delivery AS IsStopDelivery, sai.bank_uid As Bank, sai.delivery_information as additionalnotes, 
                     sai.is_promotions_block As IsPromotionsBlock, sai.building_delivery_code As BuildingDelivery, sai.price_type AS Price, sai.is_dummy_customer As IsDummyCustomer, sai.mandatory_po_number As MandatoryPoNumber, 
                     sai.is_store_credit_capture_signature_required As IsStoreCreditCaptureSignatureRequired, sai.store_credit_always_printed AS StoreCreditAlwaysPrinted, sai.is_capture_signature_required AS IsCaptureSignatureRequired, 
                     s.sold_to_store_uid as SoldToStoreUID, s.bill_to_store_uid As BillToStoreUID, ifnull(sai.stock_credit_is_purchase_order_required, 0) as StockCreditIsPurchaseOrderRequired, 
                     case when fo.store_uid is not null then 1 else 0 end as HasForwardOrder
                     from beat_history bh
                     inner join store_history sh on sh.beat_history_uid = bh.[uid] and Date(BH.visit_date,'localtime') = Date(@VisitDate,'localtime')
                     inner join store s on s.[uid] = sh.store_uid and s.is_active = 1 
                     inner join store_additional_info sai on sai.store_uid = s.[uid] 
                     --INNER JOIN Bank B ON B.[UID] = SAI.bank_uid
                    -- left join stor_erolling_stats srs on s.[uid] = srs.store_uid                                   
                     left join [address] a on a.linked_item_type = 'store' and a.linked_item_uid = s.[uid] and ifnull(a.is_default, 0) = 1
                     left join contact c on c.linked_item_type = 'store' and c.linked_item_uid = s.[uid] and ifnull(c.is_default, 0) = 1
                     left join (
                         select distinct linked_item_uid as storeuid from away_period ap 
                         where ap.linked_item_type = 'store' 
                         and ifnull(ap.is_active, 0) = 1 and ap.from_date is not null and ap.to_date is not null 
                         and date('now') between date(from_date) and date(to_date)
                     ) ap on ap.storeuid = s.[uid]   
                     left join (select distinct sh.[store_uid]
                 from store_history sh
                INNER JOIN sales_order SO ON SO.store_history_uid = sh.uid AND so.order_type IN ('forward')
                 AND so.status = 'allocated'
                 ) fo on fo.store_uid = sh.store_uid
                     order by ifnull(sh.serial_no, 9999), s.name";

        Dictionary<string, object?> parameters = new()
        {
         { "@VisitDate", dateTime }
        };

        Type type = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreItemView>().GetType();

        IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView> CustomersForSelectedDate = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreItemView>(query, parameters, type);

        return CustomersForSelectedDate;
    }

    // this for CFD Functionality later move to CFD Module 
    public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource>> GetPendingPushData()
    {
        List<IStandardListSource> standardListSourcesList = new();
        string query = @"SELECT DISTINCT table_name FROM table_group_entity WHERE has_upload = 1 
                     AND table_name NOT IN ('sales_order_line', 'sales_order_tax', 'sales_order_discount', 
                     'return_order_line', 'return_order_tax', 'return_order_discount',
                     'exchange_order_line')";
        try
        {
            DataSet ds = await ExecuteQueryDataSetAsync(new string[] { query });
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string tableName = row["table_name"].ToString();
                    IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource> pendingDataList = await GetPendingPushDataForTable(tableName);
                    if (pendingDataList != null && pendingDataList.Any())
                    {
                        standardListSourcesList.AddRange(pendingDataList);
                    }
                }
            }
        }
        catch (Exception)
        {
            // Handle exception, log, or ignore
        }
        return standardListSourcesList;
    }

    public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource>> GetPendingPushDataForTable(string tableName)
    {
        List<IStandardListSource> standardListSourcesList = new();
        string query = string.Format(@"SELECT '{0}' AS TableName, COUNT(1) AS NoOfRecords FROM {0} WHERE SS IN (-1,1,2)", tableName);
        try
        {
            DataSet ds = await ExecuteQueryDataSetAsync(new string[] { query });
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string sourceType = row["TableName"].ToString();
                    int noOfRecords = Convert.ToInt32(row["NoOfRecords"]);
                    if (noOfRecords > 0)
                    {
                        standardListSourcesList.Add(new Winit.Modules.Store.Model.Classes.StandardListSource
                        {
                            SourceType = sourceType,
                            SourceLabel = noOfRecords.ToString()
                        });
                    }
                }
            }
        }
        catch (Exception)
        {
            // Handle exception, log, or ignore
        }
        return standardListSourcesList;
    }

    public async Task<int> GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid)
    {

        int NotVisitedCount = 0;
        try
        {
            string query = @"SELECT COUNT(DISTINCT SH.store_uid) AS NotVisitedCount FROM beat_history BH
                        INNER JOIN store_history SH ON SH.beat_history_uid = BH.UID AND SH.is_planned = 1 
                        AND IFNULL(status,'') = 'Visited' AND BH.visit_date = @JourneyStartTime AND BH.route_uid = @RouteUid";

            DateTime journeyStartDate = new DateTime(JourneyStartTime.Year, JourneyStartTime.Month, JourneyStartTime.Day, 0, 0, 0);

            IDictionary<string, object?>[] parameters = new IDictionary<string, object?>[]
                 {
        new Dictionary<string, object?>
        {
            { "@JourneyStartTime", journeyStartDate },
            { "@RouteUid", RouteUid }
        }
                 };

            DataSet ds = await ExecuteQueryDataSetAsync(new string[] { query }, parameters);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                object countValue = ds.Tables[0].Rows[0]["NotVisitedCount"];
                NotVisitedCount = Convert.ToInt32(countValue);
            }
        }
        catch (Exception)
        {
            // Handle exception, log, or throw
            throw;
        }

        return NotVisitedCount;
    }

    public bool IsStockAuditCompleted(string UserJourneyUID)
    {
        bool isStockAudited = false;
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                { "@UserJourneyUID", UserJourneyUID }
            };

            //var wHStockAudit = await GetItemAsync<Shared.sFAModel.Domain.StockAudit.WHStockAudit>(query, parameters);
            //if (wHStockAudit != null)
            //{
            //    isStockAudited = true;
            //}
        }
        catch (Exception ex)
        {
            throw new Exception("Error checking stock audit completion.", ex);
        }
        return isStockAudited;
    }

    public async Task<int> CloseBeatHistoryCFD(DateTime? journeyEndTime, string userJourneyUID)
    {
        string query = @"UPDATE beat_history SET cfd_time = @JourneyEndTime, ss = 2 WHERE user_journey_uid = @UserJourneyUID";
        Dictionary<string, object?> parameters = new()
        {
            { "@JourneyEndTime", journeyEndTime },
            { "@UserJourneyUID", userJourneyUID }
        };
        int retValue;
        try
        {
            retValue = await ExecuteNonQueryAsync(query, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error closing beat history CFD.", ex);
        }
        return retValue;
    }

    public async Task<int> InsertUserJourney(IUserJourney userJourney)
    {
        DateTime CreatedTime = DateTime.Now;
        DateTime ModifiedTime = DateTime.Now;
        string sql = @"INSERT INTO user_journey (id,
                        uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, job_position_uid, emp_uid,
                        journey_start_time, journey_end_time, start_odometer_reading, end_odometer_reading,
                        Journey_Time, Vehicle_UID, EOT_Status, ReOpened_By,
                        has_audit_completed, ss, beat_history_uid, wh_stock_request_uid,
                        is_synchronizing, has_internet, internet_type, download_speed,
                        upload_speed, has_mobile_network, is_location_enabled,
                        battery_percentage_target, battery_percentage_available,
                        attendance_status, attendance_latitude, attendance_longitude
                        ) VALUES (0,
                            @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime, @JobPositionUID, @EmpUID,
                            @JourneyStartTime, @JourneyEndTime, @StartOdometerReading, @EndOdometerReading,
                            @JourneyTime, @VehicleUID, @EOTStatus, @ReOpenedBy,
                            @HasAuditCompleted, @SS, @BeatHistoryUID, @WHStockRequestUID,
                            @IsSynchronizing, @HasInternet, @InternetType, @DownloadSpeed,
                            @UploadSpeed, @HasMobileNetwork, @IsLocationEnabled,
                            @BatteryPercentageTarget, @BatteryPercentageAvailable,
                            @AttendanceStatus, @AttendanceLatitude, @AttendanceLongitude
                        )";

        Dictionary<string, object?> parameters = new()
        {
            { "@UID", userJourney.UID },
            { "@CreatedBy", userJourney.CreatedBy },
            { "@CreatedTime", CreatedTime },
            { "@ModifiedBy", userJourney.ModifiedBy },
            { "@ModifiedTime", ModifiedTime },
            { "@ServerAddTime", ModifiedTime },
            { "@ServerModifiedTime", ModifiedTime },
            { "@JobPositionUID", userJourney.JobPositionUID },
            { "@EmpUID", userJourney.EmpUID },
            { "@JourneyStartTime", userJourney.JourneyStartTime },
            { "@JourneyEndTime", userJourney.JourneyEndTime },
            { "@StartOdometerReading", userJourney.StartOdometerReading },
            { "@EndOdometerReading", userJourney.EndOdometerReading },
            { "@JourneyTime", userJourney.JourneyTime },
            { "@VehicleUID", userJourney.VehicleUID },
            { "@EOTStatus", userJourney.EOTStatus },
            { "@ReOpenedBy", userJourney.ReOpenedBy },
            { "@HasAuditCompleted", userJourney.HasAuditCompleted ? 1 : 0 },
            { "@SS", userJourney.SS },
            { "@BeatHistoryUID", userJourney.BeatHistoryUID },
            { "@WHStockRequestUID", userJourney.WHStockRequestUID },
            { "@IsSynchronizing", userJourney.IsSynchronizing ? 1 : 0 },
            { "@HasInternet", userJourney.HasInternet ? 1 : 0 },
            { "@InternetType", userJourney.InternetType },
            { "@DownloadSpeed", userJourney.DownloadSpeed },
            { "@UploadSpeed", userJourney.UploadSpeed },
            { "@HasMobileNetwork", userJourney.HasMobileNetwork ? 1 : 0 },
            { "@IsLocationEnabled", userJourney.IsLocationEnabled ? 1 : 0 },
            { "@BatteryPercentageTarget", userJourney.BatteryPercentageTarget },
            { "@BatteryPercentageAvailable", userJourney.BatteryPercentageAvailable },
            { "@AttendanceStatus", userJourney.AttendanceStatus },
            { "@AttendanceLatitude", userJourney.AttendanceLatitude },
            { "@AttendanceLongitude", userJourney.AttendanceLongitude }
        };

        try
        {
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error inserting user journey.", ex);
        }
    }
    public async Task<List<IJPBeatHistory>> GetActiveOrTodayBeatHistory()
    {
        try
        {
            if (_appUser.SelectedRole == null || _appUser.SelectedRole.JobPositionUID == null)
            {
                throw new NullReferenceException("SelectedRole or JobPositionUID is null in _appUser.");
            }
            //string plannedBeatQuery = string.Format(@"SELECT DISTINCT BH.uid AS BeatHistoryUID 
            //                        FROM beat_history BH
            //                        INNER JOIN route J ON J.[uid] = BH.route_uid 
            //                       -- INNER JOIN vehicle V ON V.[uid] = J.vehicle_uid AND V.uid = '{0}'
            //                        AND Date(BH.visit_date,'localtime') = Date('{1}','localtime')
            //                        INNER JOIN store_history SH ON SH.beat_history_uid = BH.[uid] AND SH.is_planned = 1
            //                        INNER JOIN store S ON S.[UID] = SH.store_uid AND S.number != '99999'
            //                        ",
            //                        //_appUser.Vehicle.UID,
            //                        CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date));
            string plannedBeatQuery = string.Format(@"SELECT DISTINCT BH.uid AS BeatHistoryUID 
                                    FROM beat_history BH
                                    INNER JOIN route J ON J.[uid] = BH.route_uid 
                                    AND Date(BH.visit_date,'localtime') = Date('{0}','localtime')
                                    INNER JOIN store_history SH ON SH.beat_history_uid = BH.[uid] AND SH.is_planned = 1
                                    INNER JOIN store S ON S.[UID] = SH.store_uid AND S.number != '99999'
                                    ",
                                   //_appUser.Vehicle.UID,
                                   CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date));

            string PendingMoveOrderCount = string.Format(@"SELECT DISTINCT route_uid As RouteUID, COUNT(1)  AS PendingLoadRequestCount
                                    FROM wh_Stock_request 
                                    WHERE request_type IN ('Load','DCom','FOC') 
                                    AND Date(required_by_date,'localtime') = Date('{1}','localtime') AND Status IN ('{2}','{3}') 
                                    GROUP BY route_uid",
                                        _appUser.SelectedJobPosition.UID,
                                        CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date),
                                        StockTransferStatus.APPROVED,
                                        StockTransferStatus.REQUESTED);

            string query = string.Format(@"SELECT 
                                        BH.uid AS UID, 
                                        BH.user_journey_uid AS UserJourneyUID, 
                                        J.uid AS BeatUID, 
                                        J.name AS BeatName, 
                                        J.uid AS RouteUID, 
                                        J.name AS RouteName, 
                                        BH.start_time AS StartTime, 
                                        BH.end_time AS EndTime, 
                                        BH.visit_date AS VisitDate, 
                                        BH.job_position_uid AS JobPositionUID, 
                                        BH.login_id AS LoginId, 
                                        BH.notes AS Notes,
                                        ifnull(BH.planned_store_visits, 0) AS PlannedStoreVisits, 
                                        ifnull(BH.unplanned_store_visits, 0) AS UnPlannedStoreVisits,
                                        ifnull(BH.zero_sales_store_visits, 0) AS ZeroSalesStoreVisits, 
                                        ifnull(BH.msl_store_visits, 0) AS MSLStoreVisits, 
                                        ifnull(BH.skipped_store_visits, 0) AS SkippedStoreVisits,
                                        ifnull(BH.actual_store_visits, 0) AS ActualStoreVisits, 
                                        ifnull(BH.coverage, 0) AS Coverage, 
                                        ifnull(BH.a_coverage, 0) AS ACoverage,
                                        ifnull(BH.t_coverage, 0) AS TCoverage,
                                        null AS last_visit_date, 
                                        null AS next_visit_date, 
                                        'NotCompleted' AS ExecutionStatus, 
                                        BH.has_audit_completed AS hasAuditComplete, 
                                        BH.wh_stock_audit_uid AS WHStockAuditUID, 
                                        BH.default_job_position_uid AS DefaultJobPositionUid, 
                                        PLR.PendingLoadRequestCount AS PendingLoadRequestCount
                                    FROM 
                                        beat_history BH
                                        INNER JOIN Route J ON J.[uid] = BH.route_uid 
                                      --  INNER JOIN vehicle V ON V.[UID] = J.vehicle_uid AND V.[uid] = '{1}'
                                        AND (
                                            ( DATE(BH.visit_date,'localtime') = DATE('now','localtime') AND BH.end_time IS NULL) 
                                            OR ( DATE(BH.visit_date) <  DATE('now','localtime') AND start_time IS NOT NULL AND BH.end_time IS NULL) 
                                        )
                                        AND Date(BH.visit_date,'localtime') = Date('{1}','localtime') 
                                        AND BH.[uid] IN (
                                            {2}
                                        ) 
                                        LEFT JOIN (
                                        {3}
                                        ) PLR ON PLR.routeuid = BH.route_uid
                                        ORDER BY visit_date DESC",
                                        _appUser.SelectedRole.JobPositionUID,
                                            // _appUser.Vehicle.UID,
                                            CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date),
                                            plannedBeatQuery, PendingMoveOrderCount);
            return await ExecuteQueryAsync<IJPBeatHistory>(query);
        }
        catch (Exception) { throw; }
    }

    public async Task<int> StartBeatHistory(IJPBeatHistory beatHistory)
    {
        int returnValue;
        try
        {
            DateTime StartTime = DateTime.Now;
            string query = string.Format(@"UPDATE beat_history SET user_journey_uid = '{0}', start_time = '{1}', 
                                job_position_uid = '{3}', login_id = '{4}', user_journey_vehicle_uid = '{5}', ss = 2 WHERE [uid] = '{2}'",
                               _appUser.UserJourney.UID,
                               CommonFunctions.GetDateTimeInFormatForSqlite(DateTime.Now),
                               beatHistory.UID,
                               _appUser.SelectedJobPosition.UID,
                               _appUser.Emp.LoginId,
                               _appUser.UserJourneyVehicleUID
                               );
            returnValue = await ExecuteNonQueryAsync(query);
            if (returnValue > 0)
            {
                beatHistory.StartTime = StartTime;
                beatHistory.JobPositionUID = _appUser.SelectedJobPosition.UID;
                beatHistory.LoginId = _appUser.Emp.LoginId;
                beatHistory.UserJourneyVehicleUID = _appUser.UserJourneyVehicleUID;
            }
        }
        catch (Exception)
        {
            throw;
            //DBCommonFunctions.LogErrorToDB(ex.Message, "CheckLoginOffline", "StoreAttributeDao", "UserName : " + UserName, "Password" + Password);
        }
        return returnValue;
    }
    public async Task<int> OpenBeatHistory(IJPBeatHistory beatHistory)
    {
        int returnValue;
        try
        {
            DateTime StartTime = DateTime.Now;
            string query = string.Format(@"UPDATE beat_history SET user_journey_uid = NULL, start_time = NULL, end_time = NULL, ss = 2 WHERE [uid] = '{0}'",
                               beatHistory.UID);
            returnValue = await ExecuteNonQueryAsync(query);
            if (returnValue > 0)
            {
                beatHistory.StartTime = null;
                beatHistory.EndTime = null;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return returnValue;
    }
    public async Task<int> UpdateUserJourneyUIDForBeatHistory(IJPBeatHistory beatHistory)
    {
        int returnValue;
        try
        {
            string query = string.Format(@"UPDATE beat_history SET 
            user_journey_uid = '{0}',job_position_uid = '{3}', login_Id = '{4}', ss = 2 WHERE [uid] = '{2}'",
                               _appUser.UserJourney.UID,
                               CommonFunctions.GetDateTimeInFormatForSqlite(DateTime.Now.Date),
                               beatHistory.UID,
                                _appUser.SelectedJobPosition.UID,
                                _appUser.Emp.LoginId);
            returnValue = await ExecuteNonQueryAsync(query);
            if (returnValue > 0)
            {
            }
        }
        catch (Exception)
        {
            throw;
        }
        return returnValue;
    }
    public async Task<int> GetAlreadyCollectedLoadRequestCountForRoute()
    {
        try
        {
            IUserJourney journey = _appUser.UserJourney;
            string query = string.Format(@"SELECT DISTINCT Count(uid)
                    FROM wh_stock_request 
                    WHERE request_type IN ('Load','DCom','FOC') /*AND job_position_uid = '{0}'*/ 
                    AND required_by_date = Date('{1}', 'localtime') AND Status IN ('{2}') 
                    AND route_uid = '{3}'
                    ", _appUser.SelectedJobPosition.UID,
                    CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date),
                    StockTransferStatus.COLLECTED,
                    _appUser.SelectedRoute.UID);

            return await ExecuteScalarAsync<int>(query);
        }
        catch (Exception) { throw; }
    }
    public async Task<int> GetAlreadyVisitedCustomerCountForRoute()
    {
        try
        {
            string query = string.Format(@"SELECT COUNT(DISTINCT BH.uid) AS Cnt
                        FROM beat_history BH
                        INNER JOIN store_history SH ON BH.visit_date = Date('{0}', 'localtime') AND BH.route_uid = '{1}'
                        AND SH.beat_history_uid = BH.uid AND IFNULL(Status,'') = 'visited'",
                CommonFunctions.GetDateTimeInFormatForSqlite(_appUser.JourneyStartDate.Date),
                _appUser?.SelectedRoute?.UID ?? null);

            return await ExecuteScalarAsync<int>(query);
        }
        catch (Exception) { throw; }
    }
    public async Task<int> UpdateStockAuditAndStopBeatHistory(string stockAuditUID, IBeatHistory beatHistory, DateTime stopTime)
    {
        try
        {
            string currentDate = CommonFunctions.GetDateTimeInFormatForSqlite(stopTime);
            string query = string.Format(@"UPDATE beat_history SET has_audit_completed = 1, wh_stock_audit_uid = @WHStockAuditUID, 
            end_time = @EndTime, modified_time = @ModifiedTime, user_journey_vehicle_uid = @UserJourneyVehicleUID, ss = 2 WHERE uid = @UID");
            IDictionary<string, object?> param = new Dictionary<string, object?>
            {
                {"@UID",  beatHistory?.UID  },
                {"@WHStockAuditUID",  stockAuditUID},
                {"@EndTime", currentDate },
                {"@ModifiedTime",  currentDate },
                {"@UserJourneyVehicleUID",  beatHistory?.UserJourneyVehicleUID}
            };
            return await ExecuteNonQueryAsync(query, param);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateBeatHistoryUIDInUserJourney(string beatHistoryUID, string userJourneyUID)
    {
        try
        {
            string query = string.Format(@"UPDATE user_journey SET beat_history_uid = '{0}' 
                                SS = 2 WHERE [uid] = '{1}'",
                                beatHistoryUID, userJourneyUID);
            return await ExecuteNonQueryAsync(query);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> DeleteUserJourney()
    {
        try
        {
            string query = string.Format(@"Delete from user_journey");
            return await ExecuteNonQueryAsync(query);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<bool> InsertMasterRabbitMQueue(MasterDTO masterDTO)
    {
        return false;
    }

    public async Task<int> UpdateUserJourney(IUserJourney userJourney)
    {
        try
        {
            // SQL query to update user_journey
            string sql = $"""
                      UPDATE user_journey
                      SET
                          modified_by = @ModifiedBy,
                          modified_time = @ModifiedTime,
                          journey_end_time = @JourneyEndTime,
                          journey_time = @JourneyTime,
                          eot_status = @EOTStatus,
                          ss = @SS 
                      WHERE
                          uid = @UID;
                      """;

            return await ExecuteNonQueryAsync(sql, userJourney);
        }
        catch (Exception ex)
        {
            // Handle the error, log or throw
            throw new Exception("Error updating user journey.", ex);
        }
    }

    public async Task<int> UpdateBeatHistoryjourneyEndTime(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
    {

        try
        {
            // SQL query to update user_journey
            string sql = $"""
                      UPDATE beat_history
                      SET
                          modified_by = @ModifiedBy,
                          modified_time = @ModifiedTime,
                           cfd_time =@CFDTime,
                          ss = @SS 
                      WHERE
                          uid = @UID;
                      """;

            return await ExecuteNonQueryAsync(sql, beatHistory);
        }
        catch (Exception ex)
        {
            // Handle the error, log or throw
            throw new Exception("Error updating user journey.", ex);
        }

    }
    public async Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
    {
        string query = @"SELECT DISTINCT SCGI.sku_uid 
                FROM selection_map_criteria SMC
                INNER JOIN selection_map_details SMD ON SMD.selection_map_criteria_uid = SMC.uid
                AND SMC.linked_item_type = 'SKUClassGroup' AND SMD.selection_group = 'SalesTeam'
                and SMD.type_uid  = 'BroadClassification' 
                INNER JOIN store S ON S.uid = @StoreUID AND S.broad_classification = SMD.selection_value
                INNER JOIN sku_class_group_items SCGI ON SCGI.sku_class_group_uid = SMC.linked_item_uid";
        Dictionary<string, object?> parameters = new()
        {
            { "@StoreUID", storeUID }
        };
        try
        {
            return await ExecuteQueryAsync<string>(query, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching allowed sku.", ex);
        }
    }
}
