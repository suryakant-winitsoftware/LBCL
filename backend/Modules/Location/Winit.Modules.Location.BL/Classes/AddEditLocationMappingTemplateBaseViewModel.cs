using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Location.BL.Classes
{
    public abstract class AddEditLocationMappingTemplateBaseViewModel : Interfaces.IAddEditLocationMappingTemplateBaseViewModel
    {
        protected CommonFunctions _commonFunctions;
        protected IDataManager _dataManager;
        protected IToast _toast;
        protected IAppUser _appUser;
        public LocationTemplate LocationTemplate { get; set; }
        public LocationTemplateLine LocationTemplateLine { get; set; } = new LocationTemplateLine();
        public List<LocationTemplateLine> LocationTemplateLineList { get; set; } = new();
        public string Locationlabel { get; set; }
        public string LocationTypelabel { get; set; }
        public bool IsAdd { get; set; }
        public bool IsExcluded { get; set; }
        protected List<Model.Classes.Location> LocationsByType { get; set; }
        protected List<ILocationType> LocationTypes { get; set; }
        public List<ISelectionItem> Locations { get; set; } = new();
        public List<ISelectionItem> LocationTypesForDD { get; set; }
        ISelectionItem? SelectedlocationType { get; set; }
        List<ISelectionItem>? Selectedlocations { get; set; }
        public async Task PopulateViewModel()
        {
            string page = _commonFunctions.GetParameterValueFromURL(PageType.Page);
            if (PageType.New.Equals(page))
            {
                LocationTemplate = new LocationTemplate()
                {
                    UID = Guid.NewGuid().ToString(),
                    CreatedBy = _appUser.Emp.UID,
                    CreatedTime = DateTime.Now,
                };
            }
            else
            {
                string templateUID = _commonFunctions.GetParameterValueFromURL("UID");
                object obj = _dataManager.GetData(templateUID);
                if (obj != null)
                {
                    LocationTemplate = (LocationTemplate)obj;
                    _dataManager.DeleteData(templateUID);
                }
                await GetLocationTemplateLinesFromAPI(templateUID);
            }

            await GetLocationTypeFromAPI();

        }
        public async Task AddMapping()
        {

        }
        public async Task SaveMapping()
        {
            if (SelectedlocationType != null && Selectedlocations != null && Selectedlocations.Count > 0)
            {
                foreach (var item in Selectedlocations)
                {
                    LocationTemplateLine location = new LocationTemplateLine()
                    {
                        UID = Guid.NewGuid().ToString(),
                        LocationTemplateUID = LocationTemplate.UID,
                        LocationTypeUID = SelectedlocationType?.UID,
                        Type = SelectedlocationType.Label,
                        LocationUID = item.UID,
                        Value = item.Label,
                        IsExcluded = this.IsExcluded,
                        CreatedBy=_appUser.Emp.UID,
                        ModifiedBy=_appUser.Emp.UID,
                        ModifiedTime = DateTime.Now,
                        CreatedTime = DateTime.Now,
                    };
                    LocationTemplateLineList.Add(location);
                }
            }

            IsAdd = false;
            IsExcluded = false;
            Selectedlocations = null;
        }

        #region UI LOGIC
        public async Task OnLocationTypeSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
            {
                SelectedlocationType = dropDownEvent.SelectionItems.FirstOrDefault();
                if (SelectedlocationType != null)
                {
                    await GetLocationFromAPI(SelectedlocationType.Code);
                }
            }
        }
        public async Task OnLocationSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                Selectedlocations = dropDownEvent.SelectionItems;
            }
        }
        protected (bool, string) IsValidated()
        {
            bool isVal = true;
            string message = string.Empty;
            if (string.IsNullOrEmpty(LocationTemplate.TemplateCode))
            {
                message += "Mapping Code ,";
                isVal = false;
            }
            if (string.IsNullOrEmpty(LocationTemplate.TemplateName))
            {
                message += "Mapping Name ,";
                isVal = false;
            }
            if (!string.IsNullOrEmpty(message) && message.Length > 0)
            {
                message = $"Following Fields Shouldn't be Empty :{message.Substring(0, message.Length - 2)}";
            }

            return (isVal, message);
        }

        #endregion


        #region Abstract classes
        protected abstract Task GetLocationTypeFromAPI();
        protected abstract Task GetLocationFromAPI(string locationTypeUID);
        protected abstract Task GetLocationTemplateLinesFromAPI(string templateUID);
        #endregion
    }
}
