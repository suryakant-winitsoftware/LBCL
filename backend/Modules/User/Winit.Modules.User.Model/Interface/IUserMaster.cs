using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.User.Model.Interface
{
    public interface IUserMaster
    {
        Emp.Model.Interfaces.IEmp Emp { get; set; }
        JobPosition.Model.Interfaces.IJobPosition JobPosition { get; set; }
        Winit.Modules.Role.Model.Interfaces.IRole Role { get; set; }
        List<Currency.Model.Interfaces.IOrgCurrency> Currency { get; set; }
        IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting> Settings { get; set; }
        Dictionary<string, ITax> TaxMaster { get; set; }
        List<string> OrgUIDs { get; set; }
        List<ISelectionItem>? ProductDivisionSelectionItems { get; set; }
        List<string>? AsmDivisions { get; set; }
        List<IApprovalRuleMaster>? ApprovalRuleMaster { get; set; }
    }
}
