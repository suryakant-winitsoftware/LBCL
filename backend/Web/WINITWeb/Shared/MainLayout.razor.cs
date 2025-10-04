using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using System.Resources;
using System.Security.Claims;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using WinIt.BreadCrum.Interfaces;
using WinIt.JSHelpers;

namespace WinIt.Shared
{

    public partial class MainLayout
    {
        private bool IsShow { get; set; } = false;//extra
        private bool IsLoaded = false;
        public ErrorBoundary MainErrorBoundary { get; set; } = default!;
        bool isNavigating = false;
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            NavigationManager.LocationChanged += RequestCleanup;
            _navigationhistory.RecordNavigation(NavigationManager.Uri);
            //Check for User authentication    
            if (_navigationManager.Uri != _navigationManager.BaseUri)
            {
                await CheckUserLoggedInOrNotIfInThenGetMasterData();
            }
            _loadingService.HideLoading();
            IsLoaded = true;
            //await _js.InitializeInactivityTimer(DotNetObjectReference.Create(this));
        }
        protected void RequestCleanup(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            _navigationhistory.RecordNavigation(e.Location);
            _JSRuntime.InvokeVoidAsync("clearNetworkRequests");
            CleanupService.RequestCleanup();
        }

        void ResolveError(bool shouldNavigateToHome = false)
        {
            if (shouldNavigateToHome)
            {
                isNavigating = true;
                NavigationManager.LocationChanged += NavigationManager_LocationChanged;
                NavigationManager.NavigateTo(_navigationManager.BaseUri);
            }
            else
            {
                MainErrorBoundary.Recover();
            }
        }

        private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            if (isNavigating)
            {
                MainErrorBoundary.Recover(); // Recover the error boundary
                isNavigating = false;
                NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
            }
        }
        private void Close(bool popUp)
        {
            IsShow = !IsShow;
        }

        private async Task LoadResources(object sender, string culture)
        {
            string? savedCulture = await _localStorageService.GetItem<string>("RememberedCulture");
            if (!string.IsNullOrEmpty(savedCulture))
            {
                _languageService.SelectedCulture = savedCulture;
            }
            else
            {
                _languageService.SelectedCulture = "en-GB";
            }
            //will set default laguage for system

            CultureInfo cultureInfo = new CultureInfo(_languageService.SelectedCulture);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            //CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture);
            //ResourceManager resourceManager = new ResourceManager("WinIt.LanguageResources.IndexPage.index", typeof(Index).Assembly);
            //Localizer = new CustomStringLocalizer<Index>(resourceManager, cultureInfo);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }

        private async Task CheckUserLoggedInOrNotIfInThenGetMasterData()
        {
            // Get the current authentication state
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;

            // Check if the user is authenticated
            if (user.Identity?.IsAuthenticated == true)
            {
                // Retrieve the UserData claim
                string? userID = user.FindFirst(ClaimTypes.UserData)?.Value;
                if (!string.IsNullOrEmpty(userID))
                {
                    LoadResources(null, _languageService.SelectedCulture);
                    await _userMasterDataViewModel.GetUserMasterData(userID);
                    await _iCommonMasterData.PageLoadevents();

                }
            }
            else
            {
                // Remove the token if the user is not authenticated
                if (!_navigationManager.Uri.Contains("selfRegistration"))
                {
                    // Remove token and redirect to home if the page is not selfRegistration
                    await _localStorageService.RemoveItem("token");
                    _navigationManager.NavigateTo(_navigationManager.BaseUri);
                }

            }
        }
    }

}
