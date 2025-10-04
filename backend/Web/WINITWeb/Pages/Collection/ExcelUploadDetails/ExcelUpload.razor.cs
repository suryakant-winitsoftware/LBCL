using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using static Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection;
using System.Data;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Practice;
using System.Globalization;
using Org.BouncyCastle.Asn1.Ocsp;
using Newtonsoft.Json;
using Azure;
using Microsoft.AspNetCore.Components;
using System.Resources;

using Winit.UIComponents.Common.Language;
using DocumentFormat.OpenXml.EMMA;
using NPOI.SS.Formula.Functions;
using Winit.Modules.Common.BL.Interfaces;

namespace WinIt.Pages.Collection.ExcelUploadDetails
{
    public partial class ExcelUpload
    {
            private bool showDiv { get; set; } = false;
        private List<Collections> persons = new List<Collections>();
        private List<Collections> OnAccount = new List<Collections>();
        private List<Collections> Invoice = new List<Collections>();
        private List<Collections> InvoiceCopy = new List<Collections>();
        private List<Collections> InvoiceNew = new List<Collections>();
        private static List<Collections> separateList1 = new List<Collections>();
        private static List<Collections> separateList2 = new List<Collections>();
        private static List<Collections> persons101 = new List<Collections>();
        private static List<Collections> persons102 = new List<Collections>();
        private static List<Collections> persons103 = new List<Collections>();
        private static List<Collections> NewArray = new List<Collections>();
        private static Winit.Modules.CollectionModule.Model.Classes.AccPayable ExcelBalance = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
        private static Winit.Modules.CollectionModule.Model.Classes.AccPayable ExcelBalanceChild = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
        private InputFile fileInput;
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections[] collectionInvoice { get; set; } = new Collections[1];
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections[] presentInvoices { get; set; } = new Collections[1];
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections[] collectionOnAccount { get; set; } = new Collections[1];
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collec = new Collections();
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections[] collection11 { get; set; } = new Collections[1];
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collection = new Collections();
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections PresentOnAccount = new Collections();
        private bool isLoading { get; set; } = false;
        private string StoreUID { get; set; } = "";
        private bool _entered { get; set; } = false;
        private string selectedDropdownItem { get; set; } = "";
        private List<string> Recipt { get; set; } = new List<string>();
        public List<AccCollectionAllotment> obj = new List<AccCollectionAllotment>();
        private decimal paid { get; set; } = 0;
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsExist { get; set; }
        private static Dictionary<string, string> Users { get; set; } = new Dictionary<string, string>();

        private Guid inputFileId = Guid.NewGuid();


        //private IJSRuntime JSRuntime { get; set; }


        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await SetHeaderName();
            await _cashSettlementViewModel.GetCustomerCodeName();
            Users = _cashSettlementViewModel.storeData;
            Recipt = Users.Select(kvp => $"{kvp.Value}").ToList();
        }

        public void Redirect()
        {
            _navigationManager.NavigateTo("bulkinsert");
        }

        static string BidirectionalLookup(string input)
        {
            // Check if the input is a key in the dictionary
            if (Users.TryGetValue(input, out string value))
            {
                return $"{input}";
            }

            // Check if the input is a value in the dictionary
            foreach (var pair in Users)
            {
                if (pair.Value == input)
                {
                    return $"{pair.Key}";
                }
            }

            return input;
        }

        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_collection"], IsClickable = true, URL = "showpaymentdetails" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["bulk_collection"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["bulk_collection"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        private void SendListToParent()
        {
            try
            {
                HandleListFromChild(obj);
            }
            catch
            {

            }
        }


