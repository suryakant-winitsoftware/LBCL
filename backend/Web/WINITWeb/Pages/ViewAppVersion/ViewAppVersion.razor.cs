using Microsoft.AspNetCore.Components;
using Org.BouncyCastle.Asn1;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.ViewAppVersion
{
    public partial class ViewAppVersion
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsEditPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        public string UID { get; set; }
        public string DeviceId { get; set; }
        private bool IsInitialized { get; set; }
        public IAppVersionUser appVersion { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private List<ISelectionItem> DeviceTypeSelectionItems = new List<ISelectionItem>
    {
        new Winit.Shared.Models.Common.SelectionItem{UID="Mobile",Code="Mobile",Label="Mobile"},
    };
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View App Version",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View App Version"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _appVersionViewModel.PageSize = 10;
            _appVersionViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
           // _appVersionViewModel.OrgUID = "FR001";
            await _appVersionViewModel.GetSalesman(_appVersionViewModel.OrgUID);
            await GenerateGridColumns();
            _appVersionViewModel.ORGUID = _iAppUser.SelectedJobPosition.OrgUID;
            await _appVersionViewModel.PopulateViewModel();
            IsInitialized = true;
            //await SetHeaderName();
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["user_name"], DropDownValues=_appVersionViewModel.EmpSelectionList,ColumnName="EmpUID",SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["device_type"],DropDownValues=DeviceTypeSelectionItems, ColumnName = "DeviceType",SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["app_version"],ColumnName = "AppVersion"},
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
                    if (keyValue.Key == "EmpUID")
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedValueUID = _appVersionViewModel.EmpSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.UID).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedValueUID, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = _appVersionViewModel.EmpSelectionList.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.UID, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == "DeviceType")
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
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }

                }
            }
            _appVersionViewModel.PageNumber = 1;
            await _appVersionViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["device_management"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["device_management"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["user_name"], GetValue = s => ((IAppVersionUser)s)?.Name ?? "N/A",IsSortable = true, SortField = "Name" },
                new DataGridColumn { Header = @Localizer["devivce_type"], GetValue = s => ((IAppVersionUser)s)?.DeviceType?? "N/A" ,IsSortable = true, SortField = "DeviceType"},
                new DataGridColumn { Header = @Localizer["device_id"],   GetValue = s => string.IsNullOrEmpty(((IAppVersionUser)s)?.DeviceId) ? "N/A" : ((IAppVersionUser)s)?.DeviceId,IsSortable = true, SortField = "DeviceId" },
                new DataGridColumn { Header = @Localizer["app_version"], GetValue = s => ((IAppVersionUser)s)?.AppVersion?? "N/A",IsSortable = true, SortField = "AppVersion" },
             new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        Text =@Localizer["edit"],
                        Action = item => EditAppVersion((IAppVersionUser)item)

                    },
                }
            }
             };
        }
        protected async Task EditAppVersion(IAppVersionUser appVersionUser)
        {
            //_skuViewModel.SKUCONFIG = _serviceProvider.CreateInstance<ISKUConfig>();
            DeviceId = "";
            string UID = appVersionUser.UID;
            if (UID != null)
            {
                IsEditPopUp = true;
                await _appVersionViewModel.PopulateDeviceManagementforEditDetailsData(UID);
            }
            StateHasChanged();
        }
        private async Task OnOkFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = false;
        }
        private async Task OnCloseFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = true;
        }
        private async Task OnCancelFromBackBTnPopUpClick()
        {
            IsBackBtnPopUp = true;
            IsEditPopUp = false;
            StateHasChanged();
        }
       
        private async Task SaveAppVersionItem()
        {

            await _appVersionViewModel.UpdateDeviceManagement(DeviceId);
            await _appVersionViewModel.PopulateViewModel();
            await GenerateGridColumns();
            await Task.Delay(1000);
            IsEditPopUp = false;
            _tost.Add(@Localizer["app_version_user"], "App version user details updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);

        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _appVersionViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}
