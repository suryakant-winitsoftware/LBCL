using DocumentFormat.OpenXml.Wordprocessing;
using Winit.Modules.CreditLimit.Model.Classes;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Maintain_Provisioning
{
    public partial class ViewProvisioningDetails : BaseComponentBase
    {
        public string? ProvisionItemUID { get; set; }
        public bool IsInitialised { get; set;}
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Provisioning Details",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Provisioning", URL = "Provisioning", IsClickable = true },
                new BreadCrumModel(){SlNo=1,Text="View Provisioning Details"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            ProvisionItemUID = _commonFunctions.GetParameterValueFromURL("ProvisionItemUID");
            if (ProvisionItemUID != null)
            {
                await provisioningItemViewModel.GetProvisioningItemDetailsByUID(ProvisionItemUID);
            }
            IsInitialised = true;
            HideLoader();
        }
        private void BackBtnClicked()
        {
            ShowLoader();
            _navigationManager.NavigateTo($"Provisioning");
            HideLoader();
        }
    }
}
