using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Calender.Models.Interfaces;
using Winit.Modules.Org.BL.Classes;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.SnackBar.Enum;
using Winit.UIModels.Common;
using WinIt.Pages.ApprovalRoleEngine;
using WinIt.Pages.Collection.NonCashSettlementDetails;

namespace WinIt.Pages.Scheme
{
    public partial class SellInSchemeBranchView
    {
        #region UI Properties

        bool IsApproved { get; set; }
        bool EasyFill { get; set; }
        bool DisableAddProduct { get; set; }
        bool DisableChannelPartner { get; set; }
        bool DisableCommittedQTY { get; set; }
        bool DisableEndDate { get; set; }
        bool DisableStartDate { get; set; }
        bool DisableTillMonthEnd { get; set; }
        bool DisableDealerRequestedPrice { get; set; }
        bool DisableApplyAdditionalDicount { get; set; }
        bool DisableInvoiceDicount { get; set; }
        bool DisableP1 { get; set; }
        bool DisableP2 { get; set; }
        bool DisableP3 { get; set; }
        bool HideDelete { get; set; }
        bool DisplaySaveButton = true;
        ISellInSchemeLine DiscountApplicableToAllItems { get; set; }
        DateTime minStartDate = DateTime.Now;
        DateTime maxStartDate = DateTime.Now;
        DateTime minEndDate = DateTime.Now;
        DateTime maxEndDate = DateTime.Now;

        DateTime? _maxEndDate
        {
            get
            {
                return _viewModel.IsNew
                    ? (_viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd
                        ? _viewModel._SellInSchemeDTO!.SellInHeader.ValidUpTo
                        : maxEndDate)
                    : _viewModel._SellInSchemeDTO!.SellInHeader.EndDate;
            }
        }

        DateTime? StartDate
        {
            get { return _viewModel._SellInSchemeDTO!.SellInHeader.ValidFrom; }
            set
            {
                _viewModel._SellInSchemeDTO.SellInHeader.ValidFrom = value;
                if (_viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd)
                {
                    SetEndDate();
                    //DateTime dateTime = _viewModel._SellInSchemeDTO.SellInHeader.ValidFrom ?? DateTime.Now;
                    //_viewModel._SellInSchemeDTO.SellInHeader.ValidUpTo = CommonFunctions.GetLastDayOfMonth(dateTime);
                    //_viewModel._SellInSchemeDTO.SellInHeader.EndDate = _viewModel._SellInSchemeDTO.SellInHeader.ValidUpTo;
                }

                StateHasChanged();
            }
        }

        Winit.UIComponents.Web.DialogBox.ProductDialogBox<Winit.Modules.SKU.Model.Interfaces.ISKUV1>
            AddProductDialogBox1;

        WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<Winit.Modules.SKU.Model.Interfaces.ISKUV1> AddProductDialogBox;

        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService =
            new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Manage Scheme",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                        { SlNo = 1, Text = "Manage Scheme", URL = "ManageScheme", IsClickable = true },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Sell In Scheme" },
                }
            };

        #endregion

        protected override void OnInitialized()
        {
            _viewModel.GetUserTypeWhileCreatingScheme(true, out string userType, out int ruleId);
            _viewModel.RuleId = ruleId;
            _viewModel.UserType = userType;
            DiscountApplicableToAllItems = _serviceProvider.CreateInstance<ISellInSchemeLine>();
            DiscountApplicableToAllItems.CreditNoteDiscountType = SelleInConstants.Value;
            DiscountApplicableToAllItems.InvoiceDiscountType = SelleInConstants.Value;
            _viewModel._SellInSchemeDTO.SellInHeader.ValidFrom = DateTime.Now;
            if (_iAppUser.CalenderPeriods != null && _iAppUser.CalenderPeriods.Any(x =>
                    x.StartDate >= DateTime.Now && x.EndDate >= DateTime.Now))
            {
                //minStartDate = DateTime.Now;//_iAppUser.CalenderPeriods.Min(x => x.StartDate);
                //maxStartDate = _iAppUser.CalenderPeriods.Max(x => x.StartDate);
                //minEndDate = DateTime.Now;// _iAppUser.CalenderPeriods.Min(x => x.EndDate);
                //maxEndDate = _iAppUser.CalenderPeriods.Max(x => x.EndDate);
                minStartDate = DateTime.Now; //_iAppUser.CalenderPeriods.Min(x => x.StartDate);
                maxStartDate = _iAppUser.CalenderPeriods.FirstOrDefault(x =>
                    x.StartDate >= DateTime.Now && x.EndDate >= DateTime.Now)!.EndDate;
                minEndDate = DateTime.Now; // _iAppUser.CalenderPeriods.Min(x => x.EndDate);
                maxEndDate = _iAppUser.CalenderPeriods.Max(x => x.EndDate);
                SetEndDate();
            }

            base.OnInitialized();
        }

        bool isRejected;

        protected async override Task OnInitializedAsync()
        {
            ShowLoader();

            dataService.HeaderText = "Sell In Scheme ";
            await _viewModel.PopulateViewModel();
            if (SchemeConstants.Approved.Equals(_viewModel._SellInSchemeDTO!.SellInHeader.Status,
                    StringComparison.OrdinalIgnoreCase))
            {
                DisableFields(true, true);
            }

            if (ApprovalConst.Rejected.Equals(_viewModel._SellInSchemeDTO!.SellInHeader.Status,
                    StringComparison.OrdinalIgnoreCase))
            {
                DisplaySaveButton = false;
                isRejected = true;
            }


            if (_viewModel.IsExpired)
            {
                SetExpire();
            }

            _viewModel.IsIntialize = true;
            HideLoader();
        }

        #region UI Logic

        List<IStore> stores = [];
        protected bool _showAllCustomers;

        private void OnShowAllClick()
        {
            stores.Clear();
            stores.AddRange(_viewModel.AllCustomersByFilters());
            _showAllCustomers = true;
            StateHasChanged();
        }

        void SetExpire()
        {
            DisableAddProduct = true;
            DisableEndDate = true;
            DisableAddProduct = true;
            DisableChannelPartner = true;
            DisableStartDate = true;
            DisableEndDate = true;
            DisableTillMonthEnd = true;
            DisableCommittedQTY = true;
            DisableDealerRequestedPrice = true;
            DisableApplyAdditionalDicount = true;
        }

        private void ApplyDiscountToAllLinesOfItems()
        {
            if (_viewModel._SellInSchemeDTO!.SellInSchemeLines == null ||
                _viewModel._SellInSchemeDTO.SellInSchemeLines.Count == 0)
            {
                return;
            }

            _viewModel._SellInSchemeDTO.SellInSchemeLines.ForEach(p =>
            {
                if (DiscountApplicableToAllItems.CommittedQtySelected)
                    p.CommittedQty = DiscountApplicableToAllItems.CommittedQty;

                if (DiscountApplicableToAllItems.InvoiceDiscountSelected)
                {
                    p.InvoiceDiscount = DiscountApplicableToAllItems.InvoiceDiscount;
                    p.InvoiceDiscountType = DiscountApplicableToAllItems.InvoiceDiscountType;
                    if (p.InvoiceDiscountType == SelleInConstants.Percent)
                    {
                        p.Temp_InvoiceDiscount = ((p.LadderingPrice / 100) * p.InvoiceDiscount);
                    }
                }

                if (_iAppUser.Role.HasP1Access && DiscountApplicableToAllItems.CreditNoteDiscountSelected)
                {
                    p.CreditNoteDiscount = DiscountApplicableToAllItems.CreditNoteDiscount;
                    p.CreditNoteDiscountType = DiscountApplicableToAllItems.CreditNoteDiscountType;
                    if (p.CreditNoteDiscountType == SelleInConstants.Percent)
                    {
                        p.Temp_CreditNoteDiscount = ((p.LadderingPrice / 100) * p.CreditNoteDiscount);
                    }
                }

                if (_iAppUser.Role.HasP2Access && DiscountApplicableToAllItems.P2Selected)
                {
                    p.ProvisionAmount2 = DiscountApplicableToAllItems.ProvisionAmount2;
                }

                if (_iAppUser.Role.HasP3Access && DiscountApplicableToAllItems.P3Selected)
                {
                    p.ProvisionAmount3 = DiscountApplicableToAllItems.ProvisionAmount3;
                }

                _viewModel.CalculateFinalDealerPriceMargin(p);
            });
            DiscountApplicableToAllItems = _serviceProvider.CreateInstance<ISellInSchemeLine>();
            DiscountApplicableToAllItems.CreditNoteDiscountType = SelleInConstants.Value;
            DiscountApplicableToAllItems.InvoiceDiscountType = SelleInConstants.Value;
        }

        private void OnTillMonthEndClick()
        {
            _viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd =
                !_viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd;
            if (_viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd)
            {
                SetEndDate();
            }
            else
            {
                _viewModel._SellInSchemeDTO!.SellInHeader.ValidUpTo = null;
            }

            DisableEndDate = _viewModel._SellInSchemeDTO.SellInHeader.IsTillMonthEnd;
        }

        private void SetEndDate()
        {
            DateTime dateTime = _viewModel._SellInSchemeDTO!.SellInHeader.ValidFrom ?? DateTime.Now;
            ICalender? calender = _iAppUser.CalenderPeriods.Find(p => p.StartDate <= dateTime && dateTime <= p.EndDate);
            _viewModel._SellInSchemeDTO.SellInHeader.ValidUpTo =
                _viewModel._SellInSchemeDTO.SellInHeader.EndDate = calender?.EndDate;
        }

        private async Task OnAddProduct_Click()
        {
            ShowLoader();
            if (await _viewModel.OnAddProduct_Click())
            {
                AddProductDialogBox?.OnOpenClick();
            }
            StateHasChanged();
            HideLoader();
        }

        void OnBroadClassificationSelected(DropDownEvent dropDownEvent)
        {
            _viewModel.OnBroadClassificationSelected(dropDownEvent);
            _viewModel.ChannelPartnerName = "Select Channel Partner";
            StateHasChanged();
        }

        private async void OnProduct_Delete(ISellInSchemeLine sellInSchemeLine)
        {
            if (!await _alertService.ShowConfirmationReturnType("Alert",
                    $"Are you sure do you want to delete {sellInSchemeLine.SKUName}"))
            {
                return;
            }

            if (!IsApproved)
            {
                if (sellInSchemeLine.Id == 0)
                {
                    _viewModel._SellInSchemeDTO!.SellInSchemeLines.Remove(sellInSchemeLine);
                }
                else
                {
                    sellInSchemeLine.IsDeleted = 1;
                }
            }

            StateHasChanged();
        }

        private void OnDateChange(CalenderWrappedData sender)
        {
            _viewModel._SellInSchemeDTO!.SellInHeader.ValidFrom = CommonFunctions.GetDate(sender!.SelectedValue);
        }

        private async Task GetSKUsAndPriceList_OnChannelpartnerSelection(DropDownEvent dropDownEvent)
        {
            await _viewModel.GetSKUsAndPriceList_OnChannelpartnerSelection(dropDownEvent);
            _viewModel._SellInSchemeDTO!.SellInHeader.AvailableProvision2Amount =
                CommonFunctions.GetDecimalValue(_viewModel!.Branch_P2Amount!);
            _viewModel._SellInSchemeDTO!.SellInHeader.AvailableProvision3Amount =
                CommonFunctions.GetDecimalValue(_viewModel!.HO_P3Amount!);
            _viewModel._SellInSchemeDTO!.SellInHeader.StandingProvisionAmount =
                CommonFunctions.GetDecimalValue(_viewModel!.HO_S_Amount!);
        }

        #endregion

        private decimal ValidateEnteredValue(string val)
        {
            decimal dVal = 0;
            dVal = CommonFunctions.GetDecimalValue(val);
            if (dVal < 0)
            {
                _tost.Add(title: "Alert", severity: Winit.UIComponents.SnackBar.Enum.Severity.Error,
                    message: "You are not allowed to enter negative values");
                return 0;
            }

            return dVal;
        }

        private int ValidateEnteredIntValue(string val)
        {
            int dVal = 0;
            dVal = CommonFunctions.GetIntValue(val);
            if (dVal < 0)
            {
                _tost.Add(title: "Alert", severity: Winit.UIComponents.SnackBar.Enum.Severity.Error,
                    message: "You are not allowed to enter negative values");
                return 0;
            }

            return dVal;
        }


        #region Saving Logic

        List<ISellInSchemeLine> negativeMargins = [];
        bool IsSaveNegative = false;
        bool IsSavePositive = false;
        string NegativeSchemeUID = Guid.NewGuid().ToString();
        string PositiveSchemeUID = string.Empty;

        async Task<bool> SaveNegativeScheme()
        {
            _viewModel.GetUserTypeWhileCreatingScheme(false, out string userType, out int ruleId);
            childComponentRef!.ChangeRuleID(_viewModel.RuleId = ruleId);
            _viewModel.UserType = userType;
            StateHasChanged();

            _viewModel._SellInSchemeDTO.SellInHeader.UID = NegativeSchemeUID;
            if (!_viewModel._SellInSchemeDTO.SellInHeader.Code.Contains(SchemeConstants.NegativeSchemeSuffix))
            {
                _viewModel._SellInSchemeDTO.SellInHeader.Code =
                    $"{_viewModel._SellInSchemeDTO.SellInHeader.Code}{SchemeConstants.NegativeSchemeSuffix}";
            }

            _viewModel._SellInSchemeDTO.SellInSchemeLines.Clear();
            negativeMargins.ForEach(item =>
            {
                item.SellInSchemeHeaderUID = NegativeSchemeUID;
                _viewModel._SellInSchemeDTO.SellInSchemeLines.Add(item);
            });
            _viewModel._SellInSchemeDTO.SchemeBranches.ForEach(p =>
            {
                p.LinkedItemUID = NegativeSchemeUID;
                p.UID = Guid.NewGuid().ToString();
            });
            _viewModel._SellInSchemeDTO.SchemeBroadClassifications.ForEach(p =>
            {
                p.LinkedItemUID = NegativeSchemeUID;
                p.UID = Guid.NewGuid().ToString();
            });
            _viewModel._SellInSchemeDTO.SchemeOrgs.ForEach(p =>
            {
                p.LinkedItemUID = NegativeSchemeUID;
                p.UID = Guid.NewGuid().ToString();
            });
            StateHasChanged();
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse =
                await ((SellInSchemeWebViewModel)_viewModel).Save();
            return apiResponse != null && apiResponse.IsSuccess;
        }

        private async Task Save()
        {
            ShowLoader();
            try
            {
                //if (_viewModel._SellInSchemeDTO == null || _viewModel._SellInSchemeDTO!.SellInSchemeLines == null || _viewModel._SellInSchemeDTO!.SellInSchemeLines.Count == 0)
                //{
                //    await _alertService.ShowErrorAlert("Alert", "Add atleast one item to Continue!");
                //    HideLoader();
                //    return;
                //}
                _viewModel.ValidateOnAddProduct_Click(isVal: out bool isVal, message: out string message);

                if (isVal)
                    _viewModel.IsItemsDiscountValidated(isVal: out isVal, message: out message);

                if (isVal)
                {
                    if (_viewModel.IsNew)
                    {
                        //bool isVal = _viewModel._SellInSchemeDTO.SellInSchemeLines.Any(p => p.InvoiceDiscount > 0);
                        //if (!isVal)
                        //{
                        //    await _alertService.ShowErrorAlert("Alert", "For at least one item Invoice discount should be mandatory");
                        //    HideLoader();
                        //    return;
                        //}

                        DateTime validFrom =
                            CommonFunctions.GetDate(_viewModel._SellInSchemeDTO.SellInHeader.ValidFrom.ToString()!);
                        DateTime validUpto =
                            CommonFunctions.GetDate(_viewModel._SellInSchemeDTO.SellInHeader.EndDate.ToString()!);
                        _viewModel._SellInSchemeDTO.SellInHeader.EndDate= validUpto.AddHours(23).AddMinutes(59).AddSeconds(59);
                        _viewModel._SellInSchemeDTO.SellInSchemeLines.ForEach(p =>
                        {
                            p.StartDate = validFrom;
                            p.EndDate = validUpto;
                        });

                        IsSaveNegative = _viewModel._SellInSchemeDTO.SellInSchemeLines.Any(p => p.Margin < 0);
                        IsSavePositive = _viewModel._SellInSchemeDTO.SellInSchemeLines.Any(p => p.Margin >= 0);
                        if (IsSaveNegative)
                        {
                            negativeMargins.Clear();
                            negativeMargins.AddRange(
                                _viewModel._SellInSchemeDTO.SellInSchemeLines.FindAll(p => p.Margin < 0));
                            _viewModel._SellInSchemeDTO.SellInSchemeLines.RemoveAll(p => p.Margin < 0);
                        }

                        bool isSaved = false;
                        if (IsSavePositive && _viewModel._SellInSchemeDTO.SellInSchemeLines.Count > 0)
                        {
                            Winit.Shared.Models.Common.ApiResponse<string> apiResponse =
                                await ((SellInSchemeWebViewModel)_viewModel).Save();
                            isSaved = apiResponse != null && apiResponse.IsSuccess;
                            if (apiResponse != null && apiResponse.IsSuccess)
                            {
                                PositiveSchemeUID = _viewModel._SellInSchemeDTO.SellInHeader.UID;
                            }
                            else
                            {
                                HideLoader();
                                ShowErrorSnackBar("Error", apiResponse?.ErrorMessage);
                                return;
                            }
                        }

                        if (IsSaveNegative && negativeMargins.Count > 0)
                        {
                            isSaved = await SaveNegativeScheme();
                        }

                        if (isSaved)
                        {
                            ShowSuccessSnackBar("Success", "Saved successfully");
                            _navigationManager.NavigateTo("ManageScheme");
                        }
                    }
                    else
                    {
                        Winit.Shared.Models.Common.ApiResponse<string> apiResponse =
                            await ((SellInSchemeWebViewModel)_viewModel).Save();
                        if (apiResponse != null && apiResponse.IsSuccess)
                        {
                            ShowSuccessSnackBar("Success", "Saved successfully");
                            _navigationManager.NavigateTo("ManageScheme");
                        }
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Alert", message);
                }
            }
            finally
            {
                HideLoader();
            }
        }

        private void ShowChannelPartner()
        {
            bool isBCSelected = _viewModel.BroadClassificationDDL.Any(p => p.IsSelected);
            if (isBCSelected)
            {
                _viewModel.IsDisplayChannelPartner = true;
            }
            else
            {
                ShowErrorSnackBar("Alert", "Please select atleast one Broad Classification");
            }
        }

        void OnChannelpartnerSelectedUI(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    _viewModel.ChannelPartnerName = dropDownEvent.SelectionItems.Count == 1
                        ? dropDownEvent.SelectionItems!.FirstOrDefault()!.Label
                        : $"{dropDownEvent.SelectionItems.Count} records selected";
                }
                else
                {
                    _viewModel.ChannelPartnerName = "Select Channel Partner";
                }

                _viewModel.OnChannelpartnerSelectedUI(dropDownEvent);
            }

            _viewModel.IsDisplayChannelPartner = false;
        }

        private async Task<bool> UpdateOnApproveClick()
        {
            if (!_viewModel._SellInSchemeDTO.SellInHeader.Code.Contains(SchemeConstants.NegativeSchemeSuffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (_viewModel._SellInSchemeDTO.SellInSchemeLines.Any(p => p.Margin < 0))
                {
                    await _alertService.ShowErrorAlert("Alert",
                        "Approval can not be possible when the margin in negative");
                    return false;
                }
            }

            Winit.Shared.Models.Common.ApiResponse<string> apiResponse =
                await ((SellInSchemeWebViewModel)_viewModel).Save();
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                _navigationManager.NavigateTo("ManageScheme");
            }

            return apiResponse != null && apiResponse.IsSuccess;
        }

        #endregion

        async Task OnCancel()
        {
            if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to cancel?"))
            {
                _navigationManager.NavigateTo("ManageScheme");
            }
        }

        #region Discount Type Change Logic

        private void OnInvoiceDiscountTypeChange(ISellInSchemeLine sellInSchemeLine, string discountType)
        {
            if (!discountType.Equals(sellInSchemeLine.InvoiceDiscountType))
            {
                sellInSchemeLine.InvoiceDiscountType = discountType;
                sellInSchemeLine.InvoiceDiscount = 0;
                sellInSchemeLine.Temp_InvoiceDiscount = 0;
                sellInSchemeLine.Margin = 0;
                _viewModel.OnInvoiceDiscountEnter(sellInSchemeLine, string.Empty);
            }

            StateHasChanged();
        }

        private void OnDiscountTypeChange_UI(bool isInvoice, string discountType)
        {
            if (isInvoice)
            {
                DiscountApplicableToAllItems.InvoiceDiscountType = discountType;
                DiscountApplicableToAllItems.InvoiceDiscount = 0;
            }
            else
            {
                DiscountApplicableToAllItems.CreditNoteDiscountType = discountType;
                DiscountApplicableToAllItems.CreditNoteDiscount = 0;
            }

            StateHasChanged();
        }

        private void OnCreditNoteDiscountTypeChange(ISellInSchemeLine sellInSchemeLine, string discountType)
        {
            if (!discountType.Equals(sellInSchemeLine.CreditNoteDiscountType))
            {
                sellInSchemeLine.CreditNoteDiscountType = discountType;
                sellInSchemeLine.CreditNoteDiscount = 0;
                sellInSchemeLine.Temp_CreditNoteDiscount = 0;
                _viewModel.OnCreditNoteEnter(sellInSchemeLine, string.Empty);
            }

            StateHasChanged();
        }

        #endregion

        #region Approval Engine

        //async Task<bool> OnApproveClick(bool isFinalArroval)
        //{
        //    ShowLoader();
        //    if (isFinalArroval)
        //    {
        //        _viewModel._SellInSchemeDTO.SellInHeader.Status = SchemeConstants.Approved;
        //    }
        //    bool isUpdated = await UpdateOnApproveClick();
        //    if (isUpdated)
        //    {
        //        _navigationManager.NavigateTo("ManageScheme");
        //    }
        //    HideLoader();
        //    return isUpdated;
        //}
        //async Task SaveApprovalRequestDetails(string requestId, string linkedItemUID, string linkedItemType)
        //{
        //    if (IsSavePositive)
        //    {
        //        if (await _viewModel.SaveApprovalRequestDetails(requestId: requestId.ToString(), linkedItemUID: PositiveSchemeUID, linkedItemType: "Scheme_SellIn", userHierarchyType: UserHierarchyTypeConst.StoreBM, hierarchyUID: _viewModel.SelectedChannelPartner!.UID))
        //        {
        //            IsSavePositive = false;
        //        }
        //    }
        //    else if (IsSaveNegative)
        //    {
        //        await _viewModel.SaveApprovalRequestDetails(requestId: requestId.ToString(), linkedItemUID: NegativeSchemeUID, linkedItemType: "Scheme_SellIn", userHierarchyType: UserHierarchyTypeConst.StoreBM, hierarchyUID: _viewModel.SelectedChannelPartner!.UID);
        //    }
        //}


        bool isLoadedOnInitialized;

        public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses)
        {
            try
            {
                if (!isLoadedOnInitialized)
                {
                    isLoadedOnInitialized = true;
                    ApprovalStatusResponse approvalStatusResponse = approvalStatusResponses.FirstOrDefault(p =>
                        p.ApproverId.Equals(_iAppUser.Role.Code, StringComparison.OrdinalIgnoreCase));
                    if (approvalStatusResponse == null)
                    {
                        DisableFields(true);
                        return;
                    }

                    bool isApprovalProcessStarted = approvalStatusResponses.Any(p =>
                        SchemeConstants.Approved.Equals(p.Status, StringComparison.OrdinalIgnoreCase));
                    bool finalLevelApproved = !approvalStatusResponses.Any(p => p.Status != SchemeConstants.Approved);

                    DisableFields(
                        SchemeConstants.Approved.Equals(approvalStatusResponse.Status,
                            StringComparison.OrdinalIgnoreCase), finalLevelApproved);
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

        private void DisableFields(bool isCurrentRoleApproved = false, bool finalLevelApproved = false)
        {
            DisableAddProduct = true;
            DisableChannelPartner = true;
            DisableStartDate = true;
            DisableEndDate = true;
            DisableTillMonthEnd = true;
            DisableCommittedQTY = true;
            DisableDealerRequestedPrice = true;
            DisableApplyAdditionalDicount = true;
            DisableInvoiceDicount = isCurrentRoleApproved;
            if (_iAppUser.Role.HasP1Access)
            {
                DisableP1 = isCurrentRoleApproved;
            }

            if (_iAppUser.Role.HasP2Access)
            {
                DisableP2 = isCurrentRoleApproved;
            }

            if (_iAppUser.Role.HasP3Access)
            {
                DisableP3 = isCurrentRoleApproved;
            }

            HideDelete = true;
            DisplaySaveButton = false;
            DisableEndDate = !finalLevelApproved;
        }

        #endregion

        private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
        {
            ShowLoader();
            ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
            {
                IsApprovalActionRequired = true
            };
            if (_viewModel.IsExpired)
            {
                approvalActionResponse.IsApprovalActionRequired = true;
                approvalActionResponse.IsSuccess = false;
                return approvalActionResponse;
            }

            try
            {
                _viewModel._SellInSchemeDTO.ApprovalStatusUpdate = approvalStatusUpdate;
                if (approvalStatusUpdate.IsFinalApproval)
                {
                    if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                    {
                        _viewModel._SellInSchemeDTO.SellInHeader.Status = ApprovalConst.Rejected;
                    }
                    else if (approvalStatusUpdate.Status == ApprovalConst.Approved)
                    {
                        _viewModel._SellInSchemeDTO.SellInHeader.Status = SchemeConstants.Approved;
                        _viewModel._SellInSchemeDTO.SellInHeader.ApprovedTime = DateTime.Now;
                        _viewModel._SellInSchemeDTO.SellInHeader.ApprovedBy = _appUser.Emp.UID;
                    }
                }
                else
                {
                    if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                    {
                        _viewModel._SellInSchemeDTO.SellInHeader.Status = SchemeConstants.Rejected;
                    }
                }

                if (approvalStatusUpdate.Status != ApprovalConst.Reassign)
                {
                    approvalActionResponse.IsSuccess = await UpdateOnApproveClick();
                }
            }
            catch (CustomException ex)
            {
                approvalActionResponse.IsSuccess = false;
            }
            finally
            {
                HideLoader();
            }

            return approvalActionResponse;
        }
    }
}