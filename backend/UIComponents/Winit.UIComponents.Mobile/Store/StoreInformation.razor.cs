using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.WebRequestMethods;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using System.Text;
using Winit.UIModels.Common.Store;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Events;
using Nest;
using Winit.Modules.Store.Model.Classes;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;


namespace Winit.UIComponents.Mobile.Store
{
    public partial class StoreInformation 
    {
        public UIModels.Common.Store.CustomerInformationModel _CustomerInformation { get; set; } = new Winit.UIModels.Common.Store.CustomerInformationModel();
        HttpClient http = new HttpClient();


        // public Modules.Store.Model.Interfaces.IStore iStore { get; set; }
        [Parameter] public EventCallback<Winit.UIModels.Mobile.Store.StoreSavedAlert> Status { get; set; }
        [Parameter] public EventCallback<string> Failure { get; set; }
        [Parameter] public string UID { get; set; }
        [Parameter] public bool IsNewStore { get; set; }

        public string SaveOrUpdate { get; set; }
       
        protected bool isShowCustomersinPopup { get; set; } = true;
        protected bool isShowCustomerChannelinPopup { get; set; } = true;
        protected bool isShowCustomerSubChannelinPopup { get; set; } = true;
        protected bool isShowClassificationinPopUp { get; set; } = true;
        protected bool isShowChaininPopUp { get; set; } = true;
        protected bool isShowGroupinPopUp { get; set; } = true;
        protected bool isShowMODinPopUp { get; set; } = true;
        protected bool isShowStoreSizeinPopUp { get; set; } = true;
        protected bool isShowVisitFrequencyinPopUp { get; set; } = true;

        protected bool Active;
        protected bool InActive;
        protected bool Blocked;
        protected bool Warehouse;
        protected bool School;

        protected string CustomerTypeLabel { get; set; } = "Select Customer Type";
        protected string CustomerChannelLabel { get; set; } = "Select Customer Channel";
        protected string CustomerSubChannelLabel { get; set; } = "Select Customer Sub Channel";
        protected string ClassificationLabel { get; set; } = "Select Classification";
        protected string ChainLabel { get; set; } = "Select Customer Chain";
        protected string CustomerGroupLabel { get; set; } = "Select Customer Group";
        protected string MODLabel { get; set; } = "Select Delivery Mode";
        protected string StoreSizeLabel { get; set; } = "Select Store Size";
        protected string VisitFrequencyLabel  = "Select Visit Frequency";

        public string status;
        public bool isShowPop { get; set; } = true;

        private List<ListitemModel> listItems { get; set; } = new List<ListitemModel>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _customerType = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _channel = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _subChannel = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _chain = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _customerGroup = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _mOD = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _storeSize = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _classification = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _visitFrequency = new List<ISelectionItem>();

        protected override async Task OnInitializedAsync()
        {

            if (IsNewStore)
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                iStore.Number = DateTime.Now.ToString("ddMMyyyyhhmmss");
                iStore.Code = "C" + DateTime.Now.ToString("ddMMyyyyhhmmss");
            }
            else
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Update;
                try
                {
                    iStore = await _storeBL.SelectStoreByUID(UID);
                    Active = iStore.Status == Shared.Models.Constants.CustomerStatus.Active ? true : false;
                    InActive = iStore.Status == Shared.Models.Constants.CustomerStatus.InActive ? true : false;
                    Blocked = iStore.Status == Shared.Models.Constants.CustomerStatus.Blocked ? true : false;
                    School=iStore.SchoolWarehouse== Shared.Models.Constants.ListHeaderType.School ? true : false;
                    Warehouse=iStore.SchoolWarehouse== Shared.Models.Constants.ListHeaderType.Warehouse ? true : false;
                    CustomerTypeLabel = string.IsNullOrEmpty(iStore.Type) ? "Select Customer Type" : iStore.Type;
                    weekDays = await WeekDaysBL.SelectWeekDaysByUID(UID);
                }
                catch(Exception ex)
                {

                }
            }
            await getListItemsAsync();
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        string ErrorMessage = string.Empty;
        private bool Validate()
        {
            bool isValidated = true;
            
            if (string.IsNullOrEmpty(iStore.OutletName))
            {
                ErrorMessage += nameof(iStore.OutletName); isValidated = false;
            }
            if (string.IsNullOrEmpty(iStore.ArabicName))
            {
                ErrorMessage += nameof(iStore.ArabicName)+","; isValidated = false;
            }
            if (string.IsNullOrEmpty(iStore.LegalName))
            {
                ErrorMessage += nameof(iStore.LegalName) + ","; isValidated = false;
            }
            return isValidated;
        }

