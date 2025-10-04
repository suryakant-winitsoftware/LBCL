using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WINITMobile.State;
using WINITMobile.Pages.Base;
using WINITMobile.Pages;
using Firebase.Messaging;
using Android.Gms.Extensions;
using WINITMobile.Platforms.Android;
using Android.Content;
using Android.Webkit;
using Java.Interop;
using Microsoft.JSInterop;
using Microsoft.Maui.Controls;
using Java.Lang;
using static Android.Provider.ContactsContract.CommonDataKinds;
using WINITMobile.Data;

namespace WINITMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private NavigationService _navigationService;
        //private BaseComponentBase _baseComponentBase { get; set; }
        private WINITMobile.State.IBackButtonHandler _backButtonHandler { get; set; }
        private IForegroundService _foregroundService;

        public MainActivity()
        {
            _navigationService = IPlatformApplication.Current.Services.GetRequiredService<NavigationService>();
            _backButtonHandler = IPlatformApplication.Current.Services.GetRequiredService<WINITMobile.State.IBackButtonHandler>();
            _foregroundService = IPlatformApplication.Current.Services.GetService<IForegroundService>();

        }
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialize Firebase
            Firebase.FirebaseApp.InitializeApp(this);

            await GetFirebaseToken();
            try
            {
                if (_foregroundService != null && !IsServiceRunning())
                {
                    System.Diagnostics.Debug.WriteLine("OnCreate: Starting foreground service");

                    _foregroundService.Start();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ForegroundService Start Error: {ex.Message}");
            }
        }
        //[JSInvokable]
        [JSInvokableAttribute("WINITMobile.MainActivity.GetFirebaseToken")]
        public static async Task<string> GetFirebaseToken()
        {
            string token = string.Empty;
            try
            {
                token = (string)await FirebaseMessaging.Instance.GetToken();
                //ISharedPreferences prefs = MauiApplication.Context.GetSharedPreferences("winitmobile", FileCreationMode.Private);
                //ISharedPreferencesEditor editor = prefs.Edit();
                //editor.PutString("FirebaseToken", token);
                //editor.Apply();
            }
            catch(System.Exception ex)
            {

            }
            return token;
        }

        //public async Task<string> GetFirebaseToken()
        //{
        //    string token = string.Empty;
        //    try
        //    {
        //        token = (string)await FirebaseMessaging.Instance.GetToken();
        //        //ISharedPreferences prefs = MauiApplication.Context.GetSharedPreferences("winitmobile", FileCreationMode.Private);
        //        //ISharedPreferencesEditor editor = prefs.Edit();
        //        //editor.PutString("FirebaseToken", token);
        //        //editor.Apply();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return token;
        //}
        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);
            FirebaseMessaging.Instance.SubscribeToTopic("sync_message");
        }


        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.KeyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            { 
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if(_backButtonHandler != null)
                        {
                            if (!await _backButtonHandler.HandleBackClickAsync())
                            {
                                base.DispatchKeyEvent(e);
                            }
                        }
                        
                    });
                    return true;
                
            }
            // If the custom navigation stack is empty or the Back key is not pressed, fall back to default behavior
            return base.DispatchKeyEvent(e);
        }
        //public override bool DispatchKeyEvent(KeyEvent e)
        //{
        //    if (e.KeyCode == Keycode.Back && e.Action == KeyEventActions.Down)
        //    {
        //        if (_navigationService.pageList.Count > 0)
        //        {
        //            MainThread.BeginInvokeOnMainThread(async () =>
        //            {
        //                await _navigationService.OnBackClick();
        //            });
        //            return true;
        //        }
        //    }
        //    // If the custom navigation stack is empty or the Back key is not pressed, fall back to default behavior
        //    return base.DispatchKeyEvent(e);
        //}
        protected override void OnResume()
        {
            base.OnResume();
            //// Call JavaScript function through WebView interop
            //webVi
            //webView.EvaluateJavascript("window.blazorHybrid.onAppResume()");

            //FirebaseToken = await jsRuntime.InvokeAsync<string>("androidInterop.getFirebaseToken", Array.Empty<object>());
            //// Call JavaScript function through WebView interop
            //window.androidInterop.EvaluateJavascript("window.blazorHybrid.onAppResume()");
        }
        private bool IsServiceRunning()
        {
            var manager = (ActivityManager)GetSystemService(Context.ActivityService);
            if (manager != null)
            {
                foreach (var service in manager.GetRunningServices(int.MaxValue))
                {
                    if (typeof(ForegroundServiceAndroid).Name.Equals(service.Service.ClassName))
                    {
                        System.Diagnostics.Debug.WriteLine("IsServiceRunning: Service is running");
                        return true;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("IsServiceRunning: Service is not running");
            return false;
        }
    }
}


