
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;


namespace WinIt.Pages.Administration.Roles
{
    public partial class MaintainUserRole
    {
        bool isdisabled { get; set; }
        List<DataGridColumn> productColumns = new List<DataGridColumn>()
        {
           new DataGridColumn { Header ="Role Name",GetValue=s=>((IRole)s).RoleNameEn,  IsSortable = true,SortField=nameof(IRole.RoleNameEn)},
           new DataGridColumn { Header ="Alias Name",GetValue=s=>((IRole)s).RoleNameOther,  IsSortable = false},
           new DataGridColumn { Header ="Parent Role", GetValue=s=>((IRole)s).ParentRoleName??"N/A", IsSortable = true,SortField=nameof(IRole.ParentRoleName)},
           new DataGridColumn { Header ="Is Principal Role",GetValue=s=>((IRole)s).IsPrincipalRole?"Yes":"No",  IsSortable = false},
           new DataGridColumn { Header ="Is Admin",GetValue=s=>((IRole)s).IsAdmin?"Yes":"No",  IsSortable = false},
           new DataGridColumn { Header ="Principle to Distributor Access",GetValue=s=>((IRole)s).BuToDistAccess?"Yes":"No",  IsSortable = false},
           new DataGridColumn { Header ="Is App Role",GetValue=s=>((IRole)s).IsAppUser?"Yes":"No",  IsSortable = false},
           new DataGridColumn { Header ="Is Web Role",GetValue=s=>((IRole)s).IsWebUser?"Yes":"No",  IsSortable = false},
        };
        protected override void OnInitialized()
        {
            productColumns.Add(new()
            {
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new()
                    {
                        ButtonType=ButtonTypes.Text,
                        Text="Manage web menu",
                        IsVisible=false,
                        ConditionalVisibility=s=>((IRole)s).IsWebUser,
                        Action=s=> Redirect((IRole)s,false)
                    },
                    new()
                    {
                        ButtonType=ButtonTypes.Text,
                        Text="Manage mobile menu",
                        IsVisible=false,
                        ConditionalVisibility=s=>((IRole)s).IsAppUser,
                        Action=s=> Redirect((IRole)s,true)
                    },
                    new()
                    {
                        ButtonType=ButtonTypes.Image,
                        URL="Images/edit.png",
                       Action=s=> View((IRole)s),
                    },
                }

            });

            base.OnInitialized();
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
            {
                _pageStateHandler._currentFilters = filterCriteria;
                await _maintainUserRoleBL.OnFilterApply(filterCriteria);
            }
            catch (Exception ex)
            {

            }
        }
        void View(IRole role)
        {
            _createUserRoleBL.AddOrEditRole(false, _maintainUserRoleBL.RoleList, role);
            StateHasChanged();
        }

        private void Redirect(IRole role, bool isMobileMenu)
        {
            if (isMobileMenu)
            {
                _navigationManager.NavigateTo($"maintainmobilemenu?RoleUID={role.UID}&RoleType={role.IsPrincipalRole}&RoleName={role.RoleNameEn}");
            }
            else
            {
                _navigationManager.NavigateTo($"MaintainWebMenu?RoleUID={role.UID}&RoleType={role.IsPrincipalRole}&RoleName={role.RoleNameEn}");
            }
        }
        protected override async Task OnInitializedAsync()
        {

            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            SetHeaderName();
            await _maintainUserRoleBL.PopulateViewModel();
            _createUserRoleBL.PopulateViewModel(_maintainUserRoleBL.RoleList);
            _loadingService.HideLoading();
            isdisabled = CommonFunctions.GetBooleanValue(_iAppUser?.Role?.Code?.Equals("Distributor Admin"));
            if (isdisabled)
            {
                // _createUserRoleBL.UserRole?.is_principal_role = false;
                // _createUserRoleBL.UserRole?.is_distributor_role = true;
            }
            else
            {
                //_createUserRoleBL.UserRole.is_principal_role = true;
                //_createUserRoleBL.UserRole.is_distributor_role = false;
            }
            await StateChageHandler();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("maintainUserRole", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("maintainUserRole");
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService iDataBreadcrumbService;
        public void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = Localizer["maintain_user_roles"],
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_roles"], IsClickable = true, URL = "maintainUserRole" },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_web_menu"] }
                }
            };
        }
        protected async Task Save()
        {
            if (await _createUserRoleBL.SaveOrUpdate() > 0)
            {
                if (_createUserRoleBL.IsNew)
                {
                    _maintainUserRoleBL.RoleList.Add(_createUserRoleBL.UserRole);
                }
            }
        }

        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }


        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = "RoleNameEn", Label = @Localizer["role_name"] },
             };
        }
        protected void ISAppRole()
        {
            _createUserRoleBL.UserRole.IsAppUser = !_createUserRoleBL.UserRole.IsAppUser;
            if (!_createUserRoleBL.UserRole.IsAppUser)
            {
                _createUserRoleBL.UserRole.HaveVehicle = false;
                _createUserRoleBL.UserRole.HaveWarehouse = false;
            }
        }

        private async Task OnPageChange(int pageNumber)
        {
            ShowLoader();
            await _maintainUserRoleBL.OnPageChange(pageNumber);
            HideLoader();
            StateHasChanged();
        }
        private async Task OnSort(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _maintainUserRoleBL.OnSort(sortCriteria);
            HideLoader();
            StateHasChanged();
        }
    }
}
