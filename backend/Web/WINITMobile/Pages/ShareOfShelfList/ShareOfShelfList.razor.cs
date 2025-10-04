using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.Model.Interfaces;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Modules.StoreCheck.BL.Interfaces;

namespace WINITMobile.Pages.ShareOfShelfList
{
    public partial class ShareOfShelfList
    {
        private bool show = true;
        private ISosHeaderCategoryItemView selectedItem;
        protected override void OnInitialized()
        {
            if (_appUser != null && _appUser.SelectedCustomer != null)
            {
                _viewmodel.StoreHistoryUID = _appUser.SelectedCustomer.StoreUID;

            }
        }

        protected override async Task OnInitializedAsync()
        {
            await _viewmodel.PopulateViewModel();

        }

        private void ToggleShow()
        {
            show = !show;
        }
        private async Task SelectItem(ISosHeaderCategoryItemView item)
        {
            ToggleShow();
            selectedItem = item;
            if (selectedItem != null)
            {
                _viewmodel.InitializeShareOfShelfLines(selectedItem);
            }
            await Task.CompletedTask;
        }

        private async Task SubmitShareOfShelfAsync()
        {
            try
            {
                if (await ConfirmSubmissionAsync())
                {
                    var saveResult = await _viewmodel.SaveLines();
                    await HandleSubmissionResultAsync(saveResult);
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Submission Failed", $"An error occurred: {ex.Message}");
            }
        }

        private async Task<bool> ConfirmSubmissionAsync()
        {
            return await _alertService.ShowConfirmationReturnType("Alert!", "Do you want to submit?");
        }

        private async Task HandleSubmissionResultAsync(int saveResult)
        {
            if (saveResult > 1)
            {
                await _alertService.ShowSuccessAlert("Success", "Share of Shelf successfully submitted.");
                await NavigateToCustomerDashboardAsync();
            }
            else
            {
                await _alertService.ShowErrorAlert("Submission Failed", "Share of Shelf submission failed.");
            }
        }

        protected async Task NavigateToCustomerDashboardAsync()
        {
            try
            {
                var storeActivityHistoryUid = (string)_dataManager.GetData("StoreActivityHistoryUid");

                if (_appUser.SelectedCustomer != null)
                {
                    await _StoreActivityViewmodel.UpdateStoreActivityHistoryStatus(storeActivityHistoryUid,
                        Winit.Modules.Base.Model.CommonConstant.COMPLETED);
                }

                _navigationManager.NavigateTo("CustomerCall");
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Navigation Failed", $"Failed to navigate to customer dashboard: {ex.Message}");
            }
        }

    }
}
