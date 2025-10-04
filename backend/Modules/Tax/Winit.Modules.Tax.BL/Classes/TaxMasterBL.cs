using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Nest;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.BL.Classes
{
    public class TaxMasterBL : Interfaces.ITaxMasterBL
    {
        protected readonly DL.Interfaces.ITaxMasterDL _taxMasterDL = null;
        IServiceProvider _serviceProvider;
        public TaxMasterBL(DL.Interfaces.ITaxMasterDL taxMasterDL,
            IServiceProvider serviceProvider) 
        {
            _taxMasterDL = taxMasterDL;
            _serviceProvider= serviceProvider;
        }


       public async  Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>> GetTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _taxMasterDL.GetTaxDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Tax.Model.Interfaces.ITax> GetTaxByUID(string UID)
        {
            return await _taxMasterDL.GetTaxByUID(UID);
        }
     public async   Task<int> CreateTax(Winit.Modules.Tax.Model.Interfaces.ITax createTax)
        {
            return await _taxMasterDL.CreateTax(createTax);
        }
        public async Task<int> UpdateTax(Winit.Modules.Tax.Model.Interfaces.ITax updateTax)
        {
            return await _taxMasterDL.UpdateTax(updateTax);
        }
        public async Task<int> DeleteTax(string UID)
        {
            return await _taxMasterDL.DeleteTax(UID);
        }

        public async Task<List<Model.Interfaces.ITaxMaster>> GetTaxMaster(List<string> OrgUIDs)
        {
            List<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>? taxMasters = null;
            var (taxList, taxsalbViewList) = await _taxMasterDL.GetTaxMaster(OrgUIDs);

            if (taxList != null && taxList.Count > 0)
            {
                taxMasters = new List<Model.Interfaces.ITaxMaster>();
                foreach (var tax in taxList)
                {
                    Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = _serviceProvider.CreateInstance<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>();
                    taxMaster.Tax = tax;
                    taxMaster.TaxSlabList = taxsalbViewList;
                   // taxMaster.TaxSlabList = taxsalbViewList.Where(e => e.ta == tax.UID).ToList();
                    
                    taxMasters.Add(taxMaster);
                }
            }
            return taxMasters;
        }

      public async  Task<int> CreateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO)
        {
            return await _taxMasterDL.CreateTaxMaster(taxMasterDTO);
        }
       public async Task<int> UpdateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO)
        {
            return await _taxMasterDL.UpdateTaxMaster(taxMasterDTO);
        }

        public async Task<Winit.Modules.Tax.Model.Interfaces.ITaxMaster> SelectTaxMasterViewByUID(string UID)
        {
            var (TaxList, TaxskuMapList) = await _taxMasterDL.SelectTaxMasterViewByUID(UID);

            Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = _serviceProvider.CreateInstance<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>();
            if (TaxList != null && TaxList.Count > 0)
            {
                taxMaster.Tax = TaxList.FirstOrDefault();
            }
            if (TaxskuMapList != null && TaxskuMapList.Count > 0)
            {
                taxMaster.TaxSkuMapList = TaxskuMapList;
            }
            return taxMaster;
        }
        public async Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>> GetTaxGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
     int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _taxMasterDL.GetTaxGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<ITaxGroup> GetTaxGroupByUID(string UID)
        {
           return await _taxMasterDL.GetTaxGroupByUID(UID);
        }

        public async Task<int> CreateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            return await _taxMasterDL.CreateTaxGroupMaster(taxGroupMasterDTO);
        }

        public async Task<int> UpdateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            return await _taxMasterDL.UpdateTaxGroupMaster(taxGroupMasterDTO);
        }

        public async Task<IEnumerable<ITaxSelectionItem>> GetTaxSelectionItems(string taxGroupUID)
        {
            return await _taxMasterDL.GetTaxSelectionItems(taxGroupUID);
        }
    }
}
