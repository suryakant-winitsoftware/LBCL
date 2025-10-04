using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Modules.Provisioning.Model.Classes;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;
using Microsoft.JSInterop;
using Winit.Shared.CommonUtilities.Common;
using WINITSharedObjects.Enums;

namespace WinIt.Pages.Maintain_Provisioning
{
    public partial class ProvisionApprovalSummaryView : BaseComponentBase
    {
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<FilterModel> ColumnsForFilter;
        public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>();
        public List<IProvisionApproval> SelectedProvisions { get; set; } = new List<IProvisionApproval>();
        public List<IProvisionApproval> InfoSelectedProvisions { get; set; } = new List<IProvisionApproval>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> ViewPopUpDataGridColumns { get; set; }
        public List<DataGridColumn> InfoDataGridColumns { get; set; }
        public string TabName { get; set; }
        public bool IsFirstColumnCheckbox { get; set; } = true;
        public bool ViewPopUp { get; set; }
        public bool IsShowInfo { get; set; }
        ISelectionItem SelectedTab { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Provision ( Summary View )",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Provision ( Summary View )"},
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
                    new SelectionItem { Label = ProvisionConstants.RequestHistory },
                };
                FilterInitialized();
                await GenerateGridColumns();
                await provisioningHeaderViewModel.PopulateProvisionFilter();
                await provisioningHeaderViewModel.DataSource(ProvisionConstants.Pending, ProvisionConstants.SummaryView);
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.BranchDetails,SelectionMode = SelectionMode.Multiple, Label = "Branch", ColumnName = "PD.branch_code", },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.SalesOfficeDetails,SelectionMode = SelectionMode.Multiple, Label = "Sales Office", ColumnName = "PD.sales_office_code", },
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.BroadClassificationDetails,SelectionMode = SelectionMode.Multiple, Label = "Broad Classification", ColumnName = "S.broad_classification"},
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.ChannelPartnerDetails,SelectionMode = SelectionMode.Multiple, Label = "Channel Partner", ColumnName = "PD.store_uid"},
                new FilterModel {FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues= provisioningHeaderViewModel.ProvisionTypeDetails, SelectionMode = SelectionMode.Multiple,Label = "Provision Type", ColumnName = "PD.scheme_type"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "Start Date", ColumnName = "PD.invoice_date"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "End Date", ColumnName = "InvoiceToDate"},
            };
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                SelectedProvisions.Clear();
                TabItems.ForEach(item => item.IsSelected = false);
                selectionItem.IsSelected = !selectionItem.IsSelected;
                TabName = (selectionItem.Label == ProvisionConstants.RequestHistory) ? ProvisionConstants.Requested : selectionItem.Label;
                SelectedTab = selectionItem;
                if (TabName != ProvisionConstants.Pending)
                {
                    IsFirstColumnCheckbox = false;
                }
                else
                {
                    IsFirstColumnCheckbox = true;
                }
                await provisioningHeaderViewModel.DataSource(TabName, ProvisionConstants.SummaryView);
                await GenerateGridColumns();
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
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner ", GetValue = s => ((IProvisionApproval)s)?.CustomerName ?? "N/A"},
            };
            if (TabName == ProvisionConstants.Pending)
            {
                DataGridColumns.Insert(1, new DataGridColumn { Header = "Provision Type", GetValue = s => ((IProvisionApproval)s)?.ProvisionType ?? "N/A" });
                DataGridColumns.Insert(2, new DataGridColumn { Header = "Amount", GetValue = s => CommonFunctions.FormatNumberInIndianStyle(CommonFunctions.RoundForSystem(((IProvisionApproval)s)?.Amount ?? 0)) });
                DataGridColumns.Insert(3, new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    //ButtonActions = this.buttonActions
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "Images/view_eye.png",
                            Action = async item => await OnViewClick((IProvisionApproval)item),

                        },
                    },
                });
            }
            if (TabName == ProvisionConstants.Requested)
            {
                DataGridColumns.Insert(1, new DataGridColumn { Header = "Requested Date", GetValue = s => ((IProvisionApproval)s)?.RequestedDate == null ? "N/A" : CommonFunctions.GetDateTimeInFormat(((IProvisionApproval)s)?.RequestedDate) });
                DataGridColumns.Insert(2, new DataGridColumn { Header = "Requested By", GetValue = s => ((IProvisionApproval)s)?.RequestedByEmpUID ?? "N/A" });
                DataGridColumns.Insert(3, new DataGridColumn { Header = "Provision Type", GetValue = s => ((IProvisionApproval)s)?.ProvisionType ?? "N/A" });
                DataGridColumns.Insert(4, new DataGridColumn { Header = "Amount", GetValue = s => CommonFunctions.FormatNumberInIndianStyle(CommonFunctions.RoundForSystem(((IProvisionApproval)s)?.Amount ?? 0)) });
                DataGridColumns.Insert(5, new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    //ButtonActions = this.buttonActions
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "Images/view_eye.png",
                            Action = async item => await OnViewClick((IProvisionApproval)item),

                        },
                    },
                });
            }
        }
        private async Task GenerateViewPopUpGridColumns()
        {
            ViewPopUpDataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner ", GetValue = s => ((IProvisionApproval)s)?.CustomerName ?? "N/A"},
                new DataGridColumn { Header = "Provision Type", GetValue = s => ((IProvisionApproval)s)?.ProvisionType ?? "N/A"},
                new DataGridColumn { Header = "Invoice Number", GetValue = s => ((IProvisionApproval)s)?.InvoiceNumber ?? "N/A" },
                new DataGridColumn { Header = "GL Date", GetValue = s => ((IProvisionApproval)s)?.InvoiceDate == null ? "N/A" : CommonFunctions.GetDateTimeInFormat(((IProvisionApproval)s)?.InvoiceDate)},
                new DataGridColumn { Header = "Ar No", GetValue = s => ((IProvisionApproval)s)?.ArNo ?? "N/A"},
                new DataGridColumn { Header = "Item Code", GetValue = s => ((IProvisionApproval)s)?.ItemCode ?? "N/A"},
                new DataGridColumn { Header = "Quantity", GetValue = s => ((IProvisionApproval)s)?.Quantity ?? 0 },
                new DataGridColumn {Header = "Amount", GetValue = s => CommonFunctions.FormatNumberInIndianStyle(CommonFunctions.RoundForSystem(((IProvisionApproval)s)?.Amount ?? 0 )) },
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
        private async Task GenerateInfoGridColumns()
        {
            InfoDataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Naration", GetValue = s => ((IProvisionApproval)s)?.Naration ?? "N/A" },
            };
        }
        public async Task OnInfoClick(IProvisionApproval provisionApproval)
        {
            try
            {
                await GenerateInfoGridColumns();
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
        public async Task OnViewClick(IProvisionApproval provisionApproval)
        {
            try
            {
                List<string> provisionIdsList = provisionApproval.ProvisionIDs?.Split(',').ToList() ?? new List<string>();
                await provisioningHeaderViewModel.PopulateProvisionRequestHistory(provisionIdsList);
                await GenerateViewPopUpGridColumns();
                ViewPopUp = true;
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
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
            {
                ShowLoader();
                await MapFilterFromTab(filterCriteria);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapFilterFromTab(Dictionary<string, string> filterCriteria)
        {
            try
            {
                if (filterCriteria.TryGetValue("S.broad_classification", out var uid))
                {
                    var broadClassificationDetails = provisioningHeaderViewModel.BroadClassificationDetails;
                    var matchingItem = broadClassificationDetails.FirstOrDefault(item => item.UID == uid);

                    if (matchingItem != null)
                    {
                        string name = matchingItem.Label;
                        filterCriteria["S.broad_classification"] = name;
                    }
                }
                if (filterCriteria.TryGetValue("PD.scheme_type", out var UID))
                {
                    var provisionTypeDetails = provisioningHeaderViewModel.ProvisionTypeDetails;
                    var matchingItem = provisionTypeDetails.FirstOrDefault(item => item.UID == UID);

                    if (matchingItem != null)
                    {
                        string name = matchingItem.Label;
                        filterCriteria["PD.scheme_type"] = name;
                    }
                }
                await provisioningHeaderViewModel.OnFilterApply(filterCriteria, ProvisionConstants.SummaryView);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await provisioningHeaderViewModel.PageIndexChanged(pageNumber, ProvisionConstants.SummaryView);
                HideLoader();
            });
        }
        public async Task RedemptionBtnClicked()
        {
            try
            {
                if (SelectedProvisions.Any())
                {
                    if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to request for Redemption?"))
                    {
                        if (await provisioningHeaderViewModel.UpdateStatus(SelectedProvisions, ProvisionConstants.SummaryView) && await provisioningHeaderViewModel.InsertProvisionRequestHistoryDetails(SelectedProvisions))
                        {
                            _tost.Add("Success", "Requested Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            await provisioningHeaderViewModel.DataSource(ProvisionConstants.Pending, ProvisionConstants.SummaryView);
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
                    worksheet.Cell(1, 2).Value = "Channel Partner Name";
                    worksheet.Cell(1, 3).Value = "Provision Type";
                    worksheet.Cell(1, 4).Value = "Amount";

                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < provisioningHeaderViewModel.SummaryViewDetails.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = provisioningHeaderViewModel.SummaryViewDetails[i].CustomerCode;
                        worksheet.Cell(row, 2).Value = provisioningHeaderViewModel.SummaryViewDetails[i].CustomerName;
                        worksheet.Cell(row, 3).Value = provisioningHeaderViewModel.SummaryViewDetails[i].ProvisionType;
                        worksheet.Cell(row,4).Value = provisioningHeaderViewModel.SummaryViewDetails[i].Amount;
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