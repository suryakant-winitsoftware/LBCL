using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
namespace WinIt.Pages.Provision_Comparison_Report
{
    public partial class ProvisionComparisonReport
    {
        public Winit.UIComponents.Web.ExportToExcel.ExportToExcel? ExportExcelRefrence;
        public List<DataGridColumn>? DataGridColumns { get; set; }
        public bool IsPageLoading = true;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View All Provision Comparison Report",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo = 1, Text = "View all provision comparison report", IsClickable = false}
            }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                await _viewModel.GetAllProvisionComparisonReport();
                GenerateGridColumns();
                await FilterInitialization();
                HideLoader();
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Error", ex.Message);
            }

            finally
            {
                IsPageLoading=false;
            }

        }

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn {Header = "Channel Partner", GetValue = s => ((IProvisionComparisonReportView)s)?.ChannelPartner?? "N/A"},
                new DataGridColumn {Header = "Broad Classification", GetValue = s => ((IProvisionComparisonReportView)s)?.BroadClassification?? "N/A"},
                new DataGridColumn {Header = "Ar No", GetValue = s => ((IProvisionComparisonReportView)s)?.ArNo?? "N/A"},
                new DataGridColumn {Header = "Gst Invoice Num", GetValue = s => ((IProvisionComparisonReportView)s)?.GstInvoiceNumber ?? "N/A"},
                new DataGridColumn {Header = "Gst Invoice Date", GetValue = s => ((IProvisionComparisonReportView)s)?.InvoiceDate},
                new DataGridColumn {Header = "Item", GetValue = s => ((IProvisionComparisonReportView)s)?.ItemCode ?? "N/A"},
                new DataGridColumn {Header = "Qty", GetValue = s => ((IProvisionComparisonReportView)s)?.Qty!},
                new DataGridColumn {Header = "Provision Type", GetValue = s => ((IProvisionComparisonReportView)s)?.ProvisionType!},
                new DataGridColumn {Header = "DMS Provision Amount", GetValue = s => ((IProvisionComparisonReportView)s)?.DmsProvisionAmount!},
                new DataGridColumn {Header = "Orac", GetValue = s => ((IProvisionComparisonReportView)s)?.OracleProvisionAmount!},
                new DataGridColumn {Header = "Diff", GetValue = s => ((IProvisionComparisonReportView)s)?.Diff!}
             };
        }

        #region FilterLogic

        public List<FilterModel>? ColumnsForFilter;
        public async Task FilterInitialization()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                  new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = "StartDate",
                     Label = "Start Date",
                     // IsDefaultValueNeeded=true,
                     //SelectedValue = DateTime.Now.ToString("dd/MM/yyyy"),
                     //DefaultValue=DateTime.Now.ToString("dd/MM/yyyy")
                 },
                 new FilterModel
                 {
                     FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                     ColumnName = "EndDate",
                     Label = "End Date",
                     //IsDefaultValueNeeded=true,
                     //SelectedValue = DateTime.Now.ToString("dd/MM/yyyy"),
                     //DefaultValue=DateTime.Now.ToString("dd/MM/yyyy")
                 },
                  new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetChannelPartnerDDLValues(),
                    SelectionMode=SelectionMode.Single,
                    ColumnName = "ChannelPartner",
                    Label = "Channel Partner"
                },
                   new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetBroadClassificationDDLValues(),
                    SelectionMode=SelectionMode.Single,
                    ColumnName = "BroadClassification",
                    Label = "Broad Classification"
                },
                   new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetBranchDDLValues(),
                    SelectionMode=SelectionMode.Single,
                    ColumnName = "Branch",
                    Label = "Branch"
                },
                   new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetSalesOfficeDDLValues(),
                    SelectionMode=SelectionMode.Single,
                    ColumnName = "SalesOffice",
                    Label = "Sales Office"
                },
                   new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetProvisionTypeDDLValues(),
                    SelectionMode=SelectionMode.Single,
                    ColumnName = "ProvisionType",
                    Label = "Provision Type"
                },
                 //  new FilterModel
                 //{
                 //    FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox,
                 //    ColumnName = "ShowDiffOnly",
                 //    Label = "Show Diff Only"
                 //},

             };

        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
        }
        #endregion
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _viewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }


        private async Task OnExcelDownloadClick()
        {
            try
            {
                await _viewModel.ExportProvisionComparisonReportBasedOnFilter();
                if (ExportExcelRefrence !=null)
                {
                    ExportExcelRefrence.DataSource=_viewModel.ProvisionsComparisonReportViewInExportExcel;
                    ExportExcelRefrence.FileName = "ProvisionComparisonReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                }
                else
                {
                    throw new Exception("Export Excel component refrence is null");
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
