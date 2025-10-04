using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System;
using System.IO;
using iTextSharp.text;
using iText.Kernel.Pdf;
using iText.Html2pdf;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class BalanceConfirmations : ComponentBase
    {
        private List<DataGridColumn> StoreStatementColumns { get; set; } = new List<DataGridColumn>();
        public bool IsInitialised { get; set; } = false;
        public bool ViewImageAndSignature { get; set; } = false;
        public bool IsView { get; set; } = false;
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                IsView = Convert.ToBoolean(GetParameterValueFromURL("IsView"));
                if (!IsView)
                {
                    SetHeaderName();
                }
                else
                {
                    SetHeaderNameView();
                }
                await _balanceConfirmationViewmodel.PopulateViewModel();
                GridColumns();
                IsInitialised = true;
            }
            catch (Exception)
            {
                //using var stream = await Http.GetStreamAsync($"{NavigationManager.BaseUri}BalanceConfirmationTemplate.html");
                //using var reader = new StreamReader(stream);
                //htmlContent = await reader.ReadToEndAsync();
                throw;
            }
            finally
            {
                _loadingService.HideLoading();
                StateHasChanged();
            }
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public void GridColumns()
        {
            StoreStatementColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Created Date", GetValue = s => ((StoreStatement)s).CreatedTime,  IsSortable = true, SortField = "CreatedTime"  },
                new DataGridColumn { Header = "Reference Number", GetValue = s => ((StoreStatement)s).ReferenceNumber ,  IsSortable = true, SortField = "ReferenceNumber"  },
                new DataGridColumn { Header = "Order Type", GetValue = s => ((StoreStatement)s).OrderType,  IsSortable = true, SortField = "OrderType" },
                new DataGridColumn { Header = "Transaction Type", GetValue = s => ((StoreStatement)s).TransactionType,  IsSortable = true, SortField = "TransactionType" },
                new DataGridColumn { Header = "Opening Balance(₹)", GetValue = s => CommonFunctions.RoundForSystem(((StoreStatement)s).OpeningBalance),  IsSortable = true, SortField = "OpeningBalance"},
                new DataGridColumn { Header = "Amount(₹)", GetValue = s => ((StoreStatement)s).Amount , IsSortable = true, SortField = "Amount" },
                new DataGridColumn { Header = "Closing Balance(₹)", GetValue = s => ((StoreStatement)s).ClosingBalance,  IsSortable = true, SortField = "ClosingBalance"},
            };
        }
        private async void Product_OnSort(SortCriteria sortCriteria)
        {
            await _balanceConfirmationViewmodel.OnSortApply(sortCriteria);
            StateHasChanged();
        }
        public string DateFormat(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("dd MMM yyyy, hh:mm tt");
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private async Task ExportToExcel(List<StoreStatement> ElementsList)
        {
            try
            {
                string fileName = "CollectionDetails" + Guid.NewGuid().ToString();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Collection Details");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Created Date";
                    worksheet.Cell(1, 2).Value = "Reference Number";
                    worksheet.Cell(1, 3).Value = "Order Type";
                    worksheet.Cell(1, 4).Value = "Transaction Type";
                    worksheet.Cell(1, 5).Value = "Opening Balance(₹)";//hi bro
                    worksheet.Cell(1, 6).Value = "Amount(₹)";
                    worksheet.Cell(1, 7).Value = "Closing Balance(₹)";

                    // Populate the Excel worksheet with your data from elem
                    for (int i = 0; i < ElementsList.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cell(row, 1).Value = ElementsList[i].CreatedTime;
                        worksheet.Cell(row, 2).Value = ElementsList[i].ReferenceNumber;
                        worksheet.Cell(row, 3).Value = ElementsList[i].OrderType;
                        worksheet.Cell(row, 4).Value = ElementsList[i].TransactionType;
                        worksheet.Cell(row, 5).Value = ElementsList[i].OpeningBalance;
                        worksheet.Cell(row, 6).Value = ElementsList[i].Amount;
                        worksheet.Cell(row, 7).Value = ElementsList[i].ClosingBalance;
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
        }

        public async Task RedirectToConfirmation()
        {
            try
            {
                _navigationManager.NavigateTo("confirmbalance?Amount=" + _balanceConfirmationViewmodel.BalanceConfirmationDetails.ClosingBalance + "&GeneratedOn=" + _balanceConfirmationViewmodel.BalanceConfirmationDetails.GeneratedOn);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task RedirectToViewHistory()
        {
            try
            {
                _navigationManager.NavigateTo("maintainbalanceconfirmation");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task ViewDetails()
        {
            try
            {
                ViewImageAndSignature = true;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public void Close()
        {
            try
            {
                ViewImageAndSignature = false;
            }
            catch (Exception ex)
            {

            }
        }
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Primary Channel Partner Balance Confirmation",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Primary Channel Partner Balance Confirmation", IsClickable = false },
                }
            };
        }
        protected void SetHeaderNameView()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Primary Channel Partner Balance Confirmation",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Maintain Balance Confirmation", IsClickable = true, URL="maintainbalanceconfirmation"},
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Primary Channel Partner Balance Confirmation", IsClickable = false },
                }
            };
        }

        public async Task DownloadPDF()
        {
            try
            {
                string htmlContent = await Http.GetStringAsync($"{NavigationManager.BaseUri}BalanceConfirmationTemplate.html");
                byte[] pdfBytes = await GeneratePdfAsync(htmlContent);

                var base64String = Convert.ToBase64String(pdfBytes);
                var fileName = "document.pdf";
                await JSRuntime.InvokeVoidAsync("downloadPdf", base64String, fileName);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
        public async Task<byte[]> GeneratePdfAsync(string htmlContent)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(memoryStream))
                {
                    using (var pdfDocument = new PdfDocument(pdfWriter))
                    {
                        ConverterProperties properties = new ConverterProperties();
                        HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, properties);
                    }
                }
                return memoryStream.ToArray();
            }
        }
        public string FormatNumberInIndianStyle(decimal number, string symbol = "₹ ")
        {
            // Create a new NumberFormatInfo object
            NumberFormatInfo nfi = new NumberFormatInfo();

            // Define the grouping for the Indian numbering system
            nfi.NumberGroupSizes = new[] { 3, 2 }; // 3 for the first group, 2 for subsequent groups
            nfi.NumberGroupSeparator = ","; // Use comma as the separator
            nfi.CurrencyGroupSizes = new[] { 3, 2 }; // Same grouping for currency
            nfi.CurrencyGroupSeparator = ","; // Use comma as the separator for currency
            nfi.CurrencySymbol = symbol; // Set the rupee symbol

            // Format the number using the custom NumberFormatInfo
            // "{0:C}" formats as currency
            return number.ToString("C2", nfi); // "C2" specifies currency format with 2 decimal places
        }
    }
}
