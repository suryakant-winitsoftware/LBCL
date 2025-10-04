using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.CaptureCompetitor.DL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
namespace Winit.Modules.CaptureCompetitor.DL.Classes
{
    public class PGSQLCaptureCompetitorDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ICaptureCompetitorDL
    {
        public PGSQLCaptureCompetitorDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>> GetCaptureCompetitorDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"select * from(SELECT cc.id AS Id, cc.uid AS UID, cc.created_by AS CreatedBy, cc.created_time AS CreatedTime,
                                          cc.modified_by AS ModifiedBy, cc.modified_time AS ModifiedTime, cc.server_add_time AS ServerAddTime, 
                                          cc.server_modified_time AS ServerModifiedTime, cc.ss AS SS, cc.store_uid AS StoreUID, cc.status AS Status,
                                          cc.store_history_uid AS StoreHistoryUID, cc.beat_history_uid AS BeatHistoryUID, cc.route_uid AS RouteUID, 
                                          cc.activity_date AS ActivityDate, cc.job_position_uid AS JobPositionUID, cc.emp_uid AS EmpUID, cc.our_brand AS OurBrand, 
                                          cc.our_price AS OurPrice, cc.other_company AS OtherCompany, cc.other_brand_name AS OtherBrandName, cc.other_item_name AS OtherItemName,
                                          cc.other_temperature AS OtherTemperature, cc.other_price AS OtherPrice, cc.other_promotion AS OtherPromotion,
                                          cc.other_notes AS OtherNotes FROM capture_competitor cc )as sub_query");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT cc.id AS Id, cc.uid AS UID, cc.created_by AS CreatedBy, cc.created_time AS CreatedTime,
                                          cc.modified_by AS ModifiedBy, cc.modified_time AS ModifiedTime, cc.server_add_time AS ServerAddTime, 
                                          cc.server_modified_time AS ServerModifiedTime, cc.ss AS SS, cc.store_uid AS StoreUID, cc.status AS Status,
                                          cc.store_history_uid AS StoreHistoryUID, cc.beat_history_uid AS BeatHistoryUID, cc.route_uid AS RouteUID, 
                                          cc.activity_date AS ActivityDate, cc.job_position_uid AS JobPositionUID, cc.emp_uid AS EmpUID, cc.our_brand AS OurBrand, 
                                          cc.our_price AS OurPrice, cc.other_company AS OtherCompany, cc.other_brand_name AS OtherBrandName, cc.other_item_name AS OtherItemName,
                                          cc.other_temperature AS OtherTemperature, cc.other_price AS OtherPrice, cc.other_promotion AS OtherPromotion,
                                          cc.other_notes AS OtherNotes FROM capture_competitor cc )as sub_query");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor> captureCompetitorDetails = await ExecuteQueryAsync<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor> pagedResponse = new PagedResponse<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>
                {
                    PagedData = captureCompetitorDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor> GetCaptureCompetitorDetailsByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = @"SELECT cc.id AS Id, cc.uid AS UID, cc.created_by AS CreatedBy, cc.created_time AS CreatedTime,
                            cc.modified_by AS ModifiedBy, cc.modified_time AS ModifiedTime, cc.server_add_time AS ServerAddTime, 
                            cc.server_modified_time AS ServerModifiedTime, cc.ss AS SS, cc.store_uid AS StoreUID, cc.status AS Status,
                            cc.store_history_uid AS StoreHistoryUID, cc.beat_history_uid AS BeatHistoryUID, cc.route_uid AS RouteUID, 
                            cc.activity_date AS ActivityDate, cc.job_position_uid AS JobPositionUID, cc.emp_uid AS EmpUID, cc.our_brand AS OurBrand, 
                            cc.our_price AS OurPrice, cc.other_company AS OtherCompany, cc.other_brand_name AS OtherBrandName, cc.other_item_name AS OtherItemName,
                            cc.other_temperature AS OtherTemperature, cc.other_price AS OtherPrice, cc.other_promotion AS OtherPromotion,
                            cc.other_notes AS OtherNotes FROM capture_competitor cc
                            WHERE cc.uid = @UID";
                return await ExecuteSingleAsync<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>(sql, parameters);
            }
            catch
            {
                throw;
            }

        }
        public async Task<int> CreateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor)
        {
            try
            {
                var sql = @"INSERT INTO capture_competitor ( uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                          server_modified_time, ss, store_uid, status, store_history_uid, beat_history_uid, route_uid, activity_date,
                          job_position_uid, emp_uid, our_brand, our_price, other_company, other_brand_name, other_item_name, other_temperature,
                          other_price, other_promotion, other_notes)
                          VALUES 
                         (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                          @SS, @StoreUID, @Status, @StoreHistoryUID, @BeatHistoryUID, @RouteUID, @ActivityDate, @JobPositionUID, 
                          @EmpUID, @OurBrand, @OurPrice, @OtherCompany, @OtherBrandName, @OtherItemName, @OtherTemperature, @OtherPrice, @OtherPromotion, @OtherNotes)";
                return await ExecuteNonQueryAsync(sql, captureCompetitor);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<int> UpdateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor)
        {
            try
            {
                var sql = @"UPDATE capture_competitor SET  modified_by = @ModifiedBy, modified_time = @ModifiedTime,  
                            server_modified_time = @ServerModifiedTime, ss = @SS, store_uid = @StoreUID, status = @Status,
                            our_brand = @OurBrand, our_price = @OurPrice, other_company = @OtherCompany, other_brand_name = @OtherBrandName, 
                            other_item_name = @OtherItemName, other_temperature = @OtherTemperature, other_price = @OtherPrice, other_promotion = @OtherPromotion,
                            other_notes = @OtherNotes WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, captureCompetitor);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteCaptureCompetitor(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = "DELETE  FROM capture_competitor uid = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }

        }

      
    }
}
