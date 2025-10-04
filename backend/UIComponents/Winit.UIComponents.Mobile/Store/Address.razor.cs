using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Address.BL.Classes;
using Winit.Modules.Address.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
using Winit.UIModels.Common.Store;
using static System.Net.WebRequestMethods;

namespace Winit.UIComponents.Mobile.Store
{
    public partial class Address : ComponentBase
    {
        [Parameter]public EventCallback<Winit.Modules.Store.Model.Classes.StoreSavedAlert> Status { get; set; }
        
        [Parameter] public string Type { get; set; }
        [Parameter] public string? LinkedItemUID { get; set; }
        [Parameter] public Winit.Modules.Address.Model.Interfaces.IAddress _address { get; set; }
      
        [Parameter] public EventCallback<AddressModel> OnAddressSaved { get; set; }
        [Parameter] public EventCallback<Winit.Modules.Address.Model.Interfaces.IAddress> AddressSameForBilling { get; set; }
        [Parameter] public EventCallback<Winit.Modules.Address.Model.Interfaces.IAddress> AddressSameForShipping { get; set; }
        [Parameter] public EventCallback<string> Response { get; set; }

        [Parameter] public bool IsNewStore { get; set; }

        HttpClient http = new HttpClient();

        [Parameter] public bool BillAddressSameAsStoreAddress { get; set; } =false;
        [Parameter] public bool ShipAddressSameAsStoreAddress { get; set; } = false;

        public string SaveOrUpdate { get; set; }


        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(Type))
            {
                if (Type == Shared.Models.Constants.ListHeaderType.Store)
                {
                    if(IsNewStore!=null && !string.IsNullOrEmpty(LinkedItemUID))
                    {
                        await LoadAfterParameterSet();
                    }
                }
                else if (Type == Shared.Models.Constants.ListHeaderType.Billing)
                {
                    if (IsNewStore != null && !string.IsNullOrEmpty(LinkedItemUID)&& BillAddressSameAsStoreAddress != null )
                    {
                        if (BillAddressSameAsStoreAddress)
                        {
                            if (_address.UID == null)
                            {
                                await LoadAfterParameterSet();
                            }
                        }
                        else
                        {
                            await LoadAfterParameterSet();
                        }
                        
                    }
                }
                else if (Type == Shared.Models.Constants.ListHeaderType.Shipping)
                {
                    if (IsNewStore != null && !string.IsNullOrEmpty(LinkedItemUID)&& ShipAddressSameAsStoreAddress != null )
                    {
                        if (ShipAddressSameAsStoreAddress)
                        {
                            if (_address.UID == null)
                            {
                                await LoadAfterParameterSet();
                            }
                        }
                        else
                        {
                            await LoadAfterParameterSet();
                        }
                        
                    }
                }
            }


            LoadResources(null, _languageService.SelectedCulture);
        }

        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        //protected override void OnAfterRender(bool firstRender)
        //{
        //    if (firstRender)
        //    {
        //        // Code to run after the first render
        //    }
        //    else
        //    {
        //        if (!BillAddressSameAsStoreAddress || !ShipAddressSameAsStoreAddress)
        //        {
        //            LoadAfterParameterSet();
        //        }

        //    }
        //}
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
                    _address = await AddressBL.GetAddressDetailsByUID(LinkedItemUID);
                    if (_address != null)
                    {
                        if (string.IsNullOrEmpty(_address.UID))
                        {
                            IsNewStore = true;
                            SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                        }
                    }
                    else
                    {
                        IsNewStore = true;
                        _address = new Modules.Address.Model.Classes.Address();
                        SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task SaveAddress()
        {
            Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert = new();
            if (IsNewStore)
            {
                _address.UID = Guid.NewGuid().ToString();
                _address.LinkedItemUID = LinkedItemUID;
                _address.Type = Type;
                _address.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                _address.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                _address.CreatedTime = DateTime.Now;
                _address.ModifiedTime = DateTime.Now;
                _address.ServerAddTime = DateTime.Now;
                _address.ServerModifiedTime = DateTime.Now;

                int res = await AddressBL.CreateAddressDetails(_address);
                if (res > 0)
                {
                    storeSavedAlert.Message = Type + " Saved Successfully";
                    storeSavedAlert.IsSaved=true;
                    await Status.InvokeAsync(storeSavedAlert);
                }
                else
                {
                    storeSavedAlert.Message = Type + " Not saved";
                    storeSavedAlert.IsSaved = false;
                    await Status.InvokeAsync(storeSavedAlert);
                }
            }
            else
            {
                _address.ModifiedTime = DateTime.Now;
                _address.ServerModifiedTime = DateTime.Now;

                int res = await AddressBL.UpdateAddressDetails(_address);
                if (res > 0)
                {
                    storeSavedAlert.Message = Type + " Updated Successfully";
                    storeSavedAlert.IsSaved = true;
                    await Status.InvokeAsync(storeSavedAlert);
                }
                else
                {
                    storeSavedAlert.Message = Type + " Not Updated";
                    storeSavedAlert.IsSaved = false;
                    await Status.InvokeAsync(storeSavedAlert);
                }

            }
        }

        protected void GetAddressSameForbilling()
        {
            BillAddressSameAsStoreAddress = !BillAddressSameAsStoreAddress;
            if(BillAddressSameAsStoreAddress)
            AddressSameForBilling.InvokeAsync(_address);
        }
         protected void GetAddressSameForShipping()
        {
            ShipAddressSameAsStoreAddress = !ShipAddressSameAsStoreAddress;
            if (ShipAddressSameAsStoreAddress)
                AddressSameForShipping.InvokeAsync(_address);

        }
       
    }
}
