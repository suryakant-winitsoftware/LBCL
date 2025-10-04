using Microsoft.AspNetCore.Components.Forms;
using Nest;
using OfficeOpenXml;
using System.Data;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Tally.TallyMaster
{
    public partial class SalesInvoiceMaster : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsChannelPartnerSelected { get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; }
        public bool IsItemSelectedToShow { get; set; } = false;
        public Guid fileId { get; set; } = Guid.NewGuid();
        private IBrowserFile selectedFile;
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster> ExcelTallySalesInvoiceDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster>();
        public string DistributorCode { get; set; } = "";
        List<FilterModel> ColumnsForFilter = [];
        public string SelectedDealer = null;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Sales Invoice",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Sales Invoice"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            tallyMasterViewModel.PageSize = 100;
            await tallyMasterViewModel.PopulateChannelPartners();
            SetFilters();
            IsInitialised = true;
            HideLoader();
        }
        public async Task OnChannelPartnerSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                DistributorCode = selectedItem.Code;
                SelectedDealer = selectedItem!.UID;
                await tallyMasterViewModel.GetSalesInvoiceMasterGridDataByDist(selectedItem!.UID);
                tallyMasterViewModel.UID = selectedItem.UID;
                await GenerateGridColumns();
                StateHasChanged();
                IsChannelPartnerSelected = true;
            }
            else
            {
                tallyMasterViewModel.UID = null;
                IsChannelPartnerSelected = false;
                DistributorCode = "";
                ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
            }
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await tallyMasterViewModel.ApplySort(sortCriteria, tallyMasterViewModel.UID, "SalesInvoiceMaster");
            StateHasChanged();
            HideLoader();
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            ShowLoader();
            await tallyMasterViewModel.PageIndexChanged(pageNumber, tallyMasterViewModel.UID, "SalesInvoiceMaster");
            HideLoader();
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
               // new DataGridColumn { Header = "DMS UID", GetValue = item => ((ITallySalesInvoiceMaster)item).Dmsuid ?? "N/A"},
                new DataGridColumn { Header = "Voucher Number", GetValue = item => ((ITallySalesInvoiceMaster)item).VoucherNumber ?? "N/A", IsSortable=true,SortField="VoucherNumber"},
                new DataGridColumn { Header = "Date Of Invoice", GetValue = item => ((ITallySalesInvoiceMaster)item).BasicDatetimeOfInvoice ?? "N/A", IsSortable=true,SortField="Date"},
              //  new DataGridColumn { Header = "GUID", GetValue = item => ((ITallySalesInvoiceMaster)item).Guid ?? "N/A"},
                new DataGridColumn { Header = "State Name", GetValue = item => ((ITallySalesInvoiceMaster)item).StateName ?? "N/A", IsSortable=true,SortField="StateName"},
             //   new DataGridColumn { Header = "Country of Residence", GetValue = item => ((ITallySalesInvoiceMaster)item).CountryOfResidence ?? "N/A"},
                new DataGridColumn { Header = "Party Name", GetValue = item => ((ITallySalesInvoiceMaster)item).PartyName ?? "N/A", IsSortable=true,SortField="PartyName"},
                new DataGridColumn { Header = "Voucher Type Name", GetValue = item => ((ITallySalesInvoiceMaster)item).VoucherTypeName ?? "N/A", IsSortable=true,SortField="VoucherTypeName"},
             //   new DataGridColumn { Header = "Party Ledger Name", GetValue = item => ((ITallySalesInvoiceMaster)item).PartyLedgerName ?? "N/A"},
             //   new DataGridColumn { Header = "Basic Base Party Name", GetValue = item => ((ITallySalesInvoiceMaster)item).BasicBasePartyName ?? "N/A"},
                new DataGridColumn { Header = "Place of Supply", GetValue = item => ((ITallySalesInvoiceMaster)item).PlaceOfSupply ?? "N/A", IsSortable=true,SortField="PlaceOfSupply"},
              //  new DataGridColumn { Header = "Basic Buyer Name", GetValue = item => ((ITallySalesInvoiceMaster)item).BasicBuyerName ?? "N/A"},
              //  new DataGridColumn { Header = "Basic DateTime of Invoice", GetValue = item => ((ITallySalesInvoiceMaster)item).BasicDatetimeOfInvoice ?? "N/A"},
            //   new DataGridColumn { Header = "Consignee Pin Number", GetValue = item => ((ITallySalesInvoiceMaster)item).ConsigneePinNumber ?? "N/A"},
             //   new DataGridColumn { Header = "Consignee State Name", GetValue = item => ((ITallySalesInvoiceMaster)item).ConsigneeStateName ?? "N/A"},
             //   new DataGridColumn { Header = "Voucher Key", GetValue = item => ((ITallySalesInvoiceMaster)item).VoucherKey ?? "N/A"},
                new DataGridColumn { Header = "Net Amount", GetValue = item => ((ITallySalesInvoiceMaster)item).Amount ?? "N/A", IsSortable=true,SortField="Amount"},
             //   new DataGridColumn { Header = "Persisted View", GetValue = item => ((ITallySalesInvoiceMaster)item).PersistedView ?? "N/A"},
                new DataGridColumn { Header = "Distributor Name", GetValue = item => ((ITallySalesInvoiceMaster)item).DistributorCode ?? "N/A", IsSortable=true,SortField="DistributorCode"},
            //    new DataGridColumn { Header = "CGST", GetValue = item => ((ITallySalesInvoiceMaster)item).Cgst ?? "N/A"},
            //    new DataGridColumn { Header = "SGST", GetValue = item => ((ITallySalesInvoiceMaster)item).Sgst ?? "N/A"},
             //   new DataGridColumn { Header = "GST", GetValue = item => ((ITallySalesInvoiceMaster)item).Gst ?? "N/A"},
                new DataGridColumn { Header = "Status", GetValue = item => ((ITallySalesInvoiceMaster)item).Status ?? "N/A", IsSortable=true,SortField="Status"},
                new DataGridColumn { Header = "Action", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/view.png",
                            Action = item => OnViewClick((ITallySalesInvoiceMaster)item)
                        },
                    }},
            };
        }
        private async Task OnViewClick(ITallySalesInvoiceMaster item)
        {
            _navigationManager.NavigateTo($"SalesInvoiceLineMaster?SalesInvoiceUID={item.Guid}");
        }
        private async Task OnBackButtonClick()
        {
            IsItemSelectedToShow = true;
        }
        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Voucher Number",
                    ColumnName = "VoucherNumber"

                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Party Name",
                    ColumnName = "PartyName"
                }

             };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            if (SelectedDealer != null)
            {
                _loadingService.ShowLoading();
                List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Key == "VoucherNumber")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));

                        }
                        else if (keyValue.Key == "PartyName")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                        }
                       
                    }
                }
                await tallyMasterViewModel.ApplyFilterForSalesInvoice(filterCriterias, SelectedDealer);
                StateHasChanged();
                _loadingService.HideLoading();
            }
            else
            {
                ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
            }

        }
        private async Task OnExcelDownloadClick()
        {
            try
            {
                
                tallyMasterViewModel.PageSize = 0;
                tallyMasterViewModel.PageNumber = 0;
                await tallyMasterViewModel.PopulateGridDataForEXCEL(tallyMasterViewModel.UID, "SalesInvoiceMaster");
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
        }
        private async Task UploadDetails()
        {
            ShowLoader();
            ExcelTallySalesInvoiceDetails.Clear();
            try
            {
                fileId = Guid.NewGuid();
                if (selectedFile != null && IsExcelFile(selectedFile))
                {
                    using (var stream = selectedFile.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            if (ExcelFileContainsData(memoryStream))
                            {
                                using (var package = new ExcelPackage(memoryStream))
                                {
                                    var worksheet = package.Workbook.Worksheets[0];
                                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                    {
                                        var rowDict = new Dictionary<string, string>();
                                        for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                                        {
                                            var header = worksheet.Cells[1, col].Text.Trim();
                                            var value = worksheet.Cells[rowNumber, col].Text.Trim();
                                            rowDict[header] = value;
                                        }
                                        var principle = Activator.CreateInstance<Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>();
                                        foreach (var kvp in rowDict)
                                        {
                                            var property = principle.GetType().GetProperty(kvp.Key);
                                            if (property != null && property.CanWrite)
                                            {
                                                var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                                                property.SetValue(principle, convertedValue);
                                            }
                                        }

                                        ExcelTallySalesInvoiceDetails.Add(principle);
                                    }
                                    await ValidateRecordsInExcel();
                                }
                            }
                            else
                            {
                                _tost.Add("Error", "File has empty cells. Please fill and Re-upload.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                            }
                        }
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please upload an Excel File");
                    StateHasChanged(); // Trigger re-render to show validation errors
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
            finally
            {
                HideLoader();
            }
        }
        private bool IsExcelFile(IBrowserFile file)
        {
            // Check if the file extension is .xlsx or .xls
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            return fileExtension == ".xlsx" || fileExtension == ".xls";
        }
        private bool ExcelFileContainsData(MemoryStream stream)
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var dataTable = new DataTable();

                // Read header row separately
                var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                foreach (var headerCell in headerRow)
                {
                    dataTable.Columns.Add(headerCell.Text);
                }


                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var row = dataTable.NewRow();
                    for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                    {
                        row[dataTable.Columns[columnNumber - 1].ColumnName] = worksheet.Cells[rowNumber, columnNumber].Text;
                    }
                    dataTable.Rows.Add(row);

                    // Check for empty cells in the row
                    bool rowHasEmptyCell = false;
                    for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                    {
                        var cellValue = worksheet.Cells[rowNumber, columnNumber].Text;
                        if (columnNumber == 1)
                        {
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(cellValue))
                        {
                            rowHasEmptyCell = true;
                            break; // Exit the loop if an empty cell is found
                        }
                    }
                    if (rowHasEmptyCell)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public async Task ValidateRecordsInExcel()
        {
            try
            {
                if (!string.IsNullOrEmpty(DistributorCode))
                {
                    await ValidateWithPrincipleSKU();
                   // await SaveToDataBase();
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please select distributor");
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }

        public async Task<bool> ValidateWithPrincipleSKU()
        {
            try
            {
                //await MapExcelDetails();
                //TallyDBDetails.Clear();
                //FailedOrInvalidRecords.Clear();
                ////To remove duplicates from excel
                //var distinctExcelTallySkuDetails = ExcelTallySkuDetails
                //                                   .GroupBy(p => new { p.PrincipalSKUCode })
                //                                   .Select(g => g.First())
                //                                   .ToList();
                ////To remove duplicates from excel
                //successDistributorDetails = distinctExcelTallySkuDetails.Where(p => SKUDetails.Any(q => q.Code == p.PrincipalSKUCode && !string.IsNullOrEmpty(p.DistributorSKUName))).ToList();
                //failedOrInvalidDetails = ExcelTallySkuDetails.Where(p => string.IsNullOrEmpty(p.DistributorSKUName) || !SKUDetails.Any(q => q.Code == p.PrincipalSKUCode)).ToList();
                //failedOrInvalidDetails.ForEach(p =>
                //{
                //    if (string.IsNullOrEmpty(p.DistributorSKUName))
                //        p.Status = "Distributor Sku Name is Empty";
                //    else
                //        p.Status = "Invalid Principal Sku Code";
                //});
                //TallyDBDetails.AddRange(successDistributorDetails);
                //FailedOrInvalidRecords.AddRange(failedOrInvalidDetails);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }
        public async Task MapExcelDetails()
        {
            try
            {
                //ExcelTallySkuDetails.Clear();
                //foreach (var data in ExcelTallySKUMappingDetails)
                //{
                //    data.UID = Guid.NewGuid().ToString();
                //    data.CreatedBy = "ADMIN";
                //    data.ModifiedBy = "ADMIN";
                //    data.CreatedTime = DateTime.Now;
                //    data.ModifiedTime = DateTime.Now;
                //    data.ServerAddTime = DateTime.Now;
                //    data.ServerModifiedTime = DateTime.Now;
                //    data.DistributorCode = DistributorCode;
                //    ExcelTallySkuDetails.Add(data);
                //}
            }
            catch (Exception ex)
            {

            }
        }
    }
}
