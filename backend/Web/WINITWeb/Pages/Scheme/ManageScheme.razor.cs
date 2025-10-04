using DocumentFormat.OpenXml.Spreadsheet;
using Nest;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Scheme
{
    public partial class ManageScheme
    {
        List<DataGridColumn> productColumns = new List<DataGridColumn>()
        {
            new DataGridColumn { Header = "Code", GetValue = s => ((IManageScheme)s).Code, IsSortable = false },
            //new DataGridColumn { Header ="Channel Partner",GetValue=s=>((IManageScheme)s).ChannelPartner,  IsSortable = false},
            //new DataGridColumn { Header ="Branch", GetValue=s=>((IManageScheme)s).Branch??"N/A", IsSortable = false},
            new DataGridColumn
                { Header = "Scheme Type", GetValue = s => ((IManageScheme)s).SchemeType, IsSortable = false },
            new DataGridColumn
                { Header = "Created By", GetValue = s => ((IManageScheme)s).EmpName, IsSortable = false },
            new DataGridColumn
            {
                Header = "Created ON",
                GetValue = s => (CommonFunctions.GetDateTimeInFormat(((IManageScheme)s).CreatedOn)), IsSortable = false
            },
            new DataGridColumn
            {
                Header = "Valid From",
                GetValue = s => (CommonFunctions.GetDateTimeInFormat(((IManageScheme)s).ValidFrom)), IsSortable = false
            },
            new DataGridColumn
            {
                Header = "Valid Upto",
                GetValue = s => (CommonFunctions.GetDateTimeInFormat(((IManageScheme)s).ValidUpto)), IsSortable = false
            },
            new DataGridColumn
            {
                Header = "LastUpdated",
                GetValue = s =>
                    (CommonFunctions.GetDateTimeInFormat(((IManageScheme)s).LastUpdated, "dd MMM, yyyy HH:mm:ss")),
                IsSortable = false
            },
            new DataGridColumn { Header = "Status", GetValue = s => ((IManageScheme)s).Status, IsSortable = false },
        };

        List<DataGridColumn> historyColumns = new List<DataGridColumn>()
        {
            //new DataGridColumn { Header ="Id",GetValue=s=>((ISchemeExtendHistory)s).Id,  IsSortable = false},
            //new DataGridColumn { Header ="Channel Partner",GetValue=s=>((IManageScheme)s).ChannelPartner,  IsSortable = false},
            //new DataGridColumn { Header ="Branch", GetValue=s=>((IManageScheme)s).Branch??"N/A", IsSortable = false},
            new DataGridColumn
                { Header = "Action Type", GetValue = s => ((ISchemeExtendHistory)s).ActionType, IsSortable = false },
            new DataGridColumn
            {
                Header = "Old Date",
                GetValue = s => CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).OldDate),
                IsSortable = false
            },
            new DataGridColumn
            {
                Header = "New Date",
                GetValue = s => CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).NewDate),
                IsSortable = false
            },
            new DataGridColumn
            {
                Header = "Updated ON",
                GetValue = s => (CommonFunctions.GetDateTimeInFormat(((ISchemeExtendHistory)s).UpdatedOn)),
                IsSortable = false
            },
            new DataGridColumn
                { Header = "Updated By", GetValue = s => ((ISchemeExtendHistory)s).UpdatedBy, IsSortable = false },
            new DataGridColumn
                { Header = "Comments", GetValue = s => ((ISchemeExtendHistory)s).Comments, IsSortable = false },
        };

        List<ISelectionItem> SchemesRoutes = new()
        {
            new SelectionItem()
                { UID = SchemeConstants.Sell_In, Code = "SellInSchemeBranchView", Label = "Sell In(P)" },
            new SelectionItem() { UID = SchemeConstants.QPS, Code = "QPSScheme/true", Label = "QPS Scheme(P)" },
            new SelectionItem()
                { UID = SchemeConstants.Sell_Out, Code = "SelloutLiquidation", Label = "Sell Out Liquidation(D)" },
            new SelectionItem()
            {
                UID = SchemeConstants.SellOutRealSecondary, Code = "SelloutrealSecondaryScheme",
                Label = "Sell Out on Actual Secondary Scheme(D)"
            },
            new SelectionItem()
                { UID = SchemeConstants.Sales_Promotion, Code = "salespromotion", Label = "Sales Promotion(D)" },
        };

        string SelectedSchemePath = string.Empty;

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Manage Scheme",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel() { SlNo = 1, Text = "Manage Scheme" },
            }
        };

        List<FilterModel> ColumnsForFilter = [];
        bool IsShowPopup { get; set; }
        bool IsShowViewHistoryPopup { get; set; }
        bool IsExpire { get; set; }
        string Reason = string.Empty;
        bool IsExtend { get; set; }
        string Title = string.Empty;
        DateTime OriginalEndDate = DateTime.Now;
        DateTime NewEndDate = DateTime.Now;

        //private string GetSchemeTypeName(string scheme)
        //{
        //    string name = string.Empty;
        //    switch (scheme)
        //    {
        //        case "sellin":
        //            name = "Sell In";
        //            break;
        //        case "sellout":
        //            name = "Sell Out";
        //            break;
        //        case "sellin":
        //            name = "Sell In";
        //            break;
        //        case "sellin":
        //            name = "Sell In";
        //            break;
        //        case "sellin":
        //            name = "Sell In";
        //            break;
        //        case "sellin":
        //            name = "Sell In";
        //            break;


        //    }

        //    return name;
        //}
        protected override void OnInitialized()
        {
            _loadingService.ShowLoading();
            productColumns.Add(new()
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new()
                {
                    new()
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = s => ViewOrEdit((IManageScheme)s)
                    },
                    new()
                    {
                        ButtonType = ButtonTypes.Text,
                        URL = "Images/edit.png",
                        Text = "Expire", IsVisible = false,
                        ConditionalVisibility = s => IsPermitted(((IManageScheme)s)),
                        Action = s => ExtendorExpire(manageScheme: (IManageScheme)s, isExtend: false)
                    },
                    new()
                    {
                        ButtonType = ButtonTypes.Text,
                        URL = "Images/edit.png",
                        Text = "Extend",
                        IsVisible = false,
                        ConditionalVisibility = s => IsPermitted(((IManageScheme)s)),
                        Action = s => ExtendorExpire(manageScheme: (IManageScheme)s, isExtend: true)
                    },
                    new()
                    {
                        ButtonType = ButtonTypes.Text,
                        URL = "Images/edit.png",
                        Text = "View History", IsVisible = false,
                        ConditionalVisibility = s => ((IManageScheme)s).HasHistory,
                        Action = s => ViewHistory((IManageScheme)s)
                    },
                }
            });
            base.OnInitialized();
            _loadingService.HideLoading();
        }

        List<string> Schemes = ["QPS", "SellOutRealSecondary"];

        bool IsPermitted(IManageScheme manageScheme)
        {
            List<string> strings = _appSetting.EndDateUpdatePermittedRoles.Split(",")?.ToList() ?? [];
            bool isUserAllowed = false,
                isSchemetypeAllowed = false,
                isFututureEndDate = manageScheme.ValidUpto > DateTime.Now,
                isApproved = SchemeConstants.Approved.Equals(manageScheme.Status, StringComparison.OrdinalIgnoreCase);
            if (isApproved && strings.Count > 0)
            {
                foreach (var item in strings)
                {
                    if (_appUser.Role.Code.Equals(item, StringComparison.OrdinalIgnoreCase))
                    {
                        isUserAllowed = true;
                    }
                }
            }

            if (isUserAllowed)
            {
                foreach (var item in Schemes)
                {
                    if (manageScheme.SchemeType != null &&
                        manageScheme.SchemeType.Equals(item, StringComparison.OrdinalIgnoreCase))
                    {
                        isSchemetypeAllowed = true;
                    }
                }
            }

            return isUserAllowed && isApproved && isSchemetypeAllowed && isFututureEndDate;
        }

        bool IsLoad = false;

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            await base.OnInitializedAsync();
            //if (permission == null)
            //    return;
            await _viewModel.PopulateViewModel();
            await SetSchemeTypesDropDown();
            SetFilters();
            await StateChageHandler();
            IsLoad = true;
            _loadingService.HideLoading();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("ManageScheme", ref ColumnsForFilter, out PageState pageState);

            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("ManageScheme");
        }
        protected async Task SetSchemeTypesDropDown()
        {
            var webViewModel = _viewModel as ManageSchemeWebViewModel;
            if (webViewModel != null)
            {
                var lists = await webViewModel.GetListItemsByCodes();
                if (lists != null && lists.Count() > 0)
                {
                    SchemesRoutes.RemoveAll(p => !lists.Any(q => q.Code.Equals(p.UID, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    ColumnName = nameof(IManageScheme.Code),
                    Label = "Code"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = SchemesRoutes,
                    SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IManageScheme.SchemeType),
                    Label = "Scheme Type"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = new List<ISelectionItem>()
                    {
                        new SelectionItem
                        {
                            UID = SchemeConstants.Approved, Code = SchemeConstants.Approved,
                            Label = SchemeConstants.Approved
                        },
                        new SelectionItem
                        {
                            UID = SchemeConstants.Pending, Code = SchemeConstants.Pending,
                            Label = SchemeConstants.Pending
                        },
                        new SelectionItem
                        {
                            UID = SchemeConstants.Expired, Code = SchemeConstants.Expired,
                            Label = SchemeConstants.Expired
                        },
                        new SelectionItem
                        {
                            UID = SchemeConstants.Rejected, Code = SchemeConstants.Rejected,
                            Label = SchemeConstants.Rejected
                        },
                    },
                    SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IManageScheme.Status),
                    Label = "Status"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                    ColumnName = nameof(IManageScheme.ValidFrom),
                    Label = "From Date"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                    ColumnName = nameof(IManageScheme.ValidUpto),
                    Label = "To Date"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox,
                    ColumnName = SchemeConstants.ShowInactive,
                    Label = "Show Inactive"
                },
            };
        }

        protected void ViewOrEdit(IManageScheme manageScheme)
        {
            if (manageScheme.SchemeType.ToLower() == "qps")
            {
                _navigationManager.NavigateTo($"QPSScheme/true?UID={manageScheme.UID}&{PageType.Page}={PageType.Edit}");
            }
            else if (manageScheme.SchemeType == "SellOutRealSecondary")
            {
                _navigationManager.NavigateTo(
                    $"SelloutrealSecondaryScheme?UID={manageScheme.UID}&{PageType.Page}={PageType.Edit}");
            }
            else if (manageScheme.SchemeType.ToLower() == "sell in")
            {
                _navigationManager.NavigateTo(
                    $"SellInSchemeBranchView?UID={manageScheme.UID}&{PageType.Page}={PageType.Edit}");
            }
            else if (manageScheme.SchemeType.ToLower() == "sell out")
            {
                _navigationManager.NavigateTo(
                    $"SelloutLiquidation?UID={manageScheme.UID}&{PageType.Page}={PageType.Edit}");
            }
            else if (manageScheme.SchemeType.ToLower() == "sales promotion")
            {
                _navigationManager.NavigateTo($"salespromotion?UID={manageScheme.UID}&{PageType.Page}={PageType.Edit}");
            }
        }

        private void OnSchemeSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    SelectedSchemePath = dropDownEvent!.SelectionItems!.FirstOrDefault()!.Code ?? string.Empty;
                }
                else
                {
                    SelectedSchemePath = string.Empty;
                }
            }
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            _pageStateHandler._currentFilters = filterCriteria;

            await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
        }

        private async Task OnPageChange(int pageNumber)
        {
            _loadingService.ShowLoading();
            await _viewModel.OnPageChange(pageNumber: pageNumber);
            _loadingService.HideLoading();
        }

        async Task HandleOkClick()
        {
            if (string.IsNullOrEmpty(_manageScheme.EndDateRemarks))
            {
                _tost.Add("Alert", "Please enter reason", Winit.UIComponents.SnackBar.Enum.Severity.Normal);
                return;
            }

            ShowLoader();
            string actionType = string.Empty;

            if (IsExpire)
            {
                _manageScheme.Status = SchemeConstants.Expired;
                _manageScheme.ValidUpto = DateTime.Now;
                IsExpire = false;
                actionType = SchemeConstants.Expire;
            }
            else
            {
                IsExtend = false;
                actionType = SchemeConstants.Extend;
                _manageScheme.ValidUpto = NewEndDate;
            }

            var promotion = new PromotionView()
            {
                UID = _manageScheme.UID,
                HasHistory = true,
                ValidUpto = _manageScheme.ValidUpto,
                EndDateRemarks = _manageScheme.EndDateRemarks,
                EndDateUpdatedOn = DateTime.Now,
                EndDateUpdatedByEmpUID = _appUser.Emp.UID,
                ModifiedBy = _appUser.Emp.UID,
                ModifiedTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                Status = _manageScheme.Status,
            };
            promotion.SchemeExtendHistory = new SchemeExtendHistory()
            {
                SchemeType = _manageScheme.SchemeType,
                ActionType = actionType,
                SchemeUid = promotion.UID,
                OldDate = OriginalEndDate,
                NewDate = NewEndDate,
                Comments = promotion.EndDateRemarks,
                UpdatedByEmpUid = _appUser.Emp.UID,
                UpdatedOn = DateTime.Now,

                UID = Guid.NewGuid().ToString(),
            };
            var resp = await ((ManageSchemeWebViewModel)_viewModel).ChangeEndDate(promotion);
            if (resp != null)
            {
                if (resp.IsSuccess)
                {
                    IsShowPopup = false;

                    _viewModel.ManageSchemes.ForEach(p =>
                    {
                        if (p.UID == _manageScheme.UID)
                        {
                            p.HasHistory = true;
                            if (IsExpire)
                            {
                                p.Status = _manageScheme.Status;
                                p.ValidUpto = _manageScheme.ValidUpto;
                            }
                            else
                            {
                                _manageScheme.ValidUpto = NewEndDate;
                            }
                        }
                    });
                    _tost.Add("Success", "Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    _manageScheme.ValidUpto = OriginalEndDate;
                    _tost.Add("Error", resp.ErrorMessage, Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }

            StateHasChanged();
            HideLoader();
        }

        IManageScheme _manageScheme { get; set; }

        void ExtendorExpire(IManageScheme manageScheme, bool isExtend)
        {
            _manageScheme = manageScheme;
            _manageScheme.EndDateRemarks = string.Empty;
            NewEndDate = OriginalEndDate = CommonFunctions.GetDate(manageScheme.ValidUpto.ToString());
            if (isExtend)
            {
                IsExtend = true;
                IsExpire = false;

                Title = "Extend Scheme End Date";
            }
            else
            {
                IsExtend = false;
                IsExpire = true;
                Title = "Expire Scheme";
            }

            IsShowPopup = true;
            StateHasChanged();
        }

        async Task ViewHistory(IManageScheme manageScheme)
        {
            ShowLoader();
            await _viewModel.GetschemeExtendHistoryBySchemeUID(manageScheme.UID);
            IsShowViewHistoryPopup = true;
            HideLoader();
            StateHasChanged();
        }
    }
}