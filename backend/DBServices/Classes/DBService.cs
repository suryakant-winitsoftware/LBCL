using DBServices.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System.Data;
using Winit.Modules.Base.DL.DBManager;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Winit.Shared.Models;
using Winit.Modules.FirebaseReport.Models.Interfaces;

namespace DBServices.Classes;


public class DBService : PostgresDBManager, IDBService
{
    private readonly ILogger<DBService> _logger;

    public DBService(IServiceProvider serviceProvider, IConfiguration config, ILogger<DBService> logger) : base(serviceProvider, config)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, dynamic>> UPSertLogs(List<LogEntry> logList)
    {
        Dictionary<string, dynamic> uploadResponseObjectList = new Dictionary<string, dynamic>();
        try
        {
            if (logList != null && logList.Count > 0)
            {
                foreach (LogEntry logentry in logList)
                {
                    _logger.LogInformation("UPSertLogs LogUID : {@messegeid}", logentry.UID);

                    var insertQuery = @"
                    DECLARE @logExistUid NVARCHAR(100);
                    SET @logExistUid = (SELECT uid FROM tbl_logs WHERE uid = @Uid);

                    IF @logExistUid IS NULL
                    BEGIN
                        INSERT INTO tbl_logs (
                            uid,
                            comments,
                            linked_item_uid,
                            linked_item_type,
                            request_body,
                            is_failed,
                            user_code,
                            customer_code
                            request_created_time,
                            request_posted_to_api_time,
                            modified_time,
                            notification_received_time,
                            request_sent_to_log_api_time
                        )
                        VALUES (
                            @Uid,
                            @Comments,
                            @LinkedItemUID,
                            @LinkedItemType,
                            @RequestBody,
                            @IsFailed,
                            @UserCode,
                            @CustomerCode,
                            @RequestCreatedTime,
                            @RequestPostedToApiTime,
                            @ModifiedTime,
                            @NotificationReceivedTime,
                            @RequestSent2LogApiTime
                        );
                    END
                    ELSE
                    BEGIN
                        UPDATE tbl_logs
                        SET 
                            comments = ISNULL(comments + '|' + @Comments, @Comments),
                            linked_item_uid = @LinkedItemUID,
                            linked_item_type = @LinkedItemType,
                            request_body = @RequestBody,
                            is_failed = CASE WHEN @IsFailed = '1' THEN 1 ELSE 0 END,
                            customer_code = @CustomerCode,
                            user_code = @UserCode,
                            request_created_time = CASE WHEN @RequestCreatedTime IS NOT NULL THEN @RequestCreatedTime ELSE request_created_time END,
                            request_posted_to_api_time = CASE WHEN @RequestPostedToApiTime IS NOT NULL THEN @RequestPostedToApiTime ELSE request_posted_to_api_time END,
                            modified_time = CASE WHEN @ModifiedTime IS NOT NULL THEN @ModifiedTime ELSE modified_time END,
                            notification_received_time = CASE WHEN @NotificationReceivedTime IS NOT NULL THEN @NotificationReceivedTime ELSE notification_received_time END,
                            request_sent_to_log_api_time = CASE WHEN @RequestSent2LogApiTime IS NOT NULL THEN @RequestSent2LogApiTime ELSE request_sent_to_log_api_time END
                        WHERE uid = @Uid;
                    END";

                    var parameters = new Dictionary<string, object>
                    {
                        { "@Uid", logentry.UID },
                        { "@Comments", logentry.Comments ?? "" },
                        { "@LinkedItemUID", logentry.LinkedItemUID },
                        { "@LinkedItemType", logentry.LinkedItemType },
                        { "@UserCode", logentry.UserCode },
                        { "@CustomerCode", logentry.CustomerCode },
                        { "@RequestBody", logentry.RequestBody },
                        { "@IsFailed", logentry.IsFailed == "1" ? 1 : 0 },
                        { "@RequestCreatedTime", string.IsNullOrEmpty(logentry.RequestCreatedTime) ? null : (object)Convert.ToDateTime(logentry.RequestCreatedTime) },
                        { "@RequestPostedToApiTime", string.IsNullOrEmpty(logentry.RequestPostedToApiTime) ? null : (object)Convert.ToDateTime(logentry.RequestPostedToApiTime) },
                        { "@ModifiedTime", string.IsNullOrEmpty(logentry.ModifiedTime) ? null : (object)Convert.ToDateTime(logentry.ModifiedTime) },
                        { "@NotificationReceivedTime", string.IsNullOrEmpty(logentry.NotificationReceivedTime) ? null : (object)Convert.ToDateTime(logentry.NotificationReceivedTime) },
                        { "@RequestSent2LogApiTime", string.IsNullOrEmpty(logentry.RequestSent2LogApiTime) ? null : (object)Convert.ToDateTime(logentry.RequestSent2LogApiTime) }
                    };

                    int retValue = await ExecuteNonQueryAsync(insertQuery, parameters);

                    if (!uploadResponseObjectList.ContainsKey("UpdatedLogs"))
                    {
                        uploadResponseObjectList.Add("UpdatedLogs", logList.Select(e => new { UID = e.UID }).ToList());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while upserting logs.");
            throw;
        }
        return uploadResponseObjectList;
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
                    WHERE uid = @UID";

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

    public async Task<string> GenerateLogUID(string linkeditemuid, string linkeditemtype, string body, string? usercode = null, string? customercode = null, string? UID = null)
    {
        string messageUid = UID;
        try
        {
            string insertQuery = @"
                    INSERT INTO tbl_logs (uid, linked_item_uid, linked_item_type, request_body, user_code, customer_code, request_received_by_api_time, 
                    modified_time)
                    VALUES (@UID, @LinkedItemUID, @LinkedItemType, @RequestBody::json, @UserCode, @CustomerCode, NOW(), NOW())
                    ON CONFLICT (uid) DO UPDATE
                    SET request_received_by_api_time = EXCLUDED.request_received_by_api_time,
                        modified_time = EXCLUDED.modified_time
                    RETURNING uid";




            var parameters = new Dictionary<string, object>
            {
                { "@UID", UID },
                { "@LinkedItemUID", linkeditemuid },
                { "@LinkedItemType", linkeditemtype },
                { "@RequestBody", body},
                { "@UserCode", usercode },
                { "@CustomerCode", customercode }
            };

            //var result = await ExecuteScalerResultAsync(insertQuery, parameters);
            var result = await ExecuteScalarAsync<string>(insertQuery, parameters);

            if (result != null /*&& result != DBNull.Value*/)
                messageUid = result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GenerateLogUID for linkeditemuid {linkeditemuid} and linkeditemtype {linkeditemtype}");
        }
        return messageUid;
    }

    public async Task<int> LogNotificationSent(string ReqUID, string linkeditemuid, string linkeditemtype, string title, string body, DateTime ondate)
    {
        int retval = 0;
        try
        {
            string selectQuery = @"
                SELECT 1
                FROM tbl_fb_notification_sent
                WHERE linked_item_uid = @LinkedItemUID
                    AND linked_item_type = @LinkedItemType
                    AND CAST(created_on AS DATE) = @NotificationDate
                    AND request_uid = @RequestUID";

            var selectParameters = new Dictionary<string, object>
            {
                { "@LinkedItemUID", linkeditemuid },
                { "@LinkedItemType", linkeditemtype },
                { "@NotificationDate", ondate },
                { "@RequestUID", ReqUID }
            };

            var dataSet = await ExecuteQueryDataSetAsync(selectQuery, selectParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                DataRow logRecord = dataSet.Tables[0].Rows[0];

                string updateQuery = @"
                UPDATE tbl_fb_notification_sent
                SET notification_title = @NotificationTitle,
                    notification_body = @NotificationBody,
                    modified_time = @ModifiedTime
                WHERE linked_item_uid = @LinkedItemUID
                    AND linked_item_type = @LinkedItemType
                    AND CAST(created_on AS DATE) = @NotificationDate
                    AND request_uid = @RequestUID";

                var updateParameters = new Dictionary<string, object>
            {
                { "@LinkedItemUID", linkeditemuid },
                { "@LinkedItemType", linkeditemtype },
                { "@NotificationTitle", title },
                { "@NotificationBody", body },
                { "@NotificationDate", ondate },
                { "@ModifiedTime", ondate },
                { "@RequestUID", ReqUID }
            };

                retval = await ExecuteNonQueryAsync(updateQuery, updateParameters);
            }
            else
            {
                string insertQuery = @"
                INSERT INTO tbl_fb_notification_sent (request_uid, linked_item_uid, linked_item_type, notification_title, notification_body, 
                modified_time, created_on)
                VALUES (@RequestUID, @LinkedItemUID, @LinkedItemType, @NotificationTitle, @NotificationBody, @ModifiedTime, @NotificationDate)";

                var insertParameters = new Dictionary<string, object>
                {
                    { "@RequestUID", ReqUID },
                    { "@LinkedItemUID", linkeditemuid },
                    { "@LinkedItemType", linkeditemtype },
                    { "@NotificationTitle", title },
                    { "@NotificationBody", body },
                    { "@NotificationDate", ondate },
                    { "@ModifiedTime", ondate }
                };

                retval = await ExecuteNonQueryAsync(insertQuery, insertParameters);
            }

            if (retval != null)
            {
                return retval;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred in logging linkeditemuid: {linkeditemuid}, linkeditemtype: {linkeditemtype}");
            return -1;
        }
        return retval;
    }

    public async Task<string> MoveLogToRepost(string messageUID)
    {
        try
        {
            string selectQuery = @"
            SELECT * FROM tbl_logs WHERE uid = @MessageUID";

            var selectParameters = new Dictionary<string, object>
            {
                { "@MessageUID", messageUID }
            };
            var dataSet = await ExecuteQueryDataSetAsync(selectQuery, selectParameters);

            /*if (logRecord != null)
            {
                string insertQuery = @"
                    INSERT INTO ""TblRepost"" (""UID"", ""LinkedItemUID"", ""LinkedItemType"", ""UserCode"", ""CustomerCode"", ""RequestCreatedTime"", 
                    ""RequestPosted2ApiTime"", ""RequestReceivedByApiTime"", ""RequestSent2ServiceTime"", ""RequestReceivedByServiceTime"",
                    ""RequestPostedToDBTime"", ""NotificationSentTime"", ""NotificationReceivedTime"", ""RequestSent2LogApiTime"", ""IsFailed"",
                    ""Comments"", ""ModifiedTime"", ""Status"")
                    VALUES (@UID, @LinkedItemUID, @LinkedItemType, @UserCode, @CustomerCode, @RequestCreatedTime, @RequestPosted2ApiTime, @RequestReceivedByApiTime,
                    @RequestSent2ServiceTime, @RequestReceivedByServiceTime, @RequestPostedToDBTime, @NotificationSentTime, @NotificationReceivedTime, 
                    @RequestSent2LogApiTime, @IsFailed, @Comments, @ModifiedTime, @Status)
                    RETURNING ""UID""";




                var parameters = new Dictionary<string, object>
            {
                { "@UID", logRecord["UID"] },
                { "@LinkedItemUID", logRecord["LinkedItemUID"] },
                { "@LinkedItemType", logRecord["LinkedItemType"] },
                { "@UserCode", logRecord["UserCode"] },
                { "@CustomerCode", logRecord["CustomerCode"] },
                { "@RequestCreatedTime", logRecord["RequestCreatedTime"] },
                { "@RequestPosted2ApiTime", logRecord["RequestPosted2ApiTime"] },
                { "@RequestReceivedByApiTime", logRecord["RequestReceivedByApiTime"] },
                { "@RequestSent2ServiceTime", logRecord["RequestSent2ServiceTime"] },
                { "@RequestReceivedByServiceTime", logRecord["RequestReceivedByServiceTime"] },
                { "@RequestPostedToDBTime", logRecord["RequestPostedToDBTime"] },
                { "@NotificationSentTime", logRecord["NotificationSentTime"] },
                { "@NotificationReceivedTime", logRecord["NotificationReceivedTime"] },
                { "@RequestSent2LogApiTime", logRecord["RequestSent2LogApiTime"] },
                { "@IsFailed", logRecord["IsFailed"] },
                { "@Comments", logRecord["Comments"] },
                { "@ModifiedTime", logRecord["ModifiedTime"] },
                { "@Status", logRecord["Status"] }
            };

                //var result = await ExecuteScalerResultAsync(insertQuery, parameters);
                var result = await ExecuteScalarAsync<string>(insertQuery, parameters);

                if (result != null)
                    return result.ToString();
            }*/
            string insertQuery = @"
                    INSERT INTO tbl_repost (uid, linked_item_uid, linked_item_type, user_code, customer_code, request_created_time, 
                    request_posted_to_api_time, request_received_by_api_time, request_sent_to_service_time, request_received_by_service_time,
                    request_posted_to_db_time, notification_sent_time, notification_received_time, request_sent_to_log_api_time, is_failed,
                    comments, modified_time, status)
                    VALUES (@UID, @LinkedItemUID, @LinkedItemType, @UserCode, @CustomerCode, @RequestCreatedTime, @RequestPosted2ApiTime, @RequestReceivedByApiTime,
                    @RequestSent2ServiceTime, @RequestReceivedByServiceTime, @RequestPostedToDBTime, @NotificationSentTime, @NotificationReceivedTime, 
                    @RequestSent2LogApiTime, @IsFailed, @Comments, @ModifiedTime, @Status)
                    RETURNING ""UID""";
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                DataRow logRecord = dataSet.Tables[0].Rows[0];

                var insertParameters = new Dictionary<string, object>
                {
                    { "@UID", logRecord["UID"] },
                    { "@LinkedItemUID", logRecord["LinkedItemUID"] },
                    { "@LinkedItemType", logRecord["LinkedItemType"] },
                    { "@UserCode", logRecord["UserCode"] != DBNull.Value ? logRecord["UserCode"] : null },
                    { "@CustomerCode", logRecord["CustomerCode"] != DBNull.Value ? logRecord["CustomerCode"] : null },
                    { "@RequestCreatedTime", logRecord["RequestCreatedTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestCreatedTime"]) : null },
                    { "@RequestPosted2ApiTime", logRecord["RequestPosted2ApiTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestPosted2ApiTime"]) : null },
                    { "@RequestReceivedByApiTime", logRecord["RequestReceivedByApiTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestReceivedByApiTime"]): null },
                    { "@RequestSent2ServiceTime", logRecord["RequestSent2ServiceTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestSent2ServiceTime"]) : null },
                    { "@RequestReceivedByServiceTime", logRecord["RequestReceivedByServiceTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestReceivedByServiceTime"]) : null },
                    { "@RequestPostedToDBTime", logRecord["RequestPostedToDBTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestPostedToDBTime"]) : null },
                    { "@NotificationSentTime", logRecord["NotificationSentTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["NotificationSentTime"]) : null },
                    { "@NotificationReceivedTime", logRecord["NotificationReceivedTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["NotificationReceivedTime"]) : null },
                    { "@RequestSent2LogApiTime", logRecord["RequestSent2LogApiTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["RequestSent2LogApiTime"]) : null },
                    { "@IsFailed", logRecord["IsFailed"] != DBNull.Value ? logRecord["IsFailed"] : null },
                    { "@Comments", logRecord["Comments"] != DBNull.Value ? logRecord["Comments"] : null },
                    { "@ModifiedTime", logRecord["ModifiedTime"] != DBNull.Value ? Convert.ToDateTime(logRecord["ModifiedTime"]) : null },
                    { "@Status", logRecord["Status"] != DBNull.Value ? logRecord["Status"] : null}
                };

                var result = await ExecuteScalarAsync<string>(insertQuery, insertParameters);

                if (result != null && result.Length > 0)
                    return result.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in MoveLogToRepost for messageuid {messageUID}");
        }
        return null;
    }


}