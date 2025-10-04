using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Classes;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Common;
using Winit.UIComponents.Common.Services;
using WINITMobile.Pages.Base;

namespace WINITMobile.State
{
    public class PageNavigation
    {
        public string Url { get; set; }
        public object State { get; set; }
        

        // Override GetHashCode and Equals methods to ensure correct behavior of HashSet
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ State.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PageNavigation))
                return false;

            var other = (PageNavigation)obj;
            return Url == other.Url && State == other.State;
        }
    }

    public class NavigationService
    {

        public readonly HashSet<PageNavigation> pageSet = new HashSet<PageNavigation>();
        public readonly List<PageNavigation> pageList = new List<PageNavigation>();

        public Action Action { get; set; }
        public bool initialPageAdded { get; set; }

        private NavigationManager navigationManager;
        private readonly IAppUser _appUser;

        protected Winit.UIComponents.Common.Services.ILoadingService _loadingService { get; set; }


        protected Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModelFactory _journeyPlanViewModelFactory { get; set; }
        public Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModel _journeyplanviewmodel { get; set; }

        protected WINITMobile.Data.SideBarService _sideBarService { get; set; }

        public Winit.UIComponents.Common.IAlertService _alertService { get; set; }
        // protected Winit.UIComponents.Common.IAlertService _alertService { get; set; }

        protected Winit.UIComponents.Common.Services.IDropDownService _dropdownService { get; set; }
        public NavigationService(IAppUser appUser, ILoadingService _loadingService, IJourneyPlanViewModelFactory journeyplanviewmodel, IAlertService alertService, WINITMobile.Data.SideBarService sideBarService, IDropDownService dropdownService)
        {
            _appUser = appUser;
            _journeyPlanViewModelFactory = journeyplanviewmodel;
            _alertService = alertService;
            _dropdownService = dropdownService;
            _sideBarService = sideBarService;
           
        }
        public void NavigateTo(NavigationManager navigationManager, string uri, object pageState = null)
        {
            this.navigationManager = navigationManager;
            if (!initialPageAdded)
            {
                // Add the initial page ("/") to the navigation history
                var initialPage = new PageNavigation { Url = "/", State = null };
                try
                {
                    //pageSet.Add(initialPage);
                    pageList.Add(initialPage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                };
                initialPageAdded = true;
            }
            var currentPage = new PageNavigation { Url = uri, State = pageState };

            if (pageList.Contains(currentPage))
            {
                // If the URI with the same page state is already visited, just navigate without adding it again.
                navigationManager.NavigateTo(uri);
                return;
            }

            pageSet.Add(currentPage);
            pageList.Add(currentPage);

            navigationManager.NavigateTo(uri);
        }

        public async Task NavigateBack(int stepsBack = 1)
        {
            if(pageList.Count == 1)
            {
                if (await _alertService.ShowConfirmationReturnType("Confirm?", "Close The App?"))
                {
                    Application.Current.Quit();
                }
                else
                {
                    return;
                }
            }
            if (pageList.Count >= 1)
            {

                // Ensure stepsBack is within bounds
                stepsBack = Math.Min(stepsBack, pageList.Count - 1);

                // Find the index of the target page to navigate back to
                int targetIndex = pageList.Count - stepsBack - 1;
                var targetPage = pageList.ElementAt(targetIndex);
                int currentPageIndex = pageList.Count - 1;
                if (IsDashboardPage(pageList[currentPageIndex].Url))
                {
                    if (!await _alertService.ShowConfirmationReturnType("Confirm?", "Are you sure you want to Logout?"))
                    {
                        return;
                    }
                    else
                    {
                        navigationManager.NavigateTo("/");

                        for (int i = 0; i < stepsBack; i++)
                        {
                            pageList.Remove(pageList.Last());
                        }
                        return;
                    }
                    
                }
                if (_appUser.IsCustomerCallPage)
                {
                    // If the target page is a check-in page, find the Dashboard index
                    int dashboardIndex = pageList.ToList().FindLastIndex(p => IsDashboardPage(p.Url));

                    if (dashboardIndex != -1)
                    {

                        await NavigateToDashboardAndRemoveRightPages(dashboardIndex);
                        return;
                    }

                }


                // Navigate to the target page as usual
                navigationManager.NavigateTo(targetPage.Url);

                for (int i = 0; i < stepsBack; i++)
                {
                    pageList.Remove(pageList.Last());
                }
            }
            else
            {

                navigationManager.NavigateTo("/");
            }
        }


        private async Task NavigateToDashboardAndRemoveRightPages(int dashboardIndex)
        {
            // Show confirmation dialog
            //pageList.RemoveWhere(p => pageList.ToList().IndexOf(p) > dashboardIndex);

            // var res = await _alertService.ShowConfirmationReturnType("hii", "test");
            //  var confirmation = await _alertService.ShowConfirmationReturnType("Are you sure you want to navigate to the dashboard?", "All unsaved data will be lost. Continue?");

            _sideBarService.OnCheckOutClick.Invoke();

            pageList.RemoveAll(p => pageList.IndexOf(p) > dashboardIndex);
            _appUser.IsCustomerCallPage = false;

        }
        private bool IsDashboardPage(string url)
        {
            return url.ToLowerInvariant().Contains("dashboard");
        }


        public async Task OnBackClick()
        {
            await NavigateBack(1);
        }

        //public async Task OnBackClick()
        //{
        //    if (await _alertService.ShowConfirmationReturnType("Are you sure you want to navigate to the dashboard?", "All unsaved data will be lost. Continue?"))
        //    {
        //        await NavigateBack(1);
        //    }
        //    else
        //    {
        //        return;
        //    }
        //} 

    }
}
