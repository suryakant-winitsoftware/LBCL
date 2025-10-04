using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using WINITMobile.Data;

namespace WINITMobile.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
    public class ForegroundServiceAndroid : Service, IForegroundService
    {
        private const string CHANNEL_ID = "FarmleyForegroundServiceChannel";
        private const int NOTIFICATION_ID = 1001;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Service created");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Service destroyed");
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Farmley Service Channel",
                    NotificationImportance.High
                )
                {
                    Description = "Foreground service for notifications"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
                System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Notification channel created");
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Service started");

            CreateNotificationChannel();

            string inputText = intent.GetStringExtra("inputExtra") ?? "Foreground service running...";

            var notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("Farmley")
                .SetContentText(inputText)
                .SetSmallIcon(Resource.Mipmap.appicon) // Use your app icon
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetOngoing(true)
                .Build();

            StartForeground(NOTIFICATION_ID, notification);
            System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Notification created and service started");

            return StartCommandResult.NotSticky;
        }

        public void Start()
        {
            var context = Platform.AppContext; // Use AppContext instead of CurrentActivity
            if (context != null)
            {
                System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Starting service");
                var intent = new Intent(context, typeof(ForegroundServiceAndroid));
                intent.PutExtra("inputExtra", "Service Running");
                AndroidX.Core.Content.ContextCompat.StartForegroundService(context, intent);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Context is null in Start()");
            }
        }

        public void Stop()
        {
            var context = Platform.AppContext; // Use AppContext instead of CurrentActivity
            if (context != null)
            {
                System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Stopping service");
                var intent = new Intent(context, typeof(ForegroundServiceAndroid));
                context.StopService(intent);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ForegroundServiceAndroid: Context is null in Stop()");
            }
        }
    }
}