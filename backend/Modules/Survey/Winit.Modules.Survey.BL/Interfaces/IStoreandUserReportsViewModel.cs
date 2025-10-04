using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface IStoreandUserReportsViewModel : Winit.Modules.Base.BL.Interfaces.ITableGridViewModel
    {
        public List<IStoreUserInfo> StoreUserInfoList { get; set; }
        public List<IStoreUserInfo>? StoreUserInfoListforExport { get; set; }

        Task PopulateViewModel();
        Task ExporttoExcel();
        Task GetUsers(string OrgUID);
        Task GetStores_Customers(string orgUID);
        Task GetStates();
        public List<ISelectionItem> StateselectionItems { get; set; }

        public List<ISelectionItem> EmpSelectionList { get; set; }

        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        public List<ISelectionItem> RoleSelectionList { get; set; }
        Task GetRoles();

    }
}
