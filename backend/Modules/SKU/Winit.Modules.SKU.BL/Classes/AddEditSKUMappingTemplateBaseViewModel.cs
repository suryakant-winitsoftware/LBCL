using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.SKU.BL.Classes
{
    public abstract class AddEditSKUMappingTemplateBaseViewModel : IAddEditSKUMappingTemplateBaseViewModel
    {
        protected CommonFunctions _commonFunctions;
        protected IDataManager _dataManager;
        protected IToast _toast;
        protected IAppUser _appUser;
        public SKUTemplate SKUTemplate { get; set; }
        public List<SKUTemplateLine> SKUTemplateLineList { get; set; } = new List<SKUTemplateLine>();
        public List<ISelectionItem> SKUGroupsTypes { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> SKUGroups { get; set; } = new List<ISelectionItem>();
        public string SKUGroupLabel { get; set; }
        public string SKUGroupTypeLabel { get; set; }
        public bool IsAdd { get; set; }
        public bool IsExcluded { get; set; }

        ISelectionItem? SelectedSKUGroupType { get; set; }
        List<ISelectionItem>? SelectedSKUGroups { get; set; }
        public async Task PopulateViewModel()
        {
            SKUTemplateLineList = new();
            string page = _commonFunctions.GetParameterValueFromURL(PageType.Page);
            if (PageType.New.Equals(page))
            {
                SKUTemplate = new()
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
                    SKUTemplate = (SKUTemplate)obj;
                    _dataManager.DeleteData(templateUID);
                }
                await GetSKUTemplateLinesFromAPI(templateUID);
            }

            await GetSKUGroupTypeFromAPI();

        }
        public async Task AddMapping()
        {

        }
        public async Task SaveMapping()
        {
            if (SelectedSKUGroupType != null && SelectedSKUGroups != null && SelectedSKUGroups.Count > 0)
            {
                foreach (var item in SelectedSKUGroups)
                {
                    SKUTemplateLine skuTemplate = new()
                    {
                        UID = Guid.NewGuid().ToString(),
                        SKUGroupTypeUID = SelectedSKUGroupType.UID,
                        SKUGroupUID = item.UID,
                        SKUGroupTypeName = SelectedSKUGroupType.Label,
                        SKUTemplateUID = SKUTemplate.UID,
                        SKUGroupName = item.Label,
                        SKUGroupParentName = item.Code,
                        IsExcluded = this.IsExcluded,
                        CreatedBy=_appUser.Emp.UID,
                        ModifiedBy=_appUser.Emp.UID,
                        CreatedTime= DateTime.Now,
                        ModifiedTime= DateTime.Now,
                    };
                    SKUTemplateLineList.Add(skuTemplate);
                }
            }

            IsAdd = false;
            IsExcluded = false;
            SelectedSKUGroups = null;
        }

        #region UI LOGIC
        public async Task OnSKUGroupTypeSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
            {
                SelectedSKUGroupType = dropDownEvent.SelectionItems.FirstOrDefault();
                if (SelectedSKUGroupType != null)
                {
                    await GetSKUGroupBySkuGroupTypeUIDFromAPI(SelectedSKUGroupType.UID);
                }
            }
        }
        public async Task OnSKUGroupSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                SelectedSKUGroups = dropDownEvent.SelectionItems;
            }
        }
        protected (bool, string) IsValidated()
        {
            bool isVal = true;
            string message = string.Empty;
            if (string.IsNullOrEmpty(SKUTemplate.TemplateCode))
            {
                message += "Mapping Code ,";
                isVal = false;
            }
            if (string.IsNullOrEmpty(SKUTemplate.TemplateName))
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
        protected abstract Task GetSKUGroupTypeFromAPI();
        protected abstract Task GetSKUGroupBySkuGroupTypeUIDFromAPI(string skuGroupTypeUID);
        protected abstract Task GetSKUTemplateLinesFromAPI(string templateUID);
        #endregion
    }
}
