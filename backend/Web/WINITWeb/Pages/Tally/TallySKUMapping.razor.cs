using Microsoft.AspNetCore.Components;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Store.Model.Classes;
using WinIt.Pages.Customer_Details;
using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using Nest;
using Winit.Shared.Models.Common;
using Winit.Modules.Tally.Model.Interfaces;
using ClosedXML.Excel;
using Microsoft.JSInterop;
using MathNet.Numerics.Optimization.TrustRegion;
using System.util;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.Modules.Contact.Model.Interfaces;
using System.Data;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.Tally.Model.Classes;

namespace WinIt.Pages.Tally
{
    public partial class TallySKUMapping
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsViewStatus { get; set; }
        public bool ShowHideMessage { get; set; }
        private bool IsFilterOpen = false;
        private IBrowserFile selectedFile;
        public string SelectedTab { get; set; } = TallyConstants.MappedWithCMI;
        public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { Label = TallyConstants.MappedWithCMI, IsSelected = true },
            new SelectionItem { Label = TallyConstants.UnMappedWithCMI }
        };
        public Guid fileId { get; set; } = Guid.NewGuid();
        private ValidationResult validationResult;
        [Parameter] public List<ISelectionItem> DistributorselectionItems { get; set; }
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> ExcelTallySKUMappingDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> TallySKUMappingDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> TallyDBDetailsForViewClone { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> TallySKUMappingFilterDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> ExcelTallySkuDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> TallyDBDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> TallyDBDetailsForView { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> successDistributorDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> failedOrInvalidDetails { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> PopUpItems { get; set; } = new List<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping>();
        public List<IEmp> DistributorsDetails { get; set; } = new List<IEmp>();
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKUDetails { get; set; } = new List<Winit.Modules.SKU.Model.Interfaces.ISKUV1>();
        private List<DataGridColumn> ProductColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> UnMappedProductColumns { get; set; } = new List<DataGridColumn>();
        private List<DataGridColumn> PopUpColumns { get; set; } = new List<DataGridColumn>();
        public string SearchString { get; set; }
        public string DistributorCode { get; set; } = "";
        public bool ShowCountofRecords { get; set; } = false;
        public List<ITallySKUMapping> FailedOrInvalidRecords { get; set; } = new List<ITallySKUMapping>();
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, bool> ColumnStatuses { get; set; } = new Dictionary<string, bool>();
        }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Tally SKU Mapping",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Tally SKU Mapping",IsClickable=false},
            }
        };
        public string RandomSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 1000000).ToString();
        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            SKUDetails = await _tallySKUMappingViewModel.GetAllSKUDetailsByOrgUID(_iAppUser.SelectedJobPosition.OrgUID);//sku
            DistributorselectionItems = CommonFunctions.ConvertToSelectionItems(await _tallySKUMappingViewModel.GetAllDistributors(), new List<string> { "UID", "Name", "Code" });
            GridColumns();
            IsInitialised = true;
            HideLoader();
            StateHasChanged();
        }


        public async Task OnDistributorSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                DistributorCode = selecetedValue.Code;
                await GetGridData(await SetTabName());
                ShowCountofRecords = true;
            }
            else
            {
                TallyDBDetailsForView.Clear();
                _tallySKUMappingViewModel.TotalCount = 0;
                DistributorCode = "";
            }
            StateHasChanged();
        }
        public async Task<string> SetTabName()
        {
            try
            {
                if (SelectedTab == TallyConstants.MappedWithCMI)
                    return TallyConstants.MappedWithCMI;
                else
                    return TallyConstants.UnMappedWithCMI;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task GetGridData(string Tab)
        {
            try
            {
                TallyDBDetailsForView = await _tallySKUMappingViewModel.GetAllSKUMappingDetailsByDistCode(DistributorCode, Tab);
                TallyDBDetailsForViewClone = TallyDBDetailsForView;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private async void Product_OnPageChange(int pageNumber)
        {
            try
            {
                _tallySKUMappingViewModel.PageNumber = pageNumber;
                await GetGridData(await SetTabName());
                StateHasChanged();
            }
            catch (Exception ex)
            {


            }
        }
        #region Filter
        public async Task SearchDistributor(ChangeEventArgs e)
        {
            ShowLoader();
            try
            {
                var searchString = e.Value.ToString();
                if (!string.IsNullOrEmpty(searchString))
                {
                    TallySKUMappingFilterDetails = TallyDBDetailsForViewClone.Where(p => p.DistributorCode.ToLower().Contains(searchString.ToLower())).ToList();
                    TallyDBDetailsForView = TallySKUMappingFilterDetails;
                }
                else
                {
                    TallyDBDetailsForView = TallyDBDetailsForViewClone;
                }

            }
            catch (Exception ex)
            {
                TallyDBDetailsForView = TallyDBDetailsForViewClone;
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        public async Task FilterOpen()
        {
            try
            {
                IsFilterOpen = !IsFilterOpen;
                TallyDBDetailsForView = TallyDBDetailsForViewClone;
                SearchString = string.Empty;
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        #region GridView
        public void GridColumns()
        {
            ProductColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Distributor Code", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorCode) ? "N/A" : ((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorCode},
                new DataGridColumn { Header = "DistributorSKUName", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorSKUName) ? "N/A" : ((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorSKUName},
                new DataGridColumn { Header = "Principal SKUName", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping) s).PrincipalSKUName) ? "N/A" :((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping) s).PrincipalSKUName},
                new DataGridColumn { Header = "Principal SKUCode", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping) s).PrincipalSKUCode) ? "N/A" :((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping) s).PrincipalSKUCode},
                //new DataGridColumn
                //{
                //    Header = "Actions",
                //    IsButtonColumn = true,
                //    ButtonActions = new List<ButtonAction>
                //    {
                //        new ButtonAction
                //        {
                //            ButtonType = ButtonTypes.Image,
                //            URL = "Images/delete.png",
                //            Action = item => OnDeleteClick((ITallySKUMapping)item)
                //        }
                //    }
                //}
            };
            UnMappedProductColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Distributor Code", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorCode) ? "N/A" : ((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorCode},
                new DataGridColumn { Header = "DistributorSKUName", GetValue = s => string.IsNullOrEmpty(((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorSKUName) ? "N/A" : ((Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping)s).DistributorSKUName},
                //new DataGridColumn
                //{
                //    Header = "Actions",
                //    IsButtonColumn = true,
                //    ButtonActions = new List<ButtonAction>
                //    {
                //        new ButtonAction
                //        {
                //            ButtonType = ButtonTypes.Image,
                //            URL = "Images/delete.png",
                //            Action = item => OnDeleteClick((ITallySKUMapping)item)
                //        }
                //    }
                //}
            };
            PopUpColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Principle Sku Code", GetValue = s => ((Winit.Modules.Tally.Model.Classes.TallySKUMapping)s).PrincipalSKUCode == string.Empty ? "N/A" : ((Winit.Modules.Tally.Model.Classes.TallySKUMapping)s).PrincipalSKUCode },
                new DataGridColumn { Header = "Failure Reason", GetValue = s => ((Winit.Modules.Tally.Model.Classes.TallySKUMapping)s).Status == string.Empty ? "N/A" : ((Winit.Modules.Tally.Model.Classes.TallySKUMapping)s).Status },
            };
        }
        public async Task OnDeleteClick(ITallySKUMapping tallysku)
        {
            try
            {
                bool confirm = await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure? Do you want to Delete this Record?");
                if (confirm)
                {
                    ShowLoader();
                    StateHasChanged();
                    await _alertService.ShowSuccessAlert("Success", "Deleted Successfully");
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
        #endregion
        #region Uploading Part
        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
        }
        private bool IsExcelFile(IBrowserFile file)
        {
            // Check if the file extension is .xlsx or .xls
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            return fileExtension == ".xlsx" || fileExtension == ".xls";
        }
        private async Task UploadDetails()
        {
            ShowLoader();
            ExcelTallySKUMappingDetails.Clear();
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
                                        var principle = Activator.CreateInstance<Winit.Modules.Tally.Model.Classes.TallySKUMapping>();
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
                    await SaveToDataBase();
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please select Distributor");
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
        #endregion
        #region Saving in Database 
        public async Task SaveToDataBase()
        {
            try
            {
                ShowLoader();
                if (await Validations())
                {
                    bool IsInserted = await _tallySKUMappingViewModel.InsertTallySKUMapping(TallyDBDetails);
                    if (IsInserted)
                    {
                        _tost.Add("Success", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        // TallyDBDetailsForView = await GetTallySKUDataFromDB();
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Data Saving Failed...");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await GetGridData(await SetTabName());
                selectedFile = null;
                ExcelTallySKUMappingDetails.Clear();
                HideLoader();
                ShowHideMessage = true;
                StateHasChanged();
                await HideStatusOfUploadedRecords();
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
                                                   .GroupBy(p => new { p.PrincipalSKUCode })
                                                   .Select(g => g.First())
                                                   .ToList();
                //To remove duplicates from excel
                successDistributorDetails = distinctExcelTallySkuDetails.Where(p => SKUDetails.Any(q => q.Code == p.PrincipalSKUCode && !string.IsNullOrEmpty(p.DistributorSKUName))).ToList();
                failedOrInvalidDetails = ExcelTallySkuDetails.Where(p => string.IsNullOrEmpty(p.DistributorSKUName) || !SKUDetails.Any(q => q.Code == p.PrincipalSKUCode)).ToList();
                failedOrInvalidDetails.ForEach(p =>
                {
                    if (string.IsNullOrEmpty(p.DistributorSKUName))
                        p.Status = "Distributor Sku Name is Empty";
                    else
                        p.Status = "Invalid Principal Sku Code";
                });
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
        #endregion
        #region Export to Excel
        private async Task ExportToExcel()
        {
            try
            {
                ShowLoader();
                string fileName = "TallyMapping_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tally Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "DistributorCode";
                    worksheet.Cell(1, 2).Value = "DistributorSKUName";
                    worksheet.Cell(1, 3).Value = "PrincipalSKUCode";
                    worksheet.Cell(1, 4).Value = "PrincipalSKUName";

                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < TallyDBDetailsForView.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = DistributorCode;
                        worksheet.Cell(row, 2).Value = TallyDBDetailsForView[i].DistributorSKUName;
                        worksheet.Cell(row, 3).Value = TallyDBDetailsForView[i].PrincipalSKUCode;
                        worksheet.Cell(row, 4).Value = TallyDBDetailsForView[i].PrincipalSKUName;
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
        #endregion
        #region Sample File Download
        private async Task DownloadTemplate()
        {
            try
            {
                ShowLoader();
                string fileName = "TallyTemplate_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tally Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "DistributorSKUName";
                    worksheet.Cell(1, 2).Value = "PrincipalSKUCode";

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
        private async Task DownloadCMIItemList()
        {
            try
            {
                ShowLoader();
                string fileName = "CMIItemList_" + RandomSixDigitCode();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tally Mapping");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "PrincipalSKUCode";
                    worksheet.Cell(1, 2).Value = "PrincipalSKUName";
                    worksheet.Cell(1, 3).Value = "Organisation Unit";
                    worksheet.Cell(1, 4).Value = "Division";
                    worksheet.Cell(1, 5).Value = "Category";
                    worksheet.Cell(1, 6).Value = "Type";
                    worksheet.Cell(1, 7).Value = "Star Rating";
                    worksheet.Cell(1, 8).Value = "Series";


                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < SKUDetails.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = SKUDetails[i].Code ?? "N/A";
                        worksheet.Cell(row, 2).Value = SKUDetails[i].Name ?? "N/A";
                        worksheet.Cell(row, 3).Value = SKUDetails[i].L1 ?? "N/A";
                        worksheet.Cell(row, 4).Value = SKUDetails[i].L2 ?? "N/A";
                        worksheet.Cell(row, 5).Value = SKUDetails[i].L3 ?? "N/A";
                        worksheet.Cell(row, 6).Value = SKUDetails[i].L4 ?? "N/A";
                        worksheet.Cell(row, 7).Value = SKUDetails[i].L5 ?? "N/A";
                        worksheet.Cell(row, 8).Value = SKUDetails[i].L6 ?? "N/A";
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
        #endregion
        #region Validations
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
        #endregion
        public void ViewStatusPopUp()
        {
            try
            {
                IsViewStatus = !IsViewStatus;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task HideStatusOfUploadedRecords()
        {
            try
            {
                await Task.Delay(5000);
                ShowHideMessage = false;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }

        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                TabItems.ForEach(item => item.IsSelected = false);
                selectionItem.IsSelected = !selectionItem.IsSelected;
                SelectedTab = selectionItem.Label;
                await GetGridData(await SetTabName());
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
