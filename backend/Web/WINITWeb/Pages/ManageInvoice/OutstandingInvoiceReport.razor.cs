using MathNet.Numerics;
using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Constants;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.ManageInvoice
{
    public partial class OutstandingInvoiceReport : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        List<FilterModel> ColumnsForFilter = [];
        public List<DataGridColumn> DataGridColumns { get; set; } = new List<DataGridColumn>();

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Outstanding Invoice Report",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Outstanding Invoice Report"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            IsInitialised = true;
            await invoiceViewModel.GetOutstandingInvoiceReportData();
            await masterviewmodel.PopulateChannelPartners();
            GenerateGridColumns();
            GenerateFilterColumns();
        }

        private void GenerateFilterColumns()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     DropDownValues=masterviewmodel.ChannelPartnerList,
                     SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Multiple,
                     ColumnName = nameof(IStandingProvisionScheme.SKUTypeCode),
                     IsCodeOnDDLSelect = true,
                     Label = "Channel Partenr"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(IOutstandingInvoiceReport.Ccode),
                     Label = "Customer Code"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(IOutstandingInvoiceReport.Cname),
                     Label = "Customer Name"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                     ColumnName =nameof(IOutstandingInvoiceReport.ARINVNO),
                     Label = "Invoice No"
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = nameof(IOutstandingInvoiceReport.ARDate),
                     Label = "From Date"
                 },
                //new FilterModel
                // {
                //     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                //     ColumnName = nameof(IOutstandingInvoiceReport.EndDate),
                //     Label = "To Date"
                // },
             };
        }

       

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Customer Group", GetValue = item => ((IOutstandingInvoiceReport)item).Cgroup ?? "N/A"},
                new DataGridColumn { Header = "Customer Code", GetValue = item => ((IOutstandingInvoiceReport)item).Ccode ?? "N/A"},
                new DataGridColumn { Header = "Registry ID", GetValue = item => ((IOutstandingInvoiceReport)item).RegistryID ?? "N/A"},
                new DataGridColumn { Header = "Customer Name", GetValue = item => ((IOutstandingInvoiceReport)item).Cname ?? "N/A"},
                new DataGridColumn { Header = "Sales Order No", GetValue = item => ((IOutstandingInvoiceReport)item).SONo ?? "N/A"},
                new DataGridColumn { Header = "Commercial Invoice No", GetValue = item => ((IOutstandingInvoiceReport)item).CommercialInvoiceNo ?? "N/A"},
                new DataGridColumn { Header = "VAT Invoice Date", GetValue = item => ((IOutstandingInvoiceReport)item).VatInvoiceDate.ToString("dd-MM-yyyy") ?? "N/A"},
                new DataGridColumn { Header = "AR Invoice No", GetValue = item => ((IOutstandingInvoiceReport)item).ARINVNO ?? "N/A"},
                new DataGridColumn { Header = "AR Date", GetValue = item => ((IOutstandingInvoiceReport)item).ARDate.ToString("dd-MM-yyyy") ?? "N/A"},
                new DataGridColumn { Header = "Invoice Amount", GetValue = item => ((IOutstandingInvoiceReport)item).InvAmount.ToString("N2") ?? "N/A"},
                new DataGridColumn { Header = "Unpaid Amount", GetValue = item => ((IOutstandingInvoiceReport)item).UnpaidAmount.ToString("N2") ?? "N/A"},
                new DataGridColumn { Header = "Invoice Type", GetValue = item => ((IOutstandingInvoiceReport)item).InvoiceType ?? "N/A"},
                new DataGridColumn { Header = "Terms of Payment", GetValue = item => ((IOutstandingInvoiceReport)item).TOP ?? "N/A"},
                new DataGridColumn { Header = "Invoice Due Date", GetValue = item => ((IOutstandingInvoiceReport)item).InvoiceDueDate.ToString("dd-MM-yyyy") ?? "N/A"},
                new DataGridColumn { Header = "Employee Code", GetValue = item => ((IOutstandingInvoiceReport)item).EmpCode ?? "N/A"},
                new DataGridColumn { Header = "Employee Name", GetValue = item => ((IOutstandingInvoiceReport)item).EmpName ?? "N/A"},
                new DataGridColumn { Header = "Sales Dealer Code", GetValue = item => ((IOutstandingInvoiceReport)item).SalesDealerCode ?? "N/A"},
                new DataGridColumn { Header = "Sales Dealer Name", GetValue = item => ((IOutstandingInvoiceReport)item).SalesDealerName ?? "N/A"},
                new DataGridColumn { Header = "Service Dealer Code", GetValue = item => ((IOutstandingInvoiceReport)item).ServiceDealerCode ?? "N/A"},
                new DataGridColumn { Header = "Service Dealer Name", GetValue = item => ((IOutstandingInvoiceReport)item).ServiceDealerName ?? "N/A"},
                new DataGridColumn { Header = "SO Code", GetValue = item => ((IOutstandingInvoiceReport)item).SOCode ?? "N/A"},
                new DataGridColumn { Header = "Division Code", GetValue = item => ((IOutstandingInvoiceReport)item).DivisionCode ?? "N/A"},
                new DataGridColumn { Header = "Division", GetValue = item => ((IOutstandingInvoiceReport)item).Division ?? "N/A"},
                new DataGridColumn { Header = "Warehouse Code", GetValue = item => ((IOutstandingInvoiceReport)item).WarehouseCode ?? "N/A"},
                new DataGridColumn { Header = "Warehouse", GetValue = item => ((IOutstandingInvoiceReport)item).Warehouse ?? "N/A"},
                new DataGridColumn { Header = "GL Code", GetValue = item => ((IOutstandingInvoiceReport)item).GLCode ?? "N/A"},
                new DataGridColumn { Header = "GL Description", GetValue = item => ((IOutstandingInvoiceReport)item).GLDesc ?? "N/A"},
                new DataGridColumn { Header = "Customer PO No", GetValue = item => ((IOutstandingInvoiceReport)item).CustomerPONo ?? "N/A"},
                new DataGridColumn { Header = "Invoice Age", GetValue = item => ((IOutstandingInvoiceReport)item).InvoiceAge.ToString() ?? "N/A"},
                new DataGridColumn { Header = "Invoice Age Bucket", GetValue = item => ((IOutstandingInvoiceReport)item).InvoiceAgeBucket ?? "N/A"},
                new DataGridColumn { Header = "Credit Age", GetValue = item => ((IOutstandingInvoiceReport)item).CreditAge.ToString() ?? "N/A"},
                new DataGridColumn { Header = "Credit Age Bucket", GetValue = item => ((IOutstandingInvoiceReport)item).CreditAgeBucket ?? "N/A"},
                new DataGridColumn { Header = "Bill To Site Location", GetValue = item => ((IOutstandingInvoiceReport)item).BillToSiteLocation ?? "N/A"},
                new DataGridColumn { Header = "Ship To Address", GetValue = item => ((IOutstandingInvoiceReport)item).ShipToAddress ?? "N/A"},
                new DataGridColumn { Header = "Currency", GetValue = item => ((IOutstandingInvoiceReport)item).Currency ?? "N/A"},
                new DataGridColumn { Header = "Operating Unit", GetValue = item => ((IOutstandingInvoiceReport)item).OU ?? "N/A"},
            };
        }

        private void OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            ShowLoader();

            HideLoader();
        }
    }
}
