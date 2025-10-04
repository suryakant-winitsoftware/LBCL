using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface ICreateUserRoleBaseViewModel
    {
        bool IsAddOrEdit { get; set; }
        bool IsNew { get; set; }
       
        Model.Interfaces.IRole UserRole { get; set; }

        List<ISelectionItem> SelectionItems { get; set; }
        void IsPrincipalOrDistributor(bool isPrincOrDist);
        void IsAdminIsPrincipalToDistributor(bool idAdmin);
        void ONParentRoleChanged(DropDownEvent dropDownEvent);
        void PopulateViewModel(List<Winit.Modules.Role.Model.Interfaces.IRole> roles);
        void AddOrEditRole(bool isAdd, List<Model.Interfaces.IRole> roles, Model.Interfaces.IRole userRole = null);
        Task<int> SaveOrUpdate();
    }
}
