using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.BL.Interfaces
{
    public interface IDeviceManagementViewModel
    {
        public List<IAppVersionUser> DeviceManagementLists { get; set; }
        public IAppVersionUser DeviceManagement { get; set; }
        public string ORGUID { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public string OrgUID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        Task PageIndexChanged(int pageNumber);
        Task GetSalesman(string OrgUID);
        Task PopulateViewModel();
        Task PopulateDeviceManagementforEditDetailsData(string Uid);
         Task UpdateDeviceManagement(string deviceid);
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task ApplySort(SortCriteria sortCriteria);

    }
}
