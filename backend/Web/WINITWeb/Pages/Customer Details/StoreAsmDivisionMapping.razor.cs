using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.Customer_Details
{
    public partial class StoreAsmDivisionMapping
    {
        public List<DataGridColumn> DataGridColumns { get; set; }
        private IBrowserFile selectedFile;
        public Guid fileId { get; set; } = Guid.NewGuid();
        public bool ShowErrorMessage { get; set; }
        public bool IsFileEmpty { get; set; }
        public int SuccessRecordsCount { get; set; } = 0;
        public int FailedRecordsCount { get; set; } = 0;
        private readonly IDataService DataService = new DataServiceModel()
        {
            HeaderText = "Store Asm Mapping",
            BreadcrumList =
        [
            new BreadCrumModel()
            {
                SlNo = 1, Text = "Store Asm Mapping"
            },
        ]
        };
        public List<FilterModel> ColumnsForFilter;
        public List<StoreAsmMapping> storeAsmMappingDetails { get; set; } = new List<StoreAsmMapping>();
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                await GenerateGridColumns();
                await _storeAsmMappingViewModel.PopulateViewModel();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        protected override void OnInitialized()
        {
            try
            {
                ShowLoader();
                FilterInitialized();
            }
            catch (Exception)
            {

                throw;
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "AsmEmpName", ColumnName = "AsmEmpName", IsForSearch=true, PlaceHolder="Search By AsmEmpName", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "DivisionName", ColumnName = "DivisionName",  IsForSearch=true, PlaceHolder="Search By Division Name", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "DivisionUID", ColumnName = "DivisionUID",  IsForSearch=true, PlaceHolder="Search By Division UID", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Type", ColumnName = "LinkedItemType", IsForSearch=true, PlaceHolder="Search By Type", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Store Name", ColumnName = "StoreName", IsForSearch=true, PlaceHolder="Search By Store Name", Width=1000},
            };
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Asm Emp Name", GetValue = s => ((IAsmDivisionMapping)s)?.AsmEmpName ?? "N/A"},
                new DataGridColumn {Header = "Division Name", GetValue = s =>((IAsmDivisionMapping) s) ?.DivisionName ?? "N/A"},
                new DataGridColumn {Header = "Division UID", GetValue = s =>((IAsmDivisionMapping) s) ?.DivisionUID ?? "N/A"},
                new DataGridColumn { Header = "Type", GetValue = s => ((IAsmDivisionMapping)s)?.LinkedItemType ?? "N/A"},
                new DataGridColumn { Header = "Store Name", GetValue = s => ((IAsmDivisionMapping)s)?.StoreName ?? "N/A"},
            };
        }
        public async Task InsertIntoDB(List<StoreAsmMapping> storeAsmDetails)
        {
            try
            {
                ShowLoader();
                await _storeAsmMappingViewModel.OnFileUploadInsertIntoDB(storeAsmDetails);
                if (_storeAsmMappingViewModel.StoreAsmMappingErrorRecords.Count == 0)
                {
                    ShowSuccessSnackBar("Success", "Data saved successfully");
                }
                else
                {
                    await CountSuccessRecords();
                }
            }
            catch (Exception ex)
            {
                ShowErrorSnackBar("Error", "Exception");
            }
            finally
            {
                await _storeAsmMappingViewModel.PopulateViewModel();
                fileId = Guid.NewGuid();
                HideLoader();
                StateHasChanged();
            }
        }
        public async Task CountSuccessRecords()
        {
            try
            {
                SuccessRecordsCount = storeAsmMappingDetails.Count - _storeAsmMappingViewModel.StoreAsmMappingErrorRecords.Count;
                FailedRecordsCount = storeAsmMappingDetails.Count;
                ShowErrorMessage = true;
            }
            catch (Exception)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }
        private async Task OnSortClick(SortCriteria sortCriteria)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await _storeAsmMappingViewModel.OnSorting(sortCriteria);
                HideLoader();
            });
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await _storeAsmMappingViewModel.PageIndexChanged(pageNumber);
                HideLoader();
            });
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            ShowLoader();
            try
            {
                await _storeAsmMappingViewModel.OnFilterApply(filterCriteria);
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
        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            ShowErrorMessage = false;
            StateHasChanged();
        }
        private async Task<bool> ExcelFileContainsData(ExcelWorksheet worksheet)
        {
            var allowedEmptyColumn = "SiteCode"; // Column that can be empty
            var headers = new List<string>();
            if (worksheet.Dimension == null || worksheet.Dimension.End.Row < 2)
            {
                throw new CustomException(ExceptionStatus.Failed, CustomExceptionConstants.EmptyFile);
            }
            // Extract headers
            for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                headers.Add(worksheet.Cells[1, col].Text.Trim());
            }

            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = headers[col - 1];
                    var cellValue = worksheet.Cells[row, col].Text.Trim();

                    if (string.IsNullOrEmpty(cellValue) && !header.Equals(allowedEmptyColumn, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new CustomException(ExceptionStatus.Failed, CustomExceptionConstants.EmptyCell); // Found an empty cell (excluding SiteCode)
                    }
                }
            }
            return true;
        }
        private bool IsExcelFile(IBrowserFile file)
        {
            // Check if the file extension is .xlsx or .xls
            if (file == null)
                throw new CustomException(ExceptionStatus.Failed, CustomExceptionConstants.NoFile);
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            if (fileExtension == ".xlsx" || fileExtension == ".xls")
            {
                return true;
            }
            throw new CustomException(ExceptionStatus.Failed, CustomExceptionConstants.InvalidFileFormat);
        }
        private async Task<bool> ValidateDataFormat(ExcelWorksheet worksheet)
        {
            try
            {
                var expectedHeaders = new List<string> { "CustomerCode", "CustomerName", "SiteCode", "Division", "EmpCode", "EmpName" }; //Actual headers
                var excelHeaders = new List<string>();

                // Extract headers from Excel file
                for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim();
                    excelHeaders.Add(header);
                }

                // Validate headers
                if (!expectedHeaders.All(h => excelHeaders.Contains(h)))
                {
                    throw new Exception();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new CustomException(ExceptionStatus.Failed, CustomExceptionConstants.InvalidDataFormat);
            }
        }
        public async Task OnFileUpload()
        {
            ShowLoader();
            try
            {
                storeAsmMappingDetails.Clear();
                IsFileEmpty = false;
                if (IsExcelFile(selectedFile))
                {
                    using (var stream = selectedFile.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            using (var package = new ExcelPackage(memoryStream))
                            {
                                var worksheet = package.Workbook.Worksheets[0];

                                if(await ValidateDataFormat(worksheet) && await ExcelFileContainsData(worksheet)) 
                                {
                                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                    {
                                        var rowDict = new Dictionary<string, string>();
                                        for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                                        {
                                            var header = worksheet.Cells[1, col].Text.Trim();
                                            var value = worksheet.Cells[rowNumber, col].Text.Trim();
                                            rowDict[header] = value;
                                        }
                                        var principle = Activator.CreateInstance<Winit.Modules.Store.Model.Classes.StoreAsmMapping>();
                                        foreach (var kvp in rowDict)
                                        {
                                            var property = principle.GetType().GetProperty(kvp.Key);
                                            if (property != null && property.CanWrite)
                                            {
                                                var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                                                property.SetValue(principle, convertedValue);
                                            }
                                        }
                                        storeAsmMappingDetails.Add(principle);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (CustomException ex)
            {
                await FileEmptyChanges();
                if (ex.Message == CustomExceptionConstants.InvalidDataFormat)
                {
                    ShowErrorSnackBar("Error", "Please Re-upload the file. File format is different");
                }
                else if(ex.Message == CustomExceptionConstants.EmptyCell)
                {
                    ShowErrorSnackBar("Error", "Please Re-upload the file. File contains empty cells");
                }
                else if(ex.Message == CustomExceptionConstants.InvalidFileFormat)
                {
                    ShowErrorSnackBar("Error", "Please upload an Excel File");
                }
                else if(ex.Message == CustomExceptionConstants.EmptyFile)
                {
                    ShowErrorSnackBar("Error", "File is empty");
                }
                else if(ex.Message == CustomExceptionConstants.NoFile)
                {
                    ShowErrorSnackBar("Error", "Please upload file");
                }
                else
                {
                    Console.WriteLine(ex);
                }
            }
            finally
            {
                if (!IsFileEmpty && storeAsmMappingDetails.Count > 0)
                {
                    await InsertIntoDB(storeAsmMappingDetails);
                }
                selectedFile = default;
                HideLoader();
                StateHasChanged();
            }
        }
        private async Task FileEmptyChanges()
        {
            try
            {
                IsFileEmpty = true;
                fileId = Guid.NewGuid();
                StateHasChanged();
            }
            catch (Exception)
            {

            }
        }
        
        public string RandomSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 1000000).ToString();
        }
        private async Task DownloadEmptyTemplate()
        {
            try
            {
                ShowLoader();
                string fileName = "StoreAsmMappingEmptyTemplate_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Store Asm Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "CustomerCode";
                    worksheet.Cell(1, 2).Value = "CustomerName";
                    worksheet.Cell(1, 3).Value = "SiteCode";
                    worksheet.Cell(1, 4).Value = "Division";
                    worksheet.Cell(1, 5).Value = "EmpCode";
                    worksheet.Cell(1, 6).Value = "EmpName";

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
        private async Task DownloadErrorDetails()
        {
            ShowErrorMessage = false;
            try
            {
                ShowLoader();
                string fileName = "AsmDivisionMappingErrorDetails_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Asm Division Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Customer Code";
                    worksheet.Cell(1, 2).Value = "Customer Name";
                    worksheet.Cell(1, 3).Value = "Site Code";
                    worksheet.Cell(1, 4).Value = "Division";
                    worksheet.Cell(1, 5).Value = "Emp Code";
                    worksheet.Cell(1, 6).Value = "Emp Name";
                    worksheet.Cell(1, 7).Value = "IsValid";
                    worksheet.Cell(1, 8).Value = "ErrorMessage";


                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < _storeAsmMappingViewModel.StoreAsmMappingErrorRecords.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].CustomerCode ?? "N/A";
                        worksheet.Cell(row, 2).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].CustomerName ?? "N/A";
                        worksheet.Cell(row, 3).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].SiteCode ?? "N/A";
                        worksheet.Cell(row, 4).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].Division ?? "N/A";
                        worksheet.Cell(row, 5).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].EmpCode ?? "N/A";
                        worksheet.Cell(row, 6).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].EmpName ?? "N/A";
                        worksheet.Cell(row, 7).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].IsValid;
                        worksheet.Cell(row, 8).Value = _storeAsmMappingViewModel.StoreAsmMappingErrorRecords[i].ErrorMessage ?? "N/A";
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
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        private async Task DownloadStoreAsmMappingDetails()
        {
            try
            {
                ShowLoader();
                string fileName = "StoreAsmMappingDetails_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Store Asm Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "AsmEmpName";
                    worksheet.Cell(1, 2).Value = "AsmEmpUID";
                    worksheet.Cell(1, 3).Value = "Division Name";
                    worksheet.Cell(1, 4).Value = "Division UID";
                    worksheet.Cell(1, 5).Value = "Type";
                    worksheet.Cell(1, 6).Value = "Store UID";


                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].AsmEmpName ?? "N/A";
                        worksheet.Cell(row, 2).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].AsmEmpUID ?? "N/A";
                        worksheet.Cell(row, 3).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].DivisionName ?? "N/A";
                        worksheet.Cell(row, 4).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].DivisionUID ?? "N/A";
                        worksheet.Cell(row, 5).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].LinkedItemType ?? "N/A";
                        worksheet.Cell(row, 6).Value = _storeAsmMappingViewModel.StoreAsmMappingGridViewRecords[i].LinkedItemUID ?? "N/A";
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
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
    }
}
