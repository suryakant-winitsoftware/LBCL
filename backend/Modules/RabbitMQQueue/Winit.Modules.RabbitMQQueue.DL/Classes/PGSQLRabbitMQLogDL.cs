using Microsoft.Extensions.Configuration;
using System.Data;
using Winit.Modules.RabbitMQQueue.DL.Interfaces;

namespace Winit.Modules.RabbitMQQueue.DL.Classes
{
    public class PGSQLRabbitMQLogDL: Winit.Modules.Base.DL.DBManager.PostgresDBManager, IRabbitMQLogDL
    {
        public PGSQLRabbitMQLogDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<int> InsertAppRequestInfo(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest)
        {
            string messageUID = string.Empty;
            try
            {
                string insertQuery = @"
                    INSERT INTO app_request_info(uid, org_uid, linked_item_type, year_month, app_request_uid)
                    SELECT @UID, @OrgUID, @LinkedItemType, @YearMonth, @AppRequestUID
                    WHERE NOT EXISTS (
                    SELECT 1 FROM app_request_info WHERE uid = @UID
                    )
                ";

                var parameters = new Dictionary<string, object>
                {
                    { "@UID", appRequest.UID },
                    { "@OrgUID", appRequest.OrgUID},
                    { "@LinkedItemType", appRequest.LinkedItemType },
                    { "@YearMonth", appRequest.YearMonth },
                    { "@AppRequestUID", appRequest.UID },
                };
                return await ExecuteNonQueryAsync(insertQuery, parameters);
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        public async Task<int> UpdateLogByStepAsync(string UID, string Step, bool StepResult, bool IsFailed, string comments)
        {
            try
            {
                var updateQuery = @"
                    UPDATE app_request_info
                    SET 
                        request_sent_to_service_time = CASE
                            WHEN @step = 'Step2' AND @stepResult = TRUE THEN NOW() 
                            ELSE request_sent_to_service_time
                        END,
                        request_received_by_service_time = CASE
                            WHEN @step = 'Step3' AND @stepResult = TRUE THEN NOW() 
                            ELSE request_received_by_service_time
                        END,
                        request_posted_to_db_time = CASE
                            WHEN @step = 'Step4' AND @stepResult = TRUE THEN NOW() 
                            ELSE request_posted_to_db_time
                        END,
                        notification_sent_time = CASE
                            WHEN @step = 'Step5' AND @stepResult = TRUE THEN NOW() 
                            ELSE notification_sent_time
                        END,
                        is_failed = COALESCE(is_failed, @isFailed),
                        comments = 
                            CASE
                                WHEN COALESCE(comments, '') != '' THEN 
                                    CASE WHEN COALESCE(comments, '') = '' THEN '' ELSE (comments || '|') END || COALESCE(@comments, '') 
                                ELSE comments
                            END
                        --,modified_time = NOW()
                    WHERE uid = @uid";

                var parameters = new Dictionary<string, object>
            {
                { "@uid", UID },
                { "@step", Step },
                { "@stepResult", StepResult },
                { "@isFailed", IsFailed },
                { "@comments", comments }
            };

                return await ExecuteNonQueryAsync(updateQuery, parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error in UpdateLogByStepAsync for UID {UID}, Step {Step}, and StepResult {StepResult}");
                throw;
            }
        }
    }
}
