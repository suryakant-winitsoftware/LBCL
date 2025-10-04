using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Contact.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
using Winit.UIModels.Common.Store;

namespace Winit.UIComponents.Mobile.Store
{
    public partial class StoreContacts : ComponentBase
    {
        [Parameter] public EventCallback<Winit.Modules.Store.Model.Classes.StoreSavedAlert> Status { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }
        [Parameter] public bool IsNewStore { get; set; }
        //ContactPersonDetailsModel Contact { get; set; } = new ContactPersonDetailsModel();
        HttpClient http = new HttpClient();

       // private Winit.Modules.Contact.Model.Interfaces.IContact Contact;

        public string SaveOrUpdate { get; set; }

        
       
        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(LinkedItemUID))
            {
                await LoadAfterParameterSet();
            }
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        protected async Task LoadAfterParameterSet()
        {
           
            if (IsNewStore)
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
            }
            else
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Update;
                try
                {
                    Contact = await ContactBL.GetContactDetailsByUID(LinkedItemUID);
                    if (Contact != null)
                    {
                        if (string.IsNullOrEmpty(Contact.UID))
                        {
                            IsNewStore = true;
                            SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                        }
                    }
                    else
                    {
                        IsNewStore = true;
                        Contact = new Contact();
                        SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }

        public async void Save()
        {
            Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert = new();
            if (IsNewStore)
            {
                Contact.UID = Guid.NewGuid().ToString();
                Contact.LinkedItemUID = LinkedItemUID;
                Contact.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                Contact.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                Contact.CreatedTime = DateTime.Now;
                Contact.Title = "Prem";
                Contact.Description = "Nothing";
                Contact.Designation = "Devoloper";

                Contact.ModifiedTime = DateTime.Now;
                Contact.ServerAddTime = DateTime.Now;
                Contact.ServerModifiedTime = DateTime.Now;


                var res = await ContactBL.CreateContactDetails(Contact);
                if (res > 0)
                {
                    await Status.InvokeAsync(storeSavedAlert);

                }
            }
            else
            {

                Contact.ModifiedTime = DateTime.Now;
                Contact.ServerModifiedTime = DateTime.Now;
                var res = await ContactBL.UpdateContactDetails(Contact);
                if (res > 0)
                    await Status.InvokeAsync(storeSavedAlert);

            }
        }


       

    }
}
