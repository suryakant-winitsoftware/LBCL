using Azure;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common;

namespace WinIt.Pages.Scheme;

public partial class AddQPSOrSelloutrealSecondaryScheme
{
    [Parameter] public string IsQPS { get; set; }
    WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1> AddProductDialogBox;
    public Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader;
    string FilePath = string.Empty;

    Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService =
        new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
            {
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                    { SlNo = 1, Text = "Manage Scheme", URL = "ManageScheme", IsClickable = true },
            }
        };

    List<DataGridColumn> SchemeProductsColumns = new List<DataGridColumn>()
    {
        new DataGridColumn() { Header = "Order Type", GetValue = S => ((IQPSSchemeProducts)S).OrderType },
        new DataGridColumn() { Header = "SKU Group Type", GetValue = S => ((IQPSSchemeProducts)S).SKUGroupType },
        new DataGridColumn() { Header = "Selected Values", GetValue = S => ((IQPSSchemeProducts)S).SelectedValue },
    };

    List<DataGridColumn> SchemeSlabsColumns = new List<DataGridColumn>()
    {
        new DataGridColumn()
            { Header = "Minimum", GetValue = s => CommonFunctions.RoundForSystem(((ISchemeSlab)s).Minimum) },
        new DataGridColumn()
            { Header = "Maximum", GetValue = s => CommonFunctions.RoundForSystem(((ISchemeSlab)s).Maximum) },
        new DataGridColumn() { Header = "Offer Type", GetValue = s => ((ISchemeSlab)s).OfferType },
        new DataGridColumn()
        {
            Header = "Offer Value", GetValue = s => ((ISchemeSlab)s).IsFOCType
                ? ((ISchemeSlab)s).OfferItem + "*" +
                  CommonFunctions.GetIntValue(((ISchemeSlab)s).OfferValue)
                : CommonFunctions.RoundForSystem(((ISchemeSlab)s).OfferValue)
        },
    };

    List<ISchemeSlab> SchemeSlabs
    {
        get
        {
            return _viewModel.SchemeSlabs.Where(p => p.ActionType != Winit.Shared.Models.Enums.ActionType.Delete)
                .ToList();
        }
    }

    List<IQPSSchemeProducts> SchemeProducts
    {
        get
        {
            return _viewModel.SchemeProducts.Where(p => p.ActionType != Winit.Shared.Models.Enums.ActionType.Delete)
                .ToList();
        }
    }

    private async Task DeleteSchemeSlab(ISchemeSlab schemeSlab)
    {
        if (!await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete?"))
        {
            return;
        }

        if (schemeSlab.Id == 0)
        {
            _viewModel.SchemeSlabs.Remove(schemeSlab);
            ShowSuccessSnackBar("Success", "Deleted successfully.");
            StateHasChanged();
            return;
        }

        ShowLoader();
        var response =
            await ((Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel)_viewModel)
                .DeletePromotionSlabByPromoOrderUID(schemeSlab.PromoOrderUID);
        if (response != null && response.IsSuccess)
        {
            _viewModel.SchemeSlabs.Remove(schemeSlab);
            _viewModel.PromoMasterView.PromoOrderViewList.RemoveAll(p => p.UID == schemeSlab.PromoOrderUID);
            List<string> referenceUIDs = new List<string>()
            {
                schemeSlab.PromoOrderUID
            };
            _viewModel.PromoMasterView.PromoOrderItemViewList.ForEach(p =>
            {
                if (p.PromoOrderUID == schemeSlab.PromoOrderUID)
                {
                    referenceUIDs.Add((p.UID));
                }

                ;
            });
            _viewModel.PromoMasterView.PromoOrderItemViewList.RemoveAll(e => referenceUIDs.Contains(e.UID));

            List<string> promoOfferUID = new List<string>();
            _viewModel.PromoMasterView.PromoOfferViewList.ForEach(p =>
            {
                if (p.PromoOrderUID == schemeSlab.PromoOrderUID)
                {
                    promoOfferUID.Add(p.UID);
                    referenceUIDs.Add((p.UID));
                }

                ;
            });
            _viewModel.PromoMasterView.PromoOfferViewList.RemoveAll(e => referenceUIDs.Contains(e.UID));

            _viewModel.PromoMasterView.PromoOfferItemViewList.ForEach(p =>
            {
                if (promoOfferUID.Contains(p.PromoOfferUID))
                {
                    referenceUIDs.Add((p.UID));
                }
            });
            _viewModel.PromoMasterView.PromoOfferItemViewList.RemoveAll(e => referenceUIDs.Contains(e.UID));

            _viewModel.PromoMasterView.PromoConditionViewList.RemoveAll(p => referenceUIDs.Contains(p.ReferenceUID));

            ShowSuccessSnackBar("Success", "Deleted successfully.");
            StateHasChanged();
        }

        HideLoader();

        StateHasChanged();
    }

    private async Task DeleteSchemeProducts(IQPSSchemeProducts qPSSchemeProducts)
    {
        if (!await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete?"))
        {
            return;
        }

        if (qPSSchemeProducts.Id == 0)
        {
            _viewModel.SchemeProducts.Remove(qPSSchemeProducts);
            ShowSuccessSnackBar("Success", "Deleted successfully.");
            StateHasChanged();
            return;
        }


        ShowLoader();
        var selectedOrderItems = _viewModel.PromoMasterView.PromoOrderItemViewList.Where(p =>
            p.ItemCriteriaType == qPSSchemeProducts.SKUGroupTypeCode &&
            p.ItemCriteriaSelected == qPSSchemeProducts.SelectedValueCode).ToList();
        if (selectedOrderItems != null && selectedOrderItems.Count > 0)
        {
            var response =
                await ((Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel)_viewModel)
                    .DeletePromoOrderItemsByUIDs(selectedOrderItems.Select(p => p.UID).ToList());
            if (response is not null && response.IsSuccess)
            {
                _viewModel.SchemeProducts.Remove(qPSSchemeProducts);
                _viewModel.PromoMasterView.PromoOrderItemViewList.RemoveAll(p =>
                    p.ItemCriteriaType == qPSSchemeProducts.SKUGroupTypeCode &&
                    p.ItemCriteriaSelected == qPSSchemeProducts.SelectedValueCode);
                ShowSuccessSnackBar("Success", "Deleted successfully.");
            }
        }

        HideLoader();

        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        _viewModel.GetUserTypeWhileCreatingScheme(true, out string userType, out int ruleId);
        _viewModel.RuleId = ruleId;
        _viewModel.UserType = userType;
        _viewModel.IsQPSScheme = CommonFunctions.GetBooleanValue(IsQPS);
        FilePath = FileSysTemplateControles.GetSchemeeFolderPath(_viewModel.PromoMasterView.PromotionView.UID);
        if (_viewModel.IsQPSScheme)
        {
            dataService.HeaderText = "QPS Scheme";
            dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "QPS Scheme" });
        }
        else
        {
            dataService.HeaderText = "Sell out on actual secondary scheme";
            dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "Sell out on actual secondary scheme" });
        }

        _viewModel.PromoMasterView.PromotionView.ValidFrom = DateTime.Now;
        _viewModel.PromoMasterView.PromotionView.ValidUpto = CommonFunctions.GetLastDayOfMonth(DateTime.Now);
        //_iAppUser.CalenderPeriods.Max(x => x.EndDate);
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        _loadingService.ShowLoading();
        await _viewModel.PopulateViewModel();
        if (_viewModel.IsNew || !ApprovalConst.Approved.Equals(_viewModel.PromoMasterView.PromotionView.Status,
                StringComparison.OrdinalIgnoreCase))
        {
            SchemeSlabsColumns.Add(new DataGridColumn()
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new()
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = async s => await DeleteSchemeSlab((ISchemeSlab)s),
                        IsDisabled = !_viewModel.IsNew
                    }
                }
            });
            SchemeProductsColumns.Add(new DataGridColumn()
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new()
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = async s => await DeleteSchemeProducts((IQPSSchemeProducts)s),
                        IsDisabled = !_viewModel.IsNew
                    }
                }
            });
        }

        _loadingService.HideLoading();
    }

    private async Task ValidateContributions()
    {
        await _viewModel.ValidateContributions(_viewModel.PromoMasterView.PromotionView.ContributionLevel1,
            _viewModel.PromoMasterView.PromotionView.ContributionLevel2,
            _viewModel.PromoMasterView.PromotionView.ContributionLevel3);
    }

    private void OnContributionsEnter(ChangeEventArgs e, string contribution)
    {
        decimal val = CommonFunctions.RoundForSystem(e.Value ?? 0);
        switch (contribution)
        {
            case nameof(_viewModel.PromoMasterView.PromotionView.ContributionLevel1):
                _viewModel.PromoMasterView.PromotionView.ContributionLevel1 = val;
                break;
            case nameof(_viewModel.PromoMasterView.PromotionView.ContributionLevel2):
                _viewModel.PromoMasterView.PromotionView.ContributionLevel2 = val;
                break;
            case nameof(_viewModel.PromoMasterView.PromotionView.ContributionLevel3):
                _viewModel.PromoMasterView.PromotionView.ContributionLevel3 = val;
                break;
        }

        StateHasChanged();
    }

    protected async Task Save()
    {
        //bool isVal = await _viewModel.ValidateContributions(_viewModel.PromoMasterView.PromotionView.ContributionLevel1, _viewModel.PromoMasterView.PromotionView.ContributionLevel2, _viewModel.PromoMasterView.PromotionView.ContributionLevel3);
        //if (!isVal)
        //{
        //    return;
        //}
        _viewModel.IsSchemeProductsValidated(out bool isVal, out string message);

        if (isVal)
        {
            if (_viewModel.SchemeSlabs == null || _viewModel.SchemeSlabs.Count == 0)
            {
                isVal = false;
                message = "Add atleast one slab to continue";
            }
        }

        if (isVal)
        {
            _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Pending;
            await UpdateData();
        }
        else
        {
            await _alertService.ShowErrorAlert("Alert", message);
        }
    }

    private async Task<bool> UpdateData()
    {
        try
        {
            ShowLoader();
            Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel webViewModel =
                _viewModel as Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel;
            var resp = await webViewModel.Save();
            if (resp != null)
            {
                if (resp != null && resp.IsSuccess)
                {
                    if (_viewModel.ModifiedFileSysList.Count > 0)
                    {
                        var response = await fileUploader?.MoveFiles();
                        if (response != null && response.IsSuccess)
                        {
                            response = await webViewModel.UploadFiles(_viewModel.ModifiedFileSysList);
                            if (response == null || !response.IsSuccess)
                            {
                                _tost.Add("Error", response?.ErrorMessage ?? "Error while uploading files",
                                    Winit.UIComponents.SnackBar.Enum.Severity.Error);
                                return false;
                            }
                        }
                        else
                        {
                            _tost.Add("Error", response?.ErrorMessage ?? "Error while uploading files",
                                Winit.UIComponents.SnackBar.Enum.Severity.Error);
                            return false;
                        }
                    }

                    _tost.Add("Success", "Saved successfully");
                    _navigationManager.NavigateTo("ManageScheme");
                    return true;
                }
                else
                {
                    if (resp.StatusCode == 422)
                    {
                        await _alertService.ShowErrorAlert("Alert", resp.ErrorMessage);
                    }
                    else
                    {
                        _tost.Add("Error", resp.ErrorMessage);
                    }
                }
            }

            return resp != null && resp.IsSuccess;
        }
        catch (Exception ex)
        {
            return false;
        }
        finally
        {
            HideLoader();
        }
    }

    private bool OnItemSelect(ISKUV1 item)
    {
        if (!_appSetting.IsAPMasterValidationRequired) return true;
        if (!item.IsAvailableInApMaster)
        {
            ShowErrorSnackBar("Alert", "This sku is not part of AP master. Please contact CMI team");
        }

        return item.IsAvailableInApMaster;
    }

    protected void OnAddProductClick(bool isFocType = false)
    {
        if (AddProductDialogBox != null)
        {
            AddProductDialogBox.SelectionMode = isFocType
                ? Winit.Shared.Models.Enums.SelectionMode.Single
                : Winit.Shared.Models.Enums.SelectionMode.Multiple;
            AddProductDialogBox?.OnOpenClick();
        }
    }

    protected void SchemeProducts_OnClick()
    {
        _viewModel.IsDetailsValidated(isVal: out bool isVal, message: out string message);
        if (isVal)
        {
            toggle.SchemeProducts = true;
        }
        else
        {
            _alertService.ShowErrorAlert("Alert", message);
        }
    }

    protected void Mapping_OnClick()
    {
        bool isVal = true;
        string message = string.Empty;
        if (toggle.Details)
        {
            _viewModel.IsDetailsValidated(isVal: out isVal, message: out message);
        }

        if ((isVal && toggle.Details) || toggle.SchemeProducts)
        {
            _viewModel.IsSchemeProductsValidated(isVal: out isVal, message: out message);
        }

        if (isVal)
        {
            toggle.Slab = true;
        }
        else
        {
            _alertService.ShowErrorAlert("Alert", message);
        }
    }

    void AddSelectedItems()
    {
        ShowLoader();
        _viewModel.AddSelectedItems();
        HideLoader();
    }

    public void AddSchemeSlab()
    {
        _viewModel.AddSchemeSlab();
        _viewModel.SchemeSlabs = _viewModel.SchemeSlabs.OrderBy(p => p.Minimum).ToList();
        StateHasChanged();
    }


    //private async Task HandleApproved()
    //{
    //    //after all level of approvals
    //    _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Approved;
    //    var response = await ((Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel)_viewModel).UpdateScheme();
    //}

    //private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
    //{
    //    ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
    //    {
    //        IsApprovalActionRequired = true
    //    };
    //    try
    //    {
    //            _viewModel.PromoMasterView.ApprovalStatusUpdate = approvalStatusUpdate;
    //        if (approvalStatusUpdate.IsFinalApproval)
    //        {
    //            _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Approved;
    //            approvalActionResponse.IsSuccess = await UpdateData();
    //        }
    //        else
    //        {
    //            _viewModel.PromoMasterView.ApprovalStatusUpdate = approvalStatusUpdate;
    //            if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
    //            {
    //                _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Rejected;
    //                approvalActionResponse.IsSuccess = await UpdateData();
    //            }
    //            else
    //            {
    //                approvalActionResponse.IsSuccess = await UpdateData();
    //            }
    //        }
    //    }
    //    catch (CustomException ex)
    //    {
    //        approvalActionResponse.IsSuccess = false;
    //    }
    //    return approvalActionResponse;
    //}

    public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses)
    {
    }

    private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
    {
        ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
        {
            IsApprovalActionRequired = true
        };

        try
        {
            _viewModel.PromoMasterView.ApprovalStatusUpdate = approvalStatusUpdate;
            if (approvalStatusUpdate.IsFinalApproval)
            {
                _viewModel.PromoMasterView.IsFinalApproval = true;
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _viewModel.PromoMasterView.PromotionView.Status = ApprovalConst.Rejected;
                }
                else if (approvalStatusUpdate.Status == ApprovalConst.Approved)
                {
                    _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Approved;
                }
            }
            else
            {
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _viewModel.PromoMasterView.PromotionView.Status = SchemeConstants.Rejected;
                }
            }

            if (approvalStatusUpdate.Status != ApprovalConst.Reassign)
            {
                approvalActionResponse.IsSuccess = await UpdateData();
            }
        }
        catch (CustomException ex)
        {
            approvalActionResponse.IsSuccess = false;
        }

        return approvalActionResponse;
    }
}