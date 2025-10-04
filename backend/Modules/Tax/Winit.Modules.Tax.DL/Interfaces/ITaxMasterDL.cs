using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.DL.Interfaces
{
    public interface ITaxMasterDL
    {
        Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>> GetTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Tax.Model.Interfaces.ITax?> GetTaxByUID(string UID);
        Task<int> CreateTax(Winit.Modules.Tax.Model.Interfaces.ITax createTax);
        Task<int> UpdateTax(Winit.Modules.Tax.Model.Interfaces.ITax updateTax);
        Task<int> DeleteTax(string UID);
        Task<int> CreateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO);
        Task<int> UpdateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO);
        Task<(List<Winit.Modules.Tax.Model.Interfaces.ITax>, List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>)> GetTaxMaster(List<string> OrgUIDs);
        Task<(List<Model.Interfaces.ITax>, List<Model.Interfaces.ITaxSkuMap>)> SelectTaxMasterViewByUID(string UID);
        Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>> GetTaxGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Tax.Model.Interfaces.ITaxGroup?> GetTaxGroupByUID(string UID);
        Task<int> CreateTaxGroupMaster(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMasterDTO);
        Task<int> UpdateTaxGroupMaster(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMasterDTO);
        Task<IEnumerable<ITaxSelectionItem>> GetTaxSelectionItems(string taxGroupUID);
    }
}
