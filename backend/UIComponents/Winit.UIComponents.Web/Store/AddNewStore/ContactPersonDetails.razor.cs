using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Text.RegularExpressions;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Web.Store;
using Winit.UIComponents.Common.LanguageResources.Web;
namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class ContactPersonDetails : ComponentBase
    {
        [Parameter] public List<IContact> Contacts { get; set; }
        [Parameter] public IContact Contact { get; set; }
        [Parameter] public EventCallback<IContact> SaveOrUpdateContact { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }
        [Parameter] public EventCallback<string> Response { get; set; }




        bool ShowContacts { get; set; } = false;

        bool isedit = false;
        public bool isEdited { get; set; } = false;

        string btnName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            btnName = @Localizer["add_contacts"];
            //await GetContacts();
        }

        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }
        public bool IsComponentEdited()
        {
            return isEdited;
        }
        IContact DefaultContactPerson { get; set; }
        public async Task ChangeIsDefault()
        {
            bool isConfirm = false;
            if (!Contact.IsDefault)
            {
                DefaultContactPerson = Contacts.Find(p => p.IsDefault == true);
                if (DefaultContactPerson == null)
                {
                    isConfirm = await _alertService.ShowConfirmationReturnType(@Localizer["confirm"], @Localizer["are_you_sure_you_want_to_make_this_contact_as_default"], @Localizer["yes"], @Localizer["no"]);
                }
                else
                {
                    isConfirm = await _alertService.ShowConfirmationReturnType(@Localizer["confirm"], $"{DefaultContactPerson.Name} {@Localizer["is_the_default_contact,"]} {@Localizer["are_you_sure_you_want_change_this_contact_as_default?"]}", @Localizer["yes"], @Localizer["no"]);
                }
            }
            else
            {
                Contact.IsDefault = !Contact.IsDefault;
            }
            if (isConfirm)
            {
                Contact.IsDefault = !Contact.IsDefault;
            }
        }
        public async Task GetContacts()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new();
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemUID", LinkedItemUID, FilterType.Equal));
            paging.FilterCriterias.Add(new FilterCriteria("LinkedItemType", "person", FilterType.Equal));

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}contact/SelectAllContactDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<Contact>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Contact>>(data);
                if (pagedResponse != null)
                {
                    Contacts = (List<IContact>)pagedResponse.PagedData;
                    if (pagedResponse.TotalCount >= 0)
                    {
                        // _totalItems = pagedResponse.TotalCount;
                    }
                }

            }

        }
        void EditContact(IContact contact)
        {
            isedit = true;
            Contact = contact;
            btnName = @Localizer["update"];
        }
        public async void AddContacts()
        {
            bool isVal = true;
            string message = string.Empty;
            //if (!string.IsNullOrEmpty(Contact.Email))
            //{
            //    var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            //    if (!regex.IsMatch(Contact.Email))
            //    {
            //        isVal = false;
            //        message += @Localizer["invalid_email_address"];
            //    }
            //}
            if (!CommonFunctions.IsValidEmail(Contact.Email))
            {
                isVal = false;
                message += @Localizer["invalid_email_address"];
            }
            if (isVal)
            {
                await SaveOrUpdateContact.InvokeAsync(Contact);
                isedit = !isedit;
                btnName = @Localizer["add_contacts"];
            }
            else
            {
              await  _alertService.ShowErrorAlert(@Localizer["error"],message);
            }
        }




        public async Task Change_MakeAsDefault(IContact contact)
        {
            foreach (var item in Contacts)
            {
                if (item.UID == contact.UID)
                {
                    item.IsDefault = !contact.IsDefault;
                }
            }
            await Update(contact);
        }

        public async Task Update(IContact contact)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}contact/UpdateContactDetails",
             HttpMethod.Put, contact);
            btnName = @Localizer["add_contacts"];
            if (apiResponse.StatusCode == 200)
            {
                await Response.InvokeAsync(@Localizer["updated_successfully"]);
            }
            else
            {
                await Response.InvokeAsync(apiResponse.StatusCode + apiResponse.ErrorMessage);
            }
        }

       
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
    }
}
