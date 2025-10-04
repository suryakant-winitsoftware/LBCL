using Hangfire;
using SMSApp.Constants;
using Winit.Modules.SMS.BL.Interfaces;
using Winit.Modules.Email.BL.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;
using System.Net;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Email.Model.Classes;

namespace SMSApp
{
    public class ProcessJobs
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISMSBL _SMSBL;
        private readonly IEmailBL _EmailBL;
        public ProcessJobs(IServiceProvider serviceProvider, ISMSBL sMSBL, IEmailBL emailBL)
        {
            _serviceProvider = serviceProvider;
            _SMSBL = sMSBL;
            _EmailBL = emailBL;
        }

        public static void RegisterJobs(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, ISMSBL sMSBL, IEmailBL emailBL)
        {
            var jobInstance = new ProcessJobs(serviceProvider, sMSBL, emailBL);

            recurringJobManager.AddOrUpdate(
                "SendSMS",
                () => jobInstance.SMSOtpJob(),
                 CronExpressions.EveryTenMinutes);

            recurringJobManager.AddOrUpdate(
                "SendMail",
                () => jobInstance.EmailMsg(),
                 CronExpressions.EveryFiveHours);

            recurringJobManager.AddOrUpdate(
                "CheckAndCancelLongRunningJobs",
                () => jobInstance.CheckAndCancelLongRunningJobs(),
                 CronExpressions.EveryMinute);
        }
        public async Task SMSOtpJob()
        {
            try
            {
                List<ISmsRequestModel> smsDetails = await _SMSBL.GetSmsRequestFromJob();
                //foreach (var item in smsDetails)
                //{
                //   //SMSApiResponse sMSApiResponse = await _SMSBL.SendOtp(item.Receivers, item.Content);
                //    if(sMSApiResponse.status.code == ((int)HttpStatusCode.OK).ToString())
                //    {
                //        //await _SMSBL.UpdateSmsRequest(new SmsModel
                //        //{
                //        //    UID = item.UID,
                //        //    RequestStatus = "Sent",
                //        //    SentTime = DateTime.UtcNow,
                //        //    ErrorDetails = "N/A",
                //        //    RetryCount = 0,
                //        //    ResponseCode = sMSApiResponse.status.code,
                //        //    ResponseTime = DateTime.Parse(sMSApiResponse.time),
                //        //    ResponseStatus = sMSApiResponse.status.code,
                //        //    ResponseMessage = sMSApiResponse.ackid,
                //        //    BatchId = ""
                //        //});
                //    }
                //    else
                //    {
                //        //await _SMSBL.UpdateSmsRequest(new SmsModel
                //        //{
                //        //    UID = item.UID,
                //        //    RequestStatus = "Failed",
                //        //    SentTime = DateTime.UtcNow,
                //        //    ErrorDetails = sMSApiResponse.status.desc,
                //        //    RetryCount = item.RetryCount + 1,
                //        //    ResponseCode = sMSApiResponse.status.code,
                //        //    ResponseTime = DateTime.Parse(sMSApiResponse.time),
                //        //    ResponseStatus = sMSApiResponse.status.code,
                //        //    ResponseMessage = sMSApiResponse.ackid ?? "N/A",
                //        //    BatchId = ""
                //        //});
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task EmailMsg()
        {
            try
            {
                //List<IEmailRequestModel> emailDetails = await _EmailBL.GetEmailRequestFromJob();
                //foreach (var item in emailDetails)
                //{
                //   //bool sMSApiResponse = await _EmailBL.SendEmail(item);
                //    if(sMSApiResponse)
                //    {
                //        //await _EmailBL.UpdateEmailRequest(new EmailModel
                //        //{
                //        //    UID = item.UID,
                //        //    RequestStatus = "Sent",
                //        //    ErrorDetails = "N/A",
                //        //    RetryCount = 0
                //        //});
                //    }
                //    else
                //    {
                //        //await _EmailBL.UpdateEmailRequest(new EmailModel
                //        //{
                //        //    UID = item.UID,
                //        //    RequestStatus = "Failed",
                //        //    ErrorDetails = "Failed to update",
                //        //    RetryCount = item.RetryCount + 1
                //        //});
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public async Task CheckAndCancelLongRunningJobs()
        {
            // Get all processing jobs
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var processingJobs = monitoringApi.ProcessingJobs(0, 100);
            foreach (var job in processingJobs)
            {
                var jobDetails = monitoringApi.JobDetails(job.Key);
                if (jobDetails == null)
                {
                    continue; // Skip if job details could not be found
                }

                // Calculate job duration
                var jobDuration = DateTime.UtcNow - jobDetails.CreatedAt;
                if (jobDuration > TimeSpan.FromMinutes(5)) // Timeout limit of 10 minutes
                {

                    BackgroundJob.Delete(job.Key); // Cancel the job
                }
            }
        }
    }
}
