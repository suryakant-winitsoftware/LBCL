using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Dynamic;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Shared.CommonUtilities.Extensions;
using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using System.Data;
using Newtonsoft.Json;

namespace Winit.UIComponents.Web.ImportExcel
{
    public partial class ImportExcelSheet<T>
    {
        [Parameter]
        public EventCallback<dynamic> GetUploadedData { get; set; }
       
        [Parameter]
        public RequiredDataType RequiredDataType { get; set; }=RequiredDataType.DataSet;
        private InputFile fileInput { get; set; }
        private Guid inputFileId { get; set; }
       
        IBrowserFile file { get; set; }
        protected async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
            file = e.File;
            if (file != null)
            {
                const long maxFileSize = 1000 * 1024 * 1024; // 10 MB
                using var stream = file.OpenReadStream(maxFileSize);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
            }
        }
        private bool IsExcelFile(IBrowserFile file)
        {
            // Check if the file extension is .xlsx or .xls
            var fileExtension = Path.GetExtension(file.Name).ToLower();
            return fileExtension == ".xlsx" || fileExtension == ".xls";
        }
        public async Task ImportExcel()
        {
            if (RequiredDataType == RequiredDataType.DataSet)
            {
                await UploadExcelFileAndGetDataset();
                //await UploadExcelFileAndGetDataset1();
            }
            else
            {
                await UploadExcelFile<T>();
            }
        }
        public async Task UploadExcelFile<T>()
        {
            List<T> result = new List<T>();

            try
            {
                _loadingService.ShowLoading();
                // showDiv = false;

                //var file = e.File;
                if (file != null && IsExcelFile(file))
                {
                    using (var stream = file.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            //if (ExcelFileContainsData(memoryStream))
                            //{
                            using (var package = new ExcelPackage(memoryStream))
                            {
                                var worksheet = package.Workbook.Worksheets[0];

                                // Read header row separately
                                var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                                var headers = new List<string>();

                                foreach (var headerCell in headerRow)
                                {
                                    headers.Add(headerCell.Text);
                                }

                                var properties = typeof(T).GetProperties();

                                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                {
                                    T row = _serviceProvider.CreateInstance<T>();
                                    for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                                    {
                                        var cellValue = worksheet.Cells[rowNumber, columnNumber].Text;
                                        var property = properties.FirstOrDefault(p => p.Name.Equals(headers[columnNumber - 1], StringComparison.OrdinalIgnoreCase));

                                        if (property != null)
                                        {
                                            if (property.PropertyType == typeof(int) && int.TryParse(cellValue, out var intValue))
                                            {
                                                property.SetValue(row, intValue);
                                            }
                                            else if (property.PropertyType == typeof(int?) && int.TryParse(cellValue, out var nullableIntValue))
                                            {
                                                property.SetValue(row, (int?)nullableIntValue);
                                            }
                                            else if (property.PropertyType == typeof(double) && double.TryParse(cellValue, out var doubleValue))
                                            {
                                                property.SetValue(row, doubleValue);
                                            }
                                            else if (property.PropertyType == typeof(double?) && double.TryParse(cellValue, out var nullableDoubleValue))
                                            {
                                                property.SetValue(row, (double?)nullableDoubleValue);
                                            }
                                            else if (property.PropertyType == typeof(DateTime) && DateTime.TryParse(cellValue, out var dateTimeValue))
                                            {
                                                property.SetValue(row, dateTimeValue);
                                            }
                                            else if (property.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellValue, out var nullableDateTimeValue))
                                            {
                                                property.SetValue(row, (DateTime?)nullableDateTimeValue);
                                            }
                                            else if (property.PropertyType == typeof(string))
                                            {
                                                property.SetValue(row, cellValue);
                                            }

                                            // Add more type checks if needed
                                        }
                                    }

                                    result.Add(row);
                                }
                               
                            }
                            //}
                            //else
                            //{
                            //    _loadingService.HideLoading();
                            //  //  await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_all_fields_in_excel"]);
                            //}
                        }
                    }
                }
                else
                {
                    inputFileId = Guid.NewGuid();
                    //StateHasChanged();
                    await _alertService.ShowErrorAlert(file==null?"Error":"InvalidFile", file == null ? "Please Upload excel file" : "Only Excel Files are uploadable !");
                }
            }
            catch (Exception ex)
            {
                // _loadingService.HideLoading();
                // isLoading = false;
                //  await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["excel_error"]);
            }
            finally
            {
                await GetUploadedData.InvokeAsync(result);
            }


            _loadingService.HideLoading();
           
        }
        public async Task UploadExcelFileAndGetDataset()
        {
            DataSet result = new DataSet();

            try
            {
                _loadingService.ShowLoading();

                if (file == null)
                {
                    inputFileId = Guid.NewGuid();
                    await _alertService.ShowErrorAlert("Error", "Please upload an Excel file");
                    return;
                }

                if (!IsExcelFile(file))
                {
                    inputFileId = Guid.NewGuid();
                    await _alertService.ShowErrorAlert("InvalidFile", "Only Excel files are uploadable!");
                    return;
                }

                using (var stream = file.OpenReadStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        using (var package = new ExcelPackage(memoryStream))
                        {
                            foreach (var worksheet in package.Workbook.Worksheets)
                            {
                                DataTable dataTable = new DataTable(worksheet.Name);

                                // Read header row separately
                                var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                                var headers = new List<string>();

                                foreach (var headerCell in headerRow)
                                {
                                    headers.Add(headerCell.Text);
                                    dataTable.Columns.Add(headerCell.Text);
                                }

                                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                {
                                    DataRow dataRow = dataTable.NewRow();

                                    for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                                    {
                                        var cellValue = worksheet.Cells[rowNumber, columnNumber].Text;
                                        dataRow[headers[columnNumber - 1]] = cellValue;
                                    }

                                    dataTable.Rows.Add(dataRow);
                                }
                                result.Tables.Add(dataTable);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                Console.WriteLine($"Error: {ex.Message}");
                await _alertService.ShowErrorAlert("Error", "An error occurred while processing the Excel file.");
            }
            finally
            {
                _loadingService.HideLoading();
                await GetUploadedData.InvokeAsync(result);
            }
        }

        DataTable dataTable =new DataTable();
        public async Task UploadExcelFileAndGetDataset1()
        {
            DataSet result = new DataSet();

            try
            {
                _loadingService.ShowLoading();

                if (file != null && IsExcelFile(file))
                {
                    using (var stream = file.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            using (var package = new ExcelPackage(memoryStream))
                            {
                                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                                {
                                    var worksheet = package.Workbook.Worksheets[i];
                                    DataTable dataTable = new DataTable(worksheet.Name);

                                    // Read header row separately
                                    var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                                    var headers = new List<string>();

                                    for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                                    {
                                        var headerCell = headerRow[1, j]; // EPPlus uses 1-based indexing
                                        headers.Add(headerCell.Text);
                                        dataTable.Columns.Add(headerCell.Text);
                                    }

                                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                    {
                                        DataRow dataRow = dataTable.NewRow();

                                        for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                                        {
                                            var cellValue = worksheet.Cells[rowNumber, columnNumber].Text;
                                            dataRow[headers[columnNumber - 1]] = cellValue;
                                        }

                                        dataTable.Rows.Add(dataRow);
                                    }
                                    this.dataTable = dataTable;
                                    result.Tables.Add(dataTable);
                                }
                            }
                        }
                    }
                }
                else
                {
                    inputFileId = Guid.NewGuid();
                    await _alertService.ShowErrorAlert(file == null ? "Error" : "InvalidFile", file == null ? "Please Upload excel file" : "Only Excel Files are uploadable!");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception and show an error alert
            }
            finally
            {
               int c= dataTable.Rows.Count;

                
             //   await GetUploadedData.InvokeAsync(result);
            }
            _loadingService.HideLoading();
        }
    }
}