        private void Close()
        {
            showDiv = false;
        }
        //after selecting user 
        private async Task HandleListFromChild(List<AccCollectionAllotment> data)
        {
            try
            {
                _loadingService.ShowLoading();
                string ufD = "";
                var createdResponses = "";
                var Failure = "";
                var retValue = "";
                decimal PaidAmt = 0; string Type = ""; string cash = "";
                decimal TotalAmt = 0;
                var rownum = 2;
                var groupedPersons = new Dictionary<string, Collections>();
                foreach (var list in data)
                {
                    var key = $"{list.StoreUID}_{list.PaymentType}_{list.ChequeNo}";
                    if (!groupedPersons.TryGetValue(key, out var person))
                    {
                        PaidAmt = 0;
                        TotalAmt = TotalAmt + Convert.ToDecimal(list.PaidAmount);
                        // Create a new Person object and set its properties from the DataRow
                        person = new Collections();
                        person.AccCollection = InvoiceCopy.FirstOrDefault().AccCollection;
                        person.AccCollection.UID = (Guid.NewGuid()).ToString();
                        person.AccCollection.CreatedBy = _iAppUser.Emp.UID;
                        person.AccCollection.ModifiedBy = _iAppUser.Emp.UID;
                        person.AccCollection.DefaultCurrencyUID = "INR";
                        person.AccCollection.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                        person.AccCollection.JobPositionUID = _iAppUser.SelectedJobPosition.UID;
                        person.AccCollection.EmpUID = _iAppUser.Emp.LoginId;
                        person.AccCollection.Source = "SFA";
                        person.AccCollection.IsMultimode = false;
                        person.AccCollection.Excel = true;
                        person.AccCollection.IsRealized = false;
                        person.AccCollectionPaymentMode = InvoiceCopy.FirstOrDefault().AccCollectionPaymentMode;
                        person.AccCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
                        person.AccStoreLedger = InvoiceCopy.FirstOrDefault().AccStoreLedger;
                        person.AccStoreLedger.UID = (Guid.NewGuid()).ToString();
                        person.AccCollectionAllotment = new List<IAccCollectionAllotment>(); // Initialize the list
                        person.AccPayable = new List<IAccPayable>();
                        person.AccReceivable = new List<IAccReceivable>();
                    }
                    TotalAmt = TotalAmt + Convert.ToDecimal(list.PaidAmount);
                    groupedPersons[key] = person;
                    Type = list.TargetType;
                    cash = InvoiceCopy.FirstOrDefault().AccCollection.Category;
                    var allotment = new AccCollectionAllotment
                    {
                        TargetType = list.TargetType,
                        ReferenceNumber = list.ReferenceNumber,
                        PaidAmount = list.PaidAmount,
                        Amount = list.PaidAmount,
                        StoreUID = list.StoreUID,
                    };
                    person.AccCollectionAllotment.Add(allotment);
                    if (Type.Contains("INVOICE"))
                    {
                        if (cash.Contains("Cash"))
                        {
                            var accPayabl = new AccPayable
                            {
                                SourceUID = list.ReferenceNumber,
                                SourceType = list.TargetType,
                                PaidAmount = Convert.ToDecimal(list.PaidAmount),
                                StoreUID = list.StoreUID,
                                ReferenceNumber = list.ReferenceNumber,
                                //ChequeNo = row.Field<string>("ChequeNumber"),
                                // AccCollectionUID = row.Field<string>("ReceiptNumber"),
                            };
                            person.AccPayable.Add(accPayabl);
                        }
                        else
                        {
                            var accPayable = new AccPayable
                            {
                                SourceUID = list.ReferenceNumber,
                                SourceType = list.TargetType,
                                UnSettledAmount = Convert.ToDecimal(list.PaidAmount),
                                StoreUID = list.StoreUID,
                                ReferenceNumber = list.ReferenceNumber,
                            };

                            person.AccPayable.Add(accPayable);
                        }
                    }
                    else
                    {
                        var accReceivabl = new AccReceivable
                        {
                            SourceUID = list.ReferenceNumber,
                            SourceType = list.TargetType,
                            StoreUID = list.StoreUID,
                            ReferenceNumber = list.ReferenceNumber,
                            PaidAmount = Convert.ToDecimal(list.PaidAmount),
                        };
                        person.AccReceivable.Add(accReceivabl);
                    }

                }
                persons101 = groupedPersons.Values.ToList();

                // Convert the groupedPersons dictionary to a list of persons
                separateList2 = Filtering(groupedPersons, persons101);

                var combinedList = separateList2.Concat(separateList1).ToList();
                NewArray = combinedList;
                await OnAccountCreation(NewArray, true);
                await CheckCustomerEligibleforDiscount(NewArray);
                await ApiMethod(NewArray);
            }
            catch (Exception ex)
            {

            }
        }
        //it groups according to condition
        private List<Collections> Filtering(Dictionary<string, Collections> groupedPersons, List<Collections> Persons)
        {
            try
            {
                persons102 = Persons;
                var sumOfPaidAmountsByUID = persons102
                                         .GroupBy(p => new { p.AccCollection.StoreUID, p.AccCollection.PaymentMode, p.AccCollectionPaymentMode.ChequeNo })
                                         .ToDictionary(
                                             group => new { group.Key.StoreUID, group.Key.PaymentMode, group.Key.ChequeNo }, // Use an anonymous type as the key
                                             group => group.Sum(person => person.AccCollectionAllotment.Sum(allotment => allotment.PaidAmount))
                                         );
                foreach (var item in persons102)
                {
                    var key = new { item.AccCollection.StoreUID, item.AccCollection.PaymentMode, item.AccCollectionPaymentMode.ChequeNo };

                    if (sumOfPaidAmountsByUID.TryGetValue(key, out decimal sum))
                    {
                        item.AccCollection.PaidAmount = Convert.ToDecimal(sum);
                        item.AccCollection.ReceiptNumber = (Guid.NewGuid()).ToString();
                        item.AccStoreLedger.PaidAmount = Convert.ToDecimal(sum);
                        item.AccStoreLedger.PaidAmount = Convert.ToDecimal(sum);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return persons102;
        }
        //when changes selection again and again this gets hit
        private void HandleDataFromChild((IAccCollectionAllotment, string, Collections) data)
        {
            try
            {
                selectedDropdownItem = data.Item2;
                StoreUID = BidirectionalLookup(selectedDropdownItem);
                List<AccCollectionAllotment> collectionList = new List<AccCollectionAllotment>();
                AccCollectionAllotment recordToUpdate = null;
                if (obj != null)
                {
                    // Create a new AccCollectionAllotments object to add to the collection
                    recordToUpdate = obj.FirstOrDefault(item => item.ReferenceNumber == data.Item1.ReferenceNumber);

                    if (recordToUpdate != null)
                    {
                        recordToUpdate.StoreUID = BidirectionalLookup(selectedDropdownItem);
                    }
                    else
                    {
                        AccCollectionAllotment newItem1 = new AccCollectionAllotment();
                        newItem1.ReferenceNumber = data.Item1.ReferenceNumber; // Replace with the actual property you want to set
                        newItem1.StoreUID = BidirectionalLookup(selectedDropdownItem);
                        newItem1.PaidAmount = data.Item1.PaidAmount;
                        newItem1.Amount = data.Item1.PaidAmount;
                        newItem1.TargetType = data.Item1.TargetType;

                        obj.Add(newItem1);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        //create collection method
        private async Task ApiMethod(List<Collections> NewArray)
        {
            try
            {
                bool InvoiceSuccess = false;
                bool OnAccountSuccess = false;
                var createdResponses = "";
                var Failure = "";
                var retValue = "";
                collectionInvoice = NewArray.ToArray();
                collectionOnAccount = OnAccount.ToArray();
                if (collectionInvoice.Any())
                {
                    Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}CollectionModule/CreateReceipt", HttpMethod.Post, collectionInvoice);
                    if (response.StatusCode == 201)
                    {
                        _loadingService.HideLoading();
                        isLoading = false;
                        InvoiceSuccess = true;
                    }
                    else
                    {
                        _loadingService.HideLoading();
                        isLoading = false;
                        InvoiceSuccess = false;
                    }
                }
                if (collectionOnAccount.Any())
                {
                    _loadingService.HideLoading();
                    bool result;
                    if (!IsExist)
                    {
                        result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["user_doesnt_have_selected_invoice._do_you_want_to_create_on_account?"]);
                    }
                    else
                    {
                        result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["as_amount_exceeds_on_account_will_be_created._are_you_sure_to_proceed?"]);
                    }
                    if (result)
                    {
                        _entered = true;
                        _loadingService.ShowLoading();
                        foreach (var list in collectionOnAccount)
                        {
                            collection.AccPayable = new List<IAccPayable>();
                            collection.AccCollection = new AccCollection();
                            collection.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                            collection.AccStoreLedger = new AccStoreLedger();
                            collection.AccCollectionSettlement = new AccCollectionSettlement();
                            collection.AccReceivable = new List<IAccReceivable>();
                            collection.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                            collection.AccCollection.PaidAmount = list.AccCollection.PaidAmount;
                            collection.AccCollection.ReceiptNumber = list.AccCollection.ConsolidatedReceiptNumber;
                            collection.AccCollection.SessionUserCode = "WINIT";
                            collection.AccCollection.Category = list.AccCollection.Category;
                            collection.AccCollection.Excel = true;
                            collection.AccCollection.UID = (Guid.NewGuid()).ToString();

                            Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}CollectionModule/CreateOnAccountReceipt", HttpMethod.Post, list);
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
                            OnAccountSuccess = true;
                        }
                        else if (retValue != "" && Failure == "")
                        {
                            OnAccountSuccess = false;
                        }
                        else
                        {
                            OnAccountSuccess = false;
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", "OnAccount Cancelled");
                        return;
                    }

                }
                ShowPopUp(InvoiceSuccess, OnAccountSuccess);
                _NavigationManager.NavigateTo("showpaymentdetails");
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["exception"]);
            }
        }

        public async void ShowPopUp(bool InvoiceSuccess, bool OnAccountSuccess)
        {
            if (InvoiceSuccess && OnAccountSuccess)
            {
                _loadingService.HideLoading();
                await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["invoices_and_on_accounts_are_created"]);
            }
            if (InvoiceSuccess == false && OnAccountSuccess)
            {
                _loadingService.HideLoading();
                await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["invoices_are_not_created,_on_accounts_are_created"]);
            }
            if (OnAccountSuccess == false && InvoiceSuccess)
            {
                _loadingService.HideLoading();
                if (_entered)
                {
                    await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["invoices_are_created,_on_accounts_are_not_created"]);
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["invoices_are_created"]);
                }
            }
            if (!OnAccountSuccess && !InvoiceSuccess)
            {
                _loadingService.HideLoading();
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["error"]);
            }
        }


        public string sixGuidstring()
        {
            Guid newGuid = Guid.NewGuid();

            // Convert the GUID to a string and take the first 8 characters without hyphens
            string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);

            return eightDigitGuid;
        }
        private void ResetInputFile()
        {
            // Change id so that blazor re-renders InputFile as new component
            inputFileId = Guid.NewGuid();
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

                    string payment = row.Field<string>("PaymentMode");
                    // Check for empty cells in the row
                    bool rowHasEmptyCell = false;
                    for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                    {
                        var cellValue = worksheet.Cells[rowNumber, columnNumber].Text;
                        if (payment.ToLower() == "cash" && (columnNumber == 4 || columnNumber == 5 || columnNumber == 6 || columnNumber == 1))
                        {
                            continue; // Skip this cell
                        }
                        if(columnNumber == 1)
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
                    else
                    {
                        return true;
                    }
                }
                return true;
            }
        }
        //handles the file upload 
        private async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
            try
            {
                _loadingService.ShowLoading();
                showDiv = false;
                string ufD = "";
                var createdResponses = "";
                var Failure = "";
                var retValue = "";
                decimal PaidAmt = 0; string Type = ""; string cash = "";
                decimal TotalAmt = 0;
                var rownum = 2;
                var file = e.File;
                if (file != null && IsExcelFile(file))
                {
                    using (var stream = file.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the LicenseContext

                        // Copy the stream to a memory stream
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            if (ExcelFileContainsData(memoryStream))
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

                                    var groupedPersons = new Dictionary<string, Collections>();

                                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                                    {
                                        var row = dataTable.NewRow();
                                        for (var columnNumber = 1; columnNumber <= worksheet.Dimension.End.Column; columnNumber++)
                                        {
                                            row[dataTable.Columns[columnNumber - 1].ColumnName] = worksheet.Cells[rowNumber, columnNumber].Text;
                                        }
                                        dataTable.Rows.Add(row);
                                        //if (CheckFileValidations(row))
                                        //{

                                        //}
                                        var key = $"{row.Field<string>("CustomerCode")}_{row.Field<string>("PaymentMode")}_{row.Field<string>("ChequeNumber")}";
                                        ufD = (row.Field<string>("CustomerCode")).ToString();
                                        StoreUID = BidirectionalLookup(row.Field<string>("CustomerCode"));
                                        if (!groupedPersons.TryGetValue(key, out var person))
                                        {
                                            if (!string.IsNullOrEmpty(ufD))
                                            {
                                                PaidAmt = 0;
                                                TotalAmt = TotalAmt + Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                // Create a new Person object and set its properties from the DataRow
                                                person = new Collections
                                                {
                                                    AccCollection = new AccCollection
                                                    {
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        CreatedBy = "ADMIN",
                                                        ModifiedBy = "ADMIN",
                                                        ReceiptNumber = sixGuidstring(),
                                                        ConsolidatedReceiptNumber = sixGuidstring(),
                                                        TripDate = DateTime.Now,
                                                        DefaultCurrencyUID = "INR",
                                                        DefaultCurrencyExchangeRate = 1,
                                                        DefaultCurrencyAmount = row.Field<string>("Currency").Contains("USD") ? TotalAmt * 80 : TotalAmt,
                                                        CurrencyUID = row.Field<string>("Currency"),
                                                        OrgUID = _iAppUser.SelectedJobPosition.OrgUID,
                                                        DistributionChannelUID = "",
                                                        RouteUID = "",
                                                        JobPositionUID = _iAppUser.SelectedJobPosition.UID,
                                                        EmpUID = _iAppUser.Emp.LoginId,
                                                        Status = row.Field<string>("PaymentMode") == "Cash" ? "Collected" : "Submitted",
                                                        ReferenceNumber = row.Field<string>("PaymentMode") != "Cash" ? row.Field<string>("ChequeNumber") : "",
                                                        IsRealized = false,
                                                        Latitude = "0",
                                                        Longitude = "0",
                                                        Source = "SFA",
                                                        IsMultimode = false,
                                                        TrxType = row.Field<string>("TrxType"),
                                                        StoreUID = row.Field<string>("CustomerCode"),
                                                        //ReceiptNumber = row.Field<string>("ReceiptNumber"),
                                                        Category = row.Field<string>("PaymentMode"),
                                                        Amount = TotalAmt,
                                                        Excel = true,
                                                        CollectedDate = DateTime.TryParseExact(
                                                                    row.Field<string>("CollectedDate"),
                                                                    "M/d/yyyy",
                                                                    CultureInfo.InvariantCulture,
                                                                    DateTimeStyles.None,
                                                                    out var collectedDate)
                                                                    ? collectedDate
                                                                    : DateTime.MinValue,
                                                    },
                                                    // Add other top-level properties here
                                                    AccCollectionPaymentMode = new AccCollectionPaymentMode
                                                    {
                                                        BankUID = row.Field<string>("BankCode"),
                                                        Branch = row.Field<string>("BankBranchName"),
                                                        ChequeNo = row.Field<string>("ChequeNumber"),
                                                        ChequeDate = DateTime.TryParseExact(
                                                                    row.Field<string>("CollectedDate"),
                                                                    "M/d/yyyy",
                                                                    CultureInfo.InvariantCulture,
                                                                    DateTimeStyles.None,
                                                                    out var collectedDate1)
                                                                    ? collectedDate1
                                                                    : DateTime.MinValue,
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        Amount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                        // Add other properties of accCollectionPaymentMode here
                                                    },
                                                    AccStoreLedger = new AccStoreLedger
                                                    {
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        Amount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                        CollectedAmount = TotalAmt,
                                                    },
                                                    AccCollectionAllotment = new List<IAccCollectionAllotment>(), // Initialize the list
                                                    AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>(),
                                                    AccPayable = new List<IAccPayable>(),
                                                    AccReceivable = new List<IAccReceivable>(),
                                                };
                                            }
                                            else
                                            {
                                                string trxCode = (row.Field<string>("TrxCode")).ToString();
                                                PaidAmt = 0;
                                                TotalAmt = TotalAmt + Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                // Create a new Person object and set its properties from the DataRow
                                                person = new Collections
                                                {
                                                    AccCollection = new AccCollection
                                                    {
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        CreatedBy = "WINIT",
                                                        ModifiedBy = "WINIT",
                                                        ReceiptNumber = sixGuidstring(),
                                                        ConsolidatedReceiptNumber = sixGuidstring(),
                                                        TripDate = DateTime.Now,
                                                        DefaultCurrencyUID = "INR",
                                                        DefaultCurrencyExchangeRate = 1,
                                                        DefaultCurrencyAmount = row.Field<string>("Currency").Contains("USD") ? TotalAmt * 80 : TotalAmt,
                                                        CurrencyUID = row.Field<string>("Currency"),
                                                        OrgUID = _iAppUser.SelectedJobPosition.OrgUID,
                                                        DistributionChannelUID = "",
                                                        RouteUID = "",
                                                        JobPositionUID = _iAppUser.SelectedJobPosition.UID,
                                                        EmpUID = _iAppUser.Emp.LoginId,
                                                        Status = row.Field<string>("PaymentMode") == "Cash" ? "Collected" : "Submitted",
                                                        ReferenceNumber = row.Field<string>("PaymentMode") != "Cash" ? row.Field<string>("ChequeNumber") : "",
                                                        IsRealized = false,
                                                        Latitude = "0",
                                                        Longitude = "0",
                                                        Source = "SFA",
                                                        IsMultimode = false,
                                                        TrxType = row.Field<string>("TrxType"),
                                                        StoreUID = row.Field<string>("CustomerCode"),
                                                        //ReceiptNumber = row.Field<string>("ReceiptNumber"),
                                                        Category = row.Field<string>("PaymentMode"),
                                                        Amount = TotalAmt,
                                                        Excel = true,
                                                        CollectedDate = DateTime.TryParseExact(
                                                                    row.Field<string>("CollectedDate"),
                                                                    "M/d/yyyy",
                                                                    CultureInfo.InvariantCulture,
                                                                    DateTimeStyles.None,
                                                                    out var collectedDate)
                                                                    ? collectedDate
                                                                    : DateTime.MinValue,
                                                    },
                                                    // Add other top-level properties here
                                                    AccCollectionPaymentMode = new AccCollectionPaymentMode
                                                    {
                                                        BankUID = row.Field<string>("BankCode"),
                                                        Branch = row.Field<string>("BankBranchName"),
                                                        ChequeNo = row.Field<string>("ChequeNumber"),
                                                        ChequeDate = DateTime.TryParseExact(
                                                                    row.Field<string>("CollectedDate"),
                                                                    "M/d/yyyy",
                                                                    CultureInfo.InvariantCulture,
                                                                    DateTimeStyles.None,
                                                                    out var collectedDate1)
                                                                    ? collectedDate1
                                                                    : DateTime.MinValue,
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        Amount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount"))
                                                        // Add other properties of accCollectionPaymentMode here
                                                    },
                                                    AccStoreLedger = new AccStoreLedger
                                                    {
                                                        UID = (Guid.NewGuid()).ToString(),
                                                        Amount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                        CollectedAmount = TotalAmt,
                                                    },
                                                    AccCollectionAllotment = new List<IAccCollectionAllotment>(), // Initialize the list
                                                    AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>(),
                                                    AccPayable = new List<IAccPayable>(),
                                                    AccReceivable = new List<IAccReceivable>(),
                                                };
                                                showDiv = true;

                                            }
                                        }
                                        groupedPersons[key] = person;
                                        Type = row.Field<string>("TrxType");
                                        cash = row.Field<string>("PaymentMode");

                                        if (person.AccCollectionAllotment.Any(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                        {
                                            foreach (var data in person.AccCollectionAllotment.Where(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                            {
                                                data.PaidAmount += row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                data.Amount = data.PaidAmount;
                                            }
                                        }
                                        else
                                        {
                                            var allotment = new AccCollectionAllotment
                                            {
                                                TargetType = row.Field<string>("TrxType"),
                                                ReferenceNumber = row.Field<string>("TrxCode"),
                                                PaidAmount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                Amount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                StoreUID = row.Field<string>("CustomerCode"),
                                                // Add other properties of AccCollectionAllotment here
                                            };
                                            person.AccCollectionAllotment.Add(allotment);
                                        }
                                        person.AccCollectionCurrencyDetails.Add(new AccCollectionCurrencyDetails());
                                        if (Type.Contains("INVOICE"))
                                        {
                                            if (cash.Contains("Cash"))
                                            {
                                                if (person.AccPayable.Any(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                                {
                                                    foreach (var data in person.AccPayable.Where(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                                    {
                                                        data.PaidAmount += row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                        data.Amount = data.PaidAmount;
                                                    }
                                                }
                                                else
                                                {
                                                    var accPayabl = new AccPayable
                                                    {
                                                        SourceType = row.Field<string>("TrxType"),
                                                        SourceUID = row.Field<string>("TrxCode"),
                                                        ReferenceNumber = row.Field<string>("TrxCode"),
                                                        PaidAmount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                        StoreUID = row.Field<string>("CustomerCode"),
                                                        //ChequeNo = row.Field<string>("ChequeNumber"),
                                                        // AccCollectionUID = row.Field<string>("ReceiptNumber"),
                                                    };
                                                    person.AccPayable.Add(accPayabl);
                                                }
                                            }
                                            else
                                            {
                                                if (person.AccPayable.Any(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                                {
                                                    foreach (var data in person.AccPayable.Where(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                                    {
                                                        data.PaidAmount += row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                        data.Amount = data.PaidAmount;
                                                    }
                                                }
                                                else
                                                {
                                                    var accPayable = new AccPayable
                                                    {
                                                        SourceType = row.Field<string>("TrxType"),
                                                        SourceUID = row.Field<string>("TrxCode"),
                                                        ReferenceNumber = row.Field<string>("TrxCode"),
                                                        UnSettledAmount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                        StoreUID = row.Field<string>("CustomerCode"),
                                                        //ChequeNo = row.Field<string>("ChequeNumber"),
                                                        // AccCollectionUID = row.Field<string>("ReceiptNumber"),
                                                    };
                                                    person.AccPayable.Add(accPayable);
                                                }

                                            }
                                        }
                                        else
                                        {
                                            if (person.AccReceivable.Any(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                            {
                                                foreach (var data in person.AccReceivable.Where(p => p.ReferenceNumber == row.Field<string>("TrxCode") && p.StoreUID == row.Field<string>("CustomerCode")))
                                                {
                                                    data.PaidAmount += row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount"));
                                                    data.Amount = data.PaidAmount;
                                                }
                                            }
                                            else
                                            {
                                                var accReceivabl = new AccReceivable
                                                {
                                                    SourceType = row.Field<string>("TrxType"),
                                                    SourceUID = row.Field<string>("TrxCode"),
                                                    ReferenceNumber = row.Field<string>("TrxCode"),
                                                    PaidAmount = row.Field<string>("Currency").Contains("USD") ? Convert.ToDecimal(row.Field<string>("PaidAmount")) * 80 : Convert.ToDecimal(row.Field<string>("PaidAmount")),
                                                    StoreUID = row.Field<string>("CustomerCode"),
                                                    //ChequeNo = row.Field<string>("ChequeNumber"),
                                                    // AccCollectionUID = row.Field<string>("ReceiptNumber"),
                                                };
                                                person.AccReceivable.Add(accReceivabl);
                                            }
                                        }
                                    }

                                    // Convert the groupedPersons dictionary to a list of persons
                                    persons = groupedPersons.Values.ToList();
                                    var sumOfPaidAmountsByUID = persons
                                                             .GroupBy(p => new { p.AccCollection.StoreUID, p.AccCollection.PaymentMode, p.AccCollectionPaymentMode.ChequeNo })
                                                             .ToDictionary(
                                                                 group => new { group.Key.StoreUID, group.Key.PaymentMode, group.Key.ChequeNo }, // Use an anonymous type as the key
                                                                 group => group.Sum(person => person.AccCollectionAllotment.Sum(allotment => allotment.PaidAmount))
                                                             );
                                    foreach (var item in persons)
                                    {
                                        var key = new { item.AccCollection.StoreUID, item.AccCollection.PaymentMode, item.AccCollectionPaymentMode.ChequeNo };

                                        if (sumOfPaidAmountsByUID.TryGetValue(key, out decimal sum))
                                        {
                                            item.AccCollection.Amount = Convert.ToDecimal(sum);
                                            item.AccCollection.DefaultCurrencyAmount = Convert.ToDecimal(sum);
                                            item.AccCollection.ReceiptNumber = (Guid.NewGuid()).ToString();
                                            item.AccStoreLedger.Amount = Convert.ToDecimal(sum);
                                            item.AccStoreLedger.PaidAmount = Convert.ToDecimal(sum);
                                        }
                                    }
                                    OnAccount = persons.Where(item => item.AccCollection.TrxType.Contains("OnAccount")).ToList();
                                    Invoice = persons.Where(item => !item.AccCollection.TrxType.Contains("OnAccount")).ToList();
                                    InvoiceCopy = Invoice;
                                    //checks extra amount is there
                                    if (!showDiv)
                                    {
                                        await OnAccountCreation(persons, false);
                                    }
                                    if (!string.IsNullOrEmpty(StoreUID))
                                    {
                                        await CheckCustomerEligibleforDiscount(Invoice);
                                    }
                                    //if user is not selected then separted according to it
                                    if (Invoice.Any())
                                    {
                                        separateList1 = Invoice.Where(p => !string.IsNullOrEmpty(p.AccCollection.StoreUID)).ToList();
                                        separateList2 = Invoice.Where(p => string.IsNullOrEmpty(p.AccCollection.StoreUID)).ToList();
                                    }
                                    if (showDiv)
                                    {
                                        _loadingService.HideLoading();
                                    }
                                    if (!showDiv)
                                    {
                                        bool InvoiceSuccess = false;
                                        bool OnAccountSuccess = false;
                                        if (Invoice.Any())
                                        {
                                            presentInvoices = Invoice.ToArray();
                                            Winit.Shared.Models.Common.ApiResponse<string> response1 = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}CollectionModule/CreateReceipt", HttpMethod.Post, presentInvoices);
                                            if (response1.StatusCode == 201)
                                            {
                                                _loadingService.HideLoading();
                                                isLoading = false;
                                                InvoiceSuccess = true;
                                            }
                                            else
                                            {
                                                _loadingService.HideLoading();
                                                isLoading = false;
                                                InvoiceSuccess = false;
                                            }
                                        }
                                        if (OnAccount.Any())
                                        {
                                            _loadingService.HideLoading();
                                            bool result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["as_amount_exceeds_on_account_will_be_created._are_you_sure_to_proceed?"]);
                                            if (result)
                                            {
                                                _entered = true;
                                                _loadingService.ShowLoading();
                                                foreach (var list in OnAccount)
                                                {
                                                    foreach (var lst in list.AccCollectionAllotment)
                                                    {
                                                        PresentOnAccount.AccPayable = new List<IAccPayable>();
                                                        PresentOnAccount.AccCollection = new AccCollection();
                                                        PresentOnAccount.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                                                        PresentOnAccount.AccStoreLedger = new AccStoreLedger();
                                                        PresentOnAccount.AccCollectionSettlement = new AccCollectionSettlement();
                                                        PresentOnAccount.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
                                                        PresentOnAccount.AccReceivable = new List<IAccReceivable>();
                                                        PresentOnAccount.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                                                        PresentOnAccount.AccCollection.PaidAmount = Convert.ToDecimal(lst.PaidAmount);
                                                        PresentOnAccount.AccCollection.ReceiptNumber = lst.AccCollectionUID;
                                                        PresentOnAccount.AccCollection.SessionUserCode = "WINIT";
                                                        PresentOnAccount.AccCollection.Category = lst.PaymentType;
                                                        PresentOnAccount.AccCollection.Excel = true;
                                                        PresentOnAccount.AccCollection.UID = (Guid.NewGuid()).ToString();
                                                        Winit.Shared.Models.Common.ApiResponse<string> response2 = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}CollectionModule/CreateOnAccountReceipt", HttpMethod.Post, PresentOnAccount);
                                                        if (response2.StatusCode == 201)
                                                        {
                                                            createdResponses = "Created";
                                                        }
                                                        else if (response2.StatusCode == 500)
                                                        {
                                                            retValue = "InternalServerError";
                                                        }
                                                        else
                                                        {
                                                            Failure = "failed";
                                                        }
                                                    }
                                                }
                                                if (Failure == "" && retValue == "")
                                                {
                                                    OnAccountSuccess = true;
                                                }
                                                else if (retValue != "" && Failure == "")
                                                {
                                                    OnAccountSuccess = false;
                                                }
                                                else
                                                {
                                                    OnAccountSuccess = false;
                                                }
                                            }
                                        }
                                        ShowPopUp(InvoiceSuccess, OnAccountSuccess);
                                    }
                                }
                            }
                            else
                            {
                                _loadingService.HideLoading();
                                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_all_fields_in_excel"]);
                            }
                        }
                    }
                }
                else
                {
                    _loadingService.HideLoading();
                    inputFileId = Guid.NewGuid();
                    StateHasChanged();
                    await _alertService.ShowErrorAlert(@Localizer["invalidfile"], @Localizer["only_excel_files_are_uploadable."]);
                }
            }
            catch (Exception ex)
            {
                _loadingService.HideLoading();
                isLoading = false;
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["excel_error"]);
            }

        }

        protected async Task OnAccountCreation(List<Collections> persons, bool Missing)
        {
            decimal enter = 0;
            string currentUid = "";
            decimal sumOfPaidAmount = 0;
            List<Collections> exceededBalanceList = new List<Collections>();
            List<Collections> withinBalanceList = new List<Collections>();
            foreach (var person in persons)
            {
                Collections objExceed = new Collections();
                objExceed.AccCollection = new AccCollection();
                objExceed.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                objExceed.AccStoreLedger = new AccStoreLedger();
                objExceed.AccCollection = person.AccCollection;
                objExceed.AccCollection.UID = (Guid.NewGuid()).ToString();
                objExceed.AccCollection.CreatedBy = _iAppUser.Emp.UID;
                objExceed.AccCollection.ModifiedBy = _iAppUser.Emp.UID;
                objExceed.AccCollection.DefaultCurrencyUID = "INR";
                objExceed.AccCollection.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                objExceed.AccCollection.JobPositionUID = _iAppUser.SelectedJobPosition.UID;
                objExceed.AccCollection.EmpUID = _iAppUser.SelectedJobPosition.EmpUID;
                objExceed.AccCollection.Source = "SFA";
                objExceed.AccCollection.IsMultimode = false;
                objExceed.AccCollection.Excel = true;
                objExceed.AccCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
                objExceed.AccCollectionPaymentMode = person.AccCollectionPaymentMode;
                objExceed.AccStoreLedger.UID = (Guid.NewGuid()).ToString();
                objExceed.AccStoreLedger = person.AccStoreLedger;
                objExceed.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                objExceed.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
                objExceed.AccCollectionSettlement = new AccCollectionSettlement();
                objExceed.AccReceivable = new List<IAccReceivable>();
                objExceed.AccPayable = new List<IAccPayable>();
                //----------------------------------------------//
                Collections objWithin = new Collections();
                objWithin.AccCollection = new AccCollection();
                objWithin.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                objWithin.AccStoreLedger = new AccStoreLedger();
                objWithin.AccCollection = person.AccCollection;
                objWithin.AccCollection.UID = (Guid.NewGuid()).ToString();
                objWithin.AccCollection.CreatedBy = _iAppUser.Emp.UID;
                objWithin.AccCollection.ModifiedBy = _iAppUser.Emp.UID;
                objWithin.AccCollection.DefaultCurrencyUID = "INR";
                objWithin.AccCollection.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                objWithin.AccCollection.JobPositionUID = _iAppUser.SelectedJobPosition.UID;
                objWithin.AccCollection.EmpUID = _iAppUser.SelectedJobPosition.EmpUID;
                objWithin.AccCollection.Source = "SFA";
                objWithin.AccCollection.IsMultimode = false;
                objWithin.AccCollection.Excel = true;
                objWithin.AccCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
                objWithin.AccCollectionPaymentMode = person.AccCollectionPaymentMode;
                objWithin.AccStoreLedger.UID = (Guid.NewGuid()).ToString();
                objWithin.AccStoreLedger = person.AccStoreLedger;
                objWithin.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                objWithin.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
                objWithin.AccCollectionSettlement = new AccCollectionSettlement();
                objWithin.AccReceivable = new List<IAccReceivable>();
                objWithin.AccPayable = new List<IAccPayable>();

                foreach (var accPayableItem in person.AccPayable)
                {
                    Winit.Shared.Models.Common.ApiResponse<AccPayable> response = await _apiService.FetchDataAsync<AccPayable>($"{_appConfigs.ApiBaseUrl}CollectionModule/ExcelBalance?ReceiptNumber=" + accPayableItem.ReferenceNumber + "&StoreUID=" + accPayableItem.StoreUID, HttpMethod.Get);
                    if(response.Data != null)
                    {
                        ExcelBalanceChild = response.Data;
                    }
                    decimal _balance = 0;
                    decimal _balanceSettle = 0;
                    IsExist = response.StatusCode == 404 ? false : true;
                    if (person.AccCollection.Category == "Cash")
                    {
                        _balance = ExcelBalanceChild.BalanceAmount;
                    }
                    else
                    {
                        _balance = ExcelBalanceChild.BalanceAmount;
                        _balanceSettle = ExcelBalanceChild.UnSettledAmount;
                        if (_balance - _balanceSettle == 0)
                        {
                            _balance = 0;
                        }
                        else
                        {
                            _balance = _balance > _balanceSettle ? _balance - _balanceSettle : _balanceSettle - _balance;
                        }
                    }
                    if (accPayableItem.PaidAmount > _balance)
                    {
                        IAccCollectionCurrencyDetails accCollectionCurrencyDetail = new AccCollectionCurrencyDetails();
                        if (_balance != 0)
                        {
                            IAccCollectionCurrencyDetails accCollectionCurrencyDetails = new AccCollectionCurrencyDetails();
                            AccPayable accpayWithin = new AccPayable
                            {
                                SourceUID = accPayableItem.SourceUID,
                                SourceType = accPayableItem.SourceType,
                                ReferenceNumber = accPayableItem.ReferenceNumber,
                                PaidAmount = _balance,
                            };
                            AccCollectionAllotment allotWithin = new AccCollectionAllotment
                            {
                                ReferenceNumber = accPayableItem.ReferenceNumber,
                                PaidAmount = _balance,
                                Amount = _balance,
                                TargetType = accPayableItem.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE",
                                StoreUID = accPayableItem.StoreUID,
                            };

                            objWithin.AccCollectionCurrencyDetails.Add(accCollectionCurrencyDetails);
                            objWithin.AccPayable.Add(accpayWithin);
                            objWithin.AccCollectionAllotment.Add(allotWithin);
                        }

                        objExceed.AccCollection.PaidAmount = accPayableItem.PaidAmount - _balance;

                        AccCollectionAllotment allotExceeded = new AccCollectionAllotment
                        {
                            ReferenceNumber = accPayableItem.ReferenceNumber,
                            PaidAmount = accPayableItem.PaidAmount == 0 ? accPayableItem.UnSettledAmount : accPayableItem.PaidAmount - _balance,
                            Amount = accPayableItem.PaidAmount == 0 ? accPayableItem.UnSettledAmount : accPayableItem.PaidAmount - _balance,
                            TargetType = accPayableItem.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE",
                            StoreUID = accPayableItem.StoreUID,
                        };
                        objExceed.AccCollectionCurrencyDetails.Add(accCollectionCurrencyDetail);
                        objExceed.AccCollectionAllotment.Add(allotExceeded);
                    }
                    else
                    {
                        AccCollectionAllotment allotbalanced = new AccCollectionAllotment
                        {
                            ReferenceNumber = accPayableItem.ReferenceNumber,
                            PaidAmount = accPayableItem.PaidAmount == 0 ? accPayableItem.UnSettledAmount : accPayableItem.PaidAmount,
                            Amount = accPayableItem.PaidAmount == 0 ? accPayableItem.UnSettledAmount : accPayableItem.PaidAmount,
                            TargetType = accPayableItem.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE",
                            StoreUID = accPayableItem.StoreUID,
                        };
                        objWithin.AccPayable.Add(accPayableItem);
                        objWithin.AccCollectionAllotment.Add(allotbalanced);
                    }
                }
                if (objExceed.AccCollectionAllotment != null && objExceed.AccCollectionAllotment.Any())
                {
                    exceededBalanceList.Add(objExceed);
                }

                // Check if objWithin has data before adding to withinBalanceList
                if (objWithin.AccPayable != null && objWithin.AccPayable.Any())
                {
                    withinBalanceList.Add(objWithin);
                }
            }
            if (!Missing)
            {
                OnAccount = exceededBalanceList;
                Invoice = withinBalanceList;
                InvoiceCopy = Invoice;
            }
            else
            {
                NewArray = withinBalanceList;
                InvoiceCopy = NewArray;
                OnAccount = exceededBalanceList;
            }
        }


        private EarlyPaymentDiscountConfiguration[] EligibleRecords { get; set; } = new EarlyPaymentDiscountConfiguration[0];
        private Winit.Modules.CollectionModule.Model.Classes.AccPayable[] InvoiceRecords { get; set; } = new Winit.Modules.CollectionModule.Model.Classes.AccPayable[0];
        public List<Winit.Modules.CollectionModule.Model.Classes.AccPayable> OverDueRecords { get; set; } = new List<Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        public List<Winit.Modules.CollectionModule.Model.Classes.AccPayable> OnTimeRecords { get; set; } = new List<Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        public decimal Discountval { get; set; } = 0;
        public int AdvanceDays { get; set; } = 0;
        public decimal discount { get; set; } = 0;
        private List<Winit.Modules.CollectionModule.Model.Classes.AccPayable> ResponseDaystabList { get; set; } = new List<Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        public async Task CheckCustomerEligibleforDiscount(List<Collections> Invoice)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<EarlyPaymentDiscountConfiguration[]> eligibilityRecords = await _apiService.FetchDataAsync<EarlyPaymentDiscountConfiguration[]>($"{_appConfigs.ApiBaseUrl}CollectionModule/CheckEligibleForDiscount?ApplicableCode=" + StoreUID, HttpMethod.Get, null);
                if(eligibilityRecords.Data != null)
                {
                    EligibleRecords = eligibilityRecords.Data;
                }
                if (EligibleRecords.Length > 0)
                {
                    bool IsOverDue = await CheckOverDue(EligibleRecords);
                    if (!IsOverDue)
                    {
                        await CheckAdvanceDays(EligibleRecords, Invoice);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async Task<bool> CheckOverDue(EarlyPaymentDiscountConfiguration[] EligibleRecords)
        {
            DateTime today = DateTime.Now;
            Winit.Shared.Models.Common.ApiResponse<AccPayable[]> invoices = await _apiService.FetchDataAsync<AccPayable[]>($"{_appConfigs.ApiBaseUrl}CollectionModule/GetInvoices?StoreUID=" + StoreUID, HttpMethod.Get, null);
            if(invoices.Data != null)
            {
                InvoiceRecords = invoices.Data;
            }
            OverDueRecords = InvoiceRecords.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate < today).ToList();
            OnTimeRecords = InvoiceRecords.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate > today).ToList();
            if (OverDueRecords.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public decimal ApplyDiscount(double Amount)
        {
            return 0;
        }


        public async Task CheckAdvanceDays(EarlyPaymentDiscountConfiguration[] EligibleRecords, List<Collections> Invoice)
        {
            try
            {
                AdvanceDays = EligibleRecords[0].Advance_Paid_Days;
                Discountval = EligibleRecords[0].Discount_Value;
                foreach (var list in OnTimeRecords)
                {
                    int diff = Convert.ToInt32((DateTime.Now - list.TransactionDate)?.TotalDays);
                    if (diff > 0 && diff < AdvanceDays && list.SourceType.Contains("INVOICE"))
                    {
                        foreach (var item in Invoice)
                        {
                            var allotmentsToAdd = new List<AccCollectionAllotment>();
                            foreach (var lst in item.AccCollectionAllotment)
                            {
                                if (lst.ReferenceNumber == list.ReferenceNumber && (decimal)lst.PaidAmount == list.Amount)
                                {
                                    decimal num = (decimal)lst.PaidAmount;
                                    lst.Discount = true;
                                    lst.PaidAmount = lst.PaidAmount - lst.PaidAmount * Discountval / 100;
                                    lst.DiscountAmount = num * Discountval / 100;
                                    lst.EarlyPaymentDiscountAmount = lst.DiscountAmount;
                                    lst.EarlyPaymentDiscountPercentage = Discountval;
                                    lst.EarlyPaymentDiscountReferenceNo = list.ReferenceNumber;
                                    item.AccCollection.Amount = item.AccCollectionAllotment.Sum(p => (decimal)p.PaidAmount);
                                    item.AccCollection.IsEarlyPayment = true;
                                    if (lst.DiscountAmount > 0)
                                    {
                                        AccCollectionAllotment allotment = new AccCollectionAllotment();
                                        allotment.UID = (Guid.NewGuid()).ToString();
                                        allotment.PaidAmount = lst.DiscountAmount;
                                        allotment.DiscountAmount = 0;
                                        allotment.TargetType = "CREDITNOTE";
                                        allotment.StoreUID = StoreUID;
                                        allotment.ReferenceNumber = "";
                                        allotmentsToAdd.Add(allotment);
                                    }
                                }
                            }
                            item.AccCollectionAllotment.AddRange(allotmentsToAdd);
                        }

                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
        public void CreateDiscountRecord(ICollections collection, decimal Amount, string StoreUID)
        {
            AccCollectionAllotment allotment = new AccCollectionAllotment();
            allotment.UID = (Guid.NewGuid()).ToString();
            allotment.PaidAmount = Amount;
            allotment.DiscountAmount = 0;
            allotment.TargetType = "CREDITNOTE";
            allotment.StoreUID = StoreUID;
            allotment.ReferenceNumber = "";
            collection.AccCollectionAllotment.Add(allotment);
        }



    }
}
