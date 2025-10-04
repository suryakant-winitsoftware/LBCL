using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.Clear_Data
{
    public partial class ClearData
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsAddPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        private bool IsSalesRepPopup { get; set; }
        private bool IsActionPopup { get; set; }
        private bool IsInitialized { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public string OrgUID { get; set; }
        private List<ISelectionItem> ActionSelectionItems = new List<ISelectionItem>
    {
        new Winit.Shared.Models.Common.SelectionItem{UID="NO_ACTION",Code="NO_ACTION",Label="NO_ACTION"},
        new Winit.Shared.Models.Common.SelectionItem{UID="Clear Data after Upload",Code="Clear Data after Upload",Label="Clear Data after Upload"},
    };
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Clear Data Permission",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Clear Data Permission"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
              _mobileAppActionViewModel.PageSize = 10;
            FilterInitialized();
            OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
            await _mobileAppActionViewModel.GetSalesman(OrgUID);
            await GenerateGridColumns();
            await _mobileAppActionViewModel.PopulateViewModel();
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
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["user"], DropDownValues=_mobileAppActionViewModel.EmpSelectionList,ColumnName="LoginId",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["action"],DropDownValues=ActionSelectionItems, ColumnName="Action",SelectionMode=SelectionMode.Multiple},
        };
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "LoginId")
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> existingLabels = _mobileAppActionViewModel.EmpSelectionList
                                .Where(e => selectedUids.Contains(e.UID))
                                .Select(e => e.Label)
                                .ToList();

                            if (existingLabels.Any())
                            {
                                filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", existingLabels, FilterType.In));
                            }
                        }
                        else
                        {
                            ISelectionItem? selectionItem = _mobileAppActionViewModel.EmpSelectionList
                                .Find(e => e.UID == keyValue.Value);

                            if (selectionItem != null)
                            {
                                filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                            }
                        }
                    }
                    //else if (keyValue.Key == "LoginId")
                    //{
                    //    ISelectionItem? selectionItem = _mobileAppActionViewModel.EmpSelectionList.Find
                    //        (e => e.UID == keyValue.Value);
                    //    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                    //}
                    else if(keyValue.Key== "Action")
                    {
                        if(keyValue.Value.Contains(","))
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
            _mobileAppActionViewModel.PageNumber = 1;
            await _mobileAppActionViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }

        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["clear_data_permission"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["clear_data_permission"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["login_id"], GetValue = s => ((IMobileAppAction)s)?.LoginId ?? "N/A",IsSortable = true, SortField = "LoginId"  },
                new DataGridColumn { Header = @Localizer["action"], GetValue = s => ((IMobileAppAction)s)?.Action?? "N/A",IsSortable = true, SortField = "Action"  },
                new DataGridColumn { Header = @Localizer["action_date"],GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IMobileAppAction)s)?.ActionDate),IsSortable = true, SortField = "ActionDate" },
                new DataGridColumn { Header = @Localizer["result"], GetValue = s => string.IsNullOrWhiteSpace(((IMobileAppAction)s)?.Result) ? "N/A" : ((IMobileAppAction)s)?.Result,IsSortable = true, SortField = "Result"  },
            };
        }
        public void OnsalesRepSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                //SalesRepDD.UID = selecetedValue?.UID;
                _mobileAppActionViewModel.MobileAppAction.EmpUID= selecetedValue?.UID;
               
            }
            IsSalesRepPopup = false;
        }
        public void OnActionSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                _mobileAppActionViewModel.MobileAppAction.Action = selecetedValue?.Label;
            }
            IsActionPopup = false;
        }      
        protected async Task AddClearDataPermission()
        {
            //_skuViewModel.SKUCONFIG = _serviceProvider.CreateInstance<ISKUConfig>();
            _mobileAppActionViewModel.MobileAppAction = _serviceProvider.CreateInstance<IMobileAppAction>();
           await _mobileAppActionViewModel.PopulateViewModelForDD();
            IsAddPopUp = true;
        }
        private async Task OnCancelFromBackBTnPopUpClick()
        {
            _mobileAppActionViewModel.OnCancelFromPopUp();
            IsAddPopUp = false;
        }
        private async Task SaveMobileActionItem()
        {
           await _mobileAppActionViewModel.SaveClearData();
            await _mobileAppActionViewModel.PopulateViewModel();       
            await GenerateGridColumns();
            await Task.Delay(1000);
            IsAddPopUp = false;
            StateHasChanged();
            _tost.Add(@Localizer["mobileitem"], "Mobileitem details saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _mobileAppActionViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}