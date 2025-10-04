using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WINITMobile.Pages.Startday
{
    partial class StartdayPreRequisties
    {

        protected bool isSynchronized { get; set; } = false;
        protected bool isSyncInProgress { get; set; } = false;
        protected bool IsInternetWifi { get; set; } = false;
        protected bool IsInternetOn { get; set; } = false;
        protected bool IsMobileInternetOn { get; set; } = false;
        protected bool IsCheckingWifi { get; set; } = false;
        protected bool IsCheckingInternet { get; set; } = false;
        protected bool IsGpsOn { get; set; } = false;
        protected bool IsCheckingGps { get; set; } = false;
        protected double BatteryPercentage { get; set; } = 0;
        protected decimal downloadSpeed { get; set; }
        protected decimal uploadSpeed { get; set; }
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

        protected override void OnInitialized()
        {
            _backbuttonhandler.SetCurrentPage(this);
            //  event
           // Connectivity.ConnectivityChanged += OnConnectivityChanged;
            if (_viewmodel?.UserJourney != null && _appUser?.SelectedBeatHistory != null)
            {
                _viewmodel.UserJourney.JobPositionUID = _appUser.SelectedBeatHistory.JobPositionUID;
                _viewmodel.UserJourney.EmpUID = _appUser.Emp?.UID;
                _viewmodel.UserJourney.BatteryPercentageTarget = 50;
            }
            LoadResources(null, _languageService.SelectedCulture);

        }
        protected override async Task OnInitializedAsync()
        {
            await SetTopBar();
            await CheckSync();
            await CheckInternetAndWifi();
            await CheckMobileInternet();
            await CheckGPS();
            await UpdateBatteryStatus();
            //await CheckPrinterstatus();
            // await GetDownloadAndUploadSpeed();
            await GetNetworkSpeedAsync();

        }

        private async Task CheckPrinterstatus()
        {
            var printerTest = new WINITMobile.Pages.Demo.Printer.PrinterTest();
            await printerTest.GetConnectedPrinter();
        }

        private async Task GetNetworkSpeedAsync()
        {

            var dlSpeed = await GetDownloadSpeedAsync();
            // downloadSpeed = Math.Round(dlSpeed, 2);

            downloadSpeed = Winit.Shared.CommonUtilities.Common.CommonFunctions.RoundForSystem(dlSpeed, 3);


            var ulspeed = await GetUploadSpeedAsync();
            uploadSpeed = Winit.Shared.CommonUtilities.Common.CommonFunctions.RoundForSystem(ulspeed, 3);
            StateHasChanged();
        }


        public async Task<double> GetDownloadSpeedAsync()
        {
            try
            {
                return await GetDownloadSpeedAsync("http://speed.speedtest.net/speedtest-files/10mb.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting network speed: {ex.Message}");
                return 0;
            }
        }
        public async Task<double> GetUploadSpeedAsync()
        {
            try
            {
                return await GetUploadSpeedAsync("http://upload.speedtest.net", new byte[1_000_000]); // sample 1 MB
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting upload speed: {ex.Message}");
                return 0;
            }
        }


        private async Task<double> GetDownloadSpeedAsync(string downloadTestUrl)
        {
            using (var _httpClient = new HttpClient())
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(downloadTestUrl);
                await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                var downloadSpeedInBitsPerSecond = (response.Content.Headers.ContentLength * 8.0) / stopwatch.Elapsed.TotalSeconds;
                return (double)downloadSpeedInBitsPerSecond / 1_000_000;
            }
        }
        private async Task<double> GetUploadSpeedAsync(string uploadTestUrl, byte[] data)
        {
            int retryCount = 3;
            double uploadSpeed = 0;
            int delaySeconds = 5;

            while (retryCount > 0)
            {
                try
                {
                    using (var _httpClient = new HttpClient())
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var content = new ByteArrayContent(data);
                        var response = await _httpClient.PostAsync(uploadTestUrl, content);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {

                            if (int.TryParse(response.Headers.GetValues("Retry-After").FirstOrDefault(), out int retryAfter))
                            {
                                Console.WriteLine($"Rate limit reached. Retrying in {retryAfter} seconds.");
                                await Task.Delay(retryAfter * 1000);
                            }
                            else
                            {
                                Console.WriteLine($"Rate limit reached. Retrying in {delaySeconds} seconds.");
                                await Task.Delay(delaySeconds * 1000);
                                delaySeconds *= 2;
                            }
                            retryCount--;
                            continue;
                        }

                        await response.Content.ReadAsStringAsync();
                        stopwatch.Stop();

                        var uploadSpeedInBitsPerSecond = (data.Length * 8.0) / stopwatch.Elapsed.TotalSeconds;
                        uploadSpeed = uploadSpeedInBitsPerSecond / 1_000_000;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting upload speed (Retry {retryCount}): {ex.Message}");
                    retryCount--;
                    if (retryCount == 0)
                    {
                        return 0;
                    }
                    await Task.Delay(2000);
                }
            }

            return uploadSpeed;
        }

        protected async Task CheckSync()
        {

            isSyncInProgress = true;
            StateHasChanged();

            await Task.Delay(2000);


            // isSynchronized = new Random().Next(2) == 0;
            isSynchronized = true;
            isSyncInProgress = false;
            if (_viewmodel != null)
            {
                if (_viewmodel.UserJourney != null)
                {
                    _viewmodel.UserJourney.IsSynchronizing = true;
                }

            }

            StateHasChanged();
        }
        private async Task UpdateBatteryStatus()
        {
            var battery = Battery.Default;
            if (battery != null)
            {
                BatteryPercentage = await GetBatteryPercentage();
                // _viewmodel.UserJourney.BatteryPercentageAvailable = BatteryPercentage;
            }
            StateHasChanged();
        }
        private async Task<double> GetBatteryPercentage()
        {
            var battery = Battery.Default;
            if (battery != null)
            {
                // return Math.Round(battery.ChargeLevel * 100);
                return battery.ChargeLevel * 100;
            }
            return 0;
        }
        private string GetBatteryColor(double chargeLevel)
        {
            if (chargeLevel >= 0.75)
            {
                return "success";
            }
            else if (chargeLevel >= 0.3)
            {
                return "warning";
            }
            else
            {
                return "danger";
            }
        }

        //private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        //{

        //    IEnumerable<ConnectionProfile> profiles = e.ConnectionProfiles;
        //    IsInternetWifi = profiles.Contains(ConnectionProfile.WiFi);


        //    IsMobileInternetOn = e.NetworkAccess == NetworkAccess.Internet;


        //    StateHasChanged();
        //}
        protected async Task SetTopBar()
        {
            WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
            {
                TopLabel = @Localizer["customer_call_procedure"],

                UIButton1 = null,

            };
            await Btnname.InvokeAsync(buttons);
        }
      
        private async Task CheckInternetAndWifi()
        {
            IsCheckingWifi = true;
            IsCheckingInternet = true;
            StateHasChanged();


            IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;
            IsInternetWifi = profiles.Contains(ConnectionProfile.WiFi);

            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            IsInternetOn = accessType == NetworkAccess.Internet;

            if (_viewmodel != null)
            {
                if (_viewmodel.UserJourney != null)
                {
                    if (IsInternetWifi)
                    {
                        _viewmodel.UserJourney.InternetType = "Wifi";
                    }
                    else
                    {
                        _viewmodel.UserJourney.InternetType = "MobileNetwork";
                    }
                }

            }
            IsCheckingWifi = false;
            IsCheckingInternet = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private async Task CheckMobileInternet()
        {
            IsCheckingInternet = true;
            StateHasChanged();

            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            IsMobileInternetOn = accessType == NetworkAccess.Internet;

            IsCheckingInternet = false;
            if (_viewmodel != null)
            {
                if (_viewmodel.UserJourney != null)
                {
                    _viewmodel.UserJourney.HasMobileNetwork = accessType == NetworkAccess.Internet;
                }

            }

            StateHasChanged();
            await Task.CompletedTask;
        }

        //public void Dispose()
        //{
        //    Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        //}
        private async Task CheckGPS()
        {
            IsCheckingGps = true;
            StateHasChanged();

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status == PermissionStatus.Granted)
                    {
                        IsGpsOn = status == PermissionStatus.Granted;
                    }
                }
                else if (status == PermissionStatus.Granted)
                {
                    IsGpsOn = status == PermissionStatus.Granted;

                    if (_viewmodel?.UserJourney != null)
                    {
                        _viewmodel.UserJourney.IsLocationEnabled = IsGpsOn;
                    }

                }
            }
            catch (FeatureNotSupportedException ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["gps_not_supported"], ex.Message);

            }
            catch (PermissionException ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["permission_not_granted"], ex.Message);
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], ex.Message);
            }

            IsCheckingGps = false;

            IsGpsOn = true;
            if (_viewmodel != null)
            {
                if (_viewmodel.UserJourney != null)
                {
                    _viewmodel.UserJourney.IsLocationEnabled = IsGpsOn;
                }

            }
            StateHasChanged();
            await Task.CompletedTask;
        }

        protected async Task CheckAllPermissions()
        {
            if (!isSynchronized || IsCheckingWifi || IsCheckingInternet || IsCheckingGps)
            {
                await _alertService.ShowConfirmationAlert(@Localizer["attention"], @Localizer["please_wait_until_all_prerequisites_are_checked"], null, null, null);
                return;
            }

            if (!IsInternetWifi && !IsMobileInternetOn)
            {
                await _alertService.ShowConfirmationAlert(@Localizer["attention"], @Localizer["please_connect_to_the_internet_before_proceeding"], null, null, null);
                return;
            }

            if (!IsGpsOn)
            {
                await _alertService.ShowConfirmationAlert(@Localizer["attention"], @Localizer["please_enable_gps_before_proceeding"], null, null, null);
                return;
            }

            if (Battery.Default == null || Battery.Default.ChargeLevel < 0.1)
            {
                await _alertService.ShowConfirmationAlert(@Localizer["attention"], @Localizer["please_ensure_that_the_battery_is_at_least_20%_charged_before_proceeding"], null, null, null);
                return;
            }


            //NavigateTo("/Attendence");
            _navigationManager.NavigateTo("/Attendence");
        }
        public override async Task OnBackClick()
        {
            _navigationManager.NavigateTo("DashBoard");
        }
    }
}
