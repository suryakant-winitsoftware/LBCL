using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.BL.UIInterfaces
{
    public interface ITaxGroupViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize{ get; set; }
        public int TotalTaxGroupItemsCount { get; set; }
        public List<ITaxGroupItemView> TaxGroupItemViews { get; set; }
        public List<ISelectionItem> TaxSelectionItemsDD { get; set; }
        Task PopulateViewModel();
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task ResetFilter();
        Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias);
        Task PageIndexChanged(int pageNumber);
        Task GetExistingTaxGroupWithByUID(string taxGroupUID);
        Task<bool> CreateTaxGroupMaster(ITaxGroup tax);
        Task<bool> UpdateTaxGroupMaster(ITaxGroup tax);
        Task PrepareAddTaxGroupPage(ITaxGroup taxGroup);
        ITaxGroup GetTaxGroup();
    }
}
