using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;

namespace Winit.Modules.User.Model.Classes
{
    public class UserMaster : IUserMaster
    {
        public Emp.Model.Interfaces.IEmp Emp { get; set; }
        public JobPosition.Model.Interfaces.IJobPosition JobPosition { get; set; }
        public Winit.Modules.Role.Model.Interfaces.IRole Role { get; set; }
        public List<Currency.Model.Interfaces.IOrgCurrency> Currency { get; set; }
        public IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting> Settings { get; set; }
        public Dictionary<string, ITax> TaxMaster { get; set; }
        public List<string> OrgUIDs { get; set; }
        public List<ISelectionItem>? ProductDivisionSelectionItems { get; set; }
        public List<string>? AsmDivisions { get; set; }
        public List<IApprovalRuleMaster>? ApprovalRuleMaster { get; set; }
    }
}
