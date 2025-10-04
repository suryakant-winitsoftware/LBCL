using DocumentFormat.OpenXml.Wordprocessing;
using MathNet.Numerics;
using NPOI.POIFS.NIO;
using Winit.Modules.CollectionModule.BL.Classes.OutStandingInvoice;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Invoices
{
    public partial class OutStandingInvoices : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsView { get; set; } = false;
        List<FilterModel> ColumnsForFilter = [];
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> CMIItemDataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Outstanding Invoices",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Outstanding Invoices"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await OutStanding_Invoices.PopulateAccPayableCMI();
            GenerateGrid();
            GenerateGetAwayPeriodDetailsGrid();
            await GenerateFilter();
            IsInitialised = true;
            HideLoader();
        }
        async Task GenerateFilter()
        {
            ColumnsForFilter.Clear();
            ColumnsForFilter.Add(new FilterModel
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                ColumnName = "Code/Name",
                Label = "Customer Code/Name"
            });

            ColumnsForFilter.Add(new FilterModel
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                DropDownValues = await ((OutStandingInvoicesWebViewModel)OutStanding_Invoices).GetOU(),
                SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
                ColumnName = nameof(IAccPayableCMI.OU),
                Label = "OU"
            });
        }

        private void GenerateGrid()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Customer Name",
                    GetValue = item => string.IsNullOrEmpty(((IAccPayableCMI)item).CustomerCode)?"N/A":
                    $"[{ ((IAccPayableCMI)item).CustomerCode}] { ((IAccPayableCMI)item).CustomerName}",
                    IsSortable=true,SortField=nameof(IAccPayableCMI.CustomerCode) },
                new DataGridColumn { Header = "OU", GetValue = item => ((IAccPayableCMI)item).OU ?? "N/A", IsSortable=true,SortField=nameof(IAccPayableCMI.OU)},
                new DataGridColumn { Header = "Balance",
                    GetValue = item => CommonFunctions.GetStringInNumberFormatExcludingZero(((IAccPayableCMI)item).TotalBalanceAmount) ?? "N/A",
                 IsSortable=true,SortField=nameof(IAccPayableCMI.TotalBalanceAmount)},
                new DataGridColumn { Header = "No. of Invoices",
                    GetValue = item => CommonFunctions.GetStringInNumberFormatExcludingZero(((IAccPayableCMI)item).CountOfInvoices) ?? "N/A",
                 IsSortable=true,SortField=nameof(IAccPayableCMI.CountOfInvoices)},

                new DataGridColumn { Header = "Action", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/view.png",
                            Action =async item =>await OnViewClick((IAccPayableCMI)item)
                        },
                    new ButtonAction
                        {
                            ButtonType = ButtonTypes.Text,
                            Text = "Export Excel",
                            Action =async item =>
                            {
                              Task.Run(async()=> ExportInvoices((IAccPayableCMI)item));
                            }
                        },
                    }},
            };
        }

        async Task ExportInvoices(IAccPayableCMI invoice)
        {
            var data = await ((OutStandingInvoicesWebViewModel)OutStanding_Invoices).
             GetOutstandingInvoiceDetailsByStoreCode(invoice.CustomerCode, 1, invoice.CountOfInvoices);
            if (data is not null)
            {
                data.ForEach(p =>
                {
                    p.CustomerCode = invoice.CustomerCode;
                    p.CustomerName = invoice.CustomerName;
                    p.OU = invoice.OU;
                });
            }
            var columnNames = new Dictionary<string, string>()
            {
                {nameof(OutstandingInvoiceView.CustomerCode),"Customer Code" },
                {nameof(OutstandingInvoiceView.CustomerName),"Customer Name" },
                {nameof(OutstandingInvoiceView.OU),"OU" },
                {nameof(OutstandingInvoiceView.SourceType),"Invoice Type" },
                {nameof(OutstandingInvoiceView.ReferenceNumber),"AR Number" },
                {nameof(OutstandingInvoiceView.TransactionDate),"GL Date" },
                {nameof(OutstandingInvoiceView.TaxInvoiceNumber),"GST Invoice No" },
                {nameof(OutstandingInvoiceView.TaxInvoiceDate),"GST Invoice Date" },
                {nameof(OutstandingInvoiceView.InvoiceDueDate),"Invoice Due Date" },
                {nameof(OutstandingInvoiceView.Amount),"Net Amount" },
                {nameof(OutstandingInvoiceView.BalanceAmount),"Balance Amount" },
            };
            _commonFunctions.ExportToExcelAsync<OutstandingInvoiceView>(data, columnNames, invoice.CustomerCode);
        }
        List<OutstandingInvoiceView> OutSTandingInvoicesDetails { get; set; } = [];
        List<OutstandingInvoiceView> FilteredOutSTandingInvoicesDetails
        {
            get
            {
                return string.IsNullOrEmpty(searchitem) ? OutSTandingInvoicesDetails :
                     OutSTandingInvoicesDetails.FindAll(item => item.ReferenceNumber.Contains(searchitem, StringComparison.OrdinalIgnoreCase) ||
                item.TaxInvoiceNumber.Contains(searchitem, StringComparison.OrdinalIgnoreCase)) ?? [];
            }
        }
        IAccPayableCMI SelectedInvoice { get; set; }
        string searchitem = string.Empty;
        private async Task OnViewClick(IAccPayableCMI item)
        {

            ShowLoader();
            popUpPageNumber = 1;

            SelectedInvoice = item;
            OutSTandingInvoicesDetails.Clear();
            //await OutStanding_Invoices.GetAccPayableCMIByUID(item.UID);
            OutSTandingInvoicesDetails.AddRange(await ((OutStandingInvoicesWebViewModel)OutStanding_Invoices).
                GetOutstandingInvoiceDetailsByStoreCode(item.CustomerCode, popUpPageNumber, popUpPageSize));

            IsView = true;
            StateHasChanged();
            HideLoader();
        }
        private async Task OnViewClick(IAccPayableCMI item, bool isExportExcel)
        {

            ShowLoader();
            popUpPageNumber = 1;

            SelectedInvoice = item;
            OutSTandingInvoicesDetails.Clear();
            //await OutStanding_Invoices.GetAccPayableCMIByUID(item.UID);
            OutSTandingInvoicesDetails.AddRange(await ((OutStandingInvoicesWebViewModel)OutStanding_Invoices).
                GetOutstandingInvoiceDetailsByStoreCode(item.CustomerCode, popUpPageNumber, popUpPageSize));

            IsView = true;
            StateHasChanged();
            HideLoader();
        }

        private void GenerateGetAwayPeriodDetailsGrid()
        {
            CMIItemDataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Invoice Type", GetValue = item => ((OutstandingInvoiceView)item).SourceType ?? "N/A"},
                new DataGridColumn { Header = "AR Number", GetValue = item => ((OutstandingInvoiceView)item).ReferenceNumber ?? "N/A"},
                new DataGridColumn { Header = "GL Date", GetValue = item => CommonFunctions.GetDateTimeInFormat(((OutstandingInvoiceView)item).TransactionDate) ?? "N/A"},
                new DataGridColumn { Header = "GST Invoice No", GetValue = item => ((OutstandingInvoiceView)item).TaxInvoiceNumber ?? "N/A"},
                new DataGridColumn { Header = "GST Invoice Date", GetValue = item => CommonFunctions.GetDateTimeInFormat(((OutstandingInvoiceView)item).TaxInvoiceDate) ?? "N/A"},
                new DataGridColumn { Header = "Invoice Due Date", GetValue = item =>CommonFunctions.GetDateTimeInFormat(((OutstandingInvoiceView)item).InvoiceDueDate)  ?? "N/A"},
                new DataGridColumn { Header = "Net Amount", GetValue = item => CommonFunctions.GetStringInNumberFormatExcludingZero(((OutstandingInvoiceView)item).Amount)?? "N/A"},
                new DataGridColumn { Header = "Balance Amount", GetValue = item => CommonFunctions.GetStringInNumberFormatExcludingZero(((OutstandingInvoiceView)item).BalanceAmount) ?? "N/A"},

            };
        }

        private async Task OnFilterApplied(Dictionary<string, string> filters)
        {
            ShowLoader();
            await OutStanding_Invoices.OnFilterApply(filters);
            HideLoader();
        }
        private async Task OnSort(SortCriteria sortCriteria)
        {
            ShowLoader();
            await OutStanding_Invoices.OnSort(sortCriteria);
            HideLoader();
        }
        private async Task OnPageChange(int pageNumber)
        {
            ShowLoader();
            await OutStanding_Invoices.OnPageChange(pageNumber);
            HideLoader();
        }
        private async Task OnPopUpPageChange(int pageNumber)
        {
            ShowLoader();
            popUpPageNumber = pageNumber;
            var data = await ((OutStandingInvoicesWebViewModel)OutStanding_Invoices).
                GetOutstandingInvoiceDetailsByStoreCode(SelectedInvoice.CustomerCode, popUpPageNumber, popUpPageSize);
            OutSTandingInvoicesDetails.Clear();
            OutSTandingInvoicesDetails.AddRange(data);
            StateHasChanged();
            HideLoader();
        }
        private void Search(string searchTerm)
        {
            searchitem = searchTerm;
            StateHasChanged();
        }
        int popUpPageNumber = 1;
        int popUpPageSize = 100;
    }
}
