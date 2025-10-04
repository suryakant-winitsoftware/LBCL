using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Classes
{
    public class SQLiteROTAActivityDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IROTAActivityDL
    {
        public SQLiteROTAActivityDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IROTAActivity> GetByUID(string uid)
        {
            try
            {
                var sql = @"
                    SELECT * FROM rota_activity 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IROTAActivity>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching ROTA activity by UID", ex);
            }
        }

        public async Task<List<IROTAActivity>> GetAll()
        {
            try
            {
                var sql = "SELECT * FROM rota_activity";
                return await ExecuteQueryAsync<IROTAActivity>(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all ROTA activities", ex);
            }
        }

        public async Task<bool> Insert(IROTAActivity rotaActivity)
        {
            try
            {
                var sql = @"
                    INSERT INTO rota_activity (
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, job_position_uid,
                        rota_date, rota_activity_name
                    ) VALUES (
                        @Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                        @ServerAddTime, @ServerModifiedTime, @JobPositionUID,
                        @RotaDate, @RotaActivityName
                    )";

                var parameters = new Dictionary<string, object>
                {
                    { "Id", rotaActivity.Id },
                    { "UID", rotaActivity.UID },
                    { "SS", rotaActivity.SS },
                    { "CreatedBy", rotaActivity.CreatedBy },
                    { "CreatedTime", rotaActivity.CreatedTime },
                    { "ModifiedBy", rotaActivity.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", rotaActivity.ModifiedTime },
                    { "ServerAddTime", rotaActivity.ServerAddTime },
                    { "ServerModifiedTime", rotaActivity.ServerModifiedTime },
                    { "JobPositionUID", rotaActivity.JobPositionUID },
                    { "RotaDate", rotaActivity.RotaDate },
                    { "RotaActivityName", rotaActivity.RotaActivityName }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting ROTA activity", ex);
            }
        }

        public async Task<bool> Update(IROTAActivity rotaActivity)
        {
            try
            {
                var sql = @"
                    UPDATE rota_activity SET
                        ss = @SS,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        job_position_uid = @JobPositionUID,
                        rota_date = @RotaDate,
                        rota_activity_name = @RotaActivityName
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", rotaActivity.UID },
                    { "SS", rotaActivity.SS },
                    { "ModifiedBy", rotaActivity.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", rotaActivity.ModifiedTime },
                    { "ServerModifiedTime", rotaActivity.ServerModifiedTime },
                    { "JobPositionUID", rotaActivity.JobPositionUID },
                    { "RotaDate", rotaActivity.RotaDate },
                    { "RotaActivityName", rotaActivity.RotaActivityName }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ROTA activity", ex);
            }
        }

        public async Task<bool> Delete(string uid)
        {
            try
            {
                var sql = "DELETE FROM rota_activity WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting ROTA activity", ex);
            }
        }

        public async Task<List<IROTAActivity>> GetByJobPositionUID(string jobPositionUID)
        {
            try
            {
                var sql = "SELECT * FROM rota_activity WHERE job_position_uid = @JobPositionUID";
                var parameters = new Dictionary<string, object> { { "JobPositionUID", jobPositionUID } };
                return await ExecuteQueryAsync<IROTAActivity>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching ROTA activities by job position UID", ex);
            }
        }

        public async Task<List<IROTAActivity>> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sql = "SELECT * FROM rota_activity WHERE rota_date BETWEEN @StartDate AND @EndDate";
                var parameters = new Dictionary<string, object>
                {
                    { "StartDate", startDate.Date },
                    { "EndDate", endDate.Date }
                };
                return await ExecuteQueryAsync<IROTAActivity>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching ROTA activities by date range", ex);
            }
        }

        public async Task<List<IROTAActivity>> GetByJobPositionAndDateRange(string jobPositionUID, DateTime startDate, DateTime endDate)
        {
            try
            {
                var sql = @"
                    SELECT * FROM rota_activity 
                    WHERE job_position_uid = @JobPositionUID 
                    AND rota_date BETWEEN @StartDate AND @EndDate";

                var parameters = new Dictionary<string, object>
                {
                    { "JobPositionUID", jobPositionUID },
                    { "StartDate", startDate.Date },
                    { "EndDate", endDate.Date }
                };
                return await ExecuteQueryAsync<IROTAActivity>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching ROTA activities by job position and date range", ex);
            }
        }
    }
} 