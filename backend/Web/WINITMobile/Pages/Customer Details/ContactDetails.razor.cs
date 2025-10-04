using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;
using Winit.Modules.Contact.Model.Constants;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Store.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;
using Contact = Winit.Modules.Contact.Model.Classes.Contact;
namespace WINITMobile.Pages.Customer_Details;

public partial class ContactDetails : BaseComponentBase
{
    [Parameter]
    public List<ISelectionItem> ContactTypeSelectionItems { get; set; } = new List<ISelectionItem>
            {
                new SelectionItem { UID = "1", Label = ContactType.Office },
                new SelectionItem { UID = "2", Label = ContactType.Residence }
            };
    public List<DataGridColumn> DataGridColumns { get; set; }
    public string ValidationMessage;
    //private bool IsSaveAttempted { get; set; } = false;
    private string EmailValidationMessage;
    private string PrimaryValidationMessage;
    public IContact SelectedContact { get; set; }
    public IContact OriginalSelectedContact { get; set; }
    [Parameter] public EventCallback<IContact> SaveOrUpdateContact { get; set; }
    [Parameter] public EventCallback<IContact> OnAddContact { get; set; }
    [Parameter] public EventCallback<IContact> OnEditContact { get; set; }
    [Parameter] public EventCallback<string> OnDelete { get; set; }
    [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
    [Parameter] public Func<Task<List<IContact>>> OnShowAllContactsClick { get; set; }
    [Parameter] public List<IContact> Contacts { get; set; } = new List<IContact>();
    [Parameter] public bool CustomerEditApprovalRequired { get; set; }
    public List<IContact> OriginalContacts = new List<IContact>();
    [Parameter] public string TabName { get; set; }
    [Parameter] public string NewlyContactUID { get; set; }
    [Parameter] public string LinkedItemUID { get; set; }

    private System.Timers.Timer _timer;
    private bool IsEditPage = false;
    public bool IsShowAllContacts { get; set; } = false;
    public bool IsInitialised { get; set; } = false;
    public string ButtonName { get; set; } = "Save";
    public bool IsSuccess { get; set; } = false;
    [Parameter] public bool IsBackBtnClicked { get; set; } = false;
    public IContact duplicateContact { get; set; } = new Contact();
    protected override async Task OnInitializedAsync()
    {
        _loadingService.ShowLoading();
        SelectedContact = _serviceProvider.CreateInstance<IContact>();

        if (SelectedContact is Contact contact)
        {
            duplicateContact = contact.DeepCopy();
            if (TabName == StoreConstants.Confirmed)
            {
                OriginalSelectedContact = contact.DeepCopy();// Create a deep copy
            }
        }

        //IsSuccess = true;
        //SelectedContact.IsDefault = true;
        GenerateGridColumns();
        IsInitialised = true;
        _loadingService.HideLoading();
        StateHasChanged();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!string.IsNullOrWhiteSpace(ValidationMessage))
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        _timer = new System.Timers.Timer(3000); // Set the timer for 3 seconds
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false; // Ensure the timer runs only once
        _timer.Start();
    }

    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        _timer = null;

        // Clear the validation message
        ValidationMessage = string.Empty;

