using Android.App;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common;
using static Android.Icu.Text.CaseMap;
using static Firebase.Messaging.RemoteMessage;

namespace WINITMobile.Platforms.Android
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class WINITFirebaseMessagingService : FirebaseMessagingService
    {
        public static event EventHandler<object> NavigationRequested;
        private bool _isChannelCreated = false;
        private IAndroidBlazorCommunicationBridge _androidBlazorCommunicationBridge;
        private NavigationManager _navigationManager;
        public WINITFirebaseMessagingService()
        {
            _androidBlazorCommunicationBridge = MauiApplication.Current.Services.GetRequiredService<IAndroidBlazorCommunicationBridge>();
        }

        //public static void Initialize(IAlertService alertService, NavigationManager navigationService)
        //{
        //    _alertService = alertService;
        //    _navigationService = navigationService;
        //}
        public override void OnMessageReceived(RemoteMessage message)
        {
            // Handle the received message
            // You can extract the notification payload from the RemoteMessage object
            // and display a notification or perform other actions
            ProcessMessage(message);

            //// Get the data payload
            //System.Collections.Generic.IDictionary<string, string> data = message.Data;
            //if (data != null)
            //{
            //    foreach (var entry in data)
            //    {
            //        string key = entry.Key;
            //        string value = entry.Value;
            //        // Process the data payload
            //    }
            //}
        }
        private async void ProcessMessage(RemoteMessage message)
        {
            RemoteMessage.Notification notification = message.GetNotification();
            if (notification != null)
            {
                string title = notification.Title;
                string body = notification.Body;
                string imageUrl = (string)notification.ImageUrl; // Assuming you receive the image URL from Firebase
                string notificationName = string.Empty;
                string relativePath = string.Empty;
                if (message.Data.ContainsKey("notificationName"))
                {
                    notificationName = message.Data["notificationName"];
                }
                if (message.Data.ContainsKey("relativePath"))
                {
                    relativePath = message.Data["relativePath"];
                }

                // Handle the notification based on its name
                switch (notificationName)
                {
                    case NotificationType.Notification:
                        // Show the notification
                        await SendNotification(title, body, imageUrl);
                        break;
                    case NotificationType.Alert:
                    case NotificationType.Navigate:
                    case NotificationType.NavigateWithAlert:
                        _androidBlazorCommunicationBridge.SendMessageToBlazor(title, body, imageUrl, notificationName, relativePath);
                        break;
                    case NotificationType.InternalUploadStatus:
                        _androidBlazorCommunicationBridge.SendMessageToBlazor(title, body, imageUrl, notificationName, relativePath);
                        break;
                    default:
                        await SendNotification(title, body, imageUrl);
                        break;
                }

                

            }
        }
        private async Task SendNotification(string title, string body, string imageUrl)
        {
            string channelId = "WINIT_NOTIFICATION";

            NotificationCompat.Builder builder;
            NotificationManager manager = (NotificationManager)GetSystemService(NotificationService);

            // Check if the Android version is 8.0 (API level 26) or higher
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Create a notification channel only if it hasn't been created before
                if (!_isChannelCreated)
                {
                    // Create a notification channel
                    string channelName = "WINIT Notification";
                    NotificationChannel channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default);
                    manager.CreateNotificationChannel(channel);
                    _isChannelCreated = true;
                }

                // Create a notification builder with the channel ID
                builder = new NotificationCompat.Builder(this, channelId)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetSmallIcon(Resource.Drawable.ic_notification)
                    .SetAutoCancel(true);
            }
            else
            {
                // Create a notification builder without a channel ID for older Android versions
                builder = new NotificationCompat.Builder(this)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetSmallIcon(Resource.Drawable.ic_notification)
                    .SetAutoCancel(true);
            }

            // Load the image if available
            if (!string.IsNullOrEmpty(imageUrl))
            {
                Bitmap bitmap = await GetBitmapFromUrl(imageUrl, 50, 50); // Implement this method to get Bitmap from URL
                if (bitmap != null)
                {
                    builder.SetLargeIcon(bitmap); // Set the large icon for the notification
                                                  // You can also use PictureStyle to display the image in the expanded notification view
                    builder.SetStyle(new NotificationCompat.BigPictureStyle().BigPicture(bitmap));
                }
            }
            // Show the notification
            manager.Notify(0, builder.Build());
        }
        private async Task<Bitmap> GetBitmapFromUrl(string url, int width, int height)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            if (stream != null)
                            {
                                // Load the image with specific dimensions
                                BitmapFactory.Options options = new BitmapFactory.Options
                                {
                                    InJustDecodeBounds = true // Only get the dimensions without loading the image into memory
                                };

                                await BitmapFactory.DecodeStreamAsync(stream, null, options);

                                // Calculate the inSampleSize to load a scaled down version of the image
                                options.InSampleSize = CalculateInSampleSize(options, width, height);

                                // Decode the image with the calculated inSampleSize
                                options.InJustDecodeBounds = false;

                                // Rewind the stream to read from the beginning
                                stream.Position = 0;

                                return await BitmapFactory.DecodeStreamAsync(stream, null, options);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions such as network errors or invalid URLs
                Console.WriteLine($"Error downloading image from URL: {ex.Message}");
            }

            return null; // Return null if unable to download or decode the image
        }

        // Calculate the sample size to load a scaled down version of the image
        private int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of the image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                // Calculate ratios of requested height and width to the raw height and width
                int heightRatio = (int) Math.Round((double)height / reqHeight);
                int widthRatio = (int) Math.Round((double)width / reqWidth);

                // Choose the smallest ratio as inSampleSize value, this will ensure a final image
                // with both dimensions larger than or equal to the requested dimensions
                inSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
            }

            return inSampleSize;
        }
    }
}
