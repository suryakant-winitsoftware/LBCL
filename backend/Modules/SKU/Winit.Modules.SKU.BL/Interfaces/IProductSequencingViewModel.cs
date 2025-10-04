using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IProductSequencingViewModel
    {

        public List<string> AllUIDs { get; set; }
        public List<string> ExistingUIDs { get; set; }
        List<SkuSequence> PrepareListOfUidtForDelete();
        public List<SkuSequenceUI> FilterSkuSequencelist { get; set; }
        //Task PrepareDataForTable();
        Task<bool> PrepareDataForSaveUpdate();
        public List<SkuSequenceUI> DisplaySkuSequencelist { get; set; }
        Task PopulateViewModel(string dataNeededFor = null, string pageName = null, string apiParam = null, string apiName = null);
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        //Task<List<Winit.Modules.SKU.Model.Interfaces.ISkuSequenceUI>> GetSKUSequenceData();
       // Task<bool> DeleteSKUs(List<Winit.Modules.SKU.Model.Classes.SkuSequence> skuSequenceList);

       // Task<List<Winit.Modules.SKU.Model.Classes.SKUMasterData>> GetSKUDataFromAPIAsync();

       Task<bool> CreateUpdateDeleteSKUs(List<SkuSequence> skuSequenceList);
        Task PrepareDataForTable(String SeqType);
        Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> FindCompleteSKU(IEnumerable<SKUMasterData> sKUMasters);
        void MatchingNewExistingUIDs();
        public List<FilterCriteria> FilterCriterias { get; set; }
        Task ApplyFilter(Dictionary<string, string> filterCriterias, string ActiveTab);
       Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> FindCompleteSKUAttributes(IEnumerable<SKUMasterData> sKUMasters);
    }
}
