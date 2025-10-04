using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.UserJourney_AttendanceReport
{
    public partial class UserJourney_AttendanceReportDetails
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        [Parameter]
        public bool IsIntialized { get; set; }
        public string UID;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "User Journey & Attendance Report Details",
            BreadcrumList = new List<IBreadCrum>()
      {
          new BreadCrumModel(){SlNo=1,Text="User Journey & Attendance Report",URL="UserJourney_AttendanceReport",IsClickable=true},
          new BreadCrumModel(){SlNo=1,Text="User Journey & Attendance Report Details"},
      }
        };
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            UID = _commonFunctions.GetParameterValueFromURL("UID");
            if (UID != null)
            {
                await _userJourneyViewModel.PopulateUserJourneyReportforView(UID);
                //  await _userJourneyViewModel.SetEditForviewpresales(_userJourneyViewModel.SKUViewPreSalesList);
               // await _userJourneyViewModel.PopulateViewModel();
            }
            StateHasChanged();
            IsIntialized = true;
            await SetHeaderName();
        }
       

        public async Task SetHeaderName()
        {
                _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["user_journey_&_attendance_report"] , IsClickable = true,URL= "UserJourney_AttendanceReport" });
                _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["user_journey_attendance_report_details"], IsClickable = false });
                _IDataService.HeaderText = @Localizer["user_journey_attendance_report_details"];
                await CallbackService.InvokeAsync(_IDataService);
        }
        private async Task BackBtnClicked()
        {
            _navigationManager.NavigateTo($"UserJourney_AttendanceReport");
        }
    }
}

