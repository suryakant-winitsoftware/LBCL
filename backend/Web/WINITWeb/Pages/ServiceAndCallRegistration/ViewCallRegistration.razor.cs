using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.Classes;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.ServiceAndCallRegistration
{
    public partial class ViewCallRegistration : BaseComponentBase
    {
        [Parameter]
        public string? ServiceCallID { get; set; }
        public bool IsInitialised { get; set; } = false;
        public bool ShowCallStatus {  get; set; }= false;
        private string CustomerType { get; set; }
        private string ProductCategoryName { get; set; }
        private string BrandCode { get; set; }
        private string ServiceTypeCode { get; set; }
        private string WarrantyStatus { get; set; }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Call Registration",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Call Registration", URL = "MaintainCallRegistration", IsClickable = true },
                new BreadCrumModel(){SlNo=1,Text="View Call Registration"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            ServiceCallID = _commonFunctions.GetParameterValueFromURL("ServiceCallNumber");
            await _serviceAndCallRegistrationViewModel.PopulateDropDowns();
            await _serviceAndCallRegistrationViewModel.PopulateCallRegistrationItemDetailsByUID(ServiceCallID);
            BindValues();
            IsInitialised = true;
            HideLoader();
        }

        private void BindValues()
        {
            var customerTypeLabel = _serviceAndCallRegistrationViewModel.CustTypeSelectionList
                                .FirstOrDefault(item => item.Code == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails.CustomerType.ToString());
            CustomerType = customerTypeLabel?.Label ?? string.Empty;

            //var productCategoryNameLabel = _serviceAndCallRegistrationViewModel.ProductCategorySelectionList
            //                    .FirstOrDefault(item => item.Code == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails.ProductCategoryCode);
            //ProductCategoryName = productCategoryNameLabel?.Label ?? string.Empty;
            var productCategoryNameLabel = _serviceAndCallRegistrationViewModel?.ProductCategorySelectionList?
                                .FirstOrDefault(item => item.Label == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails?.ProductCategoryCode);

            ProductCategoryName = _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails?.ProductCategoryCode;


            var brandCodeLabel = _serviceAndCallRegistrationViewModel.BrandCodeSelectionList
                                .FirstOrDefault(item => item.Code == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails.BrandCode.ToString());
            BrandCode = brandCodeLabel?.Label ?? string.Empty;

            var serviceTypeCodeLabel = _serviceAndCallRegistrationViewModel.ServiceTypeCodeSelectionList
                                .FirstOrDefault(item => item.Code == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails.ServiceType.ToString());
            ServiceTypeCode = serviceTypeCodeLabel?.Label ?? string.Empty;

            var warrentyStatusLabel = _serviceAndCallRegistrationViewModel.WarrentyStatusSelectionList
                                .FirstOrDefault(item => item.Code == _serviceAndCallRegistrationViewModel.CallRegisteredItemDetails.WarrantyStatus.ToString());
            WarrantyStatus = warrentyStatusLabel?.Label ?? string.Empty;

        }
         
        private async Task CheckCallRegistrationStatus()
        {
            _serviceAndCallRegistrationViewModel.ServiceStatus.CallId = ServiceCallID;
            ShowLoader();
            _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce = await _serviceAndCallRegistrationViewModel.GetServiceStatusBasedOnNumber(_serviceAndCallRegistrationViewModel.ServiceStatus);
            HideLoader();
            if (_serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors.Count > 0)
            {
                string errorMessages = string.Join(", ", _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors);
                ShowErrorSnackBar("Error", errorMessages);
            }
            else
            {
                ShowCallStatus = true;
            }
           // ShowCallStatus = true;
        }
    }
}
