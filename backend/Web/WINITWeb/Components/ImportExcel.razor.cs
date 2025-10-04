using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml;
using System.Data;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.UIComponents.Web.ImportExcel;

namespace WinIt.Components
{
    public partial class ImportExcel
    {
        [Parameter]
        public EventCallback<DataSet> OnImport { get; set; }


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
        public async Task Import()
        {

            await UploadExcelFileAndGetDataset();
            //await UploadExcelFileAndGetDataset1();

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
                await OnImport.InvokeAsync(result);
            }
        }

        DataTable dataTable = new DataTable();
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
                int c = dataTable.Rows.Count;


                //   await GetUploadedData.InvokeAsync(result);
            }
            _loadingService.HideLoading();
        }
    }
}
