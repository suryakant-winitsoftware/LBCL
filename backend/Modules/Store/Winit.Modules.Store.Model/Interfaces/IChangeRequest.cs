using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IChangeRequest : IBaseModel
    {
        public int Id { get; set; }
        public string EmpUid { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUid { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Status { get; set; }
        public string ChangeData { get; set; }
        public string RowRecognizer { get; set; }
        public string ChannelPartnerCode { get; set; }
        public string ChannelPartnerName { get; set; }
        public string OperationType { get; set; }
        public string Reference { get; set; }
    }
}
