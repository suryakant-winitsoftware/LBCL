using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Constants;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;


namespace WinIt.Pages.Scheme
{
    public partial class ManageHOProvisionStandingConfiguration
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            HeaderText = "Manage HO provision standing configuration",
            BreadcrumList = new()
            {
                new BreadCrumModel(){SlNo=1,Text="Manage HO provision standing configuration",}
            }
        };
        List<FilterModel> ColumnsForFilter = [];
        List<DataGridColumn> Columns = new()
        {
            new DataGridColumn{Header="Code",GetValue=s=>((IStandingProvisionScheme)s).Code},
            new DataGridColumn{Header="Description",GetValue=s=>((IStandingProvisionScheme)s).Description},
            new DataGridColumn{Header="Category",GetValue=s=>((IStandingProvisionScheme)s).SKUCategoryName},
            new DataGridColumn{Header="Type",GetValue=s=>((IStandingProvisionScheme)s).SKUTypeName},
            new DataGridColumn{Header="Star Rating",GetValue=s=>((IStandingProvisionScheme)s).StarRatingName},
            new DataGridColumn{Header="Tonage", GetValue = s =>((IStandingProvisionScheme) s).SKUTonnageName},
            new DataGridColumn{Header="Series",GetValue=s=>((IStandingProvisionScheme)s).SKUSeriesName},
            new DataGridColumn{Header="Capacity", GetValue = s =>((IStandingProvisionScheme) s).StarCapacityName},
            new DataGridColumn{Header="Amount", GetValue = s =>CommonFunctions.RoundForSystem(((IStandingProvisionScheme) s).Amount)},
            new DataGridColumn{Header="Start Date", GetValue = s =>CommonFunctions.GetDateTimeInFormat(((IStandingProvisionScheme) s).StartDate)},
            new DataGridColumn{Header="End Date", GetValue = s =>CommonFunctions.GetDateTimeInFormat(((IStandingProvisionScheme) s).EndDate)},
            //new DataGridColumn{Header="CreatedBy", GetValue = s =>((IStandingProvisionScheme) s).CreatedByEmpName??"N/A"},
            new DataGridColumn{Header="CreatedOn",GetValue=s=>CommonFunctions.GetDateTimeInFormat(((IStandingProvisionScheme) s).CreatedTime)},
            new DataGridColumn{Header="Status",GetValue=s=>(((IStandingProvisionScheme) s).Status)},

        };
        List<DataGridColumn> historyColumns = new List<DataGridColumn>()
        {
           //new DataGridColumn { Header ="Id",GetValue=s=>((ISchemeExtendHistory)s).Id,  IsSortable = false},
           //new DataGridColumn { Header ="Channel Partner",GetValue=s=>((IManageScheme)s).ChannelPartner,  IsSortable = false},
           //new DataGridColumn { Header ="Branch", GetValue=s=>((IManageScheme)s).Branch??"N/A", IsSortable = false},
           new DataGridColumn { Header ="Action Type",GetValue=s=>((ISchemeExtendHistory)s).ActionType,  IsSortable = false},
           new DataGridColumn { Header ="Old Date",GetValue=s=>CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).OldDate),  IsSortable = false},
           new DataGridColumn { Header ="New Date",GetValue=s=>CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).OldDate),  IsSortable = false},
           new DataGridColumn { Header ="Updated ON",GetValue=s=>(CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).UpdatedOn)),  IsSortable = false},
           new DataGridColumn { Header ="Updated By",GetValue=s=>((ISchemeExtendHistory)s).UpdatedBy,  IsSortable = false},
           new DataGridColumn { Header ="Comments",GetValue=s=>((ISchemeExtendHistory)s).Comments,  IsSortable = false},
        };
        protected override void OnInitialized()
        {

        }
        bool IsShowPopup { get; set; }
        bool IsShowViewHistoryPopup { get; set; }
        bool IsExpire { get; set; }
        bool IsExtend { get; set; }
        string Title = string.Empty;
        DateTime OriginalEndDate = DateTime.Now;
        DateTime NewEndDate = DateTime.Now;
        protected async override Task OnInitializedAsync()
        {
            ShowLoader();
            await base.OnInitializedAsync();
            await _viewModel.PopulateViewModel();
            SetFilters();
            Columns.Add(new()
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new ()
                    {
                        ButtonType =ButtonTypes.Image,
                        URL="Images/view.png",
                        Action=s=>ViewOrEdit((IStandingProvisionScheme)s,PageType.View),
                    },
                    new (){
                        ButtonType =ButtonTypes.Text,
                        URL="Images/edit.png",
                        Text="Expire",
                        IsVisible=false,
                        ConditionalVisibility=s=>IsPermitted(_scheme:(IStandingProvisionScheme)s),
                        Action=s=>ExtendorExpire(_scheme:(IStandingProvisionScheme)s,isExtend:false)
                    },
                    new (){
                        ButtonType =ButtonTypes.Text,
                        URL="Images/edit.png",
                         Text="Extend", IsVisible=false,
                        ConditionalVisibility=s=>IsPermitted(_scheme:(IStandingProvisionScheme)s),
                        Action=s=>ExtendorExpire(_scheme:(IStandingProvisionScheme)s,isExtend:true)
                    },
                    new (){
                        ButtonType =ButtonTypes.Text,
                        URL="Images/edit.png",
                         Text="View History", IsVisible=false,
                        ConditionalVisibility=s=>((IStandingProvisionScheme)s).HasHistory,
                        Action=async s=>await ViewHistory((IStandingProvisionScheme)s)
                    },
                }
            });
            await StateChageHandler();
            HideLoader();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("ManageHOProvisionStandingConfiguration", ref ColumnsForFilter, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ManageHOProvisionStandingConfiguration");
        }
        bool IsPermitted(IStandingProvisionScheme _scheme)
        {
            List<string> strings = _appSetting.EndDateUpdatePermittedRoles.Split(",").ToList();
            bool isFututureEndDate = _scheme.EndDate > DateTime.Now,
                isApproved = SchemeConstants.Approved.Equals(_scheme.Status, StringComparison.OrdinalIgnoreCase),
                isUserAllowed = false
                ;
            if (strings.Count > 0)
            {
                foreach (var item in strings)
                {
                    if (_appUser.Role.Code.Equals(item, StringComparison.OrdinalIgnoreCase))
                    {
                        isUserAllowed = true;
                    }
                }
            }
            return isUserAllowed && isFututureEndDate && isApproved;
        }
        protected void ViewOrEdit(IStandingProvisionScheme standingProvisionScheme, string pageType)
        {
            _navigationManager.NavigateTo($"AddHOProvisionStandingConfiguration?{PageType.Page}={pageType}&UID={standingProvisionScheme.UID}");
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            _pageStateHandler._currentFilters = filterCriteria;
            await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
        }
        async Task OnPageChange(int pageNumber)
        {
            _loadingService.ShowLoading();
            await _viewModel.OnPageChange(pageNumber);
            StateHasChanged();
            _loadingService.HideLoading();
        }
        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(IStandingProvisionScheme.Code),
                     Label = "Code"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(IStandingProvisionScheme.Description),
                     Label = "Description"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.Category),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.SKUCategoryCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Category"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.Product_Type),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.SKUTypeCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Type"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.StarRating),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.StarRatingCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Star Rating"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.TONAGE),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.SKUTonnageCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Tonnage"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.Item_Series),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.SKUSeriesCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Series"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=GetSkuGroupByGroupTypeCode(SKUGroupTypeContants.Capacity),
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                     ColumnName = nameof(IStandingProvisionScheme.StarCapacityCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Capacity"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = nameof(IStandingProvisionScheme.StartDate),
                     Label = "From Date"
                 },
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = nameof(IStandingProvisionScheme.EndDate),
                     Label = "To Date"
                 },
             };
        }
        List<ISelectionItem> GetSkuGroupByGroupTypeCode(string groupTypeCode)
        {
            string skuGroupTypeUID = _viewModel.SKUGroupType.Find(p => p.Code == groupTypeCode)!.UID;
            return _viewModel!.SKUGroup!.FindAll(p => p.SKUGroupTypeUID == skuGroupTypeUID)!.
                Select(p => new SelectionItem
                {
                    UID = p.UID,
                    Label = p.Name,
                    Code = p.Code,
                }).ToList<ISelectionItem>();
        }
        async Task HandleOkClick()
        {

            if (string.IsNullOrEmpty(scheme.EndDateRemarks))
            {
                _tost.Add("Alert", "Please enter reason", Winit.UIComponents.SnackBar.Enum.Severity.Normal);
                return;
            }
            ShowLoader();
            IsExtend = IsExpire = IsShowPopup = false;
            scheme.EndDateUpdatedByEmpUID = _appUser.Emp.UID;
            scheme.ModifiedBy = _appUser.Emp.UID;
            scheme.HasHistory = true;
            string status = scheme.Status;
            string actionType = string.Empty;
            if (IsExpire)
            {
                scheme.Status = SchemeConstants.Expired;
                scheme.EndDate = DateTime.Now;
                actionType = SchemeConstants.Extend;
            }
            else
            {
                actionType = SchemeConstants.Extend;
                scheme.EndDate = NewEndDate;
            }
            scheme.SchemeExtendHistory = new SchemeExtendHistory()
            {
                SchemeType = SchemeConstants.StandingProvision,
                ActionType = actionType,
                SchemeUid = scheme.UID,
                OldDate = OriginalEndDate,
                NewDate = NewEndDate,
                Comments = scheme.EndDateRemarks,
                UpdatedByEmpUid = _appUser.Emp.UID,
                UpdatedOn = DateTime.Now,

                UID = Guid.NewGuid().ToString(),
            };
            var resp = await ((ManageStandingProvisionWebViewModel)_viewModel).ChangeEndDate(scheme);
            if (resp != null)
            {
                if (resp.IsSuccess)
                {
                    IsShowPopup = false;
                    _viewModel.StandingProvisionSchemes.ForEach(p =>
                    {
                        if (p.UID == scheme.UID)
                        {
                            p.EndDate = scheme.EndDate;
                            if (IsExpire)
                            {
                                p.Status = scheme.Status;
                            }
                        }
                    });
                    _tost.Add("Success", "Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _viewModel.StandingProvisionSchemes.ForEach(p =>
                    {
                        if (p.UID == scheme.UID)
                        {
                            p.EndDate = OriginalEndDate;
                            if (IsExpire)
                            {
                                p.Status = status;
                            }
                        }
                    });
                    _tost.Add("Error", resp.ErrorMessage, Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            HideLoader();
            StateHasChanged();
        }
        IStandingProvisionScheme scheme { get; set; }
        void ExtendorExpire(IStandingProvisionScheme _scheme, bool isExtend)
        {
            this.scheme = _scheme;
            scheme.EndDateRemarks = string.Empty;
            OriginalEndDate = CommonFunctions.GetDate(_scheme.EndDate.ToString());
            if (isExtend)
            {
                IsExpire = false;
                IsExtend = true;
                Title = "Extend Standing Provision End Date";
            }
            else
            {
                IsExtend = false;
                Title = "Expire Standing Provision ";
                IsExpire = true;
            }
            IsShowPopup = true;
            StateHasChanged();
        }
        async Task ViewHistory(IStandingProvisionScheme standingProvision)
        {
            ShowLoader();
            await _viewModel.GetschemeExtendHistoryBySchemeUID(standingProvision.UID);
            IsShowViewHistoryPopup = true;
            HideLoader();
            StateHasChanged();
        }

    }
}
