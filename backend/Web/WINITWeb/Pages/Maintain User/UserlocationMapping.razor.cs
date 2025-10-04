using Microsoft.AspNetCore.Components;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using WinIt.Pages.Base;

namespace WinIt.Pages.Maintain_User
{
    public partial class UserlocationMapping : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public string LoginID { get; set; }
        [Parameter] public bool IsNew { get; set; }
        [Parameter] public ILocationTypeAndValue LocationTypeAndValueForLocationMapping { get; set; }
        [Parameter] public List<ISelectionItem> UserLocationTypes { get; set; }
        [Parameter] public List<ISelectionItem> UserLocationValues { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnUserLocationType { get; set; }
        [Parameter] public EventCallback<DropDownEvent> OnUserLocationValue { get; set; }
        [Parameter] public EventCallback<ILocationTypeAndValue> SaveOrUpdateLocationInformation { get; set; }

        public string validationError = "";

        protected override async Task OnInitializedAsync()
        {
            LoginID = _commonFunctions.GetParameterValueFromURL("LoginID");
            IsInitialised = true;
        }

        public async Task OnUserLocationTypeSelect(DropDownEvent dropDownEvent)
        {
            await OnUserLocationType.InvokeAsync(dropDownEvent);
        }

        public async Task OnUserLocationValueSelect(DropDownEvent dropDownEvent)
        {
            await OnUserLocationValue.InvokeAsync(dropDownEvent);
        }

        public async Task SaveUpdateLocationMapping()
        {
            if (DataValidated())
            {
                await SaveOrUpdateLocationInformation.InvokeAsync(LocationTypeAndValueForLocationMapping);
            }
            else
            {
                if (!string.IsNullOrEmpty(validationError))
                {
                    ShowErrorSnackBar("Error", validationError + " Cannot be Empty");
                }
            }
        }

        private bool DataValidated()
        {
            validationError = string.Empty;
            if (string.IsNullOrWhiteSpace(LocationTypeAndValueForLocationMapping.LocationType))
            {
                validationError += "LocationType ";
                return false;
            }
            else if (string.IsNullOrWhiteSpace(LocationTypeAndValueForLocationMapping.LocationValue))
            {
                validationError += "LocationValue ";
                return false;
            }
            else
            {
                validationError = string.Empty;
                return true;
            }
        }
    }
}
