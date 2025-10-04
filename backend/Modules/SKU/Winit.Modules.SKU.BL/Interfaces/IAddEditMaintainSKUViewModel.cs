using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.CustomControls;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IAddEditMaintainSKUViewModel
    {
        public List<ISelectionItem> ORGSupplierSelectionItems { get; set; }
        public List<ISelectionItem> BuyingUOMSelectionItems { get; set; }
        public List<ISKUUOM> SKUUOMList { get; set; }
        public List<ISKUConfig> SKUCONFIGLIST { get; set; }
        public List<CustomField> CustomFieldlist { get; set; }

        public ISKU SKU { get; set; }
        public ISKUUOM SKUUOM { get; set; }
        public ISKUConfig SKUCONFIG { get; set; }
        public List<CustomField> dbData { get; set; }
        public bool IsAddNewSKUVisiblebtn { get; set; }
        public Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields SKUCUSTOM { get; set; }
        public Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField SKUCUSTOMForDynamic { get; set; }
        public ISKUMaster SkuMaster { get; set; }
        public List<ISelectionItem> SKUUOMTypeSelectionItems { get; set; }
        public List<ISelectionItem> SellingUOMSelectionItems { get; set; }
        public List<ISelectionItem> oRGDistributionSelectionItems { get; set; }
        public List<ISelectionItem> VoumeUnitSelectionItems { get; set; }
        public List<ISelectionItem> GrossWeightUnitSelectionItems { get; set; }
        public List<ISelectionItem> WeightUnitSelectionItems { get; set; }
        public DropDown DropDown { get; set; }
        List<SKUAttributeDropdownModel> SKUAttributeData { get; set; }
        public bool IsAddSkuAttribute { get; set; }
        void AddSelectionItemsToDD(ISelectionItem selectionItem, List<ISelectionItem> selectionItems, string skuAttrType);
        Task PopulateViewModel();
        Task<List<CustomField>> PopulateCustomSkuFieldsDynamic();
        Task<(string, bool)> SaveUpdateSKUItem(ISKU sKU, bool Iscreate);
        Task OnSKUConfigEditClick(ISKUConfig sKUConfig);
        void OnSKUUOMCancelBtnClickInPopUp();
        void OnSKUConfigCancelBtnClickInPopUp();
        Task SaveSKUCustomFieldsForDynamic(List<Winit.Modules.CustomSKUField.Model.Classes.CustomField> customField);
        Task<(string, bool)> OnDistributionCreateUpdateBtnClickFromPopUp(ISKUConfig sKUConfig, bool Iscreate);
        Task<(string, bool)> OnSKUUOMCreateUpdateBtnClickFromPopUp(ISKUUOM sKUUOM, bool Iscreate);
        Task<ISKUUOM> CreateSKUUOMClone(ISKUUOM ogSKUUOM);
        Task<ISKUConfig> CreateDistributionClone(ISKUConfig ogSKUConfig);
        Task<ISKUMaster> PopulateSKUDetailsData(string orguid);
       // Task PopulateUploadImage();
        Task InstilizeFieldsForEditPage(ISKUMaster sKUMaster);
        Task OnSKUUOMEditClick(ISKUUOM sKUUOM);
        Task<List<CommonUIDResponse>> CreateUpdateFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate);
        Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID);
        Task<bool> SaveOrUpdateSKUAttributes(Dictionary<string, ISelectionItem> keyValuePairs);

    }
}