        private async Task Save()
        {
            Winit.UIModels.Mobile.Store.StoreSavedAlert storeSavedAlert = new();
            if (Validate())
            {
                if (iStore.UID == null || iStore.UID == "")
                {
                    string UID = Guid.NewGuid().ToString();
                    iStore.UID = UID;
                    iStore.CreatedTime = DateTime.Now;
                    iStore.ModifiedTime = DateTime.Now;
                    iStore.ServerAddTime = DateTime.Now;
                    iStore.ServerModifiedTime = DateTime.Now;
                    iStore.CreatedByEmpUID = "7ee9f49f-26ea-4e89-8264-674094d805e1";
                    iStore.CompanyUID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7rrytyhtjhyyF1";
                    iStore.CreatedByJobPositionUID = "df7bc6e2-273a-4ea4-90c6-ff1670d2b477";
                    iStore.CountryUID = "b86e9f24-d8d3-42ba-9e02-bd6cfc69245f";
                    iStore.RegionUID = "f147b975-ac53-4ffb-84a8-b0bddb89e13d";
                    iStore.CityUID = "2d893d92-dc1b-5904-934c-621103a900e39784s123";
                    iStore.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                    iStore.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                    var res = await _storeBL.CreateStore(iStore);

                    weekDays.CreatedTime = DateTime.Now;
                    weekDays.ModifiedTime = DateTime.Now;
                    weekDays.ServerAddTime = DateTime.Now;
                    weekDays.ServerModifiedTime = DateTime.Now;
                    weekDays.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                    weekDays.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";

                    weekDays.UID = Guid.NewGuid().ToString();
                    weekDays.StoreUID = UID;
                    res += await WeekDaysBL.CreateWeekDaysForMobile(weekDays);


                    if (res > 0)
                    {
                        storeSavedAlert.IsSaved=true;
                        storeSavedAlert.Message = "Saved Successfully!";
                        storeSavedAlert.Value = UID;
                    }

                }
                else
                {
                    var res = await _storeBL.UpdateStore(iStore);
                    res += await WeekDaysBL.UpdateWeekDaysForMobile(weekDays);
                    if (res > 0)
                    {
                        storeSavedAlert.IsSaved = true;
                        storeSavedAlert.Message = "Updated Successfully!";
                    }
                }
            }
            else
            {
               
                    storeSavedAlert.IsSaved = false;
                    storeSavedAlert.Message = ErrorMessage;
                
            }
            await Status.InvokeAsync(storeSavedAlert);
        }
      
      

        private async Task getListItemsAsync()
        {
            List<string> codes = new List<string>()
            {
                Winit.Shared.Models.Constants.ListHeaderType.CustomerType,
                Winit.Shared.Models.Constants.ListHeaderType.StoreSize,
                Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery,
                Winit.Shared.Models.Constants.ListHeaderType.Group,
                Winit.Shared.Models.Constants.ListHeaderType.Chain,
                Winit.Shared.Models.Constants.ListHeaderType.Classification,
                Winit.Shared.Models.Constants.ListHeaderType.SubChannel,
                Winit.Shared.Models.Constants.ListHeaderType.Channel,
                Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency
            };

            try
            {
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> pagedResponse  = await ListHeader.GetListItemsByCodes(codes, true);

                if (pagedResponse != null)
                {
                        if (pagedResponse.TotalCount > 0)
                        {
                            foreach (var item in pagedResponse.PagedData)
                            {
                                SelectionItem selectionItem = new SelectionItem()
                                {
                                    Code = item.Code,
                                    UID = item.UID,
                                    Label = item.Name,
                                    IsSelected = iStore.Type == item.UID ? true : false,
                                };

                                switch (item.Code)
                                {
                                    case Winit.Shared.Models.Constants.ListHeaderType.CustomerType:
                                        _customerType.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:
                                        _storeSize.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:
                                        _mOD.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.Group:
                                        _customerGroup.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.Chain:
                                        _chain.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.Classification:
                                        _classification.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:
                                        _subChannel.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:
                                        _channel.Add(selectionItem);
                                        break;
                                    case Winit.Shared.Models.Constants.ListHeaderType.Channel:
                                        _visitFrequency.Add(selectionItem);
                                        break;

                                }
                            }

                        }

                }
            }
            catch (Exception ex)
            {

            }
          
            
        }

        public void SelectedValueChanged(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    ChangeSelection(dropDownEvent.SelectionItems.FirstOrDefault(), type);
                }
                else
                {
                    ChangeSelection(new SelectionItem(), type);
                }

            }
            else
            {
                switch (type)
                {
                    case Winit.Shared.Models.Constants.ListHeaderType.CustomerType:
                      
                        isShowCustomersinPopup = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:
                       
                        isShowStoreSizeinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:
                       
                        isShowMODinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Group:
                       
                        isShowGroupinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Chain:
                       
                        isShowChaininPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Classification:
                       
                        isShowClassificationinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:
                       
                        isShowCustomerSubChannelinPopup = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:
                       
                        isShowVisitFrequencyinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Channel:
                        
                        isShowCustomerChannelinPopup = true;
                        break;

                }
            }

        }


        public void ChangeSelection(Winit.Shared.Models.Common.ISelectionItem item, string Type)
        {

            switch (Type)
            {
                case  Winit.Shared.Models.Constants.ListHeaderType.CustomerType:
                    iStore.Type = item.UID;
                    CustomerTypeLabel = string.IsNullOrEmpty(iStore.Type) ? "Select Customer Type" : item.UID;
                    isShowCustomersinPopup = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:
                     iStore.StoreSize = CommonFunctions.GetDecimalValue(item.Code);
                    StoreSizeLabel = item.Label;
                    isShowStoreSizeinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:
                    MODLabel = item.Label;
                    isShowMODinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Group:
                    CustomerGroupLabel = item.Label;
                    //iStore.Group = e.Value.ToString();
                    isShowGroupinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Chain:
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    ChainLabel = item.Label;
                    isShowChaininPopUp = true;
                    break;
                     case Winit.Shared.Models.Constants.ListHeaderType.Classification:
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    ClassificationLabel = item.Label;
                    isShowClassificationinPopUp = true;
                    break;
                     case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:
                    CustomerSubChannelLabel = item.Label;
                    //iStore.Suffix = e.Value.ToString();
                    isShowCustomerSubChannelinPopup = true;
                    break;
                     case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:
                    VisitFrequencyLabel = item.Label;
                    isShowVisitFrequencyinPopUp = true;
                    break;
                     case Winit.Shared.Models.Constants.ListHeaderType.Channel:
                    CustomerChannelLabel = item.Label;
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    isShowCustomerChannelinPopup = true;
                    break;

            }
        }

    }
}
