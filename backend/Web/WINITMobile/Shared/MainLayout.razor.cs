using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinIt;
using WINITMobile.Models.TopBar;

namespace WINITMobile.Shared
{
    public partial class MainLayout
    {
        public DataService _service { get; set; } = new DataService();
        private MainButtons button { get; set; } = null;
        EventCallback<MainButtons> event_DataService => EventCallback.Factory.Create(this, (Action<MainButtons>)setHeaderButtons);
        public ErrorBoundary MainErrorBoundary { get; set; } = default!;
        bool isNavigating = false;
        [Parameter]
        public EventCallback OnCheckOutClicked { get; set; }

        void ResolveError(bool shouldNavigateToHome = false)
        {
            if (shouldNavigateToHome)
            {
                isNavigating = true;
                _navigationManager.LocationChanged += NavigationManager_LocationChanged;
                _navigationManager.NavigateTo("/");
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
                _navigationManager.LocationChanged -= NavigationManager_LocationChanged;
            }
        }

        private async Task HandleCheckOutClicked()
        {
            await OnCheckOutClicked.InvokeAsync();
        }

        private void notifyLayOut(DataService service)
        {
            _service = service;
        }

        private bool collapseNavMenu = false;

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        private void CollapseNavMenu()
        {
            if (collapseNavMenu)
            {
                collapseNavMenu = false;
            }
        }
        public void setHeaderButtons(MainButtons name)
        {
            button = name;
        }
        
    }
}
