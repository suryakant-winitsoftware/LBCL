using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Collections.Specialized;
using Winit.Modules.Survey.Model.Classes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;



namespace Winit.Modules.Survey.DL.Classes
{
    public class PGSQLStoreandUserReportsDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStoreandUserReportsDL
    {
        public PGSQLStoreandUserReportsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>> GetStoreUserSummary(
    List<SortCriteria> sortCriterias,
    int pageNumber,
    int pageSize,
    List<FilterCriteria> filterCriterias,
    bool isCountRequired)
        {
            try
            {
                // Input validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Base query with meaningful parameter names
                var baseQuery = @"SELECT * FROM (
    SELECT 
        S.code AS StoreCode,
        S.name AS StoreName,
        E.login_id AS EmpCode,
        E.name AS EmpName,
        JP.designation,
        '' AS SaleCategory,
        SUM(CASE WHEN SH.status = 'Visited' THEN 1 ELSE 0 END) AS NoOfTimesVisited,
        MIN(BH.visit_date) AS StartDate,
        MAX(BH.visit_date) AS EndDate,
        COALESCE(LR.code, '') AS LocationCode,
        COALESCE(LR.name, '') AS LocationName
    FROM beat_history BH
    INNER JOIN store_history SH ON SH.beat_history_uid = BH.uid
        AND SH.status IN ('Visited','Skipped')
    INNER JOIN emp E ON E.uid = BH.login_id
    INNER JOIN job_position JP ON JP.emp_uid = E.uid
    INNER JOIN store S ON S.uid = SH.store_uid
    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = JP.location_value
    LEFT JOIN location LR ON LR.uid = LC.parent_uid
    WHERE BH.visit_date BETWEEN :startDate::date AND :endDate::date
    GROUP BY S.code, S.name, E.login_id, E.name, JP.designation, LR.code, LR.name
) AS subquery";

                var sql = new StringBuilder(baseQuery);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder($"SELECT COUNT(1) AS Cnt FROM ({baseQuery}) AS count_query");
                }

                var parameters = new Dictionary<string, object>();

                // Handle date filters first
                var startDateFilter = filterCriterias?.FirstOrDefault(f => f.Name?.ToLower() == "startdate");
                var endDateFilter = filterCriterias?.FirstOrDefault(f => f.Name?.ToLower() == "enddate");

                // Set default date range if not provided
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);
                var from = fromDate.ToString("yyyy-MM-dd");
                var to = toDate.ToString("yyyy-MM-dd");

                parameters.Add("startDate", startDateFilter?.Value?.ToString() ?? from);
                parameters.Add("endDate", endDateFilter?.Value?.ToString() ?? to);

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");

                    // Process non-date filters
                    var nonDateFilters = filterCriterias.Where(f =>
                        f.Name?.ToLower() != "startdate" &&
                        f.Name?.ToLower() != "enddate").ToList();

