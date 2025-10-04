using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Distributor.BL.Interfaces
{
    public interface IDistributorAdminBaseViewModel
    {
        bool IsLoad { get; set; }
        bool IsShowPopUp { get; set; }
        bool IsNewEmp { get; set; }
        bool IsEditLoginID { get; set; }
        bool IsPassword {  get; set; }
        string Name {  get; set; }
        string LoginId { get; set; }
        string Password {  get; set; }
        string ConfirmPassword { get; set; }
        List<DataGridColumn> Columns { get; set; }
        List<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpList { get; set; }

        Task PopulateViewModel();
        void CloseTab();
        void ResetPassword(Winit.Modules.Emp.Model.Interfaces.IEmp emp);
        void EditLogInIdName(Winit.Modules.Emp.Model.Interfaces.IEmp emp);

        void AddNewAdmin();
        Task SaveOrUpdate();
    }
}
