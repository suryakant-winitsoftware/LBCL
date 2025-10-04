using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common;
using Winit.UIModels.Common.Filter;

namespace WinIt.Pages.Scheme
{
    public partial class AddHOProvisionStandingConfiguration
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            HeaderText = "Add HO Provision Standing Configuration (P)",
            BreadcrumList = new()
            {
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Manage HO Provision Standing Configuration(P)",URL="ManageHOProvisionStandingConfiguration",IsClickable=true},
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=2,Text="Add HO Provision Standing Configuration (P)" },
            }
        };

        List<DataGridColumn> columns = [];
        List<ISKUV1> SelectedItems { get; set; } = [];
        private void SelectItem(ISKU item)
        {
            item.IsSelected = !item.IsSelected;
        }
        protected override void OnInitialized()
        {
            _viewModel.SetUserTypeWhileCreatingScheme();
            columns = new List<DataGridColumn>()
        {
                new()
            {
                Header="Code",
                GetValue=p=>((ISKU)p).Code,
            },
            new()
            {
                Header="Name",
                GetValue=p=>((ISKU)p).Name,
            },
        };
            if (_viewModel.IsNew)
            {
                columns.Insert(0, new()
                {
                    Header = "Select",
                    ButtonActions = new()
                {
                    new ButtonAction()
                    {
                        ButtonType=ButtonTypes.CheckBox,
                        GetValue=p=>((ISKU)p).IsSelected,
                        Action=(e)=>SelectItem((ISKU)e)
                    },
                },
                    IsButtonColumn = true,

                });
            }
            base.OnInitialized();
        }
        WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1> AddProductDialogBox;
        protected async override Task OnInitializedAsync()
        {

            await _viewModel.PopulateViewModel();


            if (_viewModel.IsNew)
            {
                columns.Insert(0, new()
                {
                    Header = "Select",
                    ButtonActions = new()
                {
                    new ButtonAction()
                    {
                        ButtonType=ButtonTypes.CheckBox,
                        GetValue=p=>((ISKU)p).IsSelected,
                        Action=(e)=>SelectItem((ISKU)e)
                    },
                },
                    IsButtonColumn = true,

                });
            }
        }
        private async Task<bool> Save()
        {
            //if (!_viewModel.IsNew)
            //{
            //    _navigationManager.NavigateTo("ManageHOProvisionStandingConfiguration");
            //    return true;
            //}
            _loadingService.ShowLoading();
            var response = await ((CreateStandingConfigurationWebViewModel)_viewModel).Save();
            if (response != null)
            {
                if (response.IsSuccess)
                {
                    // await executeRule(_iAppUser.Emp.Code, _viewModel.UserType, _viewModel.RequestId);
                    ShowSuccessSnackBar("Success", "Saved successfully");
                    _navigationManager.NavigateTo("ManageHOProvisionStandingConfiguration");
                }
                else
                {
                    await _alertService.ShowErrorAlert(response.StatusCode == 400 ? "Alert" : "Error", response!.ErrorMessage ?? response!.Data ?? "Unknown error");
                }
            }
            _loadingService.HideLoading();
            return response.IsSuccess;

        }
        private async Task OnAddProductClick()
        {
            await _viewModel.GetProductsOnAddButtonClick();
            StateHasChanged();
            AddProductDialogBox?.OnOpenClick();
        }
        private async Task DeleteSelectedItems()
        {
            if (_viewModel.ExcludedModels == null || _viewModel.ExcludedModels.Count() == 0)
                return;

            if (await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete?"))
            {
                int cnt = 0;
                if (_viewModel.IsNew)
                {

                    cnt = _viewModel.ExcludedModels.RemoveAll(p => p.IsSelected);
                    SelectedItems.Clear();
                }
                else
                    cnt = await ((CreateStandingConfigurationWebViewModel)_viewModel).DeleteExcludedItems();
                if (cnt > 0)
                    _tost.Add("Success", "Deleted successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
        }
        #region Approval Engine
        async Task<bool> OnApproveClick(bool isFinalArroval)
        {
            ShowLoader();
            if (isFinalArroval)
            {
                _viewModel.StandingProvisionSchemeMaster!.StandingProvisionScheme!.Status = SchemeConstants.Approved;
            }
            bool isUpdated = true;
            if (isUpdated)
            {
                _navigationManager.NavigateTo("ManageHOProvisionStandingConfiguration");
            }
            HideLoader();
            return isUpdated;
        }


        private async Task HandleApproved()
        {
            // after all level of approvals
            // _viewModel.StandingProvisionSchemeMaster.SellInHeader.Status = SchemeConstants.Approved;
            // await Save();
        }
        private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
        {
            ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
            {
                IsApprovalActionRequired = true,
            };
            try
            {
                _viewModel.StandingProvisionSchemeMaster.ApprovalStatusUpdate = approvalStatusUpdate;
                if (approvalStatusUpdate.IsFinalApproval)
                {
                    _viewModel.StandingProvisionSchemeMaster.IsFinalApproval = true;

                    _viewModel.StandingProvisionSchemeMaster.StandingProvisionScheme.Status = SchemeConstants.Approved;

                }
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _viewModel.StandingProvisionSchemeMaster.StandingProvisionScheme.Status = SchemeConstants.Rejected;
                }
                if (approvalStatusUpdate.Status != ApprovalConst.Reassign)
                {
                    approvalActionResponse.IsSuccess = await Save();
                }
                return approvalActionResponse;
                // _viewModel._SellInSchemeDTO.SellInHeader.Status = SchemeConstants.Approved;



                return approvalActionResponse;
            }
            catch (CustomException ex)
            {

                approvalActionResponse.IsSuccess = false;
                return approvalActionResponse;
            }
        }
        private async Task<bool> UpdateorDeleteExcludedItems()
        {
            bool isSaved = await ((CreateStandingConfigurationWebViewModel)_viewModel).UpdateorDeleteExcludedItems();
            if (isSaved)
            {
                _navigationManager.NavigateTo("ManageHOProvisionStandingConfiguration");
            }
            return isSaved;
        }
        bool isLoadedOnInitialized;
        public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses)
        {
            try
            {
                if (!isLoadedOnInitialized)
                {
                    isLoadedOnInitialized = true;
                    ApprovalStatusResponse approvalStatusResponse = approvalStatusResponses.FirstOrDefault(p => p.ApproverId.Equals(_iAppUser.Role.Code, StringComparison.OrdinalIgnoreCase));
                    if (approvalStatusResponse == null)
                    {
                        //DisableFields(true);
                        return;
                    }
                    bool isApprovalProcessStarted = approvalStatusResponses.Any(p => SchemeConstants.Approved.Equals(p.Status, StringComparison.OrdinalIgnoreCase));
                    bool finalLevelApproved = !approvalStatusResponses.Any(p => p.Status != SchemeConstants.Approved);

                    //DisableFields(SchemeConstants.Approved.Equals(approvalStatusResponse.Status, StringComparison.OrdinalIgnoreCase), finalLevelApproved);
                }
                else
                {
                    //bool isApproved = approvalStatusResponses.Any(p => p.ApproverId.Equals(_iAppUser.Role.Code, StringComparison.OrdinalIgnoreCase) && SchemeConstants.Approved.Equals(p.Status, StringComparison.OrdinalIgnoreCase));
                    //if (isApproved)
                    //{

                    //    Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await ((SellInSchemeWebViewModel)_viewModel).Save();
                    //    if (apiResponse != null && apiResponse.IsSuccess)
                    //    {
                    //        _navigationManager.NavigateTo("ManageScheme");
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }

        #endregion

    }
}
