using System.Globalization;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.ManageInvoice
{
    public partial class ManageInvoice : BaseComponentBase
    {
        private List<DataGridColumn> GridColumns = [];
        public Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
        {
            BreadcrumList =
            [
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Manage Invoices", IsClickable = false },
        ],
            HeaderText = "Manage Invoices"
        };

        private readonly List<FilterModel> FilterColumns = [];

        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            GridColumns =
            [
                new DataGridColumn { Header = "Channel Partner Code & Name", IsSortable = true, GetValue = s => $"[{((IInvoiceHeaderView)s).OrgCode}]{((IInvoiceHeaderView)s).OrgName} ", SortField="orgname" },
            new DataGridColumn { Header = "Oracle PO No", IsSortable = true,GetValue = s => ((IInvoiceHeaderView)s).OraclePONumber  ?? "N/A", SortField="oracleponumber"    },
            new DataGridColumn { Header = "Delivery No", IsSortable = true,GetValue = s => ((IInvoiceHeaderView)s).InvoiceNo ?? "N/A"  , SortField="gstinvoiceno"    },
            new DataGridColumn { Header = "AR Number", IsSortable = true,GetValue = s => ((IInvoiceHeaderView)s).ARNumber ?? "N/A"  , SortField="arnumber"    },
            new DataGridColumn { Header = "GST Invoice No", IsSortable = true,GetValue = s => ((IInvoiceHeaderView)s).GSTInvoiceNo ?? "N/A" , SortField="invoiceno"    },
            new DataGridColumn { Header = "GL Date", IsSortable = true,GetValue = s => CommonFunctions.GetDateTimeInFormat(((IInvoiceHeaderView)s).InvoiceDate)  ?? "N/A", SortField="invoicedate"    },
            new DataGridColumn
            {
                IsButtonColumn = true,
                Header = "Action", IsSortable = false,
                ButtonActions =
                [   new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/view.png",
                        Action = e => OnViewClick((IInvoiceHeaderView) e)
                    },new ButtonAction
                    {
                        ButtonType = ButtonTypes.Text,
                        Text = "Download",
                        Action = e => OnDownloadClick((IInvoiceHeaderView) e),
                        ConditionalVisibility = e => !string.IsNullOrEmpty(((IInvoiceHeaderView) e).InvoiceURL)
                    }
                ],
            }
             ];
            _viewModel.PageNumber = 1;
            _viewModel.PageSize = 50;
            await _viewModel.PopulateViewModel();
            _ = _viewModel.LoadChannelPartner();
            PrepareFilterData();
            HideLoader();
        }

        private void PrepareFilterData()
        {
            FilterColumns.AddRange(
                new List<FilterModel>
                {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = "Channel Partner",
                    ColumnName = "orguid", DropDownValues= _viewModel.ChannelPartnerSelectionItem},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Oracle PO No",
                    ColumnName = "oracleponumber"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Delivery No",
                    ColumnName = "invoiceno"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "GST Invoice No",
                    ColumnName = "gstinvoiceno"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "Start Date",
                    ColumnName = "startdate"},
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "End Date",
                    ColumnName = "enddate"},
                });
        }

        private async Task OnSortClick(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _viewModel.OnSortClick(sortCriteria);
            HideLoader();
        }
        private async Task OnPageIndexChange(int index)
        {
            ShowLoader();
            await _viewModel.PageIndexChanged(index);
            HideLoader();
        }

        private void OnViewClick(IInvoiceHeaderView invoiceHeaderView)
        {
            _navigationManager.NavigateTo($"viewinvoice/{invoiceHeaderView.UID}");
        }
        private void OnDownloadClick(IInvoiceHeaderView invoiceHeaderView)
        {
            if (string.IsNullOrEmpty(invoiceHeaderView.InvoiceURL))
            {
                ShowAlert("No Invoice Url Found..");
                return;
            }
            _navigationManager.NavigateTo(Path.Combine(_appConfigs.ApiDataBaseUrl, invoiceHeaderView.InvoiceURL!));
        }

        private async Task OnFilterClick(Dictionary<string, string> filtersDict)
        {
            var filters = filtersDict.Where(_ => !string.IsNullOrEmpty(_.Value)).Select(e =>
            {
                if (e.Key == "startdate")
                {
                    DateTime parsedDate = DateTime.ParseExact(e.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    return new FilterCriteria("invoicedate", parsedDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual);
                }
                else if (e.Key == "enddate")
                {
                    DateTime parsedDate = DateTime.ParseExact(e.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    return new FilterCriteria("invoicedate", parsedDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual);
                }
                return new FilterCriteria(e.Key, e.Value, FilterType.Like);
            }).ToList();

            await _viewModel.ApplyFilter(filters);
        }
    }
}
