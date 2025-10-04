using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Modules.Provisioning.Model.Classes;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;
using Microsoft.JSInterop;

namespace WinIt.Pages.Maintain_Provisioning
{
    public partial class ProvisionApprovalDetailView : BaseComponentBase
    {
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<FilterModel> ColumnsForFilter;
        public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>();
        public List<IProvisionApproval> SelectedProvisions { get; set; } = new List<IProvisionApproval>();
        public List<IProvisionApproval> InfoSelectedProvisions { get; set; } = new List<IProvisionApproval>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> InfoDataGridColumns { get; set; }
        public string TabName { get; set; }
        public bool IsFirstColumnCheckbox { get; set; } = true;
        public bool IsShowInfo { get; set; }
        ISelectionItem SelectedTab { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Provision ( Detail View )",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Provision ( Detail View )"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            try
            {
                TabName = ProvisionConstants.Pending;
                TabItems = new List<ISelectionItem>
                {
                    new SelectionItem { Label = ProvisionConstants.Pending, IsSelected = true },
                    new SelectionItem { Label = ProvisionConstants.Requested },
                    new SelectionItem { Label = ProvisionConstants.Confirmed },
                    new SelectionItem { Label = ProvisionConstants.PendingP1 },
                    new SelectionItem { Label = ProvisionConstants.PendingCD }
                };
                FilterInitialized();
                await GenerateGridColumns();
                await provisioningHeaderViewModel.PopulateProvisionFilter();
                await provisioningHeaderViewModel.DataSource(ProvisionConstants.Pending, ProvisionConstants.DetailView);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.BranchDetails,SelectionMode = SelectionMode.Multiple, Label = "Branch", ColumnName = "Branch", },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.SalesOfficeDetails,SelectionMode = SelectionMode.Multiple, Label = "Sales Office", ColumnName = "SalesOffice", },
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.BroadClassificationDetails,SelectionMode = SelectionMode.Multiple, Label = "Broad Classification", ColumnName = "ClassificationFilter"},
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.ChannelPartnerDetails,SelectionMode = SelectionMode.Multiple, Label = "Channel Partner", ColumnName = "ChannelPartner"},
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.ProvisionTypeDetails, SelectionMode = SelectionMode.Multiple,Label = "Provision Type", ColumnName = "SchemeType"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "Start Date", ColumnName = "InvoiceDate"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "End Date", ColumnName = "InvoiceToDate"},
            };
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            TabItems.ForEach(item => item.IsSelected = false);
            selectionItem.IsSelected = !selectionItem.IsSelected;
            TabName = selectionItem.Label;
            SelectedTab = selectionItem;
            if (TabName != ProvisionConstants.Pending)
            {
                IsFirstColumnCheckbox = false;
            }
            else
            {
                IsFirstColumnCheckbox = true;
            }
            await provisioningHeaderViewModel.DataSource(TabName, ProvisionConstants.DetailView);
            await GenerateGridColumns();
            StateHasChanged();

        }

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner", GetValue = s => ((IProvisionApproval)s)?.CustomerName ?? "N/A"},
                new DataGridColumn { Header = "Provision Type", GetValue = s => ((IProvisionApproval)s)?.ProvisionType ?? "N/A"},
                new DataGridColumn { Header = "Invoice Number", GetValue = s => ((IProvisionApproval)s)?.InvoiceNumber ?? "N/A" },
                new DataGridColumn { Header = "GL Date", GetValue = s => ((IProvisionApproval)s)?.InvoiceDate == null ? "N/A" : CommonFunctions.GetDateTimeInFormat(((IProvisionApproval)s)?.InvoiceDate)},
                new DataGridColumn { Header = "Ar No", GetValue = s => ((IProvisionApproval)s)?.ArNo ?? "N/A"},
                new DataGridColumn { Header = "Item Code", GetValue = s => ((IProvisionApproval)s)?.ItemCode ?? "N/A"},
                new DataGridColumn { Header = "Quantity", GetValue = s => ((IProvisionApproval)s)?.Quantity ?? 0 },
                new DataGridColumn { Header = "Amount", GetValue = s => CommonFunctions.FormatNumberInIndianStyle(CommonFunctions.RoundForSystem(((IProvisionApproval)s)?.Amount ?? 0 ))},
                new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    //ButtonActions = this.buttonActions
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "Images/info_1.png",
                            Action = async item => await OnInfoClick((IProvisionApproval)item),

                        },
                    },
                }
            };
        }
        public async Task OnGridCheckBoxSelection(HashSet<object> hashSet)
        {
            try
            {
                SelectedProvisions.Clear();
                SelectedProvisions.AddRange(hashSet.OfType<IProvisionApproval>().ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task OnInfoClick(IProvisionApproval provisionApproval)
        {
            try
            {
                InfoSelectedProvisions.Clear();
                InfoSelectedProvisions.AddRange(new List<IProvisionApproval> { provisionApproval });
                IsShowInfo = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                StateHasChanged();
            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await provisioningHeaderViewModel.PageIndexChanged(pageNumber, ProvisionConstants.DetailView);
                HideLoader();
            });
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
            {
                ShowLoader();
                await provisioningHeaderViewModel.OnFilterApply(filterCriteria, ProvisionConstants.DetailView);
                HideLoader();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task RedemptionBtnClicked()
        {
            try
            {
                if (SelectedProvisions.Any())
                {
                    if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to request for Redemption?"))
                    {
                        if (await provisioningHeaderViewModel.UpdateStatus(SelectedProvisions, ProvisionConstants.DetailView))
                        {
                            _tost.Add("Success", "Requested Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            await provisioningHeaderViewModel.DataSource(ProvisionConstants.Pending, ProvisionConstants.DetailView);
                            SelectedProvisions.Clear();
                        }
                        else
                        {
                            _tost.Add("Error", "Requested failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                        }
                    }
                }
                else
                {
                    _tost.Add("Alert", "Please select Provisions", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                StateHasChanged();
            }
        }
        public string RandomSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 1000000).ToString();
        }
        private async Task ExportToExcel()
        {
            try
            {
                ShowLoader();
                string fileName = "Provision - " + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Provision");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Channel Partner Code";
                    worksheet.Cell(1, 2).Value = "Channel Partner";
                    worksheet.Cell(1, 3).Value = "Provision Type";
                    worksheet.Cell(1, 4).Value = "Amount";
                    worksheet.Cell(1, 5).Value = "Invoice Number";
                    worksheet.Cell(1, 6).Value = "GL Date";
                    worksheet.Cell(1, 7).Value = "Ar No";
                    worksheet.Cell(1, 8).Value = "Item Code";
                    worksheet.Cell(1, 9).Value = "Quantity";
                    int currentColumn = 10;

                    if (TabName == ProvisionConstants.Confirmed)
                    {
                        // Add CN-specific columns
                        worksheet.Cell(1, currentColumn++).Value = "Cn Number";
                        worksheet.Cell(1, currentColumn++).Value = "Cn Date";
                        worksheet.Cell(1, currentColumn++).Value = "Cn Amount";
                    }

                    // Remaining columns
                    worksheet.Cell(1, currentColumn++).Value = "Naration";
                    worksheet.Cell(1, currentColumn).Value = "Remarks";

                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < provisioningHeaderViewModel.DetailViewDetails.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = provisioningHeaderViewModel.DetailViewDetails[i].CustomerCode ?? "N/A";
                        worksheet.Cell(row, 2).Value = provisioningHeaderViewModel.DetailViewDetails[i].CustomerName ?? "N/A";
                        worksheet.Cell(row, 3).Value = provisioningHeaderViewModel.DetailViewDetails[i].ProvisionType ?? "N/A";
                        worksheet.Cell(row, 4).Value = provisioningHeaderViewModel.DetailViewDetails[i].Amount ?? 0;
                        worksheet.Cell(row, 5).Value = provisioningHeaderViewModel.DetailViewDetails[i].InvoiceNumber ?? "N/A";
                        worksheet.Cell(row, 6).Value = CommonFunctions.GetDateTimeInFormat(provisioningHeaderViewModel.DetailViewDetails[i].InvoiceDate) ?? null;
                        worksheet.Cell(row, 7).Value = provisioningHeaderViewModel.DetailViewDetails[i].ArNo ?? "N/A";
                        worksheet.Cell(row, 8).Value = provisioningHeaderViewModel.DetailViewDetails[i].ItemCode ?? "N/A";
                        worksheet.Cell(row, 9).Value = provisioningHeaderViewModel.DetailViewDetails[i].Quantity ?? 0;
                        int ColumnNum = 10;
                        if (TabName == ProvisionConstants.Confirmed)
                        {
                            worksheet.Cell(row, ColumnNum++).Value = provisioningHeaderViewModel.DetailViewDetails[i].CnNumber ?? "N/A";
                            worksheet.Cell(row, ColumnNum++).Value = provisioningHeaderViewModel.DetailViewDetails[i].CnDate ?? null;
                            worksheet.Cell(row, ColumnNum++).Value = provisioningHeaderViewModel.DetailViewDetails[i].CnAmount ?? 0;
                        }
                        worksheet.Cell(row, ColumnNum++).Value = provisioningHeaderViewModel.DetailViewDetails[i].Naration ?? "N/A";
                        worksheet.Cell(row, ColumnNum).Value = provisioningHeaderViewModel.DetailViewDetails[i].Remarks ?? "N/A";
                    }

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var bytes = stream.ToArray();
                    string base64 = Convert.ToBase64String(bytes);

                    var anchor = $@"<a href='data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}' download='{fileName}.xlsx'>Download Excel</a>";

                    // Use JavaScript interop to trigger a click event on the anchor element
                    await JSRuntime.InvokeVoidAsync("eval", $"var a = document.createElement('a'); a.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}'; a.download = '{fileName}.xlsx'; a.click();");

                    // Optionally, you can show a confirmation message to the user
                    //Snackbar.Add("Exported to Excel", Severity.Success);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();
            }
        }
    }
}
