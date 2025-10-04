namespace Winit.Modules.ApprovalEngine.BL.Interfaces
{
    public interface IRuleMaster
    {
        void PopulateApprovalRuleMaster(List<Model.Interfaces.IApprovalRuleMaster> approvalRuleMaster);
        public List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMaster> _ApprovalRuleMaster { get; set; }
    }
}
