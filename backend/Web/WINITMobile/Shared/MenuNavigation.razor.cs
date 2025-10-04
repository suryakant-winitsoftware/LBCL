using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;

namespace WINITMobile.Shared
{
    public partial class MenuNavigation
    {
        [Parameter] public EventCallback<bool> CloseMenu { get; set; }

        protected async override Task OnInitializedAsync()
        {
            _sideBarService.RefreshSideBar = RefreshScreen;
            // _sideBarService.OnSwipeDirection = OnSwipeGesture;
        }

        private async Task HandleCheckOutClicked()
        {
            //await OnCheckOutClicked.InvokeAsync();
            _sideBarService.OnCheckOutClick.Invoke();
        }

        public async Task returnpath(string path)
        {
            //_navigationManager.NavigateTo(path, true);
            if(!string.IsNullOrEmpty(path)) 
            NavigateTo(path);

            await CloseMenu.InvokeAsync(false);
        }
        private void HandleClick(string relativePath)
        {
            _navigationManager.NavigateTo(relativePath);
            CloseMenu.InvokeAsync(false);  // Update parent state (collapseNavMenu = false)
        }
        public async Task ToggleSubMenu(SubModule subModule)
        {
            if (subModule != null &&  string.IsNullOrEmpty(subModule.RelativePath))
            {
                subModule.IsSubSubMenuDisplay = !subModule.IsSubSubMenuDisplay;
            }
            else
            {
                if(subModule != null)
                {
                    _navigationManager.NavigateTo(subModule.RelativePath);
                }
            }
        }
        private void RefreshScreen()
        {
            StateHasChanged();
        }
        private async Task OnLogoutClick()
        {
            if (await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to Logout?"))
            {
                _appUser.IsCheckedIn = false;
                _sideBarService.RefreshSideBar.Invoke();
                await _localStorageService.RemoveItem("token");
                await _authStateProvider.GetAuthenticationStateAsync();
                await returnpath("/");
            }
        }


    }
}
    
