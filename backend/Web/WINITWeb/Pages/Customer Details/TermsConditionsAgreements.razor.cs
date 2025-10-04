using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Constants;

namespace WinIt.Pages.Customer_Details
{

    public partial class TermsConditionsAgreements
    {
        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveandUpdate { get; set; }

        private bool isChecked = false;
        private bool isDisabled = true;
        private bool IsAddPopUp = false;
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            _onBoardCustomerDTO = _serviceProvider.CreateInstance<IOnBoardCustomerDTO>();
            _onBoardCustomerDTO.StoreAdditionalInfoCMI= new StoreAdditionalInfoCMI();

            await Task.CompletedTask;
            _loadingService.HideLoading();
        }
        public async Task OnUploadTermsConditions()
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.IsTcAgreed = true;
            isDisabled = false;
            IsAddPopUp = false;
            StateHasChanged();
        }
        protected async Task SaveOrUpdate()
        {
            await OnUploadTermsConditions();
            try
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.TermAndCond;
                await SaveandUpdate.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void OnCheckboxChanged(ChangeEventArgs e)
        {
            @_onBoardCustomerDTO.StoreAdditionalInfoCMI.IsTcAgreed = (bool)e.Value;
            if (!@_onBoardCustomerDTO.StoreAdditionalInfoCMI.IsTcAgreed)
            {
                isDisabled = true;
            }
        }
        private async Task OpenPopup()
        {
            IsAddPopUp = true;
            await Task.Delay(0);
        }
        public async Task OnCancelTermsConditions()
        {
            IsAddPopUp = false;
        }
    }
}
