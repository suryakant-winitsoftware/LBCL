using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using WinIt.Pages.Base;

namespace WinIt.Pages.Collection.Statement
{
    public partial class StatementReport : BaseComponentBase
    {
        public string StoreUID = "AD0DE4AC-332E-47E0-9117-5A11DA46AC87";
        //Document _document;
        //PdfPTable _pdfTable = new PdfPTable(4);
        //PdfPCell _pdfCell;
        //iTextSharp.text.Font _fontStyle;
        MemoryStream _memoryStream = new MemoryStream();
        List<AccStoreLedger> oStatement { get; set; } = new List<AccStoreLedger>();
        List<AccStoreLedger> oStatementStatic { get; set; } = new List<AccStoreLedger>();
        List<AccStoreLedger> oStatementDisplay { get; set; } = new List<AccStoreLedger>();
        List<AccStoreLedger> oStatementCopy { get; set; } = new List<AccStoreLedger>();
        List<AccPayable> oStatementPayOpen { get; set; } = new List<AccPayable>();
        List<AccPayable> oStatementPay { get; set; } = new List<AccPayable>();
        List<AccPayable> oStatementPayCopy { get; set; } = new List<AccPayable>();
        List<AccPayable> oStatementPayDisplay { get; set; } = new List<AccPayable>();
        private Winit.Modules.Store.Model.Classes.Store[] _UsersList { get; set; } = new Winit.Modules.Store.Model.Classes.Store[0];

        private string CustomerCode { get; set; } = "DIST001";
        private string Customer { get; set; } = "";
        private string FromDate { get; set; } = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
        private string ToDate { get; set; } = "";
        private List<DataGridColumn> StatementColumns { get; set; } = new List<DataGridColumn>();
        private DateTime From { get; set; } = DateTime.Now;
        private string FromCompare { get; set; }
        private DateTime To { get; set; } = DateTime.Now;
        private decimal OpeningBalance { get; set; } = 0;
        private decimal ClosingBalance { get; set; } = 0;
        public string SelectedValue { get; set; } = "";
        public bool ShowCustomers { get; set; } = false;
        public bool ShowFields { get; set; } = false;
        private string selectedValueText { get; set; } = "-- Select Customer --";
        List<ISelectionItem> customerData { get; set; } = new List<ISelectionItem>();
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool isReceiptAscending = true;
        public bool isReceiptArrowShow = false;
        public bool isDateAscending = true;
        public bool isDateArrowShow = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LoadResources(null, _languageService.SelectedCulture);
                await SetHeaderName();
                //2await _accountStatementViewModel.StatementReportCustomers(CustomerCode);
                //2_UsersList = _accountStatementViewModel._UsersList;
                //2foreach (var item in _UsersList)
                //2{
                //    SelectionItem type = new SelectionItem()
                //    {
                //        Code = item.Code,
                //        UID = item.UID,
                //        Label = item.Name,
                //    };
                //    customerData.Add(type);
                //2}
                await GetCustomer(new ChangeEventArgs());
                //2SetOpeningBalance();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["statement"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["statement"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        public void SetOpeningBalance()
        {
            try
            {
                FromCompare = From.AddMonths(-3).ToString("yyyy-MM-dd");
                oStatementCopy = oStatementDisplay.Where(item => item.TransactionDateTime?.Date <= Convert.ToDateTime(FromCompare).Date).ToList();
                oStatementPayOpen = oStatementPayDisplay.Where(item => item.TransactionDate?.Date <= Convert.ToDateTime(FromCompare).Date).ToList();
                //OpeningBalance = oStatementCopy.LastOrDefault().Balance;
                SetData();
                oStatementPay = oStatementPayDisplay.Where(item => item.TransactionDate?.Date <= Convert.ToDateTime(FromCompare).Date).ToList();
                AllotOpeningAmount();
                //OpeningBalance = oStatementPayOpen.Sum(p => p.Amount) - oStatementCopy.Where(p => p.CreditType.Contains("DR")).Sum(p => p.Amount) 
                //    + (oStatementCopy.Any(p => p.CreditType.Contains("CR") && (p.UID.Contains("R") || p.DocumentNumber.Contains("OA"))) ? (oStatementCopy.Where(p => p.CreditType.Contains("CR")).Sum(p => p.Amount)) 
                //    : oStatementCopy.Where(p => p.CreditType.Contains("CR")).Sum(p => p.Amount));
                SetClosingBalance();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                OpeningBalance = 0;
                StateHasChanged();
            }
        }

