using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using Practice;
using System.Data;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.ExcelUploadDetails
{
    public partial class Component1
    {
        private InputFile fileInput;
        private string reverse { get; set; } = "";
        private string PaymentMode { get; set; } = "";
        private bool showAlert { get; set; } = false;
        private bool showAlert1 { get; set; } = false;
        private bool showAlert2 { get; set; } = false;
        private List<string> reversals = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            await load();
            LoadResources(null, _languageService.SelectedCulture);
        }
       
        private async Task load()
        {
            throw new NotImplementedException();
        }

        private async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
            try
            {
                var createdResponses = "";
                const int V = 2;
                var rownum = V;
                var Failure = "";
                var retValue = "";
                var file = e.File;
                if (file != null)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the LicenseContext

                        // Copy the stream to a memory stream
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

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
                                    reverse = row.Field<string>("CashNumber");
                                    PaymentMode = row.Field<string>("PaymentMode");
                                    reversals.Add(reverse);
                                }
                                foreach (var ron in reversals)
                                {
                                    Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}CollectionModule/CreateReversalReceiptByReceiptNumber?ReceiptNumber=" + "" + "&TargetUID=" + PaymentMode + "&Amount=" + 0 + "&ChequeNo=" + ron + "&SessionUserCode=" + "1001" + "&ReasonforCancelation" + "", HttpMethod.Post, null);
                                    if (response.StatusCode == 201)
                                    {
                                        createdResponses = "Created";
                                    }
                                    else if (response.StatusCode == 500)
                                    {
                                        retValue = "InternalServerError";
                                    }
                                    else
                                    {
                                        Failure = "failed";
                                    }
                                }
                                if (Failure == "" && retValue == "")
                                {
                                    showAlert = true;
                                    StateHasChanged();
                                    await Task.Delay(3000); // Adjust the delay time (in milliseconds) as needed
                                    showAlert = false;
                                    StateHasChanged();
                                }
                                else if (retValue != "" && Failure == "")
                                {
                                    showAlert1 = true;
                                    StateHasChanged();
                                    await Task.Delay(3000); // Adjust the delay time (in milliseconds) as needed
                                    showAlert1 = false;
                                    StateHasChanged();
                                }
                                else
                                {
                                    showAlert2 = true;
                                    StateHasChanged();
                                    await Task.Delay(3000); // Adjust the delay time (in milliseconds) as needed
                                    showAlert2 = false;
                                    StateHasChanged();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
