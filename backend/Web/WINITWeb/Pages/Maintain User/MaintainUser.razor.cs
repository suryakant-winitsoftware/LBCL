using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Common;
using Winit.Modules.Common.UIState.Classes;

namespace WinIt.Pages.Maintain_User
{
    public partial class MaintainUser
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsAddNewPopUp { get; set; }
        public bool IsEditPopUp { get; set; }
        public bool IsDisablePopUp { get; set; }
        public bool IsResetPasswordPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        public string validationMessage;
        public string UID { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public string OrgUID { get; set; }
        private List<ISelectionItem> StatusSelectionItems = new List<ISelectionItem>
    {
        new SelectionItem{UID="Active",Code="Active",Label="Active"},
        new SelectionItem{UID="Inactive",Code="Inactive",Label="Inactive"},
    };

        private bool IsChangePassWordPopUpOpen { get; set; }
        private string? ConfirmPassword { get; set; }
        private string? ErrorMsg { get; set; }
        private Winit.Modules.Auth.Model.Interfaces.IChangePassword ChangePassword { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain User",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain User"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _maintainusersViewModel.PageSize = 10;
            OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
            await _maintainusersViewModel.GetSalesman(OrgUID);
            await _maintainusersViewModel.PopulateViewModel();
            _maintainusersViewModel.EmpDTOmaintainUser = _serviceProvider.CreateInstance<IEmpDTO>();
            _maintainusersViewModel.EmpDTOmaintainUser.EmpInfo = _serviceProvider.CreateInstance<IEmpInfo>();
            _maintainusersViewModel.EmpDTOmaintainUser.Emp = _serviceProvider.CreateInstance<IEmp>();
            IsLoaded = true;
            await GenerateGridColumns();
            //await SetHeaderName();
            await StateChageHandler();
            HideLoader();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("MaintainUsers", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainUsers");
        }
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
            {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["user_name"], DropDownValues=_maintainusersViewModel.EmpSelectionList,ColumnName="UID",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["status"],DropDownValues=StatusSelectionItems, ColumnName="Status",SelectionMode=SelectionMode.Multiple},
        };
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _pageStateHandler._currentFilters = filterCriteria;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "UID")
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedLabels = _maintainusersViewModel.EmpSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.UID).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = _maintainusersViewModel.EmpSelectionList.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.UID, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == "Status")
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", values, FilterType.In));
                        }
                        else
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                        }
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                    }
                }
            }
            _maintainusersViewModel.PageNumber = 1;
            await _maintainusersViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_users"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["maintain_users"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["login_id"], GetValue = s => ((IMaintainUser)s)?.LoginId ?? "N/A" ,IsSortable=true,SortField="LoginId"},
                new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((IMaintainUser)s)?.Code ?? "N/A",IsSortable=true,SortField="Code" },
                new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((IMaintainUser)s)?.Name?? "N/A",IsSortable=true,SortField="Name" },
                new DataGridColumn { Header = @Localizer["phone"], GetValue = s => ((IMaintainUser)s)?.Phone ?? "N/A",IsSortable=true,SortField="Phone" },
                new DataGridColumn { Header = "ApprovalStatus", GetValue = s => ((IMaintainUser)s)?.ApprovalStatus ?? "N/A",IsSortable=true,SortField="ApprovalStatus" },
                new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((IMaintainUser)s)?.Status ?? "N/A",IsSortable=true,SortField="Status" },
            new DataGridColumn
             {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                //ButtonActions = this.buttonActions
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                            Action = async item => await OnEditUsersClick((IMaintainUser)item),

                        },
                        // new ButtonAction
                        //{
                        //    ButtonType = ButtonTypes.Text,
                        //    Text=GetButtonActionText(),
                        //    Action = item => OnDisableClick((IMaintainUser)item),
                        //},
                          new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/Reset.png",
                            Action =  item =>  OnResetPwdClick(((IMaintainUser)item).LoginId),
                          },
                           new ButtonAction
                        {
                             ButtonType = ButtonTypes.Text,
                             Text=@Localizer["change_password"],
                            Action =  async item =>  await OnUpdatePwdClick(((IMaintainUser)item).UID),
                          }
                }
            }
             };
        }
        private async Task OnUpdatePwdClick(string empUID)
        {
            ChangePassword = new Winit.Modules.Auth.Model.Classes.ChangePassword();
            ChangePassword.EmpUID = empUID;
            IsChangePassWordPopUpOpen = true;
            StateHasChanged();
            await Task.CompletedTask;
        }
        private async Task OnChangePasswordClick()
        {
            var user = (await AuthenticationState).User;
            if (string.IsNullOrEmpty(ChangePassword.NewPassword))
            {
                ErrorMsg = @Localizer["please_enter_new_password"];
                return;
            }
            if (ChangePassword.NewPassword != ConfirmPassword)
            {
                ErrorMsg = @Localizer["password_miss_match"];
                return;
            }
            try
            {
                ShowLoader();
                ChangePassword.NewPassword = ChangePassword.NewPassword;
                string response = await _maintainusersViewModel.UpdateNewPassword(ChangePassword);
                IsChangePassWordPopUpOpen = false;
                ErrorMsg = null;
                HideLoader();
                ShowSuccessSnackBar(@Localizer["success"], response);
                ConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
            }
        }
        private string GetButtonActionText()
        {
            bool hasActiveStatus = false;
            bool hasInactiveStatus = false;
            foreach (var user in _maintainusersViewModel.maintainUsersList)
            {
                if (user.Status == "Active")
                {
                    hasActiveStatus = true;
                }
                else if (user.Status == "Inactive")
                {
                    hasInactiveStatus = true;
                }
            }
            if (hasActiveStatus && hasInactiveStatus)
            {
                return @Localizer["enable/disable"];
            }
            else if (hasActiveStatus)
            {
                return @Localizer["disable"];
            }
            else if (hasInactiveStatus)
            {
                return @Localizer["enable"];
            }
            else
            {
                // Ideally, you should handle the case where there are no users or all users have the same status.
                // You could return an informative message like "No users to manage" or "All users are Active/Inactive". 
                return "Unknown"; // Or a more informative message
            }
        }
        public async Task AddNewUsers()
        {
            //_maintainusersViewModel.maintainUsersList.Any(u => u.Code == code || u.LoginId == loginId || u.EmpNo == empNo)
            IsAddNewPopUp = true;
            _navigationManager.NavigateTo($"AddEditEmployee");
        }
        public async Task OnEditUsersClick(IMaintainUser userUID)

        {
            _navigationManager.NavigateTo($"AddEditEmployee?LoginID={userUID.UID}");
        }
        //public async void OnDisableClick(IMaintainUser userUID)
        //{
        //    UID = userUID.UID;
        //    if (UID != null)
        //    {
        //        IsDisablePopUp = true;
        //        await _maintainusersViewModel.PopulateUsersDetailsforEdit(UID);
        //    }
        //    StateHasChanged();
        //}
        private async Task OnOkDisbaleBtnPopUpClick()
        {
            IsDisablePopUp = false;
            await _maintainusersViewModel.DisableDataFromGridview(_maintainusersViewModel.EmpDTOmaintainUser, false);
            await Task.Delay(500);
            await _maintainusersViewModel.PopulateViewModel();
            StateHasChanged();
            _tost.Add(@Localizer["user"], _maintainusersViewModel.EmpDTOmaintainUser.Emp.Status == "Active" ? @Localizer["user_enabled_successfully"] : @Localizer["user_disabled_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        public async Task OnResetPwdClick(string UserId)
        {
            // IsResetPasswordPopUp = true;
            if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["are_you_sure_you_want_to_reset_your_password?"]))
            {
                var resposne = await _loginViewModel.VerifyUserIdAndSendRandomPassword(UserId);
                if (resposne.IsSuccessResponse)
                {
                    ShowSuccessSnackBar(@Localizer["success"], resposne.Msg);
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], resposne.Msg);
                }
            }

            StateHasChanged();
        }
        public void OnCloseResetPopUpclick()
        {
            IsResetPasswordPopUp = false;
        }
        private void OnBackBtnPopUpClick()
        {
            IsBackBtnPopUp = false;
            // IsEditPopUp = true;
            IsAddNewPopUp = true;
        }
        private void OnOkBtnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsAddNewPopUp = false;
            IsEditPopUp = false;
            IsResetPasswordPopUp = false;
        }
        private void OnCancelDisbaleBtnPopUpClick()
        {
            IsDisablePopUp = false;
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _maintainusersViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}
