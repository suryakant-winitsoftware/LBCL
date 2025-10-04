using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Interfaces
{
    public interface IMaintainUsersViewModel
    {
        public List<IMaintainUser> maintainUsersList { get; set; }
        public IMaintainUser maintainUser { get; set; }
        public IEmp empUser { get; set; }
        public IEmpInfo empInfoUser { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalSKUItemsCount { get; set; }
        public EmpDTOModel EmpDTOModelmaintainUser { get; set; }
        public IEmpDTO EmpDTOmaintainUser { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task ApplySort(SortCriteria sortCriteria);
        Task GetSalesman(string OrgUID);
        Task PopulateViewModel();
        Task DisableDataFromGridview(IEmpDTO user, bool IsCreate);
        Task PopulateUsersDetailsforEdit(string Uid);
        Task PageIndexChanged(int pageNumber);
        Task<string> UpdateNewPassword(IChangePassword changePassword);
        Task<bool> CheckUserExistsAsync(string code, string loginId, string empNo);
    }
}
