using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.Scheme.CashDiscountExcludeScheme
{
    public partial class CashDiscountScheme
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
            HeaderText = "Scheme Cash Discount Exclude",
            BreadcrumList =
        [
            new BreadCrumModel()
            {
                SlNo = 1, Text = "Scheme Cash Discount Exclude"
            },
        ]
        };
        public List<FilterModel> ColumnsForFilter;
        public List<SchemeExcludeMapping> schemeExcludeMappingsDetails { get; set; } = new List<SchemeExcludeMapping>();
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                await GenerateGridColumns();
                await _schemeExcludeMappingViewModel.PopulateViewModel();
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
        public async Task CountSuccessRecords()
        {
            try
            {
                SuccessRecordsCount = schemeExcludeMappingsDetails.Count - _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords.Count;
                FailedRecordsCount = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords.Count;
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
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Store Name", ColumnName = "StoreUID", IsForSearch=true, PlaceHolder="Search By Store Name", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "Start Date", ColumnName = "StartDate",  Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "End Date", ColumnName = "EndDate", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox, SelectedBoolValue = true, Label = "Is Active", ColumnName = "IsActive", Width=1000},
            };
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Store", GetValue = s => ((ISchemeExcludeMapping)s)?.StoreUID ?? "N/A", SortField="StoreUID", IsSortable=true},
                new DataGridColumn { Header = "Is Active", GetValue = s => ((ISchemeExcludeMapping)s)?.IsActive, SortField="IsActive", IsSortable=true},
                new DataGridColumn { Header = "Start Date", GetValue = s => ((ISchemeExcludeMapping)s)?.StartDate, SortField="StartDate", IsSortable=true},
                new DataGridColumn {Header = "End Date", GetValue = s =>((ISchemeExcludeMapping) s) ?.EndDate, SortField="EndDate", IsSortable=true},
                new DataGridColumn
                {
                    Header = "Expired On",
                    GetValue = s =>
                    {
                        var endDate = ((ISchemeExcludeMapping)s)?.EndDate;
                        if (endDate == null || endDate == DateTime.MinValue)
                            return "No End Date"; // Handle null or invalid dates
        
                        TimeSpan timeRemaining = endDate.Value - DateTime.Now;
                        int daysRemaining = timeRemaining.Days;
                        int hours = timeRemaining.Hours;

                        if (daysRemaining > 0)
                            return $"({((ISchemeExcludeMapping)s)?.EndDate}) Expires in {daysRemaining} days";
                        else if (daysRemaining == 0 && timeRemaining.TotalSeconds > 0)
                            return $"({((ISchemeExcludeMapping)s)?.EndDate}) Expires in {hours}h";
                        else if (daysRemaining == 0 && timeRemaining.TotalSeconds <= 0)
                            return $"({((ISchemeExcludeMapping)s)?.EndDate}) Expired {hours}h ago";
                        else
                            return $"({((ISchemeExcludeMapping)s)?.EndDate}) Expired {daysRemaining} days ago";
                    }
                },

                //new DataGridColumn
                //{
                //    Header = "Action",
                //    IsButtonColumn = true,
                //    //ButtonActions = this.buttonActions
                //    ButtonActions = new List<ButtonAction>
                //    {
                //        new ButtonAction
                //        {
                //             ButtonType = ButtonTypes.Image,
                //             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                //            Action = async item => await OnEditUsersClick((ISchemeExcludeMapping)item),

                //        },
                //        //new ButtonAction
                //        //{
                //        //     ButtonType = ButtonTypes.Image,
                //        //     URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                //        //    Action = async item => await OnDeleteUsersClick((IOnBoardGridview)item),

                //        //},
                //    }
                //}
            };
        }
        public async Task InsertIntoDB(List<SchemeExcludeMapping> schemeExcludeDetails)
        {
            try
            {
                ShowLoader();
                await _schemeExcludeMappingViewModel.OnFileUploadInsertIntoDB(schemeExcludeDetails);
                if (_schemeExcludeMappingViewModel.SchemeExcludeErrorRecords.Count == 0)
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
                await _schemeExcludeMappingViewModel.PopulateViewModel();
                fileId = Guid.NewGuid();
                HideLoader();
                StateHasChanged();
            }
        }
        private async Task OnSortClick(SortCriteria sortCriteria)
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    ShowLoader();

                    await _schemeExcludeMappingViewModel.OnSorting(sortCriteria);
                });
            }
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            
            try
            {
                await InvokeAsync(async () =>
                {
                    ShowLoader();
                    await _schemeExcludeMappingViewModel.PageIndexChanged(pageNumber);
                });
            }
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
            }
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
         {
            ShowLoader();
            try
            {
                await _schemeExcludeMappingViewModel.OnFilterApply(filterCriteria);
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
        public async Task OnFileUpload()
        {
            schemeExcludeMappingsDetails.Clear();
            IsFileEmpty = false;
            ShowLoader();
            try
            {
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
                                        var principle = Activator.CreateInstance<Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>();
                                        foreach (var kvp in rowDict)
                                        {
                                            var property = principle.GetType().GetProperty(kvp.Key);
                                            if (property != null && property.CanWrite)
                                            {
                                                var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                                                property.SetValue(principle, convertedValue);
                                            }
                                        }
                                        schemeExcludeMappingsDetails.Add(principle);
                                    }
                                }
                            }
                            else
                            {
                                _tost.Add("Error", "File has empty cells. Please fill and Re-upload.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                                await FileEmptyChanges();
                            }
                        }
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please upload an Excel File");
                    await FileEmptyChanges();
                    StateHasChanged(); // Trigger re-render to show validation errors
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsFileEmpty)
                {
                    await InsertIntoDB(schemeExcludeMappingsDetails);
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
        private bool IsExcelFile(IBrowserFile file)
        {
            // Check if the file extension is .xlsx or .xls
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            return fileExtension == ".xlsx" || fileExtension == ".xls";
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
                string fileName = "SchemeExcludeEmptyTemplate_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Cash Discount Exclude");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "StoreUID";
                    worksheet.Cell(1, 2).Value = "StartDate";
                    worksheet.Cell(1, 3).Value = "EndDate";

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
                string fileName = "SchemeExcludeErrorDetails_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Cash Discount Exclude");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Scheme Type";
                    worksheet.Cell(1, 2).Value = "Scheme UID";
                    worksheet.Cell(1, 3).Value = "StoreUID";
                    worksheet.Cell(1, 4).Value = "Start Date";
                    worksheet.Cell(1, 5).Value = "End Date";
                    worksheet.Cell(1, 6).Value = "Is Active";
                    worksheet.Cell(1, 7).Value = "Is Valid";
                    worksheet.Cell(1, 8).Value = "Error Message";


                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].SchemeType ?? "N/A";
                        worksheet.Cell(row, 2).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].SchemeUID ?? "N/A";
                        worksheet.Cell(row, 3).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].StoreUID ?? "N/A";
                        worksheet.Cell(row, 4).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].StartDate;
                        worksheet.Cell(row, 5).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].EndDate;
                        worksheet.Cell(row, 6).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].IsActive;
                        worksheet.Cell(row, 7).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].IsValid;
                        worksheet.Cell(row, 8).Value = _schemeExcludeMappingViewModel.SchemeExcludeErrorRecords[i].ErrorMessage ?? "N/A";
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
        private async Task DownloadSchemeExcludeDetails()
        {
            try
            {
                ShowLoader();
                string fileName = "SchemeExcludeDetails_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Cash Discount Exclude");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "StoreUID";
                    worksheet.Cell(1, 2).Value = "Start Date";
                    worksheet.Cell(1, 3).Value = "End Date";


                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < _schemeExcludeMappingViewModel.SchemeExcludeGridViewRecords.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = _schemeExcludeMappingViewModel.SchemeExcludeGridViewRecords[i].StoreUID ?? "N/A";
                        worksheet.Cell(row, 2).Value = _schemeExcludeMappingViewModel.SchemeExcludeGridViewRecords[i].StartDate;
                        worksheet.Cell(row, 3).Value = _schemeExcludeMappingViewModel.SchemeExcludeGridViewRecords[i].EndDate;
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