        public decimal CalculateAmount()
        {
            try
            {
                foreach (var item in oStatementCopy)
                {

                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task GetCustomer(ChangeEventArgs e)
        {
            try
            {
                SelectedValue = /*e.Value.ToString()*/_iAppUser.Emp.Code;
                //FromDate = From.ToString("yyyy-MM-dd");
                ToDate = To.ToString("yyyy-MM-dd");
                Customer = /*e.Value.ToString()*/_iAppUser.Emp.Code;
                //api call
                await _accountStatementViewModel.ViewAccountStatement(FromDate, ToDate, Customer);
                oStatementDisplay = _accountStatementViewModel.oStatementDisplay;
                oStatementPayDisplay = _accountStatementViewModel.oStatementDisplayPayable;
                oStatementPay = oStatementPayDisplay;
                oStatement = oStatementDisplay;
                oStatementStatic = oStatementDisplay;

                //await CommonMethod();
                SetOpeningBalance();
                StateHasChanged();
                isReceiptAscending = true;
                isDateAscending = true;
                isReceiptArrowShow = false;
                isDateArrowShow = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                oStatement = new List<AccStoreLedger>();
                StateHasChanged();
            }
        }

        public async void SetData()
        {
            try
            {
                oStatement = oStatementDisplay.Where(item => item.TransactionDateTime?.Date >= Convert.ToDateTime(FromDate).Date && item.TransactionDateTime?.Date <= Convert.ToDateTime(ToDate).Date).ToList();
                oStatementPay = oStatementPayDisplay.Where(item => item.TransactionDate?.Date >= Convert.ToDateTime(FromDate).Date && item.TransactionDate?.Date <= Convert.ToDateTime(ToDate).Date).ToList();

            }
            catch (Exception ex)
            {

            }
        }
        public async void AllotOpeningAmount()
        {
            try
            {
                decimal TotalAmount = oStatementPayOpen.Sum(p => p.Amount);
                decimal CRAmount = oStatementCopy.Where(p => p.CreditType.Contains("CR") && !p.DocumentNumber.Contains("R - OA")).Sum(p => p.Amount);
                decimal ReversedAmount = oStatementCopy.Where(p => p.DocumentNumber.Contains("R - OA")).Sum(p => p.Amount);
                decimal DRAmount = oStatementCopy.Where(p => p.CreditType.Contains("DR")).Sum(p => p.Amount);
                OpeningBalance = TotalAmount - DRAmount + (CRAmount * -1) + ReversedAmount;
            }
            catch (Exception ex)
            {

            }
        }
        public async void AllotClosingAmount()
        {
            try
            {
                decimal TotalAmount = oStatementPay.Sum(p => p.Amount);
                decimal CRAmount = oStatement.Where(p => p.CreditType.Contains("CR") && !p.DocumentNumber.Contains("R - OA")).Sum(p => p.Amount);
                decimal ReversedAmount = oStatement.Where(p => p.DocumentNumber.Contains("R - OA")).Sum(p => p.Amount);
                decimal DRAmount = oStatement.Where(p => p.CreditType.Contains("DR")).Sum(p => p.Amount);
                ClosingBalance = OpeningBalance + TotalAmount - DRAmount + (CRAmount * -1) + ReversedAmount;
            }
            catch (Exception ex)
            {

            }
        }

        public void SetClosingBalance()
        {
            try
            {
                AllotClosingAmount();
                //ClosingBalance = OpeningBalance + oStatementPay.Sum(p => p.Amount) -
                //    oStatement.Where(p => p.CreditType.Contains("DR")).Sum(p => p.Amount) +
                //    (oStatement.Any(p => p.CreditType.Contains("CR") && (p.UID.Contains("R") || p.DocumentNumber.Contains("OA"))) ? (oStatement.Where(p => p.CreditType.Contains("CR")).Sum(p => p.Amount))
                //    : oStatement.Where(p => p.CreditType.Contains("CR")).Sum(p => p.Amount));
            }
            catch (Exception ex)
            {

            }
        }

        public async void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedValue))
                {
                    FromDate = From.ToString("yyyy-MM-dd");
                    ToDate = To.ToString("yyyy-MM-dd");
                    if (await CheckDate())
                    {
                        oStatementPay = oStatementPayDisplay.Where(item => item.TransactionDate?.Date >= Convert.ToDateTime(FromDate).Date && item.TransactionDate?.Date <= Convert.ToDateTime(ToDate).Date).ToList();
                        //SetOpeningBalance();
                        StateHasChanged();
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["from-date_cannot_be_greater_than_to-date"]);
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["warning"], @Localizer["please_select_customer"]);
                }
                isReceiptAscending = true;
                isDateAscending = true;
                isReceiptArrowShow = false;
                isDateArrowShow = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                oStatement = new List<AccStoreLedger>();
                ClosingBalance = 0;
                StateHasChanged();
            }
        }
        public async Task<bool> CheckDate()
        {
            if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
            {
                return false;
            }
            return true;
        }

        public decimal SetAmount(AccStoreLedger item)
        {
            try
            {
                if (item.CreditType.Contains("DR"))
                {
                    return (item.Amount == 0 ? 0 : item.Amount * -1);
                }
                else
                {
                    if (item.DocumentNumber.Contains("R - OA"))
                    {
                        return item.Amount;
                    }
                    if (item.UID.Contains("R") || item.DocumentNumber.Contains("OA"))
                    {
                        return item.Amount * -1;
                    }
                    else
                    {
                        return item.Amount;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public decimal SetAmountPay(AccPayable item)
        {
            try
            {
                if (item.SourceType.Contains("INVOICE"))
                {
                    return (item.Amount == 0 ? 0 : item.Amount * -1);
                }
                else
                {
                    return item.Amount;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        bool IsNegative(decimal amount)
        {
            string Amount = amount.ToString();
            return Amount.Contains("-");
        }
        private async void OnSelected(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    var args = new ChangeEventArgs();
                    var values = dropDownEvent.SelectionItems.First().UID;
                    selectedValueText = dropDownEvent.SelectionItems.First().Label;
                    args.Value = values;
                    await GetCustomer(args);
                    ShowCustomers = false;
                    StateHasChanged();
                }
            }
            else
            {
                selectedValueText = "-- Select Customer --";
                ShowCustomers = false;
            }
        }
        //private string currentSortField;
        //private bool isAscending = true;
        //private void SortColumn(DataGridColumn column)
        //{
        //    if (column.IsSortable)
        //    {
        //        if (currentSortField == column.SortField)
        //        {
        //            isAscending = !isAscending;
        //        }
        //        else
        //        {
        //            currentSortField = column.SortField;
        //            isAscending = true;
        //        }
        //        SortCriteria sortCriteria = new SortCriteria(currentSortField, isAscending ? SortDirection.Asc : SortDirection.Desc);
        //        Product_OnSort(sortCriteria);
        //    }
        //}
        //private async void Product_OnSort(SortCriteria sortCriteria)
        //{
        //    ISortHelper sortHelper = new SortHelper();
        //    oStatement = await sortHelper.Sort(oStatement, sortCriteria);
        //}
        public async Task Sorting(string ColumnName)
        {
            try
            {
                switch (ColumnName)
                {
                    case "ReceiptNumber":
                        await SortReceiptWise(ColumnName);
                        break;
                    case "TransactionDate":
                        await SortDateWise(ColumnName);
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SortReceiptWise(string ColumnName)
        {
            try
            {
                isReceiptArrowShow = true;
                if (isReceiptAscending)
                {
                    oStatementPay = oStatementPay.OrderBy(p => p.ReferenceNumber).ToList();
                }
                else
                {
                    oStatementPay = oStatementPay.OrderByDescending(p => p.ReferenceNumber).ToList();
                }
                isReceiptAscending = !isReceiptAscending;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SortDateWise(string ColumnName)
        {
            try
            {
                isDateArrowShow = true;
                if (isDateAscending)
                {
                    oStatementPay = oStatementPay.OrderBy(p => p.TransactionDate).ToList();
                }
                else
                {
                    oStatementPay = oStatementPay.OrderByDescending(p => p.TransactionDate).ToList();
                }
                isDateAscending = !isDateAscending;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private async void GeneratePDFFile()
        {
            //try
            //{
            //    if (!string.IsNullOrEmpty(SelectedValue))
            //    {
            //        StateHasChanged();
            //        await Js.InvokeAsync<IAccStoreLedger>("saveAsFile", "AccountStatement.pdf", Convert.ToBase64String(Report(oStatement))

            //         );
            //    }
            //    else
            //    {
            //        await _alertService.ShowErrorAlert(@Localizer["warning"], @Localizer["please_select_customer"]);
            //    }
            //}
            //catch (Exception ex)
            //{

            //}

        }

        //public byte[] Report(List<AccStoreLedger> oStatements)
        //{
        //    try
        //    {
        //        _pdfTable = new PdfPTable(4);
        //        oStatement = oStatements;
        //        _document = new Document(PageSize.A4, 10f, 10f, 20f, 30f);
        //        _pdfTable.WidthPercentage = 100;
        //        _pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
        //        _fontStyle = FontFactory.GetFont("Tahoma", 1);
        //        PdfWriter.GetInstance(_document, _memoryStream);
        //        _document.Open();
        //        float[] sizes = new float[4];

        //        for (var i = 0; i < 4; i++)
        //        {

        //            if (i == 0) sizes[i] = 50;
        //            else sizes[i] = 100;
        //        }
        //        _pdfTable.SetWidths(sizes);
        //        this.ReportHeader();

        //        this.SpaceBetween();

        //        this.ReportBody();
        //        _pdfTable.HeaderRows = 2;
        //        _document.Add(_pdfTable);
        //        _document.Close();
        //        return _memoryStream.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        //private void ReportHeader()
        //{
        //    _fontStyle = FontFactory.GetFont("Tahoma", 10f, 1);
        //    _pdfCell = new PdfPCell(new Phrase("Account Statement", _fontStyle));
        //    _pdfCell.Colspan = 4;
        //    _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    _pdfCell.Border = 0;
        //    _pdfCell.ExtraParagraphSpace = 0;
        //    _pdfTable.AddCell(_pdfCell);
        //    _pdfTable.CompleteRow();

        //}
        //private void ReportBody()
        //{
        //    try
        //    {
        //        this.LabelsPdf();

        //        this.SpaceBetween();

        //        this.TablePdf();
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}
        //public void SpaceBetween()
        //{
        //    _pdfCell = new PdfPCell(new Phrase(" ")); // Empty cell content
        //    _pdfCell.Colspan = 4;
        //    _pdfCell.MinimumHeight = 20f; // Adjust this value to control spacing
        //    _pdfCell.Border = 0; // Set all borders to invisible
        //    _pdfCell.ExtraParagraphSpace = 0;
        //    _pdfTable.AddCell(_pdfCell);
        //    _pdfTable.CompleteRow();
        //}


        //public void LabelsPdf()
        //{
        //    try
        //    {
        //        var labelFontStyle = FontFactory.GetFont("Tahoma", 9f, 1);
        //        var valueFontStyle = FontFactory.GetFont("Tahoma", 9f, 0);

        //        // Create table for labels
        //        PdfPTable labelTable = new PdfPTable(1);

        //        // Create table for values
        //        PdfPTable valueTable = new PdfPTable(1);

        //        PdfPTable valueTable2 = new PdfPTable(1);

        //        PdfPTable valueTable3 = new PdfPTable(1);

        //        // Define labels and values
        //        string[] labels = { "Customer Name:", "Customer Code:", "Currency:", "FromDate:", "ToDate:", "OpeningBalance:", "TotalBalance:" };
        //        string[] labels2 = { "OpeningBalance:", "ClosingBalance:" };
        //        string[] values = { oStatement.First().Name, oStatement.First().Code, oStatement.First().CurrencyUID, FromDate, ToDate, Convert.ToString(OpeningBalance), Convert.ToString(ClosingBalance) }; // You can replace these with dynamic values from your data
        //        string[] values2 = { Convert.ToString(OpeningBalance) + " (" + oStatement.First().CurrencyUID + ")", Convert.ToString(ClosingBalance) + " (" + oStatement.First().CurrencyUID + ")" }; // You can replace these with dynamic values from your data

        //        // Add labels and values to respective tables
        //        for (int i = 0; i < labels.Length; i++)
        //        {
        //            // Concatenate label and value into a single string
        //            Phrase labelValuePhrase = new Phrase();

        //            // Add label with bold font style
        //            Chunk labelChunk = new Chunk(labels[i], FontFactory.GetFont("Tahoma", 9f, Font.NORMAL));
        //            labelValuePhrase.Add(labelChunk);

        //            // Add value with normal font style
        //            Chunk valueChunk = new Chunk(" " + values[i], FontFactory.GetFont("Tahoma", 9f, Font.BOLD));
        //            labelValuePhrase.Add(valueChunk);

        //            // Create cell and add the phrase
        //            PdfPCell labelValueCell = new PdfPCell(labelValuePhrase);
        //            labelValueCell.Border = 0; // Remove border
        //            labelTable.AddCell(labelValueCell);
        //        }
        //        for (int i = 0; i < labels2.Length; i++)
        //        {
        //            Phrase labelValuePhrase = new Phrase();

        //            // Add label with bold font style
        //            Chunk labelChunk = new Chunk(labels2[i], FontFactory.GetFont("Tahoma", 9f, Font.NORMAL));
        //            labelValuePhrase.Add(labelChunk);

        //            // Add value with normal font style
        //            Chunk valueChunk = new Chunk(" " + values2[i], FontFactory.GetFont("Tahoma", 9f, Font.BOLD));
        //            labelValuePhrase.Add(valueChunk);

        //            // Create cell and add the phrase
        //            PdfPCell labelValueCell = new PdfPCell(labelValuePhrase);
        //            labelValueCell.Border = 0; // Remove border
        //            valueTable2.AddCell(labelValueCell);
        //        }

        //        // Add labelTable and valueTable to the main pdfTable
        //        PdfPCell labelCellWrapper = new PdfPCell(labelTable);
        //        labelCellWrapper.Border = 0; // Remove border
        //        _pdfTable.AddCell(labelCellWrapper);

        //        PdfPCell valueCellWrapper = new PdfPCell(valueTable);
        //        valueCellWrapper.Border = 0; // Remove border
        //                                     //valueCellWrapper.Colspan = 4;
        //        _pdfTable.AddCell(valueCellWrapper);

        //        PdfPCell valueCellWrapper2 = new PdfPCell(valueTable2);
        //        valueCellWrapper2.Border = 0;
        //        _pdfTable.AddCell(valueCellWrapper2);

        //        PdfPCell valueCellWrapper3 = new PdfPCell(valueTable3);
        //        valueCellWrapper3.Border = 0;
        //        _pdfTable.AddCell(valueCellWrapper3);

        //        _pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

        //        _pdfTable.CompleteRow();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //public void TablePdf()
        //{
        //    try
        //    {
        //        _fontStyle = FontFactory.GetFont("Tahoma", 9f, 1);

        //        var fontStyle = FontFactory.GetFont("Tahoma", 9f, 0);

        //        #region Table Header
        //        float[] columnWidths = new float[] { 100f, 100f, 100f, 100f }; // Equal width for each column

        //        _pdfTable.SetWidths(columnWidths); // Set the widths for the columns


        //        _pdfCell = new PdfPCell(new Phrase("Date", _fontStyle));
        //        _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        _pdfCell.BackgroundColor = BaseColor.White;
        //        _pdfTable.AddCell(_pdfCell);

        //        _pdfCell = new PdfPCell(new Phrase("Amount", _fontStyle));
        //        _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        _pdfCell.BackgroundColor = BaseColor.White;
        //        _pdfTable.AddCell(_pdfCell);

        //        _pdfCell = new PdfPCell(new Phrase("CreditType", _fontStyle));
        //        _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        _pdfCell.BackgroundColor = BaseColor.White;
        //        _pdfTable.AddCell(_pdfCell);

        //        _pdfCell = new PdfPCell(new Phrase("Comments", _fontStyle));
        //        _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        _pdfCell.BackgroundColor = BaseColor.White;
        //        _pdfTable.AddCell(_pdfCell);

        //        _pdfTable.CompleteRow();

        //        int nSL = 1;
        //        foreach (var ostatement in oStatement)
        //        {
        //            _pdfCell = new PdfPCell(new Phrase(ostatement.TransactionDateTime.ToString(), fontStyle));
        //            _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //            _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            _pdfCell.BackgroundColor = BaseColor.White;
        //            _pdfTable.AddCell(_pdfCell);

        //            _pdfCell = new PdfPCell(new Phrase(ostatement.CreditType.Contains("DR") ? "-" + ostatement.Amount.ToString() : ostatement.Amount.ToString(), fontStyle));
        //            _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //            _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            _pdfCell.BackgroundColor = BaseColor.White;
        //            _pdfTable.AddCell(_pdfCell);

        //            _pdfCell = new PdfPCell(new Phrase(ostatement.CreditType.ToString(), fontStyle));
        //            _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //            _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            _pdfCell.BackgroundColor = BaseColor.White;
        //            _pdfTable.AddCell(_pdfCell);

        //            _pdfCell = new PdfPCell(new Phrase(ostatement.Comments.ToString(), fontStyle));
        //            _pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //            _pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //            _pdfCell.BackgroundColor = BaseColor.White;
        //            _pdfTable.AddCell(_pdfCell);

        //            _pdfTable.CompleteRow();
        //            #endregion

        //        }
        //        SpaceBetween();

        //        string generationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
        //        string boldText = "PDF-Generated-Time: "; // Assuming HTML-like formatting is supported for bold
        //        string normalText = generationTime;
        //        Phrase combinedPhrase = new Phrase();
        //        combinedPhrase.Add(new Chunk(boldText, FontFactory.GetFont("Tahoma", 9f, Font.NORMAL)));
        //        combinedPhrase.Add(new Chunk(normalText, FontFactory.GetFont("Tahoma", 9f, Font.NORMAL)));
        //        _pdfCell = new PdfPCell(combinedPhrase);
        //        _pdfCell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        _pdfCell.Border = 0;
        //        _pdfCell.Colspan = 4;
        //        _pdfTable.AddCell(_pdfCell);
        //        _pdfTable.CompleteRow();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
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
