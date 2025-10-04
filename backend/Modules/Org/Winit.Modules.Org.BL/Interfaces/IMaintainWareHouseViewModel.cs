using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Interfaces
{
    public interface IMaintainWareHouseViewModel
    {
        public string ParentUID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<IWarehouseItemView> MaintainWarehouseGridList { get; set; }
        public List<ISelectionItem> WareHouseTypeSelectionItems { get; set; }
        public List<FilterCriteria> MaintainWarehouseFilterCriterias { get; set; }
        public List<ISelectionItem> DistributorSelectionList { get; set; }
        Task ApplySort(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
        Task PopulateViewModel();
        Task<string> DeleteMaintainWareHouse(string UID);
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task GetDistributor();
    }
}
