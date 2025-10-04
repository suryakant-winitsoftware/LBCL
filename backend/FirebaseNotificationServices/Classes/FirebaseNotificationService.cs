using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using DBServices.Interfaces;
using FirebaseNotificationServices.Interfaces;

namespace FirebaseNotificationServices.Classes
{
    public class FirebaseNotificationService: IFirebaseNotificationService
    {
        private string _fcmToken; // FCM token for sending notifications
        private readonly ILogger<FirebaseNotificationService> _logger;
        private string step = "Step5";
        private readonly IDBService _dBService;
        public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, IDBService dBService)
        {
            _logger = logger;
            _dBService = dBService;

            GoogleCredential credentials = GoogleCredential.FromFile(@"fb_config1.json"); FirebaseApp.Create(new AppOptions
            {
                Credential = credentials,
            });

        }

        public void SetFCMToken(string fcmToken)
        {
            _fcmToken = fcmToken;
        }


        public async Task SendNotificationAsync(string title, string body, string token)
        {
            try
            {
                _logger.LogInformation("\n");
                if (string.IsNullOrEmpty(_fcmToken ?? token))
                {
                    _logger.LogWarning("FCM token is not set. Skipping sending notification.");
                    return;
                }

                Message message = new Message
                {
                    Token = token ?? _fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "ResponseTitle", title },
                        { "ResponseBody", body }
                    }
                };
                _logger.LogInformation("\n");
                _logger.LogInformation("FCM token is :- " + token);
                _logger.LogInformation("\n");
                _logger.LogInformation("Sending Message :- " + body);
                _logger.LogInformation("\n");
                var resp = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation(title + "FB notification response :- " + resp);
                _logger.LogInformation("\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification.");
                throw;
            }
            _logger.LogInformation("-----------------------------\n");
            //step 5
        }
        public async Task SendNotificationAsync(string messageuid, string title, string body, string token)
        {
            try
            {
                _logger.LogInformation("\n");
                _logger.LogInformation(step + " for " + messageuid);
                if (string.IsNullOrEmpty(_fcmToken ?? token))
                {
                    _dBService.UpdateLogByStepAsync(messageuid, step, false, false, "FCM token is not set. Skipping sending notification.");
                    _logger.LogWarning("FCM token is not set. Skipping sending notification.");
                    return;
                }

                Message message = new Message
                {
                    Token = token ?? _fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>
                    {
                        //{ "ResponseUID", messageuid },
                        { "ResponseTitle", title },
                        { "ResponseBody", body },
                        { "notificationName", "InternalUploadStatus" },
                        { "relativePath", "" }
                    }
                };
                _logger.LogInformation("\n");
                _logger.LogInformation("FCM token is :- " + token);
                _logger.LogInformation("\n");
                _logger.LogInformation("Sending Message :- " + body);
                _logger.LogInformation("\n");
                var resp = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _dBService.UpdateLogByStepAsync(messageuid, step, true, false, resp);
                _logger.LogInformation(title + "FB notification response :- " + resp);
                _logger.LogInformation("\n");
            }
            catch (Exception ex)
            {
                _dBService.UpdateLogByStepAsync(messageuid, step, false, false, ex.Message);
                _logger.LogError(ex, "Failed to send notification.");
            }
            _logger.LogInformation("-----------------------------\n");
        }
    }
}
