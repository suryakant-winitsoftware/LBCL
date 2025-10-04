using ClosedXML.Excel;
using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
using System.Text.RegularExpressions;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using static WinIt.Pages.Customer_Details.BusinessDetails;
using Winit.Shared.Models.Constants;
using Winit.Modules.Address.Model.Interfaces;
using Winit.UIModels.Common.GST;
using Microsoft.JSInterop;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using NPOI.HSSF.Record;
using OfficeOpenXml.Style;
using Winit.Modules.Store.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Store.BL.Classes;

namespace WinIt.Pages.Customer_Details
{
    public partial class ShowRoomDetails
    {
        [Parameter] public IAddress _locationInformation { get; set; }
        protected List<Winit.Modules.Location.Model.Classes.LocationMaster>? LocationMasters;
        [Inject] Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSettings { get; set; }
        public Winit.Modules.Location.Model.Interfaces.ILocationData SelectedLocation { get; set; }
        private bool ViewLocation { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> DataGridColumnsForExport { get; set; }
        public string ValidationMessage;
        private string EmailValidationMessage;
        private IBrowserFile selectedFile;
        public bool IsEditShowroomDetails = false;
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string TabName { get; set; }
        private bool IsSaveAttempted { get; set; } = false;
        private IStoreShowroom SelectedShowroom { get; set; } = new StoreShowroom();
        public IStoreAdditionalInfoCMI storeAdditionalInfoCMI = new StoreAdditionalInfoCMI();
        public IStoreAdditionalInfoCMI OriginalstoreAdditionalInfoCMI = new StoreAdditionalInfoCMI();
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateShowroom { get; set; }
        [Parameter] public EventCallback<IStoreShowroom> OnAddShowroom { get; set; }
        [Parameter] public EventCallback<IStoreShowroom> OnEditShowroom { get; set; }

        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public Func<Task<List<IStoreShowroom>>> OnShowAllShowroomClick { get; set; }
        [Parameter] public List<IStoreShowroom> ShowroomDetails { get; set; } = new List<IStoreShowroom>();
        [Parameter] public List<IStoreShowroom> OriginalShowroomDetails { get; set; } = new List<IStoreShowroom>();
        [Parameter] public List<IStoreShowroom> ShowroomDetailsCopy { get; set; } = new List<IStoreShowroom>();
        [Parameter] public List<IStoreShowroom> ShowroomDetailsErrorList { get; set; } = new List<IStoreShowroom>();
        [Parameter] public List<IAddress> ShiptoAddresses { get; set; } = new List<IAddress>();
        [Parameter] public EventCallback<IStoreShowroom> OnEditClickInvokeAddressDropdowns { get; set; }
        private bool IsDelete = false;
        private bool IsEditPage = false;
        public bool IsShowAllShowroom { get; set; } = false;
        public bool IsAddPopUp { get; set; }
        // public string ButtonName { get; set; } = "Add";
        public string ButtonName { get; set; } = "Save";
        public bool IsSuccess { get; set; } = false;
        private bool isNewDataAdded = false; // Tracks if new data has been added
        private bool IsExcelUpload = false; // Tracks if excel is uploading
        private bool showPopup = false; // Controls the popup visibility
        public string Message { get; set; } = "";
        [Parameter] public List<ISelectionItem> StateselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> CityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> LocalityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> BranchselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> PinCodeselectionItems { get; set; }
        [Parameter] public EventCallback<List<string>> StateSelected { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public EventCallback<List<string>> CitySelected { get; set; }
        [Parameter] public EventCallback<List<string>> LocalitySelected { get; set; }

        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _locationInformation = _serviceProvider.CreateInstance<IAddress>();
            SelectedLocation = _serviceProvider.CreateInstance<Winit.Modules.Location.Model.Interfaces.ILocationData>();
            DataGridColumnsForExport = new List<DataGridColumn>
                {
                    new DataGridColumn { Header = "Address1" },
                    new DataGridColumn { Header = "Address2" },
                    new DataGridColumn { Header = "Address3" },
                    new DataGridColumn { Header = "City" },
                    new DataGridColumn { Header = "Landmark" },
                    new DataGridColumn { Header = "PinCode" },
                    new DataGridColumn { Header = "Mobile1" },
                    new DataGridColumn { Header = "Mobile2" },
                    new DataGridColumn { Header = "Email" },
                    //new DataGridColumn { Header = "City" }
                };
            await GenerateGridColumns();
            //if (string.IsNullOrEmpty(_locationInformation.LocationLabel))
            //{
            //    _locationInformation.LocationLabel = $"Select {_appSettings?.LocationLevel}";
            //}

            //if (!string.IsNullOrEmpty(_locationInformation.LocationJson))
            //{
            //    DeserializeJsonLocation();
            //}
            ShowroomDetailsCopy = ShowroomDetails;
            TotalStoreCount = ShowroomDetails.Count();
            await Task.CompletedTask;
            if (TabName == StoreConstants.Confirmed)
            {
                OriginalstoreAdditionalInfoCMI.ShowroomDetails = CommonFunctions.ConvertToJson(ShowroomDetails);
                OriginalstoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.ShowroomDetails; ;
            }

        }
        //private void OnLocationChange(Winit.Modules.Location.Model.Interfaces.ILocationData locationMasterForUI)
        //{
        //    ViewLocation = false;
        //    if (locationMasterForUI == null)
        //    {
        //        _locationInformation.LocationLabel = $"Select {_appSettings.LocationLevel}";
        //        _locationInformation.LocationUID = string.Empty;
        //        LocationMasters = null;
        //    }
        //    else
        //    {
        //        SelectedLocation = locationMasterForUI;
        //        _locationInformation.LocationLabel = locationMasterForUI.PrimaryLabel;
        //        _locationInformation.LocationUID = locationMasterForUI.PrimaryUid;
        //        _locationInformation.LocationJson = locationMasterForUI.JsonData;

        //        DeserializeJsonLocation();
        //    }

        //}
        //protected void DeserializeJsonLocation()
        //{
        //    LocationMasters = JsonConvert.DeserializeObject<List<Winit.Modules.Location.Model.Classes.LocationMaster>>(_locationInformation.LocationJson);
        //    if (LocationMasters != null)
        //    {
        //        LocationMasters = LocationMasters.OrderBy(p => p.Level).ToList();
        //    }
        //}

        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn {Header = "Num Of Stores", GetValue = s => ((IStoreShowroom)s)?.NoOfStores??0},
                new DataGridColumn {Header = "Email", GetValue = s => ((IStoreShowroom)s)?.Email ?? "N/A"},
                new DataGridColumn {Header = "Mobile Number ", GetValue = s => ((IStoreShowroom)s)?.Mobile1 ?? "N/A"},
                new DataGridColumn {Header = "City", GetValue = s => ((IStoreShowroom)s)?.City ?? "N/A"},
                new DataGridColumn {Header = "Address 1", GetValue = s => ((IStoreShowroom)s)?.Address1 ?? "N/A"},
                new DataGridColumn {Header = "Address 2", GetValue = s => ((IStoreShowroom)s)?.Address2 ?? "N/A"},
                new DataGridColumn
                {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = item => OnEditClick((IStoreShowroom)item)

                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                        Action = item => OnDeleteClick((IStoreShowroom)item)

                    }
                }
            }
             };
        }
        public void OnEditClick(IStoreShowroom storeShowroom)
        {
            IStoreShowroom duplicateStoreRoom = (storeShowroom as StoreShowroom).DeepCopy()!;
            SelectedShowroom = duplicateStoreRoom;
            OnEditClickInvokeAddressDropdowns.InvokeAsync(SelectedShowroom);
            IsEditShowroomDetails = true; //SelectedShowroom
            ButtonName = "Update";
            FocusOnContactName();
            StateHasChanged();
        }
        public async Task OnDeleteClick(IStoreShowroom storeShowroom)
        {
            if (await _AlertMessgae.ShowConfirmationReturnType("Delete", "Are you sure want to Delete this?"))
            {
                ShowroomDetails.Remove(storeShowroom);
                ShowroomDetailsCopy.Remove(storeShowroom);
                for (int i = 0; i < ShowroomDetails.Count; i++)
                {
                    ShowroomDetails[i].NoOfStores = i + 1;
                    ShowroomDetailsCopy[i].NoOfStores = i + 1;
                }
                IsDelete = false;
                await SaveUpdateDeleteShowroomDetails();
                await OnClean();
                TotalStoreCount = ShowroomDetails.Count();

            }
        }
        private ElementReference StoreInput;
        private async void FocusOnContactName()
        {
            await StoreInput.FocusAsync();
        }
        public async Task GetAllShowRoomDetails()
        {
            IsShowAllShowroom = !IsShowAllShowroom;

        }


        public async Task ImportShowroomDetails()
        {
            IsAddPopUp = true;
            validationResult = null; // Clear validation result

        }
        protected async Task OnClean()
        {

            SelectedShowroom = new StoreShowroom
            {
                Address1 = string.Empty,
                Address2 = string.Empty,
                Address3 = string.Empty,
                Landmark = string.Empty,
                PinCode = string.Empty,
                Mobile1 = string.Empty,
                Mobile2 = string.Empty,
                City = string.Empty,
                Email = string.Empty,
                State = string.Empty,
                Locality = string.Empty,
                Branch = string.Empty,
            };
            DeselectLocationDropDowns();
            IsEditPage = false;
            // ButtonName = "Add";
            ButtonName = "Save";
            StateHasChanged();
        }
        public string ShowroomsJson { get; set; }

        public int TotalStoreCount = 0;
        protected void GetShowRoomJson()
        {
            //TotalStoreCount = ShowroomDetails.Sum(srm => srm.NoOfStores);
            //ShowroomDetails.ForEach(srm => srm.NoOfStores += TotalStoreCount);
            ShowroomsJson = JsonConvert.SerializeObject(ShowroomDetails);
        }

        public void AddShowroomDetails()
        {
            if (ValidateAllFields())
            {
                isNewDataAdded = true;


                if (!IsEditShowroomDetails)
                {
                    TotalStoreCount = ShowroomDetails.Count();
                    // Ensure the SelectedShowroom is not null
                    if (SelectedShowroom != null)
                    {
                        // Add the new showroom to the list
                        ShowroomDetails.Add(new StoreShowroom
                        {
                            NoOfStores = TotalStoreCount + 1,
                            Address1 = SelectedShowroom.Address1,
                            Address2 = SelectedShowroom.Address2,
                            Address3 = SelectedShowroom.Address3,
                            Landmark = SelectedShowroom.Landmark,
                            PinCode = SelectedShowroom.PinCode,
                            Mobile1 = SelectedShowroom.Mobile1,
                            Mobile2 = SelectedShowroom.Mobile2,
                            Email = SelectedShowroom.Email,
                            State = SelectedShowroom.State,
                            City = SelectedShowroom.City,
                            Locality = SelectedShowroom.Locality,
                            Branch = SelectedShowroom.Branch,
                        });

                        // Optionally, clear the SelectedShowroom
                        SelectedShowroom = new StoreShowroom();
                        DeselectLocationDropDowns();
                        TotalStoreCount = TotalStoreCount + 1;
                    }
                }
                int index = ShowroomDetails.FindIndex(s => s.NoOfStores == SelectedShowroom.NoOfStores);

                // If the showroom is found in the list
                if (index != -1)
                {
                    // Replace the original showroom with the edited showroom
                    ShowroomDetails[index] = SelectedShowroom;
                }
                IsEditShowroomDetails = false;
                SelectedShowroom = new StoreShowroom();
            }
            else
            {
                isNewDataAdded = false;

            }
        }
        private void DeselectLocationDropDowns()
        {
            StateselectionItems.ForEach(p => p.IsSelected = false);
            CityselectionItems.ForEach(p => p.IsSelected = false);
            LocalityselectionItems.ForEach(p => p.IsSelected = false);
            BranchselectionItems.ForEach(p => p.IsSelected = false);
            PinCodeselectionItems.ForEach(p => p.IsSelected = false);
        }

        private async Task SaveOrUpdate()
        {
            IsDelete = false;
            if (!isNewDataAdded)
            {
                showPopup = true; // Show popup if new data hasn't been added
                return;
            }
            else
            {
                isNewDataAdded = false;
                await SaveUpdateDeleteShowroomDetails();
            }
        }
        private void ClosePopup()
        {
            showPopup = false;
        }
        public async Task SaveUpdateDeleteShowroomDetails()
        {
            IsSaveAttempted = true;
            try
            {
                if (ShowroomDetails != null)
                {
                    GetShowRoomJson();
                    storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.ShowroomDetails;
                    storeAdditionalInfoCMI.ShowroomDetails = ShowroomsJson;
                    storeAdditionalInfoCMI.Action = IsDelete ? "Delete" : "Save";
                    if (TabName == StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                        await SaveOrUpdateShowroom.InvokeAsync(storeAdditionalInfoCMI);

                    }
                    else if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                    }
                    else
                    {
                        await SaveOrUpdateShowroom.InvokeAsync(storeAdditionalInfoCMI);
                    }

                    await GenerateGridColumns();
                }
                IsSuccess = true;
                IsExcelUpload = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private string MobilevalidationMessage = string.Empty;
        private void ValidateMobileNumber(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || input.Length != 10)
            {
                MobilevalidationMessage = "Mobile number must be exactly 10 digits.";
            }
            else
            {
                MobilevalidationMessage = string.Empty;
            }
        }
        public bool ValidateAllFields()
        {
            ValidationMessage = null;
            EmailValidationMessage = null;

            if (string.IsNullOrWhiteSpace(SelectedShowroom.Address1) ||
                string.IsNullOrWhiteSpace(SelectedShowroom.Mobile1) ||
                !IsValidMobileNumber(SelectedShowroom.Mobile1) ||
                string.IsNullOrWhiteSpace(SelectedShowroom.PinCode) ||
                !IsValidPinCode(SelectedShowroom.PinCode) ||
                string.IsNullOrWhiteSpace(SelectedShowroom.City) ||
                string.IsNullOrWhiteSpace(SelectedShowroom.Email) ||
                !IsValidEmail(SelectedShowroom.Email))
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(SelectedShowroom.Address1))
                {
                    ValidationMessage += "Address1, ";
                }

                if (string.IsNullOrWhiteSpace(SelectedShowroom.Mobile1) || !IsValidMobileNumber(SelectedShowroom.Mobile1))
                {
                    ValidationMessage += "Mobile, ";
                    EmailValidationMessage = "Please enter a valid mobile1 number.";
                }
                if (string.IsNullOrWhiteSpace(SelectedShowroom.Email) || !IsValidEmail(SelectedShowroom.Email))
                {
                    ValidationMessage += "Email, ";
                    EmailValidationMessage = "Please enter a valid email address.";
                }
                if (string.IsNullOrWhiteSpace(SelectedShowroom.PinCode) || !IsValidPinCode(SelectedShowroom.PinCode))
                {
                    ValidationMessage += "PinCode, ";
                    EmailValidationMessage = "Please enter a valid PinCode address.";
                }

                if (string.IsNullOrWhiteSpace(SelectedShowroom.City))
                {
                    ValidationMessage += "City, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return false;
            }
            else
            {
                return true;
            }
        }
        private void ValidateEmail()
        {
            if (!IsValidEmail(SelectedShowroom.Email))
            {
                EmailValidationMessage = "Please enter a valid email address.";
            }
            else
            {
                EmailValidationMessage = null;
            }

        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }
        private bool IsValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return false;
            }

            // Example: Mobile number validation for 10-digit numbers starting with a digit between 6-9
            var mobilePattern = @"^[6-9]\d{9}$";
            return Regex.IsMatch(mobileNumber, mobilePattern);
        }
        private bool IsValidPinCode(string pinCode)
        {
            if (string.IsNullOrWhiteSpace(pinCode))
            {
                return false;
            }

            // Example: PIN code validation for 6-digit numbers
            var pinCodePattern = @"^\d{6}$";
            return Regex.IsMatch(pinCode, pinCodePattern);
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, bool> ColumnStatuses { get; set; } = new Dictionary<string, bool>();
        }


        public ValidationResult validationResult;

        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
        }

        private async Task UploadShowroomDetails()
        {
            try
            {
                var validator = new ExcelValidator();
                validationResult = validator.IsExcelFile(selectedFile);
                if (selectedFile != null && validationResult.IsValid)
                {
                    using (var stream = selectedFile.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            if (!ExcelFileContainsData(memoryStream))
                            {
                                _tost.Add("Error", "File has empty cells. Please fill and Re-upload.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                                return;
                            }
                            validationResult = validator.ValidateExcelFile(memoryStream);

                            if (validationResult.IsValid)
                            {
                                // Proceed with processing the data
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
                                            if (!string.IsNullOrWhiteSpace(header))
                                            {
                                                rowDict[header] = value;
                                            }
                                        }

                                        var showroom = Activator.CreateInstance<StoreShowroom>();
                                        foreach (var kvp in rowDict)
                                        {
                                            var property = showroom.GetType().GetProperty(kvp.Key);
                                            if (property != null && property.CanWrite)
                                            {
                                                var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                                                property.SetValue(showroom, convertedValue);
                                            }
                                        }
                                        SelectedShowroom = showroom;
                                        if (ValidateAllFields())
                                        {
                                            validationResult = validator.IsDuplicateRecord(ShowroomDetailsCopy, showroom);
                                            if (!validationResult.IsValid)
                                            {
                                                Message = "Duplicate record found. Please remove and upload";
                                                await CreateErrorExcelFile();
                                                StateHasChanged();
                                                return;
                                            }
                                            else
                                            {
                                                TotalStoreCount = ShowroomDetailsCopy.Count + 1;
                                                showroom.NoOfStores = TotalStoreCount;
                                                ShowroomDetailsCopy.Add(showroom);
                                            }
                                        }
                                        else
                                        {

                                        }
                                        SelectedShowroom = new StoreShowroom();
                                    }

                                    // Save showroomList to your database or process as needed
                                    // Example: await _dbContext.AddRangeAsync(showroomList);
                                    // await _dbContext.SaveChangesAsync();
                                    ShowroomDetails = ShowroomDetailsCopy;
                                    isNewDataAdded = true;
                                    IsExcelUpload = true;
                                    IsAddPopUp = false; // Close the popup on successful save
                                }
                            }
                            else
                            {
                                Message = "Invalid Format. Please upload a valid File.";
                                StateHasChanged(); // Trigger re-render to show validation errors
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Message = "Only .xls and .xlsx files are allowed";
                    StateHasChanged();
                    return;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }
        private bool ExcelFileContainsData(MemoryStream stream)
        {
            try
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

                    if (worksheet.Dimension.End.Row > 1)
                    {
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
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            
        }
        public async Task CreateErrorExcelFile()
        {
            try
            {
                ShowroomDetailsErrorList = await UploadExcelFile<IStoreShowroom>(selectedFile);

                // Initialize a list to track processed records
                var processedRecords = new List<IStoreShowroom>();

                foreach (var record in ShowroomDetailsErrorList)
                {
                    // If this record has already been processed, skip it
                    if (processedRecords.Contains(record))
                        continue;

                    // Find duplicates excluding the original one
                    List<IStoreShowroom> duplicates = ShowroomDetailsErrorList
                        .Where(r => r != record &&
                                    (r.Mobile1 == record.Mobile1 ||
                                     r.Email == record.Email ||
                                     r.PinCode == record.PinCode ||
                                     r.Address1 == record.Address1))
                        .ToList();
                    
                    List<IStoreShowroom> duplicates2 = ShowroomDetailsErrorList
                                                        .Where(r => ShowroomDetails.Any(p =>
                                                            p.Address1 == r.Address1 ||
                                                            p.Email == r.Email ||
                                                            p.PinCode == r.PinCode ||
                                                            p.Mobile1 == r.Mobile1))
                                                        .ToList();

                    List<IStoreShowroom> allDuplicates = duplicates
                                                            .Union(duplicates2) // Ensures only unique items are added
                                                            .ToList();
                    //if
                    //List<IStoreShowroom> duplicates2 = ShowroomDetailsErrorList
                    //                                        .Where(r => ShowroomDetails.Any(p =>
                    //                                            p.Address1 == r.Address1 ||
                    //                                            p.Email == r.Email ||
                    //                                            p.PinCode == r.PinCode || r.Mobile1 == record.Mobile1))
                    //                                        .ToList();

                    // Mark only duplicates, skip the original
                    if (allDuplicates.Any())
                    {
                        // Collect the duplicate fields for this record

                        // Check which fields are duplicated
                        foreach (var duplicate in allDuplicates)
                        {
                            //List<string> duplicateFields = new List<string>();
                            //if (duplicate.Mobile1 == record.Mobile1)
                            //    duplicateFields.Add("Mobile1");

                            //if (duplicate.Email == record.Email)
                            //    duplicateFields.Add("Email");

                            //if (duplicate.Address1 == record.Address1)
                            //    duplicateFields.Add("Address1");

                            //if (duplicate.PinCode == record.PinCode)
                            //    duplicateFields.Add("PinCode");

                            // Mark the duplicates only, skip the original
                            if (!processedRecords.Contains(duplicate))
                            {
                                duplicate.DuplicateStatus = "Duplicate";
                                //duplicate.DuplicateStatus = $"Duplicate in: {string.Join(", ", duplicateFields)}";
                                processedRecords.Add(duplicate); // Add to processed list
                            }
                        }
                    }
                }

                // Code to save the Excel file (similar to the previous example)
                StateHasChanged();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }




        private void CancelShowroomDetails()
        {
            IsAddPopUp = false;
            Message = string.Empty;
            validationResult = null; // Clear validation result
        }
        //private void CancelShowroomDetails()
        //{
        //    selectedFile = null;
        //    ShowroomDetails.Clear();
        //    ShowroomsJson = string.Empty;
        //    IsAddPopUp = false;
        //}
        private bool isChecked;
        private bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    CheckboxChanged();
                }
            }
        }
        public StoreShowroom MapAddressToShowroom(IAddress address)
        {
            var storeShowroom = new StoreShowroom
            {
                State = address.State,
                City = address.City,
                Locality = address.Locality,
                PinCode = address.ZipCode,
                Branch = address.BranchUID,
            };
            return storeShowroom;
        }
        private void CheckboxChanged()
        {
            if (IsChecked)
            {
                IAddress? address = ShiptoAddresses.FirstOrDefault(e => e.IsDefault);
                if (address != null)
                {
                    SelectedShowroom.Address1 = address.Line1;
                    SelectedShowroom.Address2 = address.Line2;
                    SelectedShowroom.Address3 = address.Line3;
                    SelectedShowroom.PinCode = address.ZipCode;
                    SelectedShowroom.Mobile1 = address.Mobile1;
                    SelectedShowroom.Mobile2 = address.Mobile2;
                    SelectedShowroom.Email = address.Email;
                    SelectedShowroom.State = address.State;
                    SelectedShowroom.City = address.City;
                    SelectedShowroom.Locality = address.Locality;
                    SelectedShowroom.Branch = address.BranchUID;
                    OnEditClickInvokeAddressDropdowns.InvokeAsync(MapAddressToShowroom(address));
                }

                else
                {
                    ShowNoPrimaryAddressPopup();

                    SelectedShowroom.Address1 = string.Empty;
                    // SelectedAddress.Name = string.Empty; 
                    SelectedShowroom.Address2 = string.Empty;
                    SelectedShowroom.Address3 = string.Empty;
                    SelectedShowroom.PinCode = string.Empty;
                    SelectedShowroom.Mobile1 = string.Empty;
                    SelectedShowroom.Mobile2 = string.Empty;
                    SelectedShowroom.Email = string.Empty;
                    DeselectLocationDropDowns();
                }
                StateHasChanged();
            }
            else
            {
                SelectedShowroom = new StoreShowroom();
                DeselectLocationDropDowns();
            }
        }
        private void ShowNoPrimaryAddressPopup()
        {
            isChecked = false;
            // Implement logic to show a popup or modal
            // For example, you can set a flag to control the visibility of a modal
            IsNoPrimaryAddressPopupVisible = true;
        }
        private bool IsNoPrimaryAddressPopupVisible { get; set; } = false;

        private void OnPrimaryAddressClosePopup()
        {
            IsNoPrimaryAddressPopupVisible = false;
            StateHasChanged();
        }
        private async Task ExportToExcel(List<IStoreShowroom> ShowRoomList)
        {
            try
            {
                List<IStoreShowroom> ShowroomDetailsExcel = new List<IStoreShowroom>();
                if (ShowRoomList.Count > 0 )
                {
                    ShowroomDetailsExcel = ShowRoomList;
                }
                else
                {
                    ShowroomDetailsExcel = ShowroomDetails;
                }
                string fileName = "ShowRoomDetails";
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("ShowRoom Details");
                    if (ShowRoomList.Count == 0)
                    {
                        // Add headers
                        worksheet.Cell(1, 1).Value = "No of Stores";
                        worksheet.Cell(1, 2).Value = "Email";
                        worksheet.Cell(1, 3).Value = "Mobile Number";
                        worksheet.Cell(1, 4).Value = "State";
                        worksheet.Cell(1, 5).Value = "Branch";//hi bro
                        worksheet.Cell(1, 6).Value = "Address1";
                        worksheet.Cell(1, 7).Value = "Address2";

                        // Populate the Excel worksheet with your data from elem
                        for (int i = 0; i < ShowroomDetailsExcel.Count; i++)
                        {
                            var row = i + 2;
                            worksheet.Cell(row, 1).Value = ShowroomDetailsExcel[i].NoOfStores;
                            worksheet.Cell(row, 2).Value = ShowroomDetailsExcel[i].Email ?? "N/A";
                            worksheet.Cell(row, 3).Value = ShowroomDetailsExcel[i].Mobile1 ?? "N/A";
                            worksheet.Cell(row, 4).Value = ShowroomDetailsExcel[i].State ?? "N/A";
                            worksheet.Cell(row, 5).Value = ShowroomDetailsExcel[i].Branch ?? "N/A";
                            worksheet.Cell(row, 6).Value = ShowroomDetailsExcel[i].Address1 ?? "N/A";
                            worksheet.Cell(row, 7).Value = ShowroomDetailsExcel[i].Address2 ?? "N/A";
                        }
                    }
                    else
                    {
                        worksheet.Cell(1, 1).Value = "Address1";
                        worksheet.Cell(1, 2).Value = "Address2";
                        worksheet.Cell(1, 3).Value = "Address3";
                        worksheet.Cell(1, 4).Value = "Landmark";
                        worksheet.Cell(1, 5).Value = "PinCode";//hi bro
                        worksheet.Cell(1, 6).Value = "Mobile1";
                        worksheet.Cell(1, 7).Value = "Mobile2";
                        worksheet.Cell(1, 8).Value = "Email";
                        worksheet.Cell(1, 9).Value = "Status";
                        worksheet.Cell(1, 9).Style.Fill.BackgroundColor = XLColor.LightPink;

                        // Populate the Excel worksheet with your data from elem
                        for (int i = 0; i < ShowroomDetailsExcel.Count; i++)
                        {
                            var row = i + 2;
                            worksheet.Cell(row, 1).Value = ShowroomDetailsExcel[i].Address1 ?? "N/A";
                            worksheet.Cell(row, 2).Value = ShowroomDetailsExcel[i].Address2 ?? "N/A";
                            worksheet.Cell(row, 3).Value = ShowroomDetailsExcel[i].Address3 ?? "N/A";
                            worksheet.Cell(row, 4).Value = ShowroomDetailsExcel[i].Landmark ?? "N/A";
                            worksheet.Cell(row, 5).Value = ShowroomDetailsExcel[i].PinCode ?? "N/A";
                            worksheet.Cell(row, 6).Value = ShowroomDetailsExcel[i].Mobile1 ?? "N/A";
                            worksheet.Cell(row, 7).Value = ShowroomDetailsExcel[i].Mobile2 ?? "N/A";
                            worksheet.Cell(row, 8).Value = ShowroomDetailsExcel[i].Email ?? "N/A";
                            worksheet.Cell(row, 9).Value = ShowroomDetailsExcel[i].DuplicateStatus ?? "Ok";
                            if (!string.IsNullOrEmpty(ShowroomDetailsExcel[i].DuplicateStatus))
                            {
                                // Apply light pink background for duplicates
                                worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.LightPink;
                            }
                            else
                            {
                                // Apply green background for non-duplicates
                                worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.LightGreen;
                            }
                        }
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
        public class ExcelValidator
        {
            private readonly List<string> requiredColumns = new List<string>
    {
         "Address1", "Address2", "Address3", "City", "Landmark", "PinCode", "Mobile1", "Mobile2", "Email"
    };
            public ValidationResult IsExcelFile(IBrowserFile file)
            {
                // Check if the file extension is .xlsx or .xls
                var validationResult = new ValidationResult();
                validationResult.IsValid = true;
                var fileExtension = Path.GetExtension(file.Name).ToLower();
                bool res = fileExtension == ".xlsx" || fileExtension == ".xls";
                if (res)
                {
                    return validationResult;
                }
                else
                {
                    validationResult.IsValid = false;
                    return validationResult;
                }
            }
            public ValidationResult IsDuplicateRecord(List<IStoreShowroom> ShowroomDetails, StoreShowroom showroom)
            {
                var validationResult = new ValidationResult();
                validationResult.IsValid = true;
                if (ShowroomDetails.Any(p => p.Mobile1 == showroom.Mobile1 || p.Email == showroom.Email))
                {
                    validationResult.IsValid = false;
                    return validationResult;
                }
                return validationResult;
            }

            public ValidationResult ValidateExcelFile(Stream stream)
            {
                var validationResult = new ValidationResult();
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        validationResult.IsValid = false;
                        return validationResult; // Worksheet is empty
                    }

                    // Get column headers
                    var headers = new Dictionary<string, bool>();
                    for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (!string.IsNullOrWhiteSpace(header))
                        {
                            headers[header] = requiredColumns.Contains(header);
                        }
                    }

                    // Validate each row
                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        bool rowValid = true;
                        bool headersMatch = !requiredColumns.Except(headers.Keys).Any() && !headers.Keys.Except(requiredColumns).Any();

                        if (!headersMatch)
                        {
                            validationResult.IsValid = false;
                            return validationResult; // Or handle the mismatch as needed
                        }
                        foreach (var column in requiredColumns)
                        {
                            if (headers.ContainsKey(column))
                            {
                                var colIndex = headers.Keys.ToList().IndexOf(column) + 1;
                                var cellValue = worksheet.Cells[rowNumber, colIndex].Text;
                                if (string.IsNullOrWhiteSpace(cellValue))
                                {
                                    validationResult.ColumnStatuses[column] = false;
                                    rowValid = false;
                                }
                            }
                        }
                        if (rowValid)
                        {
                            // Collect valid rows if needed
                        }
                        else
                        {
                            validationResult.IsValid = false;
                        }
                    }

                    if (validationResult.ColumnStatuses.Values.All(status => status))
                    {
                        validationResult.IsValid = true;
                    }
                }

                return validationResult;
            }
        }
        public async Task<List<T>> UploadExcelFile<T>(IBrowserFile file)
        {
            List<T> result = new List<T>();

            try
            {
                _loadingService.ShowLoading();
                // showDiv = false;

                //var file = e.File;
                if (file != null)
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
                                    _loadingService.HideLoading();
                                }
                                return result;
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
                    return default;
                }
            }
            catch (Exception ex)
            {
                return default;
                // _loadingService.HideLoading();
                // isLoading = false;
                //  await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["excel_error"]);
            }
            finally
            {
            }



        }

        public async Task StateSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedShowroom.State = selecetedValue?.Code;
                SelectedShowroom.City = string.Empty;
                SelectedShowroom.Locality = string.Empty;
                SelectedShowroom.PinCode = string.Empty;
                SelectedShowroom.Branch = string.Empty;
                await StateSelected.InvokeAsync(new List<string> { selecetedValue.UID });
                StateHasChanged();
            }
        }
        public async Task CitySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedShowroom.City = selecetedValue?.Code;
                await CitySelected.InvokeAsync(new List<string> { selecetedValue.UID });
                StateHasChanged();
            }
        }
        public async Task LocalitySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedShowroom.Locality = selecetedValue?.Code;
                await LocalitySelected.InvokeAsync(new List<string> { selecetedValue.UID });
                StateHasChanged();
            }
        }
        public async Task PinCodeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedShowroom.PinCode = selecetedValue?.Code;
                StateHasChanged();
            }
        }
        public async Task BranchSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedShowroom.Branch = selecetedValue?.UID;
                StateHasChanged();
            }
        }
        #region Change RequestLogic

        public async Task RequestChange()
        {
            try
            {
                // Compare original and updated objects and get the change records
                //var changeRecords = CommonFunctions.CompareObjects(OriginalstoreAdditionalInfoCMI, storeAdditionalInfoCMI);

                //// Validate if changeRecords is not null
                //    if (changeRecords == null)
                //    {
                //        throw new InvalidOperationException("Comparison result should not be null.");
                //    }

                // Create a list of change record DTOs
                List<IChangeRecordDTO> changeRecordDTOs = new List<IChangeRecordDTO>
                    {
                        new ChangeRecordDTO
                        {
                            Action = OnboardingScreenConstant.Update,
                            ScreenModelName = OnboardingScreenConstant.ShowroomDetails,
                            UID = StoreAdditionalInfoCMIUid,
                            //ChangeRecords = CommonFunctions.GetChangedData(changeRecords)
                            ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalstoreAdditionalInfoCMI, storeAdditionalInfoCMI)!)
                        }
                    }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

                // If there are any change records, convert to JSON and invoke the change request
                if (changeRecordDTOs.Count > 0)
                {
                    var changeRecordDTOInJson = CommonFunctions.ConvertToJson(changeRecordDTOs);
                    await InsertDataInChangeRequest.InvokeAsync(changeRecordDTOInJson);
                }

                // Clear the list for future requests
                changeRecordDTOs.Clear();
            }
            catch (ArgumentException argEx)
            {
                // Handle specific ArgumentException
                await _alertService.ShowErrorAlert("Compare Object", argEx.Message);
                // Consider logging this error using a logging framework
            }
            catch (InvalidOperationException invOpEx)
            {
                // Handle specific InvalidOperationException
                Console.WriteLine($"Invalid operation: {invOpEx.Message}");
                // Consider logging this error
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Consider logging this error
            }
        }

        #endregion
    }
}
