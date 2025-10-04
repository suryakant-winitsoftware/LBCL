using Microsoft.Extensions.Configuration;
using System.Data;
using Winit.Modules.RabbitMQQueue.DL.Interfaces;

namespace Winit.Modules.RabbitMQQueue.DL.Classes
{
    public class MSSQLRabbitMQLogDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IRabbitMQLogDL
    {
        public MSSQLRabbitMQLogDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                                WHEN @step = 'Step2' AND @stepResult = 1 THEN GETDATE() 
                                ELSE request_sent_to_service_time
                            END,
                            request_received_by_service_time = CASE
                                WHEN @step = 'Step3' AND @stepResult = 1 THEN GETDATE() 
                                ELSE request_received_by_service_time
                            END,
                            request_posted_to_db_time = CASE
                                WHEN @step = 'Step4' AND @stepResult = 1 THEN GETDATE() 
                                ELSE request_posted_to_db_time
                            END,
                            notification_sent_time = CASE
                                WHEN @step = 'Step5' AND @stepResult = 1 THEN GETDATE() 
                                ELSE notification_sent_time
                            END,
                            is_failed = ISNULL(is_failed, @isFailed),
                            comments = 
                                CASE
                                    WHEN ISNULL(comments, '') != '' THEN 
                                        CASE WHEN ISNULL(comments, '') = '' THEN '' ELSE (ISNULL(comments, '') + '|') END + ISNULL(@comments, '') 
                                    ELSE ISNULL(@comments, '')
                                END
                            --,modified_time = GETDATE()
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
