using Winit.Modules.ApprovalEngine.BL.Interfaces;

namespace Winit.Modules.ApprovalEngine.BL.Classes
{
    public class RuleMaster : IRuleMaster
    {
        public List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMaster> _ApprovalRuleMaster { get; set; }

        public void PopulateApprovalRuleMaster(List<Model.Interfaces.IApprovalRuleMaster> approvalRuleMaster)
        {
            _ApprovalRuleMaster=approvalRuleMaster;
        }
    }
}
