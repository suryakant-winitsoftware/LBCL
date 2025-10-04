using Winit.Modules.ApprovalEngine.Model.Interfaces;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{
    public class ViewChangeRequestApproval : IViewChangeRequestApproval
    {
        public string UID { get; set; }
        public string EmpUid { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUid { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Status { get; set; }
        public string ChangedRecord { get; set; }
        public string RowRecognizer { get; set; }
        public string ChannelPartnerCode { get; set; }
        public string ChannelPartnerName { get; set; }
        public string RequestedBy { get; set; }
        public string OperationType { get; set; }
        public string Reference { get; set; }
    }
}
