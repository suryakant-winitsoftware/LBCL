using Winit.Modules.ServiceAndCallRegistration.BL.Classes;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.ServiceAndCallRegistration
{
    public partial class MaintainCallRegistration : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsItemSelectedToShow { get; set; } = false;
        public bool CheckStatus { get; set; } = false;
        List<FilterModel> ColumnsForFilter = [];
        public List<DataGridColumn> DataGridColumns { get; set; } = new List<DataGridColumn>();
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Call Registration",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Call Registration"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            _serviceAndCallRegistrationViewModel.PageSize = 100;
            await _serviceAndCallRegistrationViewModel.PopulateCallRegistrations();
            GenerateGridColumns();
            SetFilters();
            IsInitialised = true;
        }
        private async Task AddCallRegistration()
        {
            _navigationManager.NavigateTo($"CallRegistration");
        }
        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Date", GetValue = item => CommonFunctions.GetDateTimeInFormat(((ICallRegistration)item).PurchaseDate) ?? "N/A" ,IsSortable=true ,SortField="PurchaseDate"},
                new DataGridColumn { Header = "Service Call No", GetValue = item => ((ICallRegistration)item).ServiceCallNo ?? "N/A",IsSortable=true,SortField="ServiceCallNo"},
                new DataGridColumn { Header = "Customer Name", GetValue = item => ((ICallRegistration)item).CustomerName ?? "N/A",IsSortable=true,SortField="CustomerName"},
                new DataGridColumn { Header = "Contact Person", GetValue = item => ((ICallRegistration)item).ContactPerson ?? "N/A",IsSortable=true,SortField="ContactPerson"},
                new DataGridColumn { Header = "Mobile Number", GetValue = item => ((ICallRegistration)item).MobileNumber ?? "N/A",IsSortable=true,SortField="MobileNumber"},
                new DataGridColumn { Header = "Actions", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction{ ButtonType = ButtonTypes.Image ,URL = "Images/view.png", Action = item => OnViewClick((ICallRegistration)item) },
                        new ButtonAction{ ButtonType = ButtonTypes.Text , Text ="Check Status", Action = item => CheckCallStatus((ICallRegistration)item) },
                    }},
            };
        }
        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Service Call Number",
                    ColumnName = "ServiceCallNo"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Customer Name",
                    ColumnName = "CustomerName"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Mobile Number",
                    ColumnName = "MobileNumber"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                    Label = "Start Date",
                    ColumnName = "PurchaseOrderDateStart"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                    Label = "End Date",
                    ColumnName = "PurchaseOrderDateEnd"
                }
             };
        }
        private async Task OnViewClick(ICallRegistration item)
        {
            ShowLoader();
            string callid = item.ServiceCallNo;
            _navigationManager.NavigateTo($"ViewCallRegistration?ServiceCallNumber={callid}");
            HideLoader();
        }
        private async Task CheckCallStatus(ICallRegistration item)
        {
            ShowLoader();
            _serviceAndCallRegistrationViewModel.ServiceStatus.CallId = item.ServiceCallNo;
            _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce = await ((ServiceAndCallRegistrationWebViewModel)_serviceAndCallRegistrationViewModel).GetServiceStatusBasedOnNumber(_serviceAndCallRegistrationViewModel.ServiceStatus);
            if (_serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors.Count > 0)
            {
                string errorMessages = string.Join(", ", _serviceAndCallRegistrationViewModel.serviceRequestStatusResponce.Errors);
                ShowAlert("Error", errorMessages);
            }
            else
            {
                CheckStatus = true;
            }
            HideLoader();
            StateHasChanged();
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _loadingService.ShowLoading();
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "ServiceCallNo")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    else if (keyValue.Key == "CustomerName")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    else if (keyValue.Key == "MobileNumber")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    else if (keyValue.Key == "PurchaseOrderDateStart")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{"PurchaseDate"}", keyValue.Value, FilterType.GreaterThanOrEqual));
                    }
                    else if (keyValue.Key == "PurchaseOrderDateEnd")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{"PurchaseDate"}", keyValue.Value, FilterType.LessThanOrEqual));
                    }
                }
            }
            await _serviceAndCallRegistrationViewModel.ApplyFilterForDealer(filterCriterias);
            StateHasChanged();
            _loadingService.HideLoading();
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _serviceAndCallRegistrationViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            ShowLoader();
            await _serviceAndCallRegistrationViewModel.PageIndexChanged(pageNumber);
            HideLoader();
        }

    }
}
