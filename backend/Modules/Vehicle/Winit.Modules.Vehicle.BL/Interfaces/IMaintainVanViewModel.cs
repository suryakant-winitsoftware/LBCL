using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Interfaces
{
    public interface IMaintainVanViewModel
    {
        public int PageNumber { get; set; } 
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<IVehicle> MaintainVanList { get; set; }
        public string OrgUID { get; set; }
        /// <summary>
        /// Apply Filter
        /// </summary>
        /// <param name="filterCriterias"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
        /// <summary>
        /// Apply Search
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        Task ResetFilter();
        Task ApplySort(SortCriteria sortCriterias);
        Task PopulateViewModel();
        Task PageIndexChanged(int pageNumber);
        Task<string> DeleteVehicle(string UID);
       // void InstilizeFieldsForEditPage(IVehicle vehicleDetails);
    }
}
