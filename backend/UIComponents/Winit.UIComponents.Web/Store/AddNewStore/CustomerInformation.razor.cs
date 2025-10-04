using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Enums;
using System.Runtime.CompilerServices;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Location.Model.Classes;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class CustomerInformation<T> : Microsoft.AspNetCore.Components.ComponentBase
    {
        [Parameter] public Winit.Modules.Store.Model.Classes.Store _CustomerInformation { get; set; }
        [Parameter] public List<Modules.Location.Model.Classes.Location> Locations { get; set; }
        [Parameter] public List<Modules.Location.Model.Classes.LocationType> LocationType { get; set; }
        [Parameter] public List<ListItem> ListItems { get; set; }
        [Parameter] public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
        [Parameter]public EventCallback<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> OnUploadStoreImages { get; set; }
        [Inject]
        Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSettings { get; set; }
        Winit.Modules.Location.Model.Classes.LocationData SelectedLocation { get; set; }
        public bool isEdited { get; set; } = false;
        public bool VatOrGst { get; set; }

        public string Message = string.Empty;
        private string CustomerTypelabel { get; set; } = "Select Customer";
        private string Pricelabel { get; set; } = "Select Price";
        private string BDMlabel { get; set; } = "Select BDM";
        private string Countrylabel { get; set; } = "Select Country";
        private string Regionlabel { get; set; } = "Select Region";
        private string Statelabel { get; set; } = "Select State";
        private string Citylabel { get; set; } = "Select City";
        private string RouteTypelabel { get; set; } = "Select Route";
        private string BlockedByLabel { get; set; } = "Select Blocked  by";
        private bool ShowPricetype { get; set; } = false;
        private bool ShowCustomertype { get; set; } = false;
        private bool ShowBDM { get; set; }
        private bool ViewLocation { get; set; }
        private bool ShowCountry { get; set; }
        private bool ShowRegion { get; set; }
        private bool ShowState { get; set; }
        private bool ShowCity { get; set; }
        private bool ShowRoutetype { get; set; } = false;
        private bool ShowBlockedBy { get; set; } = false;
        private List<ISelectionItem> _customerType = new List<ISelectionItem>();
        private List<ISelectionItem> _priceType = new List<ISelectionItem>();
        private List<ISelectionItem> _blockedBy = new List<ISelectionItem>();
        private List<ISelectionItem> _bDM = new List<ISelectionItem>();
        private List<ISelectionItem> _country = new List<ISelectionItem>();
        private List<ISelectionItem> _region = new List<ISelectionItem>();
        private List<ISelectionItem> _state = new List<ISelectionItem>();
        private List<ISelectionItem> _city = new List<ISelectionItem>();
        private List<ISelectionItem> _routeType = new List<ISelectionItem>();

        private static IBrowserFile selectedImage;
        protected List<LocationMaster>? LocationMasters;
        bool isLoad { get; set; }
        string FilePath = string.Empty;
        public Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader;
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            base.OnInitialized();
        }
        protected override void OnInitialized()
        {
            _loadingService.ShowLoading();
            if (string.IsNullOrEmpty(_CustomerInformation.LocationLabel))
            {
                _CustomerInformation.LocationLabel = $"Select {_appSettings.LocationLevel}";
            }


            if (string.IsNullOrEmpty(_CustomerInformation.TaxType))
            {
                _CustomerInformation.TaxType = StoreConstants.Vat;
            }
            SetDropDowns();
            FilePath = FileSysTemplateControles.GetStoreFolderPath(_CustomerInformation.UID);

            if (!string.IsNullOrEmpty(_CustomerInformation.LocationJson))
            {
                DeserializeJsonLocation();
            }
            isLoad = true;

            StateHasChanged();
            _loadingService.HideLoading();

            base.OnInitialized();
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        protected void SetDropDowns()
        {
            SetLocations();
            SetListeItems();
        }
        protected void SetLocations()
        {
            foreach (Modules.Location.Model.Classes.Location item in Locations)
            {
                if (item.UID.Equals(_CustomerInformation.CountryUID))
                {
                    Countrylabel = item.Name;
                }
                if (item.UID.Equals(_CustomerInformation.RegionUID))
                {
                    Regionlabel = item.Name;
                }
                if (item.UID.Equals(_CustomerInformation.StateUID))
                {
                    Statelabel = item.Name;
                }
                if (item.UID.Equals(_CustomerInformation.CityUID))
                {
                    Citylabel = item.Name;
                }
            }
        }
        protected void IsTaxApplicable(bool ifYes)
        {

        }
        public void OnFilesUpload(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys)
        {
            OnUploadStoreImages.InvokeAsync(fileSys);
        }
        protected void SetListeItems()
        {
            foreach (var item in ListItems)
            {
                SelectionItem type = new SelectionItem()
                {
                    Code = item.Code,
                    UID = item.UID,
                    Label = item.Name,
                };
                if (item.ListHeaderUID == "CustomerType")
                {
                    type.IsSelected = _CustomerInformation.Type == item.Code ? true : false;
                    if (type.IsSelected)
                    {
                        CustomerTypelabel = type.Label;
                    }
                    _customerType.Add(type);
                }
                else if (item.ListHeaderUID == "PriceType")
                {
                    type.IsSelected = _CustomerInformation.PriceType == item.Code ? true : false;
                    if (type.IsSelected)
                    {
                        Pricelabel = type.Label;
                    }
                    _priceType.Add(type);
                }
                else if (item.ListHeaderUID == "RouteType")
                {
                    type.IsSelected = _CustomerInformation.RouteType == item.Code ? true : false;
                    if (type.IsSelected)
                    {
                        RouteTypelabel = type.Label;
                    }
                    _routeType.Add(type);
                }
                else if (item.ListHeaderUID == "BlockedBy")
                {
                    type.IsSelected = _CustomerInformation.BlockedByEmpUID == item.Code ? true : false;
                    if (type.IsSelected)
                    {
                        BlockedByLabel = type.Label;
                    }
                    _blockedBy.Add(type);
                }
                else if (item.ListHeaderUID == "BDM")
                {
                    type.IsSelected = _CustomerInformation.ProspectEmpUID == item.Code ? true : false;
                    if (type.IsSelected)
                    {
                        BDMlabel = type.Label;
                    }
                    _bDM.Add(type);
                }
            }
        }

        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }
        public bool IsComponentEdited()
        {
            return isEdited;
        }
        protected void StoreSize(ChangeEventArgs eventArgs)
        {
            _CustomerInformation.StoreSize = CommonFunctions.RoundForSystem(eventArgs?.Value);
        }
        private void SetCountry()
        {
            _country = new List<ISelectionItem>();
            string? LocationTypeUID = LocationType?.Find(t => t.Code == "CTY")?.UID;
            foreach (var item in Locations.Where(t => t.LocationTypeUID == LocationTypeUID))
            {
                SelectionItem type = new SelectionItem()
                {
                    Code = item.Code,
                    UID = item.UID,
                    Label = item.Name,
                    IsSelected = false,
                };
                _country.Add(type);
            }
            ShowCountry = true;
        }
        private void OnLocationChange(Winit.Modules.Location.Model.Classes.LocationData locationMasterForUI)
        {
            if (locationMasterForUI == null)
            {
                _CustomerInformation.LocationLabel = $"Select {_appSettings.LocationLevel}";
                _CustomerInformation.LocationUID = string.Empty;
                LocationMasters = null;
            }
            else
            {
                SelectedLocation = locationMasterForUI;
                _CustomerInformation.LocationLabel = locationMasterForUI.PrimaryLabel;
                _CustomerInformation.LocationUID = locationMasterForUI.PrimaryUid;
                _CustomerInformation.LocationJson = locationMasterForUI.JsonData;

                DeserializeJsonLocation();


            }
            ViewLocation = false;
        }
        protected void DeserializeJsonLocation()
        {
            LocationMasters = JsonConvert.DeserializeObject<List<LocationMaster>>(_CustomerInformation.LocationJson);
            if (LocationMasters != null)
            {
                LocationMasters = LocationMasters.OrderBy(p => p.Level).ToList();
            }
        }
        public (bool, string) IsValidated()
        {
            bool retVal = true;
            Message = string.Empty;
            if (string.IsNullOrEmpty(_CustomerInformation.Number))
            {
                Message += "Customer Number ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.Code))
            {
                Message += "Customer Code ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.Name))
            {
                Message += "Customer Name ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.AliasName))
            {
                Message += "Customer Name2 ,";
                retVal = false;
            }
            //if (string.IsNullOrEmpty(_CustomerInformation.ArabicName))
            //{
            //    Message += "Customer Arabic Name ,";
            //    retVal = false;
            //}
            if (string.IsNullOrEmpty(_CustomerInformation.LegalName))
            {
                Message += "Legal Name ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.OutletName))
            {
                Message += "Outlet Name ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.Type))
            {
                Message += "Customer Type ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.LocationUID))
            {
                Message += $"{_appSettings.LocationLevel} ,";
                retVal = false;
            }
            if (string.IsNullOrEmpty(_CustomerInformation.BlockedReasonDescription) && _CustomerInformation.IsBlocked)
            {
                Message += "Blocked Description ,";
                retVal = false;
            }

            if (string.IsNullOrEmpty(_CustomerInformation.TaxDocNumber) && _CustomerInformation.IsTaxApplicable)
            {
                Message += $"{_CustomerInformation.TaxType} Number ,";
                retVal = false;
            }
            //if (string.IsNullOrEmpty(_CustomerInformation.TaxKeyField) && _CustomerInformation.IsTaxApplicable)
            //{
            //    Message += " TaxKey Field,";
            //    retVal = false;
            //}


            if (!retVal)
            {
                Message = Message.Substring(0, Message.Length - 2);
                Message += " Field(s) are mandatory ";

            }
            return (retVal, Message);
        }

        private void OnSelected(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {

                    ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();

                    switch (type)
                    {
                        case "CustomerType":
                            CustomerTypelabel = selectionItem.Label;
                            _CustomerInformation.Type = selectionItem.Code;
                            break;
                        case "PriceType":
                            Pricelabel = selectionItem.Label;
                            _CustomerInformation.PriceType = selectionItem.Code;
                            break;
                        case "BlockedBy":
                            BlockedByLabel = selectionItem.Label;
                            _CustomerInformation.BlockedByEmpUID = selectionItem.Code;
                            break;
                        case "RouteType":
                            RouteTypelabel = selectionItem.Label;
                            _CustomerInformation.RouteType = selectionItem.Code;
                            break;
                        case "BDM":
                            BDMlabel = selectionItem.Label;
                            _CustomerInformation.ProspectEmpUID = selectionItem.Code;
                            break;

                    }
                }
                else
                {
                    switch (type)
                    {
                        case "CustomerType":
                            CustomerTypelabel = "Select CustomerType";
                            _CustomerInformation.Type = string.Empty;
                            break;
                        case "PriceType":
                            Pricelabel = "Select Price Type";
                            _CustomerInformation.PriceType = string.Empty;
                            break;
                        case "BlockedBy":
                            BlockedByLabel = "Select Blocked By";
                            _CustomerInformation.BlockedByEmpUID = string.Empty;
                            break;
                        case "RouteType":
                            RouteTypelabel = "Select Route Type";

                            _CustomerInformation.RouteType = string.Empty;
                            break;
                        case "BDM":
                            BDMlabel = "Select BDM";
                            _CustomerInformation.ProspectEmpUID = string.Empty;
                            break;

                    }
                }
            }

            if (ShowCustomertype)
                ShowCustomertype = false;
            if (ShowPricetype)
                ShowPricetype = false;
            if (ShowBlockedBy)
                ShowBlockedBy = false;
            if (ShowBlockedBy)
                ShowBlockedBy = false;
            if (ShowBDM)
                ShowBDM = false;
            if (ShowRoutetype)
                ShowRoutetype = false;
        }

    }
}
