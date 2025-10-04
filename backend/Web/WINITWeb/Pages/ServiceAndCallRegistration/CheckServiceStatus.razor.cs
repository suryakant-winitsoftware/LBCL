using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.BL.Interfaces;
namespace WinIt.Pages.ServiceAndCallRegistration
{
    public partial class CheckServiceStatus : BaseComponentBase
    {
        public bool OnInitialised { get; set; } = false;
        public string serviceNumber;
        public bool DataFetched { get; set; } = false;
         public string ErrorMessage ;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Check Status",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Check Status"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _serviceAndCallRegistrationViewModel.PopulateDropDowns();
            OnInitialised = true;
            HideLoader();
        }
        public async Task SubmitSeviceNumber()
        {
            if (!string.IsNullOrEmpty(serviceNumber))
            {
                _serviceAndCallRegistrationViewModel.ServiceStatus.CallId= serviceNumber;
                ShowLoader();
                _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce = await _serviceAndCallRegistrationViewModel.GetServiceStatusBasedOnNumber(_serviceAndCallRegistrationViewModel.ServiceStatus);
                HideLoader() ;
                if (_serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors.Count > 0)
                {
                    string errorMessages = string.Join(", ", _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors);
                    ShowErrorSnackBar("Error", errorMessages);
                }
                else
                {
                    DataFetched = true;
                }
                
            }
            else
            {
                ShowErrorSnackBar("Error", "Please enter service number");
            }
            
        }
    }
}
