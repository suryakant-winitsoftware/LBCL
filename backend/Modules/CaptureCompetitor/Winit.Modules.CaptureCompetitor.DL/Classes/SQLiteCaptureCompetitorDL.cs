using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.DL.Interfaces;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CaptureCompetitor.DL.Classes
{
    public class SQLiteCaptureCompetitorDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ICaptureCompetitorDL
    {
        public SQLiteCaptureCompetitorDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<int> CreateCaptureCompetitor(ICaptureCompetitor captureCompetitor)
        {
            try
            {
                var sql = @"INSERT INTO capture_competitor (uid, created_by, created_time, modified_by, modified_time, 
                          server_add_time, server_modified_time, ss, store_uid, status, store_history_uid, 
                          beat_history_uid, route_uid, activity_date, job_position_uid, emp_uid, our_brand, our_price, 
                          other_company, other_brand_name, other_item_name, other_temperature, other_price, other_promotion, 
                          other_notes)
                          VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                  @ServerModifiedTime, @SS, @StoreUID, @Status, @StoreHistoryUID, @BeatHistoryUID, 
                                  @RouteUID, @ActivityDate, @JobPositionUID, @EmpUID, @OurBrand, @OurPrice, 
                                  @OtherCompany, @OtherBrandName, @OtherItemName, @OtherTemperature, @OtherPrice, 
                                  @OtherPromotion, @OtherNotes)";
                return await ExecuteNonQueryAsync(sql, captureCompetitor);
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error creating Capture Competitor", ex);
            }
        }

        public async Task<int> DeleteCaptureCompetitor(string UID)
        {
            try
            {
                var sql = @"DELETE FROM capture_competitor WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", UID } };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error deleting Capture Competitor", ex);
            }
        }

        public async Task<PagedResponse<ICaptureCompetitor>> GetCaptureCompetitorDetails(
            List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
            List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"SELECT * FROM capture_competitor");

                // Filter criteria
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder filterClause = new StringBuilder(" WHERE ");
                    //AppendFilterCriteria(filterCriterias, filterClause);
                    sql.Append(filterClause);
                }

                // Sorting criteria
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                // Pagination
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                }

                var parameters = new Dictionary<string, object>();  // Add filter parameters if needed
                IEnumerable<ICaptureCompetitor> captureCompetitorDetails = await ExecuteQueryAsync<ICaptureCompetitor>(sql.ToString(), parameters);

                int totalCount = -1;
                if (isCountRequired)
                {
                    var countSql = @"SELECT COUNT(1) FROM capture_competitor";
                    totalCount = await ExecuteScalarAsync<int>(countSql, parameters);
                }

                return new PagedResponse<ICaptureCompetitor>
                {
                    PagedData = captureCompetitorDetails,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error fetching Capture Competitor details", ex);
            }
        }

        public async Task<ICaptureCompetitor> GetCaptureCompetitorDetailsByUID(string UID)
        {
            try
            {
                var sql = @"SELECT * FROM capture_competitor WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", UID } };
                return await ExecuteSingleAsync<ICaptureCompetitor>(sql, parameters);
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error fetching Capture Competitor by UID", ex);
            }
        }


        public async Task<int> UpdateCaptureCompetitor(ICaptureCompetitor captureCompetitor)
        {
            try
            {
                var sql = @"UPDATE capture_competitor SET 
                            modified_by = @ModifiedBy, modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, ss = @SS, 
                            store_uid = @StoreUID, status = @Status, 
                            our_brand = @OurBrand, our_price = @OurPrice, 
                            other_company = @OtherCompany, other_brand_name = @OtherBrandName, 
                            other_item_name = @OtherItemName, other_temperature = @OtherTemperature, 
                            other_price = @OtherPrice, other_promotion = @OtherPromotion,
                            other_notes = @OtherNotes WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, captureCompetitor);
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error updating Capture Competitor", ex);
            }
        }


    }
}
