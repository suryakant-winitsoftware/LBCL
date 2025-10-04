using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.Store;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class Address : ComponentBase
    {
        [Parameter] public string? Type { get; set; }
        [Parameter] public string? LinkedItemUID { get; set; }
        [Parameter] public IAddress? _Address { get; set; }


        public string btnName = "Save";
        private bool ViewLocation { get; set; }
        Winit.Modules.Location.Model.Classes.LocationData SelectedLocation { get; set; }
        HttpClient http = new HttpClient();
        [Inject]
        Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSettings { get; set; }
        public bool isEdited { get; set; } = false;
        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(_Address.LocationJson))
            {
                DeserializeLocationMaster();
            }
            base.OnInitialized();
        }
        public bool IsComponentEdited()
        {
            return isEdited;
        }
        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }
       
        private void OnLocationChange(Winit.Modules.Location.Model.Classes.LocationData locationMasterForUI)
        {
            if (locationMasterForUI == null)
            {
                _Address.LocationLabel = $"Select {_appSettings.LocationLevel}";
                _Address.LocationUID = string.Empty;
            }
            else
            {
                SelectedLocation = locationMasterForUI;
                _Address.LocationLabel = locationMasterForUI.Label;
                _Address.LocationUID = locationMasterForUI.PrimaryUid;
                _Address.LocationJson = locationMasterForUI.JsonData;
                DeserializeLocationMaster();
            }
            ViewLocation = false;
        }
        protected void DeserializeLocationMaster()
        {
            _Address.LocationMasters = JsonConvert.DeserializeObject<List<LocationMaster>>(_Address.LocationJson);
            if (_Address.LocationMasters != null)
            {
                _Address.LocationMasters = _Address.LocationMasters.OrderBy(p => p.Level).ToList();
            }
        }
        public (bool,string) IsValidated()
        {
            bool isVal = true;
            string message = string.Empty;
            if (string.IsNullOrEmpty(_Address.LocationUID))
            {
                isVal = false;
                message += $"{_appSettings.LocationLevel},";
            }
            if (string.IsNullOrEmpty(_Address.Line1))
            {
                isVal = false;
                message += $"Address Line1 ,";
            }
            if (string.IsNullOrEmpty(_Address.ZipCode))
            {
                isVal = false;
                message += $"Postal Code ,";
            }
            if(!string.IsNullOrEmpty(message))
            {
                isVal = false;
                message += message.Substring(0, message.Length - 2)+" Fields are mandatory!";
            }
            if (!string.IsNullOrEmpty(_Address.Email))
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!regex.IsMatch(_Address.Email))
                {
                    isVal = false;
                    message +="and Invalid email address.";
                }
            }
            return (isVal, message);
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
    }
}
