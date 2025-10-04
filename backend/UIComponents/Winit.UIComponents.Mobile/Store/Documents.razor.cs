using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Address.BL.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
using Winit.UIModels.Mobile.Store;

namespace Winit.UIComponents.Mobile.Store
{
    public partial class Documents : ComponentBase
    {
        [Parameter] public EventCallback<Winit.Modules.Store.Model.Classes.StoreSavedAlert> Status { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }
        [Parameter] public bool IsNewStore { get; set; }
        public bool showDocument = false;
        public string DocumentLabel = "Select Document Type";

        private List<Winit.Shared.Models.Common.ISelectionItem> _documentType = new List<ISelectionItem>();

        public string SaveOrUpdate { get; set; }
        protected override async Task OnInitializedAsync()
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
                    _StoreDocument = await StoreDocumentBL.GetStoreDocumentDetailsByUID(LinkedItemUID);
                    if (string.IsNullOrEmpty(_StoreDocument.UID))
                    {
                        IsNewStore = true;
                        _StoreDocument =new Winit.Modules.StoreDocument.Model.Classes.StoreDocument();
                        SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            await GetListItems();
            LoadResources(null, _languageService.SelectedCulture);

        }
        
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        protected async Task GetListItems()
        {
            PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> pagedResponse = await ListHeaderBL.GetListItemsByCodes(new List<string>() { Shared.Models.Constants.ListHeaderType.Document }, true);
            if (pagedResponse != null)
            {
                if (pagedResponse.TotalCount > 0)
                {
                    foreach (var item in pagedResponse.PagedData)
                    {
                        _documentType.Add(new SelectionItem()
                        {
                            UID = item.UID,
                            Code = item.Code,
                            Label = item.Name,
                            IsSelected = false
                        }) ; 
                    }
                }
            }
        }
        public async void Save()
        {
            Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert = new();
            if (IsNewStore)
            {
                _StoreDocument.StoreUID = LinkedItemUID;
                _StoreDocument.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                _StoreDocument.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                _StoreDocument.CreatedTime = DateTime.Now;
                _StoreDocument.ModifiedTime = DateTime.Now;
                _StoreDocument.ServerAddTime = DateTime.Now;
                _StoreDocument.ServerModifiedTime = DateTime.Now;
                _StoreDocument.UID = Guid.NewGuid().ToString();
                int count = await StoreDocumentBL.CreateStoreDocumentDetails(_StoreDocument);
                if (count > 0)
                {
                    storeSavedAlert.Message = " Documents Saved";
                    storeSavedAlert.IsSaved = true;
                    await Status.InvokeAsync(storeSavedAlert);
                }
                
            }
            else
            {
                int count = await StoreDocumentBL.UpdateStoreDocumentDetails(_StoreDocument);

                if (count > 0)
                {
                    storeSavedAlert.Message = " Documents Update";
                    storeSavedAlert.IsSaved = true;
                    await Status.InvokeAsync(storeSavedAlert);
                }
                  
            }
        }
        protected void OnSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if(dropDownEvent.SelectionItems!=null)
                {
                    ISelectionItem item = dropDownEvent.SelectionItems.FirstOrDefault();
                    _StoreDocument.DocumentType = item.UID;
                    DocumentLabel=string.IsNullOrEmpty(item.Label)?DocumentLabel:item.Label;
                }
            }

            showDocument = false;
        }

       

    }
}
