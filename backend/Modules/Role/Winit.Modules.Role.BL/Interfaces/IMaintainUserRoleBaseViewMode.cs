using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface IMaintainUserRoleBaseViewMode
    {
        int TotalItemsCount { get; set; }
        int PageNumber { get; set; }
        int PageSize { get; set; }
        bool IsLoad { get; set; }
        List<Winit.Modules.Role.Model.Interfaces.IRole> RoleList { get; set; }
        Task PopulateViewModel();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task OnPageChange(int pageNumber);
        Task OnSort(SortCriteria sortCriteria);
    }
}
