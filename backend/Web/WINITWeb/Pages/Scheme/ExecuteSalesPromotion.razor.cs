using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WinIt.Pages.Scheme
{
    public partial class ExecuteSalesPromotion : ComponentBase
    {
        private bool isEditMode = false;

        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? IDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
        {
            HeaderText = "Execute sales promotion",
            BreadcrumList = new()
            {
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                {
                    SlNo=1,
                    Text="Execute Sales Promotion"
                }
            }
        };
        decimal Amount
        {
            get
            {
                return CommonFunctions.RoundForSystem(_viewModel.SalesPromotion.Amount, _appSetting.RoundOffDecimal);
            }
            set
            {
                _viewModel.SalesPromotion.Amount = value;
            }
        }
        List<DataGridColumn> productColumns = new List<DataGridColumn>()
        {
           new DataGridColumn { Header ="Channel Partner",GetValue=s=>((ISalesPromotionScheme)s).ChannelPartnerName,  IsSortable = false},
           new DataGridColumn { Header ="ACtivity Type",GetValue=s=>((ISalesPromotionScheme)s).ActivityType,  IsSortable = false},
           new DataGridColumn { Header ="From Date",GetValue=s=>Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((ISalesPromotionScheme)s).FromDate),  IsSortable = false},
           new DataGridColumn { Header ="To Date",GetValue=s=>Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((ISalesPromotionScheme)s).ToDate),  IsSortable = false},
           new DataGridColumn { Header ="Status",GetValue=s=>((ISalesPromotionScheme)s).Status,  IsSortable = false},
        };
        public Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader;
        protected override void OnInitialized()
        {
            _loadingService.ShowLoading();
            productColumns.Add(new()
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new (){
                        ButtonType=ButtonTypes.Text,
                        Text="Edit",
                        Action=s=>ViewOrEdit((ISalesPromotionScheme)s)
                    }
                }
            });
            _loadingService.HideLoading();
            base.OnInitialized();
        }
        string FilePath = string.Empty;
        List<IFileSys> ModifiedApprovedFiles = [];
        protected override async Task OnInitializedAsync()
        {
            await _viewModel.PopulateViewModelForExecuteSales();
        }
        protected async Task ViewOrEdit(ISalesPromotionScheme salesPromotionScheme)
        {
            _viewModel.SalesPromotion = salesPromotionScheme;
            await _viewModel.SetEditModeOnClick();
            _viewModel.SalesPromotion.ChannelPartnerName = _viewModel.ChannelPartner.Find(p => p.UID == _viewModel.SalesPromotion.FranchiseeOrgUID)?.Label;
            _viewModel.ActivityType.ForEach(p =>
            {
                p.IsSelected = p.Code == _viewModel.SalesPromotion.ActivityType;
            });
            FilePath = FileSysTemplateControles.GetSchemeeFolderPath(_viewModel.SalesPromotion.UID);
            isEditMode = true;
            StateHasChanged();
        }
        private async Task HandleUpdate()
        {
            if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure to submit"))
            {
                SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO = new SalesPromotionSchemeApprovalDTO();
                 salesPromotionSchemeApprovalDTO.SalesPromotion=_viewModel.SalesPromotion;
                salesPromotionSchemeApprovalDTO.SalesPromotion.Status=SchemeConstants.Executed;

                bool res = await _viewModel.UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
                if (res)
                {
                    _tost.Add("Success", "Saved successfully");
                    _navigationManager.NavigateTo("executepromotion");
                    isEditMode = false;

                }
                else
                {
                    await _alertService.ShowErrorAlert("Failed", "Update failed");
                }
            }
        }
        private async Task HandleCancel()
        {
            if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure to cancel"))
            {
                _navigationManager.NavigateTo("executepromotion");
                isEditMode = false;

            }
        }
    }
}