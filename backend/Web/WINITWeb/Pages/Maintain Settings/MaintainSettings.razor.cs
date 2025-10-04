using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.Maintain_Settings
{
    public partial class MaintainSettings
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsAddPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        public bool IsEditPopUp { get; set; }
        public string UID { get; set; }
        public string Type { get; set; }
        private bool IsInitialized { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Settings",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Settings"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _settingViewModel.PageSize = 10;
            IsInitialized = true;
            await GenerateGridColumns();
            await _settingViewModel.PopulateViewModel();
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["name"] ,ColumnName = "Name"},
            };
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_settings"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["maintain_settings"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((ISetting)s)?.Name ?? "N/A",IsSortable = true, SortField = "Name" },
                new DataGridColumn { Header = @Localizer["value"], GetValue = s => string.IsNullOrEmpty(((ISetting)s)?.Value) ? "N/A" : ((ISetting)s)?.Value == "1" ? "Yes" :  ((ISetting)s)?.Value == "0" ?  "No" : ((ISetting)s)?.Value,IsSortable = true, SortField = "Value"},
                //new DataGridColumn { Header = "Value", GetValue = s => string.IsNullOrEmpty(((ISetting)s)?.Value) ? "N/A" : Winit.Shared.CommonUtilities.Common.CommonFunctions.GetBooleanValue(((ISetting)s).Value)? (((ISetting)s).Value) : ((ISetting)s)?.Value},
             new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,

                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Image,
                       URL="https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = item => OnEditClick((ISetting)item)

                    },
                }
            }
             };
        }
        public async Task OnEditClick(ISetting setting)
        {

            UID = setting.UID;
            if (UID != null)
            {
                IsEditPopUp = true;
                await _settingViewModel.PopulatetMaintainSettingforEditDetailsData(UID);
            }
            StateHasChanged();

        }
       
        private async Task UpdateSetting()
        {

            await _settingViewModel.UpdateMaintainSetting(UID);
            await _settingViewModel.PopulateViewModel();
            await GenerateGridColumns();
            await Task.Delay(500);
            IsEditPopUp = false;
            _tost.Add(@Localizer["setting"], "Setting details updated successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        private async Task OnCancelFromBackBTnPopUpClick()
        {
            //IsBackBtnPopUp = true;
            IsEditPopUp = false;
            StateHasChanged();
        }
        private async Task OnCloseFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = true;
        }
        private async Task OnOkFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsEditPopUp = false;
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _settingViewModel.PageNumber = 1;
            await _settingViewModel.OnFilterApply(ColumnsForFilter, keyValuePairs);
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _settingViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}