        // Trigger a re-render
        InvokeAsync(StateHasChanged);
    }
    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "Contact Type", GetValue = s => ((IContact)s)?.Type ?? "N/A" },
            new DataGridColumn {Header = "Contact Person Name", GetValue = s => ((IContact)s)?.Name ?? "N/A" },
            new DataGridColumn {Header = "Mobile Number 1", GetValue = s => ((IContact)s)?.Mobile ?? "N/A" },
            new DataGridColumn {Header = "Mobile Number 2", GetValue = s => ((IContact)s)?.Mobile2 ?? "N/A"},
            new DataGridColumn {Header = "Email", GetValue = s => ((IContact)s)?.Email ?? "N/A" },
            new DataGridColumn {Header = "Is Primary", GetValue = s => ((IContact)s)?.IsDefault.ToString() ?? "N/A"},
            new DataGridColumn
            {
            Header = "Actions",
            IsButtonColumn = true,
            ButtonActions = new List<ButtonAction>
            {
                new ButtonAction
                {
                    ButtonType = ButtonTypes.Image,
                    URL = "Images/edit.png",
                    Action = item => OnEditClick((IContact)item)

                },
                new ButtonAction
                {
                    ButtonType = ButtonTypes.Image,
                    URL = "Images/delete.png",
                    Action = item => OnDeleteClick((IContact)item)
                }
            }
        }
         };
    }
    private ElementReference contactNameInput;
    private async void FocusOnContactName()
    {
        await contactNameInput.FocusAsync();
    }
    public void OnEditClick(IContact contact)
    {
        OriginalSelectedContact = contact;
        IContact duplicateContact = (contact as Contact).DeepCopy()!;
        SelectedContact = duplicateContact;
        IsEditPage = true;
        ButtonName = "Update";
        SetEditForContactTypeDD(SelectedContact);
        FocusOnContactName();
        StateHasChanged();
    }

    public async Task OnDeleteClick(IContact contact)
    {

        OnDelete.InvokeAsync(contact.UID);
        IsShowAllContacts = !IsShowAllContacts;
    }

    public async Task OnClean()
    {
        DeselectAllContactTypeItems();

        SelectedContact = new Contact
        {
            LinkedItemType = null,
            Name = string.Empty,
            Mobile = string.Empty,
            Mobile2 = string.Empty,
            Email = string.Empty,
            IsDefault = false
        };
        ButtonName = "Save";
        IsEditPage = false;

        StateHasChanged();
    }

    private void DeselectAllContactTypeItems()
    {
        if (ContactTypeSelectionItems != null)
        {
            foreach (var item in ContactTypeSelectionItems)
            {
                item.IsSelected = false;
            }
        }
    }

    public async Task GetAllContacts()
    {
        IsShowAllContacts = !IsShowAllContacts;
        _loadingService.ShowLoading();
        Contacts.Clear();
        Contacts.AddRange(await OnShowAllContactsClick.Invoke());
        //OriginalContacts = Contacts?
        //    .Select(contact => (IContact)((Contact)contact).DeepCopy())
        //    .ToList();

        GenerateGridColumns();
        StateHasChanged();
        _loadingService.HideLoading();
    }
    public async Task SaveOrUpdate()
    {
        //  IsSaveAttempted = true;
        if (!IsBackBtnClicked)
        {
            if (!ValidateAllFields())
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(ValidationMessage) && string.IsNullOrWhiteSpace(EmailValidationMessage))
        {
            try
            {
                if (!IsEditPage)
                {
                    await OnAddContact.InvokeAsync();
                }
                else
                {
                    await OnEditContact.InvokeAsync();

                }
                string actionType = SelectedContact.UID == null ? OnboardingScreenConstant.Create : OnboardingScreenConstant.Update;
                if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                {
                    await SaveOrUpdateContact.InvokeAsync(SelectedContact);
                    await RequestChange(actionType);
                    await GetUpdatedContacts();
                }
                else if (TabName == StoreConstants.Confirmed)
                {
                    await RequestChange(actionType);
                }
                else
                {
                    await SaveOrUpdateContact.InvokeAsync(SelectedContact);
                    await GetUpdatedContacts();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    #region Change RequestLogic
    public async Task GetUpdatedContacts()
    {
        Contacts.Clear();
        Contacts.AddRange(await OnShowAllContactsClick.Invoke());
    }
    public async Task RequestChange(string actionType = OnboardingScreenConstant.Update)
    {
        try
        {

            // Directly creating List<IChangeRecordDTO> using inline creation of ChangeRecordDTOs
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                    LinkedItemUID=LinkedItemUID,
                    Action= actionType,
                    ScreenModelName = OnboardingScreenConstant.Contact,
                    UID = SelectedContact.UID==null?(NewlyContactUID==null?Guid.NewGuid().ToString():NewlyContactUID):OriginalSelectedContact?.UID!,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalSelectedContact, SelectedContact)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

            if (ChangeRecordDTOs.Count > 0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
                OriginalSelectedContact = new Contact
                {
                    LinkedItemType = null,
                    Name = string.Empty,
                    Mobile = string.Empty,
                    Mobile2 = string.Empty,
                    Email = string.Empty,
                    IsDefault = false
                };
            }
            ChangeRecordDTOs.Clear();
        }
        catch (Exception ex) { Console.WriteLine(ex.Message.ToString()); }
    }
    #endregion
    private bool IsContactTypeSelectionValid()
    {
        return !string.IsNullOrEmpty(SelectedContact.Type);
    }
    private bool MobileValidation()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SelectedContact.Mobile) ||
                !ValidateMobileNumber(SelectedContact.Mobile, "Mobile"))
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    private bool ValidateAllFields()
    {
        ValidationMessage = null;
        EmailValidationMessage = null;
        bool isValid = true;

        if (!IsContactTypeSelectionValid() ||
            string.IsNullOrWhiteSpace(SelectedContact.Name) ||
            (MobileValidation()) ||
            !IsValidEmail(SelectedContact.Email))
        {
            ValidationMessage = "The following fields have invalid field(s)" + ": ";

            if (!IsContactTypeSelectionValid())
            {
                ValidationMessage += "Contact Type, ";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SelectedContact.Name))
            {
                ValidationMessage += "Contact Person Name, ";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SelectedContact.Mobile) || !ValidateMobileNumber(SelectedContact.Mobile, "Mobile"))
            {
                ValidationMessage += "Mobile Number 1, ";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SelectedContact.Email) || !IsValidEmail(SelectedContact.Email))
            {
                ValidationMessage += "Email, ";
                EmailValidationMessage = "Please enter a valid email address.";
                isValid = false;
            }

            ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
        }

        // Validate Mobile2 silently (no message added to ValidationMessage)
        if (!string.IsNullOrEmpty(SelectedContact.Mobile2) && !ValidateMobileNumber(SelectedContact.Mobile2, "Mobile2"))
        {
            isValid = false;
        }

        return isValid;
    }

    private string MobilevalidationMessage = string.Empty;
    private string Mobile2validationMessage = string.Empty;

    //private void ValidateMobileNumber(ChangeEventArgs e)
    //{
    //    string input = e.Value?.ToString();

    //    if (string.IsNullOrEmpty(input) || input.Length != 10)
    //    {
    //        MobilevalidationMessage = "Mobile number must be exactly 10 digits.";
    //    }
    //    else
    //    {
    //        MobilevalidationMessage = string.Empty;
    //    }
    //}
    public string ContactFieldname { get; set; }
    private bool ValidateMobileNumber(string input, string fieldName)
    {
        ContactFieldname = fieldName;
        if (ContactFieldname == "Mobile")
        {
            if (input.Length != 10)
            {
                MobilevalidationMessage = "Mobile number must be exactly 10 digits.";
                return false; // Validation failed
            }
            else
            {
                MobilevalidationMessage = string.Empty;
                return true;
            }
        }
        else if (ContactFieldname == "Mobile2")
        {
            if (input.Length != 10)
            {
                Mobile2validationMessage = "Mobile number must be exactly 10 digits.";
                return false;
            }
            else
            {
                Mobile2validationMessage = string.Empty;
                return true;
            }
        }
        else
        {
            if (ContactFieldname == "Mobile")
            {
                MobilevalidationMessage = string.Empty; // Clear validation message
            }
            else if (ContactFieldname == "Mobile2")
            {
                Mobile2validationMessage = string.Empty; // Clear validation message
            }

            return true; // Validation passed
        }
    }

    private void ValidateEmail(ChangeEventArgs e)
    {
        string email = e.Value?.ToString();

        if (!IsValidEmail(email))
        {
            EmailValidationMessage = "Please enter a valid email address.";
        }
        else
        {
            EmailValidationMessage = string.Empty;
        }
    }

    //private bool IsValidMobileNumber(string mobileNumber)
    //{
    //    if (string.IsNullOrWhiteSpace(mobileNumber))
    //    {
    //        return false;
    //    }

    //    // Example: Mobile number validation for 10-digit numbers starting with a digit between 6-9
    //    var mobilePattern = @"^[6-9]\d{9}$";
    //    return Regex.IsMatch(mobileNumber, mobilePattern);
    //}
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }
    public void OnContactTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem? selecetedValue = dropDownEvent.SelectionItems.First();
            SelectedContact.Type = selecetedValue?.Label;
        }

    }
    public void SetEditForContactTypeDD(IContact contacttype)
    {

        foreach (var item in ContactTypeSelectionItems)
        {
            if (item.Label == contacttype.Type)
            {
                item.IsSelected = true;
            }
            else
            {
                item.IsSelected = false;
            }
        }
    }
    //public bool ContactDeepCopy()
    //{
    //    if(!Contacts.Equals(duplicateContact))
    //    {
    //        return true;
    //    }
    //    return false;
    //}
    public bool AreContactsEqual()
    {
        if (ArePropertiesNotFilled(duplicateContact) && !ArePropertiesNotFilled(SelectedContact))
        {
            if (duplicateContact.Type != SelectedContact.Type ||
                 duplicateContact.Mobile != SelectedContact.Mobile ||
                 duplicateContact.Name != SelectedContact.Name ||
                 duplicateContact.Mobile2 != SelectedContact.Mobile2 ||
                 duplicateContact.Email != SelectedContact.Email)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public static bool ArePropertiesNotFilled(object obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        // Get all properties of the object's type
        PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Check each property to determine if it's not filled
        foreach (var property in properties)
        {
            // Get the value of the property
            var value = property.GetValue(obj);

            // Determine if the property is filled
            if (IsPropertyFilled(value, property.PropertyType))
            {
                return false; // If any property is filled, return false
            }
        }

        return true; // If no properties are filled, return true
    }

    private static bool IsPropertyFilled(object value, Type type)
    {
        // Check for reference types and strings
        if (type.IsClass && type != typeof(string))
        {
            // Non-string reference types: consider filled if not null
            return value != null;
        }
        else if (type == typeof(string))
        {
            // Strings: consider filled if not null, empty, or whitespace
            return !string.IsNullOrWhiteSpace((string)value);
        }
        else
        {
            // Value types: consider filled if not default
            return !IsDefaultValue(value);
        }
    }

    private static bool IsDefaultValue(object value)
    {
        if (value == null) return true;

        Type type = value.GetType();
        if (type.IsValueType)
        {
            // Get default value for the type
            object defaultValue = Activator.CreateInstance(type);
            return value.Equals(defaultValue);
        }

        return false;
    }
}
