using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;
using Microsoft.AspNetCore.Components;
using Winit.UIModels.Common.Filter;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using System.Data;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Shared.Models.Enums;
using ClosedXML.Excel;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using System.IO;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Classes;
using Winit.Modules.User.BL.Interfaces;
namespace WinIt.Pages.Tally.TallyMaster
{
    public partial class DealerMaster : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        private bool ImportExcelPopup { get; set; } = false;
        public bool IsChannelPartnerSelected { get; set; } = false;
        public bool IsItemSelectedToShow {  get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; } = new List<DataGridColumn>();
        public List<DataGridColumn> DataGridColumnsForExcel { get; set; } = new List<DataGridColumn>();
        List<FilterModel> ColumnsForFilter = [];
        public Guid fileId { get; set; } = Guid.NewGuid();
        public string SelectedDealer = null;
        public string DistributorCode = null;
        private IBrowserFile selectedFile;
        
        public List<ITallyDealerMaster> FailedOrInvalidRecords { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> ExcelTallySKUMappingDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> TallySKUMappingDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> TallyDBDetailsForViewClone { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> TallySKUMappingFilterDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> ExcelTallySkuDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> TallyDBDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> TallyDBDetailsForView { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> successDistributorDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> failedOrInvalidDetails { get; set; } = new List<ITallyDealerMaster>();
        public List<ITallyDealerMaster> PopUpItems { get; set; } = new List<ITallyDealerMaster>();

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Dealer Master",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Dealer Master"},
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
                SelectedDealer = selectedItem!.UID;
                tallyMasterViewModel.UID=selectedItem.UID;
                DistributorCode = selectedItem.Code;
                await tallyMasterViewModel.GetDealerMasterGridDataByDist(selectedItem!.UID);
                GenerateGridColumns();
                GenerateGridColumnsForExcel();
                IsChannelPartnerSelected = true;
                StateHasChanged();
            }
            else
            {
                tallyMasterViewModel.UID = null;
                IsChannelPartnerSelected = false;
                ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
                StateHasChanged();
            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            ShowLoader();
            await tallyMasterViewModel.PageIndexChanged(pageNumber, tallyMasterViewModel.UID,"DealerMaster");
            HideLoader();
        }
        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Ledger Name", GetValue = item => ((ITallyDealerMaster)item).LedgerName ?? "N/A" ,IsSortable=true ,SortField="LedgerName"},
                new DataGridColumn { Header = "Parent Name", GetValue = item => ((ITallyDealerMaster)item).ParentName ?? "N/A",IsSortable=true,SortField="ParentName"},
               // new DataGridColumn { Header = "Opening Balance", GetValue = item => ((ITallyDealerMaster)item).OpeningBalance ?? "N/A"},
                new DataGridColumn { Header = "Primary Group", GetValue = item => ((ITallyDealerMaster)item).PrimaryGroup ?? "N/A",IsSortable=true,SortField="PrimaryGroup"},
              //  new DataGridColumn { Header = "Remote Alt Guid", GetValue = item => ((ITallyDealerMaster)item).RemoteAltGuid ?? "N/A"},
                new DataGridColumn { Header = "Address", GetValue = item => ((ITallyDealerMaster)item).Address ?? "N/A" ,IsSortable=true,SortField="Address"},
              //  new DataGridColumn { Header = "Address 1", GetValue = item => ((ITallyDealerMaster)item).Address1 ?? "N/A"},
             //   new DataGridColumn { Header = "Address 2", GetValue = item => ((ITallyDealerMaster)item).Address2 ?? "N/A"},
              //  new DataGridColumn { Header = "Country Name", GetValue = item => ((ITallyDealerMaster)item).CountryName ?? "N/A"},
             //   new DataGridColumn { Header = "Email", GetValue = item => ((ITallyDealerMaster)item).Email ?? "N/A"},
              //  new DataGridColumn { Header = "Phone", GetValue = item => ((ITallyDealerMaster)item).Phone ?? "N/A"},
            //    new DataGridColumn { Header = "Pincode", GetValue = item => ((ITallyDealerMaster)item).Pincode ?? "N/A"},
                new DataGridColumn { Header = "State Name", GetValue = item => ((ITallyDealerMaster)item).StateName ?? "N/A",IsSortable=true,SortField="StateName"},
             //   new DataGridColumn { Header = "Income Tax Number", GetValue = item => ((ITallyDealerMaster)item).IncomeTaxNumber ?? "N/A"},
             //   new DataGridColumn { Header = "Country of Residence", GetValue = item => ((ITallyDealerMaster)item).CountryOfResidence ?? "N/A"},
                new DataGridColumn { Header = "Distributor Name", GetValue = item => ((ITallyDealerMaster)item).DistributorCode ?? "N/A",IsSortable=true,SortField="DistributorCode"},
                new DataGridColumn { Header = "Status", GetValue = item => ((ITallyDealerMaster)item).Status ?? "N/A",IsSortable=true,SortField="Status"},
                new DataGridColumn { Header = "GSTIN", GetValue = item => ((ITallyDealerMaster)item).GSTIN ?? "N/A",IsSortable=true,SortField="Gstin"},
                new DataGridColumn { Header = "Action", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/view.png",
                            Action = item => OnViewClick((ITallyDealerMaster)item)
                        },
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
                    Label = "GST No",
                    ColumnName = "Gstin"

                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Ledger Name",
                    ColumnName = "LedgerName"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Parent Name",
                    ColumnName = "ParentName"
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
                        if (keyValue.Key == "Gstin")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));

                        }
                        else if (keyValue.Key == "LedgerName")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                        }
                        else if (keyValue.Key == "ParentName")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                        }

                    }
                }
                await tallyMasterViewModel.ApplyFilterForDealer(filterCriterias, SelectedDealer);
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
                await tallyMasterViewModel.PopulateGridDataForEXCEL(tallyMasterViewModel.UID , "DealerMaster");
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private void GenerateGridColumnsForExcel()
        {
            DataGridColumnsForExcel = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Ledger Name", GetValue = item => ((ITallyDealerMaster)item).LedgerName ?? "N/A"},
                new DataGridColumn { Header = "Parent Name", GetValue = item => ((ITallyDealerMaster)item).ParentName ?? "N/A"},
                new DataGridColumn { Header = "Opening Balance", GetValue = item => ((ITallyDealerMaster)item).OpeningBalance ?? "N/A"},
                new DataGridColumn { Header = "Primary Group", GetValue = item => ((ITallyDealerMaster)item).PrimaryGroup ?? "N/A"},
               // new DataGridColumn { Header = "Remote Alt Guid", GetValue = item => ((ITallyDealerMaster)item).RemoteAltGuid ?? "N/A"},
                new DataGridColumn { Header = "Address", GetValue = item => ((ITallyDealerMaster)item).Address ?? "N/A"},
                new DataGridColumn { Header = "Address 1", GetValue = item => ((ITallyDealerMaster)item).Address1 ?? "N/A"},
                new DataGridColumn { Header = "Address 2", GetValue = item => ((ITallyDealerMaster)item).Address2 ?? "N/A"},
                new DataGridColumn { Header = "Country Name", GetValue = item => ((ITallyDealerMaster)item).CountryName ?? "N/A"},
                new DataGridColumn { Header = "Email", GetValue = item => ((ITallyDealerMaster)item).Email ?? "N/A"},
                new DataGridColumn { Header = "Phone", GetValue = item => ((ITallyDealerMaster)item).Phone ?? "N/A"},
                new DataGridColumn { Header = "Pincode", GetValue = item => ((ITallyDealerMaster)item).Pincode ?? "N/A"},
                new DataGridColumn { Header = "State Name", GetValue = item => ((ITallyDealerMaster)item).StateName ?? "N/A"},
                new DataGridColumn { Header = "Income Tax Number", GetValue = item => ((ITallyDealerMaster)item).IncomeTaxNumber ?? "N/A"},
                new DataGridColumn { Header = "Country of Residence", GetValue = item => ((ITallyDealerMaster)item).CountryOfResidence ?? "N/A"},
                new DataGridColumn { Header = "Distributor Code", GetValue = item => ((ITallyDealerMaster)item).DistributorCode ?? "N/A"},
                new DataGridColumn { Header = "Status", GetValue = item => ((ITallyDealerMaster)item).Status ?? "N/A"},
                new DataGridColumn { Header = "GSTIN", GetValue = item => ((ITallyDealerMaster)item).GSTIN ?? "N/A"},
            };
        }
        private async Task OnViewClick(ITallyDealerMaster item)
        {
            ShowLoader();
            await tallyMasterViewModel.GetDealerMasterItemDetails(item.RemoteAltGuid);
            IsItemSelectedToShow = true;
            HideLoader();
            StateHasChanged();
        }
        private void OnBackButtonClick()
        {
            IsItemSelectedToShow = false;
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await tallyMasterViewModel.ApplySort(sortCriteria, tallyMasterViewModel.UID,"DealerMaster");
            StateHasChanged();
            HideLoader();
        }
        private async Task DownloadTemplate()
        {
            try
            {
                ShowLoader();
                string fileName = "TallyTemplate_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Dealer Master");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Ledger Name";
                    worksheet.Cell(1, 2).Value = "Parent Name";
                    worksheet.Cell(1, 3).Value = "Opening Balance";
                    worksheet.Cell(1, 4).Value = "Primary Group";
                    // worksheet.Cell(1, 5).Value = "Remote Alt Guid"; // Uncomment if needed
                    worksheet.Cell(1, 5).Value = "Address";
                    worksheet.Cell(1, 6).Value = "Address 1";
                    worksheet.Cell(1, 7).Value = "Address 2";
                    worksheet.Cell(1, 8).Value = "Country Name";
                    worksheet.Cell(1, 9).Value = "Email";
                    worksheet.Cell(1, 10).Value = "Phone";
                    worksheet.Cell(1, 11).Value = "Pincode";
                    worksheet.Cell(1, 12).Value = "State Name";
                    worksheet.Cell(1, 13).Value = "Income Tax Number";
                    worksheet.Cell(1, 14).Value = "Country of Residence";
                    worksheet.Cell(1, 15).Value = "Distributor Code";
                    worksheet.Cell(1, 16).Value = "Status";
                    worksheet.Cell(1, 17).Value = "GSTIN";


                    // Populate the Excel worksheet with your data from elem
                    //for (int i = 0; i < SKUDetails.Count; i++)
                    //{
                    //    var row = i + 2;
                    //    worksheet.Cell(row, 2).Value = SKUDetails[i].Code;
                    //    worksheet.Cell(row, 3).Value = SKUDetails[i].Name;
                    //}

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
        public string RandomSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 1000000).ToString();
        }
        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
        }
        private async Task UploadDetails()
        {
            ShowLoader();
            ExcelTallySKUMappingDetails.Clear();
            try
            {
                fileId = Guid.NewGuid();
                if (selectedFile != null && IsExcel(selectedFile))
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
                                        var principle = Activator.CreateInstance<ITallyDealerMaster>();
                                        foreach (var kvp in rowDict)
                                        {
                                            var property = principle.GetType().GetProperty(kvp.Key);
                                            if (property != null && property.CanWrite)
                                            {
                                                var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                                                property.SetValue(principle, convertedValue);
                                            }
                                        }

                                        ExcelTallySKUMappingDetails.Add(principle);
                                    }
                                    await ValidateRecordsInExcel();
                                }
                            }
                            else
                            {
                                _tost.Add("Error", "File has empty cells. Please fill and re-upload.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                            }
                        }
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please upload an excel file");
                    StateHasChanged();
                }
            }
            catch(Exception ex)
            {

            }
            HideLoader();

        }
        private async Task ValidateRecordsInExcel()
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedDealer))
                {
                    await ValidateWithPrincipleSKU();
                    await SaveToDataBase();
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
        public async Task SaveToDataBase()
        {
            try
            {
                ShowLoader();
                if (await Validations())
                {
                    bool IsInserted = await tallyMasterViewModel.InsertTallyMaster(TallyDBDetails);
                    if (IsInserted)
                    {
                        _tost.Add("Success", "Data saved successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        // TallyDBDetailsForView = await GetTallySKUDataFromDB();
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Data saving failed...");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                selectedFile = null;
                ExcelTallySKUMappingDetails.Clear();
                HideLoader();
                StateHasChanged();
            }
        }

        public async Task<bool> ValidateWithPrincipleSKU()
        {
            try
            {
                await MapExcelDetails();
                TallyDBDetails.Clear();
                FailedOrInvalidRecords.Clear();
                //To remove duplicates from excel
                var distinctExcelTallySkuDetails = ExcelTallySkuDetails
                                                   .GroupBy(p => new { p.DistributorCode })
                                                   .Select(g => g.First())
                                                   .ToList();
                //To remove duplicates from excel
              //  successDistributorDetails = distinctExcelTallySkuDetails.Where(p => SKUDetails.Any(q => q.Code == p.DistributorCode && !string.IsNullOrEmpty(p.LedgerName))).ToList();
              //  failedOrInvalidDetails = ExcelTallySkuDetails.Where(p => string.IsNullOrEmpty(p.DistributorCode) || !SKUDetails.Any(q => q.Code == p.PrincipalSKUCode)).ToList();
                //failedOrInvalidDetails.ForEach(p =>
                //{
                //    if (string.IsNullOrEmpty(p.DistributorSKUName))
                //        p.Status = "Distributor Name is Empty";
                //    else
                //        p.Status = "Invalid Principal Sku Code";
                //});
                TallyDBDetails.AddRange(successDistributorDetails);
                FailedOrInvalidRecords.AddRange(failedOrInvalidDetails);
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
                ExcelTallySkuDetails.Clear();
                foreach (var data in ExcelTallySKUMappingDetails)
                {
                    data.UID = Guid.NewGuid().ToString();
                    data.CreatedBy = "ADMIN";
                    data.ModifiedBy = "ADMIN";
                    data.CreatedTime = DateTime.Now;
                    data.ModifiedTime = DateTime.Now;
                    data.ServerAddTime = DateTime.Now;
                    data.ServerModifiedTime = DateTime.Now;
                    data.DistributorCode = DistributorCode;
                    ExcelTallySkuDetails.Add(data);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private bool ExcelFileContainsData(MemoryStream memoryStream)
        {
            using (var package = new ExcelPackage(memoryStream))
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
        public async Task<bool> Validations()
        {
            try
            {
                if (selectedFile == null)
                {
                    ShowErrorSnackBar("Error", "Please upload file");
                    return false;
                }

                if (ExcelTallySkuDetails.Count == 0)
                {
                    ShowErrorSnackBar("Error", "Data is empty");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task GetGridData(string Tab)
        {
            try
            {
        //        TallyDBDetailsForView = await _tallySKUMappingViewModel.GetAllSKUMappingDetailsByDistCode(DistributorCode, Tab);
                TallyDBDetailsForViewClone = TallyDBDetailsForView;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        
        private bool IsExcel(IBrowserFile file)
        {
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            return fileExtension == ".xlsx" || fileExtension == ".xls";
        }
        
    }
}
