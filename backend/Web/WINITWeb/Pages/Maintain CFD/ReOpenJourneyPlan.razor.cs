using Microsoft.AspNetCore.Components;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;


namespace WinIt.Pages.Maintain_CFD
{
    public partial class ReOpenJourneyPlan : WinIt.Pages.Base.BaseComponentBase
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Re-Open Journey Plan",
            BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Re-Open Journey Plan"},
         }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                await _reOpenJourneyPlanViewModel.PopulateViewModel();
                await GenerateGridColumns();
                IsLoaded = true;
                ShowSuccessSnackBar(@Localizer["success"], @Localizer["succesfully_saved"] );
                HideLoader();
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex);
                HideLoader();
            }
        }

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = @Localizer["user"], GetValue = s => ((IUserJourney)s)?.EmpUID ?? "N/A" },
                new DataGridColumn { Header = @Localizer["login_id"], GetValue = s => ((IUserJourney)s)?.LoginId?? "N/A" },
                new DataGridColumn { Header = @Localizer["journey_start_time"], GetValue = s => ((IUserJourney)s)?.JourneyStartTime.ToString() ?? "N/A" },
                new DataGridColumn { Header = @Localizer["journey_endtime"], GetValue = s => ((IUserJourney)s)?.JourneyEndTime.ToString() ?? "N/A" },
                new DataGridColumn { Header = @Localizer["cfd_status"], GetValue = s => ((IUserJourney)s)?.EOTStatus ?? "N/A" },

             };
        }

        
    }
}

