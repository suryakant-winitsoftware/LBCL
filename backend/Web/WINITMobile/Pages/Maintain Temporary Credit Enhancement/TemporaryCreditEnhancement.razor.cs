using Winit.Modules.CreditLimit.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WINITMobile.Pages.Base;
using SelectionMode = Winit.Shared.Models.Enums.SelectionMode;

namespace WINITMobile.Pages.Maintain_Temporary_Credit_Enhancement
{
    public partial class TemporaryCreditEnhancement : BaseComponentBase
    {
        public bool IsInitialised { get; set; }
        public List<FilterModel> FilterColumns = new List<FilterModel>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Temporary Credit Enhancement",
            BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel()
            {
                SlNo = 1, Text = "Temporary Credit Enhancement"
            },
        }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            _viewModel.PageNo = 1;
            _viewModel.PageSize = 50;
            await _viewModel.PopulateViewModel();
            await GenerateGridColumns();
            PopulateFilters();
            IsInitialised = true;
            HideLoader();
        }

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header = "Channel Partner",
                GetValue = item =>
                {
                    var storeUID = ((ITemporaryCredit)item).StoreUID;
                    var matchingChannelPartner = _viewModel.ChannelPartnerList
                        .FirstOrDefault(cp => cp.UID == storeUID);
                    return matchingChannelPartner?.Label ?? "N/A";
                },
                IsSortable = true,
                SortField = "StoreUID"
            },
            new DataGridColumn
            {
                Header = "Order Number",
                GetValue = item => ((ITemporaryCredit)item).OrderNumber ?? "N/A",
                IsSortable = true,
                SortField = "OrderNumber"
            },
            new DataGridColumn
            {
                Header = "Request Type",
                GetValue = item => ((ITemporaryCredit)item).RequestType ?? "N/A",
                IsSortable = true,
                SortField = "RequestType"
            },
            new DataGridColumn
            {
                Header = "Approved Date",
                GetValue = item => ((ITemporaryCredit)item).EffectiveFrom?.ToString("dd/MM/yyyy") ?? "N/A",
                IsSortable = true,
                SortField = "ApprovedOn"
            },
            new DataGridColumn
            {
                Header = "Request Date",
                GetValue = item => ((ITemporaryCredit)item).RequestDate.ToString("dd/MM/yyyy") ?? "N/A",
                IsSortable = true,
                SortField = "RequestDate"
            },
            new DataGridColumn
            {
                Header = "Amount / Days",
                GetValue = item =>
                {
                    var creditItem = (ITemporaryCredit)item;
                    return creditItem.RequestType == "Aging Days"
                        ? $"{(creditItem.RequestAmountDays)} Days"// Remove decimals and add "Days"
                        : creditItem.RequestType == "Credit Limit"
                            ? $"{creditItem.RequestAmountDays} Rs"// Restrict to 2 decimals and add "Rs"
                            : "N/A";
                },
                IsSortable = true,
                SortField = "RequestAmountDays"
            },
            new DataGridColumn
            {
                Header = "Status",
                GetValue = item => ((ITemporaryCredit)item).Status ?? "N/A",
                IsSortable = true,
                SortField = "Status"
            },
            new DataGridColumn
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/view.png",
                        Action = item => OnViewClick((ITemporaryCredit)item)
                    },
                }
            },
        };
        }

        private void OnViewClick(ITemporaryCredit item)
        {
            _navigationManager.NavigateTo($"NewTemporaryCreditEnhancement?CreditEnhancementUID={item.UID}");
        }

        private async Task AddNewTemporaryCreditEnhancementDetails()
        {
            _navigationManager.NavigateTo($"NewTemporaryCreditEnhancement");
        }

        private void PopulateFilters()
        {
            FilterColumns.AddRange(
            new List<FilterModel>
            {
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                SelectionMode = SelectionMode.Multiple,
                Label = "Channel Partner",
                ColumnName = "StoreUID",
                DropDownValues = _viewModel.ChannelPartnerList
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                Label = "Order No",
                ColumnName = "OrderNumber"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                SelectionMode = SelectionMode.Single,
                Label = "Request Type",
                ColumnName = "RequestType",
                DropDownValues = _viewModel.TemporaryCreditEnhancementRequestselectionItems
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "Approved Date",
                ColumnName = "ApprovedOn"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "End Date",
                ColumnName = "EffectiveUpto"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                Label = "Status",
                ColumnName = "Status",
                DropDownValues = _viewModel.StatusSelectionItems
            },
            });
        }
        private async Task OnFilterClick(Dictionary<string, string> filtersDict)
        {
            var filters = filtersDict.Where(_ => !string.IsNullOrEmpty(_.Value)).Select(e =>
            {
                if (e.Key == "StoreUID") return new FilterCriteria("StoreUID", e.Value.Split(","), FilterType.In);
                else if (e.Key == "RequestType") return new FilterCriteria("RequestType", e.Value.Split(","), FilterType.In);
                else if (e.Key == "Status") return new(e.Key, e.Value.Split(","), FilterType.In);
                // else if (e.Key == "orguid") return new(e.Key, e.Value.Split(","), FilterType.In);
                // else if (e.Key == "OracleOrderStatus") return new FilterCriteria(e.Key, e.Value.Split(","), FilterType.In);
                // {
                //
                // }
                return new FilterCriteria(e.Key, e.Value, FilterType.Equal);
            }).ToList();

            await _viewModel.ApplyFilter(filters, _viewModel.SortCriterias);
            StateHasChanged();
        }

        private async Task OnSortClick(SortCriteria criteria)
        {
            await _viewModel.ApplyFilter(_viewModel.FilterCriterias, [criteria]);
            StateHasChanged();
        }
    }
}
