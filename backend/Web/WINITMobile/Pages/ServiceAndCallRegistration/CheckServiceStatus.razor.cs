using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.ServiceAndCallRegistration
{
    public partial class CheckServiceStatus : BaseComponentBase
    {
        public bool OnInitialised { get; set; } = false;
        public string serviceNumber;
        public bool DataFetched { get; set; } = false;
        public string ErrorMessage;
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
                _serviceAndCallRegistrationViewModel.ServiceStatus.CallId = serviceNumber;
                ShowLoader();
                _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce = await _serviceAndCallRegistrationViewModel.GetServiceStatusBasedOnNumber(_serviceAndCallRegistrationViewModel.ServiceStatus);
                HideLoader();
                if (_serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors.Count > 0)
                {
                    string errorMessages = string.Join(", ", _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors);
                    ShowAlert("Error", errorMessages);
                }
                else
                {
                    DataFetched = true;
                }

            }
            else
            {
                ShowAlert("Error", "Please enter service number");
            }

        }
    }
}
