using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.DashBoard.BL.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Reports
{
    public partial class BranchSalesReport
    {
        List<DataGridColumn> MainGridColumns = [];
        List<DataGridColumn> AsmViewGridColumns = [];
        List<DataGridColumn> OrgViewGridColumns = [];
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;

        bool IsAsmView { get; set; }
        bool IsPopupOpen { get; set; }
        IBranchSalesReport SelectedBranch { get; set; }
        protected override void OnInitialized()
        {
            SetMainGrid();
            SetHeaderName();
            AsmViewGridColumns.Add(new() { Header = "ASM", GetValue = s => ((IBranchSalesReportAsmview)s).ASMName });
            AsmViewGridColumns.Add(new() { Header = "Total Unit", GetValue = s => CommonFunctions.RoundForSystem(((IBranchSalesReportAsmview)s).TotalUnits) });
            AsmViewGridColumns.Add(new() { Header = "Total Sales", GetValue = s => CommonFunctions.RoundForSystem(((IBranchSalesReportAsmview)s).TotalSales) });

            OrgViewGridColumns.Add(new() { Header = "Distributor", GetValue = s => ((IBranchSalesReportOrgview)s).OrgName });
            OrgViewGridColumns.Add(new() { Header = "Total Unit", GetValue = s => CommonFunctions.RoundForSystem(((IBranchSalesReportOrgview)s).TotalUnits) });
            OrgViewGridColumns.Add(new() { Header = "Total Sales", GetValue = s => CommonFunctions.RoundForSystem(((IBranchSalesReportOrgview)s).TotalSales) });
        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _viewModel.PopulateViewModel();
            HideLoader();
            await base.OnInitializedAsync();
        }

        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Branch Sales Report",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Branch Sales Report", IsClickable = false },
                }
            };
        }


        protected void SetMainGrid()
        {
            MainGridColumns.Add(new DataGridColumn()
            {
                Header = "Branch",
                GetValue = s => ((Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReport)s).BranchName
            });
            MainGridColumns.Add(new DataGridColumn()
            {
                Header = "Total Units",
                GetValue = s => ((Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReport)s).TotalUnits
            });
            MainGridColumns.Add(new DataGridColumn()
            {
                Header = "Total Sales",
                GetValue = s => ((Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReport)s).TotalSales
            });

            MainGridColumns.Add(new DataGridColumn()
            {
                IsButtonColumn = true,
                Header = "Action",
                ButtonActions = new()
                {
                    new ButtonAction()
                    {
                        ButtonType=ButtonTypes.Text,
                        Text="Asm View",
                        Action =async (s) =>
                        {
                            ShowLoader();
                            SelectedBranch=(IBranchSalesReport)s;
                            await ((BranchSalesReportWebViewModel)_viewModel).GetAsmViewByBranchCode(SelectedBranch.BranchCode);
                            IsAsmView=true;
                            IsPopupOpen=true;
                            HideLoader();
                            StateHasChanged();
                        }
                    },
                    new ButtonAction()
                    {
                        ButtonType=ButtonTypes.Text,
                        Text="CP view",
                        Action =async (s) =>
                        {
                            ShowLoader();
                            SelectedBranch=(IBranchSalesReport)s;
                            await ((BranchSalesReportWebViewModel)_viewModel).GetOrgViewByBranchCode(SelectedBranch.BranchCode);
                            IsAsmView=false;
                            IsPopupOpen=true;
                             StateHasChanged();
                            HideLoader();
                        }
                    },
                }
            });
        }

        async Task Export()
        {
            ShowLoader();
            var headers = new Dictionary<string, string>()
            {
                {nameof(IBranchSalesReport.BranchName),"Branch" },
                {nameof(IBranchSalesReport.OrgName),"Channel Partner" },
                {nameof(IBranchSalesReport.ASMName),"ASM" },
                {nameof(IBranchSalesReport.TotalUnits),"Total Units" },
                {nameof(IBranchSalesReport.TotalSales),"Total Sales" },
            };
            var data = await ((BranchSalesReportWebViewModel)_viewModel).GetBranchSalesReportExport();
            await _commonFunctions.ExportToExcelAsync(data, headers, "BranchSalesReport");
            HideLoader();
        }
    }
}
