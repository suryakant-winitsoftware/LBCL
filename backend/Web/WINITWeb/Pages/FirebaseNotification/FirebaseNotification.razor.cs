using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.FirebaseNotification
{
    public partial class FirebaseNotification 
    {
        public string message { get; set; } 
        public string deviceId { get; set; }
        private async void SendMessage()
        {
            await SendFirebaseNotification();
        }
        protected override async Task OnInitializedAsync()
        {
            await loaddata();
            LoadResources(null, _languageService.SelectedCulture);

        }
       
        private async Task loaddata()
        {
            throw new NotImplementedException();
        }

        private async Task SendFirebaseNotification()
        {
            // Your Firebase server key obtained from Firebase console
            string serverKey = "AAAAwYPNGbc:APA91bEAuqqqt4E9F4ffXpNdC34kQMVV3aHjDTsfADT16hVKp7VIjMJagbgjtLIsSuqNejHF_JVZ4ov8bDQ18qAOoZcwoM5NNvAqlCxNyhRjck_sFJ1h_mU9LjrEffzAtleyUAqK4skr";
            

            // FCM endpoint
            string fcmUrl = "https://fcm.googleapis.com/fcm/send";

            // Device token of the recipient device
            string deviceToken = "dBJtpexHQTi8vvJBnTa9b-:APA91bGxAFJTMoiDjNvU3Hf_4WfLC5ktGoegvRuFg8SGTOQjDZ6vBqU-LjhVEMnLbJ-TZu8TSno9UKKusFoeJR4hgclfj0Do0Zblk7g-1zIUmtm7iYJJhR8k1uf6JqlONP7M4t7RDIu-";

            if (!string.IsNullOrEmpty(deviceId))
            {
                deviceToken = deviceId;
            }
            // Notification content
            string notificationTitle = "Test Notification";
            string notificationBody = message;

            string notificationName = "Notification";
            string relativePath = "testcounter";

            // Create notification payload
            var data = new
            {
                to = deviceToken,
                notification = new
                {
                    title = notificationTitle,
                    body = notificationBody
                },
                data = new
                {
                    notificationName = notificationName,
                    relativePath = relativePath
                }
            };

            // Convert notification payload to JSON
            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            // Create HTTP request
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, fcmUrl))
            {
                httpRequest.Headers.TryAddWithoutValidation("Authorization", $"key={serverKey}");
                httpRequest.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // Send HTTP request
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.SendAsync(httpRequest);

                    // Check if request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(@Localizer["notification_sent_successfully."]);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send notification. Status code: {response.StatusCode}");
                    }
                }
            }
        }
    }
}
