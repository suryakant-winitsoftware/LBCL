using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Interfaces
{
    public interface IOrgViewModel
    {

        public List<IOrg> OrgItemViews { get; set; }
        public IOrg org { get; set; }
        void Dispose();
        /// <summary>
        /// Apply Filter
        /// </summary>
        /// <param name="filterCriterias"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        /// <summary>
        /// Apply Search
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        Task ResetFilter();
      
        //Task PopulateOrgMaster();
        Task PopulateViewModel();
        Task GetORGforEditDetailsData(string ORGUid);
        Task SaveUpdateORGItem(IOrg org, bool Iscreate);
        Task<string> DeleteItem(string orgUID);
    }
}
