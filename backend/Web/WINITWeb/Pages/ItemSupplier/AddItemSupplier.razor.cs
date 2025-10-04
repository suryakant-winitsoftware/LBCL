using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Org.Model.Classes;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.ItemSupplier
{
    public partial class AddItemSupplier
    {
        [Parameter]
        public string OrgUID { get; set; }

        private string validationMessage;


        public bool IsEditPage { get; set; }
        public bool IsBackBtnPopUp { get; set; }


        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            OrgUID = _commonFunctions.GetParameterValueFromURL("OrgUID");
            if (OrgUID != null)
            {
                IsEditPage = true;
                await _orgViewModel.GetORGforEditDetailsData(OrgUID);
                //_vehicleViewModel.InstilizeFieldsForEditPage(_vehicleViewModel.VEHICLE);
            }

        }
       
        private async Task SaveOrgItem()
        {
            validationMessage = null;
            if (string.IsNullOrWhiteSpace(_orgViewModel.org.Code) || string.IsNullOrWhiteSpace(_orgViewModel.org.Name))
            {
                validationMessage = @Localizer["the_following_field(s)_have_invalid_value(s):"];
                if (string.IsNullOrWhiteSpace(_orgViewModel.org.Code))
                {
                    validationMessage += @Localizer["supplier_code,"];
                }
                if (string.IsNullOrWhiteSpace(_orgViewModel.org.Name))
                {
                    validationMessage += @Localizer["supplier_description,"];
                }
                validationMessage = validationMessage.TrimEnd(' ', ',');
            }

            else
            {
                if (!IsEditPage)
                {
                    await _orgViewModel.SaveUpdateORGItem(_orgViewModel.org, true);
                    StateHasChanged();
                    _tost.Add(@Localizer["org_item"], @Localizer["org_item_details_saved_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    await Task.Delay(2000);
                    _navigationManager.NavigateTo("ItemSupplier");
                }
                else
                {

                    await _orgViewModel.SaveUpdateORGItem(_orgViewModel.org, false);
                    _tost.Add(@Localizer["org_item"], @Localizer["org_item_details_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    await Task.Delay(2000);
                    _navigationManager.NavigateTo("ItemSupplier");

                }

            }

        }
        private async Task BackBtnClicked()
        {
            IsBackBtnPopUp = true;
        }
        private async Task OnOkFromBackBTnPopUpClick()
        {
            _navigationManager.NavigateTo($"ItemSupplier");
        }
    }
}