                    if (nonDateFilters.Any())
                    {
                        foreach (var filter in nonDateFilters)
                        {
                            if (string.IsNullOrEmpty(filter.Name)) continue;

                            string paramName = filter.Name.ToLower() switch
                            {
                                "storecode" => "storeCode",
                                "empcode" => "empCode",
                                "locationcode" => "locationCode",
                                "designation" => "designation",
                                _ => filter.Name.ToLower()
                            };

                            string columnName = filter.Name.ToLower() switch
                            {
                                "storecode" => "StoreCode",
                                "empcode" => "EmpCode",
                                "locationcode" => "LocationCode",
                                "designation" => "JP.designation",
                                _ => filter.Name
                            };

                            if (sbFilterCriteria.Length > 7) // " WHERE ".Length
                            {
                                sbFilterCriteria.Append(" AND ");
                            }

                            sbFilterCriteria.Append($"{columnName} = @{paramName}");
                            parameters.Add(paramName, filter.Value);
                        }

                        sql.Append(sbFilterCriteria);
                        if (isCountRequired)
                        {
                            sqlCount.Append(sbFilterCriteria);
                        }
                    }
                }

                // Add sorting
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY StoreCode");
                }

                // Add pagination
                sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");

                // Execute queries
                IEnumerable<Model.Interfaces.IStoreUserInfo> storeandUserList;
                int totalCount = 0;

                try
                {
                    storeandUserList = await ExecuteQueryAsync<Model.Interfaces.IStoreUserInfo>(sql.ToString(), parameters);

                    if (isCountRequired)
                    {
                        totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                    }

                    return new PagedResponse<Model.Interfaces.IStoreUserInfo>
                    {
                        PagedData = storeandUserList.ToList(),
                        TotalCount = totalCount
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error executing query: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetStoreUserSummary: {ex.Message}", ex);
            }
        }

        //public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>> GetStoreUserActivityDetails(List<SortCriteria> sortCriterias, int pageNumber,
        //                                int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        //{
        //    try
        //    {
        //        var sql = new StringBuilder(@"Select * from (SELECT 
        //                        BH.visit_date AS VisitDateTime, 
        //                        BH.visit_date AS VisitDate, 
        //                        S.code AS StoreCode, 
        //                        S.name AS StoreName, 
        //                       E.login_id AS EmpCode, 
        //                        E.name AS EmpName, 
        //                        JP.designation AS Designation, 
        //                        0 AS DistanceVariance,
        //                        COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time) AS StartTime,
        //                        COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) AS EndTime,
        //                        TO_CHAR(
        //                            (COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) -  
        //                             COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time)), 'HH24:MI:SS') AS TimeSpent,
        //                        SH.status AS Status,
        //                jp.location_value as LocationValue,LR.code as LocationCode,LR.name LocationName,
        //                '[' || E.login_id || '] ' || E.name AS Users

        //                    FROM beat_history BH
        //                    INNER JOIN store_history SH 
        //                        ON SH.beat_history_uid = BH.uid
        //                       -- AND SH.status = 'Visited' 
        //                        AND BH.visit_date BETWEEN 
        //                            COALESCE(@StartDate::date, CURRENT_DATE) AND 
        //                            COALESCE(@EndDate::date, CURRENT_DATE)
        //                    INNER JOIN emp E ON E.uid = BH.login_id
        //                    INNER JOIN job_position JP ON JP.emp_uid = E.uid
        //                    INNER JOIN location LC ON LC.location_type_uid = 'City' and LC.Code=jp.location_value
        //                    INNER JOIN location LR ON LR.uid = LC.parent_uid
        //                    INNER JOIN store S ON S.uid = SH.store_uid
        //                    ORDER BY BH.visit_date DESC, StartTime ASC, S.code DESC)as subquery");
        //                                    var sqlCount = new StringBuilder();
        //                                    if (isCountRequired)
        //                                    {
        //                                        sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
        //                        BH.visit_date AS VisitDateTime, 
        //                        BH.visit_date AS VisitDate, 
        //                        S.code AS StoreCode, 
        //                        S.name AS StoreName, 
        //                        E.login_id AS EmpCode, 
        //                        E.name AS EmpName, 
        //                        JP.designation AS Designation, 
        //                        0 AS DistanceVariance,
        //                        COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time) AS StartTime,
        //                        COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) AS EndTime,
        //                        TO_CHAR(
        //                            (COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) -  
        //                             COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time)), 'HH24:MI:SS') AS TimeSpent,
        //                        SH.status AS Status,
        //                jp.location_value as LocationValue,LR.code as LocationCode,LR.name LocationName,
        //                '[' || E.login_id || '] ' || E.name AS Users

        //                    FROM beat_history BH
        //                    INNER JOIN store_history SH 
        //                        ON SH.beat_history_uid = BH.uid
        //                        --AND SH.status = 'Visited' 
        //                        AND BH.visit_date BETWEEN 
        //                            COALESCE(@StartDate::date, CURRENT_DATE) AND 
        //                            COALESCE(@EndDate::date, CURRENT_DATE)
        //                    INNER JOIN emp E ON E.uid = BH.login_id
        //                    INNER JOIN job_position JP ON JP.emp_uid = E.uid
        //                    INNER JOIN location LC ON LC.location_type_uid = 'City' and LC.Code=jp.location_value
        //                    INNER JOIN location LR ON LR.uid = LC.parent_uid
        //                    INNER JOIN store S ON S.uid = SH.store_uid
        //                  ORDER BY BH.visit_date DESC, StartTime ASC, S.code DESC
        //                     )as subquery");
        //        }
        //        var parameters = new Dictionary<string, object>()
        //    {
        //            //{ "StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd") },
        //            //{ "EndDate", DateTime.Now.ToString("yyyy-MM-dd") }
        //            { "StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) },
        //            { "EndDate", DateTime.Now.Date }
        //    };

        //        if (filterCriterias != null && filterCriterias.Count > 0)
        //        {
        //            StringBuilder sbFilterCriteria = new StringBuilder();
        //            sbFilterCriteria.Append(" WHERE ");
        //            AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>(filterCriterias, sbFilterCriteria, parameters);

        //            sql.Append(sbFilterCriteria);
        //            if (isCountRequired)
        //            {
        //                sqlCount.Append(sbFilterCriteria);
        //            }
        //        }

        //        if (sortCriterias != null && sortCriterias.Count > 0)
        //        {
        //            sql.Append(" ORDER BY ");
        //            AppendSortCriteria(sortCriterias, sql);
        //        }

        //        if (pageNumber > 0 && pageSize > 0)
        //        {
        //            if (sortCriterias != null && sortCriterias.Count > 0)
        //            {
        //                sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
        //            }
        //            else
        //            {
        //                sql.Append($" ORDER BY VisitDateTime  OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
        //            }
        //        }

        //        IEnumerable<Model.Interfaces.IStoreUserVisitDetails> storeandUserList = await ExecuteQueryAsync<Model.Interfaces.IStoreUserVisitDetails>(sql.ToString(), parameters);
        //        int totalCount = 0;
        //        if (isCountRequired)
        //        {
        //            totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
        //        }
        //        PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>
        //        {
        //            PagedData = storeandUserList,
        //            TotalCount = totalCount
        //        };

        //        return pagedResponse;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>> GetStoreUserActivityDetails(
    List<SortCriteria> sortCriterias,
    int pageNumber,
    int pageSize,
    List<FilterCriteria> filterCriterias,
    bool isCountRequired)
        {
            try
            {
                var baseQuery = @"
SELECT * FROM (
    SELECT 
        BH.visit_date AS VisitDateTime, 
        BH.visit_date AS VisitDate, 
        S.code AS StoreCode, 
        S.name AS StoreName, 
        E.login_id AS EmpCode, 
        E.name AS EmpName, 
        JP.designation AS Designation, 
        0 AS DistanceVariance,
        COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time) AS StartTime,
        COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) AS EndTime,
        TO_CHAR(
            (COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) -  
             COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time)), 'HH24:MI:SS') AS TimeSpent,
        SH.status AS Status,
        JP.location_value AS LocationValue,
        LR.code AS LocationCode,
        LR.name AS LocationName,
        '[' || E.login_id || '] ' || E.name AS Users
    FROM beat_history BH
    INNER JOIN store_history SH 
        ON SH.beat_history_uid = BH.uid
       -- AND BH.visit_date 
    INNER JOIN emp E ON E.login_id = BH.login_id
    INNER JOIN job_position JP ON JP.emp_uid = E.uid
    INNER JOIN location LC ON LC.location_type_uid = 'City' AND LC.Code = JP.location_value
    INNER JOIN location LR ON LR.uid = LC.parent_uid
    INNER JOIN store S ON S.uid = SH.store_uid
    ORDER BY 
        CASE JP.designation 
            WHEN 'ASM' THEN 1 
            WHEN 'SO' THEN 2 
            WHEN 'ISR' THEN 3 
            ELSE 4 
        END,
        BH.visit_date DESC,
        StartTime ASC
) AS subquery";

                var sql = new StringBuilder(baseQuery);
                var sqlCount = new StringBuilder();

                if (isCountRequired)
                {
                    sqlCount.Append(@"
SELECT COUNT(1) AS Cnt FROM (
    SELECT 
        BH.visit_date AS VisitDateTime, 
        BH.visit_date AS VisitDate, 
        S.code AS StoreCode, 
        S.name AS StoreName, 
        E.login_id AS EmpCode, 
        E.name AS EmpName, 
        JP.designation AS Designation, 
        0 AS DistanceVariance,
        COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time) AS StartTime,
        COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) AS EndTime,
        TO_CHAR(
            (COALESCE(CAST(SH.logout_time AS time(0)), '00:00:00'::time) -  
             COALESCE(CAST(SH.login_time AS time(0)), '00:00:00'::time)), 'HH24:MI:SS') AS TimeSpent,
        SH.status AS Status,
        JP.location_value AS LocationValue,
        LR.code AS LocationCode,
        LR.name AS LocationName,
        '[' || E.login_id || '] ' || E.name AS Users
    FROM beat_history BH
    INNER JOIN store_history SH 
        ON SH.beat_history_uid = BH.uid
       -- AND BH.visit_date 
    INNER JOIN emp E ON E.login_id = BH.login_id
    INNER JOIN job_position JP ON JP.emp_uid = E.uid
    INNER JOIN location LC ON LC.location_type_uid = 'City' AND LC.Code = JP.location_value
    INNER JOIN location LR ON LR.uid = LC.parent_uid
    INNER JOIN store S ON S.uid = SH.store_uid
) AS subquery");
                }
               
                    var parameters = new Dictionary<string, object>
        {
            { "StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) },
            { "EndDate", DateTime.Now.Date }
        };
                

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                // Pagination (on already ordered data)
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                var storeandUserList = await ExecuteQueryAsync<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                return new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>
                {
                    PagedData = storeandUserList,
                    TotalCount = totalCount
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList)
        {
            var retVal = -1;
            try
            {
                if (storeRollingStatsModelList != null && storeRollingStatsModelList.Any())
                {
                    var codes = storeRollingStatsModelList.Select(e => e.StoreUID).ToList();
                    var parametes = new Dictionary<string, object>() {
                        {"Codes",codes }
                    };
                    var UIDsQuery = @"SELECT uid AS UID, code AS Code FROM store WHERE code = any(@Codes)";
                    var list = await ExecuteQueryAsync<ISelectionItem>(UIDsQuery, parametes);
                    var codeToUIDMap = list.ToDictionary(l => l.Code, l => l.UID);
                    foreach (var item in storeRollingStatsModelList)
                    {
                        if (codeToUIDMap.TryGetValue(item.StoreUID, out var newUID))
                        {
                            item.StoreUID = newUID;
                        }
                    }
                }

                var sql = @"INSERT INTO store_rolling_stats (uid, store_uid, growth_percentage, gr_percentage, 
                            monthly_growth, mtd_sales_value, mtd_sales_volume, monthly_growth_mty, mty_sales_value, 
                            mty_sales_volume, last_order_value, avg_order_value, outstanding_payment, non_productive_call_count, 
                            avg_line_per_call, avg_category_per_call, last_6_order_value_l3m, avg_brand_per_call, last_month_sales_value, 
                            last_3_months_avg_sales_value, lm_avg_sale_value, lm_no_of_invoices, lm_invoice_value, last_delivery_amount,
                            last_delivery_invoice_no, last_delivery_date, outstanding_amount, outstanding_invoice_count, tasks_open, 
                            no_of_inv_pending_for_delivery, no_of_assets, no_of_access_points, no_of_visibilities, ageing_of_credit_in_days,
                            sku_listed, last_visit_date, ss, created_time, created_by, modified_time, modified_by, server_add_time, server_modified_time) 
                            VALUES 
                            (@UID, @StoreUID, @GrowthPercentage, @GRPercentage, @MonthlyGrowth, @MTDSalesValue, @MTDSalesVolume,
                            @MonthlyGrowthMty, @MtySalesValue, @MTYSalesVolume, @LastOrderValue, @AvgOrderValue, @OutstandingPayment,
                            @NonProductiveCallCount, @AvgLinePerCall, @AvgCategoryPerCall, @Last6OrderValueL3M, @AvgBrandPerCall,
                            @LastMonthSalesValue, @Last3MonthsAvgSalesValue, @LMAvgSaleValue, @LMNoOfInvoices, @LMInvoiceValue,
                            @LastDeliveryAmount, @LastDeliveryInvoiceNo, @LastDeliveryDate, @OutstandingAmount, @OutstandingInvoiceCount,
                            @TasksOpen, @NoOfInvPendingForDelivery, @NoOfAssets, @NoOfAccessPoints, @NoOfVisibilities, @AgeingOfCreditInDays,
                            @SKUListed, @LastVisitDate, @SS, @CreatedTime, @CreatedBy, @ModifiedTime, @ModifiedBy, @ServerAddTime, @ServerModifiedTime)";
                retVal = await ExecuteNonQueryAsync(sql, storeRollingStatsModelList);

            }
            catch
            {
                throw;
            }
            return retVal;
        }

        public Task<IStoreRollingStatsModel> GetStoreUserActivityDetailsByStoreUID(string StoreUID)
        {
            throw new NotImplementedException();
        }
    }
}
